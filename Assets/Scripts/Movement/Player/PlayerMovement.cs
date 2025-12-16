using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static VirtualSpaceManager;

public class PlayerMovement : PieceMovement
{
    // The player's movement patterns
    public enum PlayerPattern
    {
        KING,
        ROOK,
        BISHOP,
        KNIGHT
    }

    [SerializeField] private PlayerPattern _currentPattern = PlayerPattern.KING;
    [SerializeField] private AnimationCurve _UIIconSelectionAnimation;
    [SerializeField] private GameObject _playerPatternsPanel;

    // Tracks which blocks have been made clickable to reset later
    private List<CheckedBlockBehaviour> _checkedBlocks = new List<CheckedBlockBehaviour>();

    // Stores candidate positions before scripts are applied (for block highlight animation)
    private readonly List<Coordinates> _checkedBlockPositions = new List<Coordinates>();
    private void RegisterCheckedPosition(Coordinates coordinates)
    {
        for(int i = 0; i < _checkedBlockPositions.Count; i++) if(_checkedBlockPositions[i].ToWorldPosition3D() == coordinates.ToWorldPosition3D()) return;
        _checkedBlockPositions.Add(coordinates.Clone());
    }

    public override void CalculatePieceMoves()
    {
        ResetClickableBlocks();

        // Tracks the extended pathfinding of the rook and the bishop's moves
        bool isExtendedPathInterrupted;
        bool isBlockedByPiece;
        Vector3Int extendedPathPosition;

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

                            RegisterCheckedPosition(_checkingCoordinates.Clone());
                        }
                    }
                }
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

                            RegisterCheckedPosition(_checkingCoordinates.Clone());

                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = isBlockedByPiece;
                            break;
                        }

                        extendedPathPosition.x += horizon;
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

                            RegisterCheckedPosition(_checkingCoordinates.Clone());

                            extendedPathPosition.y += height;
                            isExtendedPathInterrupted = isBlockedByPiece;
                            break;
                        }

                        extendedPathPosition.z += depth;
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

                            int nextHorizonBlock = _currentCoordinates.WrappedIndexBlock.x + extendedPathPosition.x;
                            int nextDepthBlock = _currentCoordinates.WrappedIndexBlock.y + extendedPathPosition.z;

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

                                RegisterCheckedPosition(_checkingCoordinates.Clone());

                                extendedPathPosition.y += height;
                                isExtendedPathInterrupted = isBlockedByPiece;
                                break;
                            }

                            extendedPathPosition.x += horizon;
                            extendedPathPosition.z += depth;
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

                            foreach
                            (
                                Vector2Int direction in new Vector2Int[]
                                {
                                    new Vector2Int(horizon, 0),
                                    new Vector2Int(0, depth)
                                }
                            )
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

                                RegisterCheckedPosition
                                (
                                    new Coordinates
                                    (
                                        lShapeChunk,
                                        _checkingCoordinates.Layer,
                                        relativeBlockUnwrappedIndex
                                    )
                                );
                            }
                        }
                    }
                }
                break;
            }
        }

        // Filters out blocks that are non-existent and from non-match board chunks if there's a match active
        if(_virtualPieceManager.IsMatchActive)
        {
            IReadOnlyList<ChunkBehaviour> matchBoard = _virtualPieceManager.CurrentMatchBoardChunks;

            for(int i = _checkedBlockPositions.Count - 1; i >= 0; i--)
            {
                Coordinates coordinates = _checkedBlockPositions[i];

                Transform queriedPosition = FindBlock(coordinates.Clone());
                if(queriedPosition == null)
                {
                    _checkedBlockPositions.RemoveAt(i);
                    continue;
                }

                bool isInMatchBoard = false;
                foreach(ChunkBehaviour chunk in matchBoard)
                {
                    if(chunk.ConcatenatingPosition == coordinates.Chunk)
                    {
                        isInMatchBoard = true;
                        break;
                    }
                }

                if(!isInMatchBoard) _checkedBlockPositions.RemoveAt(i);
            }
        }

        // Applies the CheckedBlockBehaviour script to the remaining valid blocks
        StartCoroutine(ApplyCheckedBlockBehaviours());
    }

    // Applies the CheckedBlockBehaviour script based on what the list of positions is
    private bool _keepApplyingCheckedBlockBehaviours = true;
    private IEnumerator ApplyCheckedBlockBehaviours()
    {
        _keepApplyingCheckedBlockBehaviours = true;
        for(int i = 0; i < _checkedBlockPositions.Count; i++)
        {
            if(!_keepApplyingCheckedBlockBehaviours) yield break;
            Coordinates blockCoordinates = _checkedBlockPositions[i];
            if(blockCoordinates.ToWorldPosition3D() == _currentCoordinates.ToWorldPosition3D()) continue;
            Transform queriedPosition = FindBlock(blockCoordinates.Clone());
            CheckedBlockBehaviour checkedBlock = queriedPosition.gameObject.AddComponent<CheckedBlockBehaviour>();
            checkedBlock.InitializeBlock(PlayMove, blockCoordinates.Clone());
            _checkedBlocks.Add(checkedBlock);
            if(_keepApplyingCheckedBlockBehaviours) yield return new WaitForSeconds(0.005f);
        }
    }

    public void ResetClickableBlocks()
    {
        _keepApplyingCheckedBlockBehaviours = false;
        foreach(CheckedBlockBehaviour checkedBlock in _checkedBlocks) checkedBlock.RemoveScript();
        _checkedBlocks.Clear();
        _checkedBlockPositions.Clear();
    }

    protected void PlayMove(Coordinates blockCoordinates)
    {
        if(!_virtualPieceManager.IsMatchActive) _preMatchCoordinates = _currentCoordinates.Clone();
        _currentCoordinates = blockCoordinates.Clone();
        TranslatePiecePosition();
        ResetClickableBlocks();

        // Tries to take an enemy piece
        EnemyMovement enemyPiece = transform.parent.GetComponentInChildren<EnemyMovement>();
        if(enemyPiece != null) TakePiece(enemyPiece);

        // If the player moves to a match board, it will begin a match
        if(!_virtualPieceManager.IsMatchActive)
            if(_virtualPieceManager.IsChunkMatchBoard(_currentCoordinates.Chunk))
                _virtualPieceManager.BeginMatch(_currentCoordinates.Chunk);
            else
                CalculatePieceMoves();
        else
            StartCoroutine(_virtualPieceManager.YieldTurn());
    }

    // Player pattern selection logic
    private Dictionary<KeyCode, PlayerPattern> _patternKeyMap = new Dictionary<KeyCode, PlayerPattern>
    {
        { KeyCode.Q, PlayerPattern.KING },
        { KeyCode.W, PlayerPattern.ROOK },
        { KeyCode.E, PlayerPattern.BISHOP },
        { KeyCode.R, PlayerPattern.KNIGHT }
    };
    private Dictionary<string, RectTransform> _iconMap = new Dictionary<string, RectTransform>();
    private RectTransform _lastSelectedIcon;
    private Dictionary<RectTransform, Coroutine> _runningAnimations = new Dictionary<RectTransform, Coroutine>();
    private void Awake()
    {
        foreach(RectTransform icon in _playerPatternsPanel.transform)
        {
            _iconMap[icon.name] = icon;

            // Applies the keybind to each respective icon's labeñ
            PlayerPattern pattern;
            if(System.Enum.TryParse(icon.name, out pattern))
            {
                if(pattern == _currentPattern) _lastSelectedIcon = icon;
                KeyCode boundKey = KeyCode.None;
                foreach(var kvp in _patternKeyMap)
                {
                    if(kvp.Value == pattern)
                    {
                        boundKey = kvp.Key;
                        break;
                    }
                }

                icon.GetComponentInChildren<TextMeshProUGUI>().text = boundKey.ToString();
            }
        }
    }
    private void Update()
    {
        if(_virtualPieceManager.IsMatchActive && _virtualPieceManager.CurrentTurn != MatchTeam.PLAYER) return; // Prevents cheesing the match turn system

        // Constantly checks for pattern changes
        PlayerPattern lastPattern = _currentPattern;
        foreach(var patternKey in _patternKeyMap)
        {
            if(Input.GetKeyDown(patternKey.Key))
            {
                _currentPattern = patternKey.Value;
                break;
            }
        }

        if(lastPattern != _currentPattern)
        {
            UpdateIcons();
            ResetClickableBlocks();
            CalculatePieceMoves();
        }
    }
    public void UpdateIcons()
    {
        if(_iconMap.TryGetValue(_currentPattern.ToString(), out RectTransform selectedIcon))
        {
            // Animates new and previous selection
            StartIconAnimation(selectedIcon, true);
            if(_lastSelectedIcon != null && _lastSelectedIcon != selectedIcon) StartIconAnimation(_lastSelectedIcon, false);

            _lastSelectedIcon = selectedIcon;
        }
    }
    private void StartIconAnimation(RectTransform icon, bool isSelected)
    {
        // Interrupts any previous animation
        if(_runningAnimations.TryGetValue(icon, out Coroutine running)) StopCoroutine(running);

        Coroutine newCoroutine = StartCoroutine(IconAnimation(icon, isSelected));
        _runningAnimations[icon] = newCoroutine;
    }
    private IEnumerator IconAnimation(RectTransform icon, bool isSelected)
    {
        // Makes sure to start the animation from any previous interrupted animation
        Keyframe[] keys = _UIIconSelectionAnimation.keys;
        float startTime = keys[0].time;
        float endTime = keys[1].time;
        float duration = Mathf.Abs(endTime - startTime);

        float targetFrom = isSelected ? startTime : endTime;
        float targetTo = isSelected ? endTime : startTime;

        float currentScale = icon.localScale.x;
        float startScale = _UIIconSelectionAnimation.Evaluate(targetFrom);
        float endScale = _UIIconSelectionAnimation.Evaluate(targetTo);

        float lastTime = Mathf.InverseLerp(startScale, endScale, currentScale);
        float currentTimeOnCurve = Mathf.Lerp(targetFrom, targetTo, lastTime);

        float timeElapsed = 0f;
        float curveSpan = Mathf.Abs(targetTo - currentTimeOnCurve);
        while(timeElapsed < curveSpan)
        {
            timeElapsed += Time.deltaTime;
            float newTime = Mathf.Clamp01(timeElapsed / curveSpan);

            float curveTime = Mathf.Lerp(currentTimeOnCurve, targetTo, newTime);
            float scaleValue = _UIIconSelectionAnimation.Evaluate(curveTime);

            icon.localScale = Vector3.one * scaleValue;

            yield return null;
        }

        icon.localScale = Vector3.one * _UIIconSelectionAnimation.Evaluate(targetTo);
    }

    public override void ResetPiece()
    {
        _currentCoordinates = _preMatchCoordinates.Clone();
        TranslatePiecePosition(true);
        gameObject.SetActive(true);
    }

    // Getters
    public GameObject PlayerPatternsPanel => _playerPatternsPanel;
    public PlayerPattern CurrentPattern => _currentPattern;

    // Serialization getters
    public string CurrentPatternReference => nameof(_currentPattern);
    public string UIIconSelectionAnimationReference => nameof(_UIIconSelectionAnimation);
    public string PlayerPatternsPanelReference => nameof(_playerPatternsPanel);
}