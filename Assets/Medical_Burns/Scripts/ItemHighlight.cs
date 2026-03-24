using UnityEngine;
using DG.Tweening;

public class ItemHighlight : MonoBehaviour
{
    [Header("Icon")]
    public GameObject iconObject;      // World Space Canvas with icon image
    public float bobHeight = 0.04f;
    public float bobSpeed = 1.5f;
    public float pulseMinScale = 0.9f;
    public float pulseMaxScale = 1.1f;
    public float pulseDuration = 0.6f;

    private Vector3 _iconStartPos;
    private bool _isActive = false;

    void Start()
    {
        if (iconObject != null)
        {
            _iconStartPos = iconObject.transform.localPosition;
            iconObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!_isActive || iconObject == null) return;

        // Bob up and down
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        iconObject.transform.localPosition = _iconStartPos + Vector3.up * bob;
    }

    public void ShowIcon()
    {
        if (iconObject == null) return;
        _isActive = true;
        iconObject.SetActive(true);
        iconObject.transform.localScale = Vector3.zero;

        // Pop in
        iconObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // Pulse loop
        DOTween.Kill(iconObject.transform);
        iconObject.transform.DOScale(pulseMaxScale * Vector3.one, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void HideIcon()
    {
        if (iconObject == null) return;
        _isActive = false;
        DOTween.Kill(iconObject.transform);
        iconObject.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => iconObject.SetActive(false));
    }
}