using UnityEngine;

public class UIPanelUpdater : BaseUIUpdater
{
    [SerializeField]
    private Transform UIElement;

    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateUI(object param)
    {
        UIElement.gameObject.SetActive((bool)param);
    }
}
