using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Tracks all adjacent chunks in the current match boards
    private List<Vector2Int> _currentMatchBoardChunks = new List<Vector2Int>();

    public bool IsChunkMatchBoard(Vector2Int chunkPosition)
    {
        ChunkBehaviour[] chunks = transform.GetComponentsInChildren<ChunkBehaviour>();
        foreach(ChunkBehaviour chunk in chunks) if(chunk.IsMatchBoard && chunk.ConcatenatingPosition == chunkPosition) return true;
        return false;
    }

    // Finds all adjacent match board chunks in the match
    public void BeginMatch(Vector2Int matchBoardOriginChunk)
    {
        // Filters out all non-match board chunks
        ChunkBehaviour[] chunks = transform.GetComponentsInChildren<ChunkBehaviour>();
        List<Vector2Int> matchBoardChunks = new List<Vector2Int>();
        foreach(ChunkBehaviour chunk in chunks) if(chunk.IsMatchBoard) matchBoardChunks.Add(chunk.ConcatenatingPosition);

        // Continuously searches adjacent match board chunks until there's no more
        List<Vector2Int> adjacentMatchBoardChunks = new List<Vector2Int>();
        adjacentMatchBoardChunks.Add(matchBoardOriginChunk);
        for(int i = 0; i < adjacentMatchBoardChunks.Count; i++)
        {
            Vector2Int adjacentMatchBoardChunk = adjacentMatchBoardChunks[i];

            // Sides
            for(int direction = -1; direction <= 1; direction += 2)
            {
                // Horizon
                Vector2Int horizon = new Vector2Int(adjacentMatchBoardChunk.x + direction, adjacentMatchBoardChunk.y);
                foreach(Vector2Int matchBoardChunk in matchBoardChunks)
                {
                    if(matchBoardChunk == horizon)
                    {
                        if(adjacentMatchBoardChunks.Contains(matchBoardChunk)) continue;
                        adjacentMatchBoardChunks.Add(matchBoardChunk);
                        break;
                    }
                }

                // Depth
                Vector2Int depth = new Vector2Int(adjacentMatchBoardChunk.x, adjacentMatchBoardChunk.y + direction );
                foreach(Vector2Int matchBoardChunk in matchBoardChunks)
                {
                    if(matchBoardChunk == depth)
                    {
                        if(adjacentMatchBoardChunks.Contains(matchBoardChunk)) continue;
                        adjacentMatchBoardChunks.Add(matchBoardChunk);
                        break;
                    }
                }
            }

            // Diagonals
            for(int horizon = -1; horizon <= -1; horizon += 2)
            {
                for(int depth = -1; depth <= -1; depth += 2)
                {
                    Vector2Int diagonal = new Vector2Int(adjacentMatchBoardChunk.x + horizon, adjacentMatchBoardChunk.y + depth);
                    foreach(Vector2Int matchBoardChunk in matchBoardChunks)
                    {
                        if(matchBoardChunk == diagonal)
                        {
                            if(adjacentMatchBoardChunks.Contains(matchBoardChunk)) continue;
                            adjacentMatchBoardChunks.Add(matchBoardChunk);
                            break;
                        }
                    }
                }
            }
        }
        _currentMatchBoardChunks = adjacentMatchBoardChunks;
        foreach(Vector2Int PIO in _currentMatchBoardChunks)
        {
            Debug.Log(PIO);
        }
    }

    // Getters
    public IReadOnlyList<Vector2Int> CurrentMatchBoardChunks => _currentMatchBoardChunks;
}