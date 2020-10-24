using UnityEngine;

public class Bomb : BaseUIUser
{
    [SerializeField]
    private int initialCountdown;

    private GameKiller gameKiller;
    private MoveManager moveManager;

    private bool justArrived;
    private int countdown;

    private void Awake()
    {
        countdown = initialCountdown;
        justArrived = true;
    }

    private void Start()
    {
        gameKiller = GameKiller.Instance;
        moveManager = MoveManager.Instance;
        moveManager.AddNewMoveDependentTrigger(Countdown);
    }

    public void Countdown()
    {
        if (justArrived)
            justArrived = false;
        else
        {
            countdown--;
            view.UpdateUI(countdown);
            if (countdown < 1)
            {
                gameKiller.KillGame();
            }
            justArrived = false;
        }
    }

    private void OnDestroy()
    {
        moveManager.RemoveMoveDependentTrigger(Countdown);
    }

    public override void BindToView(IUIUpdater view)
    {
        base.BindToView(view);
        view.UpdateUI(countdown);
    }
}
