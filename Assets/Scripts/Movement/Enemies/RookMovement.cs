using UnityEngine;
using static VirtualSpaceManager;

public class RookMovement : EnemyMovement
{
    // The rook finds a maximum of GRID_SIZE blocks towards all sides from the piece's current position (follows the highest block in branch situations)
    // The rook can move one block towards any side before making its long move (transversally from the chosen side)
    // Duplicates will be present in this calculation but that shouldn't pose a problem
    public override void CalculatePieceMoves()
    {
        // Horizon
        for(int horizon = -1; horizon <= 1; horizon++)
        {
            for(int depth = -1; depth <= 1; depth += 2)
            {
                _isExtendedPathInterrupted = false;
                _isBlockedByPiece = false;
                _extendedPathPosition = new Vector3Int(horizon, _currentCoordinates.Layer, depth);
                while(!_isExtendedPathInterrupted && _extendedPathPosition.z % (GRID_SIZE + 1) != 0)
                {
                    _isExtendedPathInterrupted = true;

                    int nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + _extendedPathPosition.x;
                    int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + _extendedPathPosition.z;
                    _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                    (
                       (nextHorizonBlock + GRID_SIZE) % GRID_SIZE,
                       (nextDepthBlock + GRID_SIZE) % GRID_SIZE
                    );
                    _checkingCoordinates.Chunk = new Vector2Int
                    (
                        _currentCoordinates.Chunk.x + Mathf.FloorToInt((float)nextHorizonBlock / GRID_SIZE),
                        _currentCoordinates.Chunk.y + Mathf.FloorToInt((float)nextDepthBlock / GRID_SIZE)
                    );

                    for(int height = 1; height >= -1; height--)
                    {
                        _checkingCoordinates.Layer = _extendedPathPosition.y + height;

                        Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                        if(digPosition != null) continue;
                        Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                        if(queriedPosition == null) continue;
                        PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                        if(pieceMovement is EnemyMovement)
                            continue;
                        else if(pieceMovement is PlayerMovement)
                            _isBlockedByPiece = true;

                        _calculatedMoves.Add(_checkingCoordinates.Clone());
                        _extendedPathPosition.y += height;
                        _isExtendedPathInterrupted = _isBlockedByPiece;
                        break;
                    }
                    _extendedPathPosition.z += depth * 1;
                }
            }
        }

        // Depth
        for(int depth = -1; depth <= 1; depth++)
        {
            for(int horizon = -1; horizon <= 1; horizon += 2)
            {
                _isExtendedPathInterrupted = false;
                _isBlockedByPiece = false;
                _extendedPathPosition = new Vector3Int(horizon, _currentCoordinates.Layer, depth);
                while(!_isExtendedPathInterrupted && _extendedPathPosition.x % (GRID_SIZE + 1) != 0)
                {
                    _isExtendedPathInterrupted = true;

                    int nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + _extendedPathPosition.x;
                    int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + _extendedPathPosition.z;
                    _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                    (
                       (nextHorizonBlock + GRID_SIZE) % GRID_SIZE,
                       (nextDepthBlock + GRID_SIZE) % GRID_SIZE
                    );
                    _checkingCoordinates.Chunk = new Vector2Int
                    (
                        _currentCoordinates.Chunk.x + Mathf.FloorToInt((float)nextHorizonBlock / GRID_SIZE),
                        _currentCoordinates.Chunk.y + Mathf.FloorToInt((float)nextDepthBlock / GRID_SIZE)
                    );

                    for(int height = 1; height >= -1; height--)
                    {
                        _checkingCoordinates.Layer = _extendedPathPosition.y + height;

                        Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                        if(digPosition != null) continue;
                        Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                        if(queriedPosition == null) continue;
                        PieceMovement pieceMovement = queriedPosition.GetComponentInChildren<PieceMovement>();
                        if(pieceMovement is EnemyMovement)
                            continue;
                        else if(pieceMovement is PlayerMovement)
                            _isBlockedByPiece = true;

                        _calculatedMoves.Add(_checkingCoordinates.Clone());
                        _extendedPathPosition.y += height;
                        _isExtendedPathInterrupted = _isBlockedByPiece;
                        break;
                    }
                    _extendedPathPosition.x += horizon * 1;
                }
            }
        }
        PlayMove();
    }
}