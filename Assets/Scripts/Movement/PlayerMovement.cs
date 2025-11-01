using System.Collections.Generic;
using UnityEngine;
using static ChunkBehaviour;

public class PlayerMovement : PieceMovement
{
    public enum PlayerPatterns
    {
        KING,
        ROOK,
        BISHOP,
        KNIGHT
    }

    public override void CalculatePieceMoves()
    {
        bool isPathInterrupted = false;

        // Finds all adjacent blocks, including height and diagonally from the player's current position
        List<Vector2Int> lastCheckedLayerBlocks = new List<Vector2Int>();
        for(int height = +1; height >= 1; height--)
        {
            _checkingPosition.Layer = _currentPosition.Layer + height;
            for(int horizon = -1; horizon <= 1; horizon++)
            {
                int nextHorizonBlock = _currentPosition.Block.x + horizon;
                _checkingPosition.Block.Set
                (
                    (nextHorizonBlock + GRID_SIZE) % GRID_SIZE,
                    _currentPosition.Block.y
                );
                
                _checkingPosition.Chunk.Set
                (
                    Mathf.FloorToInt((float)nextHorizonBlock / GRID_SIZE),
                    _currentPosition.Chunk.y
                );

                for(int depth = -1; depth <= 1; depth++)
                {
                    int nextDepthBlock = _currentPosition.Block.y + depth;
                    _checkingPosition.Block.Set
                    (
                        _checkingPosition.Block.x,
                        (nextDepthBlock + GRID_SIZE) % GRID_SIZE
                    );

                    _checkingPosition.Chunk.Set
                    (
                        _checkingPosition.Block.x,
                        Mathf.FloorToInt((float)nextDepthBlock / GRID_SIZE)
                    );
                }
            }
        }
    }
}