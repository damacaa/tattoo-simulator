using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//https://www.youtube.com/watch?v=zVX9-c_aZVg
public class CameraBehaviour : MonoBehaviour
{
    public static CameraBehaviour instance;

    private float _rotationY = 0;
    private float _rotationX = 0;

    [SerializeField]
    private Transform _target;
    Vector3 defaultTargetPosition;

    [SerializeField]
    private float mouseSensitivity = 7;
    [SerializeField]
    private float scrollSensitivity = 0.1f;

    [SerializeField]
    internal float _distanceFromTarget = 3f;
    [SerializeField]
    private float maxDistance = 5f;
    [SerializeField]
    private float minDistance = 0.1f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;
    private Vector3 _movementVelocity = Vector3.zero;
    Vector3 targetPosition;

    [SerializeField]
    private float _smoothTime = 0.2f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(-90, 90);

    [SerializeField]
    private bool freeMovement = true;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        defaultTargetPosition = _target.position;
        targetPosition = defaultTargetPosition;
    }

    void Update()
    {
        // Apply clamping for x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);

        // Apply damping between rotation changes
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
        transform.localEulerAngles = _currentRotation;

        // Substract forward vector of the GameObject to point its forward vector to the target
        targetPosition = Vector3.SmoothDamp(targetPosition, _target.position, ref _movementVelocity, _smoothTime);
        transform.position = targetPosition - transform.forward * _distanceFromTarget;
    }

    public void Zoom(float amount)
    {
        _distanceFromTarget = Mathf.Clamp(_distanceFromTarget - (amount * scrollSensitivity) * _distanceFromTarget, minDistance, maxDistance);
    }

    public void Rotate(float mouseX, float mouseY)
    {
        if (freeMovement)
        {
            _rotationY += mouseX * mouseSensitivity;
            _rotationX -= mouseY * mouseSensitivity;
        }
    }

    public void RotateRight()
    {
        _rotationY -= 90;
    }

    public void RotateLeft()
    {
        _rotationY += 90;
    }

    public void RotateUp()
    {
        _rotationX += 180;
    }

    public void RotateDown()
    {
        _rotationX -= 180;
    }

    public void Reset()
    {
        _rotationY = 0f;
        _rotationX = 0f;
        _target.position = defaultTargetPosition;
    }

    public void MoveCenter(float x, float y)
    {
        _target.Translate(_distanceFromTarget* mouseSensitivity * (transform.right * x + transform.up * y));
    }

    public void MoveTargetTo(Vector3 v) {
        _target.position = v;
    }

}
