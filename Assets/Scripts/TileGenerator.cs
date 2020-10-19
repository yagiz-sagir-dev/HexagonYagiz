using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;
    [SerializeField]
    private GameObject hexagonPrefab;

    public static TileGenerator SingletonInstance { get; private set; }

    private void Awake()
    {
        if (!SingletonInstance)
        {
            SingletonInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    public static GameObject GenerateTile(Transform owner)
    {
        GameObject tile = Instantiate(SingletonInstance.hexagonPrefab, owner);
        int id = Random.Range(0, SingletonInstance.colorRange.Length);
        Hexagon tileScript = tile.GetComponent<Hexagon>();
        tileScript.TileColor = SingletonInstance.colorRange[id];
        tileScript.Id = id;

        return tile;
    }
}
