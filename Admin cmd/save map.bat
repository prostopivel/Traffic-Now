@echo off
setlocal enabledelayedexpansion

echo === PASSWORD HASHING ===
powershell -Command "$password = 'admin1234'; $hasher = [System.Security.Cryptography.SHA256]::Create(); $bytes = [System.Text.Encoding]::UTF8.GetBytes($password); $hash = $hasher.ComputeHash($bytes); $result = ''; foreach ($b in $hash) { $result += $b.ToString('x2'); } $result | Out-File -FilePath 'temp_hash.txt' -Encoding ASCII"

set /p HASHED_PASSWORD=<temp_hash.txt
del temp_hash.txt

echo Hashed password: %HASHED_PASSWORD%
echo.

echo === LOGIN ===
curl -X POST "https://traffic-now.onrender.com/api/auth/login" ^
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

:: Запрашиваем ID карты
set /p MAP_ID="Enter Map ID: "
if "!MAP_ID!"=="" (
    echo Map ID cannot be empty!
    pause
    exit /b 1
)

echo.
echo === GETTING MAP INFO ===

:: Получаем информацию о карте используя query parameter
curl -X GET "https://traffic-now.onrender.com/api/map?id=!MAP_ID!" ^
     -H "Authorization: Bearer !TOKEN!" ^
     -s -o map_info.txt -w "HTTP Status: %%{http_code}\n"

if errorlevel 1 (
    echo Error getting map info!
    type map_info.txt
    del map_info.txt
    pause
    exit /b 1
)

:: Правильное извлечение имени карты из JSON
set MAP_NAME=
for /f "delims=" %%i in ('powershell -Command "(Get-Content map_info.txt -Raw) | ConvertFrom-Json | Select-Object -ExpandProperty name"') do (
    set "MAP_NAME=%%i"
)

:: Если не удалось извлечь через PowerShell, пробуем альтернативный способ
if "!MAP_NAME!"=="" (
    for /f "tokens=2 delims=:," %%a in ('type map_info.txt ^| findstr /i "\"name\""') do (
        set "MAP_NAME=%%a"
        set "MAP_NAME=!MAP_NAME:"=!"
        set "MAP_NAME=!MAP_NAME: =!"
    )
)

del map_info.txt

if "!MAP_NAME!"=="" (
    echo ERROR: Could not extract map name!
    set "MAP_NAME=Map_!MAP_ID!"
    echo Using default name: !MAP_NAME!
)

echo Map name: !MAP_NAME!

:: Получаем путь к папке где находится bat-файл
set "BAT_DIR=%~dp0"
set "BAT_DIR=!BAT_DIR:~0,-1!"  :: Убираем последний обратный слеш

:: Формируем путь для сохранения
set "FILE_PATH=!BAT_DIR!\map_!MAP_NAME!.json"

echo.
echo === SAVING MAP TO JSON ===
echo Saving to: !FILE_PATH!
echo Map ID: !MAP_ID!

:: Кодируем путь для URL
powershell -Command "[System.Uri]::EscapeDataString('!FILE_PATH!')" > temp_encoded.txt
set /p ENCODED_PATH=<temp_encoded.txt
del temp_encoded.txt

:: Отправляем запрос на сохранение
curl -X POST "https://traffic-now.onrender.com/api/map/saveMap?path=!ENCODED_PATH!&mapId=!MAP_ID!" ^
     -H "Authorization: Bearer !TOKEN!" ^
     -s -o save_response.txt -w "HTTP Status: %%{http_code}\n"

if errorlevel 1 (
    echo Error saving map to JSON!
    echo Response:
    type save_response.txt
) else (
    echo Map saved successfully!
    echo File: !FILE_PATH!
    type save_response.txt
)

del save_response.txt

echo.
echo Done! Press any key to exit...
pause >nul