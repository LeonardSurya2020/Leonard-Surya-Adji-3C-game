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
    private float _crouchSpeed;
    
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


    [Header("Camera Parameters")]

    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private CameraManager _cameraManager;



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

    [Header("Glide Parameters")]

    [SerializeField]
    private float _glideSpeed;

    [SerializeField]
    private float _airDrag;

    [SerializeField]
    private Vector3 _glideRotationSpeed;

    [SerializeField]
    private float _minGlideRotationX;

    [SerializeField]
    private float _maxGlideRotationX;

    [SerializeField]
    private float _minGlideRotationZ;

    [SerializeField]
    private float _maxGlideRotationZ;



    [Header("Attack & Combo Parameters")]

    [SerializeField]
    private Transform _hitDetector;

    [SerializeField]
    private float _hitDetectorRadius;

    [SerializeField]
    private LayerMask _hitLayer;

    [SerializeField]
    private float _resetComboInterval;

    [Header("Audio Manager")]

    [SerializeField]
    private PlayerAudioManager _playerAudioManager;

    // non-SerializeField parameters
    private Vector2 _wallClimbAxisDirection;
    private float _rotationSmoothVelocity;
    private Rigidbody _rigidBody;
    private float _speed;
    private bool _isGrounded;
    private bool _canClimbJump;
    bool isInFrontOfClimbingWall;
    private PlayerStance _playerStance;
    private float _jumpCounter;
    private Animator _animator;
    private CapsuleCollider _collider;
    private bool _isPunching;
    private int _combo = 0;
    private Coroutine _resetCombo;
    private Vector3 rotationDegree = Vector3.zero;
    #endregion

    #region Main Function
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _speed = _walkSpeed;
        _canClimbJump = true;
        _playerStance = PlayerStance.Stand;
        HideAndLockCursor();
        _jumpCounter = 0;
    }
    private void Start()
    { 
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        CheckIsWallClimbing();
        Glide();

    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnPunchInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }
    #endregion

    #region Player Movement & Input Functions
    private void Move(Vector2 axisDirection)
    {


        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        if ((isPlayerStanding || isPlayerCrouch) && !_isPunching)
        {
            switch(_cameraManager.CameraState)
            {
                case CameraState.FirstPerson:

                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0);
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    movementDirection = horizontalDirection + verticalDirection;
                    _rigidBody.AddForce(movementDirection * _speed * Time.deltaTime);
                    break;

                case CameraState.ThirdPerson:

                    if (axisDirection.magnitude >= 0.1f)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward.normalized;
                        _rigidBody.AddForce(movementDirection * Time.deltaTime * _speed);
                    }
                    break;

                default:
                    break;
            }
            Vector3 velocity = new Vector3(_rigidBody.velocity.x, 0, _rigidBody.velocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
            _animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);


        }
        else if (isPlayerClimbing)
        {
            //movementDirection = new Vector3(axisDirection.x, axisDirection.y, 0).normalized;
            //_rigidBody.AddForce(movementDirection * _speed * Time.deltaTime);
            //_wallClimbAxisDirection = axisDirection;

            Vector3 horizontal = Vector3.zero;
            Vector3 vertical = Vector3.zero;

            Vector3 checkerLeftPosition = transform.position + (transform.up * 1) + (-transform.right * .82f);
            Vector3 checkerRightPosition = transform.position + (transform.up * 1) + (transform.right * 1.25f);
            Vector3 checkerUpPosition = transform.position + (transform.up * 2.15f);
            Vector3 checkerDownPosition = transform.position + (-transform.up * .25f);



            bool isAbleClimbLeft = Physics.Raycast(checkerLeftPosition, transform.forward, _climbCheckDistance, _climbableLayer);
            bool isAbleClimbRight = Physics.Raycast(checkerRightPosition, transform.forward, _climbCheckDistance, _climbableLayer);
            bool isAbleClimbUp = Physics.Raycast(checkerUpPosition, transform.forward, _climbCheckDistance, _climbableLayer);
            bool isAbleClimbDown = Physics.Raycast(checkerDownPosition, transform.forward, _climbCheckDistance, _climbableLayer);

            if((isAbleClimbLeft && (axisDirection.x < 0)) || (isAbleClimbRight && (axisDirection.x > 0)))
            {
                Debug.Log("nyot");
                horizontal = axisDirection.x * transform.right;
            }

            if ((isAbleClimbUp && (axisDirection.y > 0)) || (isAbleClimbDown && (axisDirection.y < 0)))
            {
                Debug.Log("nyet");
                vertical = axisDirection.y * transform.up;
            }

            movementDirection = horizontal + vertical;
            _rigidBody.AddForce(movementDirection * _climbSpeed * Time.deltaTime);

            Vector3 velocity = new Vector3(_rigidBody.velocity.x, _rigidBody.velocity.y, 0);
            _animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
            _animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);

        }
        else if(isPlayerGliding)
        {
            //Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x,_minGlideRotationX, _maxGlideRotationX);
            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
            rotationDegree.z = Mathf.Clamp(rotationDegree.z, _minGlideRotationZ, _maxGlideRotationZ);
            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);

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
        if (_isGrounded && !_isPunching && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Landing") && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
        {
            _rigidBody.AddForce(jumpDirection * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
            
        }
        else if (!_isGrounded && _playerStance == PlayerStance.Stand && _jumpCounter <= 0) // Fitur Double Jump
        {
            _jumpCounter++;
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.AddForce(jumpDirection * _jumpForce , ForceMode.Impulse);

        }
        else if (!_isGrounded && _playerStance == PlayerStance.Climb && _canClimbJump) // Fitur Jump Climb
        {
            JumpClimb();
        }


    }

    private void JumpClimb()
    {
        Vector3 jumpDirection = Vector3.up;
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
        StartCoroutine(JumpClimbCooldown());
    }

    // Fungsi untuk mengecek apakah player masih di dalam area wall climb, jika tidak akan menjalankan CancelClimb
    private void CheckIsWallClimbing()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward, out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        if(!isInFrontOfClimbingWall && _playerStance == PlayerStance.Climb)
        {
            //CancelClimb();
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
        if(_isGrounded )
        {
            _jumpCounter = 0;
            CancelGlide();
        }
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
            Vector3 climablePoint = hit.collider.bounds.ClosestPoint(transform.position);
            Vector3 direction = (climablePoint - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
            _collider.center = Vector3.up * 1.3f;
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidBody.useGravity = false;
            _speed = _climbSpeed;
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
            _animator.SetBool("IsClimbing", true);
        }
    }

    private void CancelClimb()
    {
        if(_playerStance == PlayerStance.Climb)
        {
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _rigidBody.useGravity = true;
            transform.position -= transform.forward;
            _speed = _walkSpeed;
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
            _animator.SetBool("IsClimbing", false);
        }
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("ChangePerspective");
    }

    private void Crouch()
    {
        Vector3 checkerUpPosition = transform.position + (transform.up * 1.4f);
        bool isCantStand = Physics.Raycast(checkerUpPosition, transform.up, out RaycastHit hit, 0.25f, _groundLayer);
        if(_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("IsCrouch", true);
            _speed = _crouchSpeed;
            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.66f;
        }
        else if(_playerStance == PlayerStance.Crouch && !isCantStand)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false); 
            _speed = _walkSpeed;
            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;
        }
    }
    
    private void Glide()
    {
        if(_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidBody.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void StartGlide()
    {
        if(_playerStance != PlayerStance.Glide && !_isGrounded)
        {
             rotationDegree = transform.rotation.eulerAngles;
            _playerStance = PlayerStance.Glide;
            _animator.SetBool("IsGliding", true);
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _playerAudioManager.PlayGlideSfx();
        }    
    }

    private void CancelGlide()
    {
        if(_playerStance == PlayerStance.Glide )
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsGliding", false);
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _playerAudioManager.StopGlideSfx();
        }
    }

    private void Punch()
    {
        if(!_isPunching && _playerStance == PlayerStance.Stand && _isGrounded)
        {
            _isPunching = true;
            if(_combo < 3)
            {
                _combo += 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetTrigger("Punch");
            _animator.SetInteger("Combo", _combo);
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if(_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
            }
        }
    }
    #endregion


    #region IEnumerator Functions
    // Jump climb cooldown
    private IEnumerator JumpClimbCooldown()
    {
        yield return new WaitForSeconds(1f);
        _canClimbJump = true;
    }

    private IEnumerator DoubleJumpCooldown()
    {
        yield return new WaitForSeconds(1f);

    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
    }
    #endregion
}
