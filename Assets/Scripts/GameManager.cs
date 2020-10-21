using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;

    public static GameManager Instance { get; private set; }

    private bool inputLocked = false;

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

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !inputLocked)
        {
            gridManager.OperateHandle();
        }
    }

    public void LockInput()
    {
        inputLocked = true;
        print("input locked");
    }

    public void UnlockInput()
    {
        inputLocked = false;
        print("input unlocked");
    }
}
