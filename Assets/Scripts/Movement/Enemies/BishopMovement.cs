using System.Collections.Generic;
using UnityEngine;
using static GridManager;

public class BishopMovement : EnemyMovement
{
    // The bishop can move to its last position (if not currently occupied) before making its long move
    // Duplicates will be present in this calculation but that shouldn't pose a problem
    public override void CalculatePieceMoves()
    {
        bool calculationRedundance = _currentCoordinates.ToWorldPosition3D() == _lastCoordinates.ToWorldPosition3D(); // Avoids calculation redundance

        // Current position calculation
        for(int horizon = -1; horizon <= 1; horizon += 2)
        {
            for(int depth = -1; depth <= 1; depth += 2)
            {
                _isExtendedPathInterrupted = false;
                _isBlockedByPiece = false;
                _extendedPathPosition = new Vector3Int(horizon, _currentCoordinates.Layer, depth);
                while(!_isExtendedPathInterrupted && _extendedPathPosition.z % (GRID_SIZE + 1) != 0)
                {
                    _isExtendedPathInterrupted = true;

                    Vector2Int nextBlock = new Vector2Int
                    (
                        (_currentCoordinates.WrappedIndexBlock.x + _extendedPathPosition.x),
                        (_currentCoordinates.WrappedIndexBlock.y + _extendedPathPosition.z)
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
                    _extendedPathPosition.z += depth * 1;
                }
            }
        }

        // Last position calculation
        if(!calculationRedundance && FindBlock(_lastCoordinates.Clone()).GetComponentInChildren<PieceMovement>() == null)
        {
            for(int horizon = -1; horizon <= 1; horizon += 2)
            {
                for(int depth = -1; depth <= 1; depth += 2)
                {
                    _isExtendedPathInterrupted = false;
                    _isBlockedByPiece = false;
                    _extendedPathPosition = new Vector3Int(horizon, _lastCoordinates.Layer, depth);
                    while(!_isExtendedPathInterrupted && _extendedPathPosition.z % (GRID_SIZE + 1) != 0)
                    {
                        _isExtendedPathInterrupted = true;

                        Vector2Int nextBlock = new Vector2Int
                        (
                            (_lastCoordinates.WrappedIndexBlock.x + _extendedPathPosition.x),
                            (_lastCoordinates.WrappedIndexBlock.y + _extendedPathPosition.z)
                        );
                        _checkingCoordinates.WrappedIndexBlock = new Vector2Int
                        (
                            (nextBlock.x + GRID_SIZE) % GRID_SIZE,
                            (nextBlock.y + GRID_SIZE) % GRID_SIZE
                        );
                        _checkingCoordinates.Chunk = new Vector2Int
                        (
                            _lastCoordinates.Chunk.x + Mathf.FloorToInt((float)nextBlock.x / GRID_SIZE),
                            _lastCoordinates.Chunk.y + Mathf.FloorToInt((float)nextBlock.y / GRID_SIZE)
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
                        _extendedPathPosition.z += depth * 1;
                    }
                }
            }
        }
        _lastCoordinates = _currentCoordinates.Clone();
        PlayMove();
    }
}