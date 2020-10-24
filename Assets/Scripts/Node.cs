using System;
using UnityEngine;

/*
 * Nodes mark locations that tiles can stand on. It provides communication between GridManager and tiles. Tiles attach to nodes,
 * assign to them to let them know about their color codes. This makes checking them easier.
 */

public class Node : MonoBehaviour
{
    private TileFactory tileFactory;
    private Tile tileScript;
    private GameObject assignedTile;

    public int TileColorId { get; private set; }    // Nodes know color codes of their assigned tiles so GridManager doesn't need to communicate
                                                    // with tiles.
    public Tuple<int,int> GridCoords { get; set; }  // Grid indexes of a node is known by it. That makes locating their neighbors much easier.

    private void Awake()
    {
        tileFactory = TileFactory.Instance;
    }

    public void AssignTile(GameObject tile)
    {
        assignedTile = tile;
        tileScript = tile.GetComponent<Tile>();
        TileColorId = tileScript.GetId();
    }

    public void ReplaceTile()   // Nodes can send their tiles for reroll if their colors aren't suitable
    {
        tileFactory.RollTile(assignedTile);
        TileColorId = tileScript.GetId();
    }

    public void PopTile()
    {
        tileScript.StartPopping();
        assignedTile = null;
    }

    public bool HasTile()
    {
        return assignedTile != null;
    }

    public void RelocateTile(Transform newNode) // This is used when there are empty nodes on the grid. Tiles migrate from 
    {                                           // their assigned node to an empty node if they are at the same column
        tileScript.Migrate(newNode);
        assignedTile = null;
    }
}
