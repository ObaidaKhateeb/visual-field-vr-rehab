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
   public Text startingDistanceValueText; // Display value of startingDistanceSlider
   public Slider shapeSizeSlider; // Size of the shapes
   public Text shapeSizeValueText; // Display value of shapeSizeSlider

   public Slider focusYSlider; // Focus Point position in Y-axis
   public Text focusYValueText; // Display value of focusYSlider
   public Slider focusScaleSlider; // Focus Point size 
   public Text focusScaleValueText; // Display value of focusScaleSlider
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
   public Button continueButton; // Start button, save settings

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

   //Message dialog
   public GameObject MessageDialogPanel;
   public Text MessageText;
   public Button MessageOkButton;

   public GameObject uiPanel;

    //User details panel
    public GameObject infoPannel;
    public InputField NameInput;
    public InputField IDInput;
    public InputField AgeInput;
    public Dropdown GenderDropdown;
    public Dropdown DateYearDropDown;
    public Dropdown DateMonthDropDown;
    public Dropdown DateDayDropDown;
    public Dropdown EyeDropDown; // Right eye = 0, Left eye = 1
    public Button StartButton;

    // Results popup
    public GameObject ResultsPanel;
    public Text resultsUserIDText;
    public Text resultsTimestampText;
    public Text resultsEyeText;
    public Text resultsAccuracyText;
    public Text resultsAvgResponseTimeText;
    public Text resultsTrialsText;
    public Text resultsCorrectResponsesText;
    public Button resultsCloseButton;

    void Start()
    {
        //sliders text values 
        startingDistanceSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        shapeSizeSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        focusYSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        focusScaleSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        UpdateSliderValueDisplay(); //initial display

        saveConfigButton.onClick.AddListener(ShowSaveDialog);
        loadConfigButton.onClick.AddListener(ShowLoadDialog);
        continueButton.onClick.AddListener(ShowInfoPanel);

        //save and load dialogs buttons
        saveDialogSaveButton.onClick.AddListener(SaveConfigurationWithName);
        saveDialogCancelButton.onClick.AddListener(HideSaveDialog);
        loadDialogDeleteButton.onClick.AddListener(DeleteSelectedConfiguration);
        loadDialogLoadButton.onClick.AddListener(LoadSelectedConfiguration);
        loadDialogCancelButton.onClick.AddListener(HideLoadDialog);

        //Message dialog
        MessageOkButton.onClick.AddListener(() => MessageDialogPanel.SetActive(false));

        //Start button
        StartButton.onClick.AddListener(SaveSettingsAndClose);

        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); });
        OnFocusChangeDropdownChanged();
        // focuscolorChangeDropdown.onValueChanged.AddListener(delegate { OnFocusColorChangeDropdownChanged(); });
        // OnFocusColorChangeDropdownChanged();

        CheckAndDisplaySessionResults();
    }

    void UpdateSliderValueDisplay()
    {
        if (startingDistanceValueText != null)
            startingDistanceValueText.text = startingDistanceSlider.value.ToString();
        
        if (shapeSizeValueText != null)
            shapeSizeValueText.text = shapeSizeSlider.value.ToString();
            
        if (focusYValueText != null)
            focusYValueText.text = focusYSlider.value.ToString();

        if (focusScaleValueText != null)
            focusScaleValueText.text = focusScaleSlider.value.ToString();
    }

    //A function that shows the save configuration dialog
    void ShowSaveDialog()
    {
        saveConfigNameInput.text = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); //cfg name by default is the date
        saveDialogPanel.SetActive(true);

        //select all the text 
        saveConfigNameInput.Select();
        saveConfigNameInput.ActivateInputField();
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
            showMessage(".תורדגהה טסל םש רוחבל אנ");
            return;
        }
        
        VRSettings settings = new VRSettings();
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
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
        
        showMessage(".הלחצהב רמשנ תורדגהה טס");
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
            showMessage(".הרתוא אל תורדגהה תייקית");
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
                    if (selectedConfigToLoad == capturedName)
                    {
                        selectedConfigToLoad = "";
                    }
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
            showMessage(".הניעטל תורדגה טס רוחבל אנ");
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
            
            showMessage(".ןעטנ תורדגהה טס");
            HideLoadDialog();
        }
        else
        {
            showMessage(".אצמנ אל תורדגהה ץבוק");
        }
    }

    void DeleteSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(selectedConfigToLoad))
        {
            showMessage(".הקיחמל תורדגה טס רוחבל אנ");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRConfigs");
        string path = Path.Combine(configFolder, selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            // Delete from disk
            File.Delete(path);
            showMessage(".קחמנ תורדגהה טס");
            
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
            showMessage(".אצמנ אל תורדגהה ץבוק");
        }
    }

   void SaveSettingsAndClose()
   {
        //User details validation
        string userName = NameInput.text.Trim();
        string userID = IDInput.text.Trim();
        string userAge = AgeInput.text.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            showMessage(".לופטמה םש תא ןיזהל אנ");
            return;
        }

        if (string.IsNullOrEmpty(userID))
        {
            showMessage(".תוהזה תדועת רפסמ תא ןיזהל אנ");
            return;
        }

        if (string.IsNullOrEmpty(userAge) || !int.TryParse(userAge, out int age))
        {
            showMessage(".ןיקת ליג ןיזהל אנ");
            return;
        }

       VRSettings settings = new VRSettings();
       string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
       
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

        // User details
        settings.userID = userID;
        settings.trainingEye = EyeDropDown.value; // 0 = Right, 1 = Left
        settings.sessionTimestamp = timestamp;

        // Saving the settings
       string json = JsonUtility.ToJson(settings, true);
       string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_settings.json");
       File.WriteAllText(path, json);
       
       SaveUserDetailsToCSV(userName, userID, age, GenderDropdown.value, DateYearDropDown.value, DateMonthDropDown.value, DateDayDropDown.value, EyeDropDown.value, timestamp);
       Application.Quit();
   }

    public void OnFocusChangeDropdownChanged()
    {
        intervalSetsDropdown.interactable = focusChangeDropdown.value == 1;
    }

    void ShowInfoPanel()
    {
        //Check if at least one image set is selected
        bool anySelected = false;
        for (int i = 0; i < imageSetToggles.Count; i++)
        {
            if (imageSetToggles[i].isOn)
            {
                anySelected = true;
                break;
            }
        }

        if (!anySelected)
        {
            showMessage(".דחא לפל רוחבל שי - תונומת יטס");
            return;
        }

        //Hide main panel and show info panel
        uiPanel.SetActive(false);
        infoPannel.SetActive(true);
    }

    // public void OnFocusColorChangeDropdownChanged()
    // {
    //     focuscolorChoiceDropdown.interactable = focuscolorChangeDropdown.value == 1;
    //     focuscolorDurationDropdown.interactable = focuscolorChangeDropdown.value == 1;
    // }

    void showMessage(string message)
    {
        MessageText.text = message;
        MessageDialogPanel.SetActive(true);
    }

    void SaveUserDetailsToCSV(string userName, string userID, int userAge, int userGender, int birthYear, int birthMonth, int birthDay, int trainingEye, string timestamp)
    {
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        
        if (!Directory.Exists(csvFolder))
            Directory.CreateDirectory(csvFolder);
        
        string csvPath = Path.Combine(csvFolder, "user_details.csv");
        
        bool fileExists = File.Exists(csvPath);

        if (fileExists)
        {
            string[] existingLines = File.ReadAllLines(csvPath);
            
            for (int i = 0; i < existingLines.Length; i++)
            {
                if (existingLines[i].StartsWith(userID + ","))
                {
                    string[] fields = existingLines[i].Split(',');
                    if (fields.Length >= 8)
                    {
                        //Updating EyeTrained
                        string previousEyeText = fields[7]; 
                        string currentEyeText = trainingEye == 0 ? "Right" : "Left";                        
                        string newEyeText;
                        if (previousEyeText == "Both" || previousEyeText == currentEyeText)
                        {
                            newEyeText = previousEyeText; 
                        }
                        else
                        {
                            newEyeText = "Both"; 
                        }
                        
                        //Updating the other details
                        string genderText = userGender == 0 ? "Male" : "Female";
                        string firstAdded = fields[8]; 
                        string lastUpdate = timestamp;
                        existingLines[i] = $"{userID},{userName},{userAge},{genderText},{birthYear},{birthMonth},{birthDay},{newEyeText},{firstAdded},{lastUpdate}";
                        
                        File.WriteAllLines(csvPath, existingLines);
                    }
                    
                    return;
                }
            }
        }
        
        using (StreamWriter writer = new StreamWriter(csvPath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("ID,Name,Age,Gender,BirthYear,BirthMonth,BirthDay,EyeTrained,FirstAdded,LastUpdate");
            }
            string genderText = userGender == 0 ? "Male" : "Female";
            string eyeText = trainingEye == 0 ? "Right" : "Left";
            string currentTime = timestamp;

            writer.WriteLine($"{userID},{userName},{userAge},{genderText},{birthYear},{birthMonth},{birthDay},{eyeText},{currentTime},{currentTime}");
        }
    }

    //A method to check for session results and display them
    void CheckAndDisplaySessionResults()
    {
        string resultsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "current_session_results.json");
        
        if (!File.Exists(resultsPath))
            return;
        
        string json = File.ReadAllText(resultsPath);
        SessionResults results = JsonUtility.FromJson<SessionResults>(json);
        
        if (ResultsPanel == null)
        {
            Debug.LogWarning("Results popup panel not assigned");
            File.Delete(resultsPath);
            return;
        }
        
        // Populate results
        if (resultsUserIDText != null)
            resultsUserIDText.text = ":ז.ת" + results.userID;
        
        if (resultsTimestampText != null)
            resultsTimestampText.text = ":הקידבה ןמז" + results.sessionTimestamp;
        
        if (resultsEyeText != null)
            resultsEyeText.text = ":תנמואמ ןיע" + results.eyeTrained;
        
        if (resultsAccuracyText != null)
            resultsAccuracyText.text = ":קויד זוחא" + results.overallAccuracy.ToString("F1") + "%";
        
        if (resultsAvgResponseTimeText != null)
            resultsAvgResponseTimeText.text = ":עצוממ הבוגת ןמז" + results.overallAvgResponseTime.ToString("F2") + "s";
        
        if (resultsTrialsText != null)
            resultsTrialsText.text = ":םיטס כהס" + results.totalTrials;
        
        if (resultsCorrectResponsesText != null)
            resultsCorrectResponsesText.text = ":תונוכנ תובוגת כהס" + results.correctResponses;
        
        // Setup close button
        if (resultsCloseButton != null)
            resultsCloseButton.onClick.AddListener(() => ResultsPanel.SetActive(false));
        
        // Show the popup
        ResultsPanel.SetActive(true);
        
        // Delete the results file after displaying
        File.Delete(resultsPath);
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
    public float startingDistance = 1f;
    public float shapeScale = 0.05f;
    public float successRate = 80f;
    public float failRate = 20f;
    public int chunkSize = 15;

    public List<int> imageSets = new List<int>();
    // public bool focuscolorChangeDropdown;
    // public int focuscolorChoiceDropdown;
    // public int focuscolorDurationDropdown;

    //User details
    public string userID;
    public int trainingEye; // 0 = Right eye, 1 = Left eye
    public string sessionTimestamp;
}

[System.Serializable]
public class SessionResults
{
    public string userID;
    public string sessionTimestamp;
    public string eyeTrained;
    public float overallAccuracy;
    public float overallAvgResponseTime;
    public int totalTrials;
    public int correctResponses;
}