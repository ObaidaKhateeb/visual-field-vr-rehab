# Quick Start Guide - Vision VR Python GUI

## 🚀 Running the Application

### Windows
Double-click `run_gui.bat` or run in command prompt:
```cmd
python vr_calibration_gui.py
```

### Linux / macOS
Double-click `run_gui.sh` or run in terminal:
```bash
./run_gui.sh
# or
python3 vr_calibration_gui.py
```

## 📝 Basic Workflow

### 1. Configure Settings (First Tab)
- Set test duration (default: 8 minutes)
- Adjust shape display and interval times
- Configure distance and size parameters
- Set focus point properties
- Choose image sets (select at least one!)
- **Optional:** Save your configuration for later use

### 2. Enter User Details (Second Tab)
- Fill in user information
- User ID is **required**
- Select which eye is being trained
- Click "Start Session" when ready

### 3. View Results (Third Tab)
- Browse previous session results
- View detailed statistics
- Delete old results if needed

## 💾 Where is my data?

All data is saved in your home directory:

**Windows:** `C:\Users\YourName\VRUserData\`
**Linux/macOS:** `~/VRUserData/`

### Files Created:
- `vr_settings.json` - Current session settings (read by VR game)
- `user_details.csv` - User database
- `game_results.csv` - Session results (written by VR game)
- `Configs/` - Your saved configurations

## 🎮 Integration with VR Game

1. Configure and save settings in this GUI
2. Launch your Unity VR Game
3. The game reads settings from `~/VRUserData/vr_settings.json`
4. After session, results are saved to `~/VRUserData/game_results.csv`
5. Return to this GUI to view results

## ⚙️ System Requirements

- Python 3.7 or higher
- tkinter (included with Python)
- Any modern operating system (Windows/macOS/Linux)

## 🔧 Troubleshooting

### "tkinter not found" on Linux
```bash
# Ubuntu/Debian
sudo apt-get install python3-tk

# Fedora
sudo dnf install python3-tkinter
```

### "Permission denied" on Linux/macOS
```bash
chmod +x run_gui.sh vr_calibration_gui.py
```

### GUI window is too small/large
The GUI is resizable - just drag the window corners to adjust!

## 📚 More Information

- See `README.md` for detailed documentation
- See `CONVERSION_NOTES.md` for comparison with Unity version
- Check `requirements.txt` for dependencies (spoiler: none needed!)

## 💡 Tips

- **Save configurations** before starting sessions to quickly reload settings
- **Select multiple image sets** for variety in testing
- **Check the Results tab** after each VR session to track progress
- **The GUI remembers** your last used settings automatically

## ⌨️ Keyboard Shortcuts

- **Tab** - Navigate between fields
- **Enter** - Activate focused button
- **Ctrl+Tab** - Switch between tabs (Settings/User/Results)

## 🆘 Need Help?

If you encounter issues:
1. Check that Python 3.7+ is installed: `python --version`
2. Verify tkinter is available: `python -c "import tkinter"`
3. Check file permissions in ~/VRUserData/
4. Look for error messages in the console

## 📞 Support

For technical support or questions, refer to the main README.md file or contact your system administrator.

---

**Happy Testing! 🎯**
