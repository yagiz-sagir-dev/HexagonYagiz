using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI moveCountText;

    public static ScoreManager Instance { get; private set; }

    private static int score;
    private static int moveCount;

    private void Start()
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

    void Update()
    {
        scoreText.text = score.ToString();
        moveCountText.text = moveCount.ToString();
    }

    public static void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }

    public static void MadeMove()
    {
        moveCount++;
    }
}
