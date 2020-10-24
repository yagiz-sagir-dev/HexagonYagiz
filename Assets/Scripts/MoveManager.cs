using System;

public class MoveManager : BaseUIUser
{
    public static MoveManager Instance { get; private set; }

    private int moveCount;
    private Action moveCountIncreased;

    private void Awake()
    {
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
