using System;
using UnityEngine;

public class ScoreManager : BaseUIUser
{
    public static ScoreManager Instance { get; private set; }
    private Action scoreChanged;

    private int score;

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

    public void AddScore(int scoreToAdd)
    {
        score += scoreToAdd;
        view.UpdateUI(score);
        scoreChanged?.Invoke();
    }

    public void ResetScore()
    {
        score = 0;
        view.UpdateUI(score);
    }

    public override void BindToView(IUIUpdater view)
    {
        base.BindToView(view);
        view.UpdateUI(score);
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScoreDependentTrigger(Action callback)
    {
        scoreChanged += callback;
    }

    public void RemoveScoreDependentTrigger(Action callback)
    {
        scoreChanged -= callback;
    }
}
