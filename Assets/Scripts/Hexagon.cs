using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Color TileColor
    {
        get { return TileColor; }
        set
        {
            value.a = 1;
            transform.Find("Sprite").GetComponent<SpriteRenderer>().color = value;
        }
    }
    public int Id { get; set; }
}
