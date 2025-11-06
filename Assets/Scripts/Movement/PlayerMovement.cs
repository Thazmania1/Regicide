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
        // The player shouldn't be able to "dig"
        List<Vector2Int> lastLayerFoundBlocks = new List<Vector2Int>();

        // Finds all adjacent blocks, including height and diagonally from the player's current coordinates
        for(int height = 1; height >= -1; height--)
        {
            List<Vector2Int> currentLayerFoundBlocks = new List<Vector2Int>();

            _checkingCoordinates.Layer = _currentCoordinates.Layer + height;
            for(int horizon = -1; horizon <= 1; horizon++)
            {
                int nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + horizon;
                _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                (
                    (nextHorizonBlock + GRID_SIZE) % GRID_SIZE,
                    _currentCoordinates.WrappedIndexBlock.y
                );

                _checkingCoordinates.Chunk = new Vector2Int
                (
                    _currentCoordinates.Chunk.x + Mathf.FloorToInt((float)nextHorizonBlock / GRID_SIZE),
                    _currentCoordinates.Chunk.y
                );

                for(int depth = -1; depth <= 1; depth++)
                {
                    int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + depth;
                    _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                    (
                        _checkingCoordinates.WrappedIndexBlock.x,
                        (nextDepthBlock + GRID_SIZE) % GRID_SIZE
                    );

                    _checkingCoordinates.Chunk = new Vector2Int
                    (
                        _checkingCoordinates.Chunk.x,
                        _currentCoordinates.Chunk.y + Mathf.FloorToInt((float)nextDepthBlock / GRID_SIZE)
                    );


                    if(lastLayerFoundBlocks.Contains(_checkingCoordinates.WrappedIndexBlock)) continue;

                    Transform queriedPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer, _checkingCoordinates.UnwrappedIndexBlock);
                    if(queriedPosition == null) continue;

                    queriedPosition.gameObject.AddComponent<AudioSource>();
                    currentLayerFoundBlocks.Add(_checkingCoordinates.WrappedIndexBlock);
                }
            }

            lastLayerFoundBlocks = new List<Vector2Int>(currentLayerFoundBlocks);
        }

        // The player shouldn't be able to stay still
        Destroy(transform.parent.GetComponent<AudioSource>());
    }
}