using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CPRCompressionLogic : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timeRemaining;
    public TextMeshProUGUI bpmText;
    public GameObject infoPanel;

    [Header("Placement UI")]
    public GameObject placementImage;

    [Header("References")]
    public Transform surfaceRef;
    public ActionBasedController hand;

    [Header("Audio")]
    public AudioSource metronomeAudio;
    public float metronomeBPM = 110f;

    [Header("Compression Settings")]
    public float targetDepth = 0.05f;
    public float releasePoint = 0.01f;
    public float minTimeBetweenCompressions = 0.25f;

    private int compressionCount = 0;
    private bool isPushing = false;
    private float lastCompressionTime;
    private bool isTouching = false;
    private Renderer chestRenderer;
    private bool isTimerRunning = false;
    private int sessionTimeRemaining = 60;
    private bool sessionFinished = false;

    private List<float> compressionTimes = new List<float>();

    void Start()
    {
        chestRenderer = GetComponent<Renderer>();
        if (bpmText != null) bpmText.text = "BPM: 0";
        UpdateUI();
    }

    void Update()
    {
        if (sessionFinished || !isTouching || hand == null || surfaceRef == null) return;

        // Calculate depth (Positive value means pushing down)
        float currentDepth = surfaceRef.position.y - hand.transform.position.y;

        // 1. DETECTION LOGIC (The Downward Push)
        // Corrected: Only count if we aren't already marked as 'isPushing'
        if (currentDepth >= targetDepth && !isPushing)
        {
            if ((Time.time - lastCompressionTime) > minTimeBetweenCompressions)
            {
                compressionCount++;
                isPushing = true; // LOCK: Prevents counting again until releasePoint is hit

                RegisterCompression();
                UpdateUI();

                if (chestRenderer) chestRenderer.material.color = Color.blue;
            }
        }

        // 2. RELEASE LOGIC (The Upward Lift)
        // Reset the lock only when the hand is lifted back near the surface
        if (currentDepth <= releasePoint && isPushing)
        {
            isPushing = false; // UNLOCK: Ready for the next compression
            if (chestRenderer) chestRenderer.material.color = Color.green;
        }

        // Haptics
        if (currentDepth > 0.01f)
        {
            float intensity = Mathf.Clamp01(currentDepth / targetDepth);
            hand.SendHapticImpulse(intensity, 0.05f);
        }
    }

    private void RegisterCompression()
    {
        float currentTime = Time.time;

        if (!isTimerRunning)
        {
            if (placementImage != null) placementImage.SetActive(false);
            StartCoroutine(StartCPRSession());
        }
        else
        {
            float timeDiff = currentTime - lastCompressionTime;
            CalculateSmoothedBPM(timeDiff);
        }

        lastCompressionTime = currentTime;
    }

    private void CalculateSmoothedBPM(float lastTimeDiff)
    {
        compressionTimes.Add(60f / lastTimeDiff);

        if (compressionTimes.Count > 5) compressionTimes.RemoveAt(0);

        float sum = 0;
        foreach (float b in compressionTimes) sum += b;
        float averageBPM = sum / compressionTimes.Count;

        if (bpmText != null) bpmText.text = "BPM: " + averageBPM.ToString("F0");

        if (averageBPM < 100) feedbackText.text = "Push Faster!";
        else if (averageBPM > 120) feedbackText.text = "Too Fast!";
        else feedbackText.text = "Perfect Pace!";
    }

    IEnumerator StartCPRSession()
    {
        isTimerRunning = true;
        sessionFinished = false;
        sessionTimeRemaining = 60;
        StartCoroutine(MetronomeLoop());

        while (sessionTimeRemaining > 0)
        {
            if (timeRemaining != null)
                timeRemaining.text = "Time Left: " + sessionTimeRemaining + "s";
            yield return new WaitForSeconds(1.0f);
            sessionTimeRemaining--;
        }

        isTimerRunning = false;
        sessionFinished = true;
        if (timeRemaining != null) timeRemaining.text = "FINISHED!";
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(ResetSessionRoutine());
    }

    IEnumerator MetronomeLoop()
    {
        float interval = 60f / metronomeBPM;
        while (isTimerRunning)
        {
            if (metronomeAudio != null) metronomeAudio.Play();
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator ResetSessionRoutine()
    {
        compressionCount = 0;
        compressionTimes.Clear();
        sessionFinished = false;
        UpdateUI();
        if (feedbackText != null) feedbackText.text = "Push to Start!";
        yield return null;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Count: " + compressionCount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand")) isTouching = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand")) { isTouching = false; isPushing = false; }
    }
}