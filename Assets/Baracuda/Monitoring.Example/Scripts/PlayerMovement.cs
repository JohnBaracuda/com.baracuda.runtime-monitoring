using System;
using Baracuda.Monitoring.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Baracuda.Monitoring.Example.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonitoredBehaviour
    {
        #region --- [INSPECTOR] ---

        [Header("Movement Settings")] 
        [SerializeField] private float movementSpeed = 16.5f;
        [FormerlySerializedAs("jumpSpeed")] [SerializeField] private float jumpForce = 8f;
        [SerializeField] [Range(0, 1f)] private float jumpBraceTime = .5f;
        [SerializeField] private float gravityForce = 20f;
        [SerializeField] private float lookSpeed = 2f;
        [SerializeField] private float airMovementSharpness = 1f;
        [SerializeField] private float inputSharpness = 10f;
        [SerializeField] private int jumps = 3;
        
        [Header("Dash")]
        [SerializeField] [Range(0, 100f)] private float dashForce = 22f;
        [SerializeField] [Range(0, 1f)] private float dashDuration = 0.03f;
        [SerializeField] private AnimationCurve dashCurve = default;
        [SerializeField] [Range(0, 10)] private int dashAmount = 3;
        [SerializeField] [Range(0, .5f)] private float minTimeBetweenDash = .2f;
        [SerializeField] [Range(0, 3f)] private float dashRechargeGrounded = 2f;
        [SerializeField] [Range(0, 3f)] private float dashRechargeAirborne = .5f;
        
        [Header("Ground Check")] 
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckRadius = .4f;

        [Header("Spawn")] 
        [SerializeField] private float killHeight = -50f;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        // Components & other references
        private CharacterController _characterController;
        private Transform _transform;
        private Camera _camera;
        private IPlayerInput _input;
        [Monitor] private int _jumpsLeft;
        
        // Input
        private Vector2 _processedInputDir;
        private float _rotationX = 0;
        
        // Movement
        private Vector3 _velocity = Vector3.zero;
        private bool _isJumping;
        private bool _isFalling;
        private float _lastJumpTime;
        
        // Dash
        [Monitor] [Format(Format = "0.0", FontSize = 16)]
        private float _dashEnergy;
        private bool _isDashing = false;
        private float _lastDashTime;
        private float _dashStartTime;

        // Ground check
        private GroundStatus _lastGroundCheck = GroundStatus.StableGround;
        private readonly Collider[] _raycastHits = new Collider[16];
        private static int _hitCount;
        private float _lastGroundedTime;
        
        #endregion

        #region --- [PROPERTIES] ---

        [Monitor]
        public Vector3 Velocity => _characterController.velocity;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SETUP] ---

        protected override void Awake()
        {
            // Calling base.Awake is important to register this object as a monitored target.
            base.Awake();
            _transform = transform;
            _input = GetComponent<IPlayerInput>();
            _characterController = GetComponent<CharacterController>();
            _camera = GetComponentInChildren<Camera>();

            _dashEnergy = dashAmount;
            _jumpsLeft = jumps;
        }


        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var time = Time.time;

            PerformCharacterMovement(deltaTime, time);
        }


        private void LateUpdate()
        {
            if (_transform.position.y <= killHeight)
            {
                _transform.position = Vector3.zero;
                _transform.rotation = Quaternion.identity;
                _camera.transform.localRotation = Quaternion.identity;
                _rotationX = 0;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [MOVEMENT] ---
        
        /// <summary>
        /// Quick and dirty method performing movement calculations.
        /// </summary>
        private void PerformCharacterMovement(float deltaTime, float time)
        {
            var groundCheck = GroundCheck();
            var isGrounded = groundCheck != GroundStatus.NoGround;

            var xInput = _input.Vertical;
            var yInput = _input.Horizontal;

            var rawInputDir = new Vector2(xInput, yInput);
            
            _processedInputDir = Vector2.Lerp(_processedInputDir,
                rawInputDir.normalized,
                deltaTime * inputSharpness);
            
            var yMotion = _velocity.y;
            var directionVector = _transform.forward * _processedInputDir.x + _transform.right * _processedInputDir.y;
            var movementVelocity = directionVector * movementSpeed;


            _velocity = groundCheck != GroundStatus.NoGround
                ? movementVelocity
                : Vector3.Lerp(_velocity, movementVelocity, deltaTime * airMovementSharpness);

            if (_input.JumpPressed && _jumpsLeft > 0 && time - _lastJumpTime > .4f)//!_isJumping && time - _lastGroundedTime < jumpBraceTime)
            {
                _jumpsLeft--;
                _isJumping = true;
                _velocity.y = jumpForce;
                _lastJumpTime = time;
            }
            else
            {
                _velocity.y = yMotion;
            }

            // Vertical movement including jump and gravity calculations
            if (groundCheck == GroundStatus.NoGround)
            {
                if (_lastGroundCheck != GroundStatus.NoGround && _velocity.y <= 0f)
                {
                    _velocity.y = 0f;
                }

                _velocity.y -= gravityForce * (_isFalling ? 1.5f : 1f) * deltaTime;
            }
            else if (time - _lastJumpTime > .1f)
            {
                _velocity.y = -15f;
            }

            _characterController.Move(_velocity * deltaTime);
            
            
            // Dash logic
            if (CanDash(time, groundCheck, rawInputDir))
            {
                BeginDash(time);
            }
            if (_isDashing)
            {
                PerformDash(deltaTime, time, directionVector);
            }
            
            // Camera rotation
            _rotationX += -_input.MouseY * lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
            _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, _input.MouseX * lookSpeed, 0);

            
            // other...
            if (_lastGroundCheck == GroundStatus.NoGround && groundCheck != GroundStatus.NoGround)
            {
                _isJumping = false;
                _jumpsLeft = jumps;
            }

            _dashEnergy = Mathf.Clamp(
                _dashEnergy + 
                (isGrounded? dashRechargeGrounded : dashRechargeAirborne) 
                * deltaTime, 0, dashAmount);

            _isFalling = _isJumping && _velocity.y < 0f;
            _lastGroundCheck = groundCheck;
            _lastGroundedTime = groundCheck != GroundStatus.NoGround ? time : _lastGroundedTime;
        }

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DASH] ---
        
        private bool CanDash(float time, GroundStatus groundCheck, Vector2 rawInputDir)
        {
            return _input.DashPressed
                   && !_isDashing 
                   && time - _lastDashTime > minTimeBetweenDash 
                   && _dashEnergy >= 1
                   && groundCheck == GroundStatus.NoGround
                   && rawInputDir.normalized.magnitude > .5f;
        }
        
        private void BeginDash(float time)
        {
            _dashEnergy -= 1;
            _lastDashTime = time;
            _isDashing = true;
            _dashStartTime = time;
        }
        
        private void PerformDash(float deltaTime, float time, Vector3 directionVector)
        {
            var currentDashTimer = time - _dashStartTime;
            var progression = (time - _dashStartTime) / dashDuration;
            
            var dashVector = directionVector *
                             dashForce *
                             dashCurve.Evaluate(progression) *
                             deltaTime;

            _characterController.Move(dashVector);

            _velocity.y = Mathf.Clamp(_velocity.y, 2f, float.PositiveInfinity);
            if (currentDashTimer > dashDuration)
            {
                _isDashing = false;
            }
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [GROUND CHECK] ---

        private GroundStatus GroundCheck()
        {
            var position = _transform.position;
            var upDirection = _transform.up;

            _hitCount = Physics.OverlapSphereNonAlloc(position, groundCheckRadius, _raycastHits, groundMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < _hitCount; i++)
            {
                if (Vector3.Dot(_raycastHits[i].transform.up, upDirection) >= .95f)
                {
                    return GroundStatus.StableGround;
                }
            }

            return _hitCount > 0 ? GroundStatus.UnstableGround : GroundStatus.NoGround;
        }

        #endregion
    }
}