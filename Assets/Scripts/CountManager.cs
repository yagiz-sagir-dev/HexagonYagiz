using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI moveCountText;
    [SerializeField]
    private int scoreToGenerateBomb = 1000;

    public static CountManager Instance { get; private set; }
    public int Score { get; private set; }

    private TileGenerator tileGenerator;

    private int scoreCountForBomb;
    private int moveCount;
    private List<Bomb> bombs;

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
    private void Start()
    {
        bombs = new List<Bomb>();
        tileGenerator = TileGenerator.Instance;
    }

    void Update()
    {
        scoreText.text = Score.ToString();
        moveCountText.text = moveCount.ToString();
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

    public void MadeMove()
    {
        Instance.moveCount++;

        foreach (Bomb bomb in bombs)
        {
            bomb.Countdown();
        }
    }

    public void RegisterNewBomb(Transform bomb)
    {
        bombs.Add(bomb.GetComponent<Bomb>());
    }

    public void DisposeOfBomb(Transform bomb)
    {
        bombs.Remove(bomb.GetComponent<Bomb>());
    }

    public void ResetCounts()
    {
        Score = 0;
        moveCount = 0;
    }
}
