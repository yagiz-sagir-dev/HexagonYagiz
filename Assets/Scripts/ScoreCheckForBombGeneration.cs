using UnityEngine;

public class ScoreCheckForBombGeneration : MonoBehaviour
{
    [SerializeField]
    private int scoreToGenerateBomb;

    private ScoreManager scoreManager;
    private TileFactory tileFactory;
    private GameKiller gameKiller;
    private int nBombsGenerated;

    void Start()
    {
        scoreManager = ScoreManager.Instance;
        tileFactory = TileFactory.Instance;
        gameKiller = GameKiller.Instance;

        scoreManager.AddScoreDependentTrigger(CheckScore);
        gameKiller.AddGameOverTrigger(ResetCount);
    }

    private void CheckScore()
    {
        if (scoreManager.GetScore() / scoreToGenerateBomb > nBombsGenerated)
        {
            tileFactory.GenerateBomb();
            nBombsGenerated++;
        }
    }

    private void ResetCount()
    {
        nBombsGenerated = 0;
    }
}
