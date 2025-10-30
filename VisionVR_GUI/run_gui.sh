#!/bin/bash
# Quick start script for Vision VR GUI

echo "Starting Vision VR Calibration GUI..."

# Check if Python 3 is installed
if command -v python3 &> /dev/null; then
    python3 vr_calibration_gui.py
elif command -v python &> /dev/null; then
    python vr_calibration_gui.py
else
    echo "Error: Python 3 is not installed!"
    echo "Please install Python 3.7 or higher."
    exit 1
fi
