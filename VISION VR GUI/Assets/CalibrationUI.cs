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
    public Button currentInfoButton = null;

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
   public string selectedConfigToLoad = "";

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
    public string currentUserSortMode = "latest"; //default
    public string selectedUserID = "";
    public bool isNewUser = true;
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
    public Vector2 normalPopupOffsetMin = new Vector2(600, 350);
    public Vector2 normalPopupOffsetMax = new Vector2(-600, -350);
    public Vector2 expandedPopupOffsetMin = new Vector2(600, 300);
    public Vector2 expandedPopupOffsetMax = new Vector2(-600, -300);
    public bool isResultsExpanded = false;

    // Results browser variables 
    public Button showResultsButton;
    public GameObject resultsListPanel;
    public Transform resultsListScrollContent;
    public Button resultsListCancelButton;
    public Button resultsListRemoveButton;
    public Button resultsListExpandButton;
    public List<GameResult> allGameResults = new List<GameResult>();
    public GameObject selectedResultItem = null;
    public int selectedResultIndex = -1;

    // Helper class instances
    private UIHelper uiHelper;
    private ConfigurationManager configManager;
    private UserManager userManager;
    private ResultsManager resultsManager;

    void Start()
    {
        // Initialize helper classes
        uiHelper = new UIHelper(this);
        configManager = new ConfigurationManager(this, uiHelper);
        userManager = new UserManager(this, uiHelper);
        resultsManager = new ResultsManager(this, uiHelper);

        //Main UI Panel Listeners
        //Elements/Options listeners
        startingDistanceSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateSliderValueDisplay(); });
        startingDistanceSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateMaxDistanceRange(); });
        maxDistanceSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateSliderValueDisplay(); });
        shapeSizeSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateSliderValueDisplay(); });
        focusYSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateSliderValueDisplay(); });
        focusScaleSlider.onValueChanged.AddListener(delegate { uiHelper.UpdateSliderValueDisplay(); });
        focusChangeDropdown.onValueChanged.AddListener(delegate { OnFocusChangeDropdownChanged(); }); //focus point change
        uiHelper.UpdateSliderValueDisplay(); //initial display
        uiHelper.UpdateMaxDistanceRange();
        OnFocusChangeDropdownChanged();
        focusScaleSlider.minValue = 1f;
        focusScaleSlider.maxValue = 10f;
        focusScaleSlider.value = 8f;  

        // buttons listeners
        saveConfigButton.onClick.AddListener(configManager.ShowSaveDialog);
        loadConfigButton.onClick.AddListener(configManager.ShowLoadDialog);
        continueButton.onClick.AddListener(ShowInfoPanel);
        StartButton.onClick.AddListener(SaveSettingsAndClose);

        //save and load dialogs buttons listeners
        saveDialogSaveButton.onClick.AddListener(configManager.SaveConfigurationWithName);
        saveDialogCancelButton.onClick.AddListener(configManager.HideSaveDialog);
        loadDialogDeleteButton.onClick.AddListener(configManager.DeleteSelectedConfiguration);
        loadDialogLoadButton.onClick.AddListener(configManager.LoadSelectedConfiguration);
        loadDialogCancelButton.onClick.AddListener(configManager.HideLoadDialog);

        //Message dialog listener
        MessageOkButton.onClick.AddListener(() => MessageDialogPanel.SetActive(false));


        //User selection panel listeners
        exitButton.onClick.AddListener(ExitApplication);
        userSearchInput.onValueChanged.AddListener(userManager.OnSearchTextChanged);
        newUserButton.onClick.AddListener(userManager.SelectNewUser);
        userSelectionNextButton.onClick.AddListener(userManager.MoveToInfoPanel);
        userSortButton.onClick.AddListener(userManager.ToggleUserSortOptions);
        userDeleteButton.onClick.AddListener(userManager.DeleteSelectedUser);
        userSortByNameButton.onClick.AddListener(() => userManager.SortUsers("name"));
        userSortByIDButton.onClick.AddListener(() => userManager.SortUsers("id"));
        userSortByLatestButton.onClick.AddListener(() => userManager.SortUsers("latest"));
        userSortByOldestButton.onClick.AddListener(() => userManager.SortUsers("oldest"));
        userManager.LoadUsersList();
        userSelectionNextButton.interactable = false;


        //User Info Panel listeners
        //Elements listeners
        DateYearDropDown.onValueChanged.AddListener(delegate { uiHelper.CalculateAndDisplayAge(); });
        DateMonthDropDown.onValueChanged.AddListener(delegate { uiHelper.CalculateAndDisplayAge(); });
        DateDayDropDown.onValueChanged.AddListener(delegate { uiHelper.CalculateAndDisplayAge(); });
        UIPreviousButton.onClick.AddListener(ReturnToUIPanel);
        InfoPreviousButton.onClick.AddListener(ReturnToUserSelection);

        //Results list buttons listeners
        showResultsButton.onClick.AddListener(() => resultsManager.ShowResultsList(selectedUserID));
        resultsListCancelButton.onClick.AddListener(resultsManager.HideResultsList);
        resultsListRemoveButton.onClick.AddListener(resultsManager.RemoveSelectedResult);
        resultsListExpandButton.onClick.AddListener(resultsManager.ExpandSelectedResult);


        //Main UI tooltips
        uiHelper.SetupTooltipButton("TimeInputInfoButton", uiHelper.ReverseHebrewText("משך האימון הכולל (בדקות)."));
        uiHelper.SetupTooltipButton("ShapeDurationInputInfoButton", uiHelper.ReverseHebrewText("משך הצגת סט של תמונות (במילישניות)."));
        uiHelper.SetupTooltipButton("BetweenShapeDurationInputInfoButton", uiHelper.ReverseHebrewText("משך ההמתנה בין שני סטים של תמונות (במילישניות)."));
        uiHelper.SetupTooltipButton("StartingDistanceSliderInfoButton", uiHelper.ReverseHebrewText("המרחק ההתחלתי של התמונות מנקודת המיקוד."));
        uiHelper.SetupTooltipButton("MaxDistanceSliderInfoButton", uiHelper.ReverseHebrewText("המרחק המרבי מנקודת המיקוד שהתמונות יכולות להגיע אליו."));
        uiHelper.SetupTooltipButton("ShapeSizeSliderInfoButton", uiHelper.ReverseHebrewText("גודל התמונות ההתחלתי. "));
        uiHelper.SetupTooltipButton("ImageSetScrollViewInfoButton", uiHelper.ReverseHebrewText("קטגוריות התמונות שיופיעו במהך האימון."));
        uiHelper.SetupTooltipButton("FocusShapeDropdownInfoButton", uiHelper.ReverseHebrewText("צורת נקודת המיקוד."));
        uiHelper.SetupTooltipButton("FocusYSliderInfoButton", uiHelper.ReverseHebrewText("גובה נקודת המיקוד."));
        uiHelper.SetupTooltipButton("FocusScaleSliderInfoButton", uiHelper.ReverseHebrewText("גודל נקודת המיקוד."));
        uiHelper.SetupTooltipButton("FocusPointLocationChangeDropDownInfoButton", uiHelper.ReverseHebrewText("מצב שינוי מיקום נקודת המיקוד. סטטי, משתנה במרווחים קבועים או אקראיים."));
        uiHelper.SetupTooltipButton("FocusPointLocationChangeIntervalDropdownInfoButton", uiHelper.ReverseHebrewText("תדירות שינוי נקודת המיקוד (באינטרוולים). רלוונטי במרווחים קבועים."));
        uiHelper.SetupTooltipButton("chunkSizeInputInfoButton", uiHelper.ReverseHebrewText("מספר הסטים בהם מעריכים את הביצועים לפני החלטה על שינוי ברמה."));
        uiHelper.SetupTooltipButton("SuccessRateInputInfoButton", uiHelper.ReverseHebrewText("אחוז התשובות הנכונות הנדרש כדי לעלות רמה."));
        uiHelper.SetupTooltipButton("FailRateInputInfoButton", uiHelper.ReverseHebrewText("אחוז התשובות השגויות שבו יורדים רמה."));

        resultsManager.CheckAndDisplaySessionResults();
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
            uiHelper.showMessage(".דחא לפל רוחבל שי - תונומת יטס");
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
            uiHelper.showMessage(".תוהזה תדועת רפסמ תא ןיזהל אנ");
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
       
       userManager.SaveUserDetailsToCSV(userName, userID, age, GenderDropdown.value, DateYearDropDown.value, DateMonthDropDown.value, DateDayDropDown.value, EyeDropDown.value, timestamp);
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

    //A function to exit the application
    void ExitApplication()
    {
        Application.Quit();
    }

    //A function to update the tooltip and handle clicks outside to close it
    void Update()
    {
        uiHelper.UpdateTooltips();
    }
}


public class UIHelper
{
    private CalibrationUI mainUI;

    public UIHelper(CalibrationUI ui)
    {
        mainUI = ui;
    }

    //A function to update the slider value displays
    public void UpdateSliderValueDisplay()
    {
        if (mainUI.startingDistanceValueText != null)
            mainUI.startingDistanceValueText.text = mainUI.startingDistanceSlider.value.ToString();

        if (mainUI.maxDistanceValueText != null)
            mainUI.maxDistanceValueText.text = mainUI.maxDistanceSlider.value.ToString();
        
        if (mainUI.shapeSizeValueText != null)
            mainUI.shapeSizeValueText.text = mainUI.shapeSizeSlider.value.ToString();
            
        if (mainUI.focusYValueText != null)
            mainUI.focusYValueText.text = mainUI.focusYSlider.value.ToString();

        if (mainUI.focusScaleValueText != null)
            mainUI.focusScaleValueText.text = mainUI.focusScaleSlider.value.ToString();
    }

    //A function to update the max distance slider range based on starting distance
    //Functionality: Goal distance can't be less than starting distance
    public void UpdateMaxDistanceRange()
    {
        float startingDist = mainUI.startingDistanceSlider.value;
        mainUI.maxDistanceSlider.minValue = startingDist;
        mainUI.maxDistanceSlider.maxValue = 10f;
        
        //Ensuring current value is within new range
        if (mainUI.maxDistanceSlider.value < startingDist)
            mainUI.maxDistanceSlider.value = startingDist;
    }

    //A function to show a message dialog with the given message
    public void showMessage(string message)
    {
        mainUI.MessageText.text = message;
        mainUI.MessageDialogPanel.SetActive(true);
    }

    //A function to reverse Hebrew text
    public string ReverseHebrewText(string text)
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
    public void CalculateAndDisplayAge()
    {
        int yearIndex = mainUI.DateYearDropDown.value;
        int monthIndex = mainUI.DateMonthDropDown.value;
        int dayIndex = mainUI.DateDayDropDown.value;
        
        //Validation that all fields are inserted
        if (yearIndex == 0 || monthIndex == 0 || dayIndex == 0)
            return;
        
        try
        {
            //Getting values from dropdowns
            string yearText = mainUI.DateYearDropDown.options[yearIndex].text;
            string monthText = mainUI.DateMonthDropDown.options[monthIndex].text;
            string dayText = mainUI.DateDayDropDown.options[dayIndex].text;
            
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
            mainUI.AgeInput.text = age.ToString("F1");
        }
        catch (System.Exception)
        {
            mainUI.AgeInput.text = "";
        }
    }

    //A function to show tooltip panel with given message near the specified button position
    public void ShowTooltip(string message, Vector3 buttonPosition)
    {
        if (mainUI.tooltipPanel != null && mainUI.tooltipText != null)
        {
            mainUI.tooltipText.text = message;
            mainUI.tooltipPanel.SetActive(true);
            
            //Position tooltip near the button
            RectTransform tooltipRect = mainUI.tooltipPanel.GetComponent<RectTransform>();
            tooltipRect.position = buttonPosition + new Vector3(-295, -65, 0);
        }
    }

    //A function to hide the tooltip panel
    public void HideTooltip()
    {
        if (mainUI.tooltipPanel != null)
        {
            mainUI.tooltipPanel.SetActive(false);
        }
        mainUI.currentInfoButton = null;
    }

    //A function to setup a tooltip button with given name and tooltip message
    public void SetupTooltipButton(string buttonName, string tooltipMessage)
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
            mainUI.infoButtons.Add(btn);
        }
        else
        {
            Debug.LogWarning("Tooltip button not found: " + buttonName);
        }
    }

    //A function called when an info button is clicked to show/hide tooltip
    public void OnInfoButtonClick(Button button, string tooltipMessage)
    {
        if (mainUI.currentInfoButton == button)
        {
            //Clicking same button again - hide tooltip
            HideTooltip();
        }
        else
        {
            //Show new tooltip
            mainUI.currentInfoButton = button;
            ShowTooltip(tooltipMessage, button.transform.position);
        }
    }

    //A function to update the tooltip and handle clicks outside to close it
    public void UpdateTooltips()
    {
        //Close tooltip when clicking outside
        if (mainUI.tooltipPanel != null && mainUI.tooltipPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            //Check if click was outside tooltip and info buttons
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                mainUI.tooltipPanel.GetComponent<RectTransform>(), 
                Input.mousePosition))
            {
                bool clickedInfoButton = false;
                foreach (Button btn in mainUI.infoButtons)
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
}


public class ConfigurationManager
{
    private CalibrationUI mainUI;
    private UIHelper uiHelper;

    public ConfigurationManager(CalibrationUI ui, UIHelper helper)
    {
        mainUI = ui;
        uiHelper = helper;
    }

    //A function that shows the save configuration dialog
    public void ShowSaveDialog()
    {
        mainUI.saveConfigNameInput.text = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); //cfg name by default is the date
        mainUI.saveDialogPanel.SetActive(true);

        //select all the text 
        mainUI.saveConfigNameInput.Select();
        mainUI.saveConfigNameInput.ActivateInputField();
    }

    //A function that hides the save configuration dialog
    public void HideSaveDialog()
    {
        mainUI.saveDialogPanel.SetActive(false);
    }

    //A function responsible for saving the configuration
    public void SaveConfigurationWithName()
    {
        string configName = mainUI.saveConfigNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(configName))
        {
            uiHelper.showMessage(".תורדגהה טסל םש רוחבל אנ");
            return;
        }
        
        VRSettings settings = new VRSettings();
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Durations: game, set display, and between sets.
        if (float.TryParse(mainUI.timeInput.text, out float minutes))
            settings.gameDuration = minutes * 60f;
        if (float.TryParse(mainUI.betweenShapesDuration.text, out float betweenDuration))
            settings.betweenShapesDuration = betweenDuration;
        if (float.TryParse(mainUI.shapeDisplayDuration.text, out float duration))
            settings.shapeDisplayDuration = duration;
            
        settings.startingDistance = mainUI.startingDistanceSlider.value;
        settings.maxDistance = mainUI.maxDistanceSlider.value;
        settings.shapeScale = mainUI.shapeSizeSlider.value * 0.0036f + 0.004f;

        //Focus point settings
        settings.focusY = mainUI.focusYSlider.value / 100f;
        settings.focusScale = mainUI.focusScaleSlider.value / 100f;
        settings.focusShape = mainUI.focusShapeDropdown.value;
        settings.focusChangeMode = mainUI.focusChangeDropdown.value;
        settings.intervalSets = mainUI.intervalSetsDropdown.value + 1;

        //Success/Fail rates and chunk size
        if (float.TryParse(mainUI.successRateInput.text, out float successRate))
            settings.successRate = successRate;
        else
            settings.successRate = 80f;
            
        if (float.TryParse(mainUI.failRateInput.text, out float failRate))
            settings.failRate = failRate;
        else
            settings.failRate = 20f;
            
        if (int.TryParse(mainUI.chunkSizeInput.text, out int chunkSize))
            settings.chunkSize = chunkSize;
        else
            settings.chunkSize = 15;

        //Image sets selection
        for (int i = 0; i < mainUI.imageSetToggles.Count; i++)
        {
            if (mainUI.imageSetToggles[i].isOn)
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
        
        uiHelper.showMessage(".הלחצהב רמשנ תורדגהה טס");
        HideSaveDialog();
    }

    //A function that shows the load dialog with the available configurations to load
    public void ShowLoadDialog()
    {
        //Clear previous buttons
        foreach (Transform child in mainUI.loadDialogContent)
        {
            Object.Destroy(child.gameObject);
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
            toggleObj.transform.SetParent(mainUI.loadDialogContent, false);
            
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
                    if (mainUI.selectedConfigToLoad == capturedName)
                    {
                        mainUI.selectedConfigToLoad = "";
                    }
                }
            });
            
            index++;
        }
        
        mainUI.loadDialogPanel.SetActive(true);
        mainUI.selectedConfigToLoad = "";
    }

    //A function to handle selection of a configuration set in the load dialog
    void SelectConfig(string configName)
    {
        mainUI.selectedConfigToLoad = configName;
        
        //Highlight selected and unhighlight others
        foreach (Transform child in mainUI.loadDialogContent)
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
    public void HideLoadDialog()
    {
        mainUI.loadDialogPanel.SetActive(false);
    }

    //A function that handles loading the selected configuration
    public void LoadSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(mainUI.selectedConfigToLoad))
        {
            uiHelper.showMessage(".הניעטל תורדגה טס רוחבל אנ");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        string path = Path.Combine(configFolder, mainUI.selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            //Load durations
            mainUI.timeInput.text = (settings.gameDuration / 60f).ToString();
            mainUI.betweenShapesDuration.text = settings.betweenShapesDuration.ToString();
            mainUI.shapeDisplayDuration.text = settings.shapeDisplayDuration.ToString();
            
            //Load sliders
            mainUI.startingDistanceSlider.value = settings.startingDistance;
            mainUI.maxDistanceSlider.value = settings.maxDistance;
            uiHelper.UpdateMaxDistanceRange();
            mainUI.shapeSizeSlider.value = (settings.shapeScale - 0.004f) / 0.0036f;
            mainUI.focusYSlider.value = settings.focusY * 100f;
            mainUI.focusScaleSlider.value = settings.focusScale * 100f;
            
            //Load dropdowns
            mainUI.focusShapeDropdown.value = settings.focusShape;
            mainUI.focusChangeDropdown.value = settings.focusChangeMode;
            mainUI.intervalSetsDropdown.value = settings.intervalSets - 1;
            
            //Load success/fail rates and chunk size
            mainUI.successRateInput.text = settings.successRate.ToString();
            mainUI.failRateInput.text = settings.failRate.ToString();
            mainUI.chunkSizeInput.text = settings.chunkSize.ToString();
            
            //Load image sets toggles
            for (int i = 0; i < mainUI.imageSetToggles.Count; i++)
            {
                mainUI.imageSetToggles[i].isOn = false;
            }
            foreach (int setNumber in settings.imageSets)
            {
                if (setNumber >= 1 && setNumber <= mainUI.imageSetToggles.Count)
                {
                    mainUI.imageSetToggles[setNumber - 1].isOn = true;
                }
            }
            
            uiHelper.showMessage(".ןעטנ תורדגהה טס");
            HideLoadDialog();
        }
        else
        {
            uiHelper.showMessage(".אצמנ אל תורדגהה ץבוק");
        }
    }

    //A function that deletes the configuration set selected to be deleted
    public void DeleteSelectedConfiguration()
    {
        if (string.IsNullOrEmpty(mainUI.selectedConfigToLoad))
        {
            uiHelper.showMessage(".הקיחמל תורדגה טס רוחבל אנ");
            return;
        }
        
        string configFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "Configs");        string path = Path.Combine(configFolder, mainUI.selectedConfigToLoad + ".json");
        
        if (File.Exists(path))
        {
            //Delete from disk
            File.Delete(path);
            uiHelper.showMessage(".קחמנ תורדגהה טס");
            
            //Remove from GUI 
            foreach (Transform child in mainUI.loadDialogContent)
            {
                if (child.name == mainUI.selectedConfigToLoad)
                {
                    Object.DestroyImmediate(child.gameObject);
                    break;
                }
            }
            
            //Reposition remaining toggles
            int index = 0;
            foreach (Transform child in mainUI.loadDialogContent)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -index * 35);
                    index++;
                }
            }
            
            //Clear selection
            mainUI.selectedConfigToLoad = "";
        }
        else
        {
            uiHelper.showMessage(".אצמנ אל תורדגהה ץבוק");
        }
    }
}


public class UserManager
{
    private CalibrationUI mainUI;
    private UIHelper uiHelper;

    public UserManager(CalibrationUI ui, UIHelper helper)
    {
        mainUI = ui;
        uiHelper = helper;
    }

    //A function to load existing users from the CSV file and display them in the user selection panel
    public void LoadUsersList()
    {
        //Clear previous items
        foreach (Transform child in mainUI.userListScrollContent)
        {
            Object.Destroy(child.gameObject);
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
        itemObj.transform.SetParent(mainUI.userListScrollContent, false);
        
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
        text.text = $"{userID} - ז.ת: {uiHelper.ReverseHebrewText(userName)}";
        text.color = Color.black;
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleRight;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    //A function called when an existing user item is clicked
    void SelectExistingUser(string userID, GameObject itemObj)
    {
        mainUI.selectedUserID = userID;
        mainUI.isNewUser = false;
        
        //Highlight selected user
        foreach (Transform child in mainUI.userListScrollContent)
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
        mainUI.userSelectionNextButton.interactable = true;
    }

    //A function called when "New User" button is clicked
    public void SelectNewUser()
    {
        mainUI.selectedUserID = "";
        mainUI.isNewUser = true;
        
        //Clear all fields for new user in info panel
        mainUI.NameInput.text = "";
        mainUI.IDInput.text = "";
        mainUI.AgeInput.text = "";
        mainUI.GenderDropdown.value = 0;
        mainUI.DateYearDropDown.value = 0;
        mainUI.DateMonthDropDown.value = 0;
        mainUI.DateDayDropDown.value = 0;
        mainUI.EyeDropDown.value = 0;
        
        //Go directly to info panel
        mainUI.userSelectionPanel.SetActive(false);
        mainUI.infoPannel.SetActive(true);
    }

    //A function to delete the selected user from the CSV files
    public void DeleteSelectedUser()
    {
        if (string.IsNullOrEmpty(mainUI.selectedUserID))
        {
            uiHelper.showMessage(".הקיחמל שמתשמ רוחבל אנ");
            return;
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        
        //Delete its row from user_details.csv
        string userDetailsPath = Path.Combine(csvFolder, "user_details.csv");
        if (File.Exists(userDetailsPath))
        {
            List<string> lines = new List<string>(File.ReadAllLines(userDetailsPath));
            lines.RemoveAll(line => line.StartsWith(mainUI.selectedUserID + ","));
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
                if (!lines[i].StartsWith(mainUI.selectedUserID + ","))
                {
                    filteredLines.Add(lines[i]);
                }
            }
            File.WriteAllLines(gameResultsPath, filteredLines);
        }
        
        uiHelper.showMessage(".קחמנ שמתשמה");
        
        // Clear selection and refresh list
        mainUI.selectedUserID = "";
        mainUI.userSelectionNextButton.interactable = false;
        LoadUsersList();
    }

    //A function to move to the user info panel
    public void MoveToInfoPanel()
    {
        if (mainUI.isNewUser)
        {
            //Clear all fields for new user
            mainUI.NameInput.text = "";
            mainUI.IDInput.text = "";
            mainUI.AgeInput.text = "";
            mainUI.GenderDropdown.value = 0;
            mainUI.DateYearDropDown.value = 0;
            mainUI.DateMonthDropDown.value = 0;
            mainUI.DateDayDropDown.value = 0;
            mainUI.EyeDropDown.value = 0;
        }
        else
        {
            // Load existing user data
            if (string.IsNullOrEmpty(mainUI.selectedUserID))
            {
                uiHelper.showMessage(".שמתשמ רוחבל אנ");
                return;
            }
            LoadUserData(mainUI.selectedUserID);
        }
        
        mainUI.userSelectionPanel.SetActive(false);
        mainUI.infoPannel.SetActive(true);
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
                mainUI.IDInput.text = fields[0];
                mainUI.NameInput.text = fields[1];
                mainUI.AgeInput.text = fields[2] == "N/A" ? "" : fields[2];
                mainUI.GenderDropdown.value = fields[3] == "Male" ? 0 : 1;
                
                if (int.TryParse(fields[4], out int year))
                    mainUI.DateYearDropDown.value = year;
                if (int.TryParse(fields[5], out int month))
                    mainUI.DateMonthDropDown.value = month;
                if (int.TryParse(fields[6], out int day))
                    mainUI.DateDayDropDown.value = day;
                mainUI.EyeDropDown.value = 0;
                break;
            }
        }
    }

    //A function called when the search text is changed to filter the users list
    public void OnSearchTextChanged(string searchText)
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
        foreach (Transform child in mainUI.userListScrollContent)
        {
            Object.Destroy(child.gameObject);
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
    public void ToggleUserSortOptions()
    {
        mainUI.userSortOptionsPanel.SetActive(!mainUI.userSortOptionsPanel.activeSelf);
    }

    //A function to sort the users list based on selected sort mode
    public void SortUsers(string sortMode)
    {
        mainUI.currentUserSortMode = sortMode;
        mainUI.userSortOptionsPanel.SetActive(false);
        
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
        foreach (Transform child in mainUI.userListScrollContent)
        {
            Object.Destroy(child.gameObject);
        }
        
        for (int i = 0; i < users.Count; i++)
        {
            CreateUserListItem(users[i].userID, users[i].userName, i);
        }
    }

    //A function to save user details to a CSV file
    public void SaveUserDetailsToCSV(string userName, string userID, int userAge, int userGender, int birthYear, int birthMonth, int birthDay, int trainingEye, string timestamp)
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
}


public class ResultsManager
{
    private CalibrationUI mainUI;
    private UIHelper uiHelper;

    public ResultsManager(CalibrationUI ui, UIHelper helper)
    {
        mainUI = ui;
        uiHelper = helper;
    }

    //A method to check for session results and display them
    public void CheckAndDisplaySessionResults()
    {
        string flagPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "show_latest_result.flag");
        
        //Only show results if flag file exists
        if (!File.Exists(flagPath))
            return;
        
        //Delete flag
        File.Delete(flagPath);
        
        //Load results from CSV
        LoadGameResultsFromCSV();
        
        if (mainUI.allGameResults.Count == 0)
            return;
        
        //Set the latest result as selected and expand it
        mainUI.selectedResultIndex = 0;
        ExpandSelectedResult();
    }

    //A function to show the results list panel
    public void ShowResultsList(string selectedUserID)
    {
        LoadGameResultsFromCSV();
        mainUI.allGameResults.RemoveAll(result => result.userID != selectedUserID); // keep only user results
        DisplayResultsInList();
        
        // Show cancel, hide remove and expand
        mainUI.resultsListCancelButton.gameObject.SetActive(true);
        mainUI.resultsListRemoveButton.gameObject.SetActive(false);
        mainUI.resultsListExpandButton.gameObject.SetActive(false);
        
        mainUI.selectedResultItem = null;
        mainUI.selectedResultIndex = -1;
        
        mainUI.resultsListPanel.SetActive(true);
    }

    public void HideResultsList()
    {
        mainUI.resultsListPanel.SetActive(false);
    }

    //A function to load game results from the CSV file after game ended
    void LoadGameResultsFromCSV()
    {
        mainUI.allGameResults.Clear();
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "game_results.csv");
        
        if (!File.Exists(csvPath))
        {
            uiHelper.showMessage(".אצמנ אל תואצות ץבוק");
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
            
            mainUI.allGameResults.Add(result);
        }
        mainUI.allGameResults.Sort((a, b) => string.Compare(b.timestamp, a.timestamp)); // sort by timestamp
    }

    //A function to display the loaded results in the scrollable list
    void DisplayResultsInList()
    {
        // Clear previous items
        foreach (Transform child in mainUI.resultsListScrollContent)
        {
            Object.Destroy(child.gameObject);
        }
        
        if (mainUI.allGameResults.Count == 0)
        {
            //Show "no results" message
            GameObject noDataObj = new GameObject("NoResults");
            noDataObj.transform.SetParent(mainUI.resultsListScrollContent, false);
            
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
        for (int i = 0; i < mainUI.allGameResults.Count; i++)
        {
            CreateResultItem(mainUI.allGameResults[i], i);
        }
        
        //updating content size
        RectTransform contentRect = mainUI.resultsListScrollContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, mainUI.allGameResults.Count * 80);
        }
    }

    //A function to create a single result item in the results list
    void CreateResultItem(GameResult result, int index)
    {
        GameObject itemObj = new GameObject("ResultItem_" + index);
        itemObj.transform.SetParent(mainUI.resultsListScrollContent, false);
        
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
        if (mainUI.selectedResultItem != null && mainUI.selectedResultItem != itemObj)
        {
            Image prevBg = mainUI.selectedResultItem.GetComponent<Image>();
            if (prevBg != null) prevBg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        }
        
        //Select current item
        mainUI.selectedResultItem = itemObj;
        mainUI.selectedResultIndex = index;
        
        Image bg = itemObj.GetComponent<Image>();
        if (bg != null) bg.color = new Color(0.8f, 0.9f, 1f, 1f);
        
        //Show remove and expand buttons
        mainUI.resultsListRemoveButton.gameObject.SetActive(true);
        mainUI.resultsListExpandButton.gameObject.SetActive(true);
    }

    //A function to remove the selected result from the CSV file
    public void RemoveSelectedResult()
    {
        if (mainUI.selectedResultIndex < 0 || mainUI.selectedResultIndex >= mainUI.allGameResults.Count)
        {
            uiHelper.showMessage(".הקיחמל טלפ רוחבל אנ");
            return;
        }
        
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        string csvPath = Path.Combine(csvFolder, "game_results.csv");
        
        if (!File.Exists(csvPath))
        {
            uiHelper.showMessage(".אצמנ אל תואצות ץבוק");
            return;
        }
        
        List<string> lines = new List<string>(File.ReadAllLines(csvPath));
        
        //removing the selected result line
        int lineToRemove = mainUI.allGameResults[mainUI.selectedResultIndex].csvLineIndex;
        if (lineToRemove < lines.Count)
        {
            lines.RemoveAt(lineToRemove);
            
            File.WriteAllLines(csvPath, lines);
            
            uiHelper.showMessage(".קחמנ טלפה");
            
            //refreshing the display
            LoadGameResultsFromCSV();
            DisplayResultsInList();
            
            //hiding remove and expand buttons
            mainUI.resultsListRemoveButton.gameObject.SetActive(false);
            mainUI.resultsListExpandButton.gameObject.SetActive(false);
            
            mainUI.selectedResultItem = null;
            mainUI.selectedResultIndex = -1;
        }
    }

    //A function to expand and show detailed data of the selected result
    public void ExpandSelectedResult()
    {
        if (mainUI.selectedResultIndex < 0 || mainUI.selectedResultIndex >= mainUI.allGameResults.Count)
        {
            uiHelper.showMessage(".הרחבל טלפ רוחבל אנ");
            return;
        }
        
        GameResult result = mainUI.allGameResults[mainUI.selectedResultIndex];
        
        if (mainUI.resultsPanel == null)
        {
            uiHelper.showMessage(".הרחב חולל ןיא");
            return;
        }
        
        string eyeDisplay = result.eyeTrained == "Right" ? "ןימי" : (result.eyeTrained == "Left" ? "לאמש" : result.eyeTrained);

        if (mainUI.resultsUserIDText != null)
            mainUI.resultsUserIDText.text = result.userID + " <b>:ז.ת</b>";

        if (mainUI.resultsTimestampText != null)
            mainUI.resultsTimestampText.text = result.timestamp + " <b>:הקידבה ןמז</b>";

        if (mainUI.resultsEyeText != null)
            mainUI.resultsEyeText.text = eyeDisplay + " <b>:תנמואמ ןיע</b>";

        if (mainUI.resultsTestDurationText != null)
            mainUI.resultsTestDurationText.text = result.testDuration + " <b>:(תוקד) הקידבה ךשמ</b>";

        if (mainUI.resultsFocusPositionText != null)
            mainUI.resultsFocusPositionText.text = result.focusY + " <b>:דוקימ תדוקנ םוקימ</b>";

        if (mainUI.resultsFocusScaleText != null)
            mainUI.resultsFocusScaleText.text = result.focusScale + " <b>:דוקימ תדוקנ לדוג</b>";

        if (mainUI.resultsFocusShapeText != null)
            mainUI.resultsFocusShapeText.text = result.focusShape + " <b>:דוקימ תדוקנ תרוצ</b>";

        if (mainUI.resultsSetDisplayDurationText != null)
            mainUI.resultsSetDisplayDurationText.text = result.shapeDisplayDuration + " <b>:(ms) תונומת תגצה ךשמ</b>";

        if (mainUI.resultsBetweenSetsDurationText != null)
            mainUI.resultsBetweenSetsDurationText.text = result.betweenShapesDuration + " <b>:(ms) םיטס ןיב ךשמ</b>";

        if (mainUI.resultsFocusChangeModeText != null)
            mainUI.resultsFocusChangeModeText.text = result.focusChangeMode + " <b>:דוקימ תדוקנ יוניש בצמ</b>";

        if (mainUI.resultsIntervalSetsText != null)
            mainUI.resultsIntervalSetsText.text = result.intervalSets + " <b>:(םילווטרניא) יוניש תורידת</b>";

        if (mainUI.resultsSuccessRateText != null)
            mainUI.resultsSuccessRateText.text = result.successRate + " <b>:החלצה זוחא</b>";

        if (mainUI.resultsFailRateText != null)
            mainUI.resultsFailRateText.text = result.failRate + " <b>:ןולשכ זוחא</b>";

        if (mainUI.resultsChunkSizeText != null)
            mainUI.resultsChunkSizeText.text = result.chunkSize + " <b>:הכורע לדוג</b>";

        if (mainUI.resultsStartingDistanceText != null)
            mainUI.resultsStartingDistanceText.text = result.startingDistance + " <b>:תלחתה קחרמ</b>";

        if (mainUI.resultsStartingShapeScaleText != null)
            mainUI.resultsStartingShapeScaleText.text = result.startingShapeScale + " <b>:תלחתה לדוג</b>";

        if (mainUI.resultsLevelProgressionText != null)
        {
            if (mainUI.resultsLevelProgressionScrollText == null && mainUI.resultsLevelProgressionScrollView != null)
            {
                Transform viewport = mainUI.resultsLevelProgressionScrollView.transform.Find("Viewport");
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
                        
                        mainUI.resultsLevelProgressionScrollText = text;
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
                mainUI.resultsLevelProgressionText.text = "<b>:םיבלשב תומדקתה</b>";
                
                mainUI.resultsLevelProgressionScrollView.SetActive(true);
                if (mainUI.resultsLevelProgressionScrollText != null)
                {
                    mainUI.resultsLevelProgressionScrollText.text = transformedProgression;
                }
            }
            else
            {
                mainUI.resultsLevelProgressionText.text = transformedProgression + " <b>:םיבלשב תומדקתה</b>";                
                mainUI.resultsLevelProgressionScrollView.SetActive(false);
            }
        }

        if (mainUI.resultsAccuracyText != null)
            mainUI.resultsAccuracyText.text = result.overallAccuracy + " <b>:קויד זוחא</b>";

        if (mainUI.resultsAvgResponseTimeText != null)
            mainUI.resultsAvgResponseTimeText.text = result.overallAvgResponseTime + " <b>:עצוממ הבוגת ןמז</b>";

        if (mainUI.resultsTrialsText != null)
            mainUI.resultsTrialsText.text = result.overallTrials + " <b>:םיטס כהס</b>";

        if (mainUI.resultsCorrectResponsesText != null)
            mainUI.resultsCorrectResponsesText.text = result.overallCorrectResponses + " <b>:תונוכנ תובוגת כהס</b>";
        
        //Setup close button
        if (mainUI.resultsCloseButton != null)
        {
            mainUI.resultsCloseButton.onClick.RemoveAllListeners();
            mainUI.resultsCloseButton.onClick.AddListener(() => mainUI.resultsPanel.SetActive(false));
        }
        
        //Setup expand button for level details
        if (mainUI.resultsExpandButton != null)
        {
            mainUI.resultsExpandButton.onClick.RemoveAllListeners();
            mainUI.resultsExpandButton.onClick.AddListener(() => ToggleExpandedResultView(result));
        }
        
        //Reset expansion state
        mainUI.isResultsExpanded = false;
        RectTransform popupRect = mainUI.resultsPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            popupRect.offsetMin = mainUI.normalPopupOffsetMin;
            popupRect.offsetMax = mainUI.normalPopupOffsetMax;
        }
        
        if (mainUI.partialResultsLabel != null)
            mainUI.partialResultsLabel.gameObject.SetActive(false);
        if (mainUI.resultsLevelDetailsContent != null)
            mainUI.resultsLevelDetailsContent.parent.gameObject.SetActive(false);
        
        if (mainUI.resultsExpandButtonText != null)
            mainUI.resultsExpandButtonText.text = "בחרה";
        
        //Hide the results list and show the details panel
        mainUI.resultsPanel.SetActive(true);
    }

    //A function to toggle the expanded view of level details in the results popup
    void ToggleExpandedResultView(GameResult result)
    {
        mainUI.isResultsExpanded = !mainUI.isResultsExpanded;
        
        RectTransform popupRect = mainUI.resultsPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            if (mainUI.isResultsExpanded)
            {
                popupRect.offsetMin = mainUI.expandedPopupOffsetMin;
                popupRect.offsetMax = mainUI.expandedPopupOffsetMax;
            }
            else
            {
                popupRect.offsetMin = mainUI.normalPopupOffsetMin;
                popupRect.offsetMax = mainUI.normalPopupOffsetMax;
            }
        }
        
        if (mainUI.partialResultsLabel != null)
            mainUI.partialResultsLabel.gameObject.SetActive(mainUI.isResultsExpanded);
        
        if (mainUI.resultsLevelDetailsContent != null)
            mainUI.resultsLevelDetailsContent.parent.gameObject.SetActive(mainUI.isResultsExpanded);
            mainUI.partialResultsScrollView.SetActive(mainUI.isResultsExpanded);
        
        if (mainUI.resultsExpandButtonText != null)
            mainUI.resultsExpandButtonText.text = mainUI.isResultsExpanded ? "ץווכ" : "בחרה";
        
        if (mainUI.isResultsExpanded)
        {
            DisplayExpandedLevelDetails(result);
        }
    }

    //A function to display expanded level details in the results popup
    void DisplayExpandedLevelDetails(GameResult result)
    {
        if (mainUI.resultsLevelDetailsContent == null) return;
        
        //Clear previous
        foreach (Transform child in mainUI.resultsLevelDetailsContent)
        {
            Object.Destroy(child.gameObject);
        }
        
        int d = 1;
        string sLevel = "L";
        int displayedLevels = 0;
        
        for (int i = 0; i < 20; i++)
        {
            if (i < result.levelTrials.Count && !string.IsNullOrEmpty(result.levelTrials[i]))
            {
                GameObject rowObj = new GameObject("LevelRow_D" + d + sLevel);
                rowObj.transform.SetParent(mainUI.resultsLevelDetailsContent, false);
                
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
        
        RectTransform contentRect = mainUI.resultsLevelDetailsContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, displayedLevels * 30);
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