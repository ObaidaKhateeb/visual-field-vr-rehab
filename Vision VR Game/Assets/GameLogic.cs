using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    //image sets
    public List<GameObject> imageSet1 = new List<GameObject>();
    public List<GameObject> imageSet2 = new List<GameObject>();
    public List<GameObject> imageSet3 = new List<GameObject>();
    public List<GameObject> imageSet4 = new List<GameObject>();
    public List<GameObject> imageSet5 = new List<GameObject>();
    public List<GameObject> imageSet6 = new List<GameObject>();
    public List<GameObject> imageSet7 = new List<GameObject>();
    public List<GameObject> imageSet8 = new List<GameObject>();
    public List<GameObject> imageSet9 = new List<GameObject>();
    public List<GameObject> imageSet10 = new List<GameObject>();
    public List<GameObject> imageSet11 = new List<GameObject>();
    public List<GameObject> shapePrefabs; //will be set to the chosen set
    private List<List<GameObject>> activeImageSets = new List<List<GameObject>>();

    public float shapeDistance = 2f; //Distance from camera
    public float sideOffset = 0.22f;  // Left/right separation
    public float gameDuration = 10f;   //number of rounds
    public float shapeDisplayDuration = 1500f; //Duration of showing shapes
    public float betweenShapesDuration = 1500f; //Duration between showing sets
    
    public Text countdownText;
    public Transform focusPoint;

    // sounds-related variables
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    private AudioSource audioSource;

    private GameObject leftShape, rightShape;
    private bool shapesAreSimilar;
    private bool inputAccepted;

    private int focusChangeMode;
    private int intervalSets;
    private int setsUntilChange;
    private bool waitingForFocusChange = false;
    

    private float successRate = 80f; // Rate of sets should answered True to count as success
    private float failRate = 20f; // Rate of sets should answered False to count as failure
    private int chunkSize = 15; // Chunk size
    private int currentChunkCorrect = 0;
    private int currentChunkTotal = 0;
    private float currentDistanceFromCenter = 1f; //Current distance from center (1-10)
    private float maxDistanceFromCenter = 10f; // Maximum distance from center
    private float shapeScale = 0.01f; // Scale of the shapes
    private bool nextProgressionIsSize = true;

    private bool isShapesSetSelected = false; //track if shapes set has been selected
    private bool isInShapesPhase = true; //track if in shapes phase
    private List<int> otherSetsSelected = new List<int>(); //Images sets selected 

    private int totalSimilarPairs = 0;
    private int totalNonSimilarPairs = 0;
    private int correctResponses = 0;
    private float totalResponseTime = 0f;
    private int responseCount = 0;
    private Dictionary<string, LevelStats> levelStatistics = new Dictionary<string, LevelStats>(); // level statistics dictionary
    private LevelStats currentLevelStats; //current level statistics
    private string levelProgression = "";

    private int loadedFocusShape;
    private float loadedFocusY;
    private float loadedFocusScale;
    private float loadedStartingDistance;
    private float loadedShapeScale;
    private string loadedUserID;
    private int loadedTrainingEye;
    private string loadedTimestamp;


    void Start()
    {
        LoadSettings();
        StartCoroutine(RunTrials());
        audioSource = GetComponent<AudioSource>();
    }

    //A method to load settings selected from JSON file
    void LoadSettings()
    {
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "vr_settings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            //Durations: game, set display, and between sets.
            gameDuration = settings.gameDuration;
            shapeDisplayDuration = settings.shapeDisplayDuration;
            betweenShapesDuration = settings.betweenShapesDuration;
            currentDistanceFromCenter = settings.startingDistance;
            maxDistanceFromCenter = settings.maxDistance;
            shapeScale = settings.shapeScale;
            loadedFocusShape = settings.focusShape;
            loadedFocusY = settings.focusY;
            loadedFocusScale = settings.focusScale;
            loadedShapeScale = settings.shapeScale;
            loadedStartingDistance = settings.startingDistance;

            //Success, Fail, and Chunk definitions
            successRate = settings.successRate;
            failRate = settings.failRate;
            chunkSize = settings.chunkSize;
            
            //Focus point settings: location, size, shape, and change mode.
            ApplyFocusSettings(settings);
            SetActiveImageSets(settings.imageSets);

            //Loaded settings 
            loadedUserID = settings.userID;
            loadedTrainingEye = settings.trainingEye;
            loadedTimestamp = settings.sessionTimestamp;
            
            Debug.Log("Settings loaded successfully");
        }
        else
        {
            Debug.Log("No settings file found, using defaults");
        }
    }

    //a function responsible for settings the chosen set of images 
    void SetActiveImageSets(List<int> setNumbers)
    {
        activeImageSets.Clear(); //Clear any previous selections
        otherSetsSelected.Clear();
        isShapesSetSelected = false;

        foreach(int setNumber in setNumbers)
        {
            if (setNumber == 11)
            {
                isShapesSetSelected = true;
            }
            else if (setNumber >= 1 && setNumber <= 10)
            {
                otherSetsSelected.Add(setNumber);
            }
        }

        if (isShapesSetSelected)
        {
            activeImageSets.Add(imageSet11);
            isInShapesPhase = true;
            Debug.Log("Starting with Shapes Set (Set 11) only");
        }
        else 
        {
            isInShapesPhase = false;
            foreach(int setNumber in otherSetsSelected)
            {
                switch(setNumber)
                {
                    case 1: activeImageSets.Add(imageSet1); break;
                    case 2: activeImageSets.Add(imageSet2); break;
                    case 3: activeImageSets.Add(imageSet3); break;
                    case 4: activeImageSets.Add(imageSet4); break;
                    case 5: activeImageSets.Add(imageSet5); break;
                    case 6: activeImageSets.Add(imageSet6); break;
                    case 7: activeImageSets.Add(imageSet7); break;
                    case 8: activeImageSets.Add(imageSet8); break;
                    case 9: activeImageSets.Add(imageSet9); break;
                    case 10: activeImageSets.Add(imageSet10); break;
                    default: 
                        Debug.LogWarning("Invalid image set number: " + setNumber);
                        break;
                }
            }
        }
        
        // if no valid set was selected, use all the sets as default
        if (activeImageSets.Count == 0)
        {
            activeImageSets.Add(imageSet1);
            activeImageSets.Add(imageSet2);
            activeImageSets.Add(imageSet3);
            activeImageSets.Add(imageSet4);
            activeImageSets.Add(imageSet5);
            activeImageSets.Add(imageSet6);
            activeImageSets.Add(imageSet7);
            activeImageSets.Add(imageSet8);
            activeImageSets.Add(imageSet9);
            activeImageSets.Add(imageSet10);
            activeImageSets.Add(imageSet11);
            Debug.LogWarning("No image sets selected, using all image sets as default");
        }
    }

    //A method to switch from shapes set to other sets when max level is reached
    void SwitchToOtherSets()
    {
        if (otherSetsSelected.Count == 0)
        {
            Debug.Log("No other sets available. Game will end.");
            return;
        }
        
        Debug.Log("Maximum level reached with Shapes Set! Switching to other sets and restarting from beginning.");
        
        //Clear current active sets
        activeImageSets.Clear();
        
        //Add other sets (1-10)
        foreach(int setNumber in otherSetsSelected)
        {
            switch(setNumber)
            {
                case 1: activeImageSets.Add(imageSet1); break;
                case 2: activeImageSets.Add(imageSet2); break;
                case 3: activeImageSets.Add(imageSet3); break;
                case 4: activeImageSets.Add(imageSet4); break;
                case 5: activeImageSets.Add(imageSet5); break;
                case 6: activeImageSets.Add(imageSet6); break;
                case 7: activeImageSets.Add(imageSet7); break;
                case 8: activeImageSets.Add(imageSet8); break;
                case 9: activeImageSets.Add(imageSet9); break;
                case 10: activeImageSets.Add(imageSet10); break;
            }
        }
        
        //Reset to starting difficulty
        currentDistanceFromCenter = loadedStartingDistance;
        shapeScale = loadedShapeScale;
        nextProgressionIsSize = true;
        isInShapesPhase = false;
        
        //Update level stats
        string currentSizeLevel = nextProgressionIsSize ? "L" : "S";
        currentLevelStats = GetOrCreateLevelStats((int)currentDistanceFromCenter, currentSizeLevel);
        levelProgression += $" | Restart(D{(int)currentDistanceFromCenter}{currentSizeLevel})";
    }

    //A method to apply focus point settings from loaded JSON
    void ApplyFocusSettings(VRSettings settings)
    {
        // Find both focus point GameObjects
        GameObject focusCircle = GameObject.Find("FocusPointCircle");  
        GameObject focusCross = GameObject.Find("FocusPointCross");   
        
        if (focusCircle != null && focusCross != null)
        {
            if (settings.focusShape == 0) //Circle
            {
                focusCircle.SetActive(true);
                focusCross.SetActive(false);
                focusPoint = focusCircle.transform;
            }
            else //Cross
            {
                focusCircle.SetActive(false);
                focusCross.SetActive(true);
                focusPoint = focusCross.transform;
            }
            
            //Scale and position of focus point
            Vector3 pos = focusPoint.localPosition;
            focusPoint.localPosition = new Vector3(pos.x, settings.focusY, pos.z);
            focusPoint.localScale = Vector3.one * settings.focusScale;
        }
        else if (focusPoint != null)
        {
            //if focus objects not found by name, use existing focusPoint
            Vector3 pos = focusPoint.localPosition;
            focusPoint.localPosition = new Vector3(pos.x, settings.focusY, pos.z);
            focusPoint.localScale = Vector3.one * settings.focusScale;

        }
        focusChangeMode = settings.focusChangeMode;
        intervalSets = settings.intervalSets;
        ResetSetCounter();
    }

    //A function taht resets the set counter and determine how many sets until the next focus change
    void ResetSetCounter()
    {        
        if (focusChangeMode == 1) // Interval
            setsUntilChange = intervalSets;
        else if (focusChangeMode == 2) // Random
            setsUntilChange = Random.Range(1, 11);
    }

    //A function that changes the focus point position randomly along the Y-axis
    void ChangeFocusPoint()
    {
        Vector3 newPos = focusPoint.localPosition;
        newPos.y = Random.Range(-0.5f, 0.5f);
        focusPoint.localPosition = newPos;
        
        Debug.Log("Focus point changed to: " + newPos);
    }

    // Coroutine to run the trials
    IEnumerator RunTrials()
    {
        yield return new WaitForSeconds(3f); //Wait 3 seconds before starting

        if (focusPoint != null)
            focusPoint.gameObject.SetActive(false);
        if (countdownText != null)
            {
                yield return new WaitForSeconds(1f);

                countdownText.gameObject.SetActive(true);
                
                countdownText.text = "3";
                yield return new WaitForSeconds(1f);
                
                countdownText.text = "2";
                yield return new WaitForSeconds(1f);
                
                countdownText.text = "1";
                yield return new WaitForSeconds(1f);
                
                countdownText.gameObject.SetActive(false);
            }
        if (focusPoint != null)
            focusPoint.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        float elapsedTime = 0f;
        string currentSizeLevel = nextProgressionIsSize ? "L" : "S";
        currentLevelStats = GetOrCreateLevelStats((int)currentDistanceFromCenter, currentSizeLevel);
        levelProgression = $"Start(D{(int)currentDistanceFromCenter}{currentSizeLevel})";
        while (elapsedTime < gameDuration || currentChunkTotal > 0)
        {
            float roundStartTime = Time.time; //round start time 
            
            //Changing focus point position logic 
            setsUntilChange--; 
            if (focusChangeMode != 0 && setsUntilChange <= 0 && !waitingForFocusChange)
            {
                waitingForFocusChange = true;
                ChangeFocusPoint();
                yield return new WaitForSeconds(1f); //wait 1 second after changing
                ResetSetCounter();
                waitingForFocusChange = false;
            }

            //Shapes choosing and showing
            SpawnShapes();
            //shapes hide 
            StartCoroutine(HideShapesAfterDelay(shapeDisplayDuration/1000f));
            inputAccepted = true;
            bool responded = false;

            //Wait for up to 2 seconds or betweenShapesDuration time for user to press SPACE
            float maxResponseTime = Mathf.Min(2f, betweenShapesDuration/1000f);
            float timer = 0f;
            while (timer < maxResponseTime)
            {
                if (inputAccepted && Input.GetKeyDown(KeyCode.Space))
                {
                    responded = true;
                    if (shapesAreSimilar)
                    {
                        Debug.Log("Correct (Shapes are similar)");
                        audioSource.PlayOneShot(correctSound);

                        //Update overall stats
                        correctResponses++;
                        responseCount++;
                        totalResponseTime += timer;

                        //Update level stats
                        currentLevelStats.correctResponses++;
                        currentLevelStats.responseCount++;
                        currentLevelStats.totalResponseTime += timer;
                    }
                    else
                    {
                        Debug.Log("Incorrect (Shapes are different)");
                        audioSource.PlayOneShot(incorrectSound);

                        //Update overall stats
                        responseCount++;
                        totalResponseTime += timer;

                        //Update level stats
                        currentLevelStats.responseCount++;
                        currentLevelStats.totalResponseTime += timer;
                    }
                    inputAccepted = false;
                    break;  //User pressed SPACE
                }

                timer += Time.deltaTime;
                yield return null;
            }

            //Evaluate non-response if SPACE wasn't pressed
            if (!responded)
            {
                if (!shapesAreSimilar)
                {
                    Debug.Log("Correct (Shapes are different)");
                    audioSource.PlayOneShot(correctSound);
                    correctResponses++;
                    currentLevelStats.correctResponses++;
                }
                else
                {
                    Debug.Log("Incorrect (Shapes are similar)");
                    audioSource.PlayOneShot(incorrectSound);
                }
            }

            //Clean up
            Destroy(leftShape);
            Destroy(rightShape);
            inputAccepted = false;

            //Track chunk progress
            currentChunkTotal++;
            currentLevelStats.totalTrials++;
            if ((responded && shapesAreSimilar) || (!responded && !shapesAreSimilar))
            {
                currentChunkCorrect++;
            }
            
            //Check if chunk is complete
            if (currentChunkTotal >= chunkSize)
            {
                EvaluateChunk();
                currentSizeLevel = nextProgressionIsSize ? "L" : "S";
                currentLevelStats = GetOrCreateLevelStats((int)currentDistanceFromCenter, currentSizeLevel);
                currentChunkCorrect = 0;
                currentChunkTotal = 0;
            }

            yield return new WaitForSeconds(betweenShapesDuration/1000f);

            elapsedTime += Time.time - roundStartTime; //update elapsed time
        }
        if (focusPoint != null)
            focusPoint.gameObject.SetActive(false);
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "םויס";
        }
        yield return new WaitForSeconds(3f);
        LogGameStatistics();
    }

    // Coroutine to hide shapes after a delay
    IEnumerator HideShapesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (leftShape != null) Destroy(leftShape);
        if (rightShape != null && rightShape != leftShape) Destroy(rightShape);
    }

    // A method to spawn shapes
    void SpawnShapes()
    {
        //Get camera transform
        Transform cam = Camera.main.transform;

        //Position relative to focus point
        Vector3 center = focusPoint.position + focusPoint.forward * shapeDistance;
        float screenPercent = 0.3f + ((currentDistanceFromCenter - 1f) / 9f) * 0.8f;

        Camera mainCamera = cam.GetComponent<Camera>();
        float halfHeight = shapeDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * mainCamera.aspect;
        float horizontalOffset = halfWidth * screenPercent;
        Vector3 rightPos = center + focusPoint.right * horizontalOffset;
        Vector3 leftPos = center - focusPoint.right * horizontalOffset;

        //Choose a random image set from the active sets
        int setIndex = Random.Range(0, activeImageSets.Count);
        List<GameObject> chosenSet = activeImageSets[setIndex];

        //Choose right shape from that set
        int rightIndex = Random.Range(0, chosenSet.Count);
        GameObject right = chosenSet[rightIndex];

        //50% chance to match
        bool same = Random.value < 0.5f;
        GameObject left;
        if (same)
        {
            left = right;
        }
        else
        {
            //Pick a different shape from the SAME set
            int leftIndex;
            do
            {
                leftIndex = Random.Range(0, chosenSet.Count);
            } while (leftIndex == rightIndex && chosenSet.Count > 1);
            left = chosenSet[leftIndex];
        }

        shapesAreSimilar = same;

        //Instantiate
        rightShape = Instantiate(right, rightPos, Quaternion.identity);
        leftShape = Instantiate(left, leftPos, Quaternion.identity);

        //Apply scale to shapes
        rightShape.transform.localScale = Vector3.one * shapeScale;
        leftShape.transform.localScale = Vector3.one * shapeScale;

        //make shapes face user
        rightShape.transform.LookAt(cam);
        leftShape.transform.LookAt(cam);

        if (shapesAreSimilar) 
            totalSimilarPairs++;
        else 
            totalNonSimilarPairs++;
    }

    //A method that allows exiting the game when 'escape' is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    //A method that evaluates chunk accuracy and adjusts difficulty accordingly
    void EvaluateChunk()
    {
        float accuracy = (float)currentChunkCorrect / currentChunkTotal * 100f;

        Debug.Log("Chunk completed. Accuracy: " + accuracy.ToString("F1") + "% (" + currentChunkCorrect + "/" + currentChunkTotal + ")");

        if (accuracy >= successRate)
        {
            if (nextProgressionIsSize)
            {
                if (shapeScale > 0.004f)
                {
                    shapeScale = Mathf.Max(0.004f, shapeScale - 0.0036f);
                    Debug.Log("Level UP! Shape size decreased to: " + (shapeScale / 0.0036f));
                    levelProgression += $" Up(D{(int)currentDistanceFromCenter}L)";
                }
                else
                {
                    Debug.Log("Level UP! Size already at minimum, staying at current level");
                    levelProgression += $" Same(D{(int)currentDistanceFromCenter}L)";
                }
                nextProgressionIsSize = false;
            }
            else
            {
                if (currentDistanceFromCenter < maxDistanceFromCenter)
                {
                    currentDistanceFromCenter = Mathf.Min(maxDistanceFromCenter, currentDistanceFromCenter + 1f);
                    Debug.Log("Level UP! Distance increased to: " + currentDistanceFromCenter);
                    levelProgression += $" Up(D{(int)currentDistanceFromCenter}S)";
                    nextProgressionIsSize = true;
                }
                else
                {
                    Debug.Log("Level UP! Distance at maximum on distance's turn");
                    levelProgression += $" Same(D{(int)currentDistanceFromCenter}S)";

                    if (isInShapesPhase && otherSetsSelected.Count > 0)
                    {
                        SwitchToOtherSets();
                    }
                    else
                    {
                        Debug.Log("Maximum distance reached! Ending game...");
                        if (focusPoint != null)
                            focusPoint.gameObject.SetActive(false);
                        if (countdownText != null)
                        {
                            countdownText.gameObject.SetActive(true);
                            countdownText.text = "םויס";
                        }
                        LogGameStatistics();
                        StopAllCoroutines();
                        return;
                    }
                }
            }
        }
        else if (accuracy <= failRate)
        {
            if (nextProgressionIsSize)
            {
                if (shapeScale < 0.04f)
                {
                    shapeScale = Mathf.Min(0.04f, shapeScale + 0.0036f);
                    Debug.Log("Level DOWN! Shape size increased to: " + (shapeScale / 0.0036f));
                    levelProgression += $" Down(D{(int)currentDistanceFromCenter}L)";
                }
                else
                {
                    Debug.Log("Level DOWN! Size already at maximum, staying at current level");
                    levelProgression += $" Same(D{(int)currentDistanceFromCenter}L)";
                }
                nextProgressionIsSize = false;
            }
            else
            {
                if (currentDistanceFromCenter > 1f)
                {
                    currentDistanceFromCenter = Mathf.Max(1f, currentDistanceFromCenter - 1f);
                    Debug.Log("Level DOWN! Distance decreased to: " + currentDistanceFromCenter);
                    levelProgression += $" Down(D{(int)currentDistanceFromCenter}S)";
                }
                else
                {
                    Debug.Log("Level DOWN! Distance already at minimum, staying at current level");
                    levelProgression += $" Same(D{(int)currentDistanceFromCenter}S)";
                }
                nextProgressionIsSize = true; 
            }
        }
        else
        {
            Debug.Log("Level maintained. Current: distance=" + currentDistanceFromCenter + ", size=" + (shapeScale / 0.005f));
            levelProgression += $" Same(D{(int)currentDistanceFromCenter}{(nextProgressionIsSize ? "L" : "S")})";
        }
    }

    //A method that gets or creates LevelStats instance for given distance and scale
    LevelStats GetOrCreateLevelStats(int distance, string sizeLevel)
    {
        string key = distance + "_" + sizeLevel;
        if (!levelStatistics.ContainsKey(key))
        {
            levelStatistics[key] = new LevelStats { distance = distance, sizeLevel = sizeLevel };
        }
        return levelStatistics[key];
    }

    //A method that save game statistics to CSV files and launch GUI application to show results there 
    void LogGameStatistics()
    {
        int totalTrials = totalSimilarPairs + totalNonSimilarPairs;
        float overallAccuracy = totalTrials > 0 ? (float)correctResponses / totalTrials * 100f : 0f;
        float averageResponseTime = responseCount > 0 ? (totalResponseTime / responseCount) : 0f;

        Debug.Log("=== RESULTS ===");
        Debug.Log("Overall Accuracy: " + overallAccuracy.ToString("F1") + "% (" + correctResponses + "/" + totalTrials + ")");
        Debug.Log("Overall Average Response Time: " + averageResponseTime.ToString("F1") + " seconds");

        SaveResultsToCSV(overallAccuracy, averageResponseTime, totalTrials);
        LaunchGUIApplication();
        Application.Quit();
    }

    //A helper method that saves results to CSV file
    void SaveResultsToCSV(float accuracy, float avgResponseTime, int totalTrials)
    {
        string csvFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData");
        
        if (!Directory.Exists(csvFolder))
            Directory.CreateDirectory(csvFolder);
        
        string csvPath = Path.Combine(csvFolder, "game_results.csv");
        
        bool fileExists = File.Exists(csvPath);
        
        using (StreamWriter writer = new StreamWriter(csvPath, true))
        {
            int d = 1;
            string sLevel = "L";
            //Header row writing
            if (!fileExists)
            {
                string header = "ID,Test Time,Eye Trained,Test Duration (m),Focus Point Position,Focus Point Scale,Focus Point Shape,Set Display Duration (ms),Between Sets Duration (ms),Focus Point Change Mode,Focus Point Change Frequency (Intervals),Success Rate,Fail Rate,Chunk Size,Starting Distance,Starting Shape Scale,Overall Accuracy,Overall Average Response Time (s),Overall Trials,Overall Correct Responses";               
                for (int i = 0; i < 20; i++)
                {
                    header += $",D{d}{sLevel} Accuracy,D{d}{sLevel} Avg Response Time,D{d}{sLevel} Trials,D{d}{sLevel} Correct Responses";
                    if (sLevel == "L") 
                        sLevel = "S";
                    else 
                    {
                        sLevel = "L";
                        d++;
                    }
                }
                header += ",Level Progression";
                writer.WriteLine(header);
            }
            
            string eyeText = loadedTrainingEye == 0 ? "Right" : "Left";
            string focusShapeText = loadedFocusShape == 0 ? "Circle" : "Cross";
            string focusChangeModeText = focusChangeMode == 0 ? "Fixed" : (focusChangeMode == 1 ? "Changes at fixed intervals" : "Changes Randomly");
            string intervalSetsText = focusChangeMode == 1 ? intervalSets.ToString() : "N/A";

            string dataLine = $"{loadedUserID},{loadedTimestamp},{eyeText},{gameDuration / 60f},{loadedFocusY * 100f},{loadedFocusScale * 100f},{focusShapeText},{shapeDisplayDuration},{betweenShapesDuration},{focusChangeModeText},{intervalSetsText},{successRate}%,{failRate}%,{chunkSize},{loadedStartingDistance},{Mathf.RoundToInt(loadedShapeScale / 0.005f)},{accuracy:F1}%,{avgResponseTime:F2},{totalTrials},{correctResponses}";
            d = 1;
            sLevel = "L";
            for (int i = 0; i < 20; i++)
            {
                string key = d + "_" + sLevel;
                if (levelStatistics.ContainsKey(key) && levelStatistics[key].totalTrials > 0)
                {
                    LevelStats stats = levelStatistics[key];
                    dataLine += $",{stats.GetAccuracy():F1}%,{stats.GetAvgResponseTime():F2},{stats.totalTrials},{stats.correctResponses}";
                }
                else
                {
                    dataLine += ",,,,";
                }
                if (sLevel == "L") 
                    sLevel = "S";
                else 
                {
                    sLevel = "L";
                    d++;
                }
            }
            dataLine += $",{levelProgression}";
            writer.WriteLine(dataLine);
        }
        
        Debug.Log("Results saved to CSV successfully");
        string flagPath = Path.Combine(csvFolder, "show_latest_result.flag");
        File.WriteAllText(flagPath, "");
    }
    
    //a helper method that launchs the GUI application
    void LaunchGUIApplication()
    {
        string guiPath = Path.Combine(Application.dataPath, "..","..", "VisualTraining.exe");

        if (File.Exists(guiPath))
        {
            System.Diagnostics.Process.Start(guiPath);
            Debug.Log("GUI application launched");
        }
        else
        {
            Debug.LogWarning("GUI executable not found at: " + guiPath);
        }
    }

    [System.Serializable]
    public class LevelStats
    {
        public int distance;
        public string sizeLevel;
        public int correctResponses = 0;
        public int totalTrials = 0;
        public float totalResponseTime = 0f;
        public int responseCount = 0;
        
        public float GetAccuracy()
        {
            return totalTrials > 0 ? (float)correctResponses / totalTrials * 100f : 0f;
        }
        
        public float GetAvgResponseTime()
        {
            return responseCount > 0 ? totalResponseTime / responseCount : 0f;
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
        public float shapeScale = 0.05f;
        public float successRate = 80f;
        public float failRate = 20f;
        public int chunkSize = 15;
        public List<int> imageSets = new List<int>();
        public string userID;
        public int trainingEye;
        public string sessionTimestamp;
    }
}