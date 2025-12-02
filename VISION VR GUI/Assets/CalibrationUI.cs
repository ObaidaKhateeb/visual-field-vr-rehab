

using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CalibrationUI : MonoBehaviour
{
   //Main UI Panel Elements
   public GameObject uiPanel;
   public InputField timeInput; //Duration of the game in minutes
   public InputField shapeDisplayDuration; //Duration of showing the shapes in seconds
   public InputField betweenShapesDuration; //Duration between sets in seconds 
   public Slider startingDistanceSlider; //starting distance of the shape from the focus point
   public Text startingDistanceValueText; //Display value of startingDistanceSlider
   public Slider maxDistanceSlider; //maximum distance can be reached in the game
   public Text maxDistanceValueText; //Display value of maxDistanceSlider
   public Slider shapeSizeSlider; //Size of the shapes
   public Text shapeSizeValueText; //Display value of shapeSizeSlider
   public Slider focusYSlider; // Focus Point position in Y-axis
   public Text focusYValueText; // Display value of focusYSlider
   public Slider focusScaleSlider; // Focus Point size 
   public Text focusScaleValueText; // Display value of focusScaleSlider
   public Dropdown focusShapeDropdown; //Focus Point Shape (0 = Circle, 1 = Cross)
   public Dropdown focusChangeDropdown; //Focus point changability (0 = Static, 1 = Fixed interval change, 2 = Random interval change)
   public Dropdown intervalSetsDropdown; //Number of sets for focus point fixed interval change
   public InputField successRateInput; //Number of sets should answered True to count as success
   public InputField failRateInput; // Number of sets should answered False to count as failure
   public InputField chunkSizeInput; //Chunk size
   public List<Toggle> imageSetToggles; //ScrollView for image set selection

    //Tool tip variables 
    public GameObject tooltipPanel;
    public Text tooltipText;
    public List<Button> infoButtons = new List<Button>();
    private Button currentInfoButton = null;

    //UI Panel Buttons 
    public Button saveConfigButton; // Save configuration button
    public Button loadConfigButton; // Load configuration button
    public Button StartButton;
    public Button UIPreviousButton;

   //Configurations save and load Dialogs variables
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

   //Message dialog variables 
   public GameObject MessageDialogPanel;
   public Text MessageText;
   public Button MessageOkButton;


    // User selection panel variables 
    public GameObject userSelectionPanel;
    public InputField userSearchInput;
    public Transform userListScrollContent;
    public Button newUserButton;
    public Button userSelectionNextButton;
    public Button userSortButton;
    public GameObject userSortOptionsPanel;
    public Button userSortByNameButton;
    public Button userSortByIDButton;
    public Button userSortByLatestButton;
    public Button userSortByOldestButton;
    public Button userDeleteButton;
    private string currentUserSortMode = "latest"; //default
    private string selectedUserID = "";
    private bool isNewUser = true;
    public Button exitButton;


    //User details panel variables 
    public GameObject infoPannel;
    public InputField NameInput;
    public InputField IDInput;
    public InputField AgeInput;
    public Dropdown GenderDropdown;
    public Dropdown DateYearDropDown;
    public Dropdown DateMonthDropDown;
    public Dropdown DateDayDropDown;
    public Dropdown EyeDropDown; //Right eye = 0, Left eye = 1
    public Button InfoPreviousButton;
   public Button continueButton;

    // Results popup
    public GameObject resultsPanel;
    public Text resultsUserIDText;
    public Text resultsTimestampText;
    public Text resultsEyeText;
    public Text resultsTestDurationText;
    public Text resultsFocusPositionText;
    public Text resultsFocusScaleText;
    public Text resultsFocusShapeText;
    public Text resultsSetDisplayDurationText;
    public Text resultsBetweenSetsDurationText;
    public Text resultsFocusChangeModeText;
    public Text resultsIntervalSetsText;
    public Text resultsSuccessRateText;
    public Text resultsFailRateText;
    public Text resultsChunkSizeText;
    public Text resultsStartingDistanceText;
    public Text resultsStartingShapeScaleText;
    public Text resultsLevelProgressionText;
    public GameObject resultsLevelProgressionScrollView;
    public Text resultsLevelProgressionScrollText;
    public Text resultsAccuracyText;
    public Text resultsAvgResponseTimeText;
    public Text resultsTrialsText;
    public Text resultsCorrectResponsesText;
    public Button resultsCloseButton;
    public Button resultsExpandButton;
    public Transform resultsLevelDetailsContent;
    public Text resultsExpandButtonText;
    public Text partialResultsLabel;
    public GameObject partialResultsScrollView;
    private Vector2 normalPopupOffsetMin = new Vector2(600, 350);
    private Vector2 normalPopupOffsetMax = new Vector2(-600, -350);
    private Vector2 expandedPopupOffsetMin = new Vector2(600, 300);
    private Vector2 expandedPopupOffsetMax = new Vector2(-600, -300);
    private bool isResultsExpanded = false;

    // Results browser variables 
    public Button showResultsButton;
    public GameObject resultsListPanel;
    public Transform resultsListScrollContent;
    public Button resultsListCancelButton;
    public Button resultsListRemoveButton;
    public Button resultsListExpandButton;
    private List<GameResult> allGameResults = new List<GameResult>();
    private GameObject selectedResultItem = null;
    private int selectedResultIndex = -1;


    void Start()
    {
        //Main UI Panel Listeners
        //Elements/Options listeners
        startingDistanceSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        startingDistanceSlider.onValueChanged.AddListener(delegate { UpdateMaxDistanceRange(); });
        maxDistanceSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        shapeSizeSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        focusYSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        focusScaleSlider.onValueChanged.AddListener(delegate { UpdateSliderValueDisplay(); });
        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); }); //focus point change
        UpdateSliderValueDisplay(); //initial display
        UpdateMaxDistanceRange();
        OnFocusChangeDropdownChanged();
        focusScaleSlider.minValue = 1f;
        focusScaleSlider.maxValue = 10f;
        focusScaleSlider.value = 8f;  

        // buttons listeners
        saveConfigButton.onClick.AddListener(ShowSaveDialog);
        loadConfigButton.onClick.AddListener(ShowLoadDialog);
        continueButton.onClick.AddListener(ShowInfoPanel);
        StartButton.onClick.AddListener(SaveSettingsAndClose);

        //save and load dialogs buttons listeners
        saveDialogSaveButton.onClick.AddListener(SaveConfigurationWithName);
        saveDialogCancelButton.onClick.AddListener(HideSaveDialog);
        loadDialogDeleteButton.onClick.AddListener(DeleteSelectedConfiguration);
        loadDialogLoadButton.onClick.AddListener(LoadSelectedConfiguration);
        loadDialogCancelButton.onClick.AddListener(HideLoadDialog);

        //Message dialog listener
        MessageOkButton.onClick.AddListener(() => MessageDialogPanel.SetActive(false));


        //User selection panel listeners
        exitButton.onClick.AddListener(ExitApplication);
        userSearchInput.onValueChanged.AddListener(OnSearchTextChanged);
        newUserButton.onClick.AddListener(SelectNewUser);
        userSelectionNextButton.onClick.AddListener(MoveToInfoPanel);
        userSortButton.onClick.AddListener(ToggleUserSortOptions);
        userDeleteButton.onClick.AddListener(DeleteSelectedUser);
        userSortByNameButton.onClick.AddListener(() => SortUsers("name"));
        userSortByIDButton.onClick.AddListener(() => SortUsers("id"));
        userSortByLatestButton.onClick.AddListener(() => SortUsers("latest"));
        userSortByOldestButton.onClick.AddListener(() => SortUsers("oldest"));
        LoadUsersList();
        userSelectionNextButton.interactable = false;


        //User Info Panel listeners
        //Elements listeners
        DateYearDropDown.onValueChanged.AddListener(delegate { CalculateAndDisplayAge(); });
        DateMonthDropDown.onValueChanged.AddListener(delegate { CalculateAndDisplayAge(); });
        DateDayDropDown.onValueChanged.AddListener(delegate { CalculateAndDisplayAge(); });
        UIPreviousButton.onClick.AddListener(ReturnToUIPanel);
        InfoPreviousButton.onClick.AddListener(ReturnToUserSelection);

        //Results list buttons listeners
        showResultsButton.onClick.AddListener(ShowResultsList);
        resultsListCancelButton.onClick.AddListener(HideResultsList);
        resultsListRemoveButton.onClick.AddListener(RemoveSelectedResult);
        resultsListExpandButton.onClick.AddListener(ExpandSelectedResult);


        //Main UI tooltips
        SetupTooltipButton("TimeInputInfoButton", ReverseHebrewText("משך האימון הכולל (בדקות)."));
        SetupTooltipButton("ShapeDurationInputInfoButton", ReverseHebrewText("משך הצגת סט של תמונות (במילישניות)."));
        SetupTooltipButton("BetweenShapeDurationInputInfoButton", ReverseHebrewText("משך ההמתנה בין שני סטים של תמונות (במילישניות)."));
        SetupTooltipButton("StartingDistanceSliderInfoButton", ReverseHebrewText("המרחק ההתחלתי של התמונות מנקודת המיקוד."));
        SetupTooltipButton("MaxDistanceSliderInfoButton", ReverseHebrewText("המרחק המרבי מנקודת המיקוד שהתמונות יכולות להגיע אליו."));
        SetupTooltipButton("ShapeSizeSliderInfoButton", ReverseHebrewText("גודל התמונות ההתחלתי. "));
        SetupTooltipButton("ImageSetScrollViewInfoButton", ReverseHebrewText("קטגוריות התמונות שיופיעו במהך האימון."));
        SetupTooltipButton("FocusShapeDropdownInfoButton", ReverseHebrewText("צורת נקודת המיקוד."));
        SetupTooltipButton("FocusYSliderInfoButton", ReverseHebrewText("גובה נקודת המיקוד."));
        SetupTooltipButton("FocusScaleSliderInfoButton", ReverseHebrewText("גודל נקודת המיקוד."));
        SetupTooltipButton("FocusPointLocationChangeDropDownInfoButton", ReverseHebrewText("מצב שינוי מיקום נקודת המיקוד. סטטי, משתנה במרווחים קבועים או אקראיים."));
        SetupTooltipButton("FocusPointLocationChangeIntervalDropdownInfoButton", ReverseHebrewText("תדירות שינוי נקודת המיקוד (באינטרוולים). רלוונטי במרווחים קבועים."));
        SetupTooltipButton("chunkSizeInputInfoButton", ReverseHebrewText("מספר הסטים בהם מעריכים את הביצועים לפני החלטה על שינוי ברמה."));
        SetupTooltipButton("SuccessRateInputInfoButton", ReverseHebrewText("אחוז התשובות הנכונות הנדרש כדי לעלות רמה."));
        SetupTooltipButton("FailRateInputInfoButton", ReverseHebrewText("אחוז התשובות השגויות שבו יורדים רמה."));

        CheckAndDisplaySessionResults();
    }

    //A function to update the slider value displays
    void UpdateSliderValueDisplay()
    {
        if (startingDistanceValueText != null)
            startingDistanceValueText.text = startingDistanceSlider.value.ToString();

        if (maxDistanceValueText != null)
            maxDistanceValueText.text = maxDistanceSlider.value.ToString();
        
        if (shapeSizeValueText != null)
            shapeSizeValueText.text = shapeSizeSlider.value.ToString();
            
        if (focusYValueText != null)
            focusYValueText.text = focusYSlider.value.ToString();

        if (focusScaleValueText != null)
            focusScaleValueText.text = focusScaleSlider.value.ToString();
    }

    //A function to update the max distance slider range based on starting distance
    //Functionality: Goal distance can't be less than starting distance
    void UpdateMaxDistanceRange()
    {
        float startingDist = startingDistanceSlider.value;
        maxDistanceSlider.minValue = startingDist;
        maxDistanceSlider.maxValue = 10f;
        
        //Ensuring current value is within new range
        if (maxDistanceSlider.value < startingDist)
            maxDistanceSlider.value = startingDist;
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
        settings.maxDistance = maxDistanceSlider.value;
        settings.shapeScale = shapeSizeSlider.value * 0.0036f + 0.004f;

        //Focus point settings
        settings.focusY = focusYSlider.value / 100f;
        settings.focusScale = focusScaleSlider.value / 100f;
        settings.focusShape = focusShapeDropdown.value;
        settings.focusChangeMode = focusChangeDropdown.value;
        settings.intervalSets = intervalSetsDropdown.value + 1;

        //Success/Fail rates and chunk size
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

        //Image sets selection
        for (int i = 0; i < imageSetToggles.Count; i++)
        {
            if (imageSetToggles[i].isOn)
                settings.imageSets.Add(i + 1);
        }

        //Save to config folder
        string json = JsonUtility.ToJson(settings, true);
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        
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
        //Clear previous buttons
        foreach (Transform child in loadDialogContent)
        {
            Destroy(child.gameObject);
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        
        if (!Directory.Exists(configFolder))
        {
            Directory.CreateDirectory(configFolder);
        }
        
        string[] configFiles = Directory.GetFiles(configFolder, "*.json");
        
        //Create a button for each config file
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
                    //When unchecked, return to gray
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

    //A function to handle selection of a configuration set in the load dialog
    void SelectConfig(string configName)
    {
        selectedConfigToLoad = configName;
        
        //Highlight selected and unhighlight others
        foreach (Transform child in loadDialogContent)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            Image bgImage = child.GetComponent<Image>();
            
            if (toggle != null && bgImage != null)
            {
                if (child.name == configName)
                {
                    //Highlight selected
                    bgImage.color = new Color(0.3f, 0.6f, 1f, 1f); // Blue highlight
                    toggle.isOn = true;
                }
                else
                {
                    //Unhighlight others
                    bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Gray
                    toggle.isOn = false;
                }
            }
        }
    }

    //A function that hides the load configuration dialog
    void HideLoadDialog()
    {
        loadDialogPanel.SetActive(false);
    }

    //A function that handles loading the selected configuration
    void LoadSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(selectedConfigToLoad))
        {
            showMessage(".הניעטל תורדגה טס רוחבל אנ");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        string path = Path.Combine(configFolder, selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            //Load durations
            timeInput.text = (settings.gameDuration / 60f).ToString();
            betweenShapesDuration.text = settings.betweenShapesDuration.ToString();
            shapeDisplayDuration.text = settings.shapeDisplayDuration.ToString();
            
            //Load sliders
            startingDistanceSlider.value = settings.startingDistance;
            maxDistanceSlider.value = settings.maxDistance;
            UpdateMaxDistanceRange();
            shapeSizeSlider.value = (settings.shapeScale - 0.004f) / 0.0036f;
            focusYSlider.value = settings.focusY * 100f;
            focusScaleSlider.value = settings.focusScale * 100f;
            
            //Load dropdowns
            focusShapeDropdown.value = settings.focusShape;
            focusChangeDropdown.value = settings.focusChangeMode;
            intervalSetsDropdown.value = settings.intervalSets - 1;
            
            //Load success/fail rates and chunk size
            successRateInput.text = settings.successRate.ToString();
            failRateInput.text = settings.failRate.ToString();
            chunkSizeInput.text = settings.chunkSize.ToString();
            
            //Load image sets toggles
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

    //A function that deletes the configuration set selected to be deleted
    void DeleteSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(selectedConfigToLoad))
        {
            showMessage(".הקיחמל תורדגה טס רוחבל אנ");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        string path = Path.Combine(configFolder, selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            //Delete from disk
            File.Delete(path);
            showMessage(".קחמנ תורדגהה טס");
            
            //Remove from GUI 
            foreach (Transform child in loadDialogContent)
            {
                if (child.name == selectedConfigToLoad)
                {
                    DestroyImmediate(child.gameObject);
                    break;
                }
            }
            
            //Reposition remaining toggles
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
            
            //Clear selection
            selectedConfigToLoad = "";
        }
        else
        {
            showMessage(".אצמנ אל תורדגהה ץבוק");
        }
    }

    //A function responsible for saving the settings, closing the GUI, and launching the game
   void SaveSettingsAndClose()
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

        //User details validation
        string userName = NameInput.text.Trim();
        string userID = IDInput.text.Trim();
        string userAge = AgeInput.text.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            userName = "N/A";
        }

        if (string.IsNullOrEmpty(userID))
        {
            showMessage(".תוהזה תדועת רפסמ תא ןיזהל אנ");
            return;
        }

        int age;
        if (string.IsNullOrEmpty(userAge) || !int.TryParse(userAge, out age))
        {
            age = -1;
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
        settings.maxDistance = maxDistanceSlider.value;
        settings.shapeScale = shapeSizeSlider.value * 0.0036f + 0.004f;

        //Focus point settings: location, size, shape, and change mode.
       settings.focusY = focusYSlider.value / 100f;
       settings.focusScale = focusScaleSlider.value / 100f;
       settings.focusShape = focusShapeDropdown.value;
        settings.focusChangeMode = focusChangeDropdown.value;
        settings.intervalSets = intervalSetsDropdown.value + 1;

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

        //Image set selection
        for (int i = 0; i < imageSetToggles.Count; i++)
        {
            if (imageSetToggles[i].isOn)
                settings.imageSets.Add(i + 1);
        }

        //User details
        settings.userID = userID;
        settings.trainingEye = EyeDropDown.value; // 0 = Right, 1 = Left
        settings.sessionTimestamp = timestamp;

        //Saving the settings
       string json = JsonUtility.ToJson(settings, true);
       string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
       if (!Directory.Exists(csvFolder))
            Directory.CreateDirectory(csvFolder);
       string path = Path.Combine(csvFolder, "vr_settings.json");
       File.WriteAllText(path, json);
       
       SaveUserDetailsToCSV(userName, userID, age, GenderDropdown.value, DateYearDropDown.value, DateMonthDropDown.value, DateDayDropDown.value, EyeDropDown.value, timestamp);
        LaunchGameApplication(); //Game Launch 
        //Application.Quit();
   }

    //A function to launch the game application
    void LaunchGameApplication()
    {
        string gamePath = Path.Combine(Application.dataPath, "..", "Game", "Game.exe");

        if (File.Exists(gamePath))
        {
            System.Diagnostics.Process.Start(gamePath);
            Debug.Log("Game application launched");
        }
        else
        {
            Debug.LogWarning("Game executable not found at: " + gamePath);
        }
    }

    //A listener function connected to "InfoPreviousButton". It returns to the main UI panel from the info panel
    void ReturnToUIPanel()
    {
        uiPanel.SetActive(false);
        infoPannel.SetActive(true);
    }

    void ReturnToUserSelection()
    {
        infoPannel.SetActive(false);
        userSelectionPanel.SetActive(true);
    }

    public void OnFocusChangeDropdownChanged()
    {
        intervalSetsDropdown.interactable = focusChangeDropdown.value == 1;
    }

    void ShowInfoPanel()
    {
        //Hide info panel and show main UI panel
        infoPannel.SetActive(false);
        uiPanel.SetActive(true);
    }

    //A function to show a message dialog with the given message
    void showMessage(string message)
    {
        MessageText.text = message;
        MessageDialogPanel.SetActive(true);
    }

    //A function to save user details to a CSV file
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
                        string ageText = userAge == -1 ? "N/A" : userAge.ToString();
                        string firstAdded = fields[8]; 
                        string lastUpdate = timestamp;
                        existingLines[i] = $"{userID},{userName},{ageText},{genderText},{birthYear},{birthMonth},{birthDay},{newEyeText},{firstAdded},{lastUpdate}";
                        
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
            string ageText = userAge == -1 ? "N/A" : userAge.ToString();
            string eyeText = trainingEye == 0 ? "Right" : "Left";
            string currentTime = timestamp;

            writer.WriteLine($"{userID},{userName},{ageText},{genderText},{birthYear},{birthMonth},{birthDay},{eyeText},{currentTime},{currentTime}");
        }
    }

    //A method to check for session results and display them
    void CheckAndDisplaySessionResults()
    {
        string flagPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "show_latest_result.flag");
        
        //Only show results if flag file exists
        if (!File.Exists(flagPath))
            return;
        
        //Delete flag
        File.Delete(flagPath);
        
        //Load results from CSV
        LoadGameResultsFromCSV();
        
        if (allGameResults.Count == 0)
            return;
        
        //Set the latest result as selected and expand it
        selectedResultIndex = 0;
        ExpandSelectedResult();
    }

    //A function to toggle the expansion of detailed results in the results popup
    void ToggleResultsExpansion(SessionResults results)
    {
        isResultsExpanded = !isResultsExpanded;
        
        RectTransform popupRect = resultsPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            if (isResultsExpanded)
            {
                popupRect.offsetMin = expandedPopupOffsetMin;
                popupRect.offsetMax = expandedPopupOffsetMax;
            }
            else
            {
                popupRect.offsetMin = normalPopupOffsetMin;
                popupRect.offsetMax = normalPopupOffsetMax;
            }
        }
        
        if (partialResultsLabel != null)
        {
            partialResultsLabel.gameObject.SetActive(isResultsExpanded);  // Toggle label
        }
        if (resultsLevelDetailsContent != null)
        {
            resultsLevelDetailsContent.parent.gameObject.SetActive(isResultsExpanded);  // Toggle ScrollView
            partialResultsScrollView.SetActive(isResultsExpanded);
        }
        
        if (resultsExpandButtonText != null)
        {
            resultsExpandButtonText.text = isResultsExpanded ? "כווץ" : "הרחב";
        }
        
        if (isResultsExpanded)
        {
            DisplayLevelResults(results);
        }
    }
    
    //A function to display detailed level results in the results popup
    void DisplayLevelResults(SessionResults results)
    {
        if (resultsLevelDetailsContent == null)
            return;
        
        // Clear previous level results
        foreach (Transform child in resultsLevelDetailsContent)
        {
            Destroy(child.gameObject);
        }
        
        if (results.levelResults == null || results.levelResults.Count == 0)
        {
            GameObject noDataObj = new GameObject("NoData");
            noDataObj.transform.SetParent(resultsLevelDetailsContent, false);
            
            Text noDataText = noDataObj.AddComponent<Text>();
            noDataText.text = "אין נתונים לפי רמות";
            noDataText.color = Color.gray;
            noDataText.fontSize = 14;
            noDataText.alignment = TextAnchor.MiddleCenter;
            noDataText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform rt = noDataObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 30);
            
            return;
        }
        
        int index = 0;
        foreach (var levelResult in results.levelResults)
        {
            GameObject rowObj = new GameObject("LevelRow_" + levelResult.levelName);
            rowObj.transform.SetParent(resultsLevelDetailsContent, false);
            
            RectTransform rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 1);
            rowRect.anchorMax = new Vector2(1, 1);
            rowRect.pivot = new Vector2(0.5f, 1);
            rowRect.sizeDelta = new Vector2(0, 25);
            rowRect.anchoredPosition = new Vector2(0, -index * 30);
            
            Text rowText = rowObj.AddComponent<Text>();
            rowText.text = $"{levelResult.levelName}: דיוק {levelResult.accuracy:F1}%, זמן {levelResult.avgResponseTime:F2}s, ניסיונות {levelResult.trials}, נכונות {levelResult.correctResponses}";
            rowText.color = Color.black;
            rowText.fontSize = 12;
            rowText.alignment = TextAnchor.MiddleRight;
            rowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            index++;
        }
        
        RectTransform contentRect = resultsLevelDetailsContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, index * 30);
        }
    }

    //A function to show the results list panel
    void ShowResultsList()
    {
        LoadGameResultsFromCSV();
        allGameResults.RemoveAll(result => result.userID != selectedUserID); // keep only user results
        DisplayResultsInList();
        
        // Show cancel, hide remove and expand
        resultsListCancelButton.gameObject.SetActive(true);
        resultsListRemoveButton.gameObject.SetActive(false);
        resultsListExpandButton.gameObject.SetActive(false);
        
        selectedResultItem = null;
        selectedResultIndex = -1;
        
        resultsListPanel.SetActive(true);
    }

    void HideResultsList()
    {
        resultsListPanel.SetActive(false);
    }

    //A function to load game results from the CSV file after game ended
    void LoadGameResultsFromCSV()
    {
        allGameResults.Clear();
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "game_results.csv");
        
        if (!File.Exists(csvPath))
        {
            showMessage(".אצמנ אל תואצות ץבוק");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvPath);
        
        // Skip header (line 0) and read data rows
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            
            if (fields.Length < 24) continue; //validation 
            
            GameResult result = new GameResult
            {
                userID = fields[0],
                timestamp = fields[1],
                eyeTrained = fields[2],
                testDuration = fields[3],
                focusY = fields[4],
                focusScale = fields[5],
                focusShape = fields[6],
                shapeDisplayDuration = fields[7],
                betweenShapesDuration = fields[8],
                focusChangeMode = fields[9],
                intervalSets = fields[10],
                successRate = fields[11],
                failRate = fields[12],
                chunkSize = fields[13],
                startingDistance = fields[14],
                startingShapeScale = fields[15],
                overallAccuracy = fields[16],
                overallAvgResponseTime = fields[17],
                overallTrials = fields[18],
                overallCorrectResponses = fields[19],
                csvLineIndex = i
            };
            
            //Parsing level details
            int levelStartIndex = 20;
            for (int j = 0; j < 20; j++)
            {
                int baseIdx = levelStartIndex + (j * 4);
                if (baseIdx + 3 < fields.Length)
                {
                    result.levelAccuracies.Add(fields[baseIdx]);
                    result.levelAvgResponseTimes.Add(fields[baseIdx + 1]);
                    result.levelTrials.Add(fields[baseIdx + 2]);
                    result.levelCorrectResponses.Add(fields[baseIdx + 3]);
                }
            }
            
            if (fields.Length > 100)
                result.levelProgression = fields[fields.Length - 1];
            
            allGameResults.Add(result);
        }
        allGameResults.Sort((a, b) => string.Compare(b.timestamp, a.timestamp)); // sort by timestamp
    }

    //A function to display the loaded results in the scrollable list
    void DisplayResultsInList()
    {
        // Clear previous items
        foreach (Transform child in resultsListScrollContent)
        {
            Destroy(child.gameObject);
        }
        
        if (allGameResults.Count == 0)
        {
            //Show "no results" message
            GameObject noDataObj = new GameObject("NoResults");
            noDataObj.transform.SetParent(resultsListScrollContent, false);
            
            Text noDataText = noDataObj.AddComponent<Text>();
            noDataText.text = "תואצות ןיא";
            noDataText.color = Color.gray;
            noDataText.fontSize = 18;
            noDataText.alignment = TextAnchor.MiddleCenter;
            noDataText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            RectTransform rt = noDataObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(700, 50);
            
            return;
        }
        
        //creating a result item for each result
        for (int i = 0; i < allGameResults.Count; i++)
        {
            CreateResultItem(allGameResults[i], i);
        }
        
        //updating content size
        RectTransform contentRect = resultsListScrollContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, allGameResults.Count * 80);
        }
    }

    //A function to create a single result item in the results list
    void CreateResultItem(GameResult result, int index)
    {
        GameObject itemObj = new GameObject("ResultItem_" + index);
        itemObj.transform.SetParent(resultsListScrollContent, false);
        
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.sizeDelta = new Vector2(-20, 70);
        itemRect.anchoredPosition = new Vector2(0, -index * 80);
        
        Image bgImage = itemObj.AddComponent<Image>();
        bgImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        
        Button itemButton = itemObj.AddComponent<Button>();
        int capturedIndex = index;
        itemButton.onClick.AddListener(() => OnResultItemClicked(itemObj, capturedIndex));
        
        //Main info text (Date part)
        GameObject dateObj = new GameObject("Date");
        dateObj.transform.SetParent(itemObj.transform, false);

        RectTransform dateRect = dateObj.AddComponent<RectTransform>();
        dateRect.anchorMin = new Vector2(0, 1);
        dateRect.anchorMax = new Vector2(0.5f, 1);
        dateRect.pivot = new Vector2(0, 1);
        dateRect.anchoredPosition = new Vector2(10, -5);
        dateRect.sizeDelta = new Vector2(0, 25);

        Text dateText = dateObj.AddComponent<Text>();
        dateText.text = result.timestamp;
        dateText.color = Color.black;
        dateText.fontSize = 12;
        dateText.alignment = TextAnchor.UpperLeft;
        dateText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Main info text (rest of the details)
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(itemObj.transform, false);

        RectTransform infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 0.7f);
        infoRect.offsetMin = new Vector2(10, 5);
        infoRect.offsetMax = new Vector2(-10, 0);

        Text infoText = infoObj.AddComponent<Text>();
        string eyeDisplay = result.eyeTrained == "Right" ? "ןימי" : (result.eyeTrained == "Left" ? "לאמש" : result.eyeTrained);
        infoText.text = $"{eyeDisplay} :ןיע | {result.userID} :ז.ת\n" +
                        $"{result.overallTrials}/{result.overallCorrectResponses} :תובוגת | {result.overallAccuracy} :קויד זוחא | {result.overallAvgResponseTime} :הבוגת ןמז";
        infoText.color = Color.black;
        infoText.fontSize = 14;
        infoText.alignment = TextAnchor.MiddleRight;
        infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    //A function called when a result item is clicked in the results list
    void OnResultItemClicked(GameObject itemObj, int index)
    {
        //Deselect previous item
        if (selectedResultItem != null && selectedResultItem != itemObj)
        {
            Image prevBg = selectedResultItem.GetComponent<Image>();
            if (prevBg != null) prevBg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        }
        
        //Select current item
        selectedResultItem = itemObj;
        selectedResultIndex = index;
        
        Image bg = itemObj.GetComponent<Image>();
        if (bg != null) bg.color = new Color(0.8f, 0.9f, 1f, 1f);
        
        //Show remove and expand buttons
        resultsListRemoveButton.gameObject.SetActive(true);
        resultsListExpandButton.gameObject.SetActive(true);
    }

    //A function to remove the selected result from the CSV file
    void RemoveSelectedResult()
    {
        if (selectedResultIndex < 0 || selectedResultIndex >= allGameResults.Count)
        {
            showMessage(".הקיחמל טלפ רוחבל אנ");
            return;
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "game_results.csv");
        
        if (!File.Exists(csvPath))
        {
            showMessage(".אצמנ אל תואצות ץבוק");
            return;
        }
        
        List<string> lines = new List<string>(File.ReadAllLines(csvPath));
        
        //removing the selected result line
        int lineToRemove = allGameResults[selectedResultIndex].csvLineIndex;
        if (lineToRemove < lines.Count)
        {
            lines.RemoveAt(lineToRemove);
            
            File.WriteAllLines(csvPath, lines);
            
            showMessage(".קחמנ טלפה");
            
            //refreshing the display
            LoadGameResultsFromCSV();
            DisplayResultsInList();
            
            //hiding remove and expand buttons
            resultsListRemoveButton.gameObject.SetActive(false);
            resultsListExpandButton.gameObject.SetActive(false);
            
            selectedResultItem = null;
            selectedResultIndex = -1;
        }
    }

    //A function to expand and show detailed data of the selected result
    void ExpandSelectedResult()
    {
        if (selectedResultIndex < 0 || selectedResultIndex >= allGameResults.Count)
        {
            showMessage(".הרחבל טלפ רוחבל אנ");
            return;
        }
        
        GameResult result = allGameResults[selectedResultIndex];
        
        if (resultsPanel == null)
        {
            showMessage(".הרחב חולל ןיא");
            return;
        }
        
        string eyeDisplay = result.eyeTrained == "Right" ? "ןימי" : (result.eyeTrained == "Left" ? "לאמש" : result.eyeTrained);

        if (resultsUserIDText != null)
            resultsUserIDText.text = result.userID + " <b>:ז.ת</b>";

        if (resultsTimestampText != null)
            resultsTimestampText.text = result.timestamp + " <b>:הקידבה ןמז</b>";

        if (resultsEyeText != null)
            resultsEyeText.text = eyeDisplay + " <b>:תנמואמ ןיע</b>";

        if (resultsTestDurationText != null)
            resultsTestDurationText.text = result.testDuration + " <b>:(תוקד) הקידבה ךשמ</b>";

        if (resultsFocusPositionText != null)
            resultsFocusPositionText.text = result.focusY + " <b>:דוקימ תדוקנ םוקימ</b>";

        if (resultsFocusScaleText != null)
            resultsFocusScaleText.text = result.focusScale + " <b>:דוקימ תדוקנ לדוג</b>";

        if (resultsFocusShapeText != null)
            resultsFocusShapeText.text = result.focusShape + " <b>:דוקימ תדוקנ תרוצ</b>";

        if (resultsSetDisplayDurationText != null)
            resultsSetDisplayDurationText.text = result.shapeDisplayDuration + " <b>:(ms) תונומת תגצה ךשמ</b>";

        if (resultsBetweenSetsDurationText != null)
            resultsBetweenSetsDurationText.text = result.betweenShapesDuration + " <b>:(ms) םיטס ןיב ךשמ</b>";

        if (resultsFocusChangeModeText != null)
            resultsFocusChangeModeText.text = result.focusChangeMode + " <b>:דוקימ תדוקנ יוניש בצמ</b>";

        if (resultsIntervalSetsText != null)
            resultsIntervalSetsText.text = result.intervalSets + " <b>:(םילווטרניא) יוניש תורידת</b>";

        if (resultsSuccessRateText != null)
            resultsSuccessRateText.text = result.successRate + " <b>:החלצה זוחא</b>";

        if (resultsFailRateText != null)
            resultsFailRateText.text = result.failRate + " <b>:ןולשכ זוחא</b>";

        if (resultsChunkSizeText != null)
            resultsChunkSizeText.text = result.chunkSize + " <b>:הכורע לדוג</b>";

        if (resultsStartingDistanceText != null)
            resultsStartingDistanceText.text = result.startingDistance + " <b>:תלחתה קחרמ</b>";

        if (resultsStartingShapeScaleText != null)
            resultsStartingShapeScaleText.text = result.startingShapeScale + " <b>:תלחתה לדוג</b>";

        if (resultsLevelProgressionText != null)
        {
            if (resultsLevelProgressionScrollText == null && resultsLevelProgressionScrollView != null)
            {
                Transform viewport = resultsLevelProgressionScrollView.transform.Find("Viewport");
                if (viewport != null)
                {
                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        RectTransform contentRT = content.GetComponent<RectTransform>();
                        contentRT.sizeDelta = new Vector2(3000, contentRT.sizeDelta.y);
                        contentRT.pivot = new Vector2(1f, 0.5f);
                        contentRT.anchorMin = new Vector2(1f, 0f);
                        contentRT.anchorMax = new Vector2(1f, 1f);
                        
                        GameObject textObj = new GameObject("ProgressionScrollText");
                        textObj.transform.SetParent(content, false);
                        
                        Text text = textObj.AddComponent<Text>();
                        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        text.fontSize = 14;
                        text.alignment = TextAnchor.MiddleRight;
                        text.horizontalOverflow = HorizontalWrapMode.Overflow;
                        text.verticalOverflow = VerticalWrapMode.Truncate;
                        text.color = Color.black;
                        
                        RectTransform rt = textObj.GetComponent<RectTransform>();
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                        
                        resultsLevelProgressionScrollText = text;
                    }
                }
            }
            
            float startDist = float.Parse(result.startingDistance);
            float startScale = float.Parse(result.startingShapeScale);
            
            string[] progressionSteps = result.levelProgression.Split(' ');
            System.Array.Reverse(progressionSteps);
            string transformedProgression = "";
            
            foreach (string step in progressionSteps)
            {
                string trimmedStep = step.Trim();
                string action = "";
                string color = "white";
                
                if (trimmedStep.StartsWith("Start"))
                {
                    action = "הלחתה";
                    color = "white";
                }
                else if (trimmedStep.StartsWith("Up"))
                {
                    action = "הילע";
                    color = "green";
                }
                else if (trimmedStep.StartsWith("Down"))
                {
                    action = "הדירי";
                    color = "red";
                }
                else if (trimmedStep.StartsWith("Same"))
                {
                    action = "תועיבק";
                    color = "white";
                }
                
                int startIdx = trimmedStep.IndexOf('D');
                int endIdx = trimmedStep.IndexOf(')');
                if (startIdx != -1 && endIdx != -1)
                {
                    string levelInfo = trimmedStep.Substring(startIdx + 1, endIdx - startIdx - 1);
                    string sizeLevel = levelInfo.Substring(levelInfo.Length - 1);
                    int d = int.Parse(levelInfo.Substring(0, levelInfo.Length - 1));
                    
                    int sizeValue;
                    if (sizeLevel == "L")
                        sizeValue = d - (int)(startDist - startScale);
                    else
                        sizeValue = d - (int)(startDist - startScale) - 1;
                    
                    string transformedStep = $"<color={color}>({sizeValue} לדוג , {d} קחרמ)<b>{action}</b></color>";
                    
                    if (transformedProgression.Length > 0)
                        transformedProgression += " , ";
                    transformedProgression += transformedStep;
                }
            }
            
            if (progressionSteps.Length > 4)
            {
                resultsLevelProgressionText.text = "<b>:םיבלשב תומדקתה</b>";
                
                resultsLevelProgressionScrollView.SetActive(true);
                if (resultsLevelProgressionScrollText != null)
                {
                    resultsLevelProgressionScrollText.text = transformedProgression;
                }
            }
            else
            {
                resultsLevelProgressionText.text = transformedProgression + " <b>:םיבלשב תומדקתה</b>";                
                resultsLevelProgressionScrollView.SetActive(false);
            }
        }

        if (resultsAccuracyText != null)
            resultsAccuracyText.text = result.overallAccuracy + " <b>:קויד זוחא</b>";

        if (resultsAvgResponseTimeText != null)
            resultsAvgResponseTimeText.text = result.overallAvgResponseTime + " <b>:עצוממ הבוגת ןמז</b>";

        if (resultsTrialsText != null)
            resultsTrialsText.text = result.overallTrials + " <b>:םיטס כהס</b>";

        if (resultsCorrectResponsesText != null)
            resultsCorrectResponsesText.text = result.overallCorrectResponses + " <b>:תונוכנ תובוגת כהס</b>";
        
        //Setup close button
        if (resultsCloseButton != null)
        {
            resultsCloseButton.onClick.RemoveAllListeners();
            resultsCloseButton.onClick.AddListener(() => resultsPanel.SetActive(false));
        }
        
        //Setup expand button for level details
        if (resultsExpandButton != null)
        {
            resultsExpandButton.onClick.RemoveAllListeners();
            resultsExpandButton.onClick.AddListener(() => ToggleExpandedResultView(result));
        }
        
        //Reset expansion state
        isResultsExpanded = false;
        RectTransform popupRect = resultsPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            popupRect.offsetMin = normalPopupOffsetMin;
            popupRect.offsetMax = normalPopupOffsetMax;
        }
        
        if (partialResultsLabel != null)
            partialResultsLabel.gameObject.SetActive(false);
        if (resultsLevelDetailsContent != null)
            resultsLevelDetailsContent.parent.gameObject.SetActive(false);
        
        if (resultsExpandButtonText != null)
            resultsExpandButtonText.text = "בחרה";
        
        //Hide the results list and show the details panel
        resultsPanel.SetActive(true);
    }

    //A function to toggle the expanded view of level details in the results popup
    void ToggleExpandedResultView(GameResult result)
    {
        isResultsExpanded = !isResultsExpanded;
        
        RectTransform popupRect = resultsPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            if (isResultsExpanded)
            {
                popupRect.offsetMin = expandedPopupOffsetMin;
                popupRect.offsetMax = expandedPopupOffsetMax;
            }
            else
            {
                popupRect.offsetMin = normalPopupOffsetMin;
                popupRect.offsetMax = normalPopupOffsetMax;
            }
        }
        
        if (partialResultsLabel != null)
            partialResultsLabel.gameObject.SetActive(isResultsExpanded);
        
        if (resultsLevelDetailsContent != null)
            resultsLevelDetailsContent.parent.gameObject.SetActive(isResultsExpanded);
            partialResultsScrollView.SetActive(isResultsExpanded);
        
        if (resultsExpandButtonText != null)
            resultsExpandButtonText.text = isResultsExpanded ? "ץווכ" : "בחרה";
        
        if (isResultsExpanded)
        {
            DisplayExpandedLevelDetails(result);
        }
    }

    //A function to display expanded level details in the results popup
    void DisplayExpandedLevelDetails(GameResult result)
    {
        if (resultsLevelDetailsContent == null) return;
        
        //Clear previous
        foreach (Transform child in resultsLevelDetailsContent)
        {
            Destroy(child.gameObject);
        }
        
        int d = 1;
        string sLevel = "L";
        int displayedLevels = 0;
        
        for (int i = 0; i < 20; i++)
        {
            if (i < result.levelTrials.Count && !string.IsNullOrEmpty(result.levelTrials[i]))
            {
                GameObject rowObj = new GameObject("LevelRow_D" + d + sLevel);
                rowObj.transform.SetParent(resultsLevelDetailsContent, false);
                
                RectTransform rowRect = rowObj.AddComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0, 1);
                rowRect.anchorMax = new Vector2(1, 1);
                rowRect.pivot = new Vector2(0.5f, 1);
                rowRect.sizeDelta = new Vector2(0, 25);
                rowRect.anchoredPosition = new Vector2(0, -displayedLevels * 30);
                
                Text rowText = rowObj.AddComponent<Text>();
                float startDist = float.Parse(result.startingDistance);
                float startScale = float.Parse(result.startingShapeScale);
                int sizeValue;
                if (sLevel == "L")
                    sizeValue = d - (int)(startDist - startScale);
                else
                    sizeValue = d - (int)(startDist - startScale) - 1;
                rowText.text = $"{result.levelAccuracies[i]} :קויד זוחא, {result.levelAvgResponseTimes[i]} :הבוגת ןמז, {result.levelCorrectResponses[i]}/{result.levelTrials[i]} :<b>{sizeValue} לדוג , {d} קחרמ  </b>";
                rowText.color = Color.black;
                rowText.fontSize = 12;
                rowText.alignment = TextAnchor.MiddleRight;
                rowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                
                displayedLevels++;
            }
            
            if (sLevel == "L")
                sLevel = "S";
            else
            {
                sLevel = "L";
                d++;
            }
        }
        
        RectTransform contentRect = resultsLevelDetailsContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, displayedLevels * 30);
        }
    }

    //A function to load existing users from the CSV file and display them in the user selection panel
    void LoadUsersList()
    {
        //Clear previous items
        foreach (Transform child in userListScrollContent)
        {
            Destroy(child.gameObject);
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "user_details.csv");
        
        if (!File.Exists(csvPath))
            return;
        
        string[] lines = File.ReadAllLines(csvPath);
        
        //data rows reading
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 10) continue;
            
            string userID = fields[0];
            string userName = fields[1];
            
            CreateUserListItem(userID, userName, i - 1);
        }
    }

    //A function to create a single user item in the user selection list
    void CreateUserListItem(string userID, string userName, int index)
    {
        GameObject itemObj = new GameObject("UserItem_" + userID);
        itemObj.transform.SetParent(userListScrollContent, false);
        
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.pivot = new Vector2(0.5f, 1);
        itemRect.sizeDelta = new Vector2(-20, 50);
        itemRect.anchoredPosition = new Vector2(0, -index * 60);
        
        Image bgImage = itemObj.AddComponent<Image>();
        bgImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        
        Button itemButton = itemObj.AddComponent<Button>();
        string capturedID = userID;
        itemButton.onClick.AddListener(() => SelectExistingUser(capturedID, itemObj));
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(itemObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        Text text = textObj.AddComponent<Text>();
        text.text = $"{userID} - ז.ת: {ReverseHebrewText(userName)}";
        text.color = Color.black;
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleRight;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    //A function called when an existing user item is clicked
    void SelectExistingUser(string userID, GameObject itemObj)
    {
        selectedUserID = userID;
        isNewUser = false;
        
        //Highlight selected user
        foreach (Transform child in userListScrollContent)
        {
            Image bg = child.GetComponent<Image>();
            if (bg != null)
            {
                if (child.gameObject == itemObj)
                    bg.color = new Color(0.3f, 0.6f, 1f, 1f); //Blue highlight
                else
                    bg.color = new Color(0.95f, 0.95f, 0.95f, 1f); //Gray
            }
        }
        userSelectionNextButton.interactable = true;
    }

    //A function called when "New User" button is clicked
    void SelectNewUser()
    {
        selectedUserID = "";
        isNewUser = true;
        
        //Clear all fields for new user in info panel
        NameInput.text = "";
        IDInput.text = "";
        AgeInput.text = "";
        GenderDropdown.value = 0;
        DateYearDropDown.value = 0;
        DateMonthDropDown.value = 0;
        DateDayDropDown.value = 0;
        EyeDropDown.value = 0;
        
        //Go directly to info panel
        userSelectionPanel.SetActive(false);
        infoPannel.SetActive(true);
    }

    //A function to delete the selected user from the CSV files
    void DeleteSelectedUser()
    {
        if (string.IsNullOrEmpty(selectedUserID))
        {
            showMessage(".הקיחמל שמתשמ רוחבל אנ");
            return;
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        
        //Delete its row from user_details.csv
        string userDetailsPath = Path.Combine(csvFolder, "user_details.csv");
        if (File.Exists(userDetailsPath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(userDetailsPath));
            lines.RemoveAll(line => line.StartsWith(selectedUserID + ","));
            File.WriteAllLines(userDetailsPath, lines);
        }
        
        //Deleting all entries for the selected user from game_results.csv
        string gameResultsPath = Path.Combine(csvFolder, "game_results.csv");
        if (File.Exists(gameResultsPath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(gameResultsPath));
            List<string> filteredLines = new List<string>();
            filteredLines.Add(lines[0]);
            for (int i = 1; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith(selectedUserID + ","))
                {
                    filteredLines.Add(lines[i]);
                }
            }
            File.WriteAllLines(gameResultsPath, filteredLines);
        }
        
        showMessage(".קחמנ שמתשמה");
        
        // Clear selection and refresh list
        selectedUserID = "";
        userSelectionNextButton.interactable = false;
        LoadUsersList();
    }

    //A function to move to the user info panel
    void MoveToInfoPanel()
    {
        if (isNewUser)
        {
            //Clear all fields for new user
            NameInput.text = "";
            IDInput.text = "";
            AgeInput.text = "";
            GenderDropdown.value = 0;
            DateYearDropDown.value = 0;
            DateMonthDropDown.value = 0;
            DateDayDropDown.value = 0;
            EyeDropDown.value = 0;
        }
        else
        {
            // Load existing user data
            if (string.IsNullOrEmpty(selectedUserID))
            {
                showMessage(".שמתשמ רוחבל אנ");
                return;
            }
            LoadUserData(selectedUserID);
        }
        
        userSelectionPanel.SetActive(false);
        infoPannel.SetActive(true);
    }

    //A function to load existing user data into the info panel fields
    void LoadUserData(string userID)
    {
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "user_details.csv");
        
        if (!File.Exists(csvPath))
            return;
        
        string[] lines = File.ReadAllLines(csvPath);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length >= 10 && fields[0] == userID)
            {
                IDInput.text = fields[0];
                NameInput.text = fields[1];
                AgeInput.text = fields[2] == "N/A" ? "" : fields[2];
                GenderDropdown.value = fields[3] == "Male" ? 0 : 1;
                
                if (int.TryParse(fields[4], out int year))
                    DateYearDropDown.value = year;
                if (int.TryParse(fields[5], out int month))
                    DateMonthDropDown.value = month;
                if (int.TryParse(fields[6], out int day))
                    DateDayDropDown.value = day;
                EyeDropDown.value = 0;
                break;
            }
        }
    }

    //A function called when the search text is changed to filter the users list
    void OnSearchTextChanged(string searchText)
    {
        if (searchText.Length >= 3) //filters starts only when 3 or more characters are typed
        {
            FilterUsersList(searchText);
        }
        else if (searchText.Length == 0)
        {
            LoadUsersList(); //Show all users when search is cleared
        }
    }

    //A function to filter the users list based on search text
    void FilterUsersList(string searchText)
    {
        //Clear previous items
        foreach (Transform child in userListScrollContent)
        {
            Destroy(child.gameObject);
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "user_details.csv");
        
        if (!File.Exists(csvPath))
            return;
        
        string[] lines = File.ReadAllLines(csvPath);
        
        int displayIndex = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 10) continue;
            
            string userID = fields[0];
            string userName = fields[1];
            
            if (userName.ToLower().Contains(searchText.ToLower()))
            {
                CreateUserListItem(userID, userName, displayIndex);
                displayIndex++;
            }
        }
    }
    
    //A function to toggle the visibility of user sort options panel
    void ToggleUserSortOptions()
    {
        userSortOptionsPanel.SetActive(!userSortOptionsPanel.activeSelf);
    }

    //A function to sort the users list based on selected sort mode
    void SortUsers(string sortMode)
    {
        currentUserSortMode = sortMode;
        userSortOptionsPanel.SetActive(false);
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "user_details.csv");
        
        if (!File.Exists(csvPath))
            return;
        
        string[] lines = File.ReadAllLines(csvPath);
        List<UserData> users = new List<UserData>();
        
        //Parse all users
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 10) continue;
            
            users.Add(new UserData {
                userID = fields[0],
                userName = fields[1],
                lastUpdate = fields[9]
            });
        }
        
        //Sort based on mode
        if (sortMode == "name")
            users.Sort((a, b) => string.Compare(a.userName, b.userName));
        else if (sortMode == "id")
            users.Sort((a, b) => string.Compare(a.userID, b.userID));
        else if (sortMode == "latest")
            users.Sort((a, b) => string.Compare(b.lastUpdate, a.lastUpdate)); // Descending
        else if (sortMode == "oldest")
            users.Sort((a, b) => string.Compare(a.lastUpdate, b.lastUpdate)); // Ascending
        
        //Clear and rebuild list
        foreach (Transform child in userListScrollContent)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < users.Count; i++)
        {
            CreateUserListItem(users[i].userID, users[i].userName, i);
        }
    }

    //A function to show tooltip panel with given message near the specified button position
    void ShowTooltip(string message, Vector3 buttonPosition)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = message;
            tooltipPanel.SetActive(true);
            
            //Position tooltip near the button
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            tooltipRect.position = buttonPosition + new Vector3(-295, -65, 0);
        }
    }

    //A function to hide the tooltip panel
    void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        currentInfoButton = null;
    }

    //A function to setup a tooltip button with given name and tooltip message
    void SetupTooltipButton(string buttonName, string tooltipMessage)
    {
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        Button btn = null;
        
        foreach (Button button in allButtons)
        {
            if (button.gameObject.name == buttonName)
            {
                btn = button;
                break;
            }
        }
        
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnInfoButtonClick(btn, tooltipMessage));
            infoButtons.Add(btn);
        }
        else
        {
            Debug.LogWarning("Tooltip button not found: " + buttonName);
        }
    }

    //A function to update the tooltip and handle clicks outside to close it
    void Update()
    {
        //Close tooltip when clicking outside
        if (tooltipPanel != null && tooltipPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            //Check if click was outside tooltip and info buttons
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                tooltipPanel.GetComponent<RectTransform>(), 
                Input.mousePosition))
            {
                bool clickedInfoButton = false;
                foreach (Button btn in infoButtons)
                {
                    if (btn != null && RectTransformUtility.RectangleContainsScreenPoint(
                        btn.GetComponent<RectTransform>(), 
                        Input.mousePosition))
                    {
                        clickedInfoButton = true;
                        break;
                    }
                }
                
                if (!clickedInfoButton)
                {
                    HideTooltip();
                }
            }
        }
    }

    //A function called when an info button is clicked to show/hide tooltip
    void OnInfoButtonClick(Button button, string tooltipMessage)
    {
        if (currentInfoButton == button)
        {
            //Clicking same button again - hide tooltip
            HideTooltip();
        }
        else
        {
            //Show new tooltip
            currentInfoButton = button;
            ShowTooltip(tooltipMessage, button.transform.position);
        }
    }

    //A function to exit the application
    void ExitApplication()
    {
        Application.Quit();
    }

    //A function to reverse Hebrew text
    string ReverseHebrewText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        //Check if text contains Hebrew characters
        bool hasHebrew = false;
        foreach (char c in text)
        {
            if (c >= 0x0590 && c <= 0x05FF)
            {
                hasHebrew = true;
                break;
            }
        }
        
        //if no Hebrew, return as-is
        if (!hasHebrew)
            return text;
        
        char[] charArray = text.ToCharArray();
        System.Array.Reverse(charArray);
        string reversed = new string(charArray);
        
        //Swap parentheses back to their original positions, they should not be reversed
        reversed = reversed.Replace(')', '\u0001'); 
        reversed = reversed.Replace('(', ')');
        reversed = reversed.Replace('\u0001', '(');
        
        return reversed;
    }

    //A function to calculate age automatically when date of birth is inserted
    void CalculateAndDisplayAge()
    {
        int yearIndex = DateYearDropDown.value;
        int monthIndex = DateMonthDropDown.value;
        int dayIndex = DateDayDropDown.value;
        
        //Validation that all fields are inserted
        if (yearIndex == 0 || monthIndex == 0 || dayIndex == 0)
            return;
        
        try
        {
            //Getting values from dropdowns
            string yearText = DateYearDropDown.options[yearIndex].text;
            string monthText = DateMonthDropDown.options[monthIndex].text;
            string dayText = DateDayDropDown.options[dayIndex].text;
            
            int year = int.Parse(yearText);
            int month = int.Parse(monthText);
            int day = int.Parse(dayText);
            
            //creating the date
            System.DateTime birthDate = new System.DateTime(year, month, day);
            System.DateTime today = System.DateTime.Today;
            
            //calculating age
            double ageInDays = (today - birthDate).TotalDays;
            double age = ageInDays / 365.25;
            
            //Display age with 1 decimal place
            AgeInput.text = age.ToString("F1");
        }
        catch (System.Exception)
        {
            AgeInput.text = "";
        }
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
    public float maxDistance = 10f;
    public float shapeScale = 0.04f;
    public float successRate = 80f;
    public float failRate = 20f;
    public int chunkSize = 15;

    public List<int> imageSets = new List<int>();

    //User details
    public string userID;
    public int trainingEye; 
    public string sessionTimestamp;
}

[System.Serializable]
public class SessionResults
{
    public string userID;
    public string sessionTimestamp;
    public string eyeTrained;
    public string levelProgression;
    public float overallAccuracy;
    public float overallAvgResponseTime;
    public int totalTrials;
    public int correctResponses;
    public List<LevelResult> levelResults = new List<LevelResult>();
}

[System.Serializable]
public class LevelResult
{
    public string levelName;
    public float accuracy;
    public float avgResponseTime;
    public int trials;
    public int correctResponses;
}

[System.Serializable]
public class GameResult
{
    public string userID;
    public string timestamp;
    public string eyeTrained;
    public string testDuration;
    public string focusY;
    public string focusScale;
    public string focusShape;
    public string shapeDisplayDuration;
    public string betweenShapesDuration;
    public string focusChangeMode;
    public string intervalSets;
    public string successRate;
    public string failRate;
    public string chunkSize;
    public string startingDistance;
    public string startingShapeScale;
    public string overallAccuracy;
    public string overallAvgResponseTime;
    public string overallTrials;
    public string overallCorrectResponses;
    
    public List<string> levelAccuracies = new List<string>();
    public List<string> levelAvgResponseTimes = new List<string>();
    public List<string> levelTrials = new List<string>();
    public List<string> levelCorrectResponses = new List<string>();
    
    public string levelProgression;
    
    public int csvLineIndex;
}

[System.Serializable]
public class UserData
{
    public string userID;
    public string userName;
    public string lastUpdate;
}