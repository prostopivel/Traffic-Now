@echo off
setlocal enabledelayedexpansion

echo === PASSWORD HASHING ===
powershell -Command "$password = 'admin1234'; $hasher = [System.Security.Cryptography.SHA256]::Create(); $bytes = [System.Text.Encoding]::UTF8.GetBytes($password); $hash = $hasher.ComputeHash($bytes); $result = ''; foreach ($b in $hash) { $result += $b.ToString('x2'); } $result | Out-File -FilePath 'temp_hash.txt' -Encoding ASCII"

set /p HASHED_PASSWORD=<temp_hash.txt
del temp_hash.txt

echo Hashed password: %HASHED_PASSWORD%
echo.

echo === LOGIN ===
curl -X POST "https://localhost:7003/api/auth/login" ^
     -H "Content-Type: application/json" ^
     -d "{\"Username\": \"admin@gmail.com\", \"Password\": \"%HASHED_PASSWORD%\"}" -s -o login_response.txt -w "HTTP Status: %%{http_code}\n"
     
if errorlevel 1 (
    echo Error while logging in!
    type login_response.txt
    del login_response.txt
    pause
    exit /b 1
)

:: Извлекаем токен без лишних символов
set TOKEN=
for /f "tokens=2 delims=:" %%a in ('type login_response.txt ^| findstr /i "\"token\""') do (
    set "TOKEN=%%a"
    set "TOKEN=!TOKEN:"=!"
    set "TOKEN=!TOKEN:,=!"
    set "TOKEN=!TOKEN:}=!"
    set "TOKEN=!TOKEN: =!"
)

del login_response.txt

if "!TOKEN!"=="" (
    echo ERROR: Could not extract token!
    pause
    exit /b 1
)

echo Using token: !TOKEN!
echo.

:: Запрашиваем имя карты
set /p MAP_NAME="Enter map name: "
if "!MAP_NAME!"=="" set "MAP_NAME=Default"

echo.
echo === CREATING A MAP: !MAP_NAME! ===

curl -X POST "https://localhost:7003/api/map/create" ^
     -H "Content-Type: application/json" ^
     -H "Authorization: Bearer !TOKEN!" ^
     -d "\"!MAP_NAME!\"" -s -o map_response.txt -w "HTTP Status: %%{http_code}\n"

if errorlevel 1 (
    echo Error creating map!
    echo Response:
    type map_response.txt
) else (
    echo Map created successfully!
    type map_response.txt
)

del map_response.txt

echo.
echo Done! Press any key to exit...
pause >nul