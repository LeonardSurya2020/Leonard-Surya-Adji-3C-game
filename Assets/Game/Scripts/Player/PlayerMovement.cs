using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Parameters
    // SerializeField parameters
    [Header("Player Movement")]
    [SerializeField] 
    private float _walkSpeed;
    
    [SerializeField] 
    private InputManager _input;
    
    [SerializeField] 
    private float _rotationSmoothTime = 0.1f;
    
    [SerializeField] 
    private float _sprintSpeed;
    
    [SerializeField] 
    private float _walkSpeedTransition;
    
    [SerializeField] 
    private float _jumpForce;
    
    [SerializeField] 
    private float _wallClimbJumpForce;
    
    [SerializeField] 
    private float _climbSpeed;

    [Header("Ground & Ladder Detector")]
    
    [SerializeField] 
    private Transform _groundDetector;
    
    [SerializeField] 
    private float _detectorRadius;
    
    [SerializeField] 
    private LayerMask _groundLayer;
    
    [SerializeField] 
    private Vector3 _upperStepOffset;
    
    [SerializeField] 
    private float _stepCheckerDistance;
    
    [SerializeField] 
    private float _stepForce;

    [Header("Climb Detector")]
    [SerializeField] 
    private Transform _climbDetector;
    
    [SerializeField]
    private float _climbCheckDistance;
    
    [SerializeField] 
    private LayerMask _climbableLayer;
    
    [SerializeField] 
    private Vector3 _climbOffset;

    // non-SerializeField parameters
    private Vector2 _wallClimbAxisDirection;
    private float _rotationSmoothVelocity;
    private Rigidbody _rigidBody;
    private float _speed;
    private bool _isGrounded;
    private bool _canClimbJump;
    bool isInFrontOfClimbingWall;
    private PlayerStance _playerStance;
    #endregion

    #region Main Function
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
        _canClimbJump = true;
        _playerStance = PlayerStance.Stand;
    }
    private void Start()
    { 
        _input.onMoveInput += Move;
        _input.onSprintInput += Sprint;
        _input.onJumpInput += Jump;
        _input.onClimbInput += StartClimb;
        _input.onCancelClimb += CancelClimb;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        CheckIsWallClimbing();
    }


    private void OnDestroy()
    {
        _input.onMoveInput -= Move;
        _input.onSprintInput -= Sprint;
        _input.onJumpInput -= Jump;
        _input.onClimbInput -= StartClimb;
        _input.onCancelClimb -= CancelClimb;
    }
    #endregion

    #region Player Movement & Input Functions
    private void Move(Vector2 axisDirection)
    {


        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;

        if (isPlayerStanding)
        {
            if (axisDirection.magnitude >= 0.1f)
            {
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward.normalized;
                _rigidBody.AddForce(movementDirection * Time.deltaTime * _speed);
            }
        }
        else if (isPlayerClimbing)
        {
            movementDirection = new Vector3(axisDirection.x, axisDirection.y, 0).normalized;
            _rigidBody.AddForce(movementDirection * _speed * Time.deltaTime);
            _wallClimbAxisDirection = axisDirection;

        }


    }

    private void Sprint(bool isSprint)
    {
        if(isSprint)
        {
            if(_speed < _sprintSpeed)
            {
                _speed = _speed + _walkSpeedTransition * Time.deltaTime;
            }
        }
        else
        {
            if (_speed > _walkSpeed)
            {
                _speed = _speed - _walkSpeedTransition * Time.deltaTime;
            }  
        }
    }

    private void Jump()
    {
        Vector3 jumpDirection = Vector3.up;
        if (_isGrounded)
        {
            _rigidBody.AddForce(jumpDirection * _jumpForce * Time.deltaTime);
        }
        else if (!_isGrounded && _playerStance == PlayerStance.Climb && _canClimbJump) // Fungsi untuk Jump saat Climbing
        {
            _canClimbJump = false;
            if (_wallClimbAxisDirection.magnitude >= 0.1)
            {
                Vector3 wallJumpDirection = new Vector3(_wallClimbAxisDirection.x, _wallClimbAxisDirection.y, 0);
                _rigidBody.AddForce(wallJumpDirection * _wallClimbJumpForce * Time.deltaTime);
            }
            else
            {
                _rigidBody.AddForce(jumpDirection * _wallClimbJumpForce * Time.deltaTime);
            }
            StartCoroutine(ClimbJumpCooldown());
        }

    }

    // Fungsi untuk mengecek apakah player masih di dalam area wall climb, jika tidak akan menjalankan CancelClimb
    private void CheckIsWallClimbing()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        if(!isInFrontOfClimbingWall && _playerStance == PlayerStance.Climb)
        {
            CancelClimb();
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);

        if(isHitLowerStep && !isHitUpperStep)
        {
            _rigidBody.AddForce(0, _stepForce * Time.deltaTime, 0);
        }
    }


    private void StartClimb()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;

        if(isInFrontOfClimbingWall && _isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidBody.useGravity = false;
            _speed = _climbSpeed;
        }
    }

    private void CancelClimb()
    {
        if(_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidBody.useGravity = true;
            transform.position -= transform.forward;
            _speed = _walkSpeed;
        }
    }
    #endregion


    #region IEnumerator Functions
    // Climb Jump Cooldown
    private IEnumerator ClimbJumpCooldown()
    {
        yield return new WaitForSeconds(1f);
        _canClimbJump = true;
    }
    #endregion


}
