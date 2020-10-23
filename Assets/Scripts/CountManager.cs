using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI moveCountText;

    public static CountManager Instance { get; private set; }

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
    }

    void Update()
    {
        moveCountText.text = moveCount.ToString();
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
        moveCount = 0;
    }
}
