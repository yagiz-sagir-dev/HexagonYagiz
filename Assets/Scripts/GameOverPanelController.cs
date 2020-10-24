using UnityEngine;

// This controller updates final score text on the game over panel when game is over
public class GameOverPanelController : BaseUIUser
{
    private ScoreManager scoreManager;
    private GameKiller gameKiller;

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
        gameKiller = GameKiller.Instance;
        gameKiller.AddGameOverTrigger(SetFinalScore);
    }

    public void SetFinalScore()
    {
        view.UpdateUI(scoreManager.GetScore());
    }

    public override void BindToView(IUIUpdater view)
    {
        base.BindToView(view);
    }
}
