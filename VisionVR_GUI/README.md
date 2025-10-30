# Vision VR - Calibration GUI (Python Version)

A cross-platform configuration interface for the Vision VR Game, converted from Unity C# to Python with tkinter.

## Overview

This GUI application allows users to configure VR game settings, manage user profiles, and view session results. It replaces the Unity-based GUI with a standalone Python application that works on Windows, macOS, and Linux.

## Features

### 1. Settings Configuration
- **Duration Settings**
  - Test duration (minutes)
  - Shape display duration (milliseconds)
  - Duration between shapes (milliseconds)

- **Distance & Size Settings**
  - Starting distance (1-10)
  - Maximum distance (1-10)
  - Shape size

- **Focus Point Settings**
  - Y position
  - Scale
  - Shape (Circle/Cross)
  - Change mode (Static/Fixed Interval/Random Interval)
  - Interval sets (for fixed interval mode)

- **Performance Settings**
  - Success rate threshold (%)
  - Fail rate threshold (%)
  - Chunk size

- **Image Sets Selection**
  - Select from 10 available image sets

### 2. User Management
- User name, ID, age, and gender
- Birth date
- Training eye selection (Right/Left)
- Automatic tracking of training history

### 3. Configuration Management
- Save configurations with custom names
- Load previously saved configurations
- Delete configurations
- Configurations stored in JSON format

### 4. Results Viewing
- View all session results
- Filter and sort results
- View detailed session information
- Delete old results

## Requirements

- Python 3.7 or higher
- tkinter (included with Python standard library)

## Installation

1. **Ensure Python is installed:**
   ```bash
   python --version
   # or
   python3 --version
   ```

2. **No additional packages needed** - tkinter comes with Python!

3. **Make the script executable (Linux/macOS):**
   ```bash
   chmod +x vr_calibration_gui.py
   ```

## Usage

### Running the Application

**Windows:**
```bash
python vr_calibration_gui.py
```

**Linux/macOS:**
```bash
python3 vr_calibration_gui.py
```

Or directly (if executable):
```bash
./vr_calibration_gui.py
```

### Workflow

1. **Configure Settings (Tab 1)**
   - Set test duration and timing parameters
   - Adjust distance and size settings
   - Configure focus point behavior
   - Set performance thresholds
   - Select image sets to use
   - Save configuration for future use (optional)

2. **Enter User Details (Tab 2)**
   - Fill in user information
   - Select training eye
   - Click "Start Session" to save settings

3. **View Results (Tab 3)**
   - Browse previous session results
   - View detailed statistics
   - Delete old results

## Data Storage

All data is stored in the user's home directory:

```
~/VRUserData/
├── vr_settings.json          # Current session settings
├── user_details.csv          # User information database
├── game_results.csv          # Game session results
└── Configs/                  # Saved configurations
    ├── config1.json
    ├── config2.json
    └── ...
```

### File Formats

**vr_settings.json** - Current session configuration:
```json
{
  "gameDuration": 480.0,
  "focusY": 0.455,
  "focusScale": 0.381,
  "focusShape": 1,
  "shapeDisplayDuration": 1000.0,
  "betweenShapesDuration": 5000.0,
  "focusChangeMode": 0,
  "intervalSets": 1,
  "startingDistance": 1.0,
  "maxDistance": 10.0,
  "shapeScale": 0.05,
  "successRate": 80.0,
  "failRate": 20.0,
  "chunkSize": 15,
  "imageSets": [1, 2, 3],
  "userID": "123456789",
  "trainingEye": 0,
  "sessionTimestamp": "2025-10-30 12:34:56"
}
```

**user_details.csv** - User database:
```csv
ID,Name,Age,Gender,BirthYear,BirthMonth,BirthDay,EyeTrained,FirstAdded,LastUpdate
123456789,John Doe,30,Male,1995,5,15,Right,2025-10-30 12:34:56,2025-10-30 12:34:56
```

**game_results.csv** - Session results (created by the VR game):
```csv
UserID,Timestamp,EyeTrained,TestDuration,FocusY,FocusScale,FocusShape,...
```

## Differences from Unity Version

### Maintained Features
- All configuration options
- User details management
- Configuration save/load
- Results viewing
- Data compatibility with VR game

### UI Differences
- Tabbed interface instead of multiple panels
- Standard OS widgets instead of Unity UI
- Native file dialogs
- Cross-platform support

### Technical Improvements
- No Unity dependency
- Faster startup time
- Smaller footprint
- Easier to modify and maintain
- Cross-platform without rebuilds

## Integration with VR Game

The Python GUI generates the same `vr_settings.json` file that the Unity VR game expects. The workflow is:

1. Run the Python GUI
2. Configure settings and enter user details
3. Click "Start Session"
4. Launch the Unity VR game
5. Game reads settings from `~/VRUserData/vr_settings.json`
6. Game writes results to `~/VRUserData/game_results.csv`
7. Return to Python GUI to view results

## Troubleshooting

### tkinter not found (Linux)
Install tkinter for your distribution:
```bash
# Ubuntu/Debian
sudo apt-get install python3-tk

# Fedora
sudo dnf install python3-tkinter

# Arch
sudo pacman -S tk
```

### Permission denied (Linux/macOS)
Make the script executable:
```bash
chmod +x vr_calibration_gui.py
```

### GUI appears blank or frozen
- Ensure you're using Python 3.7+
- Try updating tkinter
- Check system display settings

## Development

### Code Structure

- `VRSettings` - Data class for settings
- `CalibrationGUI` - Main application class
  - `create_settings_tab()` - Configuration interface
  - `create_user_tab()` - User details interface
  - `create_results_tab()` - Results browser
  - `collect_settings()` - Get settings from UI
  - `apply_settings()` - Apply settings to UI
  - `save_and_start()` - Main save function

### Extending the GUI

To add new settings:

1. Add field to `VRSettings` class
2. Add UI widget in appropriate tab
3. Update `collect_settings()` to read value
4. Update `apply_settings()` to apply value
5. Update `to_dict()` and `from_dict()` for JSON

## License

This is a conversion of the Unity VR GUI project to Python. Refer to the original project license.

## Credits

- Original Unity version: Vision VR Game project
- Python conversion: Automated conversion maintaining all functionality
- UI Framework: tkinter (Python standard library)

## Support

For issues or questions:
1. Check the troubleshooting section
2. Verify Python version (3.7+)
3. Ensure VRUserData folder has write permissions
4. Check console output for error messages
