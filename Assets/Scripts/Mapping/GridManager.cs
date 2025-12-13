using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class GridManager : MonoBehaviour
{
    // Defines the size of the chunks
    public const int GRID_SIZE = 8;

    // UI references
    [SerializeField] private GameObject _matchDataPanel;
    [SerializeField] private TextMeshProUGUI _matchTimerText;
    [SerializeField] private TextMeshProUGUI _playerMovesText;

    // Defines the teams in matches
    public enum MatchTeam
    {
        PLAYER,
        ENEMIES
    }

    // Tracks all adjacent chunks in the current match boards
    private bool _isMatchActive = false;
    private List<ChunkBehaviour> _currentMatchBoardChunks = new List<ChunkBehaviour>();

    // Match data trackings
    private float _matchTime = 0;
    private int _playerMoves = 0;

    // Tracks all pieces in the match board
    private List<EnemyMovement> _currentMatchEnemyPieces = new List<EnemyMovement>();
    private PlayerMovement _currentMatchPlayerPiece;

    // Tracks which team holds the turn, by default, the player always goes first
    private MatchTeam _currentTurn = MatchTeam.PLAYER;

    public bool IsChunkMatchBoard(Vector2Int chunkPosition)
    {
        ChunkBehaviour[] chunks = transform.GetComponentsInChildren<ChunkBehaviour>();
        foreach(ChunkBehaviour chunk in chunks) if(chunk.IsMatchBoard && chunk.ConcatenatingPosition == chunkPosition) return true;
        return false;
    }

    // Finds all adjacent match board chunks in the match and their pieces
    public void BeginMatch(Vector2Int matchBoardOriginChunk)
    {
        _isMatchActive = true;
        _matchTime = 0;
        _playerMoves = 0;

        // Filters out all non-match board chunks
        ChunkBehaviour[] chunks = transform.GetComponentsInChildren<ChunkBehaviour>();
        List<ChunkBehaviour> matchBoardChunks = new List<ChunkBehaviour>();
        foreach(ChunkBehaviour chunk in chunks) if(chunk.IsMatchBoard) matchBoardChunks.Add(chunk);

        // Continuously searches adjacent match board chunks until there's no more
        List<ChunkBehaviour> adjacentMatchBoardChunks = new List<ChunkBehaviour>();
        foreach(ChunkBehaviour chunk in chunks) if(chunk.ConcatenatingPosition == matchBoardOriginChunk) { adjacentMatchBoardChunks.Add(chunk); _currentMatchPlayerPiece = chunk.GetComponentInChildren<PlayerMovement>(); }
        for(int i = 0; i < adjacentMatchBoardChunks.Count; i++)
        {
            ChunkBehaviour adjacentMatchBoardChunk = adjacentMatchBoardChunks[i];
            Vector2Int adjacentMatchBoardChunkPosition = adjacentMatchBoardChunk.ConcatenatingPosition;

            // Sides
            for(int direction = -1; direction <= 1; direction += 2)
            {
                // Horizon
                Vector2Int horizon = new Vector2Int(adjacentMatchBoardChunkPosition.x + direction, adjacentMatchBoardChunkPosition.y);
                foreach(ChunkBehaviour matchBoardChunk in matchBoardChunks)
                {
                    if(matchBoardChunk.ConcatenatingPosition == horizon)
                    {
                        if(adjacentMatchBoardChunks.Contains(matchBoardChunk)) continue;
                        adjacentMatchBoardChunks.Add(matchBoardChunk);
                        break;
                    }
                }

                // Depth
                Vector2Int depth = new Vector2Int(adjacentMatchBoardChunkPosition.x, adjacentMatchBoardChunkPosition.y + direction);
                foreach(ChunkBehaviour matchBoardChunk in matchBoardChunks)
                {
                    if(matchBoardChunk.ConcatenatingPosition == depth)
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
                    Vector2Int diagonal = new Vector2Int(adjacentMatchBoardChunkPosition.x + horizon, adjacentMatchBoardChunkPosition.y + depth);
                    foreach(ChunkBehaviour matchBoardChunk in matchBoardChunks)
                    {
                        if(matchBoardChunk.ConcatenatingPosition == diagonal)
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

        // Gets all the enemy pieces in the match board and gives the player the first move
        foreach(ChunkBehaviour adjacentMatchBoardChunk in adjacentMatchBoardChunks) _currentMatchEnemyPieces.AddRange(adjacentMatchBoardChunk.GetComponentsInChildren<EnemyMovement>(true));
        _currentTurn = MatchTeam.PLAYER;
        _currentMatchPlayerPiece.CalculatePieceMoves();

        StartCoroutine(MatchTimer());
        _playerMovesText.text = $"Moves: {_playerMoves}";
        _matchDataPanel.SetActive(true);
    }

    private IEnumerator MatchTimer()
    {
        while(_isMatchActive)
        {
            _matchTime += Time.deltaTime;
            _matchTimerText.text = $"Time {_matchTime:F2}s";
            yield return null;
        }
    }

    // Team turn toggler, also checks for win conditions
    public void YieldTurn()
    {
        _currentTurn = _currentTurn == MatchTeam.PLAYER ? MatchTeam.ENEMIES : MatchTeam.PLAYER;
        if(_currentTurn == MatchTeam.PLAYER)
            _currentMatchPlayerPiece.CalculatePieceMoves();
        else
        {
            _playerMoves++;
            _playerMovesText.text = $"Moves: {_playerMoves}";

            bool areAllEnemiesTaken = true;
            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces)
            {
                if(!enemyPiece.gameObject.activeInHierarchy) continue;
                areAllEnemiesTaken = false;
                enemyPiece.CalculatePieceMoves();
                if(!_currentMatchPlayerPiece.gameObject.activeInHierarchy) { EndMatch(MatchTeam.ENEMIES); return; }
            }
            if(areAllEnemiesTaken)
                EndMatch(MatchTeam.PLAYER);
            else
                YieldTurn();
        }
    }

    // Ends the match with a winner
    public void EndMatch(MatchTeam winner)
    {
        _isMatchActive = false;
        _matchDataPanel.SetActive(false);

        // If the player wins, the match board turns into a regular explorable area, and destroys all enemies that were in it
        // If the enemies wins, the pieces are sent back to their positions before the match started
        if(winner == MatchTeam.PLAYER)
        {
            foreach(ChunkBehaviour chunk in _currentMatchBoardChunks)
            {
                chunk.IsMatchBoard = false;
                chunk.RedrawGridMaterials();
            }
            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces) Destroy(enemyPiece.gameObject);
        }
        else
        {
            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces) enemyPiece.ResetPiece();
            _currentMatchPlayerPiece.ResetPiece();
        }
        _currentMatchBoardChunks = new List<ChunkBehaviour>();
        _currentMatchEnemyPieces = new List<EnemyMovement>();
        _currentMatchPlayerPiece.CalculatePieceMoves();
    }

    // Getters
    public bool IsMatchActive => _isMatchActive;
    public IReadOnlyList<ChunkBehaviour> CurrentMatchBoardChunks => _currentMatchBoardChunks;
    public PlayerMovement CurrentMatchPlayerPiece => _currentMatchPlayerPiece;

    // Serialization getters
    public string MatchDataPanelReference = nameof(_matchDataPanel);
    public string MatchTimerTextReference = nameof(_matchTimerText);
    public string PlayerMovesReference = nameof(_playerMovesText);
}