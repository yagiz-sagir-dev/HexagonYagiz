﻿using UnityEngine;

public abstract class BaseUIUpdater : MonoBehaviour, IUIUpdater
{
    [SerializeField]
    protected BaseUIUser uiUser;

    protected virtual void Awake()
    {
        uiUser.BindToView(this);
    }

    public abstract void UpdateUI(object param);
}
