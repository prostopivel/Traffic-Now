async function hashPassword(password) {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);
    
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    
    return hashHex;
}

async function login() {
    const username = document.getElementById('Email').value;
    const password = document.getElementById('password').value;

    const hashedPassword = await hashPassword(password);

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
        getProtectedData('menu', 'GET');
    } else {
        console.log('Login failed!');

        if ([400, 401, 403, 404, 500].includes(response.status)) {
            throw new Error(`HTTP ${response.status}`);
        }
        
        throw new Error(`HTTP error! status: ${response.status}`);
    }
}

async function authorize() {
    const username = document.getElementById('Email').value;
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (password != confirmPassword)
    {
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
        getProtectedData('menu', 'GET');
    } else {
        console.log('Authorize failed!');

        if ([400, 401, 403, 404, 500].includes(response.status)) {
            throw new Error(`HTTP ${response.status}`);
        }
        
        throw new Error(`HTTP error! status: ${response.status}`);
    }
}

async function getProtectedData(path, method) {
    const token = localStorage.getItem('jwtToken');

    const response = await fetch('https://localhost:7003/api/' + path, {
        method: method,
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    if (response.ok) {
        const data = await response.json();
        console.log('Data received successfully!');

        return data;
    } else {
        console.log('Error retrieving data!');

        if ([400, 401, 403, 404, 500].includes(response.status)) {
            throw new Error(`HTTP ${response.status}`);
        }
        
        throw new Error(`HTTP error! status: ${response.status}`);
    }
}