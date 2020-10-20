using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer highlight;

    private LayerMask nodeLayerMask;

    private delegate void HandlerDelegate();
    private HandlerDelegate switchSelected;

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
        nodeLayerMask = LayerMask.GetMask("Node");
        highlight.enabled = false;
        switchSelected += () =>
        {
            highlight.enabled = !highlight.enabled;
        };
    }

    public void Attach(Transform parent)
    {
        transform.SetParent(parent);
        switchSelected.Invoke();
    }

    public void Detach()
    {
        Collider2D nodeCollider = Physics2D.OverlapCircle(transform.position, .05f, nodeLayerMask);
        if (nodeCollider)
        {
            transform.SetParent(nodeCollider.transform);
        }
        switchSelected.Invoke();
    }

    public void GetAssigned()
    {
        Collider2D nodeCollider = Physics2D.OverlapCircle(transform.position, .05f, nodeLayerMask);
        if (nodeCollider)
        {
            Node nodeScript = nodeCollider.GetComponent<Node>();
            nodeScript.AssignBlock(gameObject);
        }
    }

}
