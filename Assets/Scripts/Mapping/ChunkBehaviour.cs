using UnityEngine;
using static GridManager;

public class ChunkBehaviour : MonoBehaviour
{
    // Defines if the chunk is a match board
    [SerializeField] private bool _isMatchBoard = false;

    // Transform world position is based on the concatenating positions multiplied by the grid size
    [SerializeField] private Vector2Int _concatenatingPosition = new Vector2Int(0, 0);
    public void TranslateConcatenatingPosition()
    {
        int concatenatingXPosition = _concatenatingPosition.x, concatenatingYPosition = _concatenatingPosition.y;
        gameObject.name = $"Chunk X{concatenatingXPosition} Z{concatenatingYPosition}";
        transform.position = new Vector3Int(concatenatingXPosition * GRID_SIZE, 0, concatenatingYPosition * GRID_SIZE);
    }

    // Returns a corner point of a chunk or a layer
    public static Vector3 CalculateChunkCorner(Transform chunkOrLayer, bool isHorizonNegative = true, bool isDepthNegative = true, bool hasOffset = true)
    {
        int horizonNegation = -1, depthNegation = -1;
        if(!isHorizonNegative) horizonNegation *= -1;
        if(!isDepthNegative) depthNegation *= -1;

        int gridCenter = Mathf.FloorToInt((float)GRID_SIZE / 2);
        float gridOffset = GRID_SIZE % 2 != 0 ? 0.0f : 0.5f;
        float gridXCorner = chunkOrLayer.position.x + gridCenter * horizonNegation;
        float gridZCorner = chunkOrLayer.position.z + gridCenter * depthNegation;
        float gridYPosition = chunkOrLayer.position.y;
        if(hasOffset)
        {
            gridXCorner += gridOffset;
            gridZCorner += gridOffset;
        }

        return new Vector3(gridXCorner, gridYPosition, gridZCorner);
    }
    public static Vector3 CalculateChunkCorner(Transform chunkOrLayer, int horizonNegation, int depthNegation, bool hasOffset = true)
    {
        int gridCenter = Mathf.FloorToInt((float)GRID_SIZE / 2);
        float gridOffset = GRID_SIZE % 2 != 0 ? 0.0f : 0.5f;
        float gridXCorner = chunkOrLayer.position.x + gridCenter * horizonNegation;
        float gridZCorner = chunkOrLayer.position.z + gridCenter * depthNegation;
        float gridYPosition = chunkOrLayer.position.y;
        if(hasOffset)
        {
            gridXCorner += gridOffset;
            gridZCorner += gridOffset;
        }

        return new Vector3(gridXCorner, gridYPosition, gridZCorner);
    }

    // Wrapper method to call the layer's redraw grid method while also applying match board logic
    private bool _wasMatchBoard;
    public void RedrawGridMaterials()
    {
        if(_isMatchBoard == _wasMatchBoard)
            return;
        else
            _wasMatchBoard = _isMatchBoard;

        foreach(LayerBehaviour layer in transform.GetComponentsInChildren<LayerBehaviour>()) layer.RedrawGridMaterials(_isMatchBoard);
    }

    private void Awake()
    {
        _wasMatchBoard = _isMatchBoard;
    }

    // Getters and setters
    public bool IsMatchBoard {get => _isMatchBoard; set => _isMatchBoard = value;}
    public Vector2Int ConcatenatingPosition => _concatenatingPosition;

    // Serialization getters
    public string IsMatchBoardReference => nameof(_isMatchBoard);
    public string ConcatenatingPositionReference => nameof(_concatenatingPosition);
}