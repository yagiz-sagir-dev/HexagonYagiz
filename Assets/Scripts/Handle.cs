using UnityEngine;

public class Handle : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;

    private InputManager inputManager;
    private GridManager gridManager;

    private bool spinning = false;
    private bool clockwise = false;
    private float angle = 0f;
    private int spinBreakCount;

    private readonly float rotationAngle = 120f;
    private readonly int maxSpinBreakCount = 2;

    private delegate void DelegateType();
    private DelegateType attachAll;

    private void Start()
    {
        inputManager = InputManager.Instance;
        gridManager = GridManager.Instance;
    }

    private void FixedUpdate()
    {
        if (spinning)
        {
            if (clockwise)
                transform.Rotate(transform.forward, -rotationSpeed);
            else
                transform.Rotate(transform.forward, rotationSpeed);

            angle += rotationSpeed;
            if (angle >= rotationAngle)
            {
                SignalAttachedBlocks();
                angle = 0f;
                spinning = false;
                gridManager.IsPopTime();

                if (spinBreakCount-- > 0)
                    spinning = true;
                else inputManager.UnlockInput();
            }
        }
    }

    public void SpinCounterclokwise()
    {
        clockwise = false;
        Spin();
    }

    public void SpinClokwise()
    {
        clockwise = true;
        Spin();
    }

    private void Spin()
    {
        inputManager.LockInput();
        spinBreakCount = maxSpinBreakCount;
        spinning = true;
    }

    public void Lock(Collider2D[] colliders)
    {
        Vector2[] overlappingColliderPositions = new Vector2[colliders.Length];
        for (int index = 0; index < colliders.Length; index++)
        {
            Collider2D col = colliders[index];

            overlappingColliderPositions[index] = col.bounds.center;

            Tile tileScript = col.GetComponent<Tile>();
            attachAll += () => tileScript.AttachToHandle(transform);
        }
        transform.position = FindCenterOfMass(overlappingColliderPositions);
        attachAll?.Invoke();
        attachAll = null;
    }

    private Vector2 FindCenterOfMass(Vector2[] vectors)
    {
        Vector2 center = Vector2.zero;
        for (int index = 0; index < vectors.Length; index++)
        {
            center += vectors[index];
        }

        return center / vectors.Length;
    }

    public void Unlock()
    {
        Tile[] tileScripts = transform.GetComponentsInChildren<Tile>();
        foreach (Tile tileScript in tileScripts)
        {
            tileScript.DetachFromHandle();
        }
    }

    public void Relocate(Collider2D[] colliders)
    {
        Unlock();
        Lock(colliders);
    }

    private void SignalAttachedBlocks()
    {
        Tile[] tileScripts = transform.GetComponentsInChildren<Tile>();
        foreach (Tile tileScript in tileScripts)
        {
            tileScript.GetAssignedToNode();
        }
    }

    public void Decommission()
    {
        Unlock();
        Destroy(gameObject);
    }
}
