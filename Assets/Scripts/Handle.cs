using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10f;

    private bool spinning;
    private float angle;

    private const float rotationAngle = 120f;

    private void Awake()
    {
        spinning = false;
        angle = 0f;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pointerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pointerPos, Vector2.zero);
            if (hit)
            {
                if (hit.transform.name == transform.name)
                {
                    spinning = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (spinning)
        {
            transform.Rotate(transform.forward, rotationSpeed);
            angle += rotationSpeed;
            if (angle >= rotationAngle)
            {
                angle = 0f;
                spinning = false;
            }
        }
    }
}
