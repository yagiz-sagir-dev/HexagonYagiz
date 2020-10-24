using System;
using UnityEngine;

public class Node : MonoBehaviour
{
    private TileFactory tileFactory;
    private Tile tileScript;
    private GameObject assignedBlock;

    public int TileColorId { get; private set; }
    public Tuple<int,int> GridCoords { get; set; }

    private void Awake()
    {
        tileFactory = TileFactory.Instance;
    }

    public void AssignBlock(GameObject block)
    {
        assignedBlock = block;
        tileScript = block.GetComponent<Tile>();
        TileColorId = tileScript.GetId();
    }

    public void ReplaceBlock()
    {
        tileFactory.RollTile(assignedBlock);
        TileColorId = tileScript.GetId();
    }

    public void PopBlock()
    {
        tileScript.StartPopping();
        assignedBlock = null;
    }

    public bool HasBlock()
    {
        return assignedBlock != null;
    }

    public void RelocateBlock(Transform newNode)
    {
        tileScript.Migrate(newNode);
        assignedBlock = null;
    }
}
