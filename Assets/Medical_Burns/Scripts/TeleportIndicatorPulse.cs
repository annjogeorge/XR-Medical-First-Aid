using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TeleportIndicatorPulse : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = new Color(0.4f, 0.7f, 1.0f);
    public float minIntensity = 0.5f;
    public float maxIntensity = 2.5f;
    public float pulseSpeed = 1.5f;

    [Header("Line/Ray Settings")]
    public float arrowHeight = 1.5f;
    public float arrowBobSpeed = 2.0f;
    public float arrowBobAmount = 0.15f;
    public Color lineColor = new Color(0.4f, 0.7f, 1.0f, 0.8f);

    [Header("Label Settings")]
    [TextArea(3, 5)] // Allows for multi-line editing in Inspector
    public string labelText = "Point the ray and\npress the Grip to teleport";
    public float labelHeight = 2.2f;
    public float labelFontSize = 0.05f;   // Lowered this for smaller text

    private Material _mat;
    private LineRenderer _line;
    private TextMesh _textMesh;
    private GameObject _labelObj;
    private Camera _mainCam;

    void Start()
    {
        _mat = GetComponent<Renderer>().material;
        _mat.EnableKeyword("_EMISSION");

        // --- Line arrow ---
        _line = gameObject.GetComponent<LineRenderer>();
        if (_line == null) _line = gameObject.AddComponent<LineRenderer>();

        _line.positionCount = 2;
        _line.startWidth = 0.02f; // Slimmed down the line
        _line.endWidth = 0.002f;
        _line.useWorldSpace = true;

        // Using a standard shader that supports transparency
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.material.color = lineColor;

        // --- World-space text label ---
        _labelObj = new GameObject("TeleportLabel");

        _textMesh = _labelObj.AddComponent<TextMesh>();
        _textMesh.text = labelText;
        _textMesh.fontSize = 120; // High font size + low character size = sharp text
        _textMesh.characterSize = labelFontSize;
        _textMesh.anchor = TextAnchor.LowerCenter; // Anchored at bottom so it sits above ray
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.color = glowColor;

        _mainCam = Camera.main;
    }

    void Update()
    {
        if (_mainCam != null && _labelObj != null)
        {
            float distToPlayer = Vector3.Distance(
                new Vector3(_mainCam.transform.position.x, 0, _mainCam.transform.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)
            );
            _labelObj.SetActive(distToPlayer > 0.6f); // hide when within 0.6m of disk center
        }

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        float brightness = Mathf.Lerp(0.5f, 1.0f, t);

        // Glow pulse on the floor pad
        if (_mat != null)
            _mat.SetColor("_EmissionColor", glowColor * intensity);

        // Ray/Line bobbing
        if (_line != null)
        {
            float bob = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmount;
            _line.SetPosition(0, transform.position + Vector3.up * (arrowHeight + bob));
            _line.SetPosition(1, transform.position + Vector3.up * 0.02f);

            // Pulse the ray brightness
            Color pulseLine = lineColor;
            pulseLine.a *= brightness;
            _line.startColor = pulseLine;
            _line.endColor = pulseLine;
        }

        // Label: Billboard + Position
        if (_labelObj != null && _mainCam != null)
        {
            float bob = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmount;
            // Position it slightly above the ray tip
            _labelObj.transform.position = transform.position + Vector3.up * (labelHeight + bob);

            // Face the camera (Billboard)
            _labelObj.transform.LookAt(_mainCam.transform);
            _labelObj.transform.Rotate(0, 180f, 0);

            // Pulse text color
            _textMesh.color = glowColor * brightness;
        }
    }

    void OnDestroy()
    {
        if (_labelObj != null) Destroy(_labelObj);

    }

    void OnEnable()
    {
        if (_labelObj != null) _labelObj.SetActive(true);
        if (_line != null) _line.enabled = true;
    }

    void OnDisable()
    {
        if (_labelObj != null) _labelObj.SetActive(false);
        if (_line != null) _line.enabled = false;
    }
}