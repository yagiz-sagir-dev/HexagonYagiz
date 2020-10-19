using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int nColumns = 8;
    [SerializeField]
    private int nRows = 9;
    [SerializeField]
    private LayerMask hexagonLayerMask;
    [SerializeField]
    private float overlapCircleRadius = .15f;
    [SerializeField]
    private int nOverlapsToLayHandle = 3;
    [SerializeField]
    private Transform nodePrefab;
    [SerializeField]
    private GameObject handlePrefab;

    public static GridManager SingletonInstance { get; private set; }
    private static Transform handle;

    private Transform[,] nodeGrid;

    private const float hexagonSidesRatio = .8660254f;

    private delegate void HandlerDelegate();
    private HandlerDelegate adoptAll;

    private void Awake()
    {
        if (!SingletonInstance)
        {
            SingletonInstance = this;
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
        GenerateGrid();
        DisplayGrid();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckSurroundingHexagons();
        }
    }

    private void CheckSurroundingHexagons()
    {
        Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, overlapCircleRadius, hexagonLayerMask);
        print(colliders.Length);
        if (colliders.Length >= nOverlapsToLayHandle)
        {
            if (!handle)
            {
                handle = Instantiate(handlePrefab, transform).transform;
            }
            else
            {
                UnlockHandle();
            }
            LockHandle(colliders);
        }
    }

    private void UnlockHandle()
    {
        Hexagon[] tileScripts = handle.GetComponentsInChildren<Hexagon>();
        foreach(Hexagon tileScript in tileScripts)
        {
            tileScript.LockHexagon();
        }
    }

    private void LockHandle(Collider2D[] colliders)
    {
        Vector2[] overlappingColliderPositions = new Vector2[colliders.Length];
        for (int index = 0; index < colliders.Length; index++)
        {
            Collider2D col = colliders[index];

            overlappingColliderPositions[index] = (Vector2)col.bounds.center;
            adoptAll += () => col.transform.SetParent(handle);

            Hexagon tileScript = col.gameObject.GetComponent<Hexagon>();
            tileScript.UnlockHexagon();
        }
        handle.position = overlappingColliderPositions.FindCenterOfMass();
        adoptAll.Invoke();
        adoptAll = null;
    }

    private void DisplayGrid()
    {
        transform.localScale = new Vector3(.6f, .6f, 1f);
        transform.position = new Vector2(-nColumns * hexagonSidesRatio / 2f + .5f, nRows / 2f - .25f) * .6f;
    }

    private void GenerateGrid()
    {
        for (int col = 0; col < nColumns; col++)
        {
            for (int row = 0; row < nRows; row++)
            {
                Transform node = Instantiate(nodePrefab, transform);
                TileGenerator.GenerateTile(node);

                float posX = col * hexagonSidesRatio;
                float posY = col % 2 == 1 ? row + .5f : row;

                node.position = new Vector2(posX, -posY);
                nodeGrid[row, col] = node;
            }
        }
    }
}
