using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private LayerMask nodeLayerMask;    // Tiles use layermasks of nodes to detect nearby nodes to attach 
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer highlight;   // White highlight indicates that the tile has been picked by a handle and is enabled when
    [SerializeField]                    // tile attaches to this handle
    private float popSpeed;
    [SerializeField]
    private int scoreWhenPopped;

    private bool popping;
    private bool migrating;
    private Transform targetNode;
    private Transform nearbyNode;
    private int colorId;

    private ScoreManager scoreManager;

    Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        highlight.enabled = false;
    }

    private void Start()
    {
        scoreManager = ScoreManager.Instance;
    }

    private void Update()
    {
        if (migrating)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetNode.position, ref velocity, .2f);
            if (Vector3.Distance(transform.position, targetNode.position) < .01f)   // Tile smoothly moves towards the target node while migrating
            {                                                                       // and when they are close enough, attaches to it
                transform.position = targetNode.position;
                migrating = false;
                AttachToNode();
            }
        }
    }

    private void FixedUpdate()
    {
        if (popping)
        {
            transform.localScale -= new Vector3(popSpeed, popSpeed, 0f);    // While popping, tile slowly shrikns until it destroys itself
            if (transform.localScale.x < .1f)
            {
                scoreManager.AddScore(scoreWhenPopped);     // And add its score yield to user score
                Destroy(gameObject);
            }
        }
    }

    private void FindNearbyNode()
    {
        nearbyNode = null;
        Collider2D nodeCollider = Physics2D.OverlapCircle(transform.position, .05f, nodeLayerMask);
        if (nodeCollider)   // Overlapcircle detects nearby nodes when tile needs to attach or assign
        {
            nearbyNode = nodeCollider.transform;
        }
    }

    public void AttachToHandle(Transform handle)
    {
        transform.SetParent(handle);
        highlight.enabled = true;
    }

    public void DetachFromHandle()
    {
        AttachToNode();
        highlight.enabled = false;
    }

    private void AttachToNode()
    {
        if (GetAssignedToNode())
            transform.SetParent(nearbyNode);
    }

    public bool GetAssignedToNode()
    {
        FindNearbyNode();
        if (nearbyNode)
        {
            Node nodeScript = nearbyNode.GetComponent<Node>();
            nodeScript.AssignTile(gameObject);
            return true;
        }
        return false;
    }

    public void StartPopping()
    {
        popping = true;
    }

    public void Migrate(Transform newNode)
    {
        migrating = true;
        targetNode = newNode;
    }

    public void PlaceAtNode(Transform node)
    {
        transform.position = node.position;
        AttachToNode();
    }

    public int GetId()  // Id values exist to keep finding matching colors job overall easier and simpler than checking actual
    {                   // color values against each other.
        return colorId;
    }

    public void SetId(int colorId)
    {
        this.colorId = colorId;
    }

    public void SetColor(Color color)
    {
        color.a = 1f;
        sprite.color = color;
    }
}
