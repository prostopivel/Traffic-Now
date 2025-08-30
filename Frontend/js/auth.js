async function login() {
    const username = document.getElementById('Email').value;
    const password = document.getElementById('password').value;

    const passwordIssue = document.getElementById('passwordIssue');

    try {
        const hashedPassword = await hashPassword(password);
        if (passwordIssue) {
            passwordIssue.style.display = 'none';
        }

        const response = await fetch('https://localhost:7003/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username,
                password: hashedPassword
            })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('jwtToken', data.token);

            console.log('Login successful!');
            window.location.replace('menu.html');
        } else {
            let errorMessage = 'Ошибка авторизации';

            try {
                const errorData = await response.json();
                if (errorData && errorData.message) {
                    errorMessage = errorData.message;
                }
            } catch (e) {
                errorMessage = `Ошибка ${response.status}: ${response.statusText}`;
            }

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
        console.error('Login error:', error);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}

async function authorize() {
    const username = document.getElementById('Email').value;
    const password = document.getElementById('password').value;

    try {
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (password != confirmPassword) {
            document.getElementById('error').innerHTML = 'Пароли должны совпадать!';
            return;
        }

        const hashedPassword = await hashPassword(password);

        const response = await fetch('https://localhost:7003/api/auth/authorize', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username,
                password: hashedPassword
            })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('jwtToken', data.token);

            console.log('Authorize successful!');
            window.location.replace('menu.html');
        } else {
            console.log('Authorize failed!');

            if ([400, 401, 403, 404, 500].includes(response.status)) {
                throw new Error(`HTTP ${response.status}`);
            }

            throw new Error(`HTTP error! status: ${response.status}`);
        }
    } catch (error) {
        console.error('Login error:', error);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}