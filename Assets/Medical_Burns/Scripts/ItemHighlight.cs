using DG.Tweening;
using System.Collections;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ItemHighlight : MonoBehaviour
{
    [Header("Icon")]
    public GameObject iconObject;
    public float bobHeight = 0.04f;
    public float bobSpeed = 1.5f;
    public float pulseMinScale = 0.9f;
    public float pulseMaxScale = 1.15f;
    public float pulseDuration = 0.6f;

    [Header("Arrow")]
    public float arrowHeight = 0.25f;
    public float arrowBobSpeed = 2.0f;
    public float arrowBobAmount = 0.02f;
    public Color arrowColor = new Color(1f, 0.85f, 0f, 1f); // bright yellow

    private Vector3 _iconStartPos;
    private Vector3 _iconStartScale;
    private bool _isActive = false;

    // Runtime arrow
    private GameObject _arrowObj;
    private LineRenderer _line;
    private Material _lineMat;

    void Start()
    {
        if (iconObject != null)
        {
            _iconStartPos = iconObject.transform.localPosition;
            _iconStartScale = iconObject.transform.localScale;
            iconObject.SetActive(false);
        }

        BuildArrow();
        SetArrowVisible(false);

        var grabInteractable = GetComponentInChildren<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(_ => HideIcon());
        }
    }

    void BuildArrow()
    {
        _arrowObj = new GameObject("HighlightArrow");
        // Scene root — no parent so nothing can move it
        _arrowObj.transform.position = transform.position + Vector3.up * arrowHeight;

        _line = _arrowObj.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.05f;
        _line.endWidth = 0.008f;
        _line.useWorldSpace = true;

        _lineMat = new Material(Shader.Find("Unlit/Color"));
        _lineMat.color = arrowColor;
        _line.material = _lineMat;
    }

    void SetArrowVisible(bool visible)
    {
        if (_arrowObj != null) _arrowObj.SetActive(visible);
    }

    void Update()
    {
        if (!_isActive || iconObject == null) return;

        // Bob icon
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        iconObject.transform.localPosition = _iconStartPos + Vector3.up * bob;

        // Bob arrow above the item in world space
        if (_arrowObj != null && _arrowObj.activeSelf)
        {
            float arrowBob = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmount;
            Vector3 tip = transform.position + Vector3.up * 0.05f;
            Vector3 tail = transform.position + Vector3.up * (arrowHeight + arrowBob);
            _line.SetPosition(0, tail);
            _line.SetPosition(1, tip);

            // Pulse arrow color
            float t = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
            _lineMat.color = Color.Lerp(arrowColor * 0.5f, arrowColor, t);
        }
    }

    public void ShowIcon()
    {
        if (iconObject == null) return;
        _isActive = true;
        iconObject.SetActive(true);
        SetArrowVisible(true);

        DOTween.Kill(iconObject.transform);
        iconObject.transform.localScale = Vector3.zero;

        iconObject.transform.DOScale(_iconStartScale, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                iconObject.transform.DOScale(_iconStartScale * pulseMaxScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void HideIcon()
    {
        if (iconObject == null) return;
        _isActive = false;
        SetArrowVisible(false);
        DOTween.Kill(iconObject.transform);
        iconObject.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => iconObject.SetActive(false));
    }

    void OnDestroy()
    {
        if (_arrowObj != null) Destroy(_arrowObj);
        if (_lineMat != null) Destroy(_lineMat);
    }
}