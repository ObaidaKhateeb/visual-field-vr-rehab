#!/usr/bin/env python3
"""
Verification script for Vision VR GUI installation
Checks if all requirements are met to run the GUI
"""

import sys
import os
from pathlib import Path

def check_python_version():
    """Check if Python version is 3.7+"""
    version = sys.version_info
    if version.major >= 3 and version.minor >= 7:
        print(f"✓ Python version: {version.major}.{version.minor}.{version.micro} (OK)")
        return True
    else:
        print(f"✗ Python version: {version.major}.{version.minor}.{version.micro} (Need 3.7+)")
        return False

def check_tkinter():
    """Check if tkinter is available"""
    try:
        import tkinter
        print(f"✓ tkinter module: Available (version {tkinter.TkVersion})")
        return True
    except ImportError:
        print("✗ tkinter module: Not found")
        print("  Install with:")
        print("    Ubuntu/Debian: sudo apt-get install python3-tk")
        print("    Fedora: sudo dnf install python3-tkinter")
        print("    macOS: Should be included with Python")
        print("    Windows: Should be included with Python")
        return False

def check_files():
    """Check if required files exist"""
    required_files = [
        'vr_calibration_gui.py',
        'README.md',
        'requirements.txt',
        'QUICK_START.md'
    ]
    
    all_good = True
    for filename in required_files:
        if Path(filename).exists():
            print(f"✓ File: {filename}")
        else:
            print(f"✗ File: {filename} (Missing)")
            all_good = False
    
    return all_good

def check_data_folder():
    """Check if VRUserData folder is accessible"""
    data_folder = Path.home() / "VRUserData"
    
    try:
        data_folder.mkdir(exist_ok=True)
        test_file = data_folder / ".test"
        test_file.write_text("test")
        test_file.unlink()
        print(f"✓ Data folder: {data_folder} (Writable)")
        return True
    except Exception as e:
        print(f"✗ Data folder: {data_folder} (Error: {e})")
        return False

def main():
    """Run all checks"""
    print("=" * 60)
    print("Vision VR GUI - Installation Verification")
    print("=" * 60)
    print()
    
    checks = [
        ("Python Version", check_python_version),
        ("Tkinter Module", check_tkinter),
        ("Required Files", check_files),
        ("Data Folder", check_data_folder),
    ]
    
    results = []
    for name, check_func in checks:
        print(f"\n{name}:")
        print("-" * 40)
        result = check_func()
        results.append(result)
    
    print("\n" + "=" * 60)
    if all(results):
        print("✓ All checks passed! You're ready to run the GUI.")
        print("\nRun the GUI with:")
        print("  python vr_calibration_gui.py")
        print("or")
        print("  ./run_gui.sh  (Linux/macOS)")
        print("  run_gui.bat   (Windows)")
        return 0
    else:
        print("✗ Some checks failed. Please fix the issues above.")
        return 1

if __name__ == "__main__":
    sys.exit(main())
