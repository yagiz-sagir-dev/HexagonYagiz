using UnityEngine;

public class UIPanelUpdater : BaseUIUpdater
{
    [SerializeField]
    private Transform UIElement;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void UpdateUI(object param)
    {
        UIElement.gameObject.SetActive((bool)param);
    }
}
