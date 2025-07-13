using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    public List<GameObject> shapePrefabs; //will be set to the chosen set

    public float shapeDistance = 2f; //Distance from camera
    public float sideOffset = 0.5f;  // Left/right separation
    public float gameDuration = 10f;   //number of rounds
    public float shapeDisplayDuration = 1500f; //Duration of showing shapes
    public float betweenShapesDuration = 1500f; //Duration between showing sets
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
    

    private int successSets; // Number of sets should answered True to count as success
    private int failureSets; // Number of sets should answered False to count as failure
    private int consecutiveCorrect = 0; 
    private int consecutiveIncorrect = 0;
    private float currentDistanceFromCenter = 1f; //Current distance from center (1-5)
    private float shapeScale = 0.05f; // Scale of the shapes


    private int totalSimilarPairs = 0;
    private int totalNonSimilarPairs = 0;
    private int correctResponses = 0;
    private int incorrectResponses = 0;
    private float totalResponseTime = 0f;
    private int responseCount = 0;

    void Start()
    {
        LoadSettings();
        StartCoroutine(RunTrials());
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (inputAccepted && Input.GetKeyDown(KeyCode.Space))
        {
            if (shapesAreSimilar)
            {
                Debug.Log("Correct (Shapes are similar)");
                audioSource.PlayOneShot(correctSound);
                consecutiveCorrect++;
                consecutiveIncorrect = 0;
                CheckDistanceFromCenterIncr();
            }
            else
            {
                Debug.Log("Incorrect (Shapes are different)");
                audioSource.PlayOneShot(incorrectSound);
                consecutiveIncorrect++;
                consecutiveCorrect = 0;
                CheckDistanceFromCenterDecr();
            }
            inputAccepted = false;
        }
    }

    void LoadSettings()
    {
        string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "vr_settings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            VRSettings settings = JsonUtility.FromJson<VRSettings>(json);
            
            // Durations: game, set display, and between sets.
            gameDuration = settings.gameDuration;
            shapeDisplayDuration = settings.shapeDisplayDuration;
            betweenShapesDuration = settings.betweenShapesDuration;
            currentDistanceFromCenter = settings.startingDistance;
            shapeScale = settings.shapeScale;

            //Success and Fail definitions
            successSets = settings.successSets;
            failureSets = settings.failureSets;
            
            //Focus point settings: location, size, shape, and change mode.
            ApplyFocusSettings(settings);
            SetActiveImageSet(settings.imageSet);
            
            Debug.Log("Settings loaded successfully");
        }
        else
        {
            Debug.Log("No settings file found, using defaults");
        }
    }

    //a function responsible for settings the chosen set of images 
    void SetActiveImageSet(int setNumber)
    {
        switch(setNumber)
        {
            case 1: shapePrefabs = imageSet1; break;
            case 2: shapePrefabs = imageSet2; break;
            case 3: shapePrefabs = imageSet3; break;
            case 4: shapePrefabs = imageSet4; break;
            case 5: shapePrefabs = imageSet5; break;
            case 6: shapePrefabs = imageSet6; break;
            case 7: shapePrefabs = imageSet7; break;
            case 8: shapePrefabs = imageSet8; break;
            case 9: shapePrefabs = imageSet9; break;
            case 10: shapePrefabs = imageSet10; break;
            default: 
                shapePrefabs = imageSet1; 
                break;
        }
    }

    void ApplyFocusSettings(VRSettings settings)
    {
        // Find both focus point GameObjects
        GameObject focusCircle = GameObject.Find("FocusPointCircle");  
        GameObject focusCross = GameObject.Find("FocusPointCross");   
        
        if (focusCircle != null && focusCross != null)
        {
            if (settings.focusShape == 0) // Circle
            {
                focusCircle.SetActive(true);
                focusCross.SetActive(false);
                focusPoint = focusCircle.transform;
            }
            else // Cross
            {
                focusCircle.SetActive(false);
                focusCross.SetActive(true);
                focusPoint = focusCross.transform;
            }
            
            // Scale and position of focus point
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

    //Reset the set counter and determine how many sets until the next focus change
    void ResetSetCounter()
    {        
        if (focusChangeMode == 1) // Interval
            setsUntilChange = intervalSets;
        else if (focusChangeMode == 2) // Random
            setsUntilChange = Random.Range(1, 11);
    }

    //Chnge the focus point position randomly along the Y-axis
    void ChangeFocusPoint()
    {
        Vector3 newPos = focusPoint.localPosition;
        newPos.y = Random.Range(-0.5f, 0.5f);
        focusPoint.localPosition = newPos;
        
        Debug.Log("Focus point changed to: " + newPos);
    }

    IEnumerator RunTrials()
    {
        float elapsedTime = 0f;
        while (elapsedTime < gameDuration)
        {
            float roundStartTime = Time.time; //round start time 
            
            // Changing focus point position logic 
            setsUntilChange--; 
            if (focusChangeMode != 0 && setsUntilChange <= 0 && !waitingForFocusChange)
            {
                waitingForFocusChange = true;
                ChangeFocusPoint();
                yield return new WaitForSeconds(1f); //wait 1 second after changing
                ResetSetCounter();
                waitingForFocusChange = false;
            }

            // Shapes choosing and showing
            SpawnShapes();
            // shapes hide 
            StartCoroutine(HideShapesAfterDelay(shapeDisplayDuration/1000f));
            inputAccepted = true;
            bool responded = false;

            // Wait for up to 2 seconds or betweenShapesDuration time for user to press SPACE
            float maxResponseTime = Mathf.Min(2f, betweenShapesDuration/1000f);
            float timer = 0f;
            while (timer < maxResponseTime)
            {
                if (!inputAccepted)
                {
                    responded = true;
                    if (shapesAreSimilar)
                    {
                        totalResponseTime += timer;
                        responseCount++;
                    }
                    break;  // User pressed SPACE
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
                    consecutiveCorrect++;
                    consecutiveIncorrect = 0;
                    CheckDistanceFromCenterIncr();
                    correctResponses++;
                }
                else
                {
                    Debug.Log("Incorrect (Shapes are similar)");
                    audioSource.PlayOneShot(incorrectSound);
                    consecutiveIncorrect++;
                    consecutiveCorrect = 0;
                    CheckDistanceFromCenterDecr();
                    incorrectResponses++;
                }
            }

            //Clean up
            Destroy(leftShape);
            Destroy(rightShape);
            inputAccepted = false;

            yield return new WaitForSeconds(betweenShapesDuration/1000f);

            elapsedTime += Time.time - roundStartTime; //update elapsed time
        }
        LogGameStatistics();
    }

    // Coroutine to hide shapes after a delay
    IEnumerator HideShapesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (leftShape != null) Destroy(leftShape);
        if (rightShape != null && rightShape != leftShape) Destroy(rightShape);
    }

    void SpawnShapes()
    {
        // Get camera transform
        Transform cam = Camera.main.transform;

        // Position relative to camera
        // Vector3 center = cam.position + cam.forward * shapeDistance;
        // Vector3 rightPos = center + cam.right * sideOffset;
        // Vector3 leftPos = center - cam.right * sideOffset;

        //Position relative to focus point
        Vector3 center = focusPoint.position + focusPoint.forward * shapeDistance;
        Vector3 rightPos = center + focusPoint.right * currentDistanceFromCenter * sideOffset;
        Vector3 leftPos = center - focusPoint.right * currentDistanceFromCenter * sideOffset;


        // Choose right shape
        int index = Random.Range(0, shapePrefabs.Count);
        GameObject right = shapePrefabs[index];

        // 50% chance to match
        bool same = Random.value < 0.5f;
        GameObject left = same ? right : shapePrefabs[Random.Range(0, shapePrefabs.Count)];

        shapesAreSimilar = same;

        // Instantiate
        rightShape = Instantiate(right, rightPos, Quaternion.identity);
        leftShape = Instantiate(left, leftPos, Quaternion.identity);

        // Apply scale to shapes
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

    void CheckDistanceFromCenterIncr()
    {
        if (consecutiveCorrect >= successSets && currentDistanceFromCenter < 5f)
        {
            currentDistanceFromCenter = Mathf.Min(5f, currentDistanceFromCenter + 1f);
            consecutiveCorrect = 0;
            Debug.Log("Difficulty increased. New multiplier: " + currentDistanceFromCenter);
        }
    }

    void CheckDistanceFromCenterDecr()
    {
        if (consecutiveIncorrect >= failureSets && currentDistanceFromCenter > 1f)
        {
            currentDistanceFromCenter = Mathf.Max(1f, currentDistanceFromCenter - 1f);
            consecutiveIncorrect = 0;
            Debug.Log("Difficulty decreased. New multiplier: " + currentDistanceFromCenter);
        }
    }

    void LogGameStatistics()
    {
        float successRate = totalSimilarPairs > 0 ? (float)correctResponses / totalSimilarPairs * 100f : 0f;
        
        Debug.Log("=== RESULTS ===");
        Debug.Log("Success Rate: " + successRate.ToString("F1") + "% (" + correctResponses + "/" + totalSimilarPairs + ")");
        float failRate = totalNonSimilarPairs > 0 ? (float)incorrectResponses / totalNonSimilarPairs * 100f : 0f;
        Debug.Log("Fail Rate: " + failRate.ToString("F1") + "% (" + incorrectResponses + "/" + totalNonSimilarPairs + ")");
        Debug.Log("Average Response Time: " + (responseCount > 0 ? (totalResponseTime / responseCount).ToString("F2") : "0") + " seconds");
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
        public int successSets;
        public int failureSets;
        public int imageSet = 1;
    }
}