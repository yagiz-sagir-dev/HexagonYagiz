using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private int scoreToGenerateBomb = 1000;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public int Score { get; private set; }
    public static ScoreManager Instance { get; private set; }

    private TileGenerator tileGenerator;
    private int scoreCountForBomb;

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

    void Start()
    {
        tileGenerator = TileGenerator.Instance;
    }

    void Update()
    {
        scoreText.text = Score.ToString();
    }

    public void AddScore(int scoreToAdd)
    {
        Score += scoreToAdd;
        scoreCountForBomb += scoreToAdd;
        if (scoreCountForBomb >= scoreToGenerateBomb)
        {
            scoreCountForBomb -= scoreToGenerateBomb;
            tileGenerator.GenerateBomb();
        }
    }

    public void ResetScore()
    {
        Score = 0;
        scoreCountForBomb = 0;
    }
}
