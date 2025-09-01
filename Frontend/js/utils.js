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

function logout() {
    if (confirm("Вы уверены, что хотите выйти?")) {
        console.log("Пользователь вышел из системы");
        window.location.href = "index.html";
        localStorage.removeItem('jwtToken');
    }
}
