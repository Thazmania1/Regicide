using System.Collections.Generic;
using UnityEngine;
using static ChunkBehaviour;

public class PlayerMovement : PieceMovement
{
    // The player's movement patterns
    private enum PlayerPattern
    {
        KING,
        ROOK,
        BISHOP,
        KNIGHT
    }
    [SerializeField] private PlayerPattern _currentPattern = PlayerPattern.KNIGHT;

    // Tracks which blocks have been made clickable to reset later
    private List<CheckedBlockBehaviour> _checkedBlocks = new List<CheckedBlockBehaviour>();

    public override void CalculatePieceMoves()
    {
        // Tracks the extended pathfinding of the rook and the bishop's moves
        bool isExtendedPathInterrupted = false;
        Vector3Int extendedPathPosition = new Vector3Int();
        bool isBlockedByPiece = false;

        switch(_currentPattern)
        {
            // Finds all adjacent blocks, including height and diagonally from the player's current coordinates (allows branching decision)
            case PlayerPattern.KING:
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
                            checkedBlock.InitializeBlock(PlayMove, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                        }
                    }
                }

                // The player shouldn't be able to stay still
                Destroy(transform.parent.GetComponent<CheckedBlockBehaviour>());
                break;
            }

            // Finds a maximum of GRID_SIZE blocks towards all sides from the player's current position (follows the highest block in branch situations)
            case PlayerPattern.ROOK:
            {
                for(int horizon = -1; horizon <= 1; horizon += 2)
                {
                    isExtendedPathInterrupted = false;
                    isBlockedByPiece = false;
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
                            PieceMovement piece = queriedPosition.GetComponentInChildren<PieceMovement>();
                            if(piece != null) isBlockedByPiece = true;

                            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                            checkedBlock.InitializeBlock(PlayMove, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = isBlockedByPiece;
                            break;
                        }
                        extendedPathPosition.x += horizon * 1;
                    }
                }

                for(int depth = -1; depth <= 1; depth += 2)
                {
                    isExtendedPathInterrupted = false;
                    isBlockedByPiece = false;
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
                            PieceMovement piece = queriedPosition.GetComponentInChildren<PieceMovement>();
                            if(piece != null) isBlockedByPiece = true;

                            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                            checkedBlock.InitializeBlock(PlayMove, _checkingCoordinates.Clone());
                            _checkedBlocks.Add(checkedBlock);
                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = isBlockedByPiece;
                            break;
                        }
                        extendedPathPosition.z += depth * 1;
                    }
                }
                break;
            }

            // Finds a maximum of GRID_SIZE blocks towards all corners from the player's current position (follows the highest block in branch situations)
            case PlayerPattern.BISHOP:
            {
                for(int horizon = -1; horizon <= 1; horizon += 2)
                {
                    for(int depth = -1; depth <= 1; depth += 2)
                    {
                        isExtendedPathInterrupted = false;
                        isBlockedByPiece = false;
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
                                PieceMovement piece = queriedPosition.GetComponentInChildren<PieceMovement>();
                                if(piece != null) isBlockedByPiece = true;

                                CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                                checkedBlock.InitializeBlock(PlayMove, _checkingCoordinates.Clone());
                                _checkedBlocks.Add(checkedBlock);
                                extendedPathPosition.y += height;
                                isExtendedPathInterrupted = isBlockedByPiece;
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
            case PlayerPattern.KNIGHT:
            {
                for(int height = -3; height <= 3; height++)
                {
                    _checkingCoordinates.Layer = _currentCoordinates.Layer + height;

                    for(int horizon = -1; horizon <= 1; horizon += 2)
                    {
                        for(int depth = -1; depth <= 1; depth += 2)
                        {
                            Vector2Int nextBlock = new Vector2Int
                            (
                                _currentCoordinates.WrappedIndexBlock.x + horizon,
                                _currentCoordinates.WrappedIndexBlock.y + depth
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

                            // Avoids code redundancy
                            foreach(Vector2Int direction in new Vector2Int[] { new Vector2Int(horizon, 0), new Vector2Int(0, depth) })
                            {
                                int relativeBlockUnwrappedIndex =
                                    (GRID_SIZE + (_checkingCoordinates.WrappedIndexBlock.x + direction.x)) % GRID_SIZE
                                    + (GRID_SIZE + (_checkingCoordinates.WrappedIndexBlock.y + direction.y)) % GRID_SIZE * GRID_SIZE;

                                Vector2Int lShapeChunk = new Vector2Int
                                (
                                    _checkingCoordinates.Chunk.x + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.x + direction.x) / GRID_SIZE),
                                    _checkingCoordinates.Chunk.y + Mathf.FloorToInt((float)(_checkingCoordinates.WrappedIndexBlock.y + direction.y) / GRID_SIZE)
                                );

                                Transform digPosition = FindBlock(lShapeChunk, _checkingCoordinates.Layer + 1, relativeBlockUnwrappedIndex);
                                if(digPosition != null) continue;
                                Transform queriedPosition = FindBlock(lShapeChunk, _checkingCoordinates.Layer, relativeBlockUnwrappedIndex);
                                if(queriedPosition == null) continue;

                                CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
                                checkedBlock.InitializeBlock
                                (
                                    PlayMove,
                                    new Coordinates(lShapeChunk, _checkingCoordinates.Layer, relativeBlockUnwrappedIndex)
                                );
                                _checkedBlocks.Add(checkedBlock);
                            }
                        }
                    }
                }
                break;
            }
        }
        
        // Filters out non-match board chunks
        if(!_gridManager.IsMatchActive) return;

        IReadOnlyList<ChunkBehaviour> matchBoard = _gridManager.CurrentMatchBoardChunks;
        foreach(CheckedBlockBehaviour checkedBlockBehaviour in _checkedBlocks)
        {
            bool isInMatchBoard = false;
            foreach(ChunkBehaviour chunk in matchBoard)
            {
                if(chunk.ConcatenatingPosition == checkedBlockBehaviour.BlockCoordinates.Chunk)
                {
                    isInMatchBoard = true;
                    break;
                }
            }
            if(!isInMatchBoard) Destroy(checkedBlockBehaviour);
        }
    }

    protected void PlayMove(Coordinates blockCoordinates)
    {
        if(!_gridManager.IsMatchActive) _preMatchCoordinates = _currentCoordinates.Clone();
        _currentCoordinates = blockCoordinates.Clone();
        TranslatePiecePosition();

        // Resets the clickable blocks
        foreach(CheckedBlockBehaviour checkedBlock in _checkedBlocks)
        {
            Destroy(checkedBlock);
        }
        _checkedBlocks = new List<CheckedBlockBehaviour>();

        // Tries to take an enemy piece
        EnemyMovement enemyPiece = transform.parent.GetComponentInChildren<EnemyMovement>();
        if(enemyPiece != null) TakePiece(enemyPiece);

        // If the player moves to a match board, it will begin a match
        if(!_gridManager.IsMatchActive)
            if(_gridManager.IsChunkMatchBoard(_currentCoordinates.Chunk))
                _gridManager.BeginMatch(_currentCoordinates.Chunk);
            else
                CalculatePieceMoves();
        else
            _gridManager.YieldTurn();
    }

    // Serialization getters
    public string CurrentPatternReference => nameof(_currentPattern);
}