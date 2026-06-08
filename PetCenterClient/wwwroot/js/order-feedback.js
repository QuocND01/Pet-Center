const STAR_COLOR = '#ffc107';
const MAX_MEDIA = 5; // Tối đa 5 file mỗi feedback
const MAX_IMAGE_MB = 5;
const MAX_VIDEO_MB = 50;
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'video/mp4', 'video/webm'];

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
                const initials = name.trim().split(' ')
                    .filter(w => w).map(w => w[0].toUpperCase()).slice(0, 2).join('');

                const starsHtml = [1, 2, 3, 4, 5].map(i =>
                    `<span style="font-size:20px;color:${i <= rating ? STAR_COLOR : '#dee2e6'};">★</span>`
                ).join('');

                const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

                // Render media files (ảnh/video)
                const mediaHtml = renderMediaPreview(fb.mediaFiles || []);

                feedbackHtml += `
                <div style="padding:16px 0;${isLast ? '' : 'border-bottom:1px solid #f0f0f0;'}">
                    <div style="display:flex;align-items:flex-start;gap:12px;">
                        <div style="width:40px;height:40px;border-radius:50%;
                                    background:#ee4d2d;color:#fff;flex-shrink:0;
                                    display:flex;align-items:center;justify-content:center;
                                    font-size:16px;font-weight:500;">
                            ${initials || '?'}
                        </div>
                        <div style="flex:1;min-width:0;">
                            <div style="display:flex;align-items:center;
                                        justify-content:space-between;margin-bottom:4px;">
                                <span style="font-size:13px;font-weight:500;color:#222;">${name}</span>
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
                            <div style="display:flex;align-items:center;gap:6px;margin-bottom:8px;">
                                <div style="display:flex;gap:1px;">${starsHtml}</div>
                                <span style="font-size:12px;font-weight:500;color:${STAR_COLOR};">
                                    ${LABELS[rating] || ''}
                                </span>
                            </div>
                            <div style="display:flex;align-items:center;gap:10px;
                                        background:#fafafa;border:1px solid #f0f0f0;
                                        border-radius:8px;padding:8px 10px;margin-bottom:8px;">
                                <img src="${imgUrl}"
                                     style="width:44px;height:44px;object-fit:cover;
                                            border-radius:6px;flex-shrink:0;"
                                     onerror="this.onerror=null;this.style.display='none'">
                                <span style="font-size:12px;color:#555;white-space:nowrap;
                                             overflow:hidden;text-overflow:ellipsis;">
                                    ${product?.productName || 'Sản phẩm'}
                                </span>
                            </div>
                            <div style="font-size:13px;color:#333;line-height:1.6;">
                                ${fb.comment
                        ? fb.comment
                        : '<span style="color:#bbb;font-style:italic;">Không có nhận xét</span>'}
                            </div>
                            ${mediaHtml}
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

            document.getElementById('btnCloseViewModal')
                ?.addEventListener('click', () => closeModalById('feedbackViewModal'));
            document.getElementById('btnCloseX_feedbackViewModal')
                ?.addEventListener('click', () => closeModalById('feedbackViewModal'));

        } catch (error) {
            console.error('Error loading feedbacks:', error);
            alert('Có lỗi xảy ra.');
        }
    }

    // Render ảnh/video từ mediaFiles trả về
    function renderMediaPreview(mediaFiles) {
        if (!mediaFiles || mediaFiles.length === 0) return '';

        const items = mediaFiles.map(m => {
            if (m.mediaType === 'video') {
                return `
                <div style="position:relative;width:80px;height:80px;flex-shrink:0;">
                    <video src="${m.mediaUrl}"
                           style="width:80px;height:80px;object-fit:cover;
                                  border-radius:6px;border:1px solid #eee;cursor:pointer;"
                           onclick="OrderFeedback.openMediaViewer('${m.mediaUrl}', 'video')"
                           muted playsinline>
                    </video>
                    <div style="position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);
                                background:rgba(0,0,0,0.5);border-radius:50%;
                                width:28px;height:28px;display:flex;
                                align-items:center;justify-content:center;pointer-events:none;">
                        <span style="color:#fff;font-size:12px;">▶</span>
                    </div>
                </div>`;
            }
            return `
            <img src="${m.mediaUrl}"
                 style="width:80px;height:80px;object-fit:cover;border-radius:6px;
                        border:1px solid #eee;cursor:pointer;flex-shrink:0;"
                 onclick="OrderFeedback.openMediaViewer('${m.mediaUrl}', 'image')"
                 onerror="this.onerror=null;this.style.display='none'">`;
        }).join('');

        return `
        <div style="display:flex;flex-wrap:wrap;gap:8px;margin-top:10px;">
            ${items}
        </div>`;
    }

    // Mở fullscreen xem ảnh/video
    function openMediaViewer(url, type) {
        document.getElementById('mediaViewerOverlay')?.remove();

        const mediaEl = type === 'video'
            ? `<video src="${url}" controls autoplay
                      style="max-width:90vw;max-height:85vh;border-radius:8px;"></video>`
            : `<img src="${url}"
                    style="max-width:90vw;max-height:85vh;border-radius:8px;object-fit:contain;">`;

        document.body.insertAdjacentHTML('beforeend', `
            <div id="mediaViewerOverlay"
                 style="position:fixed;inset:0;background:rgba(0,0,0,0.85);
                        z-index:9999;display:flex;align-items:center;
                        justify-content:center;cursor:pointer;"
                 onclick="document.getElementById('mediaViewerOverlay').remove()">
                ${mediaEl}
                <button style="position:absolute;top:16px;right:20px;background:none;
                               border:none;color:#fff;font-size:28px;cursor:pointer;"
                        onclick="document.getElementById('mediaViewerOverlay').remove()">✕</button>
            </div>`);
    }

    // ===================== EDIT POPUP =====================
    function openEditPopup(feedbackId) {
        const fb = cachedFeedbacks.find(
            f => f.feedbackId?.toLowerCase() === feedbackId?.toLowerCase()
        );
        if (!fb) return;

        const product = orderProducts.find(
            p => p.productId?.toLowerCase() === fb.productId?.toLowerCase()
        );
        const imgUrl = getImageUrl(product?.imageUrl);
        const rating = parseInt(fb.rating) || 5;
        const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

        const starsHtml = [1, 2, 3, 4, 5].map(i => `
            <span class="star-btn-edit" data-rating="${i}"
                  style="font-size:28px;cursor:pointer;line-height:1;user-select:none;
                         color:${i <= rating ? STAR_COLOR : '#dee2e6'};
                         transition:color .1s,transform .1s;">★</span>
        `).join('');

        // Render ảnh/video hiện tại với nút xóa
        const existingMediaHtml = (fb.mediaFiles && fb.mediaFiles.length > 0)
            ? `<div style="margin-bottom:12px;">
                <div style="font-size:12px;color:#666;margin-bottom:6px;">
                    Ảnh/video hiện tại
                </div>
                <div id="existingMediaContainer"
                     style="display:flex;flex-wrap:wrap;gap:8px;">
                    ${fb.mediaFiles.map(m => `
                    <div style="position:relative;width:72px;height:72px;"
                         id="media_${m.mediaId}">
                        ${m.mediaType === 'video'
                    ? `<video src="${m.mediaUrl}"
                                      style="width:72px;height:72px;object-fit:cover;
                                             border-radius:6px;border:1px solid #eee;"
                                      muted></video>`
                    : `<img src="${m.mediaUrl}"
                                    style="width:72px;height:72px;object-fit:cover;
                                           border-radius:6px;border:1px solid #eee;">`}
                        <button onclick="OrderFeedback.markMediaRemoved(
                                            '${m.mediaId}','${m.publicId}','${m.mediaType}')"
                                style="position:absolute;top:-6px;right:-6px;
                                       width:20px;height:20px;border-radius:50%;
                                       background:#e74c3c;border:none;color:#fff;
                                       font-size:12px;cursor:pointer;
                                       display:flex;align-items:center;justify-content:center;
                                       line-height:1;">✕</button>
                    </div>`).join('')}
                </div>
               </div>`
            : '';

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
            <div style="display:flex;align-items:center;gap:4px;margin-bottom:12px;">
                ${starsHtml}
                <span id="editRatingDesc"
                      style="font-size:12px;color:${STAR_COLOR};
                             margin-left:8px;font-weight:500;">
                    ${LABELS[rating]}
                </span>
            </div>
            <input type="hidden" id="editRatingValue" value="${rating}">

            <div style="font-size:12px;color:#666;margin:0 0 4px;">
                Nhận xét (tuỳ chọn)
            </div>
            <textarea id="editCommentInput"
                      style="width:100%;border:1px solid #ddd;border-radius:6px;
                             padding:10px;font-size:13px;resize:vertical;
                             min-height:80px;box-sizing:border-box;margin-bottom:14px;"
                      placeholder="Chia sẻ trải nghiệm của bạn...">${fb.comment || ''}</textarea>

            ${existingMediaHtml}

            <div style="font-size:12px;color:#666;margin-bottom:6px;">
                Thêm ảnh/video mới
                <span style="color:#999;">(tối đa ${MAX_MEDIA} file, ảnh ≤${MAX_IMAGE_MB}MB, video ≤${MAX_VIDEO_MB}MB)</span>
            </div>
            <div id="editMediaUploadArea"
                 style="border:2px dashed #ddd;border-radius:8px;padding:16px;
                        text-align:center;cursor:pointer;margin-bottom:8px;
                        transition:border-color .2s;"
                 onclick="document.getElementById('editMediaInput').click()"
                 ondragover="event.preventDefault();this.style.borderColor='#ee4d2d';"
                 ondragleave="this.style.borderColor='#ddd';"
                 ondrop="OrderFeedback.handleEditDrop(event)">
                <i class="fas fa-cloud-upload-alt"
                   style="font-size:24px;color:#ccc;margin-bottom:6px;display:block;"></i>
                <span style="font-size:12px;color:#999;">
                    Kéo thả hoặc click để chọn ảnh/video
                </span>
            </div>
            <input type="file" id="editMediaInput" multiple accept="image/*,video/*"
                   style="display:none;"
                   onchange="OrderFeedback.handleEditFileSelect(this.files)">
            <div id="editNewMediaPreview"
                 style="display:flex;flex-wrap:wrap;gap:8px;margin-top:8px;"></div>
            <input type="hidden" id="editRemovedPublicIds" value="">
            <input type="hidden" id="editFeedbackId" value="${fb.feedbackId}">
        </div>`;

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

            function goBackToView() {
                const editEl = document.getElementById('feedbackEditModal');
                const editInstance = editEl ? bootstrap.Modal.getInstance(editEl) : null;
                if (editInstance) editInstance.hide();
                editEl?.remove();
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
                document.body.classList.remove('modal-open');
                document.body.style.removeProperty('overflow');
                document.body.style.removeProperty('padding-right');
                openViewFeedbackPopup();
            }

            document.getElementById('btnCancelEdit')?.addEventListener('click', goBackToView);
            document.getElementById('btnCloseX_feedbackEditModal')
                ?.addEventListener('click', goBackToView);
            document.getElementById('btnSubmitEdit')
                ?.addEventListener('click', submitEdit);

            setTimeout(attachEditStarEvents, 100);
        }, 350);
    }

    // Đánh dấu xóa media cũ
    function markMediaRemoved(mediaId, publicId, mediaType) {
        // Ẩn element
        const el = document.getElementById(`media_${mediaId}`);
        if (el) el.style.display = 'none';

        // Lưu publicId vào hidden input
        const hiddenInput = document.getElementById('editRemovedPublicIds');
        if (hiddenInput) {
            const current = hiddenInput.value ? hiddenInput.value.split(',') : [];
            if (!current.includes(publicId)) current.push(publicId);
            hiddenInput.value = current.join(',');
        }
    }

    // Handle file select cho edit
    function handleEditFileSelect(files) {
        addEditNewFiles(Array.from(files));
    }

    function handleEditDrop(event) {
        event.preventDefault();
        document.getElementById('editMediaUploadArea').style.borderColor = '#ddd';
        addEditNewFiles(Array.from(event.dataTransfer.files));
    }

    function addEditNewFiles(files) {
        const preview = document.getElementById('editNewMediaPreview');
        if (!preview) return;

        const existing = preview.querySelectorAll('.edit-new-media-item').length;
        const remaining = MAX_MEDIA - existing;
        if (remaining <= 0) {
            alert(`Tối đa ${MAX_MEDIA} file.`);
            return;
        }

        const validFiles = validateFiles(files).slice(0, remaining);

        validFiles.forEach(file => {
            const itemId = `enm_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`;
            const url = URL.createObjectURL(file);
            const isVideo = file.type.startsWith('video/');

            const div = document.createElement('div');
            div.className = 'edit-new-media-item';
            div.dataset.file = itemId;
            div.style.cssText = 'position:relative;width:72px;height:72px;flex-shrink:0;';

            div.innerHTML = isVideo
                ? `<video src="${url}" muted playsinline
                           style="width:72px;height:72px;object-fit:cover;
                                  border-radius:6px;border:1px solid #eee;"></video>
                   <div style="position:absolute;top:50%;left:50%;
                               transform:translate(-50%,-50%);
                               background:rgba(0,0,0,0.45);border-radius:50%;
                               width:24px;height:24px;display:flex;
                               align-items:center;justify-content:center;">
                       <span style="color:#fff;font-size:10px;">▶</span>
                   </div>`
                : `<img src="${url}"
                        style="width:72px;height:72px;object-fit:cover;
                               border-radius:6px;border:1px solid #eee;">`;

            const closeBtn = document.createElement('button');
            closeBtn.innerHTML = '✕';
            closeBtn.style.cssText = `position:absolute;top:-6px;right:-6px;
                width:20px;height:20px;border-radius:50%;background:#e74c3c;
                border:none;color:#fff;font-size:12px;cursor:pointer;
                display:flex;align-items:center;justify-content:center;`;
            closeBtn.onclick = () => { div.remove(); URL.revokeObjectURL(url); };

            div.appendChild(closeBtn);
            div._file = file; // Lưu file thực vào element
            preview.appendChild(div);
        });
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
        const comment = document.getElementById('editCommentInput')?.value?.trim() || '';
        const removedPublicIdsRaw = document.getElementById('editRemovedPublicIds')?.value || '';
        const removedPublicIds = removedPublicIdsRaw
            ? removedPublicIdsRaw.split(',').filter(x => x) : [];

        if (!feedbackId || rating < 1 || rating > 5) {
            alert('Vui lòng chọn số sao.');
            return;
        }

        const submitBtn = document.getElementById('btnSubmitEdit');
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Đang lưu...'; }

        try {
            // Build FormData để gửi kèm file mới
            const formData = new FormData();
            formData.append('FeedbackId', feedbackId);
            formData.append('Rating', rating);
            formData.append('Comment', comment);

            // Append từng publicId bị xóa
            removedPublicIds.forEach(id => {
                formData.append('RemovedPublicIds', id);
            });

            // Append file mới từ preview
            const newItems = document.querySelectorAll('#editNewMediaPreview .edit-new-media-item');
            newItems.forEach(item => {
                if (item._file) {
                    formData.append('NewMediaFiles', item._file, item._file.name);
                }
            });

            const response = await fetch('/Feedback/Update', {
                method: 'POST',
                body: formData // Không set Content-Type, browser tự set multipart
            });

            if (!response.ok) {
                alert(`Lỗi server: ${response.status}`);
                return;
            }

            const result = await response.json();
            if (result.success) {
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
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Lưu thay đổi';
            }
        }
    }

    // ===================== WRITE FEEDBACK =====================
    function openWriteFeedbackPopup() {
        let formHtml = '';
        const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Xuất sắc'];

        orderProducts.forEach((product, idx) => {
            const imgUrl = getImageUrl(product.imageUrl);

            const starsHtml = [1, 2, 3, 4, 5].map(i => `
                <span class="star-btn" data-rating="${i}"
                      style="font-size:28px;cursor:pointer;line-height:1;user-select:none;
                             color:${STAR_COLOR};transition:color .1s,transform .1s;
                             transform:scale(1.1);">★</span>
            `).join('');

            formHtml += `
            <div class="product-feedback" data-product-id="${product.productId}"
                 data-index="${idx}"
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
                <div style="display:flex;align-items:center;gap:4px;margin-bottom:4px;"
                     class="star-rating">
                    ${starsHtml}
                    <span class="rating-desc"
                          style="font-size:12px;color:${STAR_COLOR};
                                 margin-left:8px;font-weight:500;min-width:80px;">
                        ${LABELS[5]}
                    </span>
                </div>
                <input type="hidden" class="rating-value" value="5">
                <div class="rating-error"
                     style="display:none;font-size:11px;color:#c0392b;margin-bottom:6px;">
                    Vui lòng chọn số sao
                </div>
                <div style="font-size:12px;color:#666;margin:12px 0 4px;">
                    Nhận xét (tuỳ chọn)
                </div>
                <textarea class="comment-input"
                          style="width:100%;border:1px solid #ddd;border-radius:6px;
                                 padding:10px;font-size:13px;resize:vertical;
                                 min-height:80px;box-sizing:border-box;margin-bottom:12px;"
                          placeholder="Chia sẻ trải nghiệm của bạn..."></textarea>

                <!-- Khu vực upload media -->
                <div style="font-size:12px;color:#666;margin-bottom:6px;">
                    Ảnh/video (tuỳ chọn)
                    <span style="color:#999;">
                        (tối đa ${MAX_MEDIA} file, ảnh ≤${MAX_IMAGE_MB}MB, video ≤${MAX_VIDEO_MB}MB)
                    </span>
                </div>
                <div class="media-upload-area"
                     style="border:2px dashed #ddd;border-radius:8px;padding:14px;
                            text-align:center;cursor:pointer;margin-bottom:8px;
                            transition:border-color .2s;"
                     onclick="document.querySelector(
                                 '.product-feedback[data-index=\\'${idx}\\'] .media-input'
                              ).click()"
                     ondragover="event.preventDefault();this.style.borderColor='#ee4d2d';"
                     ondragleave="this.style.borderColor='#ddd';"
                     ondrop="OrderFeedback.handleWriteDrop(event, ${idx})">
                    <i class="fas fa-camera"
                       style="font-size:20px;color:#ccc;display:block;margin-bottom:4px;"></i>
                    <span style="font-size:11px;color:#999;">
                        Kéo thả hoặc click để chọn
                    </span>
                </div>
                <input type="file" class="media-input" multiple accept="image/*,video/*"
                       style="display:none;"
                       onchange="OrderFeedback.handleWriteFileSelect(this, ${idx})">
                <div class="media-preview"
                     style="display:flex;flex-wrap:wrap;gap:8px;margin-top:4px;"></div>
            </div>`;
        });

        showModal('feedbackWriteModal', 'Đánh giá sản phẩm', formHtml,
            `<button class="btn btn-secondary" id="btnCancelWrite">Trở lại</button>
             <button class="btn btn-danger px-4" id="btnSubmitWrite">Hoàn thành</button>`
        );

        document.getElementById('btnCancelWrite')
            ?.addEventListener('click', () => closeModalById('feedbackWriteModal'));
        document.getElementById('btnCloseX_feedbackWriteModal')
            ?.addEventListener('click', () => closeModalById('feedbackWriteModal'));
        document.getElementById('btnSubmitWrite')
            ?.addEventListener('click', submitFeedback);

        setTimeout(attachStarEvents, 100);
    }

    function handleWriteFileSelect(input, idx) {
        const files = Array.from(input.files);
        addWriteFiles(files, idx);
        input.value = ''; // Reset input để chọn lại được
    }

    function handleWriteDrop(event, idx) {
        event.preventDefault();
        const area = event.currentTarget;
        area.style.borderColor = '#ddd';
        addWriteFiles(Array.from(event.dataTransfer.files), idx);
    }

    function addWriteFiles(files, idx) {
        const container = document.querySelector(
            `.product-feedback[data-index="${idx}"] .media-preview`
        );
        if (!container) return;

        const existing = container.querySelectorAll('.write-media-item').length;
        const remaining = MAX_MEDIA - existing;
        if (remaining <= 0) { alert(`Tối đa ${MAX_MEDIA} file.`); return; }

        const validFiles = validateFiles(files).slice(0, remaining);

        validFiles.forEach(file => {
            const url = URL.createObjectURL(file);
            const isVideo = file.type.startsWith('video/');

            const div = document.createElement('div');
            div.className = 'write-media-item';
            div.style.cssText = 'position:relative;width:72px;height:72px;flex-shrink:0;';

            div.innerHTML = isVideo
                ? `<video src="${url}" muted playsinline
                           style="width:72px;height:72px;object-fit:cover;
                                  border-radius:6px;border:1px solid #eee;"></video>
                   <div style="position:absolute;top:50%;left:50%;
                               transform:translate(-50%,-50%);
                               background:rgba(0,0,0,0.45);border-radius:50%;
                               width:24px;height:24px;display:flex;
                               align-items:center;justify-content:center;">
                       <span style="color:#fff;font-size:10px;">▶</span>
                   </div>`
                : `<img src="${url}"
                        style="width:72px;height:72px;object-fit:cover;
                               border-radius:6px;border:1px solid #eee;">`;

            const closeBtn = document.createElement('button');
            closeBtn.innerHTML = '✕';
            closeBtn.style.cssText = `position:absolute;top:-6px;right:-6px;
                width:20px;height:20px;border-radius:50%;background:#e74c3c;
                border:none;color:#fff;font-size:12px;cursor:pointer;
                display:flex;align-items:center;justify-content:center;`;
            closeBtn.onclick = () => { div.remove(); URL.revokeObjectURL(url); };

            div.appendChild(closeBtn);
            div._file = file;
            container.appendChild(div);
        });
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
                    if (errMsg) errMsg.style.display = 'none';
                    paint(v);
                    desc.textContent = LABELS[v];
                });
            });
        });
    }

    // ===================== SUBMIT WRITE =====================
    async function submitFeedback() {
        // Validate rating
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

        const submitBtn = document.getElementById('btnSubmitWrite');
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Đang gửi...'; }

        try {
            // Build FormData — gửi multipart thay vì JSON
            const formData = new FormData();

            document.querySelectorAll('.product-feedback').forEach((item, i) => {
                const productId = item.getAttribute('data-product-id');
                const rating = item.querySelector('.rating-value').value;
                const comment = item.querySelector('.comment-input').value.trim();

                // Text fields theo format Feedbacks[i].Field
                formData.append(`Feedbacks[${i}].ProductId`, productId);
                formData.append(`Feedbacks[${i}].OrderId`, currentOrderId);
                formData.append(`Feedbacks[${i}].Rating`, rating);
                formData.append(`Feedbacks[${i}].Comment`, comment);

                // Files theo format Feedbacks[i].MediaFiles
                const mediaItems = item.querySelectorAll('.write-media-item');
                mediaItems.forEach(mediaItem => {
                    if (mediaItem._file) {
                        formData.append(
                            `Feedbacks[${i}].MediaFiles`,
                            mediaItem._file,
                            mediaItem._file.name
                        );
                    }
                });
            });

            const response = await fetch('/Feedback/CreateBulk', {
                method: 'POST',
                body: formData // Không set Content-Type, browser tự set multipart
            });

            if (!response.ok) {
                alert(`Lỗi server: ${response.status}`);
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
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Hoàn thành';
            }
        }
    }

    // ===================== HELPERS =====================
    function validateFiles(files) {
        const valid = [];
        files.forEach(file => {
            if (!ACCEPTED_TYPES.includes(file.type)) {
                alert(`File "${file.name}" không hợp lệ. Chỉ chấp nhận ảnh và video.`);
                return;
            }
            const isVideo = file.type.startsWith('video/');
            const maxBytes = isVideo
                ? MAX_VIDEO_MB * 1024 * 1024
                : MAX_IMAGE_MB * 1024 * 1024;
            if (file.size > maxBytes) {
                alert(`File "${file.name}" vượt quá dung lượng cho phép.`);
                return;
            }
            valid.push(file);
        });
        return valid;
    }

    function showModal(modalId, title, bodyHtml, customFooter) {
        document.getElementById(modalId)?.remove();
        const xBtnId = `btnCloseX_${modalId}`;

        document.body.insertAdjacentHTML('beforeend', `
            <div class="modal fade" id="${modalId}" tabindex="-1"
                 data-bs-backdrop="static" data-bs-keyboard="false">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" id="${xBtnId}"></button>
                        </div>
                        <div class="modal-body"
                             style="max-height:500px;overflow-y:auto;padding:16px 20px;">
                            ${bodyHtml}
                        </div>
                        <div class="modal-footer">${customFooter}</div>
                    </div>
                </div>
            </div>`);

        new bootstrap.Modal(document.getElementById(modalId)).show();
    }

    function closeModalById(modalId, callback) {
        const el = document.getElementById(modalId);
        if (!el) { if (callback) callback(); return; }
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
            return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg'
                width='60' height='60'%3E%3Crect width='60' height='60'
                fill='%23dee2e6'/%3E%3Ctext x='50%25' y='50%25'
                dominant-baseline='middle' text-anchor='middle'
                fill='%236c757d' font-size='10'%3ENo img%3C/text%3E%3C/svg%3E`;
        }
        return url;
    }

    return {
        init,
        openViewFeedbackPopup,
        openWriteFeedbackPopup,
        openEditPopup,
        markMediaRemoved,
        handleEditFileSelect,
        handleEditDrop,
        handleWriteFileSelect,
        handleWriteDrop,
        openMediaViewer
    };
})();