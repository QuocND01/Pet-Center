import 'dart:async';
import 'dart:convert';
import 'dart:io' show Platform, SocketException, File;
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:http/http.dart' as http;
import 'auth_service.dart';
import '../models/customer_model.dart';
import '../models/product_model.dart';
import '../models/cart_model.dart';
import '../models/address_model.dart';
import '../models/service_model.dart';
import '../models/pet_model.dart';
import '../models/product_feedback_model.dart';
import '../models/order_model.dart';

class ApiService {
  // Singleton pattern
  static final ApiService _instance = ApiService._internal();
  factory ApiService() => _instance;
  ApiService._internal();

  // Dynamic Base URL detection for Android Emulator (10.0.2.2) vs Localhost (iOS/Web/Desktop)
  static String get baseUrl {
    if (!kIsWeb && Platform.isAndroid) {
      return 'http://10.0.2.2:5163/api';
    }
    return 'http://localhost:5163/api';
  }

  final http.Client _client = http.Client();
  String? _token;
  String? _customerId;
  String? _customerEmail;

  // Initialize saved session from local storage
  Future<bool> initSession() async {
    final session = await AuthService().loadSession();
    _token = session['token'];
    _customerId = session['customerId'];
    _customerEmail = session['email'];

    if (_token != null && _token!.isNotEmpty) {
      try {
        final profile = await getCustomerProfile();
        _customerId = profile.customerId;
        await AuthService().saveSession(
          token: _token!,
          customerId: _customerId!,
          email: _customerEmail ?? profile.email ?? '',
        );
        return true;
      } catch (e) {
        // Token expired or invalid
        await clearAuthData();
        return false;
      }
    }
    return false;
  }

  // Save session auth data
  void setAuthData(String token, String customerId, String email) {
    _token = token;
    _customerId = customerId;
    _customerEmail = email;
    AuthService()
        .saveSession(token: token, customerId: customerId, email: email);
  }

  void setToken(String token) {
    _token = token;
  }

  String? get token => _token;
  String? get customerId => _customerId;
  String? get customerEmail => _customerEmail;
  bool get isAuthenticated => _token != null && _token!.isNotEmpty;

  Future<void> clearAuthData() async {
    _token = null;
    _customerId = null;
    _customerEmail = null;
    await AuthService().clearSession();
  }

  Future<void> logout() async {
    try {
      await _client.post(
        Uri.parse('$baseUrl/auths/logout'),
        headers: _getHeaders(),
      );
    } catch (_) {
      // Ignore network failure on logout
    } finally {
      await clearAuthData();
    }
  }

  Map<String, String> _getHeaders() {
    final headers = {'Content-Type': 'application/json'};
    if (_token != null) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  // Helper to execute any HTTP request with a 20-second timeout & connection error handling
  Future<http.Response> _sendRequest(
      Future<http.Response> Function() fn) async {
    try {
      final response = await fn().timeout(
        const Duration(seconds: 20),
        onTimeout: () => throw Exception(
            'Connection timeout (20s). Please check if backend API server is running.'),
      );
      return response;
    } on SocketException catch (_) {
      throw Exception(
          'Cannot connect to server ($baseUrl). Please verify backend API server is running.');
    } on TimeoutException catch (_) {
      throw Exception(
          'Connection timed out (20s). Backend API server is not responding.');
    } catch (e) {
      if (e.toString().contains('SocketException') ||
          e.toString().contains('Connection refused') ||
          e.toString().contains('Failed host lookup')) {
        throw Exception(
            'Cannot connect to backend API server. Please check backend API status.');
      }
      rethrow;
    }
  }

  // ============================================================
  // AUTHENTICATION (AuthsController)
  // ============================================================

  // Login
  Future<Map<String, dynamic>> customerLogin(
      String email, String password) async {
    final response = await _sendRequest(() => _client.post(
          Uri.parse('$baseUrl/auths/customer-login'),
          headers: {'Content-Type': 'application/json'},
          body: json.encode({'email': email, 'password': password}),
        ));

    if (response.body.isEmpty) {
      throw Exception(
          'Empty server response. Please verify backend API status.');
    }

    final data = json.decode(response.body);
    final isSuccess = data['success'] == true || data['Success'] == true;
    final tokenVal = data['token'] ?? data['Token'];

    if (response.statusCode == 200 && (isSuccess || tokenVal != null)) {
      _token = tokenVal;
      _customerEmail = email;

      // Fetch Profile to secure customerId
      try {
        final profile = await getCustomerProfile();
        _customerId = profile.customerId;
        await AuthService().saveSession(
          token: _token!,
          customerId: _customerId!,
          email: email,
        );
      } catch (e) {
        await AuthService().saveSession(
          token: _token!,
          customerId: '',
          email: email,
        );
      }
    }
    return data;
  }

  // Register
  Future<Map<String, dynamic>> customerRegister({
    required String fullName,
    required String email,
    required String phoneNumber,
    required String password,
    required String gender,
    required String birthDay,
  }) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/auths/register'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'fullName': fullName,
        'email': email,
        'phoneNumber': phoneNumber,
        'password': password,
        'gender': gender,
        'birthDay': birthDay,
      }),
    );
    return json.decode(response.body);
  }

  // Verify OTP
  Future<Map<String, dynamic>> verifyOtp(String email, String code) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/auths/verify-otp'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'email': email, 'code': code}),
    );
    return json.decode(response.body);
  }

  // Resend OTP
  Future<Map<String, dynamic>> resendOtp(String email) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/auths/resend-otp'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'email': email}),
    );
    return json.decode(response.body);
  }

  // ============================================================
  // PRODUCT CATALOG (ProductsController)
  // ============================================================

  // Get products with optional pagination (OData $top & $skip)
  Future<List<ProductModel>> getProducts({int? top, int? skip}) async {
    String url = '$baseUrl/Products';
    if (top != null || skip != null) {
      final queryParams = <String>[];
      if (top != null) queryParams.add('\$top=$top');
      if (skip != null) queryParams.add('\$skip=$skip');
      url += '?${queryParams.join('&')}';
    }

    final response = await _client.get(
      Uri.parse(url),
      headers: _getHeaders(),
    );

    final data = _handleResponse(response);
    if (data is List) {
      return data.map((json) => ProductModel.fromJson(json)).toList();
    } else if (data is Map && data['value'] != null) {
      final List odataList = data['value'];
      return odataList.map((json) => ProductModel.fromJson(json)).toList();
    }
    return [];
  }

  // Get product details
  Future<ProductModel> getProductDetails(String productId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Products/$productId'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    return ProductModel.fromJson(data);
  }

  // Get feedbacks by product id (ProductFeedbacksController)
  Future<List<ProductFeedbackModel>> getFeedbacksByProductId(String productId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/ProductFeedbacks/product/$productId'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    if (data != null && (data['success'] == true || data['Success'] == true)) {
      final List list = data['data'] ?? data['Data'] ?? [];
      return list.map((json) => ProductFeedbackModel.fromJson(json)).toList();
    }
    return [];
  }

  // ============================================================
  // SHOPPING CART (CartsController)
  // ============================================================

  // Fetch cart and its product details
  Future<CartResponseModel> getCart(String customerId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/cart/$customerId'),
      headers: _getHeaders(),
    );

    final data = _handleResponse(response);
    final cart = CartResponseModel.fromJson(data);

    for (var detail in cart.cartDetails) {
      try {
        final product = await getProductDetails(detail.productId);
        detail.product = product;
      } catch (e) {
        // Skip product details load error
      }
    }
    return cart;
  }

  // Add to cart
  Future<bool> addToCart(String productId, int quantity) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/cart/add'),
      headers: _getHeaders(),
      body: json.encode({
        'productId': productId,
        'quantity': quantity,
      }),
    );
    return response.statusCode == 200;
  }

  // Update item quantity
  Future<bool> updateCartQuantity(String cartDetailId, int quantity) async {
    final response = await _client.put(
      Uri.parse('$baseUrl/cart/details/$cartDetailId'),
      headers: _getHeaders(),
      body: json.encode({
        'quantity': quantity,
      }),
    );
    return response.statusCode == 200;
  }

  // Delete item from cart
  Future<bool> removeFromCart(String cartDetailId) async {
    final response = await _client.delete(
      Uri.parse('$baseUrl/cart/details/$cartDetailId'),
      headers: _getHeaders(),
    );
    return response.statusCode == 200;
  }

  // Clear cart
  Future<bool> clearCart(String customerId) async {
    final response = await _client.delete(
      Uri.parse('$baseUrl/cart/clear/$customerId'),
      headers: _getHeaders(),
    );
    return response.statusCode == 200;
  }

  // ============================================================
  // CUSTOMER PROFILE (CustomersProfileController)
  // ============================================================

  // Get customer profile details
  Future<CustomerModel> getCustomerProfile() async {
    final response = await _client.get(
      Uri.parse('$baseUrl/customer/profile'),
      headers: _getHeaders(),
    );
    final jsonResult = _handleResponse(response);
    final profileData = jsonResult?['data'] ?? jsonResult?['Data'];
    if (jsonResult != null && profileData != null) {
      return CustomerModel.fromJson(profileData);
    }
    throw Exception('Invalid customer profile data.');
  }

  // Update customer profile
  Future<bool> updateCustomerProfile(CustomerModel customer) async {
    final response = await _client.put(
      Uri.parse('$baseUrl/customer/profile'),
      headers: _getHeaders(),
      body: json.encode(customer.toJson()),
    );
    final jsonResult = _handleResponse(response);
    final isSuccess =
        jsonResult?['success'] == true || jsonResult?['Success'] == true;
    return jsonResult != null && isSuccess;
  }

  // ============================================================
  // ADDRESSES (AddressesController)
  // ============================================================
  Future<List<AddressModel>> getMyAddresses() async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Addresses/my-addresses'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    if (data is List) {
      return data.map((json) => AddressModel.fromJson(json)).toList();
    }
    return [];
  }

  Future<bool> addAddress({
    required String province,
    required String district,
    required String ward,
    required String addressDetails,
    required bool isDefault,
  }) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/Addresses'),
      headers: _getHeaders(),
      body: json.encode({
        'province': province,
        'district': district,
        'ward': ward,
        'addressDetails': addressDetails,
        'isDefault': isDefault,
      }),
    );
    return response.statusCode == 200;
  }

  Future<bool> updateAddress({
    required String addressId,
    required String province,
    required String district,
    required String ward,
    required String addressDetails,
    required bool isDefault,
  }) async {
    final response = await _client.put(
      Uri.parse('$baseUrl/Addresses/$addressId'),
      headers: _getHeaders(),
      body: json.encode({
        'province': province,
        'district': district,
        'ward': ward,
        'addressDetails': addressDetails,
        'isDefault': isDefault,
      }),
    );
    return response.statusCode == 200;
  }

  Future<bool> deleteAddress(String addressId) async {
    final response = await _client.delete(
      Uri.parse('$baseUrl/Addresses/$addressId'),
      headers: _getHeaders(),
    );
    return response.statusCode == 200;
  }

  // ============================================================
  // ORDERS & CHECKOUT (OrdersController)
  // ============================================================
  Future<Map<String, dynamic>> placeCodOrder({
    required String addressId,
    required List<CartDetailModel> items,
    String? voucherId,
  }) async {
    if (_customerId == null) {
      throw Exception('Unauthenticated user session.');
    }

    final List<Map<String, dynamic>> itemsJson = items.map((item) {
      return {
        'cartDetailId': item.cartDetailId,
        'productId': item.productId,
        'quantity': item.quantity,
        'unitPrice': item.product?.productPrice ?? 0.0,
      };
    }).toList();

    final response = await _client.post(
      Uri.parse('$baseUrl/Orders/Checkout'),
      headers: _getHeaders(),
      body: json.encode({
        'customerId': _customerId,
        'addressId': addressId,
        'voucherId': voucherId,
        'items': itemsJson,
      }),
    );

    return json.decode(response.body);
  }

  // Get customer order history
  Future<List<OrderModel>> getMyOrders() async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Orders/my-orders'),
      headers: _getHeaders(),
    );

    final data = _handleResponse(response);
    if (data is List) {
      return data.map((json) => OrderModel.fromJson(json)).toList();
    }
    return [];
  }

  // Get order details
  Future<OrderModel> getOrderDetails(String orderId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Orders/$orderId'),
      headers: _getHeaders(),
    );

    final data = _handleResponse(response);
    return OrderModel.fromJson(data);
  }

  // Cancel order
  Future<bool> cancelOrder(String orderId) async {
    final response = await _client.patch(
      Uri.parse('$baseUrl/Orders/$orderId/cancel'),
      headers: _getHeaders(),
    );

    return response.statusCode == 200;
  }

  // ============================================================
  // PET SERVICES (ServicesController)
  // ============================================================
  Future<List<ServiceModel>> getServices(
      {String? search, int? serviceType}) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Services'),
      headers: _getHeaders(),
    );

    final data = _handleResponse(response);
    List<ServiceModel> list = [];
    if (data is List) {
      list = data.map((json) => ServiceModel.fromJson(json)).toList();
    } else if (data is Map && data['value'] != null) {
      final List odataList = data['value'];
      list = odataList.map((json) => ServiceModel.fromJson(json)).toList();
    }

    if (search != null && search.trim().isNotEmpty) {
      final query = search.trim().toLowerCase();
      list = list
          .where((s) => s.serviceName.toLowerCase().contains(query))
          .toList();
    }

    if (serviceType != null && serviceType > 0) {
      list = list.where((s) => s.serviceType == serviceType).toList();
    }

    return list;
  }

  Future<ServiceModel> getServiceDetails(String serviceId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Services/$serviceId'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    return ServiceModel.fromJson(data);
  }

  // ============================================================
  // FORGOT & RESET PASSWORD (AuthsController)
  // ============================================================
  Future<Map<String, dynamic>> forgotPassword(String email) async {
    final response = await _sendRequest(() => _client.post(
          Uri.parse('$baseUrl/Auths/forgot-password'),
          headers: _getHeaders(),
          body: json.encode({
            'Email': email.trim(),
            'email': email.trim(),
          }),
        ));
    if (response.body.isEmpty)
      return {'success': false, 'message': 'Empty server response'};
    return json.decode(response.body);
  }

  Future<Map<String, dynamic>> resetPassword({
    required String email,
    required String token,
    required String newPassword,
    required String confirmPassword,
  }) async {
    final response = await _sendRequest(() => _client.post(
          Uri.parse('$baseUrl/Auths/reset-password'),
          headers: _getHeaders(),
          body: json.encode({
            'Email': email.trim(),
            'Token': token.trim(),
            'NewPassword': newPassword,
            'ConfirmPassword': confirmPassword,
            'email': email.trim(),
            'token': token.trim(),
            'newPassword': newPassword,
            'confirmPassword': confirmPassword,
          }),
        ));
    if (response.body.isEmpty)
      return {'success': false, 'message': 'Empty server response'};
    return json.decode(response.body);
  }

  // Change Password (AuthsController)
  Future<Map<String, dynamic>> changePassword({
    required String currentPassword,
    required String newPassword,
    required String confirmNewPassword,
  }) async {
    final response = await _sendRequest(() => _client.post(
      Uri.parse('$baseUrl/auths/change-password'),
      headers: _getHeaders(),
      body: json.encode({
        'currentPassword': currentPassword.trim(),
        'newPassword': newPassword.trim(),
        'confirmNewPassword': confirmNewPassword.trim(),
      }),
    ));
    if (response.body.isEmpty) return {'success': false, 'message': 'Empty server response'};
    return json.decode(response.body);
  }

  // ============================================================
  // PETS (PetsController)
  // ============================================================
  Future<List<PetModel>> getMyPets() async {
    final response = await _sendRequest(() => _client.get(
          Uri.parse('$baseUrl/Pets/my-pets'),
          headers: _getHeaders(),
        ));

    final data = _handleResponse(response);
    if (data is List) {
      return data.map((json) => PetModel.fromJson(json)).toList();
    }
    return [];
  }

  Future<PetModel> getPetDetails(String petId) async {
    final response = await _sendRequest(() => _client.get(
          Uri.parse('$baseUrl/Pets/$petId'),
          headers: _getHeaders(),
        ));

    final data = _handleResponse(response);
    return PetModel.fromJson(data);
  }

  Future<bool> addPet({
    required String petName,
    required String species,
    required String breed,
    required String gender,
    double? weight,
    String? note,
    String? dateOfBirth,
    File? imageFile,
  }) async {
    final uri = Uri.parse('$baseUrl/Pets');
    final request = http.MultipartRequest('POST', uri);

    if (_token != null) {
      request.headers['Authorization'] = 'Bearer $_token';
    }

    request.fields['PetName'] = petName;
    request.fields['Species'] = species;
    request.fields['Breed'] = breed;
    request.fields['Gender'] = gender;
    if (weight != null) request.fields['Weight'] = weight.toString();
    if (note != null && note.isNotEmpty) request.fields['Note'] = note;
    if (dateOfBirth != null && dateOfBirth.isNotEmpty)
      request.fields['DateOfBirth'] = dateOfBirth;

    if (imageFile != null) {
      request.files.add(await http.MultipartFile.fromPath(
        'ImageFile',
        imageFile.path,
      ));
    }

    final streamedResponse =
        await request.send().timeout(const Duration(seconds: 30));
    final response = await http.Response.fromStream(streamedResponse);
    return response.statusCode == 200;
  }

  Future<bool> updatePet({
    required String petId,
    required String petName,
    required String species,
    required String breed,
    required String gender,
    double? weight,
    String? note,
    String? dateOfBirth,
    File? imageFile,
  }) async {
    final uri = Uri.parse('$baseUrl/Pets/$petId');
    final request = http.MultipartRequest('PUT', uri);

    if (_token != null) {
      request.headers['Authorization'] = 'Bearer $_token';
    }

    request.fields['PetName'] = petName;
    request.fields['Species'] = species;
    request.fields['Breed'] = breed;
    request.fields['Gender'] = gender;
    if (weight != null) request.fields['Weight'] = weight.toString();
    if (note != null && note.isNotEmpty) request.fields['Note'] = note;
    if (dateOfBirth != null && dateOfBirth.isNotEmpty)
      request.fields['DateOfBirth'] = dateOfBirth;

    if (imageFile != null) {
      request.files.add(await http.MultipartFile.fromPath(
        'ImageFile',
        imageFile.path,
      ));
    }

    final streamedResponse =
        await request.send().timeout(const Duration(seconds: 30));
    final response = await http.Response.fromStream(streamedResponse);
    return response.statusCode == 200;
  }

  Future<bool> deletePet(String petId) async {
    final response = await _sendRequest(() => _client.delete(
          Uri.parse('$baseUrl/Pets/$petId'),
          headers: _getHeaders(),
        ));
    return response.statusCode == 200;
  }

  // ============================================================
  // UTILITIES
  // ============================================================
  dynamic _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return null;
      return json.decode(response.body);
    } else {
      throw Exception(
          'Request failed: ${response.statusCode} - ${response.body}');
    }
  }
}
