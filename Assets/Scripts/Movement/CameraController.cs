using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Tracks the focused piece
    [SerializeField] private PieceMovement _initialPieceTarget;
    private Transform _pieceTargetTransform;

    // Camera properties
    private float _distance = -10f;
    private float _speed = 5f;
    private Vector2 _orbitAngles = new Vector2(33f, -33f);

    // Camera focus change animation
    [SerializeField] private AnimationCurve _cameraFocusChangeAnimation;
    [SerializeField] private Transform _invisibleFocus;

    private void Start()
    {
        _pieceTargetTransform = _initialPieceTarget.transform;
    }

    private void Update()
    {
        if(_pieceTargetTransform == null) return;

        // Right click and drag to rotate around the focused piece
        if(Input.GetMouseButton(1))
        {
            _orbitAngles.x += Input.GetAxis("Mouse Y") * -_speed;
            _orbitAngles.y += Input.GetAxis("Mouse X") * _speed;
        }
        Quaternion newRotation = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0f);
        Vector3 offset = newRotation * new Vector3(0f, 0f, _distance);
        transform.position = _pieceTargetTransform.position + offset;
        transform.rotation = newRotation;

        // Mouse wheel to zoom in and out
        _distance += Input.GetAxis("Mouse ScrollWheel") * _speed * 1.5f;
        _distance = Mathf.Clamp(_distance, -20, -10);
    }

    // Changes the focus of the camera
    public IEnumerator ChangeCameraFocus(PieceMovement pieceTarget, bool isInstantaneous = false)
    {
        if(pieceTarget == null) yield break;

        if(!isInstantaneous)
        {
            Vector3 startPostion = _pieceTargetTransform.position;
            Vector3 targetPosition = pieceTarget.transform.position;

            // Temporarily sets camera target to invisible object
            _invisibleFocus.position = startPostion;
            _pieceTargetTransform = _invisibleFocus;

            Keyframe[] keyframes = _cameraFocusChangeAnimation.keys;
            float animationTime = keyframes[keyframes.Length - 1].time;
            float elapsedTime = 0f;
            while(elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp(elapsedTime / animationTime, 0f, 1f);
                float animationProgress = _cameraFocusChangeAnimation.Evaluate(progress * animationTime);
                _invisibleFocus.position = Vector3.LerpUnclamped(startPostion, targetPosition, animationProgress);
                yield return null;
            }
        }

        _pieceTargetTransform = pieceTarget.transform;
    }
}