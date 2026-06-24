@echo off
REM Luon chay tai dung thu muc chua file .bat nay.
REM Hai cua so con (start cmd) se ke thua thu muc nay -> khong can cd ben trong.
cd /d "%~dp0"

title PetCenter RASA - Khoi dong server
color 0A
echo ============================================
echo   PetCenter RASA - BUOC 3: KHOI DONG
echo ============================================
echo.

REM Kiem tra rasa.exe
if not exist "venv\Scripts\rasa.exe" (
    echo [LOI] Khong tim thay rasa.exe!
    echo Hay chay file 1_install.bat truoc.
    pause
    exit /b 1
)

REM Kiem tra model da train chua
if not exist "models\*.tar.gz" (
    echo [LOI] Chua co model! Hay chay file 2_train.bat truoc.
    pause
    exit /b 1
)

REM Hai cua so con ke thua thu muc nay; duong dan tuong doi venv\Scripts khong co dau cach -> an toan.
echo Mo cua so Action Server...
start "RASA Action Server" cmd /k "venv\Scripts\rasa.exe run actions"

echo Cho Action Server khoi dong (5 giay)...
timeout /t 5 /nobreak >nul

echo Mo cua so RASA Server...
start "RASA Server" cmd /k "venv\Scripts\rasa.exe run --enable-api --cors *"

echo.
echo ============================================
echo   DA MO 2 CUA SO RASA
echo   Cho den khi thay dong:
echo     "Rasa server is up and running"
echo   o cua so RASA Server thi vao website va chat!
echo ============================================
echo.
pause >nul
