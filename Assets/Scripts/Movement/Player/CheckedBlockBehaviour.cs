using System.Collections;
using UnityEditor;
using UnityEngine;
using static PieceMovement;

// Represents a clickable block for the player piece
public class CheckedBlockBehaviour : MonoBehaviour
{
    private System.Action<Coordinates> _onBlockClicked;
    private Coordinates _blockCoordinates;

    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private static AnimationCurve _higlightAnimation;
    private Coroutine _runningAnimation;

    // Animation must be declared in code due to monobehaviour limitations
    static CheckedBlockBehaviour()
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

        _higlightAnimation = new AnimationCurve(keyFrame1, keyFrame2, keyFrame3);
        _higlightAnimation.SmoothTangents(1, 0f);
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void OnMouseDown()
    {
        StartCoroutine(AntiClickFreeze());
    }

    // Wrapper method to prevent unexpected behaviour
    private IEnumerator AntiClickFreeze()
    {
        yield return null;
        _onBlockClicked?.Invoke(_blockCoordinates);
    }

    // Sets the method to invoke, the block's coordinates, and triggers the visual cue on
    public void InitializeBlock(System.Action<Coordinates> clickEvent, Coordinates blockCoordinates)
    {
        _onBlockClicked = clickEvent;
        _blockCoordinates = blockCoordinates;

        _runningAnimation = StartCoroutine(HighlightBlock());
    }

    // Visual cue for clickable blocks
    private IEnumerator HighlightBlock()
    {
        Color baseGold = Color.yellow;

        bool isBlockWhite = _renderer.sharedMaterial == GridMaterialsUtility.WhiteMaterial || _renderer.sharedMaterial == GridMaterialsUtility.WhiteRedMaterial;
        float colorMultiplier = isBlockWhite ? 1.2f : 0.6f;
        float emissionMultiplier = isBlockWhite ? 2f : 1.2f;

        _propertyBlock.Clear();
        _propertyBlock.SetColor("_BaseColor", baseGold * colorMultiplier);
        _propertyBlock.SetColor("_EmissionColor", baseGold * emissionMultiplier);

        _renderer.SetPropertyBlock(_propertyBlock);

        // Little bounce animation
        Keyframe[] animationKeyframes = _higlightAnimation.keys;
        float animationTime = animationKeyframes[animationKeyframes.Length - 1].time;
        float elapsedTime = 0;
        Vector3 blockPosition = transform.localPosition;
        while(elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            transform.localPosition = new Vector3(blockPosition.x, _higlightAnimation.Evaluate(elapsedTime), blockPosition.z);
            yield return null;
        }
        transform.localPosition = new Vector3(blockPosition.x, 0, blockPosition.z);
    }

    // Cleanly removes the script
    public void RemoveScript()
    {
        if(_runningAnimation != null) StopCoroutine(_runningAnimation);
        _renderer.SetPropertyBlock(null);
        Vector3 blockPosition = transform.localPosition;
        transform.localPosition = new Vector3(blockPosition.x, 0, blockPosition.z);
        Destroy(this);
    }

    // Getters
    public Coordinates BlockCoordinates => _blockCoordinates.Clone();
}