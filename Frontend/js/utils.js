export const serverPath = 'https://localhost:7003/';

function loadTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    changeTheme(savedTheme);
}

export function changeTheme(theme) {
    if (theme === 'dark') {
        document.body.classList.add('dark-theme');
    } else {
        document.body.classList.remove('dark-theme');
    }
}

export async function hashPassword(password) {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);

    const hashBuffer = await crypto.subtle.digest('SHA-256', data);

    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

    return hashHex;
}

export async function getProtectedData(path, method, params = {}) {
    try {
        const token = localStorage.getItem('jwtToken');

        let url = serverPath + 'api/' + path;

        if (Object.keys(params).length > 0) {
            const queryParams = new URLSearchParams(params).toString();
            url += '?' + queryParams;
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            try {
                const data = await response.json();
                console.log('Data received successfully!');
                return data;
            }
            catch {
                return null;
            }
        } else {
            if (response.status === 401 && errorMessage.includes('пароль')) {
                const passwordIssue = document.getElementById('passwordIssue');
                if (passwordIssue) {
                    passwordIssue.textContent = errorMessage;
                    passwordIssue.style.display = 'block';
                    return;
                }
            }

            redirectToError(response.status, errorMessage, response.statusText);
        }
    } catch (error) {
        console.error('Request error:', error);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}

export async function getBodyProtectedData(path, method, params = {}) {
    try {
        const token = localStorage.getItem('jwtToken');

        let url = serverPath + 'api/' + path;

        const response = await fetch(url, {
            method: method,
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(params)
        });

        if (response.ok) {
            const data = await response.json();
            console.log('Data received successfully!');
            return data;
        } else {
            if (response.status === 401 && errorMessage.includes('пароль')) {
                const passwordIssue = document.getElementById('passwordIssue');
                if (passwordIssue) {
                    passwordIssue.textContent = errorMessage;
                    passwordIssue.style.display = 'block';
                    return;
                }
            }

            redirectToError(response.status, errorMessage, response.statusText);
        }
    } catch (error) {
        console.error('Request error:', error);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}

export function redirectToError(errorCode, errorMessage, errorStatus) {
    const message = encodeURIComponent(errorMessage);
    window.location.href = `error.html?code=${errorCode}&message=${message}&status=${errorStatus}`;
}

export function logout() {
    if (confirm("Вы уверены, что хотите выйти?")) {
        console.log("Пользователь вышел из системы");
        window.location.href = "index.html";
        localStorage.removeItem('jwtToken');
    }
}

function createSidebar() {
    const sidebarHTML = `
        <div class="sidebar">
            <div class="sidebar-header">
                <h2>🚗 TrafficNow</h2>
            </div>
            <div class="nav-items">
                <a href="menu.html" id="nav-menu" class="nav-item">
                    <i class="fas fa-home"></i> Главная
                </a>
                <a href="userMaps.html" id="nav-maps" class="nav-item">
                    <i class="fas fa-map"></i> Мои карты
                </a>
                <a href="transport.html" id="nav-transport" class="nav-item">
                    <i class="fas fa-bus"></i> Просмотр транспорта
                </a>
                <a href="userChange.html" id="nav-userChange" class="nav-item">
                    <i class="fas fa-user-cog"></i> Изменение информации
                </a>
                <a href="settings.html" id="nav-settings" class="nav-item">
                    <i class="fas fa-cog"></i> Настройки
                </a>
            </div>
        </div>
    `;

    const container = document.getElementById('sidebar-container');
    if (container) {
        container.innerHTML = sidebarHTML;
        applySidebarStyles();
        setActiveNavItem();
    }
}

function setActiveNavItem() {
    const currentPage = window.location.pathname.split('/').pop();
    const pageTitle = document.title.toLowerCase();

    let activeNavId = '';

    if (currentPage === 'menu.html' || pageTitle.includes('панель') || pageTitle.includes('главная')) {
        activeNavId = 'nav-menu';
    } else if (currentPage === 'userMaps.html' || currentPage === 'map.html' || pageTitle.includes('карт') || pageTitle.includes('map')) {
        activeNavId = 'nav-maps';
    } else if (currentPage === 'transport.html' || pageTitle.includes('транспорт') || pageTitle.includes('transport') || pageTitle.includes('bus')) {
        activeNavId = 'nav-transport';
    } else if (pageTitle.includes('изменение') || pageTitle.includes('userChange') || pageTitle.includes('информации')) {
        activeNavId = 'nav-userChange';
    } else if (pageTitle.includes('настройк') || pageTitle.includes('settings') || pageTitle.includes('cog')) {
        activeNavId = 'nav-settings';
    }

    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.classList.remove('active');
    });

    if (activeNavId) {
        const activeNav = document.getElementById(activeNavId);
        if (activeNav) {
            activeNav.classList.add('active');

            const icon = activeNav.querySelector('i');
            if (icon) {
                if (localStorage.getItem('theme') == 'dark') {
                    icon.style.color = '#ecf0f1';
                } else {
                    icon.style.color = '#667eea';
                }
            }
        }
    }
}

function applySidebarStyles() {
    const container = document.getElementById('sidebar-container');
    const sidebar = document.querySelector('.sidebar');

    if (container && sidebar) {
        container.style.height = '100vh';
        container.style.position = 'sticky';
        container.style.top = '0';

        sidebar.style.height = '100%';
        sidebar.style.minHeight = '100vh';
        sidebar.style.display = 'flex';
        sidebar.style.flexDirection = 'column';

        const navItems = document.querySelector('.nav-items');
        if (navItems) {
            navItems.style.flexGrow = '1';
            navItems.style.display = 'flex';
            navItems.style.flexDirection = 'column';
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    createSidebar();
    loadTheme();
});

window.addEventListener('resize', applySidebarStyles);

let lastTitle = document.title;
const observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (mutation) {
        if (mutation.type === 'attributes' && mutation.attributeName === 'title') {
            setActiveNavItem();
        }
    });
});

observer.observe(document.querySelector('title'), {
    attributes: true,
    attributeFilter: ['title']
});