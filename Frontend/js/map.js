let currentMap = null;
let showConnections = true;
let currentZoom = 1.0;
let selectedPoint = null;
let isDragging = false;
let startX, startY;
let scrollLeft, scrollTop;

document.addEventListener('DOMContentLoaded', function () {
    loadMapData();
    updateMapInteractions();
});

async function loadMapData() {
    try {
        const mapId = localStorage.getItem('map');
        const map = await getProtectedData('map', 'GET', { id: mapId });
        renderMap(map);
    } catch (error) {
        console.error('Ошибка загрузки карты:', error);
        alert('Не удалось загрузить карту');
    }
}

function renderMap(mapData) {
    currentMap = mapData;
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

    updateMapInteractions();
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
        selectPoint(point);
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

function downloadMap() {
    alert('Функция экспорта карты будет реализована позже');
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