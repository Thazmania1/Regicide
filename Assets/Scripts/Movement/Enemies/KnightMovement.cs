using UnityEngine;
using static GridManager;
  
public class KnightMovement : EnemyMovement
{
    // As long as there's a chunk and layers, the knight can move to inactive blocks
    private GameObject _inactiveBlock = null;
    public override void CalculatePieceMoves()
    {
        for(int height = -3; height <= 3; height++)
        {
            // Skips layer check entirely if it doesn't exist
            _checkingCoordinates.Layer = _currentCoordinates.Layer + height;
            Transform queriedLayer = FindLayer(GetChunk, _checkingCoordinates.Layer);
            if(queriedLayer == null) continue;

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

                        Coordinates newCoordinates = new Coordinates
                        (
                            lShapeChunk,
                            _checkingCoordinates.Layer,
                            relativeBlockUnwrappedIndex
                        );
                        _calculatedMoves.Add(newCoordinates);
                    }
                }
            }
        }
        PlayMove();
    }

    // Overrides the default functionality to allow the knight's gimmick to work
    public override bool TranslatePiecePosition(bool isInstantaneous = false)
    {
        Transform queriedBlock = FindBlock(_currentCoordinates.Chunk, _currentCoordinates.Layer, _currentCoordinates.UnwrappedIndexBlock, false);
        if(queriedBlock == null) return false;

        // If the knight is currently on an invisible block, the block becomes inactive again
        if(_inactiveBlock != null)
        {
            _inactiveBlock.SetActive(false);
            _inactiveBlock.GetComponent<MeshRenderer>().enabled = true;
        }

        // The knight becomes inactive if they move to an inactive block, so they make said block active but invisible until they leave from it
        GameObject queriedBlockObject = queriedBlock.gameObject;
        if(!queriedBlockObject.activeInHierarchy)
        {
            queriedBlockObject.GetComponent<MeshRenderer>().enabled = false;
            queriedBlockObject.SetActive(true);
            _inactiveBlock = queriedBlockObject;
        }
        else
            _inactiveBlock = null;

        if(!isInstantaneous)
        {
            transform.SetParent(queriedBlock, true);
            StartCoroutine(PieceMovementAnimation());
        }
        else
        {
            transform.SetParent(queriedBlock);
            transform.localPosition = new Vector3(0, 1, 0);
        }

        // Resets the pathfinder
        _checkingCoordinates = _currentCoordinates.Clone();
        return true;
    }
}