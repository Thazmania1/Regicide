using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Trackers
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Transform _invisibleFocus;

    // Camera focus change animation
    [SerializeField] private AnimationCurve _cameraFocusChangeAnimation;

    // Camera properties
    private float _distance = 10f;
    private float _minimumDistance = 10f;
    private float _maximumDistance = 20f;
    private float _speed = 5f;
    private Vector2 _orbitAngles = new Vector2(33f, -33f);

    private void Update()
    {
        if(_targetTransform == null) return;

        // Right click and drag to rotate around the focused piece
        if(Input.GetMouseButton(1))
        {
            _orbitAngles.x += Input.GetAxis("Mouse Y") * -_speed;
            _orbitAngles.y += Input.GetAxis("Mouse X") * _speed;
        }

        Quaternion newRotation = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0f);
        Vector3 offset = newRotation * new Vector3(0f, 0f, -_distance);
        transform.position = _targetTransform.position + offset;
        transform.rotation = newRotation;

        // Mouse wheel to zoom in and out
        _distance += -Input.GetAxis("Mouse ScrollWheel") * _speed;
        _distance = Mathf.Clamp(_distance, _minimumDistance, _maximumDistance);
    }

    // Changes the focus of the camera
    public IEnumerator ChangeCameraFocus(Transform target, bool isInstantaneous = false, float newMinimumDistance = 10f, float newMaximumDistance = 20f)
    {
        Vector3 startPosition = _targetTransform.position;
        Vector3 targetPosition = target.position;

        float startDistance = _distance;
        float relativeDistancePosition = (_distance - _minimumDistance) / (_maximumDistance - _minimumDistance);
        float targetDistance = Mathf.Lerp(newMinimumDistance, newMaximumDistance, relativeDistancePosition);

        // Uses invisible focus as the animation anchor
        _invisibleFocus.position = startPosition;
        _targetTransform = _invisibleFocus;

        if(!isInstantaneous)
        {
            Keyframe[] keyframes = _cameraFocusChangeAnimation.keys;
            float animationTime = keyframes[keyframes.Length - 1].time;
            float elapsedTime = 0f;
            while(elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / animationTime);
                float animationProgress = _cameraFocusChangeAnimation.Evaluate(progress * animationTime);
                _invisibleFocus.position = Vector3.LerpUnclamped(startPosition, targetPosition, animationProgress);
                _distance = Mathf.LerpUnclamped(startDistance, targetDistance, animationProgress);
                yield return null;
            }
        }
        else
        {
            _invisibleFocus.position = targetPosition;
            _distance = targetDistance;
        }

        // Ensures final values are exact
        _invisibleFocus.position = targetPosition;
        _minimumDistance = newMinimumDistance;
        _maximumDistance = newMaximumDistance;
        _distance = targetDistance;
        _targetTransform = target;
    }

    public IEnumerator ChangeCameraFocus(Vector3 targetPosition, bool isInstantaneous = false, float newMinimumDistance = 10f, float newMaximumDistance = 20f)
    {
        Vector3 startPosition = _targetTransform.position;

        float startDistance = _distance;
        float relativeDistancePosition = (_distance - _minimumDistance) / (_maximumDistance - _minimumDistance);
        float targetDistance = Mathf.Lerp(newMinimumDistance, newMaximumDistance, relativeDistancePosition);

        _invisibleFocus.position = startPosition;
        _targetTransform = _invisibleFocus;

        if(!isInstantaneous)
        {
            Keyframe[] keyframes = _cameraFocusChangeAnimation.keys;
            float animationTime = keyframes[keyframes.Length - 1].time;
            float elapsedTime = 0f;
            while(elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / animationTime);
                float animationProgress = _cameraFocusChangeAnimation.Evaluate(progress * animationTime);
                _invisibleFocus.position = Vector3.LerpUnclamped(startPosition, targetPosition, animationProgress);
                _distance = Mathf.LerpUnclamped(startDistance, targetDistance, animationProgress);
                yield return null;
            }
        }
        else
        {
            _invisibleFocus.position = targetPosition;
            _distance = targetDistance;
        }

        // Ensures final values are exact
        _invisibleFocus.position = targetPosition;
        _minimumDistance = newMinimumDistance;
        _maximumDistance = newMaximumDistance;
        _distance = targetDistance;
    }

    public IEnumerator MatchBoardConversionFocus(IReadOnlyList<ChunkBehaviour> matchBoard)
    {
        if(matchBoard == null) yield break;

        Vector3 sumXZ = Vector3.zero;
        int chunkCount = 0;
        foreach(ChunkBehaviour chunk in matchBoard)
        {
            sumXZ.x += chunk.transform.position.x;
            sumXZ.z += chunk.transform.position.z;
            chunkCount++;
        }
        float centerX = sumXZ.x / chunkCount;
        float centerZ = sumXZ.z / chunkCount;

        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach(ChunkBehaviour chunk in matchBoard)
        {
            LayerBehaviour[] layers = chunk.GetComponentsInChildren<LayerBehaviour>();
            foreach(LayerBehaviour layer in layers)
            {
                float y = layer.transform.position.y;
                if(y < minY) minY = y;
                if(y > maxY) maxY = y;
            }
        }
        float centerY = (minY + maxY) * 0.5f;
        Vector3 boardCenter = new Vector3(centerX, centerY, centerZ);

        yield return StartCoroutine(ChangeCameraFocus(boardCenter, isInstantaneous: false, newMinimumDistance: 20f, newMaximumDistance: 30f));
    }
}