using UnityEngine;

public class Bomb : BaseUIUser
{
    [SerializeField]
    private int initialCountdown;

    private GameKiller gameKiller;
    private MoveManager moveManager;

    private bool justArrived;   // Countdown of the bomb should not be triggered in the same round that the bomb is generated. justArrived
    private int countdown;      // is used for that purpose

    private void Awake()
    {
        countdown = initialCountdown;
        justArrived = true;
    }

    private void Start()
    {
        gameKiller = GameKiller.Instance;
        moveManager = MoveManager.Instance;
        moveManager.AddNewMoveDependentTrigger(Countdown);      // Everytime user makes a move, moveManager runs its MadeMove() method which invokes
    }                                                           // countdown subroutines in every bomb in the game.

    public void Countdown()
    {
        if (justArrived)
            justArrived = false;
        else
        {
            countdown--;
            view.UpdateUI(countdown);       // Everytime countdown value changes, UI element that shows this value on the bomb is updated
            if (countdown < 1)
            {
                gameKiller.KillGame();      // if the countdown hits zero, bomb goes off and game is over
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
