let currentMap = null;
let showConnections = true;
let showTransport = true;
let currentZoom = 1.0;
let selectedPoint = null;
let isDragging = false;
let startX, startY;
let scrollLeft, scrollTop;
let mapTransports = [];
let hubConnection = null;
let transportElements = new Map();

document.addEventListener('DOMContentLoaded', function () {
    loadMapData();
    updateMapInteractions();

    window.addEventListener('resize', function () {
        renderMap(currentMap);
    });
});

async function loadMapData() {
    try {
        const tokenValid = await validateToken();
        if (!tokenValid) {
            console.error('Invalid token, redirecting to login');
            //window.location.href = "login.html";
            return;
        }

        const mapId = localStorage.getItem('map');
        const map = await getProtectedData('map', 'GET', {
            id: mapId
        });
        currentMap = map;

        await loadMapTransport(mapId);

        renderMap(map);
        initializeSignalRConnection();
    } catch (error) {
        console.error('Ошибка загрузки карты:', error);
        alert('Не удалось загрузить карту');
    }
}

async function validateToken() {
    try {
        const response = await getProtectedData('auth/validate', 'GET');
        return response.ok;
    } catch (error) {
        return false;
    }
}

async function loadMapTransport(mapId) {
    try {
        const transports = await getProtectedData('transport/getMapTransport', 'GET', {
            mapId: mapId
        });
        mapTransports = transports;
        updateTransportCount();
    } catch (error) {
        console.error('Ошибка загрузки транспорта:', error);
    }
}

function initializeSignalRConnection() {
    const token = localStorage.getItem('jwtToken');

    if (!token) {
        console.error('No authentication token found');
        setTimeout(initializeSignalRConnection, 5000);
        return;
    }

    console.log('Initializing SignalR with token:', token.substring(0, 20) + '...');

    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7003/transportClientHub', {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Debug)
        .build();

    hubConnection.on('TransportMapData', (transportDataList) => {
        try {
            console.log('Received transport data:', transportDataList);
            updateTransportPositions(transportDataList);
        } catch (error) {
            console.error('Ошибка обработки данных транспорта:', error);
        }
    });

    hubConnection.on('SubscriptionConfirmed', (confirmedUserId) => {
        console.log('Подписка подтверждена для пользователя:', confirmedUserId);
    });

    hubConnection.on('SubscriptionError', (errorMessage) => {
        console.error('Ошибка подписки:', errorMessage);
    });

    hubConnection.onreconnecting((error) => {
        console.log('SignalR переподключается:', error);
    });

    hubConnection.onreconnected((connectionId) => {
        console.log('SignalR переподключен:', connectionId);
        hubConnection.invoke('SubscribeToUser').catch(err => console.error('Ошибка подписки при переподключении:', err));
    });

    hubConnection.start()
        .then(() => {
            console.log('SignalR соединение установлено');
            return hubConnection.invoke('SubscribeToUser');
        })
        .then(() => {
            console.log('Подписка на транспорт отправлена');
        })
        .catch(error => {
            console.error('Ошибка подключения к SignalR:', error);
            setTimeout(initializeSignalRConnection, 5000);
        });
}

async function getValidToken() {
    let token = localStorage.getItem('jwtToken');

    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const expiry = payload.exp * 1000;
            if (Date.now() >= expiry) {
                console.log('Token expired, refreshing...');
                token = await refreshToken();
            }
        } catch (e) {
            console.error('Error parsing token:', e);
        }
    }

    return token;
}

async function refreshToken() {
    try {
        const response = await fetch('https://localhost:7003/api/auth/refresh', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include'
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('token', data.token);
            return data.token;
        }
    } catch (error) {
        console.error('Token refresh failed:', error);
    }
    return null;
}

function updateTransportPositions(transportDataList) {
    transportDataList.forEach(transportData => {
        const transport = mapTransports.find(t => t.id === transportData.transportId);
        if (transport) {
            transport.point.x = transportData.x;
            transport.point.y = transportData.y;
            transport.isActive = true;

            updateTransportElement(transport);
        }
    });

    const receivedTransportIds = transportDataList.map(t => t.transportId);
    mapTransports.forEach(transport => {
        if (!receivedTransportIds.includes(transport.id) && transport.isActive) {
            transport.isActive = false;
            updateTransportElement(transport);
        }
    });

    updateTransportCount();
}

function updateTransportElement(transport) {
    const transportElement = transportElements.get(transport.id);
    if (transportElement) {
        transportElement.style.left = `${transport.point.x}%`;
        transportElement.style.top = `${transport.point.y}%`;

        if (transport.isActive) {
            transportElement.classList.add('active');
            transportElement.classList.remove('inactive');
        } else {
            transportElement.classList.add('inactive');
            transportElement.classList.remove('active');
        }
    } else {
        addTransportElement(transport);
    }
}

function addTransportElement(transport) {
    const mapCanvas = document.getElementById('mapCanvas');
    const contentContainer = mapCanvas.querySelector('.map-content');

    if (contentContainer) {
        const transportElement = document.createElement('div');
        transportElement.className = 'map-transport';
        transportElement.style.left = `${transport.point.x}%`;
        transportElement.style.top = `${transport.point.y}%`;
        transportElement.dataset.transportId = transport.id;

        if (transport.isActive) {
            transportElement.classList.add('active');
        } else {
            transportElement.classList.add('inactive');
        }

        const icon = document.createElement('i');
        icon.className = 'fas fa-bus';
        transportElement.appendChild(icon);

        const label = document.createElement('div');
        label.className = 'transport-label';
        label.textContent = `Транспорт ${transport.id.substring(0, 8)}`;
        transportElement.appendChild(label);

        transportElement.addEventListener('click', function (e) {
            e.stopPropagation();
            selectTransport(transport);
        });

        contentContainer.appendChild(transportElement);
        transportElements.set(transport.id, transportElement);
    }
}

function renderMap(mapData) {
    const mapCanvas = document.getElementById('mapCanvas');
    mapCanvas.innerHTML = '';

    const contentContainer = document.createElement('div');
    contentContainer.className = 'map-content';
    contentContainer.style.position = 'relative';
    contentContainer.style.width = '100%';
    contentContainer.style.height = '100%';
    contentContainer.style.transformOrigin = '0 0';
    contentContainer.style.transform = `scale(${currentZoom})`;

    mapCanvas.appendChild(contentContainer);

    document.getElementById('mapName').textContent = mapData.name;
    document.getElementById('pointsCount').textContent = mapData.points.length;

    let totalConnections = 0;
    mapData.points.forEach(point => {
        totalConnections += point.connectedPointsIds.length;
    });
    document.getElementById('connectionsCount').textContent = totalConnections;

    if (showConnections) {
        renderConnections(mapData.points, contentContainer);
    }

    mapData.points.forEach(point => {
        renderPoint(point, contentContainer);
    });

    if (showTransport) {
        renderTransports(contentContainer);
    }

    updateMapInteractions();
}

function renderTransports(container) {
    transportElements.clear();

    mapTransports.forEach(transport => {
        if (transport.point) {
            const transportElement = document.createElement('div');
            transportElement.className = 'map-transport';
            transportElement.style.left = `${transport.point.x}%`;
            transportElement.style.top = `${transport.point.y}%`;
            transportElement.dataset.transportId = transport.id;

            if (transport.isActive) {
                transportElement.classList.add('active');
            } else {
                transportElement.classList.add('inactive');
            }

            const icon = document.createElement('i');
            icon.className = 'fas fa-bus';
            transportElement.appendChild(icon);

            const label = document.createElement('div');
            label.className = 'transport-label';
            label.textContent = `Транспорт ${transport.id.substring(0, 8)}`;
            transportElement.appendChild(label);

            transportElement.addEventListener('click', function (e) {
                e.stopPropagation();
                selectTransport(transport);
            });

            transportElement.addEventListener('mouseenter', function () {
                transportElement.style.zIndex = '100';
            });

            transportElement.addEventListener('mouseleave', function () {
                if (!transportElement.classList.contains('active')) {
                    transportElement.style.zIndex = '10';
                }
            });

            container.appendChild(transportElement);
            transportElements.set(transport.id, transportElement);
        }
    });
}

function selectTransport(transport) {
    document.getElementById('pointInfo').textContent =
        `Транспорт: ${transport.id.substring(0, 8)} | Статус: ${transport.isActive ? 'Активен' : 'Неактивен'}`;
}

function updateTransportCount() {
    const activeCount = mapTransports.filter(t => t.isActive).length;
    document.getElementById('activeTransportCount').textContent =
        `${activeCount} / ${mapTransports.length}`;
}

function toggleTransportVisibility() {
    showTransport = !showTransport;

    const toggleIcon = document.getElementById('transportToggleIcon');
    const toggleText = document.getElementById('transportToggleText');

    if (showTransport) {
        toggleIcon.className = 'fas fa-bus';
        toggleText.textContent = 'Скрыть транспорт';
        const contentContainer = document.querySelector('.map-content');
        if (contentContainer) {
            renderTransports(contentContainer);
        }
    } else {
        toggleIcon.className = 'fas fa-eye-slash';
        toggleText.textContent = 'Показать транспорт';
        document.querySelectorAll('.map-transport').forEach(el => el.remove());
        transportElements.clear();
    }
}

function renderConnections(points, container) {
    const pointsDict = {};
    points.forEach(point => {
        pointsDict[point.id] = point;
    });

    const containerRect = container.getBoundingClientRect();
    const containerWidth = containerRect.width;
    const containerHeight = containerRect.height;

    points.forEach(point => {
        point.connectedPointsIds.forEach(connectedPointId => {
            const connectedPoint = pointsDict[connectedPointId];
            if (connectedPoint) {
                const connection = document.createElement('div');
                connection.className = 'connection-line';

                const x1 = (point.x / 100) * containerWidth / currentZoom;
                const y1 = (point.y / 100) * containerHeight / currentZoom;
                const x2 = (connectedPoint.x / 100) * containerWidth / currentZoom;
                const y2 = (connectedPoint.y / 100) * containerHeight / currentZoom;

                const deltaX = x2 - x1;
                const deltaY = y2 - y1;
                const length = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                const angle = Math.atan2(deltaY, deltaX) * 180 / Math.PI;

                connection.style.width = `${length}px`;
                connection.style.height = '4px';
                connection.style.left = `${point.x}%`;
                connection.style.top = `${point.y}%`;
                connection.style.transform = `rotate(${angle}deg)`;
                connection.style.transformOrigin = '0 0';

                container.appendChild(connection);
            }
        });
    });
}

function renderPoint(point, container) {
    const pointElement = document.createElement('div');
    pointElement.className = 'map-point';
    pointElement.style.left = `${point.x}%`;
    pointElement.style.top = `${point.y}%`;
    pointElement.dataset.pointId = point.id;

    const label = document.createElement('div');
    label.className = 'point-label';
    label.textContent = point.name;
    pointElement.appendChild(label);

    pointElement.addEventListener('click', function (e) {
        e.stopPropagation();

        document.querySelectorAll('.map-point.active').forEach(point => {
            point.classList.remove('active');
        });

        pointElement.classList.add('active');

        selectPoint(point);
    });

    pointElement.addEventListener('mouseenter', function () {
        pointElement.style.zIndex = '100';
    });

    pointElement.addEventListener('mouseleave', function () {
        if (!pointElement.classList.contains('active')) {
            pointElement.style.zIndex = '10';
        }
    });

    container.appendChild(pointElement);
}

function selectPoint(point) {
    if (selectedPoint) {
        const prevPoint = document.querySelector(`[data-point-id="${selectedPoint.id}"]`);
        if (prevPoint) prevPoint.classList.remove('active');
    }

    selectedPoint = point;
    const pointElement = document.querySelector(`[data-point-id="${point.id}"]`);
    if (pointElement) pointElement.classList.add('active');

    showPointInfo(point);
}

function showPointInfo(point) {
    console.log('Выбрана точка:', point);
    document.getElementById('pointInfo').textContent = point.name;
}

function refreshMap() {
    loadMapData();
}

async function downloadMap() {
    try {
        const response = await getProtectedData('map/exportMap', 'GET', {
            mapId: currentMap.id
        });

        console.log('Тип ответа:', typeof response);
        console.log('Ответ:', response);

        let content;
        let filename = currentMap.name;

        if (typeof response === 'string') {
            try {
                JSON.parse(response);
                content = response;
            } catch (e) {
                content = response;
                filename = currentMap.name;
            }
        } else if (typeof response === 'object') {
            content = JSON.stringify(response, null, 2);
        } else {
            content = String(response);
        }

        downloadContent(content, filename, 'application/json');

    } catch (error) {
        console.error('Ошибка:', error);
        alert('Не удалось скачать карту');
    }
}

function downloadContent(content, filename, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    setTimeout(() => URL.revokeObjectURL(url), 100);
}

function toggleConnections() {
    showConnections = !showConnections;
    loadMapData();
}

function zoomIn() {
    if (currentZoom < 2.0) {
        currentZoom += 0.1;
        updateZoom();
    }
}

function zoomOut() {
    if (currentZoom > 1) {
        currentZoom -= 0.1;
        updateZoom();
    }
}


function updateZoom() {
    const mapCanvas = document.getElementById('mapCanvas');
    const content = mapCanvas.querySelector('.map-content');

    if (content) {
        content.style.transform = `scale(${currentZoom})`;
    }
}

function updateMapInteractions() {
    const mapCanvas = document.getElementById('mapCanvas');
    const points = document.querySelectorAll('.map-point');

    mapCanvas.style.cursor = 'grab';
    mapCanvas.style.overflow = 'auto';

    updateScrollableArea();

    mapCanvas.onmousedown = startDrag;
    mapCanvas.onmousemove = drag;
    mapCanvas.onmouseup = endDrag;
    mapCanvas.onmouseleave = endDrag;
}

function updateScrollableArea() {
    const mapCanvas = document.getElementById('mapCanvas');
    const content = mapCanvas.querySelector('.map-content');
}

function startDrag(e) {
    if (e.target.closest('.map-point')) {
        return;
    }

    isDragging = true;
    const mapCanvas = document.getElementById('mapCanvas');

    startX = e.pageX - mapCanvas.offsetLeft;
    startY = e.pageY - mapCanvas.offsetTop;
    scrollLeft = mapCanvas.scrollLeft;
    scrollTop = mapCanvas.scrollTop;

    mapCanvas.style.cursor = 'grabbing';
    e.preventDefault();
}

function drag(e) {
    if (!isDragging) return;

    const mapCanvas = document.getElementById('mapCanvas');

    const x = e.pageX - mapCanvas.offsetLeft;
    const y = e.pageY - mapCanvas.offsetTop;
    const walkX = (x - startX) * 2 / currentZoom;
    const walkY = (y - startY) * 2 / currentZoom;

    mapCanvas.scrollLeft = scrollLeft - walkX;
    mapCanvas.scrollTop = scrollTop - walkY;
}

function endDrag() {
    isDragging = false;
    const mapCanvas = document.getElementById('mapCanvas');
    mapCanvas.style.cursor = 'grab';
}

window.addEventListener('beforeunload', function () {
    if (hubConnection) {
        hubConnection.stop();
    }
});