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
    public List<List<GameObject>> activeImageSets = new List<List<GameObject>>();

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
    public AudioSource audioSource;

    public GameObject leftShape, rightShape;
    public bool shapesAreSimilar;
    public bool inputAccepted;

    public int focusChangeMode;
    public int intervalSets;
    public int setsUntilChange;
    public bool waitingForFocusChange = false;
    

    public float successRate = 80f; // Rate of sets should answered True to count as success
    public float failRate = 20f; // Rate of sets should answered False to count as failure
    public int chunkSize = 15; // Chunk size
    public int currentChunkCorrect = 0;
    public int currentChunkTotal = 0;
    public float currentDistanceFromCenter = 1f; //Current distance from center (1-10)
    public float maxDistanceFromCenter = 10f; // Maximum distance from center
    public float shapeScale = 0.01f; // Scale of the shapes
    public bool nextProgressionIsSize = true;

    public bool isShapesSetSelected = false; //track if shapes set has been selected
    public bool isInShapesPhase = true; //track if in shapes phase
    public List<int> otherSetsSelected = new List<int>(); //Images sets selected 

    public int totalSimilarPairs = 0;
    public int totalNonSimilarPairs = 0;
    public int correctResponses = 0;
    public float totalResponseTime = 0f;
    public int responseCount = 0;
    public Dictionary<string, LevelStats> levelStatistics = new Dictionary<string, LevelStats>(); // level statistics dictionary
    public LevelStats currentLevelStats; //current level statistics
    public string levelProgression = "";

    public int loadedFocusShape;
    public float loadedFocusY;
    public float loadedFocusScale;
    public float loadedStartingDistance;
    public float loadedShapeScale;
    public string loadedUserID;
    public int loadedTrainingEye;
    public string loadedTimestamp;

    // Helper class instances
    private SettingsLoader settingsLoader;
    private ImageSetManager imageSetManager;
    private FocusPointManager focusPointManager;
    private TrialRunner trialRunner;
    private ShapeSpawner shapeSpawner;
    private DifficultyManager difficultyManager;
    private StatisticsLogger statisticsLogger;

    void Start()
    {
        // Initialize helper classes
        settingsLoader = new SettingsLoader(this);
        imageSetManager = new ImageSetManager(this);
        focusPointManager = new FocusPointManager(this);
        trialRunner = new TrialRunner(this, focusPointManager, shapeSpawner, difficultyManager);
        shapeSpawner = new ShapeSpawner(this);
        difficultyManager = new DifficultyManager(this, imageSetManager);
        statisticsLogger = new StatisticsLogger(this);

        settingsLoader.LoadSettings();
        StartCoroutine(trialRunner.RunTrials());
        audioSource = GetComponent<AudioSource>();
    }

    //A method that allows exiting the game when 'escape' is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}


public class SettingsLoader
{
    private GameLogic mainGame;

    public SettingsLoader(GameLogic game)
    {
        mainGame = game;
    }

    //A method to load settings selected from JSON file
    public void LoadSettings()
    {
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "VRUserData", "vr_settings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            //Durations: game, set display, and between sets.
            mainGame.gameDuration = settings.gameDuration;
            mainGame.shapeDisplayDuration = settings.shapeDisplayDuration;
            mainGame.betweenShapesDuration = settings.betweenShapesDuration;
            mainGame.currentDistanceFromCenter = settings.startingDistance;
            mainGame.maxDistanceFromCenter = settings.maxDistance;
            mainGame.shapeScale = settings.shapeScale;
            mainGame.loadedFocusShape = settings.focusShape;
            mainGame.loadedFocusY = settings.focusY;
            mainGame.loadedFocusScale = settings.focusScale;
            mainGame.loadedShapeScale = settings.shapeScale;
            mainGame.loadedStartingDistance = settings.startingDistance;

            //Success, Fail, and Chunk definitions
            mainGame.successRate = settings.successRate;
            mainGame.failRate = settings.failRate;
            mainGame.chunkSize = settings.chunkSize;
            
            //Focus point settings: location, size, shape, and change mode.
            FocusPointManager focusManager = new FocusPointManager(mainGame);
            focusManager.ApplyFocusSettings(settings);
            
            ImageSetManager imageManager = new ImageSetManager(mainGame);
            imageManager.SetActiveImageSets(settings.imageSets);

            //Loaded settings 
            mainGame.loadedUserID = settings.userID;
            mainGame.loadedTrainingEye = settings.trainingEye;
            mainGame.loadedTimestamp = settings.sessionTimestamp;
            
            Debug.Log("Settings loaded successfully");
        }
        else
        {
            Debug.Log("No settings file found, using defaults");
        }
    }
}


public class ImageSetManager
{
    private GameLogic mainGame;

    public ImageSetManager(GameLogic game)
    {
        mainGame = game;
    }

    //a function responsible for settings the chosen set of images 
    public void SetActiveImageSets(List<int> setNumbers)
    {
        mainGame.activeImageSets.Clear(); //Clear any previous selections
        mainGame.otherSetsSelected.Clear();
        mainGame.isShapesSetSelected = false;

        foreach(int setNumber in setNumbers)
        {
            if (setNumber == 11)
            {
                mainGame.isShapesSetSelected = true;
            }
            else if (setNumber >= 1 && setNumber <= 10)
            {
                mainGame.otherSetsSelected.Add(setNumber);
            }
        }

        if (mainGame.isShapesSetSelected)
        {
            mainGame.activeImageSets.Add(mainGame.imageSet11);
            mainGame.isInShapesPhase = true;
            Debug.Log("Starting with Shapes Set (Set 11) only");
        }
        else 
        {
            mainGame.isInShapesPhase = false;
            foreach(int setNumber in mainGame.otherSetsSelected)
            {
                switch(setNumber)
                {
                    case 1: mainGame.activeImageSets.Add(mainGame.imageSet1); break;
                    case 2: mainGame.activeImageSets.Add(mainGame.imageSet2); break;
                    case 3: mainGame.activeImageSets.Add(mainGame.imageSet3); break;
                    case 4: mainGame.activeImageSets.Add(mainGame.imageSet4); break;
                    case 5: mainGame.activeImageSets.Add(mainGame.imageSet5); break;
                    case 6: mainGame.activeImageSets.Add(mainGame.imageSet6); break;
                    case 7: mainGame.activeImageSets.Add(mainGame.imageSet7); break;
                    case 8: mainGame.activeImageSets.Add(mainGame.imageSet8); break;
                    case 9: mainGame.activeImageSets.Add(mainGame.imageSet9); break;
                    case 10: mainGame.activeImageSets.Add(mainGame.imageSet10); break;
                    default: 
                        Debug.LogWarning("Invalid image set number: " + setNumber);
                        break;
                }
            }
        }
        
        // if no valid set was selected, use all the sets as default
        if (mainGame.activeImageSets.Count == 0)
        {
            mainGame.activeImageSets.Add(mainGame.imageSet1);
            mainGame.activeImageSets.Add(mainGame.imageSet2);
            mainGame.activeImageSets.Add(mainGame.imageSet3);
            mainGame.activeImageSets.Add(mainGame.imageSet4);
            mainGame.activeImageSets.Add(mainGame.imageSet5);
            mainGame.activeImageSets.Add(mainGame.imageSet6);
            mainGame.activeImageSets.Add(mainGame.imageSet7);
            mainGame.activeImageSets.Add(mainGame.imageSet8);
            mainGame.activeImageSets.Add(mainGame.imageSet9);
            mainGame.activeImageSets.Add(mainGame.imageSet10);
            mainGame.activeImageSets.Add(mainGame.imageSet11);
            Debug.LogWarning("No image sets selected, using all image sets as default");
        }
    }

    //A method to switch from shapes set to other sets when max level is reached
    public void SwitchToOtherSets()
    {
        if (mainGame.otherSetsSelected.Count == 0)
        {
            Debug.Log("No other sets available. Game will end.");
            return;
        }
        
        Debug.Log("Maximum level reached with Shapes Set! Switching to other sets and restarting from beginning.");
        
        //Clear current active sets
        mainGame.activeImageSets.Clear();
        
        //Add other sets (1-10)
        foreach(int setNumber in mainGame.otherSetsSelected)
        {
            switch(setNumber)
            {
                case 1: mainGame.activeImageSets.Add(mainGame.imageSet1); break;
                case 2: mainGame.activeImageSets.Add(mainGame.imageSet2); break;
                case 3: mainGame.activeImageSets.Add(mainGame.imageSet3); break;
                case 4: mainGame.activeImageSets.Add(mainGame.imageSet4); break;
                case 5: mainGame.activeImageSets.Add(mainGame.imageSet5); break;
                case 6: mainGame.activeImageSets.Add(mainGame.imageSet6); break;
                case 7: mainGame.activeImageSets.Add(mainGame.imageSet7); break;
                case 8: mainGame.activeImageSets.Add(mainGame.imageSet8); break;
                case 9: mainGame.activeImageSets.Add(mainGame.imageSet9); break;
                case 10: mainGame.activeImageSets.Add(mainGame.imageSet10); break;
            }
        }
        
        //Reset to starting difficulty
        mainGame.currentDistanceFromCenter = mainGame.loadedStartingDistance;
        mainGame.shapeScale = mainGame.loadedShapeScale;
        mainGame.nextProgressionIsSize = true;
        mainGame.isInShapesPhase = false;
        
        //Update level stats
        string currentSizeLevel = mainGame.nextProgressionIsSize ? "L" : "S";
        DifficultyManager diffManager = new DifficultyManager(mainGame, this);
        mainGame.currentLevelStats = diffManager.GetOrCreateLevelStats((int)mainGame.currentDistanceFromCenter, currentSizeLevel);
        mainGame.levelProgression += $" | Restart(D{(int)mainGame.currentDistanceFromCenter}{currentSizeLevel})";
    }
}


public class FocusPointManager
{
    private GameLogic mainGame;

    public FocusPointManager(GameLogic game)
    {
        mainGame = game;
    }

    //A method to apply focus point settings from loaded JSON
    public void ApplyFocusSettings(VRSettings settings)
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
                mainGame.focusPoint = focusCircle.transform;
            }
            else //Cross
            {
                focusCircle.SetActive(false);
                focusCross.SetActive(true);
                mainGame.focusPoint = focusCross.transform;
            }
            
            //Scale and position of focus point
            Vector3 pos = mainGame.focusPoint.localPosition;
            mainGame.focusPoint.localPosition = new Vector3(pos.x, settings.focusY, pos.z);
            mainGame.focusPoint.localScale = Vector3.one * settings.focusScale;
        }
        else if (mainGame.focusPoint != null)
        {
            //if focus objects not found by name, use existing focusPoint
            Vector3 pos = mainGame.focusPoint.localPosition;
            mainGame.focusPoint.localPosition = new Vector3(pos.x, settings.focusY, pos.z);
            mainGame.focusPoint.localScale = Vector3.one * settings.focusScale;

        }
        mainGame.focusChangeMode = settings.focusChangeMode;
        mainGame.intervalSets = settings.intervalSets;
        ResetSetCounter();
    }

    //A function taht resets the set counter and determine how many sets until the next focus change
    public void ResetSetCounter()
    {        
        if (mainGame.focusChangeMode == 1) // Interval
            mainGame.setsUntilChange = mainGame.intervalSets;
        else if (mainGame.focusChangeMode == 2) // Random
            mainGame.setsUntilChange = Random.Range(1, 11);
    }

    //A function that changes the focus point position randomly along the Y-axis
    public void ChangeFocusPoint()
    {
        Vector3 newPos = mainGame.focusPoint.localPosition;
        newPos.y = Random.Range(-0.5f, 0.5f);
        mainGame.focusPoint.localPosition = newPos;
        
        Debug.Log("Focus point changed to: " + newPos);
    }
}


public class TrialRunner
{
    private GameLogic mainGame;
    private FocusPointManager focusManager;
    private ShapeSpawner shapeSpawner;
    private DifficultyManager difficultyManager;

    public TrialRunner(GameLogic game, FocusPointManager focusMgr, ShapeSpawner spawner, DifficultyManager diffMgr)
    {
        mainGame = game;
        focusManager = focusMgr;
        shapeSpawner = spawner;
        difficultyManager = diffMgr;
    }

    // Coroutine to run the trials
    public IEnumerator RunTrials()
    {
        yield return new WaitForSeconds(3f); //Wait 3 seconds before starting

        if (mainGame.focusPoint != null)
            mainGame.focusPoint.gameObject.SetActive(false);
        if (mainGame.countdownText != null)
            {
                yield return new WaitForSeconds(1f);

                mainGame.countdownText.gameObject.SetActive(true);
                
                mainGame.countdownText.text = "3";
                yield return new WaitForSeconds(1f);
                
                mainGame.countdownText.text = "2";
                yield return new WaitForSeconds(1f);
                
                mainGame.countdownText.text = "1";
                yield return new WaitForSeconds(1f);
                
                mainGame.countdownText.gameObject.SetActive(false);
            }
        if (mainGame.focusPoint != null)
            mainGame.focusPoint.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        float elapsedTime = 0f;
        string currentSizeLevel = mainGame.nextProgressionIsSize ? "L" : "S";
        difficultyManager = new DifficultyManager(mainGame, new ImageSetManager(mainGame));
        mainGame.currentLevelStats = difficultyManager.GetOrCreateLevelStats((int)mainGame.currentDistanceFromCenter, currentSizeLevel);
        mainGame.levelProgression = $"Start(D{(int)mainGame.currentDistanceFromCenter}{currentSizeLevel})";
        while (elapsedTime < mainGame.gameDuration || mainGame.currentChunkTotal > 0)
        {
            float roundStartTime = Time.time; //round start time 
            
            //Changing focus point position logic 
            mainGame.setsUntilChange--; 
            focusManager = new FocusPointManager(mainGame);
            if (mainGame.focusChangeMode != 0 && mainGame.setsUntilChange <= 0 && !mainGame.waitingForFocusChange)
            {
                mainGame.waitingForFocusChange = true;
                focusManager.ChangeFocusPoint();
                yield return new WaitForSeconds(1f); //wait 1 second after changing
                focusManager.ResetSetCounter();
                mainGame.waitingForFocusChange = false;
            }

            //Shapes choosing and showing
            shapeSpawner = new ShapeSpawner(mainGame);
            shapeSpawner.SpawnShapes();
            //shapes hide 
            mainGame.StartCoroutine(HideShapesAfterDelay(mainGame.shapeDisplayDuration/1000f));
            mainGame.inputAccepted = true;
            bool responded = false;

            //Wait for up to 2 seconds or betweenShapesDuration time for user to press SPACE
            float maxResponseTime = Mathf.Min(2f, mainGame.betweenShapesDuration/1000f);
            float timer = 0f;
            while (timer < maxResponseTime)
            {
                if (mainGame.inputAccepted && Input.GetKeyDown(KeyCode.Space))
                {
                    responded = true;
                    if (mainGame.shapesAreSimilar)
                    {
                        Debug.Log("Correct (Shapes are similar)");
                        mainGame.audioSource.PlayOneShot(mainGame.correctSound);

                        //Update overall stats
                        mainGame.correctResponses++;
                        mainGame.responseCount++;
                        mainGame.totalResponseTime += timer;

                        //Update level stats
                        mainGame.currentLevelStats.correctResponses++;
                        mainGame.currentLevelStats.responseCount++;
                        mainGame.currentLevelStats.totalResponseTime += timer;
                    }
                    else
                    {
                        Debug.Log("Incorrect (Shapes are different)");
                        mainGame.audioSource.PlayOneShot(mainGame.incorrectSound);

                        //Update overall stats
                        mainGame.responseCount++;
                        mainGame.totalResponseTime += timer;

                        //Update level stats
                        mainGame.currentLevelStats.responseCount++;
                        mainGame.currentLevelStats.totalResponseTime += timer;
                    }
                    mainGame.inputAccepted = false;
                    break;  //User pressed SPACE
                }

                timer += Time.deltaTime;
                yield return null;
            }

            //Evaluate non-response if SPACE wasn't pressed
            if (!responded)
            {
                if (!mainGame.shapesAreSimilar)
                {
                    Debug.Log("Correct (Shapes are different)");
                    mainGame.audioSource.PlayOneShot(mainGame.correctSound);
                    mainGame.correctResponses++;
                    mainGame.currentLevelStats.correctResponses++;
                }
                else
                {
                    Debug.Log("Incorrect (Shapes are similar)");
                    mainGame.audioSource.PlayOneShot(mainGame.incorrectSound);
                }
            }

            //Clean up
            Object.Destroy(mainGame.leftShape);
            Object.Destroy(mainGame.rightShape);
            mainGame.inputAccepted = false;

            //Track chunk progress
            mainGame.currentChunkTotal++;
            mainGame.currentLevelStats.totalTrials++;
            if ((responded && mainGame.shapesAreSimilar) || (!responded && !mainGame.shapesAreSimilar))
            {
                mainGame.currentChunkCorrect++;
            }
            
            //Check if chunk is complete
            if (mainGame.currentChunkTotal >= mainGame.chunkSize)
            {
                difficultyManager.EvaluateChunk();
                currentSizeLevel = mainGame.nextProgressionIsSize ? "L" : "S";
                mainGame.currentLevelStats = difficultyManager.GetOrCreateLevelStats((int)mainGame.currentDistanceFromCenter, currentSizeLevel);
                mainGame.currentChunkCorrect = 0;
                mainGame.currentChunkTotal = 0;
            }

            yield return new WaitForSeconds(mainGame.betweenShapesDuration/1000f);

            elapsedTime += Time.time - roundStartTime; //update elapsed time
        }
        if (mainGame.focusPoint != null)
            mainGame.focusPoint.gameObject.SetActive(false);
        if (mainGame.countdownText != null)
        {
            mainGame.countdownText.gameObject.SetActive(true);
            mainGame.countdownText.text = "םויס";
        }
        yield return new WaitForSeconds(3f);
        StatisticsLogger statsLogger = new StatisticsLogger(mainGame);
        statsLogger.LogGameStatistics();
    }

    // Coroutine to hide shapes after a delay
    IEnumerator HideShapesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (mainGame.leftShape != null) Object.Destroy(mainGame.leftShape);
        if (mainGame.rightShape != null && mainGame.rightShape != mainGame.leftShape) Object.Destroy(mainGame.rightShape);
    }
}


public class ShapeSpawner
{
    private GameLogic mainGame;

    public ShapeSpawner(GameLogic game)
    {
        mainGame = game;
    }

    // A method to spawn shapes
    public void SpawnShapes()
    {
        //Get camera transform
        Transform cam = Camera.main.transform;

        //Position relative to focus point
        Vector3 center = mainGame.focusPoint.position + mainGame.focusPoint.forward * mainGame.shapeDistance;
        float screenPercent = 0.3f + ((mainGame.currentDistanceFromCenter - 1f) / 9f) * 0.8f;

        Camera mainCamera = cam.GetComponent<Camera>();
        float halfHeight = mainGame.shapeDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * mainCamera.aspect;
        float horizontalOffset = halfWidth * screenPercent;
        Vector3 rightPos = center + mainGame.focusPoint.right * horizontalOffset;
        Vector3 leftPos = center - mainGame.focusPoint.right * horizontalOffset;

        //Choose a random image set from the active sets
        int setIndex = Random.Range(0, mainGame.activeImageSets.Count);
        List<GameObject> chosenSet = mainGame.activeImageSets[setIndex];

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

        mainGame.shapesAreSimilar = same;

        //Instantiate
        mainGame.rightShape = Object.Instantiate(right, rightPos, Quaternion.identity);
        mainGame.leftShape = Object.Instantiate(left, leftPos, Quaternion.identity);

        //Apply scale to shapes
        mainGame.rightShape.transform.localScale = Vector3.one * mainGame.shapeScale;
        mainGame.leftShape.transform.localScale = Vector3.one * mainGame.shapeScale;

        //make shapes face user
        mainGame.rightShape.transform.LookAt(cam);
        mainGame.leftShape.transform.LookAt(cam);

        if (mainGame.shapesAreSimilar) 
            mainGame.totalSimilarPairs++;
        else 
            mainGame.totalNonSimilarPairs++;
    }
}


public class DifficultyManager
{
    private GameLogic mainGame;
    private ImageSetManager imageSetManager;

    public DifficultyManager(GameLogic game, ImageSetManager imgSetMgr)
    {
        mainGame = game;
        imageSetManager = imgSetMgr;
    }

    //A method that evaluates chunk accuracy and adjusts difficulty accordingly
    public void EvaluateChunk()
    {
        float accuracy = (float)mainGame.currentChunkCorrect / mainGame.currentChunkTotal * 100f;

        Debug.Log("Chunk completed. Accuracy: " + accuracy.ToString("F1") + "% (" + mainGame.currentChunkCorrect + "/" + mainGame.currentChunkTotal + ")");

        if (accuracy >= mainGame.successRate)
        {
            if (mainGame.nextProgressionIsSize)
            {
                if (mainGame.shapeScale > 0.004f)
                {
                    mainGame.shapeScale = Mathf.Max(0.004f, mainGame.shapeScale - 0.0036f);
                    Debug.Log("Level UP! Shape size decreased to: " + (mainGame.shapeScale / 0.0036f));
                    mainGame.levelProgression += $" Up(D{(int)mainGame.currentDistanceFromCenter}L)";
                }
                else
                {
                    Debug.Log("Level UP! Size already at minimum, staying at current level");
                    mainGame.levelProgression += $" Same(D{(int)mainGame.currentDistanceFromCenter}L)";
                }
                mainGame.nextProgressionIsSize = false;
            }
            else
            {
                if (mainGame.currentDistanceFromCenter < mainGame.maxDistanceFromCenter)
                {
                    mainGame.currentDistanceFromCenter = Mathf.Min(mainGame.maxDistanceFromCenter, mainGame.currentDistanceFromCenter + 1f);
                    Debug.Log("Level UP! Distance increased to: " + mainGame.currentDistanceFromCenter);
                    mainGame.levelProgression += $" Up(D{(int)mainGame.currentDistanceFromCenter}S)";
                    mainGame.nextProgressionIsSize = true;
                }
                else
                {
                    Debug.Log("Level UP! Distance at maximum on distance's turn");
                    mainGame.levelProgression += $" Same(D{(int)mainGame.currentDistanceFromCenter}S)";

                    if (mainGame.isInShapesPhase && mainGame.otherSetsSelected.Count > 0)
                    {
                        imageSetManager.SwitchToOtherSets();
                    }
                    else
                    {
                        Debug.Log("Maximum distance reached! Ending game...");
                        if (mainGame.focusPoint != null)
                            mainGame.focusPoint.gameObject.SetActive(false);
                        if (mainGame.countdownText != null)
                        {
                            mainGame.countdownText.gameObject.SetActive(true);
                            mainGame.countdownText.text = "םויס";
                        }
                        StatisticsLogger statsLogger = new StatisticsLogger(mainGame);
                        statsLogger.LogGameStatistics();
                        mainGame.StopAllCoroutines();
                        return;
                    }
                }
            }
        }
        else if (accuracy <= mainGame.failRate)
        {
            if (mainGame.nextProgressionIsSize)
            {
                if (mainGame.shapeScale < 0.04f)
                {
                    mainGame.shapeScale = Mathf.Min(0.04f, mainGame.shapeScale + 0.0036f);
                    Debug.Log("Level DOWN! Shape size increased to: " + (mainGame.shapeScale / 0.0036f));
                    mainGame.levelProgression += $" Down(D{(int)mainGame.currentDistanceFromCenter}L)";
                }
                else
                {
                    Debug.Log("Level DOWN! Size already at maximum, staying at current level");
                    mainGame.levelProgression += $" Same(D{(int)mainGame.currentDistanceFromCenter}L)";
                }
                mainGame.nextProgressionIsSize = false;
            }
            else
            {
                if (mainGame.currentDistanceFromCenter > 1f)
                {
                    mainGame.currentDistanceFromCenter = Mathf.Max(1f, mainGame.currentDistanceFromCenter - 1f);
                    Debug.Log("Level DOWN! Distance decreased to: " + mainGame.currentDistanceFromCenter);
                    mainGame.levelProgression += $" Down(D{(int)mainGame.currentDistanceFromCenter}S)";
                }
                else
                {
                    Debug.Log("Level DOWN! Distance already at minimum, staying at current level");
                    mainGame.levelProgression += $" Same(D{(int)mainGame.currentDistanceFromCenter}S)";
                }
                mainGame.nextProgressionIsSize = true; 
            }
        }
        else
        {
            Debug.Log("Level maintained. Current: distance=" + mainGame.currentDistanceFromCenter + ", size=" + (mainGame.shapeScale / 0.005f));
            mainGame.levelProgression += $" Same(D{(int)mainGame.currentDistanceFromCenter}{(mainGame.nextProgressionIsSize ? "L" : "S")})";
        }
    }

    //A method that gets or creates LevelStats instance for given distance and scale
    public LevelStats GetOrCreateLevelStats(int distance, string sizeLevel)
    {
        string key = distance + "_" + sizeLevel;
        if (!mainGame.levelStatistics.ContainsKey(key))
        {
            mainGame.levelStatistics[key] = new LevelStats { distance = distance, sizeLevel = sizeLevel };
        }
        return mainGame.levelStatistics[key];
    }
}


public class StatisticsLogger
{
    private GameLogic mainGame;

    public StatisticsLogger(GameLogic game)
    {
        mainGame = game;
    }

    //A method that save game statistics to CSV files and launch GUI application to show results there 
    public void LogGameStatistics()
    {
        int totalTrials = mainGame.totalSimilarPairs + mainGame.totalNonSimilarPairs;
        float overallAccuracy = totalTrials > 0 ? (float)mainGame.correctResponses / totalTrials * 100f : 0f;
        float averageResponseTime = mainGame.responseCount > 0 ? (mainGame.totalResponseTime / mainGame.responseCount) : 0f;

        Debug.Log("=== RESULTS ===");
        Debug.Log("Overall Accuracy: " + overallAccuracy.ToString("F1") + "% (" + mainGame.correctResponses + "/" + totalTrials + ")");
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
            
            string eyeText = mainGame.loadedTrainingEye == 0 ? "Right" : "Left";
            string focusShapeText = mainGame.loadedFocusShape == 0 ? "Circle" : "Cross";
            string focusChangeModeText = mainGame.focusChangeMode == 0 ? "Fixed" : (mainGame.focusChangeMode == 1 ? "Changes at fixed intervals" : "Changes Randomly");
            string intervalSetsText = mainGame.focusChangeMode == 1 ? mainGame.intervalSets.ToString() : "N/A";

            string dataLine = $"{mainGame.loadedUserID},{mainGame.loadedTimestamp},{eyeText},{mainGame.gameDuration / 60f},{mainGame.loadedFocusY * 100f},{mainGame.loadedFocusScale * 100f},{focusShapeText},{mainGame.shapeDisplayDuration},{mainGame.betweenShapesDuration},{focusChangeModeText},{intervalSetsText},{mainGame.successRate}%,{mainGame.failRate}%,{mainGame.chunkSize},{mainGame.loadedStartingDistance},{Mathf.RoundToInt(mainGame.loadedShapeScale / 0.005f)},{accuracy:F1}%,{avgResponseTime:F2},{totalTrials},{mainGame.correctResponses}";
            d = 1;
            sLevel = "L";
            for (int i = 0; i < 20; i++)
            {
                string key = d + "_" + sLevel;
                if (mainGame.levelStatistics.ContainsKey(key) && mainGame.levelStatistics[key].totalTrials > 0)
                {
                    LevelStats stats = mainGame.levelStatistics[key];
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
            dataLine += $",{mainGame.levelProgression}";
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