const STAR_COLOR = '#ffc107';
const MAX_MEDIA = 5;
const MAX_IMAGE_MB = 5;
const MAX_VIDEO_MB = 50;
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'video/mp4', 'video/webm'];
const STAR_LABELS = ['', 'Terrible', 'Poor', 'Average', 'Good', 'Excellent'];

const OrderFeedback = (function () {
    let currentOrderId = null;
    let orderProducts = [];
    let isInitialized = false;
    let cachedFeedbacks = [];

    // ============================================================
    // INIT
    // ============================================================
    function init(orderId, products) {
        currentOrderId = orderId;
        orderProducts = products || [];
        isInitialized = true;
        checkAndRenderButton();
    }

    // ============================================================
    // CHECK STATUS — decide which button to show
    // ============================================================
    async function checkAndRenderButton() {
        try {
            const response = await fetch(`/Feedback/CheckOrderFeedback?orderId=${currentOrderId}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const result = await response.json();
            const container = document.getElementById('feedback-button-container');
            if (!container) return;

            if (result.success && result.hasFeedback) {
                container.innerHTML = `
                    <button type="button"
                            class="btn btn-sm btn-outline-info fw-semibold"
                            onclick="OrderFeedback.openViewFeedbackPopup()"
                            style="border-radius:8px;padding:6px 16px;">
                        <i class="fas fa-eye me-1"></i>View Reviews
                    </button>`;
            } else {
                container.innerHTML = `
                    <button type="button"
                            class="btn btn-sm fw-semibold text-white"
                            onclick="OrderFeedback.openWriteFeedbackPopup()"
                            style="border-radius:8px;padding:6px 16px;
                                   background:linear-gradient(135deg,#f59e0b,#d97706);">
                        <i class="fas fa-star me-1"></i>Write a Review
                    </button>`;
            }
        } catch (error) {
            const container = document.getElementById('feedback-button-container');
            if (container) {
                container.innerHTML = `
                    <span class="text-muted" style="font-size:12px;">
                        Unable to load review status
                    </span>`;
            }
        }
    }

    // ============================================================
    // VIEW FEEDBACK POPUP
    // ============================================================
    async function openViewFeedbackPopup() {
        try {
            const response = await fetch(`/Feedback/GetOrderFeedbacks?orderId=${currentOrderId}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const result = await response.json();

            if (!result.success || !result.data || result.data.length === 0) {
                showAlert('No reviews found for this order.', 'warning');
                return;
            }

            cachedFeedbacks = result.data;
            const customerName = document.querySelector('meta[name="customer-name"]')?.content || 'You';

            let bodyHtml = '';
            result.data.forEach((fb, index) => {
                const product = orderProducts.find(
                    p => p.productId?.toLowerCase() === fb.productId?.toLowerCase()
                );
                const imgUrl = getImageUrl(product?.imageUrl);
                const rating = parseInt(fb.rating) || 0;
                const isLast = index === result.data.length - 1;
                const name = fb.customerName || customerName;
                const initials = name.trim().split(' ')
                    .filter(w => w).map(w => w[0].toUpperCase()).slice(0, 2).join('') || '?';

                const starsHtml = [1, 2, 3, 4, 5].map(i =>
                    `<span style="font-size:18px;color:${i <= rating ? STAR_COLOR : '#e2e8f0'};">★</span>`
                ).join('');

                const mediaHtml = renderMediaGrid(fb.mediaFiles || []);

                bodyHtml += `
                <div style="padding:18px 0;${isLast ? '' : 'border-bottom:1.5px solid #f1f5f9;'}">
                    <!-- Product row -->
                    <div style="display:flex;align-items:center;gap:10px;
                                background:#f8fafc;border-radius:10px;
                                padding:10px 12px;margin-bottom:14px;">
                        <img src="${imgUrl}"
                             style="width:44px;height:44px;object-fit:cover;
                                    border-radius:8px;flex-shrink:0;border:1px solid #e2e8f0;"
                             onerror="this.onerror=null;this.style.display='none'">
                        <div style="flex:1;min-width:0;">
                            <div style="font-size:13px;font-weight:600;color:#1e293b;
                                        white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">
                                ${product?.productName || 'Product'}
                            </div>
                        </div>
                        <div style="display:flex;gap:1px;flex-shrink:0;">${starsHtml}</div>
                    </div>

                    <!-- Reviewer row -->
                    <div style="display:flex;align-items:center;gap:10px;margin-bottom:10px;">
                        <div style="width:34px;height:34px;border-radius:50%;flex-shrink:0;
                                    background:linear-gradient(135deg,#6366f1,#8b5cf6);
                                    color:#fff;display:flex;align-items:center;
                                    justify-content:center;font-size:13px;font-weight:700;">
                            ${initials}
                        </div>
                        <div>
                            <div style="font-size:13px;font-weight:600;color:#1e293b;">${name}</div>
                            <div style="font-size:11px;color:#94a3b8;">${formatDate(fb.createdDate)}</div>
                        </div>
                        <div style="margin-left:auto;display:flex;gap:6px;align-items:center;">
                            <span style="font-size:11px;font-weight:600;color:${STAR_COLOR};">
                                ${STAR_LABELS[rating] || ''}
                            </span>
                            <button onclick="OrderFeedback.openEditPopup('${fb.feedbackId}')"
                                    style="font-size:11px;padding:3px 12px;
                                           border:1.5px solid #6366f1;border-radius:6px;
                                           background:#fff;color:#6366f1;cursor:pointer;
                                           font-weight:600;transition:all .15s;"
                                    onmouseover="this.style.background='#ede9fe'"
                                    onmouseout="this.style.background='#fff'">
                                ✏ Edit
                            </button>
                        </div>
                    </div>

                    <!-- Comment -->
                    <div style="font-size:13px;color:#374151;line-height:1.7;
                                padding:10px 12px;background:#f8fafc;
                                border-radius:8px;margin-bottom:${mediaHtml ? '10px' : '0'};">
                        ${fb.comment
                        ? fb.comment
                        : '<span style="color:#94a3b8;font-style:italic;">No comment provided.</span>'}
                    </div>

                    <!-- Media -->
                    ${mediaHtml}

                    <!-- Shop reply -->
                    ${fb.reply ? `
                    <div style="margin-top:12px;padding:12px 14px;
                                background:linear-gradient(135deg,#f0fdf4,#dcfce7);
                                border-left:3px solid #22c55e;border-radius:0 8px 8px 0;">
                        <div style="font-size:11px;font-weight:700;color:#16a34a;
                                    margin-bottom:6px;display:flex;align-items:center;gap:6px;">
                            🏪 Shop Reply
                            ${fb.replyDate ? `<span style="font-weight:400;color:#86efac;">· ${formatDate(fb.replyDate)}</span>` : ''}
                        </div>
                        <div style="font-size:13px;color:#15803d;line-height:1.6;">${fb.reply}</div>
                    </div>` : ''}
                </div>`;
            });

            showFeedbackModal('feedbackViewModal', `My Reviews (${result.data.length})`, bodyHtml,
                `<button class="btn btn-secondary btn-sm px-4" id="btnCloseViewModal">Close</button>`
            );

            document.getElementById('btnCloseViewModal')
                ?.addEventListener('click', () => closeFeedbackModal('feedbackViewModal'));
            document.getElementById('btnCloseX_feedbackViewModal')
                ?.addEventListener('click', () => closeFeedbackModal('feedbackViewModal'));

        } catch (error) {
            console.error('[OrderFeedback] openViewFeedbackPopup error:', error);
            showAlert('Unable to load reviews. Please try again.', 'danger');
        }
    }

    // ============================================================
    // WRITE FEEDBACK POPUP
    // ============================================================
    function openWriteFeedbackPopup() {
        let bodyHtml = `
        <div style="margin-bottom:12px;padding:10px 14px;background:#fffbeb;
                    border:1px solid #fcd34d;border-radius:8px;font-size:12px;color:#92400e;">
            <i class="fas fa-info-circle me-1"></i>
            Please rate <strong>all ${orderProducts.length} product${orderProducts.length > 1 ? 's' : ''}</strong>
            in this order. Your feedback helps other customers!
        </div>`;

        orderProducts.forEach((product, idx) => {
            const imgUrl = getImageUrl(product.imageUrl);
            const starsHtml = [1, 2, 3, 4, 5].map(i => `
                <span class="star-btn" data-rating="${i}" data-idx="${idx}"
                      style="font-size:26px;cursor:pointer;line-height:1;
                             user-select:none;color:${STAR_COLOR};
                             transition:color .1s,transform .1s;transform:scale(1.1);">★</span>
            `).join('');

            bodyHtml += `
            <div class="fb-product-item" data-product-id="${product.productId}" data-index="${idx}"
                 style="border:1.5px solid #e2e8f0;border-radius:12px;padding:16px;
                        margin-bottom:14px;background:#fff;">

                <!-- Product header -->
                <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px;
                            padding-bottom:12px;border-bottom:1px solid #f1f5f9;">
                    <img src="${imgUrl}"
                         style="width:48px;height:48px;object-fit:cover;border-radius:8px;
                                flex-shrink:0;border:1px solid #e2e8f0;"
                         onerror="this.onerror=null;this.style.display='none'">
                    <div>
                        <div style="font-size:13px;font-weight:700;color:#1e293b;">
                            ${product.productName}
                        </div>
                        <div style="font-size:11px;color:#94a3b8;margin-top:2px;">
                            Product ${idx + 1} of ${orderProducts.length}
                        </div>
                    </div>
                </div>

                <!-- Stars -->
                <div style="margin-bottom:12px;">
                    <div style="font-size:11px;font-weight:700;color:#64748b;
                                text-transform:uppercase;letter-spacing:.05em;margin-bottom:8px;">
                        Rating <span style="color:#ef4444;">*</span>
                    </div>
                    <div style="display:flex;align-items:center;gap:4px;" class="star-row">
                        ${starsHtml}
                        <span class="rating-label"
                              style="font-size:12px;color:${STAR_COLOR};
                                     margin-left:8px;font-weight:700;min-width:70px;">
                            ${STAR_LABELS[5]}
                        </span>
                    </div>
                    <input type="hidden" class="rating-value" value="5">
                    <div class="rating-error"
                         style="display:none;font-size:11px;color:#ef4444;margin-top:4px;">
                        Please select a rating.
                    </div>
                </div>

                <!-- Comment -->
                <div style="margin-bottom:12px;">
                    <div style="font-size:11px;font-weight:700;color:#64748b;
                                text-transform:uppercase;letter-spacing:.05em;margin-bottom:6px;">
                        Comment <span style="color:#94a3b8;font-weight:400;">(optional)</span>
                    </div>
                    <textarea class="comment-input"
                              rows="3"
                              style="width:100%;border:1.5px solid #e2e8f0;border-radius:8px;
                                     padding:10px 12px;font-size:13px;resize:none;
                                     outline:none;transition:border .2s;box-sizing:border-box;
                                     font-family:inherit;color:#374151;"
                              placeholder="Share your experience with this product..."
                              onfocus="this.style.borderColor='#6366f1'"
                              onblur="this.style.borderColor='#e2e8f0'"></textarea>
                </div>

                <!-- Media upload -->
                <div>
                    <div style="font-size:11px;font-weight:700;color:#64748b;
                                text-transform:uppercase;letter-spacing:.05em;margin-bottom:6px;">
                        Photos / Videos
                        <span style="font-weight:400;color:#94a3b8;">
                            (optional · max ${MAX_MEDIA} files · img ≤${MAX_IMAGE_MB}MB · video ≤${MAX_VIDEO_MB}MB)
                        </span>
                    </div>
                    <div class="media-drop-zone"
                         style="border:2px dashed #cbd5e1;border-radius:8px;
                                padding:14px;text-align:center;cursor:pointer;
                                transition:all .2s;background:#f8fafc;"
                         onclick="document.querySelector('.fb-product-item[data-index=\\'${idx}\\'] .media-file-input').click()"
                         ondragover="event.preventDefault();this.style.borderColor='#6366f1';this.style.background='#eef2ff';"
                         ondragleave="this.style.borderColor='#cbd5e1';this.style.background='#f8fafc';"
                         ondrop="OrderFeedback.handleWriteDrop(event,${idx})">
                        <div style="font-size:22px;margin-bottom:4px;">📷</div>
                        <div style="font-size:12px;color:#64748b;">
                            Drag & drop or <span style="color:#6366f1;font-weight:600;">browse files</span>
                        </div>
                    </div>
                    <input type="file" class="media-file-input" multiple accept="image/*,video/*"
                           style="display:none;"
                           onchange="OrderFeedback.handleWriteFileSelect(this,${idx})">
                    <div class="media-preview-row"
                         style="display:flex;flex-wrap:wrap;gap:8px;margin-top:8px;"></div>
                </div>
            </div>`;
        });

        showFeedbackModal('feedbackWriteModal', '✍ Write a Review', bodyHtml,
            `<button class="btn btn-light btn-sm px-4" id="btnCancelWrite">Cancel</button>
             <button class="btn btn-warning btn-sm px-4 fw-bold text-white" id="btnSubmitWrite">
                 <i class="fas fa-paper-plane me-1"></i>Submit Review
             </button>`
        );

        document.getElementById('btnCancelWrite')
            ?.addEventListener('click', () => closeFeedbackModal('feedbackWriteModal'));
        document.getElementById('btnCloseX_feedbackWriteModal')
            ?.addEventListener('click', () => closeFeedbackModal('feedbackWriteModal'));
        document.getElementById('btnSubmitWrite')
            ?.addEventListener('click', submitFeedback);

        setTimeout(attachWriteStarEvents, 50);
    }

    // ============================================================
    // EDIT POPUP
    // ============================================================
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

        const starsHtml = [1, 2, 3, 4, 5].map(i => `
            <span class="star-btn-edit" data-rating="${i}"
                  style="font-size:28px;cursor:pointer;line-height:1;user-select:none;
                         color:${i <= rating ? STAR_COLOR : '#e2e8f0'};
                         transition:color .1s,transform .1s;">★</span>
        `).join('');

        const existingMediaHtml = (fb.mediaFiles && fb.mediaFiles.length > 0)
            ? `<div style="margin-bottom:14px;">
                <div style="font-size:11px;font-weight:700;color:#64748b;
                            text-transform:uppercase;letter-spacing:.05em;margin-bottom:8px;">
                    Current Photos / Videos
                </div>
                <div style="display:flex;flex-wrap:wrap;gap:8px;" id="existingMediaContainer">
                    ${fb.mediaFiles.map(m => `
                    <div style="position:relative;width:72px;height:72px;" id="media_${m.mediaId}">
                        ${m.mediaType === 'video'
                    ? `<video src="${m.mediaUrl}"
                                      style="width:72px;height:72px;object-fit:cover;
                                             border-radius:8px;border:1.5px solid #e2e8f0;"
                                      muted></video>`
                    : `<img src="${m.mediaUrl}"
                                    style="width:72px;height:72px;object-fit:cover;
                                           border-radius:8px;border:1.5px solid #e2e8f0;">`}
                        <button onclick="OrderFeedback.markMediaRemoved('${m.mediaId}','${m.publicId}','${m.mediaType}')"
                                title="Remove"
                                style="position:absolute;top:-6px;right:-6px;
                                       width:20px;height:20px;border-radius:50%;
                                       background:#ef4444;border:none;color:#fff;
                                       font-size:11px;cursor:pointer;line-height:1;
                                       display:flex;align-items:center;justify-content:center;">✕</button>
                    </div>`).join('')}
                </div>
               </div>`
            : '';

        const bodyHtml = `
        <!-- Product info -->
        <div style="display:flex;align-items:center;gap:12px;padding:12px 14px;
                    background:#f8fafc;border-radius:10px;margin-bottom:18px;
                    border:1px solid #e2e8f0;">
            <img src="${imgUrl}"
                 style="width:48px;height:48px;object-fit:cover;border-radius:8px;flex-shrink:0;"
                 onerror="this.onerror=null;this.style.display='none'">
            <div style="font-size:13px;font-weight:600;color:#1e293b;">
                ${product?.productName || 'Product'}
            </div>
        </div>

        <!-- Stars -->
        <div style="margin-bottom:16px;">
            <div style="font-size:11px;font-weight:700;color:#64748b;
                        text-transform:uppercase;letter-spacing:.05em;margin-bottom:8px;">
                Rating <span style="color:#ef4444;">*</span>
            </div>
            <div style="display:flex;align-items:center;gap:4px;">
                ${starsHtml}
                <span id="editRatingLabel"
                      style="font-size:12px;color:${STAR_COLOR};margin-left:8px;font-weight:700;">
                    ${STAR_LABELS[rating]}
                </span>
            </div>
            <input type="hidden" id="editRatingValue" value="${rating}">
        </div>

        <!-- Comment -->
        <div style="margin-bottom:16px;">
            <div style="font-size:11px;font-weight:700;color:#64748b;
                        text-transform:uppercase;letter-spacing:.05em;margin-bottom:6px;">
                Comment <span style="color:#94a3b8;font-weight:400;">(optional)</span>
            </div>
            <textarea id="editCommentInput" rows="4"
                      style="width:100%;border:1.5px solid #e2e8f0;border-radius:8px;
                             padding:10px 12px;font-size:13px;resize:none;
                             outline:none;transition:border .2s;box-sizing:border-box;
                             font-family:inherit;color:#374151;"
                      placeholder="Share your experience..."
                      onfocus="this.style.borderColor='#6366f1'"
                      onblur="this.style.borderColor='#e2e8f0'">${fb.comment || ''}</textarea>
        </div>

        <!-- Existing media -->
        ${existingMediaHtml}

        <!-- Upload new -->
        <div>
            <div style="font-size:11px;font-weight:700;color:#64748b;
                        text-transform:uppercase;letter-spacing:.05em;margin-bottom:6px;">
                Add New Photos / Videos
                <span style="font-weight:400;color:#94a3b8;">
                    (max ${MAX_MEDIA} total · img ≤${MAX_IMAGE_MB}MB · video ≤${MAX_VIDEO_MB}MB)
                </span>
            </div>
            <div id="editMediaUploadArea"
                 style="border:2px dashed #cbd5e1;border-radius:8px;padding:14px;
                        text-align:center;cursor:pointer;background:#f8fafc;
                        transition:all .2s;"
                 onclick="document.getElementById('editMediaInput').click()"
                 ondragover="event.preventDefault();this.style.borderColor='#6366f1';this.style.background='#eef2ff';"
                 ondragleave="this.style.borderColor='#cbd5e1';this.style.background='#f8fafc';"
                 ondrop="OrderFeedback.handleEditDrop(event)">
                <div style="font-size:20px;margin-bottom:4px;">📎</div>
                <div style="font-size:12px;color:#64748b;">
                    Drag & drop or <span style="color:#6366f1;font-weight:600;">browse files</span>
                </div>
            </div>
            <input type="file" id="editMediaInput" multiple accept="image/*,video/*"
                   style="display:none;"
                   onchange="OrderFeedback.handleEditFileSelect(this.files)">
            <div id="editNewMediaPreview"
                 style="display:flex;flex-wrap:wrap;gap:8px;margin-top:8px;"></div>
        </div>

        <input type="hidden" id="editRemovedPublicIds" value="">
        <input type="hidden" id="editFeedbackId" value="${fb.feedbackId}">`;

        // Close view modal first, then open edit
        const viewEl = document.getElementById('feedbackViewModal');
        const viewInstance = viewEl ? bootstrap.Modal.getInstance(viewEl) : null;
        if (viewInstance) viewInstance.hide();

        setTimeout(() => {
            cleanupModalBackdrops();

            showFeedbackModal('feedbackEditModal', '✏ Edit Review', bodyHtml,
                `<button class="btn btn-light btn-sm px-4" id="btnCancelEdit">Back</button>
                 <button class="btn btn-primary btn-sm px-4 fw-bold" id="btnSubmitEdit">
                     Save Changes
                 </button>`
            );

            function goBackToView() {
                closeFeedbackModal('feedbackEditModal', () => {
                    openViewFeedbackPopup();
                });
            }

            document.getElementById('btnCancelEdit')?.addEventListener('click', goBackToView);
            document.getElementById('btnCloseX_feedbackEditModal')?.addEventListener('click', goBackToView);
            document.getElementById('btnSubmitEdit')?.addEventListener('click', submitEdit);

            setTimeout(attachEditStarEvents, 100);
        }, 350);
    }

    // ============================================================
    // SUBMIT WRITE
    // ============================================================
    async function submitFeedback() {
        let isValid = true;
        document.querySelectorAll('.fb-product-item').forEach(item => {
            const rating = parseInt(item.querySelector('.rating-value').value);
            const errMsg = item.querySelector('.rating-error');
            if (rating < 1 || rating > 5) {
                isValid = false;
                if (errMsg) errMsg.style.display = 'block';
            }
        });
        if (!isValid) return;

        const submitBtn = document.getElementById('btnSubmitWrite');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Submitting...';
        }

        try {
            const formData = new FormData();
            document.querySelectorAll('.fb-product-item').forEach((item, i) => {
                formData.append(`Feedbacks[${i}].ProductId`, item.getAttribute('data-product-id'));
                formData.append(`Feedbacks[${i}].OrderId`, currentOrderId);
                formData.append(`Feedbacks[${i}].Rating`, item.querySelector('.rating-value').value);
                formData.append(`Feedbacks[${i}].Comment`, item.querySelector('.comment-input').value.trim());
                item.querySelectorAll('.write-media-item').forEach(m => {
                    if (m._file) formData.append(`Feedbacks[${i}].MediaFiles`, m._file, m._file.name);
                });
            });

            const response = await fetch('/Feedback/CreateBulk', { method: 'POST', body: formData });
            if (!response.ok) {
                showAlert(`Server error: ${response.status}`, 'danger');
                return;
            }
            const result = await response.json();
            if (result.success) {
                closeFeedbackModal('feedbackWriteModal', () => {
                    showAlert('Your review has been submitted successfully! 🎉', 'success');
                    checkAndRenderButton();
                });
            } else {
                showAlert(result.message || 'An error occurred. Please try again.', 'danger');
            }
        } catch (error) {
            console.error('[OrderFeedback] submitFeedback error:', error);
            showAlert('Network error. Please try again.', 'danger');
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-paper-plane me-1"></i>Submit Review';
            }
        }
    }

    // ============================================================
    // SUBMIT EDIT
    // ============================================================
    async function submitEdit() {
        const feedbackId = document.getElementById('editFeedbackId')?.value;
        const rating = parseInt(document.getElementById('editRatingValue')?.value);
        const comment = document.getElementById('editCommentInput')?.value?.trim() || '';
        const removedRaw = document.getElementById('editRemovedPublicIds')?.value || '';
        const removedPublicIds = removedRaw ? removedRaw.split(',').filter(x => x) : [];

        if (!feedbackId || rating < 1 || rating > 5) {
            showAlert('Please select a star rating.', 'warning');
            return;
        }

        const submitBtn = document.getElementById('btnSubmitEdit');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';
        }

        try {
            const formData = new FormData();
            formData.append('FeedbackId', feedbackId);
            formData.append('Rating', rating);
            formData.append('Comment', comment);
            removedPublicIds.forEach(id => formData.append('RemovedPublicIds', id));
            document.querySelectorAll('#editNewMediaPreview .edit-new-media-item').forEach(item => {
                if (item._file) formData.append('NewMediaFiles', item._file, item._file.name);
            });

            const response = await fetch('/Feedback/Update', { method: 'POST', body: formData });
            if (!response.ok) {
                showAlert(`Server error: ${response.status}`, 'danger');
                return;
            }
            const result = await response.json();
            if (result.success) {
                closeFeedbackModal('feedbackEditModal', () => {
                    showAlert('Review updated successfully!', 'success');
                    checkAndRenderButton();
                });
            } else {
                showAlert(result.message || 'An error occurred.', 'danger');
            }
        } catch (error) {
            console.error('[OrderFeedback] submitEdit error:', error);
            showAlert('Network error. Please try again.', 'danger');
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Save Changes';
            }
        }
    }

    // ============================================================
    // STAR EVENTS
    // ============================================================
    function attachWriteStarEvents() {
        document.querySelectorAll('.fb-product-item').forEach(item => {
            const stars = item.querySelectorAll('.star-btn');
            const input = item.querySelector('.rating-value');
            const label = item.querySelector('.rating-label');
            const errMsg = item.querySelector('.rating-error');

            function paint(upTo) {
                stars.forEach(s => {
                    const v = parseInt(s.dataset.rating);
                    s.style.color = v <= upTo ? STAR_COLOR : '#e2e8f0';
                    s.style.transform = v <= upTo ? 'scale(1.15)' : 'scale(1)';
                });
            }

            stars.forEach(star => {
                star.addEventListener('mouseenter', () => {
                    paint(parseInt(star.dataset.rating));
                    label.textContent = STAR_LABELS[parseInt(star.dataset.rating)];
                });
                star.addEventListener('mouseleave', () => {
                    paint(parseInt(input.value));
                    label.textContent = STAR_LABELS[parseInt(input.value)] || '';
                });
                star.addEventListener('click', () => {
                    const v = parseInt(star.dataset.rating);
                    input.value = v;
                    if (errMsg) errMsg.style.display = 'none';
                    paint(v);
                    label.textContent = STAR_LABELS[v];
                });
            });
        });
    }

    function attachEditStarEvents() {
        const stars = document.querySelectorAll('.star-btn-edit');
        const input = document.getElementById('editRatingValue');
        const label = document.getElementById('editRatingLabel');

        function paint(upTo) {
            stars.forEach(s => {
                const v = parseInt(s.dataset.rating);
                s.style.color = v <= upTo ? STAR_COLOR : '#e2e8f0';
                s.style.transform = v <= upTo ? 'scale(1.1)' : 'scale(1)';
            });
        }

        stars.forEach(star => {
            star.addEventListener('mouseenter', () => {
                paint(parseInt(star.dataset.rating));
                label.textContent = STAR_LABELS[parseInt(star.dataset.rating)];
            });
            star.addEventListener('mouseleave', () => {
                paint(parseInt(input.value));
                label.textContent = STAR_LABELS[parseInt(input.value)] || '';
            });
            star.addEventListener('click', () => {
                const v = parseInt(star.dataset.rating);
                input.value = v;
                paint(v);
                label.textContent = STAR_LABELS[v];
            });
        });
    }

    // ============================================================
    // MEDIA HELPERS
    // ============================================================
    function renderMediaGrid(mediaFiles) {
        if (!mediaFiles || mediaFiles.length === 0) return '';
        const items = mediaFiles.map(m => {
            if (m.mediaType === 'video') {
                return `
                <div style="position:relative;width:76px;height:76px;flex-shrink:0;">
                    <video src="${m.mediaUrl}"
                           style="width:76px;height:76px;object-fit:cover;
                                  border-radius:8px;border:1.5px solid #e2e8f0;cursor:pointer;"
                           onclick="OrderFeedback.openMediaViewer('${m.mediaUrl}','video')"
                           muted playsinline></video>
                    <div style="position:absolute;inset:0;display:flex;align-items:center;
                                justify-content:center;pointer-events:none;">
                        <div style="background:rgba(0,0,0,.5);border-radius:50%;
                                    width:26px;height:26px;display:flex;align-items:center;
                                    justify-content:center;">
                            <span style="color:#fff;font-size:11px;">▶</span>
                        </div>
                    </div>
                </div>`;
            }
            return `
            <img src="${m.mediaUrl}"
                 onclick="OrderFeedback.openMediaViewer('${m.mediaUrl}','image')"
                 style="width:76px;height:76px;object-fit:cover;border-radius:8px;
                        border:1.5px solid #e2e8f0;cursor:pointer;flex-shrink:0;"
                 onerror="this.onerror=null;this.style.display='none'">`;
        }).join('');
        return `<div style="display:flex;flex-wrap:wrap;gap:8px;margin-top:10px;">${items}</div>`;
    }

    function openMediaViewer(url, type) {
        document.getElementById('fbMediaViewerOverlay')?.remove();
        const mediaEl = type === 'video'
            ? `<video src="${url}" controls autoplay
                      style="max-width:90vw;max-height:85vh;border-radius:10px;"></video>`
            : `<img src="${url}"
                    style="max-width:90vw;max-height:85vh;border-radius:10px;object-fit:contain;">`;

        document.body.insertAdjacentHTML('beforeend', `
            <div id="fbMediaViewerOverlay"
                 style="position:fixed;inset:0;background:rgba(0,0,0,.88);
                        z-index:9999;display:flex;align-items:center;
                        justify-content:center;cursor:pointer;"
                 onclick="document.getElementById('fbMediaViewerOverlay').remove()">
                ${mediaEl}
                <button onclick="event.stopPropagation();document.getElementById('fbMediaViewerOverlay').remove()"
                        style="position:absolute;top:18px;right:22px;background:rgba(255,255,255,.15);
                               border:none;color:#fff;font-size:22px;cursor:pointer;
                               border-radius:50%;width:38px;height:38px;
                               display:flex;align-items:center;justify-content:center;">✕</button>
            </div>`);
    }

    function markMediaRemoved(mediaId, publicId, mediaType) {
        const el = document.getElementById(`media_${mediaId}`);
        if (el) el.style.display = 'none';
        const hiddenInput = document.getElementById('editRemovedPublicIds');
        if (hiddenInput) {
            const current = hiddenInput.value ? hiddenInput.value.split(',') : [];
            if (!current.includes(publicId)) current.push(publicId);
            hiddenInput.value = current.join(',');
        }
    }

    function handleEditFileSelect(files) { addEditNewFiles(Array.from(files)); }
    function handleEditDrop(event) {
        event.preventDefault();
        document.getElementById('editMediaUploadArea').style.borderColor = '#cbd5e1';
        document.getElementById('editMediaUploadArea').style.background = '#f8fafc';
        addEditNewFiles(Array.from(event.dataTransfer.files));
    }

    function addEditNewFiles(files) {
        const preview = document.getElementById('editNewMediaPreview');
        if (!preview) return;
        const existing = preview.querySelectorAll('.edit-new-media-item').length;
        const remaining = MAX_MEDIA - existing;
        if (remaining <= 0) { showAlert(`Maximum ${MAX_MEDIA} files allowed.`, 'warning'); return; }
        validateFiles(files).slice(0, remaining).forEach(file => addMediaThumb(file, preview, 'edit-new-media-item'));
    }

    function handleWriteFileSelect(input, idx) {
        addWriteFiles(Array.from(input.files), idx);
        input.value = '';
    }

    function handleWriteDrop(event, idx) {
        event.preventDefault();
        const zone = event.currentTarget;
        zone.style.borderColor = '#cbd5e1';
        zone.style.background = '#f8fafc';
        addWriteFiles(Array.from(event.dataTransfer.files), idx);
    }

    function addWriteFiles(files, idx) {
        const container = document.querySelector(`.fb-product-item[data-index="${idx}"] .media-preview-row`);
        if (!container) return;
        const existing = container.querySelectorAll('.write-media-item').length;
        const remaining = MAX_MEDIA - existing;
        if (remaining <= 0) { showAlert(`Maximum ${MAX_MEDIA} files allowed.`, 'warning'); return; }
        validateFiles(files).slice(0, remaining).forEach(file => addMediaThumb(file, container, 'write-media-item'));
    }

    function addMediaThumb(file, container, className) {
        const url = URL.createObjectURL(file);
        const isVideo = file.type.startsWith('video/');
        const div = document.createElement('div');
        div.className = className;
        div.style.cssText = 'position:relative;width:68px;height:68px;flex-shrink:0;';
        div.innerHTML = isVideo
            ? `<video src="${url}" muted playsinline
                      style="width:68px;height:68px;object-fit:cover;
                             border-radius:8px;border:1.5px solid #e2e8f0;"></video>
               <div style="position:absolute;inset:0;display:flex;align-items:center;
                           justify-content:center;pointer-events:none;">
                   <div style="background:rgba(0,0,0,.4);border-radius:50%;
                               width:22px;height:22px;display:flex;
                               align-items:center;justify-content:center;">
                       <span style="color:#fff;font-size:9px;">▶</span>
                   </div>
               </div>`
            : `<img src="${url}"
                    style="width:68px;height:68px;object-fit:cover;
                           border-radius:8px;border:1.5px solid #e2e8f0;">`;
        const rm = document.createElement('button');
        rm.innerHTML = '✕';
        rm.title = 'Remove';
        rm.style.cssText = `position:absolute;top:-6px;right:-6px;width:18px;height:18px;
            border-radius:50%;background:#ef4444;border:none;color:#fff;font-size:10px;
            cursor:pointer;display:flex;align-items:center;justify-content:center;`;
        rm.onclick = () => { div.remove(); URL.revokeObjectURL(url); };
        div.appendChild(rm);
        div._file = file;
        container.appendChild(div);
    }

    // ============================================================
    // MODAL HELPERS — fixed z-index above #orderModal
    // ============================================================
    function showFeedbackModal(modalId, title, bodyHtml, footerHtml) {
        document.getElementById(modalId)?.remove();
        const xBtnId = `btnCloseX_${modalId}`;

        document.body.insertAdjacentHTML('beforeend', `
            <div class="modal fade" id="${modalId}" tabindex="-1"
                 data-bs-backdrop="false" data-bs-keyboard="false"
                 style="z-index:1200;">
                <div class="modal-dialog modal-lg modal-dialog-scrollable"
                     style="margin-top:48px;">
                    <div class="modal-content" style="border-radius:14px;border:none;
                         box-shadow:0 24px 60px rgba(0,0,0,.25);">
                        <div class="modal-header" style="border-bottom:1px solid #f1f5f9;
                             padding:16px 20px;">
                            <h5 class="modal-title fw-bold" style="font-size:16px;color:#1e293b;">
                                ${title}
                            </h5>
                            <button type="button" class="btn-close" id="${xBtnId}"
                                    style="width:28px;height:28px;"></button>
                        </div>
                        <div class="modal-body" style="padding:20px 22px;max-height:60vh;overflow-y:auto;">
                            ${bodyHtml}
                        </div>
                        <div class="modal-footer" style="border-top:1px solid #f1f5f9;
                             padding:14px 20px;gap:8px;">
                            ${footerHtml}
                        </div>
                    </div>
                </div>
            </div>
            <div id="${modalId}_backdrop"
                 style="position:fixed;inset:0;background:rgba(0,0,0,.5);
                        z-index:1150;"></div>`);

        // Show using Bootstrap but suppress default backdrop
        const modalEl = document.getElementById(modalId);
        const bsModal = new bootstrap.Modal(modalEl, { backdrop: false, keyboard: false });
        bsModal.show();
    }

    function closeFeedbackModal(modalId, callback) {
        const el = document.getElementById(modalId);
        const backdropEl = document.getElementById(`${modalId}_backdrop`);
        if (backdropEl) backdropEl.remove();
        if (!el) { if (callback) callback(); return; }
        const instance = bootstrap.Modal.getInstance(el);
        if (instance) instance.hide();
        setTimeout(() => {
            el.remove();
            if (callback) callback();
        }, 300);
    }

    function cleanupModalBackdrops() {
        document.querySelectorAll('[id$="_backdrop"]').forEach(b => b.remove());
    }

    // ============================================================
    // UTILITY
    // ============================================================
    function validateFiles(files) {
        return files.filter(file => {
            if (!ACCEPTED_TYPES.includes(file.type)) {
                showAlert(`"${file.name}" is not a valid file type. Only images and videos are allowed.`, 'warning');
                return false;
            }
            const isVideo = file.type.startsWith('video/');
            const maxBytes = isVideo ? MAX_VIDEO_MB * 1024 * 1024 : MAX_IMAGE_MB * 1024 * 1024;
            if (file.size > maxBytes) {
                showAlert(`"${file.name}" exceeds the size limit (${isVideo ? MAX_VIDEO_MB : MAX_IMAGE_MB}MB).`, 'warning');
                return false;
            }
            return true;
        });
    }

    function showAlert(message, type = 'success') {
        const existing = document.getElementById('fb-inline-alert');
        if (existing) existing.remove();

        const colors = {
            success: { bg: '#f0fdf4', border: '#86efac', text: '#15803d', icon: '✓' },
            danger: { bg: '#fef2f2', border: '#fca5a5', text: '#dc2626', icon: '✕' },
            warning: { bg: '#fffbeb', border: '#fcd34d', text: '#d97706', icon: '!' }
        };
        const c = colors[type] || colors.success;

        const el = document.createElement('div');
        el.id = 'fb-inline-alert';
        el.style.cssText = `position:fixed;top:20px;right:20px;z-index:9999;
            background:${c.bg};border:1.5px solid ${c.border};color:${c.text};
            padding:12px 18px;border-radius:10px;font-size:13px;font-weight:600;
            max-width:320px;box-shadow:0 8px 24px rgba(0,0,0,.12);
            display:flex;align-items:center;gap:8px;`;
        el.innerHTML = `<span style="font-size:16px;">${c.icon}</span> ${message}`;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 4000);
    }

    function formatDate(dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleString('en-GB', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    function getImageUrl(url) {
        if (!url || url.trim() === '') return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='60' height='60'%3E%3Crect width='60' height='60' fill='%23f1f5f9'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%2394a3b8' font-size='22'%3E📦%3C/text%3E%3C/svg%3E`;
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