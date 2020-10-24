using TMPro;
using UnityEngine;

public class UITextUpdater : BaseUIUpdater
{
    [SerializeField]
    private TextMeshProUGUI UIElement;

    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateUI(object param)
    {
        UIElement.text = param.ToString();
    }
}
