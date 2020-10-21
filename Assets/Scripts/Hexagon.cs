using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [SerializeField]
    private LayerMask nodeLayerMask;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer highlight;
    [SerializeField]
    private float popSpeed = .05f;
    [SerializeField]
    private int scoreWhenPopped = 5;

    private bool popping;
    private bool migrating;
    private Transform targetNode;
    private Transform nearbyNode;

    private delegate void DelegateType();
    private DelegateType switchSelected;

    public Color TileColor
    {
        get { return TileColor; }
        set
        {
            value.a = 1;
            sprite.color = value;
        }
    }
    public int Id { get; set; }

    private void Awake()
    {
        highlight.enabled = false;
        switchSelected += () =>
        {
            highlight.enabled = !highlight.enabled;
        };
    }

    private void Update()
    {
        if (migrating)
        {
            transform.position = Vector3.Slerp(transform.position, targetNode.position, .05f);
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
                ScoreManager.AddScore(scoreWhenPopped);
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
        switchSelected.Invoke();
    }

    public void DetachFromHandle()
    {
        AttachToNode();
        switchSelected.Invoke();
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
}
