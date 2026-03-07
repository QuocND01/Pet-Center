(function () {
    'use strict';

    function getJWTFromServer() {
        return new Promise((resolve) => {
            fetch('/Auth/CheckAuth')
                .then(response => response.json())
                .then(data => {
                    if (data.isAuthenticated && data.token) {
                        localStorage.setItem('JWT', data.token);
                        resolve(data.token);
                    } else {
                        resolve(null);
                    }
                })
                .catch(err => {
                    console.error('❌ CheckAuth Error:', err);
                    resolve(null);
                });
        });
    }

    async function checkAuthStatus() {
        let token = localStorage.getItem('JWT');

        // Nếu không có token trong localStorage, kiểm tra từ server
        if (!token) {
            token = await getJWTFromServer();
        }

        const loginBtn = document.querySelector('.login-nav-item');
        const userMenu = document.querySelector('.user-menu');

        if (token) {
            // User đã login
            if (loginBtn) {
                loginBtn.classList.add('hidden');
                loginBtn.style.display = 'none';
            }
            if (userMenu) {
                userMenu.classList.remove('hidden');
                userMenu.style.display = 'block';
            }
            return true;
        } else {
            // User chưa login
            if (loginBtn) {
                loginBtn.classList.remove('hidden');
                loginBtn.style.display = 'block';
            }
            if (userMenu) {
                userMenu.classList.add('hidden');
                userMenu.style.display = 'none';
            }
            return false;
        }
    }

    function setupAvatarDropdown() {
        const avatarBtn = document.getElementById('avatarBtn');
        const userDropdown = document.getElementById('userDropdown');

        if (!avatarBtn || !userDropdown) {
            return;
        }

        avatarBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            userDropdown.classList.toggle('show');
        });

        document.addEventListener('click', function (e) {
            if (!userDropdown.contains(e.target) && e.target !== avatarBtn) {
                userDropdown.classList.remove('show');
            }
        });

        const dropdownLinks = userDropdown.querySelectorAll('a, button');
        dropdownLinks.forEach(link => {
            link.addEventListener('click', function () {
                userDropdown.classList.remove('show');
            });
        });
    }

    function setupLogout() {
        const logoutBtn = document.getElementById('logoutBtn');
        if (!logoutBtn) {
            return;
        }

        console.log('✅ Setting up logout button');

        logoutBtn.addEventListener('click', function (e) {
            e.preventDefault();
            localStorage.removeItem('JWT');
            window.location.href = '/Auth/Logout';
        });
    }

    document.addEventListener('DOMContentLoaded', async function () {

        // Kiểm tra auth status ngay khi page load
        const isAuthenticated = await checkAuthStatus();

        // Chỉ setup dropdown nếu user đã login
        if (isAuthenticated) {
            setupAvatarDropdown();
            setupLogout();
        } else {
            console.log('⏭️ Skipping dropdown setup - user not authenticated');
        }

        // Kiểm tra lại sau 1 giây
        setTimeout(async () => {
            console.log('🔄 Re-checking auth status after 1 second');
            await checkAuthStatus();
        }, 1000);
    });

    window.Auth = {
        checkAuthStatus: checkAuthStatus,
        getToken: () => localStorage.getItem('JWT'),
        setToken: (token) => localStorage.setItem('JWT', token),
        clearToken: () => localStorage.removeItem('JWT')
    };
})();