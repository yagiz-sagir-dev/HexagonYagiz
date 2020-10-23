using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;

    public static InputManager Instance { get; private set; }

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
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            gridManager.OperateHandle();
        }
    }

    public void LockInput()
    {
        inputLocked = true;
    }

    public void UnlockInput()
    {
        inputLocked = false;
    }
}
