using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ChunkBehaviour;

public class LayerBehaviour : MonoBehaviour, ISerializedFieldProvider
{
    // Transform Yposition is based on the height.
    [SerializeField] private int _height = 0;
    public void TranslateHeightPosition()
    {
        gameObject.name = $"Layer {_height}";
        transform.localPosition = new Vector3Int(0, _height, 0);
    }

    // Tracks the occupied positions of the layer's grid
    [SerializeField] private bool[] _grid = new bool[GRID_SIZE * GRID_SIZE];

    // Getters
    public int Height => _height;
    public IReadOnlyList<bool> LayerGrid => System.Array.AsReadOnly(_grid);

    public string GetSerializedFieldName(string name)
    {
        return name switch
        {
            "Grid" => nameof(_grid),
            "Height" => nameof(_height),
            _ => throw new System.ArgumentException($"Unknown field: {name}")
        };
    }
}