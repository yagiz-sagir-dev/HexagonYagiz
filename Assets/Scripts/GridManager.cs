using System;
using System.Collections.Generic;
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
    private GameObject handlePrefab;
    [SerializeField]
    private GameManager gameManager;

    public static GridManager Instance { get; private set; }

    private Transform[,] nodeGrid;
    private TileGenerator tileGenerator;
    private bool moveMade;
    private Handle handleScript;
    private Transform handle;

    private readonly int nOverlapsToLayHandle = 3;
    private readonly float hexagonSidesRatio = .8660254f;

    private static readonly float overlapCircleRadius = .3f;
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
        tileGenerator = TileGenerator.Instance;
        GenerateGrid();
        DisplayGrid();
    }

    private void Update()
    {
        if (moveMade)
        {
            if (!CheckForEmptyNodes())
            {
                if (!IsPopTime())
                {
                    gameManager.UnlockInput();
                    moveMade = false;
                }
            }
        }
    }

    private void DisplayGrid()
    {
        //transform.localScale = new Vector3(.6f, .6f, 1f);
        transform.position = new Vector2(-nColumns * hexagonSidesRatio / 2f + .5f, nRows / 2f - .25f);
    }

    private void GenerateGrid()
    {
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                Transform node = Instantiate(nodePrefab, transform);
                Hexagon tileScript = tileGenerator.GenerateTile().GetComponent<Hexagon>();
                tileScript.PlaceAtNode(node);

                float posX = col * hexagonSidesRatio;
                float posY = col % 2 == 1 ? row + .5f : row;

                node.position = new Vector2(posX, -posY);
                node.GetComponent<Node>().GridCoords = new Tuple<int, int>(row, col);
                nodeGrid[row, col] = node;
            }
        }

        HashSet<Node> nodesToPop = CheckForPops();
        while (nodesToPop.Count > 0)
        {
            foreach (Node nodeToPop in nodesToPop)
            {
                nodeToPop.ReplaceBlock();
            }
            nodesToPop = CheckForPops();
        }
    }

    public bool IsPopTime()
    {
        HashSet<Node> nodesToPop = CheckForPops();
        if (nodesToPop.Count > 0)
        {
            moveMade = true;
            if (handle)
                handleScript.Decommission();
            HashSet<int> popColumns = new HashSet<int>();
            Queue<Node> emptyNodes = new Queue<Node>();
            List<Node> emptyNodesInWait = new List<Node>();

            foreach (Node nodeToPop in nodesToPop)
            {
                popColumns.Add(nodeToPop.GridCoords.Item2);
                nodeToPop.PopBlock();
            }

            foreach (int popColumn in popColumns)
            {
                for (int row = nRows - 1; row >= 0; row--)
                {
                    Node nodeScript = nodeGrid[row, popColumn].GetComponent<Node>();
                    if (!nodeScript.HasBlock())
                        emptyNodes.Enqueue(nodeScript);
                    else
                    {
                        if (emptyNodes.Count > 0)
                        {
                            Node emptyNode = emptyNodes.Dequeue();
                            nodeScript.RelocateBlock(emptyNode.transform);
                            emptyNodes.Enqueue(nodeScript);
                        }
                    }
                }
                emptyNodesInWait.AddRange(emptyNodes);
                emptyNodes.Clear();
            }

            foreach (Node emptyNode in emptyNodesInWait)
            {
                GameObject tile = tileGenerator.GenerateTile();
                Hexagon tileScript = tile.GetComponent<Hexagon>();

                tile.transform.position = new Vector2(emptyNode.transform.position.x, 10f);
                tileScript.Migrate(emptyNode.transform);
            }
            return true;
        }
        return false;
    }

    private bool CheckForEmptyNodes()
    {
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                if (!nodeGrid[row, col].GetComponent<Node>().HasBlock())
                    return true;
            }
        }
        return false;
    }

    private HashSet<Node> CheckForPops()
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

                        Node node = nodeGrid[row, col].GetComponent<Node>();
                        Node neighbor1 = nodeGrid[row + neighbor1Coords.Item1, col + neighbor1Coords.Item2].GetComponent<Node>();
                        Node neighbor2 = nodeGrid[row + neighbor2Coords.Item1, col + neighbor2Coords.Item2].GetComponent<Node>();

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
        return nodesToPop;
    }

    private Collider2D[] CheckPosition(Vector2 pointerPos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, overlapCircleRadius, hexagonLayerMask);
        return colliders;
    }

    public void OperateHandle()
    {
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = CheckPosition(pointerPos);
        if (colliders.Length >= nOverlapsToLayHandle)
        {
            if (!handle)
            {
                GameObject newHandle = Instantiate(handlePrefab, transform);
                handle = newHandle.transform;
                handleScript = newHandle.GetComponent<Handle>();
                handleScript.Lock(colliders);
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(pointerPos, Vector2.zero);
                if (hit)
                {
                    if (hit.transform.name == handle.name)
                        handleScript.Spin();
                }
                else
                {
                    handleScript.Relocate(colliders);
                }
            }
        }
    }
}
