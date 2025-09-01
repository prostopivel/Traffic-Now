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
                <a href="menu.html" class="nav-item active">
                    <i class="fas fa-home"></i> Главная
                </a>
                <a href="userMaps.html" class="nav-item">
                    <i class="fas fa-map"></i> Просмотр карты
                </a>
                <a href="#" class="nav-item">
                    <i class="fas fa-bus"></i> Просмотр транспорта
                </a>
                <a href="#" class="nav-item">
                    <i class="fas fa-user-cog"></i> Изменение информации
                </a>
                <a href="#" class="nav-item">
                    <i class="fas fa-history"></i> История поездок
                </a>
                <a href="#" class="nav-item">
                    <i class="fas fa-chart-line"></i> Аналитика
                </a>
                <a href="#" class="nav-item">
                    <i class="fas fa-cog"></i> Настройки
                </a>
            </div>
        </div>
    `;
    
    const container = document.getElementById('sidebar-container');
    if (container) {
        container.innerHTML = sidebarHTML;
        applySidebarStyles();
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

document.addEventListener('DOMContentLoaded', createSidebar);

window.addEventListener('resize', applySidebarStyles);