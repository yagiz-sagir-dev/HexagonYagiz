using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    private Color[] colorRange;
    [SerializeField]
    private GameObject hexagonPrefab;

    public GameObject GenerateTile(Transform assignedNode)
    {
        GameObject tile = Instantiate(hexagonPrefab, assignedNode);
        int id = Random.Range(0, colorRange.Length);
        Hexagon tileScript = tile.GetComponent<Hexagon>();
        tileScript.TileColor = colorRange[id];
        tileScript.Id = id;
        tileScript.GetAssigned();

        return tile;
    }
}
