import { serverPath, hashPassword, redirectToError } from "./utils.js";

document.getElementById('login-btn')?.addEventListener('click', login);
document.getElementById('authorize-btn')?.addEventListener('click', authorize);
document.getElementById('password')?.addEventListener('input', checkPasswordMatch);
document.getElementById('confirmPassword')?.addEventListener('input', checkPasswordMatch);

async function login() {
    const username = document.getElementById('Email').value;
    const password = document.getElementById('password').value;

    const passwordIssue = document.getElementById('passwordIssue');

    try {
        const hashedPassword = await hashPassword(password);
        if (passwordIssue) {
            passwordIssue.style.display = 'none';
        }

        const response = await fetch(serverPath + 'api/auth/login', {
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
            console.log('Login failed!');
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
                    emailIssue.style.display = 'none';
                    passwordIssue.textContent = errorMessage;
                    passwordIssue.style.display = 'block';
                    return;
                }
            } else if (response.status === 401 && errorMessage.includes('не найден')) {
                const emailIssue = document.getElementById('emailIssue');
                if (emailIssue) {
                    passwordIssue.style.display = 'none';
                    emailIssue.textContent = errorMessage;
                    emailIssue.style.display = 'block';
                    return;
                }
            }

            redirectToError(response.status, errorMessage, response.statusText);
        }
    } catch (error) {
        console.error('Full error details:', error);
        console.error('Error name:', error.name);
        console.error('Error message:', error.message);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}

function checkPasswordMatch() {
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword')?.value;
    const match = document.getElementById('passwordMatch');
    const mismatch = document.getElementById('passwordMismatch');

    if (password === '' || confirmPassword === '') {
        match.style.display = 'none';
        mismatch.style.display = 'none';
        return;
    }

    if (match && mismatch && password === confirmPassword) {
        match.style.display = 'block';
        mismatch.style.display = 'none';
    } else if (match && mismatch) {
        match.style.display = 'none';
        mismatch.style.display = 'block';
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

        const response = await fetch(serverPath + 'api/auth/authorize', {
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

            let errorMessage = 'Ошибка авторизации';

            try {
                const errorData = await response.json();
                if (errorData && errorData.message) {
                    errorMessage = errorData.message;
                }
            } catch (e) {
                errorMessage = `Ошибка ${response.status}: ${response.statusText}`;
            }

            if (response.status === 401 && errorMessage.includes('уже существует')) {
                const emailIssue = document.getElementById('emailIssue');
                if (emailIssue) {
                    emailIssue.textContent = errorMessage;
                    emailIssue.style.display = 'block';
                    return;
                }
            }

            redirectToError(response.status, errorMessage, response.statusText);
        }
    } catch (error) {
        console.error('Full error details:', error);
        console.error('Error name:', error.name);
        console.error('Error message:', error.message);
        redirectToError('0', 'Ошибка соединения с сервером', 'Network Error');
    }
}