import 'dart:async';
import 'dart:convert';
import 'dart:io' show Platform, SocketException, File;
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:http/http.dart' as http;
import 'package:pet_center_mobile/src/models/ProductResponse.dart';
import '../models/ai_result_model.dart';
import '../models/brand_model.dart';
import '../models/category_model.dart';
import 'auth_service.dart';
import '../models/customer_model.dart';
import '../models/product_model.dart';
import '../models/cart_model.dart';
import '../models/address_model.dart';
import '../models/service_model.dart';
import '../models/pet_model.dart';
import '../models/product_feedback_model.dart';
import '../models/order_model.dart';
import '../models/order_feedback_input.dart';

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

  static String get odataBaseUrl {
    if (!kIsWeb && Platform.isAndroid) {
      return 'https://10.0.2.2:7004';
    }

    return 'https://localhost:7004';
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

  Future<List<BrandModel>> getBrands() async {
    final uri = Uri.parse(
      '$odataBaseUrl/odata/Brands',
    ).replace(
      queryParameters: {
        r'$count': 'true',
      },
    );

    final response = await _client.get(
      uri,
      headers: {
        'Accept': 'application/json',
        if (_token != null)
          'Authorization': 'Bearer $_token',
      },
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Failed to load brands: '
            '${response.statusCode} - ${response.body}',
      );
    }

    final Map<String, dynamic> json =
    jsonDecode(response.body);

    final List<dynamic> values =
        json['value'] ?? [];

    return values
        .map(
          (item) => BrandModel.fromJson(
        item as Map<String, dynamic>,
      ),
    )
        .toList();
  }


  Future<List<CategoryModel>> getCategories() async {
    final uri = Uri.parse(
      '$odataBaseUrl/odata/Categories',
    ).replace(
      queryParameters: {
        r'$count': 'true',
      },
    );

    final response = await _client.get(
      uri,
      headers: {
        'Accept': 'application/json',
        if (_token != null)
          'Authorization': 'Bearer $_token',
      },
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Failed to load categories: '
            '${response.statusCode} - ${response.body}',
      );
    }

    final Map<String, dynamic> json =
    jsonDecode(response.body);

    final List<dynamic> values =
        json['value'] ?? [];

    return values
        .map(
          (item) => CategoryModel.fromJson(
        item as Map<String, dynamic>,
      ),
    )
        .toList();
  }


  // ============================================================
  // PRODUCT CATALOG (ProductsController)
  // ============================================================
  Future<ProductResponse> getProducts({
    String? search,
    double? minPrice,
    double? maxPrice,
    DateTime? fromDate,
    DateTime? toDate,
    String? sortBy,
    String? categoryId,
    String? brandId,
    String sortOrder = 'asc',
    int page = 1,
  }) async {
    const int pageSize = 24;

    if (page < 1) {
      page = 1;
    }

    final List<String> filters = [];

    // =========================
    // SEARCH
    // =========================

    if (search != null &&
        search.trim().isNotEmpty) {
      final escapedSearch =
      search.trim().replaceAll("'", "''");

      filters.add(
        "contains(ProductName,'$escapedSearch')",
      );
    }

    // =========================
    // PRICE FILTER
    // =========================

    if (minPrice != null) {
      filters.add(
        'ProductPrice ge $minPrice',
      );
    }

    if (maxPrice != null) {
      filters.add(
        'ProductPrice le $maxPrice',
      );
    }

    // =========================
    // CATEGORY FILTER
    // =========================

    if (categoryId != null &&
        categoryId.isNotEmpty) {
      filters.add(
        'CategoryId eq $categoryId',
      );
    }

    // =========================
    // BRAND FILTER
    // =========================

    if (brandId != null &&
        brandId.isNotEmpty) {
      filters.add(
        'BrandId eq $brandId',
      );
    }

    // =========================
    // DATE FILTER
    // =========================

    if (fromDate != null) {
      filters.add(
        "AddedAt ge ${fromDate.toUtc().toIso8601String()}",
      );
    }

    if (toDate != null) {
      filters.add(
        "AddedAt le ${toDate.toUtc().toIso8601String()}",
      );
    }

    // =========================
    // QUERY PARAMETERS
    // =========================

    final Map<String, String> queryParameters = {
      r'$count': 'true',
      r'$skip': ((page - 1) * pageSize).toString(),
      r'$top': pageSize.toString(),
    };

    if (filters.isNotEmpty) {
      queryParameters[r'$filter'] =
          filters.join(' and ');
    }

    // =========================
    // SORT
    // =========================

    if (sortBy != null &&
        sortBy.isNotEmpty) {
      String column;

      switch (sortBy.toLowerCase()) {
        case 'price':
          column = 'ProductPrice';
          break;

        case 'name':
          column = 'ProductName';
          break;

        case 'date':
          column = 'AddedAt';
          break;

        default:
          column = 'ProductName';
      }

      final order =
      sortOrder.toLowerCase() == 'desc'
          ? 'desc'
          : 'asc';

      queryParameters[r'$orderby'] =
      '$column $order';
    }

    // =========================
    // BUILD URL
    // =========================

    final uri = Uri.parse(
      '$odataBaseUrl/odata/Products',
    ).replace(
      queryParameters:
      queryParameters,
    );

    // =========================
    // REQUEST
    // =========================

    final response = await _client.get(
      uri,
      headers: {
        'Accept': 'application/json',

        if (_token != null)
          'Authorization':
          'Bearer $_token',
      },
    );

    // =========================
    // RESPONSE
    // =========================

    if (response.statusCode == 200) {
      final Map<String, dynamic> json =
      jsonDecode(response.body);

      return ProductResponse.fromJson(
        json,
      );
    }

    throw Exception(
      'Failed to load products: '
          '${response.statusCode} - '
          '${response.body}',
    );
  }

  Future<List<ProductModel>>
  getHotProducts() async {
    final uri = Uri.parse(
      '$baseUrl/Products/hot-products',
    );

    final response = await _client.get(
      uri,
      headers: {
        'Accept': 'application/json',

        if (_token != null)
          'Authorization':
          'Bearer $_token',
      },
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Failed to load hot products: '
            '${response.statusCode} - '
            '${response.body}',
      );
    }

    final List<dynamic> json =
    jsonDecode(response.body);

    return json
        .map(
          (item) =>
          ProductModel.fromJson(
            item as Map<String, dynamic>,
          ),
    )
        .toList();
  }

  Future<List<ProductModel>>
  getNewProducts() async {
    final uri = Uri.parse(
      '$baseUrl/Products/new-products',
    );

    final response = await _client.get(
      uri,
      headers: {
        'Accept': 'application/json',

        if (_token != null)
          'Authorization':
          'Bearer $_token',
      },
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Failed to load new products: '
            '${response.statusCode} - '
            '${response.body}',
      );
    }

    final List<dynamic> json =
    jsonDecode(response.body);

    return json
        .map(
          (item) =>
          ProductModel.fromJson(
            item as Map<String, dynamic>,
          ),
    )
        .toList();
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

  // Check if order has feedback
  Future<bool> checkHasFeedback(String orderId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/ProductFeedbacks/check/$orderId'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    if (data != null && (data['success'] == true || data['Success'] == true)) {
      return data['hasFeedback'] == true || data['HasFeedback'] == true;
    }
    return false;
  }

  // Get feedbacks by order id
  Future<List<ProductFeedbackModel>> getFeedbacksByOrderId(String orderId) async {
    final response = await _client.get(
      Uri.parse('$baseUrl/ProductFeedbacks/order/$orderId'),
      headers: _getHeaders(),
    );
    final data = _handleResponse(response);
    if (data != null && (data['success'] == true || data['Success'] == true)) {
      final List list = data['data'] ?? data['Data'] ?? [];
      return list.map((json) => ProductFeedbackModel.fromJson(json)).toList();
    }
    return [];
  }

  // Submit bulk feedback for an order
  Future<Map<String, dynamic>> createBulkFeedback(List<ProductFeedbackInput> items) async {
    final uri = Uri.parse('$baseUrl/ProductFeedbacks/bulk');
    final request = http.MultipartRequest('POST', uri);

    final headers = _getHeaders();
    headers.forEach((key, value) {
      if (key != 'Content-Type') {
        request.headers[key] = value;
      }
    });

    for (int i = 0; i < items.length; i++) {
      final item = items[i];
      request.fields['Feedbacks[$i].ProductId'] = item.productId;
      request.fields['Feedbacks[$i].OrderId'] = item.orderId;
      request.fields['Feedbacks[$i].Rating'] = item.rating.toString();
      request.fields['Feedbacks[$i].Comment'] = item.comment;

      for (var imgFile in item.imageFiles) {
        request.files.add(await http.MultipartFile.fromPath(
          'Feedbacks[$i].MediaFiles',
          imgFile.path,
        ));
      }

      if (item.videoFile != null) {
        request.files.add(await http.MultipartFile.fromPath(
          'Feedbacks[$i].MediaFiles',
          item.videoFile!.path,
        ));
      }
    }

    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);
    if (response.body.isNotEmpty) {
      return json.decode(response.body);
    }
    return {'success': response.statusCode >= 200 && response.statusCode < 300};
  }

  // Update single feedback for a product
  Future<Map<String, dynamic>> updateFeedback({
    required String feedbackId,
    required int rating,
    String? comment,
    List<File>? newMediaFiles,
    List<String>? removedPublicIds,
  }) async {
    final uri = Uri.parse('$baseUrl/ProductFeedbacks/update');
    final request = http.MultipartRequest('POST', uri);

    final headers = _getHeaders();
    headers.forEach((key, value) {
      if (key != 'Content-Type') {
        request.headers[key] = value;
      }
    });

    request.fields['FeedbackId'] = feedbackId;
    request.fields['Rating'] = rating.toString();
    if (comment != null) {
      request.fields['Comment'] = comment;
    }

    if (removedPublicIds != null) {
      for (int i = 0; i < removedPublicIds.length; i++) {
        request.fields['RemovedPublicIds[$i]'] = removedPublicIds[i];
      }
    }

    if (newMediaFiles != null) {
      for (var file in newMediaFiles) {
        request.files.add(await http.MultipartFile.fromPath(
          'NewMediaFiles',
          file.path,
        ));
      }
    }

    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);
    if (response.body.isNotEmpty) {
      return json.decode(response.body);
    }
    return {'success': response.statusCode >= 200 && response.statusCode < 300};
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


  Future<AIResultModel> classifyAI(
      File image,
      ) async {
    final uri = Uri.parse(
      '$baseUrl/AI/predict',
    );

    final request =
    http.MultipartRequest(
      'POST',
      uri,
    );
    request.files.add(
      await http.MultipartFile.fromPath(
        'file',
        image.path,
      ),
    );

    final streamedResponse =
    await request.send();

    final response =
    await http.Response.fromStream(
      streamedResponse,
    );

    if (response.statusCode < 200 ||
        response.statusCode >= 300) {
      throw Exception(
        'Unable to classify image. '
            'Status code: ${response.statusCode}',
      );
    }

    final Map<String, dynamic> json =
    jsonDecode(response.body);

    return AIResultModel.fromJson(json);
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
