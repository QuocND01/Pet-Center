@echo off
REM Luon chay tai dung thu muc chua file .bat nay
cd /d "%~dp0"

title PetCenter RASA - Train model
color 0A
echo ============================================
echo   PetCenter RASA - BUOC 2: TRAIN MODEL
echo ============================================
echo.

REM Kiem tra rasa.exe ton tai chua
if not exist "venv\Scripts\rasa.exe" (
    echo [LOI] Khong tim thay rasa.exe trong venv!
    echo Hay chay file 1_install.bat de cai dat truoc.
    pause
    exit /b 1
)

echo Dang train model RASA, lan dau mat 3-5 phut...
echo.
"venv\Scripts\rasa.exe" train

echo.
echo ============================================
echo   TRAIN XONG.
echo   - Neu o tren thay dong "Your Rasa model is
echo     trained and saved" thi la THANH CONG.
echo   - Neu thay chu mau do thi keo len doc loi.
echo   Tiep theo: chay file  3_start.bat
echo ============================================
echo.
pause
