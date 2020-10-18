using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;

    private static GameObject hexagon;
    public static TileGenerator Instance;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        hexagon = (GameObject)Resources.Load("Hexagon");
    }

    public static GameObject GenerateTile(Transform owner)
    {
        GameObject tile = Instantiate(hexagon, owner);
        tile.name = "Hexagon";

        int id = (int)Random.Range(0, Instance.colorRange.Length);
        Hexagon tileScript = tile.GetComponent<Hexagon>();
        tileScript.TileColor = Instance.colorRange[id];
        tileScript.Id = id;

        return tile;
    }
}
