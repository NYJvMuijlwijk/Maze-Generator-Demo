using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private CharacterController _controller;

    #region UnityEvents

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        MazeGenerator.OnMazeGenerated += ResetPlayerPosition;
    }

    private void OnDestroy()
    {
        MazeGenerator.OnMazeGenerated -= ResetPlayerPosition;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    #endregion

    #region Private

    private void ResetPlayerPosition(Vector2 mazeSize, float cellSize, float wallWidth)
    {
        _controller.enabled = false;
        transform.position = MazeGenerator.GetPositionInMaze(0, 0, mazeSize, cellSize);
        _controller.enabled = true;
    }

    private void HandleMovement()
    {
        Vector3 movement = new Vector3();

        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;
        }

        movement = movement.normalized * moveSpeed * Time.deltaTime;

        _controller.Move(movement);
    }

    #endregion
}