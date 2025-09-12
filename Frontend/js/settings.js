import { changeTheme, getProtectedData } from "./utils.js";

document.addEventListener('DOMContentLoaded', function () {
    loadSettings();
});

document.getElementById('setTheme-light').addEventListener('click', () => setTheme('light'));
document.getElementById('setTheme-dark').addEventListener('click', () => setTheme('dark'));
document.getElementById('bus').addEventListener('click', () => selectIcon('bus'));
document.getElementById('car').addEventListener('click', () => selectIcon('car'));
document.getElementById('truck').addEventListener('click', () => selectIcon('truck'));
document.getElementById('ship').addEventListener('click', () => selectIcon('ship'));
document.getElementById('bicycle').addEventListener('click', () => selectIcon('bicycle'));
document.getElementById('motorcycle').addEventListener('click', () => selectIcon('motorcycle'));
document.getElementById('train').addEventListener('click', () => selectIcon('train'));
document.getElementById('subway').addEventListener('click', () => selectIcon('subway'));
document.getElementById('delete-btn').addEventListener('click', showConfirmation);
document.getElementById('delete-profile-btn').addEventListener('click', deleteProfile);
document.getElementById('hide-btn').addEventListener('click', hideConfirmation);

function loadSettings() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    setTheme(savedTheme, true);

    const savedIcon = localStorage.getItem('transportIcon') || 'bus';
    selectIcon(savedIcon, true);
}

function setTheme(theme, initialLoad = false) {
    document.querySelectorAll('.theme-option').forEach(option => {
        option.classList.remove('active');
    });
    document.querySelector(`.theme-option[data-theme="${theme}"]`).classList.add('active');

    if (!initialLoad) {
        localStorage.setItem('theme', theme);
    }

    changeTheme(theme);
}

function selectIcon(icon, initialLoad = false) {
    document.querySelectorAll('.icon-option').forEach(option => {
        option.classList.remove('active');
    });
    document.querySelector(`.icon-option[data-icon="${icon}"]`).classList.add('active');

    if (!initialLoad) {
        localStorage.setItem('transportIcon', icon);
    }
}

function showConfirmation() {
    document.getElementById('deleteConfirmation').style.display = 'block';
}

function hideConfirmation() {
    document.getElementById('deleteConfirmation').style.display = 'none';
}

async function deleteProfile() {
    try {
        const response = await getProtectedData('auth', 'DELETE');

        if (response.ok) {
            alert('Ваш профиль был успешно удален.');
            localStorage.removeItem('jwtToken');
            window.location.href = 'index.html';
        } else {
            alert('Произошла ошибка при удалении профиля. Пожалуйста, попробуйте еще раз.');
        }
    } catch (error) {
        console.error('Ошибка при удалении профиля:', error);
        alert('Произошла ошибка при удалении профиля. Пожалуйста, попробуйте еще раз.');
    }
}