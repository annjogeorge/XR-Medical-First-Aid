using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class BurnCardUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Card UI")]
    public Image cardBackground;
    public Image topAccentBar;
    public TextMeshProUGUI degreeLabel;
    public TextMeshProUGUI degreeName;
    public TextMeshProUGUI tagline;
    public Image selectedIndicator;

    [Header("Card Colors")]
    public Color normalBG = new Color(0.97f, 0.97f, 0.97f, 0.95f);
    public Color selectedBG = new Color(0.92f, 0.96f, 1.00f, 0.98f);
    public Color hoverBG = new Color(0.93f, 0.93f, 0.93f, 0.98f);

    private BurnProfile _profile;
    private MainMenuManager _menu;
    private bool _isSelected = false;

    public void Setup(BurnProfile profile, MainMenuManager menu)
    {
        _profile = profile;
        _menu = menu;

        if (degreeLabel != null)
            degreeLabel.text = profile.burnDegree.ToUpper();

        if (degreeName != null)
        {
            string num = profile.burnDegree.Contains("First") ? "1st" :
                         profile.burnDegree.Contains("Second") ? "2nd" : "3rd";
            degreeName.text = num;
        }

        if (tagline != null)
        {
            tagline.text = profile.burnDegree.Contains("First")
                ? "Minor — outer skin only"
                : profile.burnDegree.Contains("Second")
                    ? "Moderate — blistering"
                    : "Severe — requires 999";
        }

        //Color accent = profile.burnDegree.Contains("First")
        //    ? new Color(0.00f, 0.65f, 0.00f, 1f)
        //    : profile.burnDegree.Contains("Second")
        //        ? new Color(0.80f, 0.20f, 0.10f, 1f)
        //        : new Color(0.20f, 0.20f, 0.20f, 1f);

        //if (topAccentBar != null) topAccentBar.color = accent;
        if (cardBackground != null) cardBackground.color = normalBG;
        if (selectedIndicator != null)
            selectedIndicator.gameObject.SetActive(false);

        // Wire up Button component as backup
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClicked);
        }
    }

    // ── IPointerClickHandler ─────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked();
    }

    public void OnClicked()
    {
        if (_profile == null || _menu == null)
        {
            Debug.LogWarning("CARD: _profile or _menu is null on " + gameObject.name);
            return;
        }
        Debug.Log("CARD: Clicked " + _profile.burnDegree);
        _menu.OnBurnSelected(_profile, this);
    }

    // ── IPointerEnterHandler ─────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return;
        if (cardBackground != null)
            cardBackground.DOColor(hoverBG, 0.15f);
        transform.DOScale(Vector3.one * 1.03f, 0.15f);
        Debug.Log("CARD: Hover enter " + gameObject.name);
    }

    // ── IPointerExitHandler ──────────────────────────────────────────────────
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;
        if (cardBackground != null)
            cardBackground.DOColor(normalBG, 0.15f);
        transform.DOScale(Vector3.one, 0.15f);
        Debug.Log("CARD: Hover exit " + gameObject.name);
    }

    // ── Selection ────────────────────────────────────────────────────────────
    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (cardBackground != null)
            cardBackground.DOColor(selected ? selectedBG : normalBG, 0.2f);

        if (selectedIndicator != null)
            selectedIndicator.gameObject.SetActive(selected);

        if (selected)
            transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 3, 0.5f);

        Debug.Log("CARD: SetSelected " + selected + " on " + gameObject.name);
    }
}