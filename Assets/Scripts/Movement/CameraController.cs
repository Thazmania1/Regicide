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
    public void ChangeCameraFocus(PieceMovement pieceTarget)
    {
        _pieceTargetTransform = pieceTarget.transform;
    }
}