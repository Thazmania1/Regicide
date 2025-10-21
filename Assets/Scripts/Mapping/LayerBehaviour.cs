using System.Collections.Generic;
using UnityEngine;
using static ChunkBehaviour;

public class LayerBehaviour : MonoBehaviour, ISerializedFieldProvider
{
    // Transform Yposition is based on the height.
    [SerializeField] private int _height = 0;
    public void TranslateHeightPosition()
    {
        gameObject.name = $"Layer {_height}";
        transform.position = new Vector3Int(0, _height, 0);
    }

    // Tracks the occupied positions of the layer's grid
    [SerializeField] private bool[] _layerGrid = new bool[GRID_SIZE * GRID_SIZE];

    // Getters
    public int Height => _height;
    public IReadOnlyList<bool> LayerGrid => System.Array.AsReadOnly(_layerGrid);

    public string GetSerializedFieldName(string name)
    {
        return name switch
        {
            "LayerGrid" => nameof(_layerGrid),
            "Height" => nameof(_height),
            _ => throw new System.ArgumentException($"Unknown field: {name}")
        };
    }

    // Setters
    public void ToggleLayerGridBlock(int index)
    {
        _layerGrid[index] = !_layerGrid[index];
    }
}