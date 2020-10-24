using UnityEngine;

public class GameOverPanelController : BaseUIUser
{
    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
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
