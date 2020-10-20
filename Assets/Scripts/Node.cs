using UnityEngine;

public class Node : MonoBehaviour
{
    private Hexagon blockScript;
    private GameObject assignedBlock;

    public int BlockColorId { get; private set; }

    public void AssignBlock(GameObject block)
    {
        assignedBlock = block;
        blockScript = block.GetComponent<Hexagon>();
        BlockColorId = blockScript.Id;
    }

    public void PopBlock()
    {
        assignedBlock.transform.localScale = new Vector3(.5f, .5f, 1f);
    }
}
