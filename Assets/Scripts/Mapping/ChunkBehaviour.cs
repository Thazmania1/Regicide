using System.Collections;
using UnityEngine;
using static VirtualSpaceManager;

public class ChunkBehaviour : MonoBehaviour
{
    // Defines if the chunk is a match board
    [SerializeField] private bool _isMatchBoard = false;

    // Transform world position is based on the concatenating positions multiplied by the grid size
    [SerializeField] private Vector2Int _concatenatingPosition = new Vector2Int(0, 0);
    public void TranslateConcatenatingPosition()
    {
        int concatenatingXPosition = _concatenatingPosition.x, concatenatingYPosition = _concatenatingPosition.y;
        gameObject.name = $"Chunk X{concatenatingXPosition} Z{concatenatingYPosition}";
        transform.position = new Vector3Int(concatenatingXPosition * GRID_SIZE, 0, concatenatingYPosition * GRID_SIZE);
    }

    // Returns a corner point of a chunk or a layer
    public static Vector3 CalculateChunkCorner(Transform chunkOrLayer, bool isHorizonNegative = true, bool isDepthNegative = true, bool hasOffset = true)
    {
        int horizonNegation = -1, depthNegation = -1;
        if(!isHorizonNegative) horizonNegation *= -1;
        if(!isDepthNegative) depthNegation *= -1;

        int gridCenter = Mathf.FloorToInt((float)GRID_SIZE / 2);
        float gridOffset = GRID_SIZE % 2 != 0 ? 0.0f : 0.5f;
        float gridXCorner = chunkOrLayer.position.x + gridCenter * horizonNegation;
        float gridZCorner = chunkOrLayer.position.z + gridCenter * depthNegation;
        float gridYPosition = chunkOrLayer.position.y;
        if(hasOffset)
        {
            gridXCorner += gridOffset;
            gridZCorner += gridOffset;
        }

        return new Vector3(gridXCorner, gridYPosition, gridZCorner);
    }
    public static Vector3 CalculateChunkCorner(Transform chunkOrLayer, int horizonNegation, int depthNegation, bool hasOffset = true)
    {
        int gridCenter = Mathf.FloorToInt((float)GRID_SIZE / 2);
        float gridOffset = GRID_SIZE % 2 != 0 ? 0.0f : 0.5f;
        float gridXCorner = chunkOrLayer.position.x + gridCenter * horizonNegation;
        float gridZCorner = chunkOrLayer.position.z + gridCenter * depthNegation;
        float gridYPosition = chunkOrLayer.position.y;
        if(hasOffset)
        {
            gridXCorner += gridOffset;
            gridZCorner += gridOffset;
        }

        return new Vector3(gridXCorner, gridYPosition, gridZCorner);
    }

    // Animation must be declared in code due to monobehaviour limitations
    private static AnimationCurve _boardStateChangeAnimation;
    static ChunkBehaviour()
    {
        Keyframe keyFrame1 = new Keyframe(0f, 0f);
        Keyframe keyFrame2 = new Keyframe(0.125f, 0.25f);
        Keyframe keyFrame3 = new Keyframe(0.25f, 0f);

        keyFrame1.inTangent = 0f;
        keyFrame1.outTangent = 0f;

        keyFrame3.inTangent = 0f;
        keyFrame3.outTangent = 0f;

        keyFrame2.inTangent = float.PositiveInfinity;
        keyFrame2.outTangent = float.PositiveInfinity;

        _boardStateChangeAnimation = new AnimationCurve(keyFrame1, keyFrame2, keyFrame3);
        _boardStateChangeAnimation.SmoothTangents(1, 0f);
    }

    // Board state change animation
    private bool _wasMatchBoard;
    public void BoardStateChange()
    {
        if(_isMatchBoard == _wasMatchBoard)
            return;
        else
            _wasMatchBoard = _isMatchBoard;

        // Redraws the grids in the layers
        foreach(LayerBehaviour layer in transform.GetComponentsInChildren<LayerBehaviour>()) layer.RedrawGridMaterials(_isMatchBoard);
    }
    public IEnumerator AnimatedBoardStateChange()
    {
        if(_isMatchBoard == _wasMatchBoard)
            yield break;
        else
            _wasMatchBoard = _isMatchBoard;

        // Redraws the grids in the layers
        foreach(LayerBehaviour layer in transform.GetComponentsInChildren<LayerBehaviour>()) StartCoroutine(layer.AnimatedRedrawGridMaterials(_isMatchBoard));

        // Little bounce animation
        Keyframe[] animationKeyframes = _boardStateChangeAnimation.keys;
        float animationTime = animationKeyframes[animationKeyframes.Length - 1].time;
        float elapsedTime = 0;
        Vector3 blockPosition = transform.localPosition;
        while(elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            transform.localPosition = new Vector3(blockPosition.x, _boardStateChangeAnimation.Evaluate(elapsedTime), blockPosition.z);
            yield return null;
        }
        transform.localPosition = new Vector3(blockPosition.x, 0, blockPosition.z);
    }

    private void Awake()
    {
        _wasMatchBoard = _isMatchBoard;
    }

    // Getters and setters
    public bool IsMatchBoard {get => _isMatchBoard; set => _isMatchBoard = value;}
    public Vector2Int ConcatenatingPosition => _concatenatingPosition;

    // Serialization getters
    public string IsMatchBoardReference => nameof(_isMatchBoard);
    public string ConcatenatingPositionReference => nameof(_concatenatingPosition);
}