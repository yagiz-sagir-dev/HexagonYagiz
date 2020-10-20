using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager gridManager;
    [SerializeField]
    private GameObject handlePrefab;

    public static GameManager Instance { get; private set; }

    private bool inputLocked = false;

    private readonly int nOverlapsToLayHandle = 3;

    private Handle handleScript;
    private Transform handle;

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
            Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = gridManager.CheckPosition(pointerPos);
            if (colliders.Length >= nOverlapsToLayHandle)
            {
                if (!handle)
                {
                    GameObject newHandle = Instantiate(handlePrefab, transform);
                    handle = newHandle.transform;
                    handleScript = newHandle.GetComponent<Handle>();
                    handleScript.Lock(colliders);
                }
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(pointerPos, Vector2.zero);
                    if (hit)
                    {
                        if (hit.transform.name == handle.name)
                            handleScript.Spin();
                    }
                    else
                    {
                        handleScript.Relocate(colliders);
                    }
                }
            }
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
