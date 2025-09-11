document.addEventListener('DOMContentLoaded', function () {
    getStats();
});

async function getStats() {
    const taransport = await getProtectedData('transport/getFirstTransport', 'GET');

    if (taransport == null || taransport == undefined)
    {
        setPosition({ x: '-', y: '-' });
        setSpeed('?');
    }
    else
    {
        setPosition(taransport);
        setSpeed(taransport.speed);
    }
}

function setPosition(position) {
    const text = document.getElementById('currentPos');
    text.setAttribute('style', 'text-align: center;');
    text.innerHTML = `Ваш транспорт находится в районе<p style="font-size: large;">${String(position.x).substring(0, 4)} : ${String(position.y).substring(0, 4)}</p>`;
}

function setSpeed(speed) {
    const text = document.getElementById('currentSpeed');
    text.setAttribute('style', 'text-align: center;');
    text.innerHTML = `Текущая сорость вашего транспорта<p style="font-size: large;">${speed} км/ч</p>`
}