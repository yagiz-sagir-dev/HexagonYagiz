using UnityEngine;

public class TileFactory : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;
    [SerializeField]
    private GameObject hexagonPrefab;
    [SerializeField]
    private GameObject bombprefab;

    public static TileFactory Instance { get; private set; }

    private bool bombNext;  // Indicates that there is a bomb next in the production line.

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    public GameObject GenerateTile()
    {
        GameObject tile;

        if (bombNext)
        {
            tile = Instantiate(bombprefab);
            bombNext = false;
        }
        else
            tile = Instantiate(hexagonPrefab);

        RollTile(tile);
        return tile;
    }

    public void RollTile(GameObject tile)   // Randomizes properties of a tile. It could be new or sent by its node to reroll
    {
        int id = Random.Range(0, colorRange.Length);
        Tile tileScript = tile.GetComponent<Tile>();
        tileScript.SetColor(colorRange[id]);
        tileScript.SetId(id);
    }

    public void GenerateBomb()
    {
        Instance.bombNext = true;
    }
}
