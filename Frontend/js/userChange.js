document.addEventListener('DOMContentLoaded', function () {
    loadUserData();

    document.getElementById('passwordChangeForm').addEventListener('submit', function (e) {
        e.preventDefault();
        changePassword();
    });

    document.getElementById('currentPassword').addEventListener('input', function () {
        hideIssue('currentPasswordIssue');
    });

    document.getElementById('newPassword').addEventListener('input', function () {
        hideIssue('newPasswordIssue');
        hideIssue('confirmPasswordIssue');
    });

    document.getElementById('confirmPassword').addEventListener('input', function () {
        hideIssue('confirmPasswordIssue');
    });
});

async function loadUserData() {
    const user = await getProtectedData('auth', 'GET');

    document.getElementById('userEmail').textContent = user.userEmail;
    document.getElementById('username').textContent = user.userEmail.split('@')[0];
}

async function changePassword() {
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    hideAllIssues();

    let isValid = true;

    if (newPassword !== confirmPassword) {
        showIssue('confirmPasswordIssue', 'Пароли не совпадают');
        isValid = false;
    }

    if (newPassword.length < 6) {
        showIssue('newPasswordIssue', 'Пароль должен содержать не менее 6 символов');
        isValid = false;
    }

    if (!isValid)
        return;

    const hashedCurrentPassword = await hashPassword(currentPassword);
    const hashedNewPassword = await hashPassword(newPassword);

    const response = await getBodyProtectedData('auth', 'PUT', {
        OldPassword: hashedCurrentPassword,
        NewPassword: hashedNewPassword
    });

    if (response.ok) {
        showSuccess('Пароль успешно изменен!');
        document.getElementById('passwordChangeForm').reset();
    } else {
        let errorMessage = 'Ошибка при изменении пароля';

        try {
            const errorData = await response.json();
            if (errorData && errorData.message) {
                errorMessage = errorData.message;
            }
        } catch (e) {
            errorMessage = `Ошибка ${response.status}: ${response.statusText}`;
        }

        if (response.status === 401 && errorMessage.includes('текущий пароль')) {
            showIssue('currentPasswordIssue', 'Неверный текущий пароль');
        } else {
            alert(errorMessage);
        }
    }
}

function togglePassword(inputId, icon) {
    const passwordInput = document.getElementById(inputId);
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

function showIssue(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.style.display = 'block';
    }
}

function hideIssue(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = 'none';
    }
}

function hideAllIssues() {
    hideIssue('currentPasswordIssue');
    hideIssue('newPasswordIssue');
    hideIssue('confirmPasswordIssue');
}

function showSuccess(message) {
    const notification = document.getElementById('successNotification');
    const messageElement = document.getElementById('successMessage');

    if (notification && messageElement) {
        messageElement.textContent = message;
        notification.style.display = 'flex';

        setTimeout(() => {
            notification.style.display = 'none';
        }, 5000);
    }
}