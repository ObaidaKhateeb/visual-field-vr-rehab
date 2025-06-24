using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameLogic : MonoBehaviour
{
    public List<GameObject> shapePrefabs;  //Shapes
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
            }
            else
            {
                Debug.Log("Incorrect (Shapes are different)");
                audioSource.PlayOneShot(incorrectSound);
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
            
            //Focus point settings: location, size, shape, and change mode.
            ApplyFocusSettings(settings);
            
            Debug.Log("Settings loaded successfully");
        }
        else
        {
            Debug.Log("No settings file found, using defaults");
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

            yield return new WaitForSeconds(betweenShapesDuration/1000f);

            elapsedTime += Time.time - roundStartTime; //update elapsed time
        }
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
        Vector3 rightPos = center + focusPoint.right * 5 * sideOffset;
        Vector3 leftPos = center - focusPoint.right * 5 * sideOffset;


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

        // Optional: make shapes face user
        rightShape.transform.LookAt(cam);
        leftShape.transform.LookAt(cam);
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
}