using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverTester : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public Color hoverColor = Color.green;
    private Color originalColor;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    // Triggered when the Raycast first hits the object
    public void OnHoverEntered()
    {
        Debug.Log("<color=yellow>Hover Entered!</color> Raycast hit the tap.");
        if (meshRenderer != null) meshRenderer.material.color = hoverColor;
    }

    // Triggered when the Raycast moves off the object
    public void OnHoverExited()
    {
        Debug.Log("<color=white>Hover Exited!</color>");
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }
}