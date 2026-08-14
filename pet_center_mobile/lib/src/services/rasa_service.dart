import 'dart:async';
import 'dart:convert';
import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb, debugPrint;
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'auth_service.dart';
import 'api_service.dart';

class RasaButton {
  final String title;
  final String payload;

  RasaButton({required this.title, required this.payload});

  factory RasaButton.fromJson(Map<String, dynamic> json) {
    return RasaButton(
      title: json['title'] ?? '',
      payload: json['payload'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'title': title,
      'payload': payload,
    };
  }
}

class RasaChatMessage {
  final String id;
  final String sender; // 'bot', 'user', 'system', 'error'
  final String text;
  final List<RasaButton>? buttons;
  final Map<String, dynamic>? custom;
  final DateTime timestamp;

  RasaChatMessage({
    required this.id,
    required this.sender,
    required this.text,
    this.buttons,
    this.custom,
    DateTime? timestamp,
  }) : timestamp = timestamp ?? DateTime.now();

  factory RasaChatMessage.fromJson(Map<String, dynamic> json) {
    return RasaChatMessage(
      id: json['id'] ?? '',
      sender: json['sender'] ?? 'bot',
      text: json['text'] ?? '',
      buttons: (json['buttons'] as List<dynamic>?)
          ?.map((b) => RasaButton.fromJson(b as Map<String, dynamic>))
          .toList(),
      custom: json['custom'] as Map<String, dynamic>?,
      timestamp: json['timestamp'] != null
          ? DateTime.tryParse(json['timestamp']) ?? DateTime.now()
          : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'sender': sender,
      'text': text,
      'buttons': buttons?.map((b) => b.toJson()).toList(),
      'custom': custom,
      'timestamp': timestamp.toIso8601String(),
    };
  }
}

class RasaService {
  static final RasaService _instance = RasaService._internal();
  factory RasaService() => _instance;
  RasaService._internal();

  static const String _historyKey = 'pc_rasa_chat_history';
  static const String _senderIdKey = 'pc_rasa_sender_id';

  static String get rasaUrl {
    if (!kIsWeb && Platform.isAndroid) {
      return 'http://10.0.2.2:5005';
    }
    return 'http://localhost:5005';
  }

  String? _senderId;

  Future<String> getSenderId() async {
    if (_senderId != null) return _senderId!;

    final prefs = await SharedPreferences.getInstance();
    var id = prefs.getString(_senderIdKey);
    if (id == null || id.isEmpty) {
      id = 'u_${DateTime.now().millisecondsSinceEpoch}_${(1000 + DateTime.now().microsecond % 9000)}';
      await prefs.setString(_senderIdKey, id);
    }
    _senderId = id;
    return id;
  }

  Future<void> clearSession() async {
    _senderId = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_historyKey);
    await prefs.remove(_senderIdKey);
  }

  Future<List<RasaChatMessage>> loadHistory() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyStr = prefs.getString(_historyKey);
      if (historyStr != null && historyStr.isNotEmpty) {
        final List<dynamic> jsonList = jsonDecode(historyStr);
        return jsonList.map((item) => RasaChatMessage.fromJson(item)).toList();
      }
    } catch (e) {
      debugPrint('Error loading chat history: $e');
    }
    return [];
  }

  Future<void> saveHistory(List<RasaChatMessage> messages) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      // Keep up to 50 most recent messages to prevent storage bloat
      var listToSave = messages;
      if (listToSave.length > 50) {
        listToSave = listToSave.sublist(listToSave.length - 50);
      }
      final jsonString = jsonEncode(listToSave.map((m) => m.toJson()).toList());
      await prefs.setString(_historyKey, jsonString);
    } catch (e) {
      debugPrint('Error saving chat history: $e');
    }
  }

  Future<List<RasaChatMessage>> sendMessage(String messageText, {String? customPayload}) async {
    final session = await AuthService().loadSession();
    final jwt = session['token'] ?? '';
    final customerId = session['customerId'] ?? '';
    final sid = await getSenderId();

    final payloadText = customPayload ?? messageText;
    final url = Uri.parse('$rasaUrl/webhooks/rest/webhook');

    final bodyData = {
      'sender': sid,
      'message': payloadText,
      'metadata': {
        'customer_id': customerId,
        'jwt': jwt,
      }
    };

    try {
      final response = await http
          .post(
            url,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode(bodyData),
          )
          .timeout(const Duration(seconds: 45));

      if (response.statusCode == 200) {
        final List<dynamic> responseList = jsonDecode(response.body);
        final List<RasaChatMessage> messages = [];

        for (var item in responseList) {
          final text = item['text'] as String?;
          final buttonsJson = item['buttons'] as List<dynamic>?;
          final customJson = item['custom'] as Map<String, dynamic>?;

          List<RasaButton>? buttons;
          if (buttonsJson != null && buttonsJson.isNotEmpty) {
            buttons = buttonsJson.map((b) => RasaButton.fromJson(b)).toList();
          }

          if (text != null && text.isNotEmpty) {
            messages.add(
              RasaChatMessage(
                id: 'bot_${DateTime.now().microsecondsSinceEpoch}',
                sender: 'bot',
                text: text,
                buttons: buttons,
                custom: customJson,
              ),
            );
          }

          // Handle custom payload actions directly
          if (customJson != null) {
            await _handleCustomAction(customJson, messages);
          }
        }

        return messages;
      } else {
        return [
          RasaChatMessage(
            id: 'err_${DateTime.now().microsecondsSinceEpoch}',
            sender: 'error',
            text: '⚠️ Không thể xử lý yêu cầu. Mã lỗi: ${response.statusCode}',
          )
        ];
      }
    } catch (e) {
      debugPrint('Rasa Connection Error: $e');
      return [
        RasaChatMessage(
          id: 'err_${DateTime.now().microsecondsSinceEpoch}',
          sender: 'error',
          text: '⚠️ Không thể kết nối tới Rasa Chatbot ($rasaUrl). Vui lòng kiểm tra server Rasa.',
        )
      ];
    }
  }

  Future<void> _handleCustomAction(Map<String, dynamic> custom, List<RasaChatMessage> messages) async {
    final type = custom['type'] as String?;
    if (type == null) return;

    try {
      if (type == 'add_to_cart') {
        final productId = custom['productId']?.toString();
        final productName = custom['productName']?.toString() ?? 'Sản phẩm';
        final quantity = (custom['quantity'] as num?)?.toInt() ?? 1;

        if (productId != null) {
          final success = await ApiService().addToCart(productId, quantity);
          if (success) {
            messages.add(
              RasaChatMessage(
                id: 'sys_${DateTime.now().microsecondsSinceEpoch}',
                sender: 'system',
                text: '✅ Đã thêm "$productName" vào giỏ hàng!',
              ),
            );
          } else {
            messages.add(
              RasaChatMessage(
                id: 'err_${DateTime.now().microsecondsSinceEpoch}',
                sender: 'error',
                text: '❌ Không thể thêm "$productName" vào giỏ hàng.',
              ),
            );
          }
        }
      } else if (type == 'cancel_order') {
        final orderId = custom['orderId']?.toString();
        if (orderId != null) {
          final success = await ApiService().cancelOrder(orderId);
          if (success) {
            messages.add(
              RasaChatMessage(
                id: 'sys_${DateTime.now().microsecondsSinceEpoch}',
                sender: 'system',
                text: '✅ Đã hủy đơn hàng #$orderId thành công!',
              ),
            );
          } else {
            messages.add(
              RasaChatMessage(
                id: 'err_${DateTime.now().microsecondsSinceEpoch}',
                sender: 'error',
                text: '❌ Hủy đơn hàng thất bại.',
              ),
            );
          }
        }
      }
    } catch (e) {
      debugPrint('Error handling Rasa custom action: $e');
    }
  }
}
