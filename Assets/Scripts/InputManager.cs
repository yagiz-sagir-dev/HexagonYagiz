using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GridManager gridManager;

    public static InputManager Instance { get; private set; }

    private bool inputLocked = false;

    private Vector2 pointerDownPos;
    private Vector2 pointerUpPos;
    private Vector2 currentPointerPos;

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

#if UNITY_EDITOR

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !inputLocked)
        {
            pointerDownPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && !inputLocked)
        {
            currentPointerPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Vector3.Distance(pointerDownPos, currentPointerPos) > .25f)
            {
                Vector2 pointerDownWorldPos = Camera.main.ViewportToWorldPoint(pointerDownPos);
                Vector2 currentPointerWorldPos = Camera.main.ViewportToWorldPoint(currentPointerPos);
                gridManager.ProcessSwipe(pointerDownWorldPos, currentPointerWorldPos);
            }
        }
        else if (Input.GetMouseButtonUp(0) && !inputLocked)
        {
            pointerUpPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Vector3.Distance(pointerUpPos, pointerDownPos) < .01f)
            {
                gridManager.OperateHandle();
            }
        }
    }

#elif UNITY_ANDROID

    void Update()
    {
        if (Input.touchCount > 0 && !inputLocked)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                pointerDownPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
            }
            else if (Input.touches[0].phase == TouchPhase.Moved)
            {
                currentPointerPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                if (Vector3.Distance(pointerDownPos, currentPointerPos) > .25f)
                {
                    Vector2 pointerDownWorldPos = Camera.main.ViewportToWorldPoint(pointerDownPos);
                    Vector2 currentPointerWorldPos = Camera.main.ViewportToWorldPoint(currentPointerPos);
                    gridManager.ProcessSwipe(pointerDownWorldPos, currentPointerWorldPos);
                }
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                pointerUpPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                if (Vector3.Distance(pointerUpPos, pointerDownPos) < .01f)
                {
                    gridManager.OperateHandle();
                }
            }
        }
    }

#endif

    public void LockInput()
    {
        inputLocked = true;
    }

    public void UnlockInput()
    {
        inputLocked = false;
    }
}
