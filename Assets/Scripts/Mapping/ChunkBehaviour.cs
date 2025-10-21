using UnityEngine;

public class ChunkBehaviour : MonoBehaviour, ISerializedFieldProvider
{
    // Transform positions are based on the concatenating positions multiplied by the grid size
    public const int GRID_SIZE = 8;
    [SerializeField] private Vector2Int _concatenatingPosition = new Vector2Int(0, 0);
    public void TranslateConcatenatingPosition()
    {
        int concatenatingXPosition = _concatenatingPosition.x, concatenatingYPosition = _concatenatingPosition.y;
        gameObject.name = $"Chunk X{concatenatingXPosition} Z{concatenatingYPosition}";
        transform.position = new Vector3Int(concatenatingXPosition * GRID_SIZE, 0, concatenatingYPosition * GRID_SIZE);
    }

    // Getters
    public Vector2Int ConcatenatingPosition => _concatenatingPosition;

    public string GetSerializedFieldName(string name)
    {
        return name switch
        {
            "ConcatenatingPosition" => nameof(_concatenatingPosition),
            _ => throw new System.ArgumentException($"Unknown field: {name}")
        };
    }
}