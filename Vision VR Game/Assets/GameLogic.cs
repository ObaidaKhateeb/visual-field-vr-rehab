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
    private List<List<GameObject>> activeImageSets = new List<List<GameObject>>();

    public float shapeDistance = 2f; //Distance from camera
    public float sideOffset = 0.35f;  // Left/right separation
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
    

    private float successRate = 80f; // Rate of sets should answered True to count as success
    private float failRate = 20f; // Rate of sets should answered False to count as failure
    private int chunkSize = 15; // Chunk size
    private int currentChunkCorrect = 0;
    private int currentChunkTotal = 0;
    private float currentDistanceFromCenter = 1f; //Current distance from center (1-10)
    private float shapeScale = 0.05f; // Scale of the shapes
    private bool nextProgressionIsSize = true;


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

            //Success, Fail, and Chunk definitions
            successRate = settings.successRate;
            failRate = settings.failRate;
            chunkSize = settings.chunkSize;
            
            //Focus point settings: location, size, shape, and change mode.
            ApplyFocusSettings(settings);
            SetActiveImageSets(settings.imageSets);
            
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
        activeImageSets.Clear(); // Clear any previous selections
        
        foreach(int setNumber in setNumbers)
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
            Debug.LogWarning("No image sets selected, using all image sets as default");
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
                if (inputAccepted && Input.GetKeyDown(KeyCode.Space))
                {
                    responded = true;
                    if (shapesAreSimilar)
                    {
                        Debug.Log("Correct (Shapes are similar)");
                        audioSource.PlayOneShot(correctSound);
                        correctResponses++;
                        totalResponseTime += timer;
                        responseCount++;
                    }
                    else
                    {
                        Debug.Log("Incorrect (Shapes are different)");
                        audioSource.PlayOneShot(incorrectSound);
                        incorrectResponses++;
                    }
                    inputAccepted = false;
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
                    correctResponses++;
                }
                else
                {
                    Debug.Log("Incorrect (Shapes are similar)");
                    audioSource.PlayOneShot(incorrectSound);
                    incorrectResponses++;
                }
            }

            //Clean up
            Destroy(leftShape);
            Destroy(rightShape);
            inputAccepted = false;

            // Track chunk progress
            currentChunkTotal++;
            if ((responded && shapesAreSimilar) || (!responded && !shapesAreSimilar))
            {
                currentChunkCorrect++;
            }
            
            // Check if chunk is complete
            if (currentChunkTotal >= chunkSize)
            {
                EvaluateChunk();
                currentChunkCorrect = 0;
                currentChunkTotal = 0;
            }

            yield return new WaitForSeconds(betweenShapesDuration/1000f);

            elapsedTime += Time.time - roundStartTime; //update elapsed time
        }
        LogGameStatistics();
    }

    // IEnumerator RunTrials()
    // {
    //     float elapsedTime = 0f;
    //     while (elapsedTime < gameDuration)
    //     {
    //         float roundStartTime = Time.time; //round start time 

    //         // Changing focus point position logic 
    //         setsUntilChange--; 
    //         if (focusChangeMode != 0 && setsUntilChange <= 0 && !waitingForFocusChange)
    //         {
    //             waitingForFocusChange = true;
    //             ChangeFocusPoint();
    //             yield return new WaitForSeconds(1f); //wait 1 second after changing
    //             ResetSetCounter();
    //             waitingForFocusChange = false;
    //         }

    //         // Shapes choosing and showing
    //         SpawnShapes();
    //         // shapes hide 
    //         StartCoroutine(HideShapesAfterDelay(shapeDisplayDuration/1000f));
    //         inputAccepted = true;
    //         bool responded = false;

    //         // Wait for up to 2 seconds or betweenShapesDuration time for user to press SPACE
    //         float maxResponseTime = Mathf.Min(2f, betweenShapesDuration/1000f);
    //         float timer = 0f;
    //         while (timer < maxResponseTime)
    //         {
    //             if (!inputAccepted)
    //             {
    //                 responded = true;
    //                 if (shapesAreSimilar)
    //                 {
    //                     totalResponseTime += timer;
    //                     responseCount++;
    //                 }
    //                 break;  // User pressed SPACE
    //             }

    //             timer += Time.deltaTime;
    //             yield return null;
    //         }

    //         //Evaluate non-response if SPACE wasn't pressed
    //         if (!responded)
    //         {
    //             if (!shapesAreSimilar)
    //             {
    //                 Debug.Log("Correct (Shapes are different)");
    //                 audioSource.PlayOneShot(correctSound);
    //                 consecutiveCorrect++;
    //                 consecutiveIncorrect = 0;
    //                 correctResponses++;
    //             }
    //             else
    //             {
    //                 Debug.Log("Incorrect (Shapes are similar)");
    //                 audioSource.PlayOneShot(incorrectSound);
    //                 consecutiveIncorrect++;
    //                 consecutiveCorrect = 0;
    //                 incorrectResponses++;
    //             }
    //         }

    //         //Clean up
    //         Destroy(leftShape);
    //         Destroy(rightShape);
    //         inputAccepted = false;

    //         // Track chunk progress
    //         currentChunkTotal++;
    //         if (responded && shapesAreSimilar) // correctly responded to similar shapes
    //         {
    //             currentChunkCorrect++;
    //         }
    //         else if (!responded && !shapesAreSimilar) //correctly didn't responded to non-similar shapes
    //         {
    //             currentChunkCorrect++;
    //         }

    //         // Check if chunk is complete
    //         if (currentChunkTotal >= chunkSize)
    //         {
    //             EvaluateChunk();
    //             currentChunkCorrect = 0;
    //             currentChunkTotal = 0;
    //         }

    //         yield return new WaitForSeconds(betweenShapesDuration / 1000f);

    //         elapsedTime += Time.time - roundStartTime; //update elapsed time
    //     }
    //     LogGameStatistics();
    // }

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

        // Choose a random image set from the active sets
        int setIndex = Random.Range(0, activeImageSets.Count);
        List<GameObject> chosenSet = activeImageSets[setIndex];

        // Choose right shape from that set
        int rightIndex = Random.Range(0, chosenSet.Count);
        GameObject right = chosenSet[rightIndex];

        // 50% chance to match
        bool same = Random.value < 0.5f;
        GameObject left;
        if (same)
        {
            left = right;
        }
        else
        {
            // Pick a different shape from the SAME set
            int leftIndex = Random.Range(0, chosenSet.Count);
            left = chosenSet[leftIndex];
        }

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


    void EvaluateChunk()
    {
        float accuracy = (float)currentChunkCorrect / currentChunkTotal * 100f;
        
        Debug.Log("Chunk completed. Accuracy: " + accuracy.ToString("F1") + "% (" + currentChunkCorrect + "/" + currentChunkTotal + ")");
        
        if (accuracy >= successRate)
        {
            // Level UP - alternate between size and distance
            if (nextProgressionIsSize)
            {
                // Try to decrease size (make shapes smaller/harder)
                if (shapeScale > 0.005f) // Minimum size check (1 * 0.005f)
                {
                    shapeScale = Mathf.Max(0.005f, shapeScale - 0.005f);
                    Debug.Log("Level UP! Shape size decreased to: " + (shapeScale / 0.005f));
                    nextProgressionIsSize = false; // Next time change distance
                }
                else if (currentDistanceFromCenter < 10f)
                {
                    // Size at minimum, try distance instead
                    currentDistanceFromCenter = Mathf.Min(10f, currentDistanceFromCenter + 1f);
                    Debug.Log("Level UP! Size at minimum, distance increased to: " + currentDistanceFromCenter);
                    nextProgressionIsSize = false; // Next time still try size first
                }
                else
                {
                    Debug.Log("Level UP! Already at maximum difficulty (size=1, distance=10)");
                }
            }
            else
            {
                // Try to increase distance
                if (currentDistanceFromCenter < 10f)
                {
                    currentDistanceFromCenter = Mathf.Min(10f, currentDistanceFromCenter + 1f);
                    Debug.Log("Level UP! Distance increased to: " + currentDistanceFromCenter);
                    nextProgressionIsSize = true; // Next time change size
                }
                else if (shapeScale > 0.005f)
                {
                    // Distance at maximum, try size instead
                    shapeScale = Mathf.Max(0.005f, shapeScale - 0.005f);
                    Debug.Log("Level UP! Distance at maximum, size decreased to: " + (shapeScale / 0.005f));
                    nextProgressionIsSize = true; // Next time still try distance first
                }
                else
                {
                    Debug.Log("Level UP! Already at maximum difficulty (size=1, distance=10)");
                }
            }
        }
        else if (accuracy <= failRate)
        {
            // Level DOWN - alternate between size and distance (opposite direction)
            if (nextProgressionIsSize)
            {
                // Try to increase size (make shapes bigger/easier)
                if (shapeScale < 0.05f) // Maximum size check (10 * 0.005f)
                {
                    shapeScale = Mathf.Min(0.05f, shapeScale + 0.005f);
                    Debug.Log("Level DOWN! Shape size increased to: " + (shapeScale / 0.005f));
                    nextProgressionIsSize = false; // Next time change distance
                }
                else if (currentDistanceFromCenter > 1f)
                {
                    // Size at maximum, try distance instead
                    currentDistanceFromCenter = Mathf.Max(1f, currentDistanceFromCenter - 1f);
                    Debug.Log("Level DOWN! Size at maximum, distance decreased to: " + currentDistanceFromCenter);
                    nextProgressionIsSize = false; // Next time still try size first
                }
                else
                {
                    Debug.Log("Level DOWN! Already at minimum difficulty (size=10, distance=1)");
                }
            }
            else
            {
                // Try to decrease distance
                if (currentDistanceFromCenter > 1f)
                {
                    currentDistanceFromCenter = Mathf.Max(1f, currentDistanceFromCenter - 1f);
                    Debug.Log("Level DOWN! Distance decreased to: " + currentDistanceFromCenter);
                    nextProgressionIsSize = true; // Next time change size
                }
                else if (shapeScale < 0.05f)
                {
                    // Distance at minimum, try size instead
                    shapeScale = Mathf.Min(0.05f, shapeScale + 0.005f);
                    Debug.Log("Level DOWN! Distance at minimum, size increased to: " + (shapeScale / 0.005f));
                    nextProgressionIsSize = true; // Next time still try distance first
                }
                else
                {
                    Debug.Log("Level DOWN! Already at minimum difficulty (size=10, distance=1)");
                }
            }
        }
        else
        {
            Debug.Log("Level maintained. Current: distance=" + currentDistanceFromCenter + ", size=" + (shapeScale / 0.005f));
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
        public float successRate = 80f;
        public float failRate = 20f;
        public int chunkSize = 15;
        public List<int> imageSets = new List<int>();
    }
}