async function hashPassword(password) {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);

    const hashBuffer = await crypto.subtle.digest('SHA-256', data);

    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

    return hashHex;
}

async function getProtectedData(path, method, params = {}) {
    try {
        const token = localStorage.getItem('jwtToken');

        let url = 'https://localhost:7003/api/' + path;

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

function redirectToError(errorCode, errorMessage, errorStatus) {
    const message = encodeURIComponent(errorMessage);
    window.location.href = `error.html?code=${errorCode}&message=${message}&status=${errorStatus}`;
}

function logout() {
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
                <a href="#" id="nav-transport" class="nav-item">
                    <i class="fas fa-bus"></i> Просмотр транспорта
                </a>
                <a href="#" id="nav-profile" class="nav-item">
                    <i class="fas fa-user-cog"></i> Изменение информации
                </a>
                <a href="#" id="nav-history" class="nav-item">
                    <i class="fas fa-history"></i> История поездок
                </a>
                <a href="#" id="nav-analytics" class="nav-item">
                    <i class="fas fa-chart-line"></i> Аналитика
                </a>
                <a href="#" id="nav-settings" class="nav-item">
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
    } else if (pageTitle.includes('транспорт') || pageTitle.includes('transport') || pageTitle.includes('bus')) {
        activeNavId = 'nav-transport';
    } else if (pageTitle.includes('профиль') || pageTitle.includes('profile') || pageTitle.includes('информация')) {
        activeNavId = 'nav-profile';
    } else if (pageTitle.includes('история') || pageTitle.includes('history')) {
        activeNavId = 'nav-history';
    } else if (pageTitle.includes('аналитика') || pageTitle.includes('analytics') || pageTitle.includes('chart')) {
        activeNavId = 'nav-analytics';
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
                icon.style.color = '#667eea';
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

document.addEventListener('DOMContentLoaded', function() {
    createSidebar();
});

window.addEventListener('resize', applySidebarStyles);

let lastTitle = document.title;
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.type === 'attributes' && mutation.attributeName === 'title') {
            setActiveNavItem();
        }
    });
});

observer.observe(document.querySelector('title'), { 
    attributes: true,
    attributeFilter: ['title']
});