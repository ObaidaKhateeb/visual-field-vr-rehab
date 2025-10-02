using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class CalibrationUI : MonoBehaviour
{
   //UI other Elements 
   public InputField timeInput; //Duration of the game in minutes
   public InputField shapeDisplayDuration; //Duration of showing the shapes in seconds
   public InputField betweenShapesDuration; //Duration between sets in seconds 
   public Slider startingDistanceSlider; // starting distance of the shape from the focus point
   public Slider shapeSizeSlider; // Size of the shapes

   public Slider focusYSlider; // Focus Point position in Y-axis
   public Slider focusScaleSlider; // Focus Point size 
   public Dropdown focusShapeDropdown; //Focus Point Shape (0 = Circle, 1 = Cross)
   public Dropdown focusChangeDropdown; // Focus point changability (0 = Static, 1 = Fixed interval change, 2 = Random interval change)
   public Dropdown intervalSetsDropdown; // Number of sets for focus point fixed interval change
//    public Dropdown focuscolorChangeDropdown; // Color change on/off
//    public Dropdown focuscolorChoiceDropdown; // Which color to change to
//    public Dropdown focuscolorDurationDropdown; // Duration of change in seconds

   public InputField successRateInput; // Number of sets should answered True to count as success
   public InputField failRateInput; // Number of sets should answered False to count as failure

   public InputField chunkSizeInput; // Chunk size

   public List<Toggle> imageSetToggles; // ScrollView for image set selection

   public Button saveConfigButton; // Save configuration button
   public Button loadConfigButton; // Load configuration button
   public Button startButton; // Start button, save settings

   public GameObject uiPanel;

    void Start()
    {
        saveConfigButton.onClick.AddListener(SaveConfiguration);
        loadConfigButton.onClick.AddListener(LoadConfiguration);
        startButton.onClick.AddListener(SaveSettingsAndClose);
        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); });
        OnFocusChangeDropdownChanged();
        // focuscolorChangeDropdown.onValueChanged.AddListener(delegate { OnFocusColorChangeDropdownChanged(); });
        // OnFocusColorChangeDropdownChanged();
    }

    void SaveConfiguration()
    {
        VRSettings settings = new VRSettings();
        
        // Durations: game, set display, and between sets.
        if (float.TryParse(timeInput.text, out float minutes))
            settings.gameDuration = minutes * 60f;
        if (float.TryParse(betweenShapesDuration.text, out float betweenDuration))
            settings.betweenShapesDuration = betweenDuration;
        if (float.TryParse(shapeDisplayDuration.text, out float duration))
            settings.shapeDisplayDuration = duration;
            
        settings.startingDistance = startingDistanceSlider.value;
        settings.shapeScale = shapeSizeSlider.value * 0.005f;

        // Focus point settings
        settings.focusY = focusYSlider.value / 100f;
        settings.focusScale = focusScaleSlider.value / 100f;
        settings.focusShape = focusShapeDropdown.value;
        settings.focusChangeMode = focusChangeDropdown.value;
        settings.intervalSets = intervalSetsDropdown.value + 1;

        // Success/Fail rates and chunk size
        if (float.TryParse(successRateInput.text, out float successRate))
            settings.successRate = successRate;
        else
            settings.successRate = 80f;
            
        if (float.TryParse(failRateInput.text, out float failRate))
            settings.failRate = failRate;
        else
            settings.failRate = 20f;
            
        if (int.TryParse(chunkSizeInput.text, out int chunkSize))
            settings.chunkSize = chunkSize;
        else
            settings.chunkSize = 15;

        // Image sets selection
        for (int i = 0; i < imageSetToggles.Count; i++)
        {
            if (imageSetToggles[i].isOn)
                settings.imageSets.Add(i + 1);
        }

        // Save to a config file (different from the runtime vr_settings.json)
        string json = JsonUtility.ToJson(settings, true);
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_config.json");
        File.WriteAllText(path, json);
        
        Debug.Log("Configuration saved to: " + path);
    }

    void LoadConfiguration()
    {
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_config.json");
        
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            // Load durations
            timeInput.text = (settings.gameDuration / 60f).ToString();
            betweenShapesDuration.text = settings.betweenShapesDuration.ToString();
            shapeDisplayDuration.text = settings.shapeDisplayDuration.ToString();
            
            // Load sliders
            startingDistanceSlider.value = settings.startingDistance;
            shapeSizeSlider.value = settings.shapeScale / 0.005f;
            focusYSlider.value = settings.focusY * 100f;
            focusScaleSlider.value = settings.focusScale * 100f;
            
            // Load dropdowns
            focusShapeDropdown.value = settings.focusShape;
            focusChangeDropdown.value = settings.focusChangeMode;
            intervalSetsDropdown.value = settings.intervalSets - 1;
            
            // Load success/fail rates and chunk size
            successRateInput.text = settings.successRate.ToString();
            failRateInput.text = settings.failRate.ToString();
            chunkSizeInput.text = settings.chunkSize.ToString();
            
            // Load image sets toggles
            // First, uncheck all toggles
            for (int i = 0; i < imageSetToggles.Count; i++)
            {
                imageSetToggles[i].isOn = false;
            }
            // Then check the saved ones
            foreach (int setNumber in settings.imageSets)
            {
                if (setNumber >= 1 && setNumber <= imageSetToggles.Count)
                {
                    imageSetToggles[setNumber - 1].isOn = true;
                }
            }
            
            Debug.Log("Configuration loaded from: " + path);
        }
        else
        {
            Debug.Log("No configuration file found at: " + path);
        }
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
        
        settings.startingDistance = startingDistanceSlider.value;
        settings.shapeScale = shapeSizeSlider.value * 0.005f;


        //Focus point settings: location, size, shape, and change mode.
       settings.focusY = focusYSlider.value / 100f;
       settings.focusScale = focusScaleSlider.value / 100f;
       settings.focusShape = focusShapeDropdown.value;
        settings.focusChangeMode = focusChangeDropdown.value;
        settings.intervalSets = intervalSetsDropdown.value + 1;
        // settings.focuscolorChangeDropdown = focuscolorChangeDropdown.value == 1;
        // settings.focuscolorChoiceDropdown = focuscolorChoiceDropdown.value;
        // settings.focuscolorDurationDropdown = focuscolorDurationDropdown.value + 1;

        // Success and Fail definitions
        if (float.TryParse(successRateInput.text, out float successRate))
            settings.successRate = successRate;
        else
            settings.successRate = 80f;
            
        if (float.TryParse(failRateInput.text, out float failRate))
            settings.failRate = failRate;
        else
            settings.failRate = 20f;
            
        if (int.TryParse(chunkSizeInput.text, out int chunkSize))
            settings.chunkSize = chunkSize;
        else
            settings.chunkSize = 15;

        // Image set selection
        for (int i = 0; i < imageSetToggles.Count; i++)
        {
            if (imageSetToggles[i].isOn)
                settings.imageSets.Add(i + 1);
        }

        // Saving the settings
       string json = JsonUtility.ToJson(settings, true);
       string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_settings.json");
       File.WriteAllText(path, json);
       
       Application.Quit();
   }

    public void OnFocusChangeDropdownChanged()
    {
        intervalSetsDropdown.interactable = focusChangeDropdown.value == 1;
    }

    // public void OnFocusColorChangeDropdownChanged()
    // {
    //     focuscolorChoiceDropdown.interactable = focuscolorChangeDropdown.value == 1;
    //     focuscolorDurationDropdown.interactable = focuscolorChangeDropdown.value == 1;
    // }
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
    public float startingDistance = 1f;
    public float shapeScale = 0.05f;
    public float successRate = 80f;
    public float failRate = 20f;
    public int chunkSize = 15;

    public List<int> imageSets = new List<int>();
    // public bool focuscolorChangeDropdown;
    // public int focuscolorChoiceDropdown;
    // public int focuscolorDurationDropdown;
}