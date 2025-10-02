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

   //Save and load Dialogs
   public GameObject saveDialogPanel;
   public InputField saveConfigNameInput;
   public Button saveDialogSaveButton;
   public Button saveDialogCancelButton;
   public GameObject loadDialogPanel;
   public Transform loadDialogContent; // The Content of the ScrollView
   public Button loadDialogLoadButton;
   public Button loadDialogDeleteButton;
   public Button loadDialogCancelButton;
   public GameObject configButtonPrefab; // a button prefab
   private string selectedConfigToLoad = "";

   public GameObject uiPanel;

    void Start()
    {
        saveConfigButton.onClick.AddListener(ShowSaveDialog);
        loadConfigButton.onClick.AddListener(ShowLoadDialog);
        startButton.onClick.AddListener(SaveSettingsAndClose);

        //save and load dialogs buttons
        saveDialogSaveButton.onClick.AddListener(SaveConfigurationWithName);
        saveDialogCancelButton.onClick.AddListener(HideSaveDialog);
        loadDialogDeleteButton.onClick.AddListener(DeleteSelectedConfiguration);
        loadDialogLoadButton.onClick.AddListener(LoadSelectedConfiguration);
        loadDialogCancelButton.onClick.AddListener(HideLoadDialog);

        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); });
        OnFocusChangeDropdownChanged();
        // focuscolorChangeDropdown.onValueChanged.AddListener(delegate { OnFocusColorChangeDropdownChanged(); });
        // OnFocusColorChangeDropdownChanged();
    }

    //A function that shows the save configuration dialog
    void ShowSaveDialog()
    {
        saveConfigNameInput.text = "";
        saveDialogPanel.SetActive(true);
    }

    //A function that hides the save configuration dialog
    void HideSaveDialog()
    {
        saveDialogPanel.SetActive(false);
    }

    //A function responsible for saving the configuration
    void SaveConfigurationWithName()
    {
        string configName = saveConfigNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogWarning("Please enter a configuration name");
            return;
        }
        
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

        // Save to config folder
        string json = JsonUtility.ToJson(settings, true);
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRConfigs");
        
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);
        
        string filename = configName + ".json";
        string path = Path.Combine(configFolder, filename);
        File.WriteAllText(path, json);
        
        Debug.Log("Configuration saved to: " + path);
        HideSaveDialog();
    }

    //A function that shows the load dialog with the available configurations to load
    void ShowLoadDialog()
    {
        // Clear previous buttons
        foreach (Transform child in loadDialogContent)
        {
            Destroy(child.gameObject);
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRConfigs");
        
        if (!Directory.Exists(configFolder))
        {
            Debug.Log("No configurations folder found");
            return;
        }
        
        string[] configFiles = Directory.GetFiles(configFolder, "*.json");
        
        if (configFiles.Length == 0)
        {
            Debug.Log("No saved configurations found");
            return;
        }
        
        // Create a button for each config file
        int index = 0;
        foreach (string filePath in configFiles)
        {
            string configName = Path.GetFileNameWithoutExtension(filePath);
            
            GameObject toggleObj = new GameObject(configName);
            toggleObj.transform.SetParent(loadDialogContent, false);
            
            RectTransform toggleRt = toggleObj.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0, 1);
            toggleRt.anchorMax = new Vector2(1, 1);
            toggleRt.pivot = new Vector2(0.5f, 1);
            toggleRt.sizeDelta = new Vector2(0, 30);
            toggleRt.anchoredPosition = new Vector2(0, -index * 35);
            
            Toggle toggle = toggleObj.AddComponent<Toggle>();
            
            Image bgImage = toggleObj.AddComponent<Image>();
            bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            
            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;
            labelRt.offsetMin = new Vector2(10, 0);
            labelRt.offsetMax = new Vector2(-10, 0);
            
            Text label = labelObj.AddComponent<Text>();
            label.text = configName;
            label.color = Color.black;
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleLeft;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            string capturedName = configName;
            toggle.onValueChanged.AddListener((isOn) => {
                if (isOn) 
                {
                    SelectConfig(capturedName);
                }
                else
                {
                    // When unchecked, return to gray
                    bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
            });
            
            index++;
        }
        
        loadDialogPanel.SetActive(true);
        selectedConfigToLoad = "";
    }

    void SelectConfig(string configName)
    {
        selectedConfigToLoad = configName;
        Debug.Log("Configuration Selected: " + configName);
        
        // Highlight selected and unhighlight others
        foreach (Transform child in loadDialogContent)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            Image bgImage = child.GetComponent<Image>();
            
            if (toggle != null && bgImage != null)
            {
                if (child.name == configName)
                {
                    // Highlight selected
                    bgImage.color = new Color(0.3f, 0.6f, 1f, 1f); // Blue highlight
                    toggle.isOn = true;
                }
                else
                {
                    // Unhighlight others
                    bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Gray
                    toggle.isOn = false;
                }
            }
        }
    }

    void HideLoadDialog()
    {
        loadDialogPanel.SetActive(false);
    }

    void LoadSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(selectedConfigToLoad))
        {
            Debug.LogWarning("Please select a configuration to load");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRConfigs");
        string path = Path.Combine(configFolder, selectedConfigToLoad + ".json");
        
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
            for (int i = 0; i < imageSetToggles.Count; i++)
            {
                imageSetToggles[i].isOn = false;
            }
            foreach (int setNumber in settings.imageSets)
            {
                if (setNumber >= 1 && setNumber <= imageSetToggles.Count)
                {
                    imageSetToggles[setNumber - 1].isOn = true;
                }
            }
            
            Debug.Log("Configuration loaded: " + selectedConfigToLoad);
            HideLoadDialog();
        }
        else
        {
            Debug.LogWarning("Configuration file not found: " + path);
        }
    }

    void DeleteSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(selectedConfigToLoad))
        {
            Debug.LogWarning("Please select a configuration to delete");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRConfigs");
        string path = Path.Combine(configFolder, selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            // Delete from disk
            File.Delete(path);
            Debug.Log("Configuration deleted: " + selectedConfigToLoad);
            
            // Remove from GUI immediately
            foreach (Transform child in loadDialogContent)
            {
                if (child.name == selectedConfigToLoad)
                {
                    DestroyImmediate(child.gameObject);
                    break;
                }
            }
            
            // Reposition remaining toggles
            int index = 0;
            foreach (Transform child in loadDialogContent)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -index * 35);
                    index++;
                }
            }
            
            // Clear selection
            selectedConfigToLoad = "";
        }
        else
        {
            Debug.LogWarning("Configuration file not found: " + path);
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