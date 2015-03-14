using UnityEngine;
using System.Collections;

public enum GamepadAndKeyboardPlayerPOV { FIRSTPERSON, THIRDPERSON, OVERSHOULDER, TOPDOWN };
public enum GamepadAndKeyboardPlayerState { idle, aiming, walking, running, sprinting, jumping };

public class GamepadAndKeyboardNetworkedPlayer : MonoBehaviour
{


    /// <summary>
    /// PUBLICS
    /// </summary>
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.0f, 0.7f, -3.0f);
    public float smooth = 10f;
    public Vector3 aimPivotOffset = new Vector3(0.0f, 1.7f, -0.3f);
    public Vector3 aimCamOffset = new Vector3(0.8f, 0.0f, -1.0f);
    public float horizontalAimingSpeed = 400f;
    public float verticalAimingSpeed = 400f;
    public float maxVerticalAngle = 30f;
    public float flyMaxVerticalAngle = 60f;
    public float minVerticalAngle = -60f;

    public float mouseSensitivity = 0.3f;

    public float sprintFOV = 100f;

    public float playerSpeedWhileAim = 1.0f;
    public float playerSpeedWhileWalk = 2.0f;
    public float playerSpeedWhileRun = 4.0f;
    public float playerSpeedWhileSprint = 7.0f;
    public float jumpHeight = 5.0f;
    public float jumpCooldown = 1.0f;
    public float smoothingTurn = 2.0f;
    public float smoothingAim = 5.0f;

    public float playerHeight;
    public float playerWidth;

    public bool debug;
    public bool useGamepad = true;
    public bool singlePlayer;

    public bool isGrounded;
    public PlayerState state;

    public bool inverted = false;

    /// <summary>
    /// PRIVATES
    /// </summary>
    float angleH = 0;
    float angleV = 0;

    Vector3 relCameraPos;
    float relCameraPosMag;

    Vector3 smoothPivotOffset;
    Vector3 smoothCamOffset;
    Vector3 targetPivotOffset;
    Vector3 targetCamOffset;

    float defaultFOV;
    float targetFOV;

    void Awake()
    {
        relCameraPos = Camera.main.transform.position - this.gameObject.transform.position;
        relCameraPosMag = relCameraPos.magnitude;
        defaultFOV = Camera.main.camera.fieldOfView;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }
}
