using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attach to: InspectionPanel
/// Fixed: leader lines connect to individual rows, characteristics no longer overlap
/// </summary>
public class UIAnimator : MonoBehaviour
{
    [Header("Panel References")]
    public CanvasGroup canvasGroup;
    public RectTransform panelRect;
    public Image panelBackground;

    [Header("Camera Zoom")]
    public Transform xrOrigin;
    public Transform viewPoint;        // empty GameObject positioned in front of panel
    public float zoomDuration = 1.2f;
    public Ease zoomEase = Ease.InOutSine;
    public float returnDuration = 0.4f;
    public Ease returnEase = Ease.InOutSine;
    public float pullBackDistance = 0.5f;

    private Vector3 _originStartPos;
    private Quaternion _originStartRot;
    private bool _isZoomed = false;

    [Header("Text Fields")]
    public TextMeshProUGUI degreeText;
    public TextMeshProUGUI causeText;
    public TextMeshProUGUI descriptionText;

    [Header("Characteristics")]
    public Transform characteristicsContainer;
    public GameObject characteristicRowPrefab;

    [Header("Leader Lines")]
    public GameObject leaderLinePrefab;

    [Header("Treatment Button")]
    public StartTreatmentButton startTreatmentButton;

    [Header("Colors")]
    public Color panelColor = new Color(0.97f, 0.97f, 0.97f, 0.95f);
    public Color headerColor = new Color(0.10f, 0.10f, 0.10f, 1.00f);
    public Color subTextColor = new Color(0.35f, 0.35f, 0.35f, 1.00f);
    public Color bodyTextColor = new Color(0.20f, 0.20f, 0.20f, 1.00f);
    public Color bulletColor = new Color(0.80f, 0.10f, 0.10f, 1.00f);
    public Color lineColor = new Color(0.80f, 0.10f, 0.10f, 0.90f);

    [Header("Animation")]
    public float fadeInDuration = 0.3f;
    public float scaleDuration = 0.4f;
    public float typingSpeed = 0.025f;
    public float rowStaggerDelay = 0.2f;

    // Internal state
    private Coroutine _animCoroutine;
    private bool _isVisible = false;
    private BurnZoneData _currentData;
    private List<LineRenderer> _activeLines = new List<LineRenderer>();
    private List<RectTransform> _activeRows = new List<RectTransform>();

    private Vector3 _originalScale;

    void Awake()
    {
        _originalScale = transform.localScale; // save YOUR scale first
        transform.localScale = Vector3.zero;   // then zero for animation
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }



    // ─── Public API ───────────────────────────────────────────────────────────

    public void ShowPanel(BurnZoneData data)
    {
        if (_isVisible) return;
        _isVisible = true;
        _currentData = data;


        ZoomIn();

        if (degreeText != null) degreeText.text = data.burnDegree.ToUpper() + " BURN";
        if (causeText != null) causeText.text = data.burnCause;
        if (descriptionText != null) descriptionText.text = "";

        ClearAll();

        DOVirtual.DelayedCall(zoomDuration * 0.8f, () =>
        {
            panelRect.DOScale(_originalScale, scaleDuration).SetEase(Ease.OutBack);
            if (canvasGroup != null) canvasGroup.DOFade(1f, fadeInDuration);
        }); if (canvasGroup != null) canvasGroup.DOFade(1f, fadeInDuration);

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateContent(data));

        data.ActivateHighlight();

        // Show treatment button after panel appears
        if (startTreatmentButton != null)
            startTreatmentButton.Show();
    }

    public void HidePanel()
    {
        if (!_isVisible) return;
        _isVisible = false;

        ZoomOut();
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);

        panelRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(ClearAll);

        if (_currentData != null) _currentData.DeactivateHighlight();

        // Hide treatment button too
        if (startTreatmentButton != null)
            startTreatmentButton.Hide();
    }

    void ZoomIn()
    {
        if (xrOrigin == null || viewPoint == null || _isZoomed) return;
        _isZoomed = true;

        _originStartPos = xrOrigin.position;
        _originStartRot = xrOrigin.rotation;

        // Find the actual camera inside XR Origin
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Calculate offset between XR Origin and camera
            Vector3 cameraOffset = xrOrigin.position - cam.transform.position;

            // Move origin so CAMERA lands at viewPoint, not origin
            float pullBackDistance = 0.5f; // increase this to stop further away
            Vector3 targetPos = viewPoint.position + cameraOffset
                              + (viewPoint.forward * -pullBackDistance);

            xrOrigin.DOKill();
            xrOrigin.DOMove(targetPos, zoomDuration).SetEase(zoomEase);
            xrOrigin.DORotateQuaternion(viewPoint.rotation, zoomDuration).SetEase(zoomEase);
        }
        else
        {
            // Fallback if no camera found
            xrOrigin.DOMove(viewPoint.position, zoomDuration).SetEase(zoomEase);
            xrOrigin.DORotateQuaternion(viewPoint.rotation, zoomDuration).SetEase(zoomEase);
        }
    }

    void ZoomOut()
    {
        if (!_isZoomed) return;
        _isZoomed = false;

        xrOrigin.DOKill();
        xrOrigin.DOMove(_originStartPos, returnDuration)
                .SetEase(returnEase);
        xrOrigin.DORotateQuaternion(_originStartRot, returnDuration)
                .SetEase(returnEase);
    }

    // Called by LeaderLineController every LateUpdate
    // Each line: burn annotation point → its own characteristic row
    public void UpdateLeaderLines(BurnZoneData data)
    {
        for (int i = 0; i < _activeLines.Count; i++)
        {
            if (_activeLines[i] == null) continue;
            if (i >= data.annotations.Count) continue;

            // Point A: specific spot on the burn surface
            Vector3 burnPoint = data.GetAnnotationWorldPosition(i);

            // Point B: the matching row's world position
            Vector3 rowPoint = burnPoint; // fallback
            if (i < _activeRows.Count && _activeRows[i] != null)
            {
                // Left edge of the row in world space
                rowPoint = _activeRows[i].TransformPoint(
                    new Vector3(-_activeRows[i].rect.width * 0.5f, 0f, 0f)
                );
            }

            _activeLines[i].SetPosition(0, burnPoint);
            _activeLines[i].SetPosition(1, rowPoint);
        }
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    IEnumerator AnimateContent(BurnZoneData data)
    {
        yield return new WaitForSeconds(0.3f);

        // Type out description
        if (descriptionText != null)
        {
            foreach (char c in data.burnDescription)
            {
                descriptionText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Reveal each characteristic row one at a time with its own leader line
        for (int i = 0; i < data.annotations.Count; i++)
        {
            RectTransform row = SpawnRow(data.annotations[i].label);
            _activeRows.Add(row);

            LineRenderer lr = SpawnLine();
            _activeLines.Add(lr);

            yield return new WaitForSeconds(rowStaggerDelay);
        }
    }

    RectTransform SpawnRow(string label)
    {
        if (characteristicRowPrefab == null || characteristicsContainer == null)
            return null;

        GameObject row = Instantiate(characteristicRowPrefab, characteristicsContainer);

        TextMeshProUGUI text = row.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = bodyTextColor;
            text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(bulletColor)}>●</color>  {label}";
        }

        // Slide in from left
        RectTransform rt = row.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = new Vector3(0f, 1f, 1f);
            rt.DOScaleX(1f, 0.2f).SetEase(Ease.OutCubic);
        }

        return rt;
    }

    LineRenderer SpawnLine()
    {
        if (leaderLinePrefab == null) return null;

        GameObject lineObj = Instantiate(leaderLinePrefab, transform.parent);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.positionCount = 2;
            lr.startWidth = 0.0015f;
            lr.endWidth = 0.0005f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lineColor;
            lr.endColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0.15f);
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.zero);
        }

        return lr;
    }

    void ClearAll()
    {
        // Destroy row GameObjects
        foreach (RectTransform rt in _activeRows)
            if (rt != null) Destroy(rt.gameObject);
        _activeRows.Clear();

        // Destroy line GameObjects
        foreach (LineRenderer lr in _activeLines)
            if (lr != null) Destroy(lr.gameObject);
        _activeLines.Clear();

        // Clear any leftover children in characteristics container
        if (characteristicsContainer != null)
        {
            foreach (Transform child in characteristicsContainer)
                Destroy(child.gameObject);
        }
    }
}