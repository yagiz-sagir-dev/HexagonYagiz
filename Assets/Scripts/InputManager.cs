using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
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

    private void Start()
    {
        gridManager = GridManager.Instance;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !inputLocked)
        {
            if (EventSystem.current.IsPointerOverGameObject())  // If the point that user touched is on a UI element, input is disregarded
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
