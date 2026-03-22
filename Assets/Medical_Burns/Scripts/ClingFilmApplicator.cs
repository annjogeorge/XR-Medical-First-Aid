using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attach to: ClingFilm roll GameObject
/// Player grabs it and moves it around burn to wrap
/// Same movement-based logic as BandageWrap but fewer strips
/// Completes Step 3 for First Degree burn
/// </summary>
public class ClingFilmApplicator : MonoBehaviour
{
    [Header("Cling Film Strips")]
    public GameObject[] clingFilmStrips;        // 2-3 strips, fewer than bandage
    public float wrapDistanceRequirement = 0.12f;
    public float timeBetweenStrips = 0.4f;

    [Header("Progress UI")]
    public Image progressFill;
    public TextMeshProUGUI progressText;
    public GameObject progressPanel;

    [Header("Settings")]
    public string burnZoneTag = "BurnArea";

    private int _currentStripIndex = 0;
    private float _lastActivationTime;
    private Vector3 _lastWrapPosition;
    private bool _isInitialized = false;
    private bool _isHeld = false;
    private bool _complete = false;
    private XRGrabInteractable _grab;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        if (_grab == null)
            _grab = gameObject.AddComponent<XRGrabInteractable>();

        _grab.selectEntered.AddListener(_ =>
        {
            _isHeld = true;
            Debug.Log("CLINGFILM: Picked up");
        });

        _grab.selectExited.AddListener(_ =>
        {
            _isHeld = false;
            _isInitialized = false;
            if (progressPanel != null) progressPanel.SetActive(false);
            Debug.Log("CLINGFILM: Put down");
        });
    }

    public void OnBurnEnter(Collider other)
    {
        if (!_isHeld || _complete) return;
        if (!other.CompareTag(burnZoneTag)) return;

        _lastWrapPosition = transform.position;
        _isInitialized = true;

        if (progressPanel != null) progressPanel.SetActive(true);
        UpdateProgressUI();
        Debug.Log("CLINGFILM: Entered burn zone");
    }

    public void OnBurnStay(Collider other)
    {
        if (!_isHeld || !_isInitialized || _complete) return;
        if (!other.CompareTag(burnZoneTag)) return;
        if (_currentStripIndex >= clingFilmStrips.Length) return;

        float distanceMoved = Vector3.Distance(transform.position, _lastWrapPosition);

        if (distanceMoved > wrapDistanceRequirement
            && Time.time > _lastActivationTime + timeBetweenStrips)
        {
            clingFilmStrips[_currentStripIndex].SetActive(true);
            _currentStripIndex++;
            _lastWrapPosition = transform.position;
            _lastActivationTime = Time.time;

            Debug.Log("CLINGFILM: Strip " + (_currentStripIndex - 1) + " applied");
            UpdateProgressUI();

            if (!_complete && _currentStripIndex >= clingFilmStrips.Length)
            {
                _complete = true;
                OnWrappingComplete();
            }
        }
    }

    public void OnBurnExit(Collider other)
    {
        if (!other.CompareTag(burnZoneTag)) return;
        _isInitialized = false;
        if (progressPanel != null) progressPanel.SetActive(false);
        Debug.Log("CLINGFILM: Left burn zone");
    }

    void OnWrappingComplete()
    {
        Debug.Log("CLINGFILM: Complete");
        if (progressPanel != null) progressPanel.SetActive(false);
        gameObject.SetActive(false);

        // Find which step CoverLoosely is on
        BurnProfile burn = TreatmentManager.Instance?.currentBurnProfile;
        if (burn != null)
        {
            for (int i = 0; i < burn.treatmentSteps.Length; i++)
            {
                if (burn.treatmentSteps[i].stepType == StepType.CoverLoosely)
                {
                    int step = i + 1;
                    if (step == 1) TreatmentManager.Instance?.TryCompleteStep1();
                    if (step == 2) TreatmentManager.Instance?.TryCompleteStep2();
                    if (step == 3) TreatmentManager.Instance?.TryCompleteStep3();
                    return;
                }
            }
        }
    }

    void UpdateProgressUI()
    {
        if (clingFilmStrips.Length == 0) return;

        float progress = (float)_currentStripIndex / clingFilmStrips.Length;

        if (progressFill != null)
            progressFill.fillAmount = progress;

        if (progressText != null)
        {
            progressText.text = _currentStripIndex < clingFilmStrips.Length
                ? $"Wrapping... {_currentStripIndex}/{clingFilmStrips.Length}"
                : "Covered!";
        }
    }
}