using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CalibrationUI : MonoBehaviour
{
   //UI other Elements 
   public InputField timeInput; //Duration of the game in minutes
   public InputField shapeDisplayDuration; //Duration of showing the shapes in seconds
   public InputField betweenShapesDuration; //Duration between sets in seconds 

   public Slider focusYSlider; // Focus Point position in Y-axis
   public Slider focusScaleSlider; // Focus Point size 
   public Dropdown focusShapeDropdown; //Focus Point Shape (0 = Circle, 1 = Cross)
   public Dropdown focusChangeDropdown; // Focus point changability (0 = Static, 1 = Fixed interval change, 2 = Random interval change)
   public Dropdown intervalSetsDropdown; // Number of sets for focus point fixed interval change

   public Button startButton; // Start button, save settings

   public GameObject uiPanel;

    void Start()
    {
        startButton.onClick.AddListener(SaveSettingsAndClose);
        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); });
    }

   void SaveSettingsAndClose()
   {
       VRSettings settings = new VRSettings();
       
       // Durations: game, set display, and between sets.
       if (float.TryParse(timeInput.text, out float minutes))
           settings.gameDuration = minutes * 60f;
       if (float.TryParse(betweenShapesDuration.text, out float betweenDuration))
           settings.betweenShapesDuration = betweenDuration;
       if (float.TryParse(shapeDisplayDuration.text, out float duration))
           settings.shapeDisplayDuration = duration; 

        //Focus point settings: location, size, shape, and change mode.
       settings.focusY = focusYSlider.value / 100f;
       settings.focusScale = focusScaleSlider.value / 100f;
       settings.focusShape = focusShapeDropdown.value;
        settings.focusChangeMode = focusChangeDropdown.value;
        settings.intervalSets = intervalSetsDropdown.value + 1;

        // Saving the settings
       string json = JsonUtility.ToJson(settings, true);
       string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_settings.json");
       File.WriteAllText(path, json);
       
       Application.Quit();
   }

    public void OnFocusChangeDropdownChanged()
    {
        intervalSetsDropdown.gameObject.SetActive(focusChangeDropdown.value == 1);
    }
}

[System.Serializable]
public class VRSettings
{
   public float gameDuration;
   public float focusY;
   public float focusScale;
   public int focusShape;
   public float shapeDisplayDuration;
   public float betweenShapesDuration;
   public int focusChangeMode; 
   public int intervalSets;
}