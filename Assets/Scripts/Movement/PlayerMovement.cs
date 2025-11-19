using System.Collections.Generic;
using UnityEngine;
using static ChunkBehaviour;

public class PlayerMovement : PieceMovement
{
    // The player's movement patterns
    private enum PlayerPatterns
    {
        KING,
        ROOK,
        BISHOP,
        KNIGHT
    }
    [SerializeField] private PlayerPatterns _currentPattern = PlayerPatterns.KNIGHT;

    // Tracks which blocks have been made clickable to reset later
    private List<CheckedBlockBehaviour> _checkedBlocks = new List<CheckedBlockBehaviour>();

    public override void CalculatePieceMoves()
    {
        // Tracks the extended pathfinding of the rook and the bishop's moves
        bool isExtendedPathInterrupted = false;
        Vector3Int extendedPathPosition = new Vector3Int();

        switch(_currentPattern)
        {
            // Finds all adjacent blocks, including height and diagonally from the player's current coordinates (allows branching decision)
            case PlayerPatterns.KING:
            {
                for(int height = -1; height <= 1; height++)
                {
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

                            Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                            if(digPosition != null) continue;
                            Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                            if(queriedPosition == null) continue;

                            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                            checkedBlock.InitializeBlock(ChangePlayerCurrentPosition, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                        }
                    }
                }

                // The player shouldn't be able to stay still
                Destroy(transform.parent.GetComponent<CheckedBlockBehaviour>());
                break;
            }

            // Finds a maximum of GRID_SIZE blocks towards all sides from the player's current position (follows the highest block in branch situations)
            case PlayerPatterns.ROOK:
            {
                for(int horizon = -1; horizon <= 1; horizon += 2)
                {
                    isExtendedPathInterrupted = false;
                    extendedPathPosition = new Vector3Int(horizon, _currentCoordinates.Layer, 0);
                    while(!isExtendedPathInterrupted && extendedPathPosition.x % (GRID_SIZE + 1) != 0)
                    {
                        isExtendedPathInterrupted = true;

                        int nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + extendedPathPosition.x;
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

                        for(int height = 1; height >= -1; height--)
                        {
                            _checkingCoordinates.Layer = extendedPathPosition.y + height;

                            Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                            if(digPosition != null) continue;
                            Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                            if(queriedPosition == null) continue;

                            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                            checkedBlock.InitializeBlock(ChangePlayerCurrentPosition, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = false;
                            break;
                        }
                        extendedPathPosition.x += horizon * 1;
                    }
                }

                for(int depth = -1; depth <= 1; depth += 2)
                {
                    isExtendedPathInterrupted = false;
                    extendedPathPosition = new Vector3Int(0, _currentCoordinates.Layer, depth);
                    while(!isExtendedPathInterrupted && extendedPathPosition.z % (GRID_SIZE + 1) != 0)
                    {
                        isExtendedPathInterrupted = true;

                        int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + extendedPathPosition.z;
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

                        for(int height = 1; height >= -1; height--)
                        {
                            _checkingCoordinates.Layer = extendedPathPosition.y + height;

                            Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                            if(digPosition != null) continue;
                            Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                            if(queriedPosition == null) continue;

                            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                            checkedBlock.InitializeBlock(ChangePlayerCurrentPosition, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = false;
                            break;
                        }
                        extendedPathPosition.z += depth * 1;
                    }
                }
                break;
            }

            // Finds a maximum of GRID_SIZE blocks towards all corners from the player's current position (follows the highest block in branch situations)
            case PlayerPatterns.BISHOP:
            {
                for(int horizon = -1; horizon <= 1; horizon += 2)
                {
                    for(int depth = -1; depth <= 1; depth += 2)
                    {
                        isExtendedPathInterrupted = false;
                        extendedPathPosition = new Vector3Int(horizon, _currentCoordinates.Layer, depth);
                        while(!isExtendedPathInterrupted && extendedPathPosition.z % (GRID_SIZE + 1) != 0)
                        {
                            isExtendedPathInterrupted = true;

                            int
                                nextHorizonBlock = (_currentCoordinates.WrappedIndexBlock.x + extendedPathPosition.x),
                                nextDepthBlock = (_currentCoordinates.WrappedIndexBlock.y + extendedPathPosition.z);
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
                                _checkingCoordinates.Layer = extendedPathPosition.y + height;

                                Transform digPosition = FindBlock(_checkingCoordinates.Chunk, _checkingCoordinates.Layer + 1, _checkingCoordinates.UnwrappedIndexBlock);
                                if(digPosition != null) continue;
                                Transform queriedPosition = FindBlock(_checkingCoordinates.Clone());
                                if(queriedPosition == null) continue;

                                CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                                checkedBlock.InitializeBlock(ChangePlayerCurrentPosition, _checkingCoordinates.Clone());
                                _checkedBlocks.Add(checkedBlock);
                                extendedPathPosition.y += height;
                                isExtendedPathInterrupted = false;
                                break;
                            }
                            extendedPathPosition.x += horizon * 1;
                            extendedPathPosition.z += depth * 1;
                        }
                    }
                }
                break;
            }

            // Finds blocks in an L shape just like regular chess, but it also allows to make jumps up to 3 layers of height (allows going through blocks)
            case PlayerPatterns.KNIGHT:
            {
                for(int height = -3; height <= 3; height++)
                {
                    _checkingCoordinates.Layer = _currentCoordinates.Layer + height;
                    for(int horizon = -1; horizon <= 1; horizon += 2)
                    {
                        for(int depth = -1; depth <= 1; depth += 2)
                        {
                            int
                                nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + horizon,
                                nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + depth;
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
                            int
                                horizonLShapeRelativeBlockUnwrappedIndex = (GRID_SIZE + (_checkingCoordinates.WrappedIndexBlock.x + horizon)) % GRID_SIZE + _checkingCoordinates.WrappedIndexBlock.y * GRID_SIZE,
                                depthLShapeRelativeBlockUnwrappedIndex = _checkingCoordinates.WrappedIndexBlock.x + (GRID_SIZE + (_checkingCoordinates.WrappedIndexBlock.y + depth)) % GRID_SIZE * GRID_SIZE;

                            Vector2Int
                                horizonLShapeChunk = new Vector2Int
                                (
                                    _checkingCoordinates.Chunk.x + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.x + horizon) / GRID_SIZE),
                                    _checkingCoordinates.Chunk.y
                                ),
                                depthLShapeChunk = new Vector2Int
                                (
                                    _checkingCoordinates.Chunk.x,
                                    _checkingCoordinates.Chunk.y + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.y + depth) / GRID_SIZE)
                                );

                            Transform digHorizonPosition = FindBlock(horizonLShapeChunk, _checkingCoordinates.Layer + 1, horizonLShapeRelativeBlockUnwrappedIndex);
                            Transform queriedHorizonPosition = FindBlock(horizonLShapeChunk, _checkingCoordinates.Layer, horizonLShapeRelativeBlockUnwrappedIndex);
                            if(digHorizonPosition == null && queriedHorizonPosition != null)
                            {
                                CheckedBlockBehaviour checkedHorizonBlock = queriedHorizonPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                                checkedHorizonBlock.InitializeBlock
                                (
                                    ChangePlayerCurrentPosition,
                                    new Coordinates
                                    (
                                        horizonLShapeChunk,
                                        _checkingCoordinates.Layer,
                                        horizonLShapeRelativeBlockUnwrappedIndex
                                    )
                                );
                                _checkedBlocks.Add(checkedHorizonBlock);
                            }
                            
                            Transform digDepthPosition = FindBlock(depthLShapeChunk, _checkingCoordinates.Layer + 1, depthLShapeRelativeBlockUnwrappedIndex);
                            Transform queriedDepthPosition = FindBlock(depthLShapeChunk, _checkingCoordinates.Layer, depthLShapeRelativeBlockUnwrappedIndex);
                            if(digDepthPosition == null && queriedDepthPosition != null)
                            {
                                CheckedBlockBehaviour checkedDepthBlock = queriedDepthPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                                checkedDepthBlock.InitializeBlock
                                (
                                    ChangePlayerCurrentPosition,
                                    new Coordinates
                                    (
                                        depthLShapeChunk,
                                        _checkingCoordinates.Layer,
                                        depthLShapeRelativeBlockUnwrappedIndex
                                    )
                                );
                                _checkedBlocks.Add(checkedDepthBlock);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    public void ChangePlayerCurrentPosition(Coordinates blockCoordinates)
    {
        _currentCoordinates = blockCoordinates;
        TranslatePiecePosition();

        // Resets the clickable blocks
        foreach(CheckedBlockBehaviour checkedBlock in _checkedBlocks)
        {
            Destroy(checkedBlock);
        }
        _checkedBlocks = new List<CheckedBlockBehaviour>();

        GridManager gridManager = transform.root.GetComponent<GridManager>();
        if(gridManager.IsChunkMatchBoard(_currentCoordinates.Chunk) && gridManager.CurrentMatchBoardChunks.Count == 0)
        {
            gridManager.BeginMatch(_currentCoordinates.Chunk);
        }
        else
        {
            CalculatePieceMoves();
        }
    }

    // Serialization getters
    public string CurrentPatternReference => nameof(_currentPattern);
}