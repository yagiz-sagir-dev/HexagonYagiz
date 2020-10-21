using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;
    [SerializeField]
    private GameObject hexagonPrefab;

    public static TileGenerator Instance { get; private set; }

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
        GameObject tile = Instantiate(hexagonPrefab);
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
}
