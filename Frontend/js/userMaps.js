import { logout, getProtectedData, getBodyProtectedData } from "./utils.js";

let allMaps = [];
let searchTimeout = null;

document.addEventListener('DOMContentLoaded', function () {
    loadMaps();

    document.addEventListener('click', function (e) {
        const searchResults = document.getElementById('searchResults');
        const searchBox = document.querySelector('.search-box');

        if (searchResults && searchBox &&
            !searchResults.contains(e.target) &&
            !searchBox.contains(e.target)) {
            searchResults.classList.remove('visible');
        }
    });

    window.addEventListener('resize', function () {
        renderMaps(allMaps);
    });

    document.addEventListener('click', (event) => {
        if (event.target.classList.contains('add-map-btn-search') ||
            event.target.closest('.add-map-btn-search')) {

            const button = event.target.classList.contains('add-map-btn-search')
                ? event.target
                : event.target.closest('.add-map-btn-search');

            if (button.disabled) return;

            const id = button.id.replace('add-map-btn-search-', '');
            addMapToUser(id);
        }
    });

    document.addEventListener('click', (event) => {
        if (event.target.classList.contains('delete-map-btn') ||
            event.target.closest('.delete-map-btn')) {

            const button = event.target.classList.contains('delete-map-btn')
                ? event.target
                : event.target.closest('.delete-map-btn');

            if (button.disabled) return;

            event.stopPropagation();
            event.preventDefault();

            const id = button.id.replace('delete-map-btn-', '');
            deleteMap(id, event);
        }
    });
});

document.getElementById('logout-btn').addEventListener('click', logout);
document.getElementById('clear-search').addEventListener('click', clearSearch);
document.getElementById('mapSearch').addEventListener('input', searchMaps);

async function loadMaps() {
    try {
        const maps = await getProtectedData('map/getMaps', 'GET');
        allMaps = maps;
        renderMaps(maps);
    } catch (error) {
        console.error('Ошибка загрузки карт:', error);
        showError('Не удалось загрузить список карт');
    }
}

function searchMaps() {
    const searchTerm = document.getElementById('mapSearch').value.trim();
    const searchResults = document.getElementById('searchResults');

    if (searchTimeout) {
        clearTimeout(searchTimeout);
    }

    if (!searchTerm) {
        searchResults.classList.remove('visible');
        searchResults.innerHTML = '';
        return;
    }

    searchResults.innerHTML = `
        <div class="search-loading">
            <i class="fas fa-spinner"></i>
            Поиск карт...
        </div>
    `;
    searchResults.classList.add('visible');

    searchTimeout = setTimeout(async () => {
        try {
            const foundMaps = await getProtectedData('map/search', 'GET', {
                name: searchTerm
            });

            displaySearchResults(foundMaps);
        } catch (error) {
            console.error('Ошибка поиска карт:', error);
            searchResults.innerHTML = `
                <div class="search-error">
                    <i class="fas fa-exclamation-triangle"></i>
                    Ошибка поиска
                </div>
            `;
        }
    }, 500);
}

function displaySearchResults(maps) {
    const searchResults = document.getElementById('searchResults');

    if (!maps || maps.length === 0) {
        searchResults.innerHTML = `
            <div class="search-no-results">
                <i class="fas fa-search"></i>
                <p>Карты не найдены</p>
            </div>
        `;
        return;
    }

    let html = '';

    maps.forEach(map => {
        const isAlreadyAdded = allMaps.some(userMap => userMap.id === map.id);
        const pointsCount = map.points ? map.points.length : 0;
        const connectionsCount = countConnections(map.points || []);

        html += `
            <div class="search-result-item" data-map-id="${map.id}">
                <div class="search-result-info">
                    <div class="search-result-name">${escapeHtml(map.name)}</div>
                    <div class="search-result-stats">
                        <div class="search-result-stat">
                            <i class="fas fa-dot-circle"></i>
                            ${pointsCount} точек
                        </div>
                        <div class="search-result-stat">
                            <i class="fas fa-link"></i>
                            ${connectionsCount} связей
                        </div>
                    </div>
                </div>
                <button class="add-map-btn-search"
                        id="add-map-btn-search-${map.id}"
                        ${isAlreadyAdded ? 'disabled' : ''}>
                    ${isAlreadyAdded ? 'Добавлена' : 'Добавить'}
                </button>
            </div>
        `;
    });

    searchResults.innerHTML = html;
}

async function addMapToUser(mapId) {
    try {
        const result = await getProtectedData('map/addUserMap', 'POST', {
            mapId: mapId
        });

        if (result) {
            alert('Карта успешно добавлена!');

            await loadMaps();

            const addButton = document.querySelector(`[data-map-id="${mapId}"] .add-map-btn-search`);
            if (addButton) {
                addButton.textContent = 'Добавлена';
                addButton.disabled = true;
                addButton.classList.add('added');
            }

            setTimeout(() => {
                const searchResults = document.getElementById('searchResults');
                if (searchResults) {
                    searchResults.classList.remove('visible');
                }
            }, 1500);
        }
    } catch (error) {
        console.error('Ошибка добавления карты:', error);
        alert('Не удалось добавить карту');
    }
}

function clearSearch() {
    document.getElementById('mapSearch').value = '';
    const searchResults = document.getElementById('searchResults');
    searchResults.classList.remove('visible');
    searchResults.innerHTML = '';
}

function renderMaps(maps) {
    const container = document.getElementById('mapsContainer');

    if (!maps || maps.length === 0) {
        container.innerHTML = `
            <div class="no-results">
                <i class="fas fa-map-marked-alt"></i>
                <p>Нет доступных карт</p>
                <p style="font-size: 0.9em; margin-top: 10px; color: #888;">
                    Используйте поиск выше чтобы найти и добавить карты
                </p>
            </div>
        `;
        return;
    }

    container.innerHTML = '';
    maps.forEach(map => {
        const mapCard = createMapCard(map);
        container.appendChild(mapCard);
    });
}

function createMapCard(map) {
    const card = document.createElement('div');
    card.className = 'map-card';
    card.onclick = () => viewMap(map.id);

    card.innerHTML = `
        <div class="map-header">
            <i class="fas fa-map"></i>
            <h3>${escapeHtml(map.name)}</h3>
        </div>
        <div class="map-preview">
            <div class="map-canvas" id="preview-${map.id}">
                <div class="map-content"></div>
            </div>
       </div>
        <div class="map-stats">
            <div class="map-stat">
                <i class="fas fa-dot-circle"></i>
                <span>${map.points.length} точек</span>
            </div>
            <div class="map-stat">
                <i class="fas fa-link"></i>
                <span>${countConnections(map.points)} связей</span>
            </div>
        </div>
        <div class="map-actions">
            <button class="delete-map-btn" id="delete-map-btn-${map.id}">
                <i class="fas fa-trash"></i> Удалить
            </button>
        </div>
    `;
    setTimeout(() => renderMapPreview(map, card), 100);

    return card;
}

function renderMapPreview(map, card) {
    const previewContainer = card.querySelector(`#preview-${map.id} .map-content`);
    if (!previewContainer) return;

    const containerRect = previewContainer.getBoundingClientRect();
    const containerWidth = containerRect.width;
    const containerHeight = containerRect.height;

    previewContainer.innerHTML = '';

    if (map.points && map.points.length > 0) {
        renderConnectionsPreview(map.points, previewContainer, containerWidth, containerHeight);

        map.points.forEach(point => {
            renderPointPreview(point, previewContainer, containerWidth, containerHeight);
        });
    }
}

function renderConnectionsPreview(points, container, containerWidth, containerHeight) {
    const pointsDict = {};
    points.forEach(point => {
        pointsDict[point.id] = point;
    });

    points.forEach(point => {
        if (point.connectedPointsIds) {
            point.connectedPointsIds.forEach(connectedPointId => {
                const connectedPoint = pointsDict[connectedPointId];
                if (connectedPoint) {
                    const connection = document.createElement('div');
                    connection.className = 'connection-line';

                    const x1 = (point.x / 100) * containerWidth;
                    const y1 = (point.y / 100) * containerHeight;
                    const x2 = (connectedPoint.x / 100) * containerWidth;
                    const y2 = (connectedPoint.y / 100) * containerHeight;

                    const deltaX = x2 - x1;
                    const deltaY = y2 - y1;
                    const length = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                    const angle = Math.atan2(deltaY, deltaX) * 180 / Math.PI;

                    connection.style.width = `${length}px`;
                    connection.style.height = '2px';
                    connection.style.left = `${point.x}%`;
                    connection.style.top = `${point.y}%`;
                    connection.style.transform = `rotate(${angle}deg)`;
                    connection.style.transformOrigin = '0 0';
                    connection.style.backgroundColor = '#667eea';
                    connection.style.opacity = '0.4';

                    container.appendChild(connection);
                }
            });
        }
    });
}

function renderPointPreview(point, container, containerWidth, containerHeight) {
    const pointElement = document.createElement('div');
    pointElement.className = 'map-point';
    pointElement.style.left = `${point.x}%`;
    pointElement.style.top = `${point.y}%`;
    pointElement.title = point.name;

    const label = document.createElement('div');
    label.className = 'point-label';
    label.textContent = point.name;
    pointElement.appendChild(label);

    container.appendChild(pointElement);
}

function countConnections(points) {
    if (!points) return 0;

    let total = 0;
    points.forEach(point => {
        if (point.connectedPointsIds) {
            total += point.connectedPointsIds.length;
        }
    });
    return total;
}

function viewMap(mapId) {
    localStorage.setItem('map', mapId);
    window.location.href = `map.html`;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

async function deleteMap(mapId, event) {
    if (event) {
        event.stopPropagation();
        event.preventDefault();
    }

    if (!confirm('Вы уверены, что хотите удалить эту карту?')) {
        return;
    }

    try {
        await getProtectedData('map/deleteMap', 'DELETE', {
            mapId: mapId
        });

        allMaps = allMaps.filter(m => m.id !== mapId);
        renderMaps(allMaps);

        showSuccess('Карта успешно удалена!');

    } catch (error) {
        console.error('Ошибка удаления карты:', error);
        showError('Не удалось удалить карту');
    }
}

function showSuccess(message) {
    const messageContainer = document.getElementById('messageContainer');
    messageContainer.innerHTML = `
                <div class="message message-success">
                    <i class="fas fa-check-circle"></i>
                    ${message}
                </div>
            `;

    setTimeout(() => {
        messageContainer.innerHTML = '';
    }, 3000);
}

function showError(message) {
    const messageContainer = document.getElementById('messageContainer');
    messageContainer.innerHTML = `
                <div class="message message-error">
                    <i class="fas fa-exclamation-circle"></i>
                    ${message}
                </div>
            `;

    setTimeout(() => {
        messageContainer.innerHTML = '';
    }, 5000);
}