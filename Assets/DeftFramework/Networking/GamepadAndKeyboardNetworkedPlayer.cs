//using UnityEngine;
//using System.Collections;

//public enum GamepadAndKeyboardPlayerPOV { FIRSTPERSON, THIRDPERSON, OVERSHOULDER, TOPDOWN };
//public enum GamepadAndKeyboardPlayerState { idle, aiming, walking, running, sprinting, jumping };

//public class GamepadAndKeyboardNetworkedPlayer : MonoBehaviour
//{


//    /// <summary>
//    /// PUBLICS
//    /// </summary>
//    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
//    public Vector3 camOffset = new Vector3(0.0f, 0.7f, -3.0f);
//    public float smooth = 10f;
//    public Vector3 aimPivotOffset = new Vector3(0.0f, 1.7f, -0.3f);
//    public Vector3 aimCamOffset = new Vector3(0.8f, 0.0f, -1.0f);
//    public float horizontalAimingSpeed = 400f;
//    public float verticalAimingSpeed = 400f;
//    public float maxVerticalAngle = 85f;
//    public float minVerticalAngle = -85f;

//    public float mouseSensitivity = 0.3f;

//    public float sprintFOV = 100f;

//    public float playerSpeedWhileAim = 1.0f;
//    public float playerSpeedWhileWalk = 2.0f;
//    public float playerSpeedWhileRun = 4.0f;
//    public float playerSpeedWhileSprint = 7.0f;
//    public float jumpHeight = 5.0f;
//    public float jumpCooldown = 1.0f;
//    public float smoothingTurn = 2.0f;
//    public float smoothingAim = 5.0f;

//    public float playerHeight;
//    public float playerWidth;

//    public bool debug;
//    public bool useGamepad = true;
//    public bool singlePlayer;

//    public bool isGrounded;
//    public PlayerState state;

//    public bool inverted = false;

//    Vector3 relCameraPos;
//    float relCameraPosMag;

//    Vector3 smoothPivotOffset;
//    Vector3 smoothCamOffset;
//    Vector3 targetPivotOffset;
//    Vector3 targetCamOffset;

//    float defaultFOV;
//    float targetFOV;
//    Vector3 angleH;
//    Vector3 angleV;


//    /// <summary>
//    /// Variables stored here for performance purposes and if anothere class would like to observe.
//    /// </summary>
//    public bool controllerJump;
//    public bool controllerRun;
//    public bool controllerSprint;
//    public float controllerAim;
//    public Vector2 controllerMoveDirection;
//    public Vector2 controllerLookDirection;
//    public Vector2 dpadDown;

//    void Awake()
//    {
//        relCameraPos = Camera.main.transform.position - this.gameObject.transform.position;
//        relCameraPosMag = relCameraPos.magnitude;
//        defaultFOV = Camera.main.camera.fieldOfView;
//    }

//    void UpdateCamera()
//    {
//        angleH += controllerLookDirection.x * horizontalAimingSpeed * Time.deltaTime;
//        angleV += controllerLookDirection.y * verticalAimingSpeed * Time.deltaTime;
//        angleV = Mathf.Clamp(angleV, minVerticalAngle, maxVerticalAngle);

//        Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
//        Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
//        cam.rotation = aimRotation;

//        if (player_controller.state == PlayerState.aiming)
//        {
//            targetPivotOffset = aimPivotOffset;
//            targetCamOffset = aimCamOffset;
//        }
//        else
//        {
//            targetPivotOffset = pivotOffset;
//            targetCamOffset = camOffset;
//        }

//        if (player_controller.state == PlayerState.sprinting)
//        {
//            targetFOV = sprintFOV;
//        }
//        else
//        {
//            targetFOV = defaultFOV;
//        }
//        cam.camera.fieldOfView = Mathf.Lerp(cam.camera.fieldOfView, targetFOV, Time.deltaTime);

//        Vector3 baseTempPosition = player.transform.position + camYRotation * targetPivotOffset;
//        Vector3 tempOffset = targetCamOffset;
//        for (float zOffset = targetCamOffset.z; zOffset < 0; zOffset += 0.5f)
//        {
//            tempOffset.z = zOffset;
//            if (DoubleViewingPosCheck(baseTempPosition + aimRotation * tempOffset))
//            {
//                targetCamOffset.z = tempOffset.z;
//                break;
//            }
//        }

//        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
//        smoothCamOffset = Vector3.Lerp(smoothCamOffset, targetCamOffset, smooth * Time.deltaTime);

//        cam.position = player.transform.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
//    }

//    void Update()
//    {

//    }

//    void FixedUpdate()
//    {

//    }
//}
