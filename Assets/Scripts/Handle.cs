using UnityEngine;
using ExtensionMethods;
using System.Collections;

public class Handle : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;

    private GameManager gameManager;
    private GridManager gridManager;
    private bool spinning = false;
    private float angle = 0f;
    private int spinBreakCount;

    private readonly float rotationAngle = 120f;
    private readonly int maxSpinBreakCount = 2;

    private delegate void DelegateType();
    private DelegateType attachAll;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;
    }

    private void FixedUpdate()
    {
        if (spinning)
        {
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
                else gameManager.UnlockInput();
            }
        }
    }

    public void Spin()
    {
        gameManager.LockInput();
        spinBreakCount = maxSpinBreakCount;
        spinning = true;
    }

    public void Lock(Collider2D[] colliders)
    {
        Vector2[] overlappingColliderPositions = new Vector2[colliders.Length];
        for (int index = 0; index < colliders.Length; index++)
        {
            Collider2D col = colliders[index];

            overlappingColliderPositions[index] = (Vector2)col.bounds.center;

            Hexagon tileScript = col.gameObject.GetComponent<Hexagon>();
            attachAll += () => tileScript.AttachToHandle(transform);
        }
        transform.position = overlappingColliderPositions.FindCenterOfMass();
        attachAll.Invoke();
        attachAll = null;
    }

    public void Unlock()
    {
        Hexagon[] tileScripts = transform.GetComponentsInChildren<Hexagon>();
        foreach (Hexagon tileScript in tileScripts)
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
        Hexagon[] tileScripts = transform.GetComponentsInChildren<Hexagon>();
        foreach (Hexagon tileScript in tileScripts)
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
