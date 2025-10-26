using System.Collections.Generic;
using UnityEngine;
using static ChunkBehaviour;

public class LayerBehaviour : MonoBehaviour
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
    public IReadOnlyList<bool> Grid => System.Array.AsReadOnly(_grid);

    // Serialization getters
    public string HeightReference => nameof(_height);
    public string GridReference => nameof(_grid);
}