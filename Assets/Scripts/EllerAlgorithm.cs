using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "Eller's", menuName = "Algorithm", order = 0)]
public class EllerAlgorithm : Algorithm
{
    [SerializeField, Range(0, 1)] private float mergeChance = 0.5f;
    [SerializeField, Range(0, 1)] private float verticalChance = 0.5f;

    private readonly List<List<CellCollection>> _generatedMaze = new List<List<CellCollection>>();

    public override Maze Generate(int width, int height)
    {
        Init(height);

        for (int i = 0; i < height - 1; i++)
        {
            InitRow(i, width);

            MergeCollections(i);

            AddVerticalConnections(i);
        }

        InitLastRow(width, height);

        return ParseGeneratedMaze(width, height);
    }

    private Maze ParseGeneratedMaze(int width, int height)
    {
        MazeCell[,] cells = new MazeCell[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool top = false, right = false, bottom = false, left = false;
                ConnectionResult cellConnectionResult = GetCellConnections(x, y);

                if (y == height - 1 || !cellConnectionResult.HasVerticalConnection) top = true;
                if (x == width - 1 || !cellConnectionResult.HasConnectionRight) right = true;
                if (y == 0 || cells[x, y - 1].Walls.Top) bottom = true;
                if (x == 0 || cells[x - 1, y].Walls.Right) left = true;

                WallLocations walls = new WallLocations(top, right, bottom, left);

                MazeCell cell = new MazeCell(x, y, walls);
                cells[x, y] = cell;
            }
        }

        return new Maze(width, height, cells);
    }

    private void Init(int height)
    {
        _generatedMaze.Clear();

        for (int i = 0; i < height; i++)
            _generatedMaze.Add(new List<CellCollection>());
    }

    private void InitRow(int index, int width)
    {
        if (index > 0)
        {
            int minIndex = _generatedMaze[index - 1].Select(c => c.ID).Max() + 1;

            for (int i = 0; i < width; i++)
            {
                ConnectionResult result = GetCellConnections(i, index - 1);

                _generatedMaze[index].Add(result.HasVerticalConnection
                    ? new CellCollection(result.CollectionId)
                    : new CellCollection(minIndex + i)
                );
            }
        }
        else
            for (int i = 0; i < width; i++)
                _generatedMaze[index].Add(new CellCollection(i));
    }

    private void MergeCollections(int index)
    {
        List<CellCollection> row = _generatedMaze[index];

        for (int i = row.Count - 1; i > 0; i--)
        {
            CellCollection current = row[i];
            CellCollection next = row[i - 1];

            if (next.ID == current.ID || Random.value > mergeChance) continue;

            next.Merge(current);
            row.Remove(current);
        }
    }

    private void AddVerticalConnections(int index)
    {
        List<CellCollection> row = _generatedMaze[index];

        foreach (CellCollection cellCollection in row)
        {
            int randIndex = Random.Range(0, cellCollection.VerticalConnections.Count);
            cellCollection.VerticalConnections[randIndex] = true;

            if (cellCollection.VerticalConnections.Count == 1) continue;

            for (int i = 0; i < cellCollection.VerticalConnections.Count; i++)
            {
                if (cellCollection.VerticalConnections[i] || Random.value > verticalChance) continue;

                cellCollection.VerticalConnections[i] = true;
            }
        }
    }

    private ConnectionResult GetCellConnections(int x, int y)
    {
        int count = 0;
        int collectionId = -1;
        bool verticalConnection = false;
        bool connectionRight = false;
        // bool connectionLeft = false;

        foreach (CellCollection collection in _generatedMaze[y])
        {
            if (count + collection.VerticalConnections.Count > x)
            {
                collectionId = collection.ID;
                verticalConnection = collection.VerticalConnections[x - count];
                connectionRight = x - count < collection.VerticalConnections.Count - 1;
                // connectionLeft = x - count > 0;
                break;
            }

            count += collection.VerticalConnections.Count;
        }

        if (collectionId == -1) throw new Exception("Collection ID not found or set");

        return new ConnectionResult(verticalConnection, connectionRight, collectionId);
    }

    private void InitLastRow(int width, int height)
    {
        InitRow(height - 1, width);
        List<CellCollection> row = _generatedMaze.Last();

        for (int i = row.Count - 1; i > 0; i--)
        {
            CellCollection current = row[i];
            CellCollection next = row[i - 1];

            if (next.ID == current.ID) continue;

            next.Merge(current);
            row.Remove(current);
        }
    }
}

internal readonly struct ConnectionResult
{
    public bool HasVerticalConnection { get; }

    public bool HasConnectionRight { get; }

    // public bool HasConnectionLeft { get; }
    public int CollectionId { get; }

    public ConnectionResult(bool hasVerticalConnection, bool hasConnectionRight, /*bool hasConnectionLeft,*/
        int collectionId)
    {
        HasVerticalConnection = hasVerticalConnection;
        HasConnectionRight = hasConnectionRight;
        // HasConnectionLeft = hasConnectionLeft;
        CollectionId = collectionId;
    }
}

internal class CellCollection
{
    public List<bool> VerticalConnections { get; } = new List<bool>();
    public int ID { get; }

    public CellCollection(int id, int length = 1)
    {
        ID = id;
        for (int i = 0; i < length; i++)
            VerticalConnections.Add(false);
    }

    public void Merge(CellCollection otherCollection)
    {
        VerticalConnections.AddRange(otherCollection.VerticalConnections);
    }
}