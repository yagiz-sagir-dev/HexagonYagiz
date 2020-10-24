using UnityEngine;

public class ScoreCheckForBombGeneration : MonoBehaviour
{
    [SerializeField]
    private int scoreToGenerateBomb;

    private ScoreManager scoreManager;
    private TileFactory tileFactory;
    private int nBombsGenerated;

    void Start()
    {
        scoreManager = ScoreManager.Instance;
        tileFactory = TileFactory.Instance;

        scoreManager.AddScoreDependentTrigger(CheckScore);
    }

    private void CheckScore()
    {
        if (scoreManager.GetScore() / scoreToGenerateBomb > nBombsGenerated)
        {
            tileFactory.GenerateBomb();
            nBombsGenerated++;
        }
    }
}
