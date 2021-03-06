﻿using System;

 // MoveManager class keeps a count for the moves that user makes and warns observing components when a move is made

public class MoveManager : BaseUIUser
{
    public static MoveManager Instance { get; private set; }

    private int moveCount;
    private Action moveCountIncreased;

    private void Awake()
    {
        #region Singleton
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        #endregion
    }

    public void MadeMove()
    {
        moveCount++;
        view.UpdateUI(moveCount);
        moveCountIncreased?.Invoke();
    }

    public void ResetMoves()
    {
        moveCount = 0;
        view.UpdateUI(moveCount);
    }

    public override void BindToView(IUIUpdater view)
    {
        base.BindToView(view);
        view.UpdateUI(moveCount);
    }

    public void AddNewMoveDependentTrigger(Action callback)
    {
        moveCountIncreased += callback;
    }

    public void RemoveMoveDependentTrigger(Action callback)
    {
        moveCountIncreased -= callback;
    }
}
