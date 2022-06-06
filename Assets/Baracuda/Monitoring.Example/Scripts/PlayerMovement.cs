// Copyright (c) 2022 Jonathan Lang
using System.Collections;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonitoredBehaviour
    {
        [Monitor]
        private Vector3 Position => transform.position;
        
        #region --- Inspector ---
        
        [Header("Movement Settings")] 
        [SerializeField] private float movementSpeed = 16.5f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravityForce = 20f;
        [SerializeField] private float mouseSensitivity = 2f;
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
        
        #region --- Fields ---

        // Components & other references
        private CharacterController _characterController;
        private Transform _transform;
        private Camera _camera;
        private IPlayerInput _input;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        
        // Input
        private Vector2 _processedInputDir;
        private float _rotationX = 0;
        
        // Movement
        private Vector3 _velocity = Vector3.zero;
        [Monitor]
        private bool _isJumping;
        [Monitor]
        private bool _isFalling;
        private float _lastJumpTime;
        [Monitor]
        private int _jumpsLeft;
        
        // Dash
        [Monitor]
        private float _dashEnergy;
        [Monitor]
        private bool _isDashing = false;
        private float _lastDashTime;
        private float _dashStartTime;

        // Ground check
        private GroundStatus _lastGroundCheck = GroundStatus.StableGround;
        private static int hitCount;
        private float _lastGroundedTime;
        private readonly Collider[] _raycastHits = new Collider[4];
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

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

        private IEnumerator Start()
        {
            yield return null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            var self = transform;
            _spawnPosition = self.position;
            _spawnRotation = self.rotation;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Update ---

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
                _transform.position = _spawnPosition;
                _transform.rotation = _spawnRotation;
                _camera.transform.localRotation = Quaternion.identity;
                _rotationX = 0;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Movement ---
        
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
            _rotationX += -_input.MouseY * _camera.fieldOfView * mouseSensitivity  * .001f;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
            _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            
            var yRotation = _input.MouseX * _camera.fieldOfView * mouseSensitivity * .001f;
            transform.rotation *= Quaternion.Euler(0, yRotation, 0);
            
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

        #region --- Dash ---
        
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

        #region --- Ground Check ---

        private GroundStatus GroundCheck()
        {
            var position = _transform.position;
            var upDirection = _transform.up;

            hitCount = Physics.OverlapSphereNonAlloc(position, groundCheckRadius, _raycastHits, groundMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hitCount; i++)
            {
                if (Vector3.Dot(_raycastHits[i].transform.up, upDirection) >= .95f)
                {
                    return GroundStatus.StableGround;
                }
            }

            return hitCount > 0 ? GroundStatus.UnstableGround : GroundStatus.NoGround;
        }

        #endregion
    }
}