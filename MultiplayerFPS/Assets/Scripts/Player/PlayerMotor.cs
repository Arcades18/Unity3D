using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    private Vector3 _velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotation = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    [SerializeField] private float cameraRotationLimit = 85f;

    [SerializeField] private Camera _camera;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }
    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }
    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }
    public void ApplyThrusterForce(Vector3 _thruterForce)
    {
        thrusterForce = _thruterForce;
    }
    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }
    private void PerformMovement()
    {
        if(_velocity != Vector3.zero)
        {
            _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
        }
        if(thrusterForce != Vector3.zero)
        {
            _rigidbody.AddForce(thrusterForce * Time.fixedDeltaTime,ForceMode.Acceleration);
        }
    }
    private void PerformRotation()
    {
        if(rotation != Vector3.zero)
        {
            _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(rotation));
        }

        if(_camera != null)
        {
            currentCameraRotation -= cameraRotationX;
            currentCameraRotation = Mathf.Clamp(currentCameraRotation,-cameraRotationLimit,cameraRotationLimit);

            _camera.transform.localEulerAngles = new Vector3(currentCameraRotation, 0f, 0f);
        }
    }

}
