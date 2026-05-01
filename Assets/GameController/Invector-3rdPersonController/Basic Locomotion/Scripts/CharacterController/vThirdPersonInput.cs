using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vCharacterController
{
    [vClassHeader("Input Manager", iconName = "inputIcon")]
    public class vThirdPersonInput : vMonoBehaviour, vIAnimatorMoveReceiver
    {
        public delegate void OnUpdateEvent();
        public event OnUpdateEvent onUpdate;
        public event OnUpdateEvent onLateUpdate;
        public event OnUpdateEvent onFixedUpdate;
        public event OnUpdateEvent onAnimatorMove;

        #region Variables        

        [vEditorToolbar("Inputs")]
        [vHelpBox("Check these options if you need to use the mouse cursor, ex: <b>2.5D, Topdown or Mobile</b>", vHelpBoxAttribute.MessageType.Info)]
        public bool unlockCursorOnStart = false;
        public bool showCursorOnStart = false;
        public bool MobileControl;
        public bool _lockJump = false;
        public bool LockedInput = false;

        public Joystick moveJoystick;
        public Joystick cameraJoystick;
        public Button JumpButtonUI;
        [HideInInspector] public event UnityAction OnJump;

        [vHelpBox("PC only - use it to toggle between run/walk", vHelpBoxAttribute.MessageType.Info)]
        public KeyCode toggleWalk = KeyCode.CapsLock;

        [Header("Movement Input")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
        public GenericInput verticalInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput crouchInput = new GenericInput("C", "Y", "Y");
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput jumpInput = new GenericInput("Space", "X", "X");
        public GenericInput rollInput = new GenericInput("Q", "B", "B");

        protected bool _lockInput = false;
        [HideInInspector] public virtual bool lockInput { get { return _lockInput; } set { _lockInput = value; } }

        [vEditorToolbar("Camera Settings")]
        public bool lockCameraInput;
        public bool invertCameraInputVertical, invertCameraInputHorizontal;
        [vEditorToolbar("Inputs")]
        [Header("Camera Input")]
        public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");
        public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");
        public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");

        [vEditorToolbar("Events")]
        public UnityEvent OnLockCamera;
        public UnityEvent OnUnlockCamera;
        public UnityEvent onEnableAnimatorMove = new UnityEvent();
        public UnityEvent onDisableDisableAnimatorMove = new UnityEvent();

        [HideInInspector]
        public vCamera.vThirdPersonCamera tpCamera;         // access tpCamera info
        [HideInInspector]
        public bool ignoreTpCamera;                         // controls whether update the cameraStates of not                
        [HideInInspector]
        public string customCameraState;                    // generic string to change the CameraState
        [HideInInspector]
        public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode
        [HideInInspector]
        public bool changeCameraState;                      // generic bool to change the CameraState
        [HideInInspector]
        public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp
        [HideInInspector]
        public vThirdPersonController cc;                   // access the ThirdPersonController component
        [HideInInspector]
        public vHUDController hud;                          // access vHUDController component
        protected bool updateIK = false;
        protected bool isInit;
        [HideInInspector] public bool lockMoveInput;
        protected InputDevice inputDevice { get { return vInput.instance.inputDevice; } }

        protected Camera _cameraMain;
        protected bool withoutMainCamera;
        public virtual bool lockUpdateMoveDirection { get; set; }                // lock the method UpdateMoveDirection


        public void LockInputExceptCamera(bool state)
        {
            LockedInput = state;
        }
        public virtual Camera cameraMain
        {
            get
            {
                if (!_cameraMain && !withoutMainCamera)
                {
                    if (!Camera.main)
                    {
                        Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                        withoutMainCamera = true;
                    }
                    else
                    {
                        _cameraMain = Camera.main;
                        if(!LockedInput)
                            cc.rotateTarget = _cameraMain.transform;
                    }
                }
                return _cameraMain;
            }
            set
            {
                _cameraMain = value;
            }
        }

        public Animator animator
        {
            get
            {
                if (cc == null)
                {
                    cc = GetComponent<vThirdPersonController>();
                }

                if (cc.animator == null)
                {
                    return GetComponent<Animator>();
                }

                return cc.animator;
            }
        }

        #endregion

        #region Initialize Character, Camera & HUD when LoadScene

        protected virtual void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            MobileControl = false;
#endif

            cc = GetComponent<vThirdPersonController>();
            //JumpButtonUI.onClick.AddListener(() => JumpButton());
            //JumpButtonTrigger.OnPointerDown() += JumpButton;

            if (cc != null)
            {
                cc.Init();
            }

            cc.onDead.AddListener((GameObject _gameObject) => { cc.ResetInputAnimatorParameters(); SetLockAllInput(true); cc.StopCharacter(); });
            StartCoroutine(CharacterInit());

            ShowCursor(showCursorOnStart);
            LockCursor(unlockCursorOnStart);
            EnableOnAnimatorMove();
        }

        protected virtual IEnumerator CharacterInit()
        {
            FindCamera();
            yield return new WaitForEndOfFrame();
            FindHUD();
        }

        public virtual void FindHUD()
        {
            if (hud == null && vHUDController.instance != null)
            {
                hud = vHUDController.instance;
                hud.Init(cc);
            }
        }

        public virtual void FindCamera()
        {
            var tpCameras = FindObjectsOfType<vCamera.vThirdPersonCamera>();

            if (tpCameras.Length > 1)
            {
                tpCamera = System.Array.Find(tpCameras, tp => !tp.isInit);

                if (tpCamera == null)
                {
                    tpCamera = tpCameras[0];
                }

                if (tpCamera != null)
                {
                    for (int i = 0; i < tpCameras.Length; i++)
                    {
                        if (tpCamera != tpCameras[i])
                        {
                            Destroy(tpCameras[i].gameObject);
                        }
                    }
                }
            }
            else if (tpCameras.Length == 1)
            {
                tpCamera = tpCameras[0];
            }

            if (tpCamera && tpCamera.mainTarget != transform)
            {
                tpCamera.SetMainTarget(this.transform);
            }
        }

        #endregion

        protected virtual void LateUpdate()
        {
            if (cc == null)
            {
                return;
            }

            if (!updateIK && !LockedInput)
            {
                return;
            }
            //Debug.Log("Camera input");

            if (onLateUpdate != null)
            {
                onLateUpdate.Invoke();
            }

            CameraInput();                      // update camera input
            UpdateCameraStates();               // update camera states                        
            updateIK = false;
        }

        protected virtual void FixedUpdate()
        {
            if (onFixedUpdate != null)
            {
                onFixedUpdate.Invoke();
            }

            if (LockedInput)
                return;
            Physics.SyncTransforms();
            cc.UpdateMotor();                                                   // handle the ThirdPersonMotor methods            
            cc.ControlLocomotionType();                                         // handle the controller locomotion type and movespeed   

            ControlRotation();
            cc.UpdateAnimator();                                                // handle the ThirdPersonAnimator methods
            updateIK = true;
        }

        protected virtual void Update()
        {
            if (cc == null || Time.timeScale == 0 || LockedInput)
            {
                return;
            }

            if (onUpdate != null)
            {
                onUpdate.Invoke();
            }

            InputHandle();                      // update input methods                        
            UpdateHUD();                        // update hud graphics            
        }

        public virtual void OnAnimatorMoveEvent()
        {
            if (cc == null)
            {
                return;
            }

            cc.ControlAnimatorRootMotion();
            if (onAnimatorMove != null)
            {
                onAnimatorMove.Invoke();
            }
        }

        #region Generic Methods
        // you can call this methods anywhere in the inspector or third party assets to have better control of the controller or cutscenes

        /// <summary>
        /// Lock all Basic  Input from the Player
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockBasicInput(bool value)
        {
            lockInput = value;
            if (value)
            {
                //cc.input = Vector2.zero;
                //cc.isSprinting = false;
                //cc.animator.SetFloat("InputHorizontal", 0, 0.25f, Time.deltaTime);
                //cc.animator.SetFloat("InputVertical", 0, 0.25f, Time.deltaTime);
                //cc.animator.SetFloat("InputMagnitude", 0, 0.25f, Time.deltaTime);
            }
        }

        /// <summary>
        /// Lock all Inputs 
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockAllInput(bool value)
        {
            SetLockBasicInput(value);
        }

        /// <summary>
        /// Show/Hide Cursor
        /// </summary>
        /// <param name="value"></param>
        public virtual void ShowCursor(bool value)
        {
            Cursor.visible = value;
        }

        /// <summary>
        /// Lock/Unlock the cursor to the center of screen
        /// </summary>
        /// <param name="value"></param>
        public virtual void LockCursor(bool value)
        {
            if (!value)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Lock the Camera Input
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockCameraInput(bool value)
        {
            lockCameraInput = value;

            if (lockCameraInput)
            {
                OnLockCamera.Invoke();
            }
            else
            {
                OnUnlockCamera.Invoke();
            }
        }

        /// <summary>
        /// If you're using the MoveCharacter method with a custom targetDirection, check this true to align the character with your custom targetDirection
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockUpdateMoveDirection(bool value)
        {
            lockUpdateMoveDirection = value;
        }

        /// <summary>
        /// Limits the character to walk only, useful for cutscenes and 'indoor' areas
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetWalkByDefault(bool value)
        {
            cc.freeSpeed.walkByDefault = value;
            cc.strafeSpeed.walkByDefault = value;
        }

        /// <summary>
        /// Reset the character to the default walk settings
        /// </summary>        
        public virtual void ResetWalkByDefault()
        {
            cc.freeSpeed.walkByDefault = cc.freeSpeed.defaultWalkByDefault;
            cc.strafeSpeed.walkByDefault = cc.strafeSpeed.defaultWalkByDefault;
        }

        /// <summary>
        /// Set the character to Strafe Locomotion
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetStrafeLocomotion(bool value)
        {
            cc.lockInStrafe = value;
            cc.isStrafing = value;
        }

        /// <summary>
        /// OnAnimatorMove Event Sender 
        /// </summary>
        public virtual vAnimatorMoveSender animatorMoveSender { get; set; }

        /// <summary>
        /// Use Animator Move Event Sender <seealso cref="vAnimatorMoveSender"/>
        /// </summary>
        protected bool _useAnimatorMove { get; set; }

        /// <summary>
        /// Check if OnAnimatorMove is Enabled
        /// </summary>
        public virtual bool UseAnimatorMove
        {
            get
            {
                return _useAnimatorMove;
            }
            set
            {

                if (_useAnimatorMove != value)
                {
                    if (value)
                    {
                        animatorMoveSender = gameObject.AddComponent<vAnimatorMoveSender>();
                        onEnableAnimatorMove?.Invoke();
                    }
                    else
                    {
                        if (animatorMoveSender)
                        {
                            Destroy(animatorMoveSender);
                        }

                        onEnableAnimatorMove?.Invoke();
                    }
                }
                _useAnimatorMove = value;
            }
        }

        /// <summary>
        /// Enable OnAnimatorMove event
        /// </summary>
        public virtual void EnableOnAnimatorMove()
        {
            UseAnimatorMove = true;
        }

        /// <summary>
        /// Disable OnAnimatorMove event
        /// </summary>
        public virtual void DisableOnAnimatorMove()
        {
            UseAnimatorMove = false;
        }

        #endregion

        #region Basic Locomotion Inputs

        public virtual void InputHandle()
        {
            if (lockInput || cc.ragdolled)
            {
                return;
            }

            MoveInput();
            SprintInput();
            CrouchInput();
            StrafeInput();
            JumpInput();
            RollInput();
        }

        public virtual void MoveInput()
        {
            if (!lockMoveInput)
            {
                // gets input
                var input = cc.input;
                if (!MobileControl)
                {
                    input.x = horizontalInput.GetAxisRaw();
                    input.z = verticalInput.GetAxisRaw();
                }
                else
                { 
                    input.x = moveJoystick.Horizontal;
                    input.z = moveJoystick.Vertical;
                }
                cc.input = input;
            }

            if (Input.GetKeyDown(toggleWalk))
            {
                cc.alwaysWalkByDefault = !cc.alwaysWalkByDefault;
            }

            cc.ControlKeepDirection();
        }

        public virtual bool rotateToLockTargetConditions => tpCamera && tpCamera.lockTarget && cc.isStrafing && !cc.isRolling && !cc.isJumping && !cc.customAction;
        public virtual void ControlRotation()
        {
            if (cameraMain && !lockUpdateMoveDirection)
            {
                if (!cc.keepDirection)
                {
                    cc.UpdateMoveDirection(cameraMain.transform);
                }
            }

            if (rotateToLockTargetConditions)
            {
                cc.RotateToPosition(tpCamera.lockTarget.position);          // rotate the character to a specific target
            }
            else
            {
                cc.ControlRotationType();                                   // handle the controller rotation type (strafe or free)
            }
        }

        public virtual void StrafeInput()
        {
            if (strafeInput.GetButtonDown())
            {
                cc.Strafe();
            }
        }

        public virtual void SprintInput()
        {
            if (sprintInput.useInput)
            {
                cc.Sprint(cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton());
            }
        }

        public virtual void CrouchInput()
        {
            cc.AutoCrouch();

            if (crouchInput.useInput && crouchInput.GetButtonDown())
            {
                cc.Crouch();
            }
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        public virtual bool JumpConditions()
        {
            return !cc.inJumpStarted && !cc.customAction && !cc.isCrouching && cc.isGrounded && !((int)cc.GroundAngle() > cc.slopeLimit) && cc.currentStamina >= cc.jumpStamina && !cc.isJumping && !cc.isRolling;
        }
        private bool _canDoubleJump;
        public virtual bool DoubleJumpConditions()
        {
            return _canDoubleJump && !cc.customAction;
        }

        public void ResetDoubleJump()
        {
            _canDoubleJump = true;
        }
        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        public virtual void JumpInput()
        {
            //Debug.Log("Is Jumping = " + cc.isJumping);
            if (jumpInput.GetButtonDown() && JumpConditions())
            {
                cc.Jump(true);
            }
        }

        public void LockJump(bool state)
        { 
            _lockJump = state;
        }
        public virtual void JumpButton()
        {
            if (_lockJump)
                return;
            Debug.Log("<color=orange>Jump: </color>Button Pressed");
            if (JumpConditions())
            {
                cc.Jump(true);
                _canDoubleJump = true;
                Debug.Log("<color=green>Jump: </color>Confirmed");
                OnJump?.Invoke();
            }
            else if(DoubleJumpConditions())
            {
                cc.DoubleJump(true);
                _canDoubleJump = false;
                OnJump?.Invoke();
            }
        }

        /// <summary>
        /// Conditions to trigger the Roll animation & behavior
        /// </summary>
        /// <returns></returns>
        public virtual bool RollConditions()
        {
            return (!cc.isRolling || cc.canRollAgain) && cc.isGrounded && cc.input != Vector3.zero && !cc.customAction && cc.currentStamina > cc.rollStamina && !cc.isJumping && !cc.isSliding;
        }

        /// <summary>
        /// Input to trigger the Roll
        /// </summary>
        public virtual void RollInput()
        {
            if (rollInput.GetButtonDown() && RollConditions())
            {
                cc.Roll();
            }
        }

        #endregion       

        #region Camera Methods

        private bool isTouchingRightSide = false;
        private int touchIndex = -1;

        [HideInInspector] public float cameraInputX, cameraInputY;
        [HideInInspector] public bool isMovingCamera;
        public virtual void CameraInput()
        {
            if (!cameraMain)
            {
                return;
            }

            /*if (tpCamera == null)
            {
                return;
            }*/

            /*if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var Y = lockCameraInput ? 0f : rotateCameraYInput.GetAxis();
                var X = lockCameraInput ? 0f : rotateCameraXInput.GetAxis();

                if (invertCameraInputHorizontal)
                {
                    X *= -1;
                }

                if (invertCameraInputVertical)
                {
                    Y *= -1;
                }

                var zoom = cameraZoomInput.GetAxis();

                tpCamera.RotateCamera(X, Y);
                if (!lockCameraInput)
                {
                    tpCamera.Zoom(zoom);
                }
            }*/

            if (MobileControl)
            {
                // Проверяем, что имеется хотя бы одно касание
                if (Input.touchCount > 0)
                {
                    // Проходим по всем активным касаниям
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);

                        // Если тач не используется для джойстика (не начался в левой части экрана)
                        if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2 && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            isTouchingRightSide = true;
                            touchIndex = i; // Сохраняем индекс тача
                        }

                        // Если началось касание в правой части экрана, обрабатываем его
                        if (isTouchingRightSide && touchIndex == i)
                        {
                            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                            {
                                var Y = lockCameraInput ? 0f : touch.deltaPosition.y;
                                var X = lockCameraInput ? 0f : touch.deltaPosition.x;

                                if (invertCameraInputHorizontal)
                                {
                                    X *= -1;
                                }

                                if (invertCameraInputVertical)
                                {
                                    Y *= -1;
                                }

                                if(tpCamera)
                                    tpCamera.RotateCamera(X, Y);

                                cameraInputX = X;
                                cameraInputY = Y;
                                isMovingCamera = true;

                                if (!lockCameraInput)
                                {
                                    /*// Zoom камеры через тач (например, pinch zoom)
                                    if (Input.touchCount == 2)
                                    {
                                        Touch touch1 = Input.GetTouch(1);
                                        float prevTouchDeltaMag = (touch1.position - touch1.deltaPosition).magnitude;
                                        float touchDeltaMag = (touch1.position - touch.deltaPosition).magnitude;
                                        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                                        tpCamera.Zoom(deltaMagnitudeDiff);
                                    }*/
                                }
                            }

                            // Завершаем обработку касания
                            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            {
                                isTouchingRightSide = false;
                                isMovingCamera = false;
                                touchIndex = -1; // Сбрасываем индекс тача
                            }
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    var Y = lockCameraInput ? 0f : rotateCameraYInput.GetAxis();
                    var X = lockCameraInput ? 0f : rotateCameraXInput.GetAxis();

                    if (invertCameraInputHorizontal)
                    {
                        X *= -1;
                    }

                    if (invertCameraInputVertical)
                    {
                        Y *= -1;
                    }

                    var zoom = cameraZoomInput.GetAxis();

                    if (tpCamera)
                        tpCamera.RotateCamera(X * 10, Y * 10);

                    cameraInputX = X;
                    cameraInputY = Y;
                    isMovingCamera = true;
                }
                else
                {
                    isMovingCamera = false;
                }
            }


        }

        public virtual void UpdateCameraStates()
        {

            /*// CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
            if (ignoreTpCamera)
            {
                return;
            }

            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vCamera.vThirdPersonCamera>();
                if (tpCamera == null)
                {
                    return;
                }

                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }

            if (changeCameraState)
            {
                tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
            }
            else if (cc.isCrouching)
            {
                tpCamera.ChangeState("Crouch", true);
            }
            else if (cc.isStrafing)
            {
                tpCamera.ChangeState("Strafing", true);
            }
            else
            {
                tpCamera.ChangeState("Default", true);
            }*/
        }

        public virtual void ChangeCameraState(string cameraState, bool useLerp = true)
        {
            if (useLerp)
            {
                ChangeCameraStateWithLerp(cameraState);
            }
            else
            {
                ChangeCameraStateNoLerp(cameraState);
            }
        }

        public virtual void ResetCameraAngleSmooth()
        {
            if (tpCamera)
            {
                tpCamera.ResetAngle();
            }
        }

        public virtual void ResetCameraAngleWithoutSmooth()
        {
            if (tpCamera)
            {
                tpCamera.ResetAngleWithoutSmooth();
            }
        }

        public virtual void ChangeCameraStateWithLerp(string cameraState)
        {
            changeCameraState = true;
            customCameraState = cameraState;
            smoothCameraState = true;
        }

        public virtual void ChangeCameraStateNoLerp(string cameraState)
        {
            changeCameraState = true;
            customCameraState = cameraState;
            smoothCameraState = false;
        }

        public virtual void ResetCameraState()
        {
            changeCameraState = false;
            customCameraState = string.Empty;
        }

        #endregion

        #region HUD       

        public virtual void UpdateHUD()
        {
            if (hud == null)
            {
                if (vHUDController.instance != null)
                {
                    hud = vHUDController.instance;
                    hud.Init(cc);
                }
                else
                {
                    return;
                }
            }

            hud.UpdateHUD(cc);
        }

        #endregion
    }

    /// <summary>
    /// Interface to receive events from <seealso cref="vAnimatorMoveSender"/>
    /// </summary>
    public interface vIAnimatorMoveReceiver
    {
        /// <summary>
        /// Check if Component is Enabled
        /// </summary>
        bool enabled { get; set; }
        /// <summary>
        /// Method Called from <seealso cref="vAnimatorMoveSender"/>
        /// </summary>
        void OnAnimatorMoveEvent();
    }

    /// <summary>
    /// OnAnimatorMove Event Sender 
    /// </summary>
    public class vAnimatorMoveSender : MonoBehaviour
    {
        protected virtual void Awake()
        {
            ///Hide in Inpector
            this.hideFlags = HideFlags.HideInInspector;
            vIAnimatorMoveReceiver[] animatorMoves = GetComponents<vIAnimatorMoveReceiver>();
            for (int i = 0; i < animatorMoves.Length; i++)
            {
                var receiver = animatorMoves[i];
                animatorMoveEvent += () =>
                {
                    if (receiver.enabled)
                    {
                        receiver.OnAnimatorMoveEvent();
                    }
                };
            }
        }

        /// <summary>
        /// AnimatorMove event called using  default unity OnAnimatorMove
        /// </summary>
        public System.Action animatorMoveEvent;

        protected virtual void OnAnimatorMove()
        {
            animatorMoveEvent?.Invoke();
        }
    }
}