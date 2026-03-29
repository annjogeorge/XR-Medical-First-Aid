using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UrgentButtonPulse : MonoBehaviour
{
    public Color colorA = new Color(0.85f, 0.1f, 0.1f, 1f);  // deep red
    public Color colorB = new Color(1f, 0.3f, 0.2f, 1f);      // bright red
    public float pulseDuration = 0.6f;

    private Image _img;

    void Start()
    {
        _img = GetComponent<Image>();

        // Scale pulse
        transform.DOScale(1.04f, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Color pulse
        _img.DOColor(colorB, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable()
    {
        DOTween.Kill(transform);
        if (_img != null) DOTween.Kill(_img);
    }
}