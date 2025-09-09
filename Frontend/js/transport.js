let allTransport = [];

document.addEventListener('DOMContentLoaded', function () {
    loadTransport();
});

async function loadTransport() {
    try {
        const transport = await getProtectedData('transport/getTransport', 'GET');

        allTransport = transport;
        renderTransport(allTransport);

    } catch (error) {
        console.error('Ошибка загрузки транспорта:', error);
        showError('Не удалось загрузить список транспорта');
    }
}

function renderTransport(transportList) {
    const container = document.getElementById('transportContainer');

    if (!transportList || transportList.length === 0) {
        container.innerHTML = `
                    <div class="no-results" style="grid-column: 1 / -1; text-align: center; padding: 40px;">
                        <i class="fas fa-bus" style="font-size: 3em; color: #667eea; margin-bottom: 15px;"></i>
                        <p>Нет подключенного транспорта</p>
                        <p style="font-size: 0.9em; margin-top: 10px; color: #888;">
                            Введите URL GPS сервера транспорта ниже чтобы добавить транспорт
                        </p>
                    </div>
                `;
        return;
    }

    container.innerHTML = '';

    transportList.forEach(transport => {
        const transportCard = createTransportCard(transport);
        container.appendChild(transportCard);
    });
}

function createTransportCard(transport) {
    const card = document.createElement('div');
    card.className = 'transport-card';
    card.dataset.id = transport.id;

    let statusClass = 'status-offline';
    let statusText = 'Не в сети';
    let statusIcon = 'fa-question-circle';

    if (transport.isActive) {
        statusClass = 'status-online';
        statusText = 'В сети';
        statusIcon = 'fa-check-circle';
    } else if (transport.hasError) {
        statusClass = 'status-error';
        statusText = 'Ошибка';
        statusIcon = 'fa-exclamation-circle';
    }

    const coordinates = transport.point ?
        `X: ${transport.point.x.toFixed(2)}%, Y: ${transport.point.y.toFixed(2)}%` :
        'Неизвестно';

    card.innerHTML = `
                <div class="transport-header">
                    <i class="fas fa-bus"></i>
                    <h3>${escapeHtml(transport.point?.name || 'Транспорт')}</h3>
                </div>
                <div class="transport-info">
                    <div class="transport-details">
                        <div class="transport-detail">
                            <i class="fas fa-link"></i>
                            <span>${transport.url ? shortenUrl(transport.url) : 'URL не указан'}</span>
                        </div>
                        <div class="transport-detail">
                            <i class="fas fa-map-marker-alt"></i>
                            <span>Координаты:</span>
                        </div>
                        <div class="coordinates">
                            <span class="coordinate">X: ${transport.point?.x?.toFixed(2) || '0'}%</span>
                            <span class="coordinate">Y: ${transport.point?.y?.toFixed(2) || '0'}%</span>
                        </div>
                        <div class="transport-detail">
                            <i class="fas fa-clock"></i>
                            <span>Обновлено: ${transport.lastUpdate || 'Неизвестно'}</span>
                        </div>
                        <div class="transport-detail">
                            <i class="fas fa-project-diagram"></i>
                            <span>Связей: ${transport.point?.connectedPointsIds?.length || 0}</span>
                        </div>
                    </div>
                    <div class="transport-status ${statusClass}">
                        <i class="fas ${statusIcon}"></i>
                        <span>&nbsp;${statusText}</span>
                    </div>
                </div>
                <div class="transport-actions">
                    <button class="delete-transport-btn" onclick="deleteTransport('${transport.id}', event)">
                        <i class="fas fa-trash"></i> Удалить
                    </button>
                </div>
            `;

    return card;
}

async function connectTransport() {
    const urlInput = document.getElementById('transportUrl');
    const url = urlInput.value.trim();
    const button = document.getElementById('addTransportBtn');

    if (!url) {
        showError('Пожалуйста, введите URL GPS сервера транспорта');
        return;
    }

    if (!isValidUrl(url)) {
        showError('Пожалуйста, введите корректный URL');
        return;
    }

    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Подключение...';

    try {
        const result = await getProtectedData('transport/connect', 'POST', { url: url });

        setTimeout(async () => {
            showSuccess('Транспорт успешно подключен!');
            urlInput.value = '';

            const newTransport = await getProtectedData('transport', 'GET', { transportId: result });

            allTransport.push(newTransport);
            renderTransport(allTransport);

            button.disabled = false;
            button.innerHTML = '<i class="fas fa-plus"></i> Подключить';
        }, 3000);

    } catch (error) {
        console.error('Ошибка подключения транспорта:', error);
        showError('Не удалось подключить транспорт: ' + (error.message || 'неизвестная ошибка'));

        button.disabled = false;
        button.innerHTML = '<i class="fas fa-plus"></i> Подключить';
    }
}

async function deleteTransport(transportId, event) {
    if (event) {
        event.stopPropagation();
    }

    if (!confirm('Вы уверены, что хотите удалить этот транспорт?')) {
        return;
    }

    try {
        await getProtectedData('transport/deleteUser', 'DELETE', { transportId: transportId });

        allTransport = allTransport.filter(t => t.id !== transportId);
        renderTransport(allTransport);

        showSuccess('Транспорт успешно удален!');

    } catch (error) {
        console.error('Ошибка удаления транспорта:', error);
        showError('Не удалось удалить транспорт');
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

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function shortenUrl(url, maxLength = 30) {
    if (!url) return '';
    if (url.length <= maxLength) return url;

    return url.substring(0, maxLength - 3) + '...';
}

function getUrl(url) {
    const lastPart = url.lastIndexOf('/');
    const result = lastPart !== -1 ? url.slice(0, lastPart) : url;
    return result;
}

function isValidUrl(string) {
    try {
        new URL(string);
        return true;
    } catch (_) {
        return false;
    }
}