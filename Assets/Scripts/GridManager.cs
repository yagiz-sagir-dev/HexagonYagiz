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

    private readonly List<Tuple<int, int>> neighborsForOddColumns = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,-1),
            new Tuple<int, int>(1,-1),
            new Tuple<int, int>(1,0),
            new Tuple<int, int>(1,1),
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(-1,0)
        };
    private readonly List<Tuple<int, int>> neighborsForEvenColumns = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,-1),
            new Tuple<int, int>(1,0),
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(-1,1),
            new Tuple<int, int>(-1,0),
            new Tuple<int, int>(-1,-1)
        };
    private readonly List<Tuple<int, int>> pairsToCheck = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(1,2),
            new Tuple<int, int>(2,3),
            new Tuple<int, int>(3,4),
            new Tuple<int, int>(4,5),
            new Tuple<int, int>(5,0)
        };

    public static GridManager Instance { get; private set; }

    private CountManager countManager;
    private TileGenerator tileGenerator;
    private InputManager inputManager;
    private GameKiller gameKiller;

    private bool moveMade;
    private Handle handleScript;
    private Transform handle;
    private Transform[,] nodeGrid;

    private readonly int nOverlapsToLayHandle = 3;
    private readonly float hexagonSidesRatio = .8660254f;

    private static readonly float overlapCircleRadius = .5f;
    private LayerMask hexagonLayerMask;
    private LayerMask handleLayerMask;

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
        handleLayerMask = LayerMask.GetMask("Handle");
    }

    void Start()
    {
        countManager = CountManager.Instance;
        tileGenerator = TileGenerator.Instance;
        inputManager = InputManager.Instance;
        gameKiller = GameKiller.Instance;
        GenerateGrid();
        DisplayGrid();
    }

    private void Update()
    {
        if (moveMade)
        {
            if (!CheckForEmptyNodes() && !IsPopTime())
            {
                countManager.MadeMove();
                moveMade = false;
                if (!CheckMovesLeft())
                {
                    gameKiller.KillGame();
                    return;
                }
                inputManager.UnlockInput();
            }
        }
    }

    private void DisplayGrid()
    {
        Transform camNode1 = Instantiate(nodePrefab, transform);
        Transform camNode2 = Instantiate(nodePrefab, transform);

        camNode1.position = new Vector2(-1, 1);
        camNode2.position = new Vector2(nColumns, -nRows);

        Camera cam = Camera.main;

        Transform firstNode = nodeGrid[0, 0];
        Transform hLastNode = nodeGrid[0, nColumns - 1];
        Transform vLastNode = nodeGrid[nRows - 1, 0];

        float extent = firstNode.GetComponent<CircleCollider2D>().bounds.extents.x;

        float hMin = firstNode.transform.position.x - extent;
        float hMax = hLastNode.transform.position.x + extent;

        float vMin = vLastNode.transform.position.y - extent;
        float vMax = firstNode.transform.position.y + extent;

        Vector2 center = new Vector2((hMax + hMin) / 2, (vMax + vMin) / 2);

        cam.transform.position = new Vector3(center.x, center.y + extent * 4, -10f);

        Vector3 point1 = cam.WorldToViewportPoint(camNode1.position);
        Vector3 point2 = cam.WorldToViewportPoint(camNode2.position);

        while (point1.x <= 0f || point2.y <= 0f)
        {
            Camera.main.orthographicSize += .5f;
            point1 = cam.WorldToViewportPoint(camNode1.position);
            point2 = cam.WorldToViewportPoint(camNode2.position);
        }
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

        if (!CheckMovesLeft())
        {
            RestartGrid();
            return;
        }
    }

    private void ResetGrid()
    {
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                Destroy(nodeGrid[row, col].gameObject);
                nodeGrid[row, col] = null;
            }
        }
    }

    public bool IsPopTime()
    {
        HashSet<Node> nodesToPop = CheckForPops();
        if (nodesToPop.Count > 0)
        {
            if (!moveMade)
            {
                moveMade = true;
            }
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

    private bool CheckMovesLeft()
    {
        HashSet<int> consecutiveColors = new HashSet<int>();
        HashSet<int> consecutiveIndexes = new HashSet<int>();
        List<int> colorCodesInOrder = new List<int>();

        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                if (col % 2 > 0)
                {
                    foreach (Tuple<int, int> neighborCoords in neighborsForOddColumns)
                    {
                        Node neighborNode;
                        try { neighborNode = nodeGrid[row + neighborCoords.Item1, col + neighborCoords.Item2].GetComponent<Node>(); }
                        catch (Exception) { continue; }
                        colorCodesInOrder.Add(neighborNode.BlockColorId);
                    }
                }
                else
                {
                    foreach (Tuple<int, int> neighborCoords in neighborsForEvenColumns)
                    {
                        Node neighborNode;
                        try { neighborNode = nodeGrid[row + neighborCoords.Item1, col + neighborCoords.Item2].GetComponent<Node>(); }
                        catch (Exception) { continue; }
                        colorCodesInOrder.Add(neighborNode.BlockColorId);
                    }
                }

                for (int index = 0; index < colorCodesInOrder.Count; index++)
                {
                    if (colorCodesInOrder[index] == colorCodesInOrder[(index + 1) % colorCodesInOrder.Count])
                    {
                        if (consecutiveColors.Contains(colorCodesInOrder[index]))
                        {
                            return true;
                        }
                        consecutiveColors.Add(colorCodesInOrder[index]);
                        consecutiveIndexes.Add(index);
                        consecutiveIndexes.Add((index + 1) % colorCodesInOrder.Count);
                    }
                }
                for (int index = 0; index < colorCodesInOrder.Count; index++)
                {
                    if (consecutiveColors.Contains(colorCodesInOrder[index]) && !consecutiveIndexes.Contains(index))
                    {
                        return true;
                    }
                }
                consecutiveColors.Clear();
                consecutiveIndexes.Clear();
                colorCodesInOrder.Clear();
            }
        }
        return false;
    }

    private HashSet<Node> CheckForPops()
    {
        HashSet<Node> nodesToPop = new HashSet<Node>();
        Node node, neighbor1, neighbor2;

        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                node = nodeGrid[row, col].GetComponent<Node>();

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

                        neighbor1 = nodeGrid[row + neighbor1Coords.Item1, col + neighbor1Coords.Item2].GetComponent<Node>();
                        neighbor2 = nodeGrid[row + neighbor2Coords.Item1, col + neighbor2Coords.Item2].GetComponent<Node>();

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

        if (colliders.Length < nOverlapsToLayHandle)
            return null;

        else if (colliders.Length > nOverlapsToLayHandle)
        {
            Collider2D[] closestColliders = new Collider2D[nOverlapsToLayHandle];

            for (int a = 0; a < nOverlapsToLayHandle; a++)
            {
                int closestColliderIndex = a;
                for (int b = a + 1; b < colliders.Length; b++)
                {
                    if (Vector3.Distance(colliders[closestColliderIndex].bounds.center, pointerPos) > Vector3.Distance(colliders[b].bounds.center, pointerPos))
                        closestColliderIndex = b;
                }
                Collider2D temp = colliders[a];
                colliders[a] = colliders[closestColliderIndex];
                colliders[closestColliderIndex] = temp;

                closestColliders[a] = colliders[a];
            }
            return closestColliders;
        }

        return colliders;
    }

    public void OperateHandle()
    {
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = CheckPosition(pointerPos);
        if (colliders != null)
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
                handleScript.Relocate(colliders);
            }
        }
    }

    public void RestartGrid()
    {
        ResetGrid();
        GenerateGrid();
        inputManager.UnlockInput();
    }
}
