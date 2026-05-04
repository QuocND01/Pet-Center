const STAR_COLOR = '#ffc107';

// wwwroot/js/order-feedback.js
const OrderFeedback = (function () {
    let currentOrderId = null;
    let orderProducts = [];
    let isInitialized = false;
    let cachedFeedbacks = [];

    function init(orderId, products) {
        if (isInitialized) return;
        currentOrderId = orderId;
        orderProducts = products;
        isInitialized = true;
        checkAndRenderButton();
    }

    async function checkAndRenderButton() {
        try {
            const response = await fetch(`/Feedback/CheckOrderFeedback?orderId=${currentOrderId}`);
            const result = await response.json();
            const container = document.getElementById('feedback-button-container');
            if (!container) return;

            if (result.success && result.hasFeedback) {
                container.innerHTML = `
                    <button type="button" class="btn btn-info text-white"
                            onclick="OrderFeedback.openViewFeedbackPopup()">
                        <i class="fas fa-eye"></i> Xem đánh giá
                    </button>`;
            } else {
                container.innerHTML = `
                    <button type="button" class="btn btn-primary"
                            onclick="OrderFeedback.openWriteFeedbackPopup()">
                        <i class="fas fa-star"></i> Viết đánh giá
                    </button>`;
            }
        } catch (error) {
            console.error('Error checking feedback:', error);
        }
    }

    // ===================== VIEW FEEDBACK =====================
    async function openViewFeedbackPopup() {
        try {
            const response = await fetch(`/Feedback/GetOrderFeedbacks?orderId=${currentOrderId}`);
            const result = await response.json();

            if (!result.success || !result.data || result.data.length === 0) {
                alert('Không thể tải đánh giá.');
                return;
            }

            cachedFeedbacks = result.data;

            const customerName = document.querySelector('meta[name="customer-name"]')?.content || 'Bạn';

            let feedbackHtml = '';
            result.data.forEach((fb, index) => {
                const product = orderProducts.find(
                    p => p.productId?.toLowerCase() === fb.productId?.toLowerCase()
                );
                const imgUrl = getImageUrl(product?.imageUrl);
                const rating = parseInt(fb.rating) || 0;
                const isLast = index === result.data.length - 1;
                const name = fb.customerName || customerName;
                const initials = name.trim().split(' ').filter(w => w).map(w => w[0].toUpperCase()).slice(0, 2).join('');

                const starsHtml = [1, 2, 3, 4, 5].map(i =>
                    `<span style="font-size:20px;color:${i <= rating ? STAR_COLOR : '#dee2e6'};">★</span>`
                ).join('');

                const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

                feedbackHtml += `
                <div style="padding:16px 0;${isLast ? '' : 'border-bottom:1px solid #f0f0f0;'}">
                    <div style="display:flex;align-items:flex-start;gap:12px;">
                        <!-- Avatar -->
                        <div style="width:40px;height:40px;border-radius:50%;
                                    background:#ee4d2d;color:#fff;flex-shrink:0;
                                    display:flex;align-items:center;justify-content:center;
                                    font-size:16px;font-weight:500;">
                            ${initials || '?'}
                        </div>

                        <div style="flex:1;min-width:0;">
                            <!-- Tên + ngày + nút Edit -->
                            <div style="display:flex;align-items:center;
                                        justify-content:space-between;margin-bottom:4px;">
                                <span style="font-size:13px;font-weight:500;color:#222;">
                                    ${name}
                                </span>
                                <div style="display:flex;align-items:center;gap:8px;">
                                    <span style="font-size:11px;color:#999;">
                                        ${formatDate(fb.createdDate)}
                                    </span>
                                    <button onclick="OrderFeedback.openEditPopup('${fb.feedbackId}')"
                                            style="font-size:11px;padding:2px 10px;
                                                   border:1px solid #ee4d2d;border-radius:4px;
                                                   background:#fff;color:#ee4d2d;cursor:pointer;">
                                        Sửa
                                    </button>
                                </div>
                            </div>

                            <!-- Sao + nhãn -->
                            <div style="display:flex;align-items:center;gap:6px;margin-bottom:8px;">
                                <div style="display:flex;gap:1px;">${starsHtml}</div>
                                <span style="font-size:12px;font-weight:500;color:${STAR_COLOR};">
                                    ${LABELS[rating] || ''}
                                </span>
                            </div>

                            <!-- Ảnh + tên sản phẩm -->
                            <div style="display:flex;align-items:center;gap:10px;
                                        background:#fafafa;border:1px solid #f0f0f0;
                                        border-radius:8px;padding:8px 10px;margin-bottom:8px;">
                                <img src="${imgUrl}"
                                     style="width:44px;height:44px;object-fit:cover;
                                            border-radius:6px;flex-shrink:0;"
                                     onerror="this.onerror=null;this.style.display='none'">
                                <span style="font-size:12px;color:#555;
                                             white-space:nowrap;overflow:hidden;
                                             text-overflow:ellipsis;">
                                    ${product?.productName || 'Sản phẩm'}
                                </span>
                            </div>

                            <!-- Nhận xét -->
                            <div style="font-size:13px;color:#333;line-height:1.6;">
                                ${fb.comment
                        ? fb.comment
                        : '<span style="color:#bbb;font-style:italic;">Không có nhận xét</span>'}
                            </div>

                            <!-- Phản hồi shop -->
                            ${fb.reply ? `
                            <div style="margin-top:10px;padding:10px 12px;
                                        background:#fff8f5;border-left:3px solid #ee4d2d;
                                        border-radius:0 6px 6px 0;">
                                <div style="font-size:11px;color:#ee4d2d;
                                            font-weight:500;margin-bottom:4px;">
                                    <i class="fas fa-store me-1"></i>Phản hồi từ cửa hàng
                                </div>
                                <div style="font-size:12px;color:#555;">${fb.reply}</div>
                            </div>` : ''}
                        </div>
                    </div>
                </div>`;
            });

            showModal('feedbackViewModal', 'Đánh giá của bạn', feedbackHtml,
                `<button class="btn btn-secondary" id="btnCloseViewModal">Đóng</button>`
            );

            // Gán sự kiện đóng View modal
            document.getElementById('btnCloseViewModal')?.addEventListener('click', () => closeModalById('feedbackViewModal'));
            document.getElementById('btnCloseX_feedbackViewModal')?.addEventListener('click', () => closeModalById('feedbackViewModal'));

        } catch (error) {
            console.error('Error loading feedbacks:', error);
            alert('Có lỗi xảy ra.');
        }
    }

    // ===================== EDIT POPUP =====================
    function openEditPopup(feedbackId) {
        const fb = cachedFeedbacks.find(f => f.feedbackId?.toLowerCase() === feedbackId?.toLowerCase());
        if (!fb) return;

        const product = orderProducts.find(
            p => p.productId?.toLowerCase() === fb.productId?.toLowerCase()
        );
        const imgUrl = getImageUrl(product?.imageUrl);
        const rating = parseInt(fb.rating) || 5;

        const starsHtml = [1, 2, 3, 4, 5].map(i => `
            <span class="star-btn-edit" data-rating="${i}"
                  style="font-size:28px;cursor:pointer;line-height:1;user-select:none;
                         color:${i <= rating ? STAR_COLOR : '#dee2e6'};
                         transition:color .1s,transform .1s;">★</span>
        `).join('');

        const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

        const bodyHtml = `
        <div style="margin-bottom:16px;">
            <div style="display:flex;align-items:center;gap:12px;
                        background:#fafafa;border:1px solid #f0f0f0;
                        border-radius:8px;padding:10px 12px;margin-bottom:16px;">
                <img src="${imgUrl}"
                     style="width:52px;height:52px;object-fit:cover;
                            border-radius:6px;flex-shrink:0;"
                     onerror="this.onerror=null;this.style.display='none'">
                <span style="font-size:13px;font-weight:500;color:#333;">
                    ${product?.productName || 'Sản phẩm'}
                </span>
            </div>
            <div style="margin-bottom:4px;font-size:12px;color:#666;">
                Chất lượng sản phẩm <span style="color:#e74c3c">*</span>
            </div>
            <div style="display:flex;align-items:center;gap:4px;margin-bottom:4px;">
                ${starsHtml}
                <span id="editRatingDesc"
                      style="font-size:12px;color:${STAR_COLOR};margin-left:8px;font-weight:500;">
                    ${LABELS[rating]}
                </span>
            </div>
            <input type="hidden" id="editRatingValue" value="${rating}">
            <div style="font-size:12px;color:#666;margin:12px 0 4px;">Nhận xét (tuỳ chọn)</div>
            <textarea id="editCommentInput"
                      style="width:100%;border:1px solid #ddd;border-radius:6px;
                             padding:10px;font-size:13px;resize:vertical;
                             min-height:100px;box-sizing:border-box;"
                      placeholder="Chia sẻ trải nghiệm của bạn...">${fb.comment || ''}</textarea>
            <input type="hidden" id="editFeedbackId" value="${fb.feedbackId}">
        </div>`;

        // Ẩn View modal trước
        const viewEl = document.getElementById('feedbackViewModal');
        const viewInstance = viewEl ? bootstrap.Modal.getInstance(viewEl) : null;
        if (viewInstance) viewInstance.hide();

        setTimeout(() => {
            document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
            document.body.classList.remove('modal-open');
            document.body.style.removeProperty('overflow');
            document.body.style.removeProperty('padding-right');

            showModal('feedbackEditModal', 'Chỉnh sửa đánh giá', bodyHtml,
                `<button class="btn btn-secondary" id="btnCancelEdit">Hủy</button>
                 <button class="btn btn-primary" id="btnSubmitEdit">Lưu thay đổi</button>`
            );

            // Gán sự kiện nút Cancel và X → quay lại View NGAY (không delay)
            function goBackToView() {
                const editEl = document.getElementById('feedbackEditModal');
                const editInstance = editEl ? bootstrap.Modal.getInstance(editEl) : null;
                if (editInstance) editInstance.hide();
                editEl?.remove();
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
                document.body.classList.remove('modal-open');
                document.body.style.removeProperty('overflow');
                document.body.style.removeProperty('padding-right');
                openViewFeedbackPopup(); // Mở lại View ngay
            }

            document.getElementById('btnCancelEdit')?.addEventListener('click', goBackToView);
            document.getElementById('btnCloseX_feedbackEditModal')?.addEventListener('click', goBackToView); // 👈 id đúng
            document.getElementById('btnSubmitEdit')?.addEventListener('click', submitEdit);

            setTimeout(attachEditStarEvents, 100);
        }, 350);
    }

    function attachEditStarEvents() {
        const stars = document.querySelectorAll('.star-btn-edit');
        const input = document.getElementById('editRatingValue');
        const desc = document.getElementById('editRatingDesc');
        const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

        function paint(upTo) {
            stars.forEach(s => {
                const v = parseInt(s.dataset.rating);
                s.style.color = v <= upTo ? STAR_COLOR : '#dee2e6';
                s.style.transform = v <= upTo ? 'scale(1.1)' : 'scale(1)';
            });
        }

        stars.forEach(star => {
            star.addEventListener('mouseenter', function () {
                paint(parseInt(this.dataset.rating));
                desc.textContent = LABELS[parseInt(this.dataset.rating)];
            });
            star.addEventListener('mouseleave', function () {
                const sel = parseInt(input.value);
                paint(sel);
                desc.textContent = LABELS[sel] || '';
            });
            star.addEventListener('click', function () {
                const v = parseInt(this.dataset.rating);
                input.value = v;
                paint(v);
                desc.textContent = LABELS[v];
            });
        });
    }

    async function submitEdit() {
        const feedbackId = document.getElementById('editFeedbackId')?.value;
        const rating = parseInt(document.getElementById('editRatingValue')?.value);
        const comment = document.getElementById('editCommentInput')?.value?.trim() || null;

        if (!feedbackId || rating < 1 || rating > 5) {
            alert('Vui lòng chọn số sao.');
            return;
        }

        const submitBtn = document.getElementById('btnSubmitEdit');
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Đang lưu...'; }

        try {
            const response = await fetch('/Feedback/Update', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ feedbackId, rating, comment })
            });

            if (!response.ok) {
                alert(`Lỗi server: ${response.status}`);
                return;
            }

            const result = await response.json();
            if (result.success) {
                // Đóng Edit modal → reload button (không mở lại View)
                closeModalById('feedbackEditModal', () => {
                    alert('Cập nhật đánh giá thành công!');
                    isInitialized = false;
                    checkAndRenderButton();
                });
            } else {
                alert(result.message || 'Có lỗi xảy ra.');
            }
        } catch (error) {
            console.error('Submit edit error:', error);
            alert('Có lỗi xảy ra. Vui lòng thử lại.');
        } finally {
            if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Lưu thay đổi'; }
        }
    }

    // ===================== WRITE FEEDBACK =====================
    function openWriteFeedbackPopup() {
        let formHtml = '';
        orderProducts.forEach(product => {
            const imgUrl = getImageUrl(product.imageUrl);
            const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

            const starsHtml = [1, 2, 3, 4, 5].map(i => `
                <span class="star-btn" data-rating="${i}"
                      style="font-size:28px;cursor:pointer;line-height:1;user-select:none;
                             color:${STAR_COLOR};
                             transition:color .1s,transform .1s;transform:scale(1.1);">★</span>
            `).join('');

            formHtml += `
            <div class="product-feedback" data-product-id="${product.productId}"
                 style="margin-bottom:20px;padding-bottom:20px;border-bottom:1px solid #f0f0f0;">
                <div style="display:flex;align-items:center;gap:12px;margin-bottom:14px;">
                    <img src="${imgUrl}"
                         style="width:52px;height:52px;object-fit:cover;
                                border-radius:6px;flex-shrink:0;"
                         onerror="this.onerror=null;this.style.display='none'">
                    <span style="font-size:13px;font-weight:500;color:#333;">
                        ${product.productName}
                    </span>
                </div>
                <div style="font-size:12px;color:#666;margin-bottom:6px;">
                    Chất lượng sản phẩm <span style="color:#e74c3c">*</span>
                </div>
                <div style="display:flex;align-items:center;gap:4px;margin-bottom:4px;" class="star-rating">
                    ${starsHtml}
                    <span class="rating-desc"
                          style="font-size:12px;color:${STAR_COLOR};margin-left:8px;font-weight:500;min-width:80px;">
                        ${LABELS[5]}
                    </span>
                </div>
                <input type="hidden" class="rating-value" value="5">
                <div class="rating-error" style="display:none;font-size:11px;color:#c0392b;margin-bottom:6px;">
                    Vui lòng chọn số sao
                </div>
                <div style="font-size:12px;color:#666;margin:12px 0 4px;">Nhận xét (tuỳ chọn)</div>
                <textarea class="comment-input"
                          style="width:100%;border:1px solid #ddd;border-radius:6px;
                                 padding:10px;font-size:13px;resize:vertical;
                                 min-height:100px;box-sizing:border-box;"
                          placeholder="Chia sẻ trải nghiệm của bạn..."></textarea>
            </div>`;
        });

        showModal('feedbackWriteModal', 'Đánh giá sản phẩm', formHtml,
            `<button class="btn btn-secondary" id="btnCancelWrite">Trở lại</button>
             <button class="btn btn-danger px-4" id="btnSubmitWrite">Hoàn thành</button>`
        );

        document.getElementById('btnCancelWrite')?.addEventListener('click', () => closeModalById('feedbackWriteModal'));
        document.getElementById('btnCloseX_feedbackWriteModal')?.addEventListener('click', () => closeModalById('feedbackWriteModal'));
        document.getElementById('btnSubmitWrite')?.addEventListener('click', submitFeedback);

        setTimeout(attachStarEvents, 100);
    }

    function attachStarEvents() {
        const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];
        document.querySelectorAll('.product-feedback').forEach(item => {
            const stars = item.querySelectorAll('.star-btn');
            const input = item.querySelector('.rating-value');
            const desc = item.querySelector('.rating-desc');
            const errMsg = item.querySelector('.rating-error');

            function paint(upTo) {
                stars.forEach(s => {
                    const v = parseInt(s.dataset.rating);
                    s.style.color = v <= upTo ? STAR_COLOR : '#dee2e6';
                    s.style.transform = v <= upTo ? 'scale(1.1)' : 'scale(1)';
                });
            }

            stars.forEach(star => {
                star.addEventListener('mouseenter', function () {
                    const v = parseInt(this.dataset.rating);
                    paint(v);
                    desc.textContent = LABELS[v];
                    desc.style.color = STAR_COLOR;
                });
                star.addEventListener('mouseleave', function () {
                    const sel = parseInt(input.value);
                    paint(sel);
                    desc.textContent = LABELS[sel] || '';
                });
                star.addEventListener('click', function () {
                    const v = parseInt(this.dataset.rating);
                    input.value = v;
                    if (errMsg) errMsg.style.display = 'none';
                    paint(v);
                    desc.textContent = LABELS[v];
                });
            });
        });
    }

    // ===================== SUBMIT WRITE =====================
    async function submitFeedback() {
        let isValid = true;
        document.querySelectorAll('.product-feedback').forEach(item => {
            const rating = parseInt(item.querySelector('.rating-value').value);
            const errMsg = item.querySelector('.rating-error');
            if (rating < 1 || rating > 5) {
                isValid = false;
                if (errMsg) errMsg.style.display = 'block';
            }
        });
        if (!isValid) return;

        const feedbacks = [];
        document.querySelectorAll('.product-feedback').forEach(item => {
            feedbacks.push({
                productId: item.getAttribute('data-product-id'),
                orderId: currentOrderId,
                rating: parseInt(item.querySelector('.rating-value').value),
                comment: item.querySelector('.comment-input').value.trim() || null
            });
        });

        const submitBtn = document.getElementById('btnSubmitWrite');
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Đang gửi...'; }

        try {
            const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            const headers = { 'Content-Type': 'application/json' };
            if (tokenEl) headers['RequestVerificationToken'] = tokenEl.value;

            const response = await fetch('/Feedback/CreateBulk', {
                method: 'POST',
                headers,
                body: JSON.stringify({ feedbacks })
            });

            if (!response.ok) {
                const errText = await response.text();
                console.error('Server error:', response.status, errText);
                alert(`Lỗi server: ${response.status}. Vui lòng thử lại.`);
                return;
            }

            const result = await response.json();
            if (result.success) {
                closeModalById('feedbackWriteModal', () => {
                    alert('Đánh giá đã được gửi thành công!');
                    isInitialized = false;
                    checkAndRenderButton();
                });
            } else {
                alert(result.message || 'Có lỗi xảy ra.');
            }
        } catch (error) {
            console.error('Submit feedback error:', error);
            alert('Có lỗi xảy ra. Vui lòng thử lại.');
        } finally {
            if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Hoàn thành'; }
        }
    }

    // ===================== HELPERS =====================

    // Hàm tạo modal dùng chung — mỗi modal có id riêng, nút X có id riêng
    function showModal(modalId, title, bodyHtml, customFooter) {
        document.getElementById(modalId)?.remove();

        const xBtnId = `btnCloseX_${modalId}`;

        document.body.insertAdjacentHTML('beforeend', `
            <div class="modal fade" id="${modalId}" tabindex="-1" data-bs-backdrop="static" data-bs-keyboard="false">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" id="${xBtnId}"></button>
                        </div>
                        <div class="modal-body" style="max-height:500px;overflow-y:auto;padding:16px 20px;">
                            ${bodyHtml}
                        </div>
                        <div class="modal-footer">${customFooter}</div>
                    </div>
                </div>
            </div>`);

        new bootstrap.Modal(document.getElementById(modalId)).show();
    }

    // Đóng modal theo id, dọn sạch backdrop, gọi callback nếu có
    function closeModalById(modalId, callback) {
        const el = document.getElementById(modalId);
        if (!el) {
            if (callback) callback();
            return;
        }
        const instance = bootstrap.Modal.getInstance(el);
        if (instance) instance.hide();

        setTimeout(() => {
            el.remove();
            document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
            document.body.classList.remove('modal-open');
            document.body.style.removeProperty('overflow');
            document.body.style.removeProperty('padding-right');
            if (callback) callback();
        }, 350);
    }

    function formatDate(dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleString('vi-VN');
    }

    function getImageUrl(url) {
        if (!url || url.trim() === '') {
            return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='60' height='60'%3E%3Crect width='60' height='60' fill='%23dee2e6'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%236c757d' font-size='10'%3ENo img%3C/text%3E%3C/svg%3E`;
        }
        return url;
    }

    return {
        init,
        openViewFeedbackPopup,
        openWriteFeedbackPopup,
        openEditPopup,
        submitFeedback,
        submitEdit
    };
})();