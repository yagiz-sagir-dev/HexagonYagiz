using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;
    [SerializeField]
    private GameObject hexagonPrefab;
    [SerializeField]
    private GameObject bombprefab;

    public static TileGenerator Instance { get; private set; }

    private bool bombNext;

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

        int id = Random.Range(0, colorRange.Length);
        Hexagon tileScript = tile.GetComponent<Hexagon>();
        tileScript.TileColor = colorRange[id];
        tileScript.Id = id;

        return tile;
    }

    public void RerollTile(GameObject tile)
    {
        int id = Random.Range(0, colorRange.Length);
        Hexagon tileScript = tile.GetComponent<Hexagon>();
        tileScript.TileColor = colorRange[id];
        tileScript.Id = id;
    }

    public void GenerateBomb()
    {
        Instance.bombNext = true;
    }
}
