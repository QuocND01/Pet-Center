(function () {
  'use strict';

  // ── Config (set by _Layout.cshtml before this script loads) ──────────────
  var RASA_URL = window.__RASA_URL || 'http://localhost:5005';
  var API_URL  = window.__API_URL  || 'https://localhost:7004';

  function getJwt()        { return window.__JWT || ''; }
  function getCustomerId() { return window.__CUSTOMER_ID || ''; }

  // Metadata gắn vào MỖI tin nhắn → Action Server (Python) đọc để gọi API (Pattern A)
  function getMetadata() {
    return { customer_id: getCustomerId(), jwt: getJwt() };
  }

  function getSenderId() {
    var id = sessionStorage.getItem('pc_chat_sid');
    if (!id) {
      id = 'u_' + Date.now() + '_' + Math.random().toString(36).slice(2, 7);
      sessionStorage.setItem('pc_chat_sid', id);
    }
    return id;
  }

  // ── Styles ─────────────────────────────────────────────────────────────────
  var css = `
    #pc-btn {
      position: fixed; bottom: 24px; right: 24px; z-index: 9998;
      width: 56px; height: 56px; border-radius: 50%;
      background: #22c55e; border: none; cursor: pointer;
      box-shadow: 0 4px 20px rgba(34,197,94,.45);
      font-size: 26px; transition: transform .2s, box-shadow .2s;
      display: flex; align-items: center; justify-content: center;
    }
    #pc-btn:hover { transform: scale(1.1); box-shadow: 0 6px 28px rgba(34,197,94,.55); }

    #pc-panel {
      position: fixed; bottom: 90px; right: 24px; z-index: 9999;
      width: 360px; max-height: 540px;
      background: #fff; border-radius: 18px;
      box-shadow: 0 8px 40px rgba(0,0,0,.16);
      display: none; flex-direction: column; overflow: hidden;
      font-family: 'Segoe UI', sans-serif; font-size: 13.5px;
    }
    #pc-panel.open { display: flex; }

    .pc-head {
      background: linear-gradient(135deg, #22c55e, #16a34a);
      padding: 13px 16px; display: flex; align-items: center; gap: 10px; color: #fff;
    }
    .pc-head-avatar {
      width: 36px; height: 36px; border-radius: 50%;
      background: rgba(255,255,255,.25);
      display: flex; align-items: center; justify-content: center; font-size: 20px;
    }
    .pc-head-info { flex: 1; }
    .pc-head-title { font-weight: 700; font-size: 14px; }
    .pc-head-sub { font-size: 11px; opacity: .85; }
    .pc-close {
      background: none; border: none; color: #fff;
      font-size: 20px; cursor: pointer; opacity: .8; padding: 0 2px;
      line-height: 1;
    }
    .pc-close:hover { opacity: 1; }

    .pc-msgs {
      flex: 1; overflow-y: auto; padding: 14px 12px;
      display: flex; flex-direction: column; gap: 8px; background: #f8fafc;
    }

    .pc-msg {
      max-width: 82%; padding: 9px 13px; border-radius: 14px;
      line-height: 1.55; white-space: pre-wrap; word-break: break-word;
    }
    .pc-bot {
      background: #fff; border: 1px solid #e5e7eb; color: #1e293b;
      border-radius: 14px 14px 14px 3px; align-self: flex-start;
    }
    .pc-user {
      background: #22c55e; color: #fff;
      border-radius: 14px 14px 3px 14px; align-self: flex-end;
    }
    .pc-system {
      background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0;
      border-radius: 8px; font-size: 12.5px; align-self: center;
      text-align: center; max-width: 95%;
    }
    .pc-err {
      background: #fff7ed; color: #9a3412; border: 1px solid #fed7aa;
      border-radius: 8px; font-size: 12.5px; align-self: center;
      text-align: center; max-width: 95%;
    }

    .pc-btns {
      display: flex; flex-direction: column; gap: 5px;
      align-self: flex-start; max-width: 88%;
    }
    .pc-btn-choice {
      background: #fff; border: 1.5px solid #22c55e; color: #16a34a;
      padding: 7px 14px; border-radius: 20px; font-size: 13px; font-weight: 600;
      cursor: pointer; text-align: left; transition: .15s; font-family: inherit;
    }
    .pc-btn-choice:hover { background: #dcfce7; }

    .pc-typing {
      display: flex; gap: 4px; padding: 10px 14px; align-self: flex-start;
      background: #fff; border: 1px solid #e5e7eb; border-radius: 14px;
    }
    .pc-typing span {
      width: 7px; height: 7px; border-radius: 50%; background: #94a3b8;
      animation: pcBounce 1.2s ease-in-out infinite;
    }
    .pc-typing span:nth-child(2) { animation-delay: .2s; }
    .pc-typing span:nth-child(3) { animation-delay: .4s; }
    @keyframes pcBounce {
      0%, 60%, 100% { transform: translateY(0); }
      30%           { transform: translateY(-7px); }
    }

    .pc-foot {
      padding: 10px 12px; border-top: 1px solid #e5e7eb;
      display: flex; gap: 8px; background: #fff;
    }
    .pc-input {
      flex: 1; border: 1.5px solid #e5e7eb; border-radius: 24px;
      padding: 8px 14px; font-size: 13.5px; outline: none;
      font-family: inherit; transition: border-color .2s;
    }
    .pc-input:focus { border-color: #22c55e; }
    .pc-send {
      width: 38px; height: 38px; border-radius: 50%; background: #22c55e;
      border: none; color: #fff; cursor: pointer; font-size: 16px;
      display: flex; align-items: center; justify-content: center;
      transition: background .2s; flex-shrink: 0;
    }
    .pc-send:hover   { background: #16a34a; }
    .pc-send:disabled { background: #94a3b8; cursor: default; }

    @media (max-width: 420px) {
      #pc-panel { width: calc(100vw - 24px); right: 12px; bottom: 80px; }
    }
  `;

  // ── Inject CSS ─────────────────────────────────────────────────────────────
  var styleEl = document.createElement('style');
  styleEl.textContent = css;
  document.head.appendChild(styleEl);

  // ── Inject HTML ────────────────────────────────────────────────────────────
  var wrap = document.createElement('div');
  wrap.innerHTML = `
    <button id="pc-btn" title="Chat với PetCenter Bot">🐾</button>
    <div id="pc-panel">
      <div class="pc-head">
        <div class="pc-head-avatar">🐾</div>
        <div class="pc-head-info">
          <div class="pc-head-title">PetCenter Bot</div>
          <div class="pc-head-sub">Hỗ trợ mua sắm 24/7</div>
        </div>
        <button class="pc-close" id="pc-close">✕</button>
      </div>
      <div class="pc-msgs" id="pc-msgs"></div>
      <div class="pc-foot">
        <input class="pc-input" id="pc-input" type="text" placeholder="Nhập tin nhắn..." autocomplete="off" />
        <button class="pc-send" id="pc-send">➤</button>
      </div>
    </div>
  `;
  document.body.appendChild(wrap);

  var btn    = document.getElementById('pc-btn');
  var panel  = document.getElementById('pc-panel');
  var close  = document.getElementById('pc-close');
  var msgs   = document.getElementById('pc-msgs');
  var input  = document.getElementById('pc-input');
  var sendEl = document.getElementById('pc-send');

  // ── Lưu/khôi phục lịch sử qua sessionStorage (giữ chat khi chuyển trang) ──
  var HKEY = 'pc_chat_history';   // mảng tin nhắn đã hiển thị
  var restoring = false;          // true khi đang khôi phục → không lưu trùng

  function loadHistory() {
    try { return JSON.parse(sessionStorage.getItem(HKEY)) || []; }
    catch (e) { return []; }
  }
  function recordHistory(entry) {
    if (restoring) return;
    var h = loadHistory();
    h.push(entry);
    if (h.length > 100) h = h.slice(-100);   // giới hạn 100 tin gần nhất
    sessionStorage.setItem(HKEY, JSON.stringify(h));
  }
  function isGreeted() { return sessionStorage.getItem('pc_chat_greeted') === '1'; }
  function setGreeted() { sessionStorage.setItem('pc_chat_greeted', '1'); }
  function isOpen() { return sessionStorage.getItem('pc_chat_open') === '1'; }
  function setOpen(v) { sessionStorage.setItem('pc_chat_open', v ? '1' : '0'); }

  // ── Helpers ────────────────────────────────────────────────────────────────
  function scrollBottom() { msgs.scrollTop = msgs.scrollHeight; }

  function addMsg(text, cls) {
    var d = document.createElement('div');
    d.className = 'pc-msg ' + cls;
    d.textContent = text;
    msgs.appendChild(d);
    scrollBottom();
    recordHistory({ kind: 'msg', text: text, cls: cls });
    return d;
  }

  function addButtons(buttons) {
    var wrap = document.createElement('div');
    wrap.className = 'pc-btns';
    buttons.forEach(function (b) {
      var btn = document.createElement('button');
      btn.className = 'pc-btn-choice';
      btn.textContent = b.title;
      btn.addEventListener('click', function () {
        wrap.remove();
        sendMsg(b.payload, b.title);
      });
      wrap.appendChild(btn);
    });
    msgs.appendChild(wrap);
    scrollBottom();
    recordHistory({ kind: 'buttons', buttons: buttons });
  }

  // Vẽ lại toàn bộ lịch sử đã lưu (không gọi API, không lưu lại)
  function restoreHistory() {
    var h = loadHistory();
    if (!h.length) return false;
    restoring = true;
    h.forEach(function (e) {
      if (e.kind === 'msg') addMsg(e.text, e.cls);
      else if (e.kind === 'buttons') addButtons(e.buttons);
    });
    restoring = false;
    return true;
  }

  function showTyping() {
    var d = document.createElement('div');
    d.className = 'pc-typing';
    d.innerHTML = '<span></span><span></span><span></span>';
    msgs.appendChild(d);
    scrollBottom();
    return d;
  }

  // ── Cart API call (done here in browser so JWT stays in browser) ──────────
  async function addToCart(productId, productName, quantity) {
    var jwt = getJwt();
    if (!jwt) {
      addMsg('🔒 Bạn cần đăng nhập để thêm vào giỏ hàng!', 'pc-system');
      return;
    }
    try {
      var resp = await fetch(API_URL + '/api/cart/add', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + jwt
        },
        body: JSON.stringify({ productId: productId, quantity: quantity || 1 })
      });
      var data = await resp.json();
      if (resp.ok) {
        addMsg('✅ Đã thêm "' + productName + '" vào giỏ hàng!', 'pc-system');
        if (window.refreshCartBadge) window.refreshCartBadge();
      } else {
        addMsg('❌ ' + (data.message || 'Thêm vào giỏ thất bại.'), 'pc-err');
      }
    } catch (e) {
      addMsg('❌ Không thể kết nối. Vui lòng thử lại!', 'pc-err');
    }
  }

  // ── Cancel order (Pattern B: JWT ở browser) ───────────────────────────────
  async function cancelOrder(orderId) {
    var jwt = getJwt();
    if (!jwt) {
      addMsg('🔒 Bạn cần đăng nhập để hủy đơn hàng!', 'pc-system');
      return;
    }
    try {
      var resp = await fetch(API_URL + '/api/orders/' + orderId + '/cancel', {
        method: 'PATCH',
        headers: { 'Authorization': 'Bearer ' + jwt }
      });
      var data = await resp.json();
      if (resp.ok) addMsg('✅ Đã hủy đơn hàng thành công!', 'pc-system');
      else         addMsg('❌ ' + (data.message || 'Hủy đơn thất bại.'), 'pc-err');
    } catch (e) {
      addMsg('❌ Không thể kết nối. Vui lòng thử lại!', 'pc-err');
    }
  }

  // ── Handle RASA responses ─────────────────────────────────────────────────
  async function handleResponses(responses) {
    for (var i = 0; i < responses.length; i++) {
      var r = responses[i];
      if (r.text)    addMsg(r.text, 'pc-bot');
      if (r.buttons && r.buttons.length) addButtons(r.buttons);
      if (r.custom) {
        if (r.custom.type === 'add_to_cart') {
          await addToCart(r.custom.productId, r.custom.productName, r.custom.quantity);
        } else if (r.custom.type === 'cancel_order') {
          await cancelOrder(r.custom.orderId);
        } else if (r.custom.type === 'navigate' && r.custom.url) {
          addMsg('👉 Đang chuyển bạn tới trang...', 'pc-system');
          setTimeout(function () { window.location.href = r.custom.url; }, 1200);
        }
      }
    }
  }

  // ── Send to RASA ──────────────────────────────────────────────────────────
  async function sendMsg(payload, displayText) {
    var text = (payload || '').trim();
    if (!text) return;

    addMsg(displayText || text, 'pc-user');
    input.value = '';
    setLoading(true);
    var typing = showTyping();

    try {
      var resp = await fetch(RASA_URL + '/webhooks/rest/webhook', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sender: getSenderId(), message: text, metadata: getMetadata() })
      });
      typing.remove();
      var data = await resp.json();
      if (!data || data.length === 0) {
        addMsg('Xin lỗi, tôi chưa hiểu. Bạn có thể hỏi lại nhé!', 'pc-bot');
      } else {
        await handleResponses(data);
      }
    } catch (e) {
      typing.remove();
      addMsg('⚠️ Không thể kết nối đến chatbot. RASA chưa chạy hoặc đang khởi động!', 'pc-err');
    } finally {
      setLoading(false);
      input.focus();
    }
  }

  function setLoading(on) {
    input.disabled = on;
    sendEl.disabled = on;
  }

  // ── Greeting (chỉ gọi 1 lần khi mở chat lần đầu) ─────────────────────────
  async function initGreeting() {
    var typing = showTyping();
    try {
      var resp = await fetch(RASA_URL + '/webhooks/rest/webhook', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sender: getSenderId(), message: '/greet', metadata: getMetadata() })
      });
      var data = await resp.json();
      typing.remove();
      await handleResponses(data);
    } catch {
      typing.remove();
      addMsg('Xin chào! 🐾 Tôi là trợ lý PetCenter. Bạn cần gì?', 'pc-bot');
    }
  }

  // ── Events ────────────────────────────────────────────────────────────────
  btn.addEventListener('click', async function () {
    panel.classList.toggle('open');
    var open = panel.classList.contains('open');
    setOpen(open);
    if (open) {
      input.focus();
      scrollBottom();
      if (!isGreeted()) { setGreeted(); await initGreeting(); }
    }
  });

  close.addEventListener('click', function () {
    panel.classList.remove('open');
    setOpen(false);
  });

  sendEl.addEventListener('click', function () { sendMsg(input.value); });

  input.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMsg(input.value); }
  });

  // ── Khôi phục khi load trang (giữ chat liên tục khi chuyển trang) ─────────
  (function init() {
    var hadHistory = restoreHistory();
    if (isOpen()) {
      panel.classList.add('open');
      scrollBottom();
      // Nếu mở sẵn mà chưa từng chào (vd mở tab mới) thì chào
      if (!hadHistory && !isGreeted()) { setGreeted(); initGreeting(); }
    }
  })();

})();
