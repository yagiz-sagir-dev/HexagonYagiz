using System;
using UnityEngine;

public class GameKiller : BaseUIUser
{
    private InputManager inputManager;
    private MoveManager moveManager;
    private ScoreManager scoreManager;
    private GridManager gridManager;

    private Action gameIsOver;
    
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
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
        moveManager = MoveManager.Instance;
        scoreManager = ScoreManager.Instance;
        gridManager = GridManager.Instance;
        view.UpdateUI(false);       // View of this class controls game over panel which starts disabled when the game begins
    }

    public void KillGame()
    {
        gameIsOver?.Invoke();
        view.UpdateUI(true);        // When the game is over, game over panel is activated by the ui controller
        inputManager.LockInput();
    }

    public void RestartGame()
    {
        gridManager.RestartGrid();
        scoreManager.ResetScore();
        moveManager.ResetMoves();

        view.UpdateUI(false);
    }

    public override void BindToView(IUIUpdater view)
    {
        base.BindToView(view);
        view.UpdateUI(false);
    }

    public void AddGameOverTrigger(Action callback)
    {
        gameIsOver += callback;
    }

    public void RemoveGameOverTrigger(Action callback)
    {
        gameIsOver -= callback;
    }
}
