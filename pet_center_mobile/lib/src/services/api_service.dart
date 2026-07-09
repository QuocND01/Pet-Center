import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/customer_model.dart';
import '../models/product_model.dart';
import '../models/cart_model.dart';
import '../models/address_model.dart';

class ApiService {
  // Singleton pattern
  static final ApiService _instance = ApiService._internal();
  factory ApiService() => _instance;
  ApiService._internal();

  // Change to your computer's LAN IP when connecting with a physical phone (e.g. http://192.168.1.15:5163/api)
  static const String baseUrl = 'http://localhost:5163/api';

  final http.Client _client = http.Client();
  String? _token;
  String? _customerId;
  String? _customerEmail;

  // Save session auth data
  void setAuthData(String token, String customerId, String email) {
    _token = token;
    _customerId = customerId;
    _customerEmail = email;
  }

  void setToken(String token) {
    _token = token;
  }

  String? get token => _token;
  String? get customerId => _customerId;
  String? get customerEmail => _customerEmail;

  void clearAuthData() {
    _token = null;
    _customerId = null;
    _customerEmail = null;
  }

  Map<String, String> _getHeaders() {
    final headers = {'Content-Type': 'application/json'};
    if (_token != null) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  // ============================================================
  // AUTHENTICATION (AuthsController)
  // ============================================================

  // Login
  Future<Map<String, dynamic>> customerLogin(String email, String password) async {
    final response = await _client.post(
      Uri.parse('$baseUrl/auths/customer-login'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'email': email, 'password': password}),
    );

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
      } catch (e) {
        // Fallback
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

  // Get all products
  Future<List<ProductModel>> getProducts() async {
    final response = await _client.get(
      Uri.parse('$baseUrl/Products'),
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
    final isSuccess = jsonResult?['success'] == true || jsonResult?['Success'] == true;
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

  // ============================================================
  // UTILITIES
  // ============================================================
  dynamic _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return null;
      return json.decode(response.body);
    } else {
      throw Exception('Request failed: ${response.statusCode} - ${response.body}');
    }
  }
}
