using UnityEngine;

public class HandleController : MonoBehaviour
{
    [SerializeField]
    private GameObject handlePrefab;
    [SerializeField]
    private LayerMask tileLayerMask;

    enum Directions
    {
        Right,
        Left,
        Up,
        Down,
        None
    }

    public static HandleController Instance { get; private set; }

    private Transform handle;
    private Handle handleScript;
    private GridManager gridManager;
    private InputManager inputManager;

    private readonly float overlapCircleRadius = .38f;
    private readonly int nOverlapsToLayHandle = 3;

    private void Awake()
    {
        #region Singleton
        if (!Instance)                              // Singleton instances are used for each manager class to make sure
        {                                           // of their persistence and uniqueness throughout the lifetime of the game
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        #endregion
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
        inputManager = InputManager.Instance;
    }

    public void OperateHandle()     // This method is called by the input manager to handle the user input.
    {
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = CheckPosition(pointerPos);
        if (colliders != null)                  // CheckPosition method returns the required number of colliders if there are enough around
        {                                       // the input position
            if (!handle)                        // If there is not a handle in the game, a new one is created and placed at where the user input
            {                                   // shows
                GameObject newHandle = Instantiate(handlePrefab, transform);
                handle = newHandle.transform;
                handleScript = newHandle.GetComponent<Handle>();
                handleScript.SetController(this);
                handleScript.Lock(colliders);
            }
            else
            {
                handleScript.Relocate(colliders);       // If there is already a handle in the game, it is relocated to the new
            }                                           // position that the user input determined           
        }
    }

    public void ProcessSwipe(Vector2 pointerDownPos, Vector2 pointerUpPos)  // Processes user input to spin handle
    {
        if (handle)
        {
            Directions swipeDirection = Directions.None;
            Vector2 centerPoint = (pointerUpPos + pointerDownPos) / 2;
            float swipeAngle = Vector3.Angle(centerPoint - (Vector2)handle.position, pointerUpPos - pointerDownPos);
            if (!IsValueInRange(swipeAngle, 30f, 150f))
                return;
            float swipeDirectionAngle = Vector3.SignedAngle(pointerUpPos - pointerDownPos, new Vector3(1f, 0f, 0f), Vector3.back);

            if (IsValueInRange(swipeDirectionAngle, 0f, 45f) || IsValueInRange(swipeDirectionAngle, -45f, 0f))
            {
                swipeDirection = Directions.Right;                      // A line is drawn between input up and down positions.
            }                                                           // Another line is drawn between handle position and center
            else if (IsValueInRange(swipeDirectionAngle, 45f, 135f))    // position of the first line.
            {                                                           // Angle between them is checked. If the angle is too tight,
                swipeDirection = Directions.Up;                         // handle is not spinned. This was done to simulate torque
            }                                                           // on the handle.
            else if (IsValueInRange(swipeDirectionAngle, 135f, 180f) || IsValueInRange(swipeDirectionAngle, -180f, -135f))
            {
                swipeDirection = Directions.Left;                       // Spin direction is decided depending on the swipe direction
            }                                                           // and its position to the handle.
            else if (IsValueInRange(swipeDirectionAngle, -135f, -45f))
            {
                swipeDirection = Directions.Down;
            }

            switch (swipeDirection)
            {
                case Directions.Left:
                    if (centerPoint.y < handle.position.y)
                        handleScript.SpinClokwise();
                    else if (centerPoint.y > handle.position.y)
                        handleScript.SpinCounterclokwise();
                    break;
                case Directions.Right:
                    if (centerPoint.y < handle.position.y)
                        handleScript.SpinCounterclokwise();
                    else if (centerPoint.y > handle.position.y)
                        handleScript.SpinClokwise();
                    break;
                case Directions.Up:
                    if (centerPoint.x < handle.position.x)
                        handleScript.SpinClokwise();
                    else if (centerPoint.x > handle.position.x)
                        handleScript.SpinCounterclokwise();
                    break;
                case Directions.Down:
                    if (centerPoint.x < handle.position.x)
                        handleScript.SpinCounterclokwise();
                    else if (centerPoint.x > handle.position.x)
                        handleScript.SpinClokwise();
                    break;
            }
        }
    }

    /*
     * CheckPosition method runs when user input taken. It checks the pointer position to see if user touched a valid spot
     * to place the handle.
     */

    private Collider2D[] CheckPosition(Vector2 pointerPos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, overlapCircleRadius, tileLayerMask);    // An overlap circle is drawn to check
                                                                                                                // nearby tile colliders
        if (colliders.Length < nOverlapsToLayHandle)        // If the number of colliders that the circle overlaps aren't enough to 
            return null;                                    // place a handle, method simply returns nothing

        else if (colliders.Length > nOverlapsToLayHandle)   // If it is more than required, closest colliders of required number 
        {                                                   // are taken and returned
            Collider2D[] closestColliders = new Collider2D[nOverlapsToLayHandle];

            for (int a = 0; a < nOverlapsToLayHandle; a++)
            {
                int closestColliderIndex = a;
                for (int b = a + 1; b < colliders.Length; b++)
                {
                    if (Vector3.Distance(colliders[closestColliderIndex].bounds.center, pointerPos) > Vector3.Distance(colliders[b].bounds.center, pointerPos))
                        closestColliderIndex = b;
                }
                Collider2D temp = colliders[a];
                colliders[a] = colliders[closestColliderIndex];
                colliders[closestColliderIndex] = temp;

                closestColliders[a] = colliders[a];
            }
            return closestColliders;
        }
        return colliders;
    }

    public void SpinRoundCompleted()            // After the handle completes one round of its spinning, matching tiles are checked.
    {                                           // If there are any, handle is destroyed 
        if (gridManager.IsPopTime())
            handleScript.Decommission();
    }

    private bool IsValueInRange(float value, float min, float max)
    {
        return value <= max && value > min;
    }

    public void HandleStartedSpinning()
    {
        inputManager.LockInput();
    }
    public void HandleStoppedSpinning()
    {
        inputManager.UnlockInput();
    }
}
