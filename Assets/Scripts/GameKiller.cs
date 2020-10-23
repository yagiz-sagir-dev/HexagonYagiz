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
    }

    public void KillGame()
    {
        inputManager.LockInput();
        gameOverPanel.gameObject.SetActive(true);
        finalScore.text = CountManager.Instance.Score.ToString();
    }

    public void RestartGame()
    {
        GridManager gridManager = GridManager.Instance;
        CountManager countManager = CountManager.Instance;
        gridManager.RestartGrid();
        countManager.ResetCounts();

        gameOverPanel.gameObject.SetActive(false);
    }
}
