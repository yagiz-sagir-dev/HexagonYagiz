using TMPro;
using UnityEngine;

public class GameKiller : BaseUIUser
{
    [SerializeField]
    private GameOverPanelController gameOverPanelController;

    private InputManager inputManager;
    private MoveManager moveManager;
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
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
        moveManager = MoveManager.Instance;
        scoreManager = ScoreManager.Instance;
        gridManager = GridManager.Instance;
        view.UpdateUI(false);
    }

    public void KillGame()
    {
        gameOverPanelController.SetFinalScore();
        view.UpdateUI(true);
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
}
