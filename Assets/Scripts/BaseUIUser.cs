using UnityEngine;

public abstract class BaseUIUser : MonoBehaviour,IUIUser
{
    protected IUIUpdater view;
    public virtual void BindToView(IUIUpdater view)
    {
        this.view = view;
    }
}
