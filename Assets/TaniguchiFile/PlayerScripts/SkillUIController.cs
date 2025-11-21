
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private Image rangeIndicator;
    [SerializeField] private Image cooldownFill;

    public void ShowSkillRange(float range)
    {
        rangeIndicator.gameObject.SetActive(true);
        rangeIndicator.rectTransform.sizeDelta = new Vector2(range * 10, range * 10);
    }

    public void HideSkillRange()
    {
        rangeIndicator.gameObject.SetActive(false);
    }

    public void UpdateCooldown(float remainingTime, float maxCooldown)
    {
        cooldownFill.fillAmount = remainingTime / maxCooldown;
    }
}
