@echo off
REM Luon chay tai dung thu muc chua file .bat nay (du double-click hay goi tu noi khac)
cd /d "%~dp0"

title PetCenter RASA - Cai dat lan dau
color 0A
echo ============================================
echo   PetCenter RASA - BUOC 1: CAI DAT
echo ============================================
echo.

REM ── Tim Python 3.10 (uu tien py launcher -3.10, roi den python neu dung 3.10) ──
set "PYCMD="
py -3.10 --version >nul 2>&1 && set "PYCMD=py -3.10"
if not defined PYCMD (
    python --version 2>&1 | findstr /c:"3.10" >nul && set "PYCMD=python"
)

if not defined PYCMD (
    echo [LOI] Khong tim thay Python 3.10!
    echo.
    echo RASA 3.6 CHI chay voi Python 3.10 (khong dung 3.11/3.12).
    echo Tai tai: https://www.python.org/downloads/release/python-31011/
    echo Khi cai nho TICK "Add python.exe to PATH".
    echo.
    echo Neu may ban da co Python 3.10, thu mo CMD go:  py -3.10 --version
    pause
    exit /b 1
)

echo [OK] Dung Python:
%PYCMD% --version
echo.

REM ── Neu venv ton tai nhung HONG (thieu python.exe) -> xoa tao lai ──
if exist "venv" (
    if not exist "venv\Scripts\python.exe" (
        echo [!] Phat hien venv hong, dang xoa de tao lai...
        rmdir /s /q venv
    )
)

REM ── Tao virtual environment ──
if not exist "venv\Scripts\python.exe" (
    echo [1/4] Dang tao virtual environment...
    %PYCMD% -m venv venv
    if not exist "venv\Scripts\python.exe" (
        echo [LOI] Khong the tao venv!
        echo Co the do Antivirus chan, hoac Python loi. Thu tat AV tam thoi va chay lai.
        pause
        exit /b 1
    )
    echo [OK] Da tao venv
) else (
    echo [OK] Virtual environment da ton tai
)
echo.

echo [2/4] Nang cap pip...
"venv\Scripts\python.exe" -m pip install --upgrade pip

echo.
echo [3/4] Cai dat cac thu vien can thiet...
"venv\Scripts\python.exe" -m pip install wheel setuptools

echo.
echo [4/4] Dang cai dat RASA (mat 5-15 phut, vui long doi)...
echo.
"venv\Scripts\python.exe" -m pip install rasa==3.6.21

echo.
REM ── Kiem tra ket qua ──
if exist "venv\Scripts\rasa.exe" (
    echo ============================================
    echo   [OK] CAI DAT THANH CONG!
    echo   Tiep theo: chay file  2_train.bat
    echo ============================================
) else (
    echo ============================================
    echo   [LOI] Khong tim thay rasa.exe sau khi cai!
    echo   Keo len tren xem dong loi mau do cua pip,
    echo   thuong do sai version Python hoac mang gian doan.
    echo   Thu chay lai file nay mot lan nua.
    echo ============================================
)
pause
