using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Setting")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sensitivity = 3f;
    [SerializeField] private float thrusterForce = 1000f;
    [SerializeField] private float thrusterFuelBurnSpeed = 1f;
    [SerializeField] private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [Header("Spring Setting")]
    [SerializeField] private float jointSpring = 20f;
    [SerializeField] private float jointMaxForce = 40f;

    private Animator animator;
    private ConfigurableJoint joint;
    private PlayerMotor motor;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();
        SetJointSetting(jointSpring);
    }
    private void Update()
    {
        RaycastHit _hit;
        if(Physics.Raycast(transform.position,Vector3.down, out _hit, 100f))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");

        Vector3 movHorizontal = transform.right * xMove;
        Vector3 movVertical = transform.forward * zMove;

        Vector3 velocity = (movHorizontal + movVertical) * speed;

        animator.SetFloat("ForwardVelocity", zMove);

        motor.Move(velocity);

        float yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, yRotation, 0f) * sensitivity;

        motor.Rotate(_rotation);

        float xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = xRotation * sensitivity;

        motor.RotateCamera(_cameraRotationX);

        Vector3 _thruterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if(thrusterFuelAmount >= 0.01f)
            {
                _thruterForce = Vector3.up * thrusterForce;
                SetJointSetting(0f);
            }    
        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSetting(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);
            motor.ApplyThrusterForce(_thruterForce);
    }
    private void SetJointSetting(float _jointSpring)
    {
        joint.yDrive = new JointDrive
        {
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce
        };
    }
}
