using TMPro;
using UnityEngine;

public class GameKiller : MonoBehaviour
{
    [SerializeField]
    private Transform gameOverPanel;
    [SerializeField]
    private TextMeshProUGUI finalScore;

    private InputManager inputManager;
    private CountManager countManager;
    private ScoreManager scoreManager;
    private GridManager gridManager;

    public static GameKiller Instance { get; private set; }

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

        gameOverPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
        countManager = CountManager.Instance;
        scoreManager = ScoreManager.Instance;
        gridManager = GridManager.Instance;
    }

    public void KillGame()
    {
        inputManager.LockInput();
        gameOverPanel.gameObject.SetActive(true);
        finalScore.text = scoreManager.Score.ToString();
    }

    public void RestartGame()
    {
        gridManager.RestartGrid();
        countManager.ResetCounts();
        scoreManager.ResetScore();

        gameOverPanel.gameObject.SetActive(false);
    }
}
