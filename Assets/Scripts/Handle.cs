using UnityEngine;

public class Handle : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;

    private HandleController controller;

    private bool spinning = false;
    private bool clockwise = false;
    private float angle = 0f;
    private int spinBreakCount = 0;

    private readonly float rotationAngle = 120f;
    private readonly int maxSpinBreakCount = 2;     // Handle makes 3 spins with 2 breaks and at the end of each spin it lets the GridManager
                                                    // know that it is the time to check the grid
    private delegate void DelegateType();
    private DelegateType attachAll;

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
                SignalAttachedTiles();      // When a spin ends, the handle sends a signal to the tiles that are attached to the nodes on which
                angle = 0f;                 // the handle resides   
                spinning = false;
                controller.SpinRoundCompleted();

                if (spinBreakCount-- > 0)
                    spinning = true;
                else
                {                                               // User input is disabled during
                    controller.HandleStoppedSpinning(); ;                 // spins. When the handle is done, it is enabled again.
                }
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
        controller.HandleStartedSpinning();
        spinBreakCount = maxSpinBreakCount;
        spinning = true;
    }
    

    public void Lock(Collider2D[] colliders)        // Colliders that GridManager gets as a result of a user input are sent to the handle.
    {
        Vector2[] overlappingColliderPositions = new Vector2[colliders.Length];
        for (int index = 0; index < colliders.Length; index++)
        {
            Collider2D col = colliders[index];

            overlappingColliderPositions[index] = col.bounds.center;

            Tile tileScript = col.GetComponent<Tile>();
            attachAll += () => tileScript.AttachToHandle(transform);    // Tiles must attach to the handle after it takes position otherwise
        }                                                               // attached tile change their positions along with it
        transform.position = FindCenterOfMass(overlappingColliderPositions);    // Handle finds the center point of those colliders and 
        attachAll?.Invoke();                                                    // goes there, then attaches to corresponding tiles
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
    /*
     * Handle sends a signal to attached tiles between spins so the assign to the nodes that they stand on, telling those nodes what
     * color they are. So when GridManager checks the grid between handle spins, it can find matching tiles correctly.
     */
    private void SignalAttachedTiles()
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

    public void SetController(HandleController controller)
    {
        this.controller = controller;
    }
}
