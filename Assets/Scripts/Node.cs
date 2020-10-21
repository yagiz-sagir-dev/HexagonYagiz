using System;
using UnityEngine;

public class Node : MonoBehaviour
{
    private TileGenerator tileGenerator;
    private Hexagon blockScript;
    private GameObject assignedBlock;

    public int BlockColorId { get; private set; }
    public Tuple<int,int> GridCoords { get; set; }

    private void Awake()
    {
        tileGenerator = TileGenerator.Instance;
    }

    public void AssignBlock(GameObject block)
    {
        assignedBlock = block;
        blockScript = block.GetComponent<Hexagon>();
        BlockColorId = blockScript.Id;
    }

    public void ReplaceBlock()
    {
        tileGenerator.RerollTile(assignedBlock);
        BlockColorId = blockScript.Id;
    }

    public void PopBlock()
    {
        blockScript.StartPopping();
        assignedBlock = null;
    }

    public bool HasBlock()
    {
        return assignedBlock != null;
    }

    public void RelocateBlock(Transform newNode)
    {
        blockScript.Migrate(newNode);
        assignedBlock = null;
    }
}
