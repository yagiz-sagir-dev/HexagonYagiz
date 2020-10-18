using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int nColumns = 8;
    [SerializeField]
    private int nRows = 9;
    [SerializeField]
    private Transform nodePrefab;

    Transform[,] nodeGrid;

    private const float hexagonSideMultiplier = .866f;

    void Start()
    {
        nodeGrid = new Transform[nRows, nColumns];
        GenerateGrid();
        transform.localScale = new Vector3(.6f, .6f, 1f);
        transform.position = new Vector2(-nColumns * hexagonSideMultiplier / 2f + .5f, nRows / 2f -.25f) * .6f;
    }

    private void GenerateGrid()
    {
        for (int col = 0; col < nColumns; col++)
        {
            for (int row = 0; row < nRows; row++)
            {
                Transform node = Instantiate(nodePrefab, transform);
                node.name = "Node";
                TileGenerator.GenerateTile(node);

                float posX = col * hexagonSideMultiplier;
                float posY = col % 2 == 1 ? row + .5f : row;

                node.position = new Vector2(posX, -posY);
                nodeGrid[row, col] = node;
            }
        }
    }
}
