using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // GridManager class creates the game grid of nodes. Checks the grid
    // for any tile matches at specific points. Finds if there is any moves
    // left for the player to make. If not, it calls game ending method from
    // GameKiller class

    enum Directions
    {
        Right,
        Left,
        Up,
        Down,
        None
    }

    [SerializeField]
    private LayerMask tileLayerMask;
    [SerializeField]
    private int nColumns = 8;
    [SerializeField]
    private int nRows = 9;
    [SerializeField]
    private Transform nodePrefab;
    [SerializeField]
    private GameObject handlePrefab;

    /* 
     * Code was written specifically for this layout. In this layout, nodes at columns of odd numbers are placed
     * lower than those of even numbers. So the order of neighbor nodes to be checked against each other are different
     * for these two groups. 
    */
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

    public static GridManager Instance { get; private set; }

    private MoveManager moveManager;
    private TileFactory tileFactory;
    private InputManager inputManager;
    private GameKiller gameKiller;

    private bool moveMade;
    private Handle handleScript;
    private Transform handle;
    private Transform[,] nodeGrid;

    private readonly int nOverlapsToLayHandle = 3;
    /*
     * Since horizontal and vertical dimensions of a hexagon are not equal, rows can be placed closer to each other than
     * columns are or vice versa. This ratio helps placing tiles at equal intervals horizontally and vertically
     */
    private readonly float hexagonSidesRatio = .8660254f;

    private static readonly float overlapCircleRadius = .5f;

    private void Awake()
    {
        if (!Instance)                              // Singleton instances are used for each manager class to make sure
        {                                           // of their persistence and uniqueness throughout the lifetime of the game
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        nodeGrid = new Transform[nRows, nColumns];
    }

    void Start()
    {
        moveManager = MoveManager.Instance;
        tileFactory = TileFactory.Instance;
        inputManager = InputManager.Instance;
        gameKiller = GameKiller.Instance;
        GenerateGrid();
        DisplayGrid();
    }

    private void Update()
    {
        if (moveMade)                                       // boolean moveMade becomes true when GridManager finds matching
        {                                                   // tiles. This makes sure that the class checks for empty tiles only
            if (!CheckForEmptyNodes() && !IsPopTime())      // when there are breaking tiles in the game, keeping it from making
            {                                               // redundant checks
                moveManager.MadeMove();                     // when the class can no longer find any matching tiles in the game,
                moveMade = false;                           // it lets the MoveManager class know that the player has made a move
                if (!CheckMovesLeft())                      // Before GridManager decides it is all right to return the game to normal
                {                                           // and unlock user input, it checks if there are any moves that the play can
                    gameKiller.KillGame();                  // make. If there is not any, game ending method is called
                    return;
                }
                inputManager.UnlockInput();
            }
        }
    }

    private void DisplayGrid()
    {
        Transform camNode1 = Instantiate(nodePrefab, transform);            // Two nodes are placed at the highest and lowest corners
        Transform camNode2 = Instantiate(nodePrefab, transform);            // of the game grid.

        camNode1.position = new Vector2(-1, 1);                             // They are placed a bit above the highest and below the 
        camNode2.position = new Vector2(nColumns, -nRows);                  // lowest nodes in the grid.

        Camera cam = Camera.main;

        Transform firstNode = nodeGrid[0, 0];                               // Furthest nodes in the grid are located to find the exact
        Transform hLastNode = nodeGrid[0, nColumns - 1];                    // center point of the grid
        Transform vLastNode = nodeGrid[nRows - 1, 0];

        float extent = firstNode.GetComponent<CircleCollider2D>().bounds.extents.x;

        float hMin = firstNode.transform.position.x - extent;
        float hMax = hLastNode.transform.position.x + extent;

        float vMin = vLastNode.transform.position.y - extent;
        float vMax = firstNode.transform.position.y + extent;

        Vector2 center = new Vector2((hMax + hMin) / 2, (vMax + vMin) / 2);

        cam.transform.position = new Vector3(center.x, center.y + extent * 4, -10f);    // Main camera is placed at the center,

        Vector3 point1 = cam.WorldToViewportPoint(camNode1.position);
        Vector3 point2 = cam.WorldToViewportPoint(camNode2.position);

        while (point1.x <= 0f || point2.y <= 0f)                    // then the orthographic size of the camera is increased until
        {                                                           // the cam nodes that were created in the beginning of the method
            Camera.main.orthographicSize += .5f;                    // are in the viewport of the camera to make sure that the whole
            point1 = cam.WorldToViewportPoint(camNode1.position);   // grid is visible. This placement is done only at the beginning
            point2 = cam.WorldToViewportPoint(camNode2.position);   // of the game.
        }
    }

    private void GenerateGrid()
    {
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                Transform node = Instantiate(nodePrefab, transform);                    // Node are generated and placed at corresponding
                Tile tileScript = tileFactory.GenerateTile().GetComponent<Tile>();      // positions. Then tiles are generated and attached
                tileScript.PlaceAtNode(node);                                           // to each node

                float posX = col * hexagonSidesRatio;           // tiles are positioned lower at odd columns
                float posY = col % 2 == 1 ? row + .5f : row;

                node.position = new Vector2(posX, -posY);
                node.GetComponent<Node>().GridCoords = new Tuple<int, int>(row, col);
                nodeGrid[row, col] = node;
            }
        }

        HashSet<Node> nodesToPop = CheckForPops();      // After creating the grid and placing tiles, GridManager checks for
                                                        // any matches and rerolls those matching tiles until there aren't any
        while (nodesToPop.Count > 0)                    // left since getting points without even touching the screen is not
        {                                               // really appropriate or fun for such a game
            foreach (Node nodeToPop in nodesToPop)
            {
                nodeToPop.ReplaceTile();
            }
            nodesToPop = CheckForPops();
        }

        if (!CheckMovesLeft())                      // If there aren't any matching tiles left, class checks for available moves
        {                                           // as the last thing before game begins. If there aren't any moves to make,
            RestartGrid();                          // grid is reset and created again
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
        if (nodesToPop.Count > 0)                               // When there are matching tiles in the game after making a move,
        {                                                       // player is held back until all matching tiles are broken, new
            if (!moveMade)                                      // ones are placed and settle down. This is done repeatedly until
            {                                                   // there aren't any matches left
                moveMade = true;
            }

            HashSet<int> popColumns = new HashSet<int>();
            Queue<Node> emptyNodes = new Queue<Node>();
            List<Node> emptyNodesInWait = new List<Node>();

            foreach (Node nodeToPop in nodesToPop)
            {
                popColumns.Add(nodeToPop.GridCoords.Item2);     // All matching blocks are popped and the method keeps track of their
                nodeToPop.PopTile();                           // column indexes
            }

            foreach (int popColumn in popColumns)               // All columns with missing tiles are checked from the bottom to the top
            {                                                   // to shift existing tiles down
                for (int row = nRows - 1; row >= 0; row--)
                {
                    Node nodeScript = nodeGrid[row, popColumn].GetComponent<Node>();
                    if (!nodeScript.HasTile())
                        emptyNodes.Enqueue(nodeScript);
                    else
                    {
                        if (emptyNodes.Count > 0)
                        {
                            Node emptyNode = emptyNodes.Dequeue();
                            nodeScript.RelocateTile(emptyNode.transform);
                            emptyNodes.Enqueue(nodeScript);
                        }
                    }
                }
                emptyNodesInWait.AddRange(emptyNodes);          // After shifting existing tiles, any remaining empty nodes are kept in
                emptyNodes.Clear();                             // emptyNodesInWait list to populate them again with new tiles
            }

            foreach (Node emptyNode in emptyNodesInWait)        // For each empty node, a new tile is created right above the position of
            {                                                   // this node
                GameObject tile = tileFactory.GenerateTile();
                Tile tileScript = tile.GetComponent<Tile>();

                tile.transform.position = new Vector2(emptyNode.transform.position.x, 10f);
                tileScript.Migrate(emptyNode.transform);                                        // and starts moving towards the empty node
            }
            return true;
        }
        return false;
    }

    private bool CheckForEmptyNodes()                   // GridManager keeps input locked until all nodes have their tiles attached.
    {                                                   // CheckForEmptyNodes() method is used to detect if there are any empty nodes
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                if (!nodeGrid[row, col].GetComponent<Node>().HasTile())
                    return true;
            }
        }
        return false;
    }

    public void HandleSpinned()             // After the handle completes one round of its spinning, matching tiles are checked.
    {                                       // If there are any, handle is destroyed 
        if (IsPopTime())
            handleScript.Decommission();
    }

    /*
     * The method CheckMovesLeft() iterates through groups of tiles to search for a certain pattern to find any user moves left
     * that can be made. It checks neighbors of a tile 2 by 2. If any 2 consecutive neighbors of a tile are of same color, this color and
     * indexes of these neighbors are recorded and method keeps searching. If there is a neighbor tile with a color which is already recorded
     * as the color of two consecutive neighbors and this tile is not one of those consecutive neighbors that were recorded before, 
     * this shows that there exists a move that the player can make.
     */

    private bool CheckMovesLeft()
    {
        HashSet<int> consecutiveColors = new HashSet<int>();        // Colors that consecutive neighbors have
        HashSet<int> consecutiveIndexes = new HashSet<int>();       // Indexes of these consecutive neighbors of same color
        List<int> neighborColorCodesInOrder = new List<int>();      // List of all colors of every neighbor of a node

        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                if (col % 2 > 0)
                {
                    foreach (Tuple<int, int> neighborCoords in neighborsForOddColumns)      // First all neighbor colors are stored in a 
                    {                                                                       // list
                        Node neighborNode;
                        try { neighborNode = nodeGrid[row + neighborCoords.Item1, col + neighborCoords.Item2].GetComponent<Node>(); }
                        catch (Exception) { continue; }
                        neighborColorCodesInOrder.Add(neighborNode.TileColorId);
                    }
                }
                else
                {
                    foreach (Tuple<int, int> neighborCoords in neighborsForEvenColumns)
                    {
                        Node neighborNode;
                        try { neighborNode = nodeGrid[row + neighborCoords.Item1, col + neighborCoords.Item2].GetComponent<Node>(); }
                        catch (Exception) { continue; }
                        neighborColorCodesInOrder.Add(neighborNode.TileColorId);
                    }
                }

                for (int index = 0; index < neighborColorCodesInOrder.Count; index++)
                {
                    int nextIndex = (index + 1) % neighborColorCodesInOrder.Count;                  // Then consecutive neighbors of the same 
                    if (neighborColorCodesInOrder[index] == neighborColorCodesInOrder[nextIndex])   // color are found and recorded
                    {
                        if (consecutiveColors.Contains(neighborColorCodesInOrder[index]))
                        {
                            return true;
                        }
                        consecutiveColors.Add(neighborColorCodesInOrder[index]);
                        consecutiveIndexes.Add(index);
                        consecutiveIndexes.Add(nextIndex);
                    }
                }
                for (int index = 0; index < neighborColorCodesInOrder.Count; index++)   // Finally, list of all colors is searched through
                {                                                                       // to see if there are any matches
                    if (consecutiveColors.Contains(neighborColorCodesInOrder[index]) && !consecutiveIndexes.Contains(index))
                    {
                        return true;
                    }
                }
                consecutiveColors.Clear();              // Lists are cleared in the end for each node to get ready to check the next node
                consecutiveIndexes.Clear();
                neighborColorCodesInOrder.Clear();
            }
        }
        return false;
    }

    // The method CheckForPops() goes through the grid to check every tile against its neighbors 2 by 2 to find and matching 3 tiles

    private HashSet<Node> CheckForPops()
    {
        HashSet<Node> nodesToPop = new HashSet<Node>();
        Node node, neighbor1, neighbor2;

        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nColumns; col++)
            {
                node = nodeGrid[row, col].GetComponent<Node>();

                for (int index = 0; index < neighborsForEvenColumns.Count; index++)
                {
                    int nextIndex = (index + 1) % neighborsForEvenColumns.Count;
                    try
                    {
                        Tuple<int, int> neighbor1Coords;
                        Tuple<int, int> neighbor2Coords;

                        if (col % 2 > 0)
                        {
                            neighbor1Coords = neighborsForOddColumns[index];
                            neighbor2Coords = neighborsForOddColumns[nextIndex];
                        }
                        else
                        {
                            neighbor1Coords = neighborsForEvenColumns[index];
                            neighbor2Coords = neighborsForEvenColumns[nextIndex];
                        }

                        neighbor1 = nodeGrid[row + neighbor1Coords.Item1, col + neighbor1Coords.Item2].GetComponent<Node>();
                        neighbor2 = nodeGrid[row + neighbor2Coords.Item1, col + neighbor2Coords.Item2].GetComponent<Node>();

                        if (node.TileColorId == neighbor1.TileColorId && neighbor1.TileColorId == neighbor2.TileColorId)
                        {
                            nodesToPop.Add(node);       // Any group of 3 tiles of matching color found in the grid are stored in nodesToPop hashlist
                            nodesToPop.Add(neighbor1);  // and returned at the end of the method
                            nodesToPop.Add(neighbor2);
                        }
                    }
                    catch (Exception)   // If the node which is being checked is at one of the edges of the grid, some of these neighbor
                    {                   // indexes are going to make the game throw IndexOutofRange exception since those indexes don't
                        continue;       // actually exist.
                    }
                }
            }
        }
        return nodesToPop;
    }

    /*
     * CheckPosition method runs when user input taken. It checks the pointer position to see if user touched a valid spot
     * to place the handle.
     */

    private Collider2D[] CheckPosition(Vector2 pointerPos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, overlapCircleRadius, tileLayerMask);    // An overlap circle is drawn to check
                                                                                                                // nearby tile colliders
        if (colliders.Length < nOverlapsToLayHandle)    // If the number of colliders that the circle overlaps aren't enough to 
            return null;                                // place a handle, method simply returns nothing

        else if (colliders.Length > nOverlapsToLayHandle)   // If it is more than required, closest colliders of required number 
        {                                                   // are taken and returned
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

    public void OperateHandle()     // This method is called by the input manager to handle the user input.
    {
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = CheckPosition(pointerPos);
        if (colliders != null)  // CheckPosition method returns the required number of colliders if there are enough around
        {                       // the input position
            if (!handle)    // If there is not a handle in the game, a new one is created and placed at where the user input
            {               // shows
                GameObject newHandle = Instantiate(handlePrefab, transform);
                handle = newHandle.transform;
                handleScript = newHandle.GetComponent<Handle>();
                handleScript.Lock(colliders);
            }
            else
            {
                handleScript.Relocate(colliders);       // If there is already a handle in the game, it is relocated to the new
            }                                           // position that the user input determined           
        }
    }

    public void RestartGrid()
    {
        ResetGrid();
        GenerateGrid();
        inputManager.UnlockInput();
    }

    public void ProcessSwipe(Vector2 pointerDownPos, Vector2 pointerUpPos)
    {
        Directions swipeDirection = Directions.None;
        if (handle)
        {
            Vector2 projection = Vector3.Project(pointerDownPos, pointerUpPos);
            float swipeProjectionAngle = Vector3.Angle(projection - (Vector2)handle.position, pointerUpPos - pointerDownPos);
            if (!IsValueInRange(swipeProjectionAngle, 30f, 150f))
                return;
            float swipeDirectionAngle = Vector3.SignedAngle(pointerUpPos - pointerDownPos, new Vector3(1f, 0f, 0f), Vector3.back);

            if(IsValueInRange(swipeDirectionAngle, 0f, 45f) || IsValueInRange(swipeDirectionAngle, -45f, 0f))
            {
                swipeDirection = Directions.Right;
            }
            else if (IsValueInRange(swipeDirectionAngle, 45f, 135f))
            {
                swipeDirection = Directions.Up;
            }
            else if (IsValueInRange(swipeDirectionAngle, 135f, 180f) || IsValueInRange(swipeDirectionAngle, -180f, -135f))
            {
                swipeDirection = Directions.Left;
            }
            else if (IsValueInRange(swipeDirectionAngle, -135f, -45f))
            {
                swipeDirection = Directions.Down;
            }

            switch (swipeDirection)
            {
                case Directions.Left:
                    if (projection.y < handle.position.y)
                        handleScript.SpinClokwise();
                    else if (projection.y > handle.position.y)
                        handleScript.SpinCounterclokwise();
                    break;
                case Directions.Right:
                    if (projection.y < handle.position.y)
                        handleScript.SpinCounterclokwise();
                    else if (projection.y > handle.position.y)
                        handleScript.SpinClokwise();
                    break;
                case Directions.Up:
                    if (projection.x < handle.position.x)
                        handleScript.SpinClokwise();
                    else if (projection.x > handle.position.x)
                        handleScript.SpinCounterclokwise();
                    break;
                case Directions.Down:
                    if (projection.x < handle.position.x)
                        handleScript.SpinCounterclokwise();
                    else if (projection.x > handle.position.x)
                        handleScript.SpinClokwise();
                    break;
            }
        }
    }

    private bool IsValueInRange(float value, float min, float max)
    {
        return value <= max && value > min;
    }
}
