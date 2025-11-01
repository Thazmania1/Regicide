using System.Linq;
using UnityEngine;
using static ChunkBehaviour;

// Used for individual piece movement patterns
public abstract class PieceMovement : MonoBehaviour
{
    // Tracks a chunk position, a layer, and a block position
    public class RegicideVector5
    {
        private Vector2Int _chunk;
        private int _layer;
        private Vector2Int _block;

        // Getters and setters
        public Vector2Int Chunk {get => _chunk; set => _chunk = value;}
        public int Layer {get => _layer; set => _layer = value;}
        public Vector2Int Block {get => _block; set => _block = value;}
    }

    // Tracks the piece's current position
    protected RegicideVector5 _currentPosition;

    // Tracks what position is being checked
    protected RegicideVector5 _checkingPosition;

    // Tracks if the piece is currently moving
    protected bool _isMoving = false;


    private void Start()
    {
        UpdatePiecePosition();
        CalculatePieceMoves();
    }

    public void UpdatePiecePosition()
    {
        _currentPosition.Chunk = transform.parent.parent.parent.GetComponent<ChunkBehaviour>().ConcatenatingPosition;
        _currentPosition.Layer = transform.parent.parent.GetComponent<LayerBehaviour>().Height;

        // Translates the block's index to an X and Z axis position for readability
        int blockUnwrappedIndex = transform.parent.GetSiblingIndex();
        int horizon = blockUnwrappedIndex % GRID_SIZE;
        int depth = blockUnwrappedIndex / GRID_SIZE;
        _currentPosition.Block = new Vector2Int(horizon, depth);

        // Resets the pathfinder
        _checkingPosition = _currentPosition;
    }

    // The piece's movement possibilities
    public abstract void CalculatePieceMoves();

    // Queries
    // Unity complains about inline null checks, so queries must explicitly look for null values
    public Transform FindChunk(Vector2Int concatenatingPositionQuery)
    {
        
        var queriedChunk = transform.root
            .GetComponentsInChildren<ChunkBehaviour>()
            .FirstOrDefault(chunk => chunk != null && chunk.ConcatenatingPosition == concatenatingPositionQuery);

        return queriedChunk != null ? queriedChunk.transform : null;
    }
}