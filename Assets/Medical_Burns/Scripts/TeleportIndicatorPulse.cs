using UnityEngine;

public class TeleportIndicatorPulse : MonoBehaviour
{
    public Color glowColor = new Color(0.4f, 0.7f, 1.0f);
    public float minIntensity = 0.5f;
    public float maxIntensity = 2.5f;
    public float pulseSpeed = 1.5f;

    private Material _mat;

    void Start()
    {
        // Creates unique material instance for this object
        _mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (_mat == null) return;

        float intensity = Mathf.Lerp(minIntensity, maxIntensity,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        _mat.SetColor("_EmissionColor", glowColor * intensity);
    }
}