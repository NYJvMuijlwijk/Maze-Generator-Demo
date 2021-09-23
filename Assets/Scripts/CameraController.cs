using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private float _aspectRatio;

    // Start is called before the first frame update
    void Start()
    {
        MazeGenerator.OnMazeGenerated += UpdateCameraSize;
        _camera = GetComponent<Camera>();
        _aspectRatio = _camera.aspect;
    }

    private void OnDestroy()
    {
        MazeGenerator.OnMazeGenerated -= UpdateCameraSize;
    }

    private void UpdateCameraSize(Vector2 mazeDimensions, float cellSize, float wallWidth)
    {
        float widthRequired = mazeDimensions.x / _camera.aspect;
        _camera.orthographicSize =
            (widthRequired > mazeDimensions.y ? widthRequired : mazeDimensions.y) / 2 * cellSize + wallWidth;
    }
}