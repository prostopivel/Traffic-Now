document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    const errorCode = urlParams.get('code');
    const errorMessage = urlParams.get('message');
    const errorStatus = urlParams.get('status');

    if (errorCode) {
        document.getElementById('errorCode').textContent = errorCode;
        document.getElementById('errorStatus').textContent = errorStatus || 'Unknown';
        document.getElementById('errorDetails').style.display = 'block';
    }

    if (errorMessage) {
        document.getElementById('errorMessage').textContent = decodeURIComponent(errorMessage);
    }

    switch(errorCode) {
        case '400':
            document.getElementById('errorTitle').textContent = 'Неверный запрос';
            break;
        case '401':
            document.getElementById('errorTitle').textContent = 'Неавторизованный доступ';
            break;
        case '403':
            document.getElementById('errorTitle').textContent = 'Доступ запрещен';
            break;
        case '404':
            document.getElementById('errorTitle').textContent = 'Страница не найдена';
            break;
        case '500':
            document.getElementById('errorTitle').textContent = 'Ошибка сервера';
            break;
        default:
            document.getElementById('errorTitle').textContent = 'Произошла ошибка';
    }
});

function redirectToError(errorCode, errorMessage, errorStatus) {
    const message = encodeURIComponent(errorMessage);
    window.location.href = `error.html?code=${errorCode}&message=${message}&status=${errorStatus}`;
}