using System.Linq;
using UnityEngine;
using static ChunkBehaviour;

// Used for individual piece movement patterns
public abstract class PieceMovement : MonoBehaviour
{
    // Tracks a chunk position, a layer height, and a block position
    [System.Serializable] public struct PieceCoordinates
    {
        [SerializeField] private Vector2Int _chunk;
        [SerializeField] private int _layer;
        [SerializeField] private Vector2Int _block;

        // Getters and setters
        public Vector2Int Chunk {get => _chunk; set => _chunk = value;}
        public int Layer {get => _layer; set => _layer = value;}
        public Vector2Int WrappedIndexBlock {get => _block; set => _block = value;}
        public int UnwrappedIndexBlock => _block.x + _block.y * GRID_SIZE;

        // Serialization getters
        public string ChunkReference => nameof(_chunk);
        public string LayerReference => nameof(_layer);
        public string BlockReference => nameof(_block);
    }

    // Tracks the piece's current position
    [SerializeField] protected PieceCoordinates _currentCoordinates = new PieceCoordinates();

    // Tracks what position is being checked
    protected PieceCoordinates _checkingCoordinates = new PieceCoordinates();

    private void Start()
    {
        TranslatePiecePosition();
        CalculatePieceMoves();
    }

    // Block parent is based on the piece's current coordinates
    public bool TranslatePiecePosition()
    {
        Transform queriedPosition = FindBlock(_currentCoordinates.Chunk, _currentCoordinates.Layer, _currentCoordinates.UnwrappedIndexBlock);
        if(queriedPosition == null) return false;

        transform.parent = queriedPosition;
        transform.localPosition = new Vector3(0, 1, 0);

        // Resets the pathfinder
        _checkingCoordinates = _currentCoordinates;
        return true;
    }

    // Queries
    public Transform FindChunk(Vector2Int concatenatingPositionQuery)
    {
        var queriedChunk = transform.root
            .GetComponentsInChildren<ChunkBehaviour>()
            .FirstOrDefault(chunk => chunk != null && chunk.ConcatenatingPosition == concatenatingPositionQuery);

        return queriedChunk != null ? queriedChunk.transform : null;
    }

    public Transform FindLayer(Transform chunk, int heightQuery)
    {
        var queriedLayer = chunk
            .GetComponentsInChildren<LayerBehaviour>()
            .FirstOrDefault(layer => layer != null && layer.Height == heightQuery);

        return queriedLayer != null ? queriedLayer.transform : null;
    }

    public Transform FindBlock(Transform layer, int unwrappedIndexQuery)
    {
        Transform queriedBlock = layer.GetChild(unwrappedIndexQuery);
        return queriedBlock.gameObject.activeInHierarchy ? queriedBlock : null;
    }

    public Transform FindBlock(Vector2Int concatenatingPositionQuery, int heightQuery, int unwrappedIndexQuery)
    {
        Transform queriedChunk = FindChunk(concatenatingPositionQuery);
        if(queriedChunk == null) return null;

        Transform queriedLayer = FindLayer(queriedChunk, heightQuery);
        if(queriedLayer == null) return null;

        Transform queriedBlock = FindBlock(queriedLayer, unwrappedIndexQuery);
        if(queriedBlock == null) return null;

        return queriedBlock;
    }

    // The piece's movement possibilities
    public abstract void CalculatePieceMoves();


    // Getters
    public PieceCoordinates CurrentPosition => _currentCoordinates;
    public PieceCoordinates CheckingPosition => _checkingCoordinates;

    // Serialization getters
    public string CurrentPositionReference => nameof(_currentCoordinates);
}