﻿using UnityEngine;

public class InputManager : MonoBehaviour
{
    private HandleController handleController;

    public static InputManager Instance { get; private set; }

    private bool inputLocked = false;

    private Vector2 pointerDownPos;
    private Vector2 pointerUpPos;
    private Vector2 currentPointerPos;

    private readonly float minSwipeDistance = .1f;

    private void Awake()
    {
        #region Singleton
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
        #endregion
    }

    private void Start()
    {
        handleController = HandleController.Instance; ;
    }

#if UNITY_EDITOR

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !inputLocked)
        {
            pointerDownPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && !inputLocked)
        {
            currentPointerPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Vector3.Distance(pointerDownPos, currentPointerPos) > minSwipeDistance)
            {
                Vector2 pointerDownWorldPos = Camera.main.ViewportToWorldPoint(pointerDownPos);
                Vector2 currentPointerWorldPos = Camera.main.ViewportToWorldPoint(currentPointerPos);
                handleController.ProcessSwipe(pointerDownWorldPos, currentPointerWorldPos);
            }
        }
        else if (Input.GetMouseButtonUp(0) && !inputLocked)
        {
            pointerUpPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Vector3.Distance(pointerUpPos, pointerDownPos) < .01f)
            {
                handleController.OperateHandle();
            }
        }
    }

#elif UNITY_ANDROID

    void Update()
    {
        if (Input.touchCount > 0 && !inputLocked)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                pointerDownPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
            }
            else if (Input.touches[0].phase == TouchPhase.Moved)
            {
                currentPointerPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                if (Vector3.Distance(pointerDownPos, currentPointerPos) > minSwipeDistance)
                {
                    Vector2 pointerDownWorldPos = Camera.main.ViewportToWorldPoint(pointerDownPos);
                    Vector2 currentPointerWorldPos = Camera.main.ViewportToWorldPoint(currentPointerPos);
                    handleController.ProcessSwipe(pointerDownWorldPos, currentPointerWorldPos);
                }
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                pointerUpPos = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                if (Vector3.Distance(pointerUpPos, pointerDownPos) < .01f)
                {
                    handleController.OperateHandle();
                }
            }
        }
    }

#endif

    public void LockInput()
    {
        inputLocked = true;
    }

    public void UnlockInput()
    {
        inputLocked = false;
    }
}
