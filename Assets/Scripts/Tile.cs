using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private LayerMask nodeLayerMask;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer highlight;
    [SerializeField]
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
            if (Vector3.Distance(transform.position, targetNode.position) < .01f)
            {
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
            transform.localScale -= new Vector3(popSpeed, popSpeed, 0f);
            if (transform.localScale.x < .1f)
            {
                scoreManager.AddScore(scoreWhenPopped);
                Destroy(gameObject);
            }
        }
    }

    private void FindNearbyNode()
    {
        nearbyNode = null;
        Collider2D nodeCollider = Physics2D.OverlapCircle(transform.position, .05f, nodeLayerMask);
        if (nodeCollider)
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
            nodeScript.AssignBlock(gameObject);
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

    public int GetId()
    {
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
