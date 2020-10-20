using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int nColumns = 8;
    [SerializeField]
    private int nRows = 9;
    [SerializeField]
    private Transform nodePrefab;
    [SerializeField]
    private TileGenerator tileGenerator;

    public static GridManager Instance { get; private set; }

    private Transform[,] nodeGrid;

    private readonly float hexagonSidesRatio = .8660254f;

    private static readonly float overlapCircleRadius = .15f;
    private static LayerMask hexagonLayerMask;

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
        nodeGrid = new Transform[nRows, nColumns];
        hexagonLayerMask = LayerMask.GetMask("Block");
    }

    void Start()
    {
        GenerateGrid();
        DisplayGrid();
    }

    private void DisplayGrid()
    {
        transform.localScale = new Vector3(.6f, .6f, 1f);
        transform.position = new Vector2(-nColumns * hexagonSidesRatio / 2f + .5f, nRows / 2f - .25f) * .6f;
    }

    private void GenerateGrid()
    {
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                Transform node = Instantiate(nodePrefab, transform);
                tileGenerator.GenerateTile(node);

                float posX = col * hexagonSidesRatio;
                float posY = col % 2 == 1 ? row + .5f : row;

                node.position = new Vector2(posX, -posY);
                nodeGrid[row, col] = node;
            }
        }
        CheckPops();
    }

    public void CheckPops()
    {
        HashSet<Node> nodesToPop = new HashSet<Node>();
        List<Tuple<int, int>> neighborsForOddColumns = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,-1),
            new Tuple<int, int>(1,-1),
            new Tuple<int, int>(1,0),
            new Tuple<int, int>(1,1),
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(-1,0)
        };
        List<Tuple<int, int>> neighborsForEvenColumns = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,-1),
            new Tuple<int, int>(1,0),
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(-1,1),
            new Tuple<int, int>(-1,0),
            new Tuple<int, int>(-1,-1)
        };
        List<Tuple<int, int>> pairsToCheck = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(1,2),
            new Tuple<int, int>(2,3),
            new Tuple<int, int>(3,4),
            new Tuple<int, int>(4,5),
            new Tuple<int, int>(5,0)
        };
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                foreach (Tuple<int, int> pair in pairsToCheck)
                {
                    try
                    {
                        Tuple<int, int> neighbor1Coords;
                        Tuple<int, int> neighbor2Coords;
                        if (col % 2 > 0)
                        {
                            neighbor1Coords = neighborsForOddColumns[pair.Item1];
                            neighbor2Coords = neighborsForOddColumns[pair.Item2];
                        }
                        else
                        {
                            neighbor1Coords = neighborsForEvenColumns[pair.Item1];
                            neighbor2Coords = neighborsForEvenColumns[pair.Item2];
                        }

                        Node node = nodeGrid[row, col].gameObject.GetComponent<Node>();
                        Node neighbor1 = nodeGrid[row + neighbor1Coords.Item1, col + neighbor1Coords.Item2].gameObject.GetComponent<Node>();
                        Node neighbor2 = nodeGrid[row + neighbor2Coords.Item1, col + neighbor2Coords.Item2].gameObject.GetComponent<Node>();

                        if (node.BlockColorId == neighbor1.BlockColorId && neighbor1.BlockColorId == neighbor2.BlockColorId)
                        {
                            nodesToPop.Add(node);
                            nodesToPop.Add(neighbor1);
                            nodesToPop.Add(neighbor2);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
        foreach (Node nodeToPop in nodesToPop)
        {
            nodeToPop.PopBlock();
        }
    }

    public Collider2D[] CheckPosition(Vector2 pointerPos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, overlapCircleRadius, hexagonLayerMask);
        return colliders;
    }
}
