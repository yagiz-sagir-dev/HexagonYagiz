using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer highlight;
    [SerializeField]
    private LayerMask nodeLayerMask;

    private bool selected;

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
        highlight.enabled = false;
        switchSelected += () =>
        {
            selected = !selected;
            highlight.enabled = !highlight.enabled;
        };
    }

    public void UnlockHexagon()
    {
        switchSelected.Invoke();
    }

    public void LockHexagon()
    {
        switchSelected.Invoke();
        Collider2D nodeCollider = Physics2D.OverlapCircle(transform.position, .05f, nodeLayerMask);
        if (nodeCollider)
        {
            transform.SetParent(nodeCollider.transform);
        }
    }


}
