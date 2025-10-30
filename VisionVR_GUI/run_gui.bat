@echo off
REM Quick start script for Vision VR GUI (Windows)

echo Starting Vision VR Calibration GUI...

REM Check if Python is installed
where python >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    python vr_calibration_gui.py
) else (
    echo Error: Python is not installed!
    echo Please install Python 3.7 or higher from python.org
    pause
    exit /b 1
)
