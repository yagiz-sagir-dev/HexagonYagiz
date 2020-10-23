using TMPro;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private int initialCountdown;
    [SerializeField]
    private TextMeshProUGUI countdownText;

    private GameKiller gameKiller;
    private CountManager countManager;

    private bool justArrived;
    private int countdown;

    private void Awake()
    {
        countdown = initialCountdown;
        justArrived = true;
    }

    private void Start()
    {
        gameKiller = GameKiller.Instance;
        countManager = CountManager.Instance;
        countManager.RegisterNewBomb(transform);
    }

    private void Update()
    {
        countdownText.text = countdown.ToString();
    }

    public void Countdown()
    {
        if (justArrived)
            justArrived = false;
        else
        {
            countdown--;
            if (countdown < 1)
            {
                gameKiller.KillGame();
            }
            justArrived = false;
        }
    }

    private void OnDestroy()
    {
        countManager.DisposeOfBomb(transform);
    }
}
