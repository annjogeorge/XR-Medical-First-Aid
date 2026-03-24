using UnityEngine;
using System.Collections;

public class GhostHandController : MonoBehaviour
{
    [Header("Ghost Hands")]
    public GameObject ghostPoint;
    public GameObject ghostTurnTap;
    public GameObject ghostWrap;

    [Header("Animators")]
    public Animator pointAnimator;
    public Animator tapAnimator;
    public Animator wrapAnimator;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;
    public float ghostAlpha = 0.35f;

    private GameObject _currentGhost;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        HideAll();
    }

    void HideAll()
    {
        if (ghostPoint != null) ghostPoint.SetActive(false);
        if (ghostTurnTap != null) ghostTurnTap.SetActive(false);
        if (ghostWrap != null) ghostWrap.SetActive(false);
    }

    public void PlayPointAtBurn()
    {
        Show(ghostPoint, pointAnimator);
    }

    public void PlayForStepType(StepType stepType)
    {
        Debug.Log($"Ghost: PlayForStepType called with {stepType}");

        switch (stepType)
        {
            case StepType.CoolWater:
                Show(ghostTurnTap, tapAnimator);
                break;
            case StepType.ApplyBandage:
            case StepType.ApplyClingFilm:
                Show(ghostWrap, wrapAnimator);
                break;
            case StepType.ApplyMoisturiser:
                Show(ghostPoint, pointAnimator);
                break;
            default:
                Hide(); // ← hides for any unrecognised step type
                break;
        }
    }

    public void Hide()
    {
        if (_currentGhost == null) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOut(_currentGhost));
    }

    void Show(GameObject ghost, Animator anim)
    {
        if (ghost == null) return;

        if (_currentGhost != null && _currentGhost != ghost)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOut(_currentGhost));
        }

        _currentGhost = ghost;
        ghost.SetActive(true);

        if (anim != null) anim.SetTrigger("PlayAction");

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeIn(ghost));
    }

    IEnumerator FadeIn(GameObject ghost)
    {
        var renderers = ghost.GetComponentsInChildren<SkinnedMeshRenderer>();
        float elapsed = 0f;

        foreach (var r in renderers)
        {
            Color c = r.material.color;
            c.a = 0f;
            r.material.color = c;
        }

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, ghostAlpha, elapsed / fadeInDuration);
            foreach (var r in renderers)
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }
            yield return null;
        }
    }

    IEnumerator FadeOut(GameObject ghost)
    {
        var renderers = ghost.GetComponentsInChildren<SkinnedMeshRenderer>();
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(ghostAlpha, 0f, elapsed / fadeOutDuration);
            foreach (var r in renderers)
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }
            yield return null;
        }

        ghost.SetActive(false);
        if (_currentGhost == ghost) _currentGhost = null;
    }
}