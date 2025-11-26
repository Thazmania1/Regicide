using UnityEngine;
using static ChunkBehaviour;

public class PawnMovement : EnemyMovement
{
    // If the pawn has started moving towards a direction, it will only be able to keep going that same direction until interrupted. When interrupted it gets to choose its next direction
    // The pawn can use diagonal movement only when taking the player
    private Vector2Int _lockedDirection = Vector2Int.zero;
    public override void CalculatePieceMoves()
    {
        Vector2Int
            currentPosition = _currentCoordinates.ToWorldPosition2D(),
            lastPosition = _lastCoordinates.ToWorldPosition2D();
        _lockedDirection = new Vector2Int
        (
            currentPosition.x - lastPosition.x,
            currentPosition.y - lastPosition.y
        );
        _lastCoordinates = _currentCoordinates.Clone();

        if(_lockedDirection == Vector2Int.zero)
        {
            for(int height = -1; height <= 1; height++)
            {
                _checkingCoordinates.Layer = _currentCoordinates.Layer + height;

                // Horizon
                for(int horizon = -1; horizon <= 1; horizon += 2)
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

                    Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                    if(digPosition != null) continue;
                    Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                    if(queriedPosition == null) continue;
                    PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                    if(pieceMovement is EnemyMovement) continue;
                    
                    _calculatedMoves.Add(_checkingCoordinates.Clone());
                }

                // Depth
                for(int depth = -1; depth <= 1; depth += 2)
                {
                    int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + depth;
                    _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                    (
                        _currentCoordinates.WrappedIndexBlock.x,
                        (nextDepthBlock + GRID_SIZE) % GRID_SIZE
                    );
                    _checkingCoordinates.Chunk = new Vector2Int
                    (
                        _currentCoordinates.Chunk.x,
                        _currentCoordinates.Chunk.y + Mathf.FloorToInt((float)nextDepthBlock / GRID_SIZE)
                    );

                    Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                    if(digPosition != null) continue;
                    Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                    if(queriedPosition == null) continue;
                    PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                    if(pieceMovement is EnemyMovement) continue;

                    _calculatedMoves.Add(_checkingCoordinates.Clone());
                }

                // Diagonal
                for(int horizon = -1; horizon <= 1; horizon += 2)
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

                    for(int depth = -1; depth <= 1; depth += 2)
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

                        Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                        if(digPosition != null) continue;
                        Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                        if(queriedPosition == null) continue;
                        PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                        if(pieceMovement is PlayerMovement) _calculatedMoves.Add(_checkingCoordinates.Clone());
                    }
                }
            }
        }
        else
        {
            Vector2Int nextBlock = new Vector2Int
            (
                _currentCoordinates.WrappedIndexBlock.x + _lockedDirection.x,
                _currentCoordinates.WrappedIndexBlock.y + _lockedDirection.y
            );
            _checkingCoordinates.WrappedIndexBlock = new Vector2Int
            (
                (nextBlock.x + GRID_SIZE) % GRID_SIZE,
                (nextBlock.y + GRID_SIZE) % GRID_SIZE
            );
            _checkingCoordinates.Chunk = new Vector2Int
            (
                _currentCoordinates.Chunk.x + Mathf.FloorToInt((float)nextBlock.x / GRID_SIZE),
                _currentCoordinates.Chunk.y + Mathf.FloorToInt((float)nextBlock.y / GRID_SIZE)
            );

            bool isPathInterrupted = true;
            for(int height = -1; height <= 1; height++)
            {
                _checkingCoordinates.Layer = _currentCoordinates.Layer + height;

                // Checks diagonals first, as they're independent of the possibility of moving straight forward
                for(int direction = -1; direction <= 1; direction += 2)
                {
                    int nextRelativeBlockUnwrappedIndex =
                        (((_checkingCoordinates.WrappedIndexBlock.x + _lockedDirection.y * direction) + GRID_SIZE) % GRID_SIZE)
                        + (((_checkingCoordinates.WrappedIndexBlock.y + _lockedDirection.x * direction) + GRID_SIZE) % GRID_SIZE) * GRID_SIZE;
                    Vector2Int nextRelativeChunk = new Vector2Int
                    (
                        _checkingCoordinates.Chunk.x + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.x + _lockedDirection.y * direction) / GRID_SIZE),
                        _checkingCoordinates.Chunk.y + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.y + _lockedDirection.x * direction) / GRID_SIZE)
                    );
                    Coordinates checkingRelativeCoordenates = new Coordinates
                    (
                        nextRelativeChunk,
                        _checkingCoordinates.Layer,
                        nextRelativeBlockUnwrappedIndex
                    );

                    Transform digRelativePosition = FindBlock(checkingRelativeCoordenates.Chunk, checkingRelativeCoordenates.Layer + 1, checkingRelativeCoordenates.UnwrappedIndexBlock);
                    if(digRelativePosition != null) continue;
                    Transform queriedRelativePosition = FindBlock(checkingRelativeCoordenates.Chunk, checkingRelativeCoordenates.Layer, checkingRelativeCoordenates.UnwrappedIndexBlock);
                    if(queriedRelativePosition == null) continue;
                    PieceMovement relativePieceMovement = queriedRelativePosition.GetComponentInChildren<PieceMovement>();
                    if(relativePieceMovement is PlayerMovement) _calculatedMoves.Add(checkingRelativeCoordenates);
                }

                Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                if(digPosition != null) continue;
                Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                if(queriedPosition == null) continue;
                PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                if(pieceMovement is EnemyMovement) continue;

                _calculatedMoves.Add(_checkingCoordinates.Clone());
                isPathInterrupted = false;
            }

            if(isPathInterrupted) { CalculatePieceMoves(); return; }
        }
        PlayMove();
    }
}