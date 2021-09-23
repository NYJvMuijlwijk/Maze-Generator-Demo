using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

/// <summary>
/// <para>
/// Class for generating mazes using the selected algorithm.
/// </para>
/// <para>
/// The variables for algorithm, sizing and material can be set in the inspector. 
/// </para>
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private Algorithm algorithm;
    [SerializeField] private float wallHeight = 1f;
    [SerializeField] private float wallWidth = .2f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField, Range(1, 100)] private int sizeX = 10, sizeY = 10;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private GameObject playerPrefab;

    private Maze _currentMaze;
    private const float FloorThickness = .2f;
    private GameObject _floor;
    private GameObject _mazeEmpty;
    private MeshCollider _mazeCollider;
    private GameObject _player;

    private Renderer _renderer;
    private MeshFilter _meshFilter;

    public delegate void MazeGenerated(Vector2 mazeSize, float cellSize, float wallWidth);

    public static event MazeGenerated OnMazeGenerated;

    #region UnityEvents

    private void Awake()
    {
        _mazeEmpty = new GameObject("Maze");
        _meshFilter = _mazeEmpty.AddComponent<MeshFilter>();
        _renderer = _mazeEmpty.AddComponent<MeshRenderer>();
    }

    private void Reset()
    {
        wallMaterial = new Material(Shader.Find("Standard"))
        {
            color = Color.black
        };
    }

    private void OnDestroy()
    {
        Destroy(_mazeEmpty);
        DestroyMaze();
    }

    #endregion

    #region Public

    /// <summary>
    /// Generates a maze using the selected algorithm to the selected size
    /// </summary>
    public void BuildMaze()
    {
        if (algorithm == null)
        {
            Debug.Log("No algorithm selected");
            return;
        }

        DestroyMaze();

        _currentMaze = algorithm.Generate(sizeX, sizeY);

        InstantiateFloorAndOuterWalls();

        InstantiateInnerWalls();

        CombineMeshes();

        if (_player != null)
        {
            _player.transform.position =
                GetPositionInMaze(0, 0, new Vector2(_currentMaze.SizeX, _currentMaze.SizeY), cellSize);
        }
        else if (playerPrefab != null)
            _player = Instantiate(playerPrefab,
                GetPositionInMaze(0, 0, new Vector2(_currentMaze.SizeX, _currentMaze.SizeY), cellSize),
                Quaternion.identity);

        OnMazeGenerated?.Invoke(new Vector2(_currentMaze.SizeX, _currentMaze.SizeY), cellSize, wallWidth);
    }

    public static Vector3 GetPositionInMaze(int x, int y, Vector2 mazeSize, float cellSize)
    {
        Vector3 bottomLeft = new Vector3(-mazeSize.x / 2f * cellSize + cellSize / 2, 0,
            -mazeSize.y / 2f * cellSize + cellSize / 2);

        return bottomLeft + new Vector3(x * cellSize, 0, y * cellSize);
    }

    #endregion

    #region Private

    private void CombineMeshes()
    {
        MeshFilter[] meshFilters = _mazeEmpty.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
        _renderer.material = wallMaterial;

        for (int i = meshFilters.Length - 1; i >= 1; i--) // Skip first as its the parent's and empty anyway
        {
            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            Destroy(meshFilters[i].gameObject);
        }

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combine);

        if (_mazeCollider == null)
            _mazeCollider = _mazeEmpty.AddComponent<MeshCollider>();
        else
            _mazeCollider.sharedMesh = _meshFilter.mesh;
    }

    private void InstantiateInnerWalls()
    {
        GameObject tempWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempWall.transform.localScale = new Vector3(wallWidth, wallHeight, cellSize);
        tempWall.name = "Wall";

        Vector3 bottomLeft = new Vector3(-_currentMaze.SizeX / 2f * cellSize + cellSize / 2, wallHeight / 2,
            -_currentMaze.SizeY / 2f * cellSize + cellSize / 2);

        for (int y = 0; y < _currentMaze.SizeY; y++)
        {
            for (int x = 0; x < _currentMaze.SizeX; x++)
            {
                MazeCell cell = _currentMaze.Cells[x, y];

                if (y != _currentMaze.SizeY - 1 && cell.Walls.Top)
                    Instantiate(
                        tempWall,
                        bottomLeft + new Vector3(x, 0, y) * cellSize + Vector3.forward * cellSize / 2,
                        Quaternion.Euler(0, 90, 0),
                        _mazeEmpty.transform
                    );

                if (x != _currentMaze.SizeX - 1 && cell.Walls.Right)
                    Instantiate(
                        tempWall,
                        bottomLeft + new Vector3(x, 0, y) * cellSize + Vector3.right * cellSize / 2,
                        Quaternion.identity,
                        _mazeEmpty.transform
                    );
            }
        }

        Destroy(tempWall);
    }

    private void InstantiateFloorAndOuterWalls()
    {
        Transform root = _mazeEmpty.transform;

        _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _floor.transform.localScale =
            new Vector3(_currentMaze.SizeX * cellSize, FloorThickness, _currentMaze.SizeY * cellSize);
        _floor.transform.position = new Vector3(0, -FloorThickness / 2);
        _floor.name = "Floor";

        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.transform.parent = root;
        southWall.transform.localScale =
            new Vector3(_currentMaze.SizeX * cellSize + wallWidth * 2, wallHeight, wallWidth);
        southWall.transform.position =
            new Vector3(0, wallHeight / 2, -(_currentMaze.SizeY / 2f * cellSize + wallWidth / 2));
        southWall.name = "South Wall";

        Instantiate(
            southWall,
            new Vector3(0, wallHeight / 2, _currentMaze.SizeY / 2f * cellSize + wallWidth / 2),
            Quaternion.identity,
            root
        ).name = "North Wall";

        Instantiate(
            southWall,
            new Vector3(-(_currentMaze.SizeX / 2f * cellSize + wallWidth / 2), wallHeight / 2),
            Quaternion.Euler(0, 90, 0),
            root
        ).name = "West Wall";

        Instantiate(
            southWall,
            new Vector3(_currentMaze.SizeX / 2f * cellSize + wallWidth / 2, wallHeight / 2),
            Quaternion.Euler(0, 90, 0),
            root
        ).name = "East Wall";
    }

    private void DestroyMaze()
    {
        if (_floor != null) Destroy(_floor);
    }

    #endregion
}