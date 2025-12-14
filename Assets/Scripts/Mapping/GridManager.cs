using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    // Defines the size of the chunks
    public const int GRID_SIZE = 8;

    // UI references
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _matchDataPanel;
    [SerializeField] private TextMeshProUGUI _matchTimerText;
    [SerializeField] private TextMeshProUGUI _playerMovesText;
    [SerializeField] private Image _matchLossCover;
    [SerializeField] private float _matchLossCoverTime;

    // Camera reference
    [SerializeField] private CameraController _cameraController;

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
    private PlayerMovement _currentMatchPlayerPiece = null;

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

        _runningTimer = StartCoroutine(MatchTimer());
        _playerMovesText.text = $"Moves: {_playerMoves}";
        _matchDataPanel.SetActive(true);
    }

    private Coroutine _runningTimer = null;
    private IEnumerator MatchTimer()
    {
        while(true)
        {
            _matchTime += Time.deltaTime;
            _matchTimerText.text = $"Time {_matchTime:F2}s";
            yield return null;
        }
    }

    // Team turn toggler, also checks for win conditions
    public IEnumerator YieldTurn()
    {
        _currentTurn = _currentTurn == MatchTeam.PLAYER ? MatchTeam.ENEMIES : MatchTeam.PLAYER;
        if(_currentTurn == MatchTeam.PLAYER)
        {
            _currentTurn = MatchTeam.ENEMIES; // Prevents the player from switching to another pattern momentarily

            // Pans camera to the player
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(_cameraController.ChangeCameraFocus(_currentMatchPlayerPiece.transform));
            _currentMatchPlayerPiece.CalculatePieceMoves();
            _currentTurn = MatchTeam.PLAYER;
        }
        else
        {
            _playerMoves++;
            _playerMovesText.text = $"Moves: {_playerMoves}";

            bool areAllEnemiesTaken = true;
            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces)
            {
                if(!enemyPiece.gameObject.activeInHierarchy) continue;
                areAllEnemiesTaken = false;

                // Pans camera to the next moving enemy and fakes AI thinking
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(_cameraController.ChangeCameraFocus(enemyPiece.transform));
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));

                enemyPiece.CalculatePieceMoves();
                if(!_currentMatchPlayerPiece.gameObject.activeInHierarchy) { StartCoroutine(EndMatch(MatchTeam.ENEMIES)); yield break; }
            }
            
            if(areAllEnemiesTaken)
                StartCoroutine(EndMatch(MatchTeam.PLAYER));
            else
                StartCoroutine(YieldTurn());
        }
    }

    // Ends the match with a winner
    public IEnumerator EndMatch(MatchTeam winner)
    {
        StopCoroutine(_runningTimer);

        // If the player wins, the match board turns into a regular explorable area, and destroys all enemies that were in it
        // If the enemies wins, the pieces are sent back to their positions before the match started
        float elapsedTime = 0;
        if(winner == MatchTeam.PLAYER)
        {
            // Fades off the entire UI
            elapsedTime = 1;
            while(elapsedTime > 0)
            {
                elapsedTime -= Time.deltaTime;
                _canvasGroup.alpha = elapsedTime;
                yield return null;
            }
            _matchDataPanel.SetActive(false);

            // Scene for match board conversion
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(_cameraController.MatchBoardConversionFocus(CurrentMatchBoardChunks));
            yield return new WaitForSeconds(1f);
            foreach(ChunkBehaviour chunk in _currentMatchBoardChunks)
            {
                chunk.IsMatchBoard = false;
                StartCoroutine(chunk.AnimatedBoardStateChange());
                yield return new WaitForSeconds(0.01f);
            }
            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces) Destroy(enemyPiece.gameObject);
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(_cameraController.ChangeCameraFocus(_currentMatchPlayerPiece.transform));
            
            // Fades on the entire UI
            while(elapsedTime < 1)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = elapsedTime;
                yield return null;
            }
        }
        else
        {
            // Black screen fade in animation
            while(elapsedTime < _matchLossCoverTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp(elapsedTime / _matchLossCoverTime, 0, 1);
                _matchLossCover.color = new Color(0, 0, 0, progress);
                yield return null;
            }

            foreach(EnemyMovement enemyPiece in _currentMatchEnemyPieces) enemyPiece.ResetPiece();
            _currentMatchPlayerPiece.ResetPiece();
            yield return StartCoroutine(_cameraController.ChangeCameraFocus(_currentMatchPlayerPiece.transform, true));
            _matchDataPanel.SetActive(false);
            yield return new WaitForSeconds(0.1f);

            // Black screen fade out animation
            elapsedTime = _matchLossCoverTime;
            while(elapsedTime > 0f)
            {
                elapsedTime -= Time.deltaTime;
                float progress = Mathf.Clamp(elapsedTime / _matchLossCoverTime, 0f, 1f);
                _matchLossCover.color = new Color(0, 0, 0, progress);
                yield return null;
            }
        }
        _currentMatchBoardChunks = new List<ChunkBehaviour>();
        _currentMatchEnemyPieces = new List<EnemyMovement>();
        _isMatchActive = false;
        _currentMatchPlayerPiece.CalculatePieceMoves();
        _currentMatchPlayerPiece = null;
    }

    // Getters
    public bool IsMatchActive => _isMatchActive;
    public IReadOnlyList<ChunkBehaviour> CurrentMatchBoardChunks => _currentMatchBoardChunks;
    public PlayerMovement CurrentMatchPlayerPiece => _currentMatchPlayerPiece;
    public MatchTeam CurrentTurn => _currentTurn;
}