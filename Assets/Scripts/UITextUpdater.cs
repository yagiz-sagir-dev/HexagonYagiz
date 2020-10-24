using TMPro;
using UnityEngine;

public class UITextUpdater : BaseUIUpdater
{
    [SerializeField]
    private TextMeshProUGUI UIElement;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void UpdateUI(object param)
    {
        UIElement.text = param.ToString();
    }
}
