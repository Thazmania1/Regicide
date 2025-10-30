using System.Collections.Generic;
using UnityEngine;

// Used for individual piece movement patterns
public class PieceMovement : MonoBehaviour
{
    // Tracks the piece's current chunk, layer, and block position
    protected ChunkBehaviour _currentChunk;
    protected LayerBehaviour _currentLayer;
    protected int _currentBlock;

    // Tracks if the piece is currently moving
    protected bool _isMoving = false;

    private void Start()
    {
        UpdatePiecePosition();
        CalculatePieceMoves();
    }

    public void UpdatePiecePosition()
    {
        _currentChunk = transform.root.GetComponent<ChunkBehaviour>();
        _currentLayer = transform.parent.parent.GetComponent<LayerBehaviour>();
        _currentBlock = transform.parent.GetSiblingIndex();
    }

    // The piece's movement possibilities
    public void CalculatePieceMoves()
    {
        
    }
}