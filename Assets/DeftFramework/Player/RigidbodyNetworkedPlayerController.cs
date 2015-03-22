using UnityEngine;
using System.Collections;
using GamepadInput;

public enum PlayerControllerState { IDLE, WALKING, RUNNING, AIMING, ZOOM_OUT };
public enum MovementType { IMPULSE, SIMPLEWALK, SETVELOCITY, FORCE, THRUSTERS };

public class RigidbodyNetworkedPlayerController : MonoBehaviour
{

  public PlayerControllerState playerState;
  public MovementType movementType;
  public string name;
  public bool inverted;

  
  public float baseSpeed = 0.45f;
  //public float runSpeedMultiplier = 1.3f;
  public float aimSpeedMultiplier = 0.2f;

  [HideInInspector]
  public float horizontalAimingSpeed = 175f;
  [HideInInspector]
  public float verticalAimingSpeed = 175f;
  [HideInInspector]
  public float velocityDampingSpeed = 0.02f;
  [HideInInspector]
  public float impulseDampingSpeed = 0.02f;

  public float jumpHeight = 4.0f;
  public float jumpCooldown = 2.0f;
  float jumpCooldownTmp;

  public bool grounded;

  [HideInInspector]
  public float relCameraPosMag = 1.5f;
  public Vector3 pivotOffset = new Vector3(1.0f, 0.0f, -1.0f);
  public Vector3 camOffset = new Vector3(1.0f, 3.5f, -7f);
  public Vector3 aimPivotOffset = new Vector3(1.0f, 0.0f, 0.0f);
  public Vector3 aimCamOffset = new Vector3(1f, 3f, -5.0f);
  public Vector3 runPivotOffset = new Vector3(0f, 0.0f, 0.0f);
  public Vector3 runCamOffset = new Vector3(1f, 3.5f, -8f);
  public float runFOV = 80f;
  public float aimFOV = 40f;
  public float FOV = 60f;

  public float smoothingTurn = 5.0f;
  public float smoothingAim = 10.0f;

  public Vector2 controllerMoveDirection;
  public Vector2 controllerLookDirection;
  public float exponentialControllerJoystickModifier = 5.0f;
  Vector3 moveDirection;
  Vector3 lastInput;

  public bool debug = true;

  public GamePad.Index padIndex;
  [HideInInspector]
  public GamepadState gamepadState;

  [HideInInspector]
  public bool isThisMachinesPlayer = false;
  public bool useGamePad = true;

  [HideInInspector]
  public Camera myCamera;
  Vector3 smoothPivotOffset;
  Vector3 smoothCamOffset;
  Vector3 targetPivotOffset;
  Vector3 targetCamOffset;
  float defaultFOV;
  float targetFOV;

  [HideInInspector]
  public bool showCrosshair = false;
  public Texture2D verticalTexture;
  public Texture2D horizontalTexture;

  public float fullSyncRate = 1.0f;
  float fullSyncRateTmp;
  GameManager gameManager;

  
  public bool controllableHittingFloor = false;

  void Awake()
  {
	 if(GameObject.Find ("GameManager")){
			gameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();
		}
	
    if (debug)
    {
      Debug.Log(this.ToString() + " awake.");
    }
    foreach (NetworkView view in this.GetComponents<NetworkView>())
    {
      view.observed = this;
    }
    if (Network.isClient || Network.isServer)
    {
      if (this.GetComponent<NetworkView>().isMine)
      {
        this.isThisMachinesPlayer = true;
      }
    }
    else
    {
      this.isThisMachinesPlayer = true;
    }
    if (this.isThisMachinesPlayer)
    {
      if (debug)
      {
        Debug.Log("This machine owns player " + this.ToString());
      }
      GrabCamera(Camera.main);
    }
	this.gamepadState = GamePad.GetState(this.padIndex);
	inverted = false;
  }

  #region Networking
  DeftBodyState goalState;
  float syncTime;

  [RPC]

  public void UpdateFullPlayerState(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float health, NetworkViewID id)
  {
    if (debug)
    {
      Debug.Log("Performing full update on " + this.name);
    }
    if (this.networkView.viewID == id)
    {
      this.GetComponent<Rigidbody>().position = position;
      this.GetComponent<Rigidbody>().rotation = rotation;
      this.GetComponent<Rigidbody>().velocity = velocity;
      this.GetComponent<Rigidbody>().angularVelocity = angularVelocity;
      this.GetComponent<PlayerFields>().health = health;
    }
  }

  [RPC]
  public void UpdatePartialPlayerState(Vector3 position, Quaternion rotation, Vector3 velocity, NetworkViewID id)
  {
    if (debug)
    {
      Debug.Log("Performing partial update on " + this.name);
    }
    if (this.networkView.viewID == id)
    {
      this.GetComponent<Rigidbody>().position = position;
      this.GetComponent<Rigidbody>().rotation = rotation;
      this.GetComponent<Rigidbody>().velocity = velocity;
    }
  }
  #endregion

  #region CameraCalculators

  void GrabCamera(Camera cam)
  {
    this.myCamera = cam;
    smoothPivotOffset = pivotOffset;
    smoothCamOffset = camOffset;
    Debug.Log("Camera has been reset by player");
    if (debug)
    {
      Debug.Log("Player grabbed camera " + cam.ToString());
    }
  }

  float angleH;
  float angleV;
  public float maxVerticalAngle = 60f;
  public float minVerticalAngle = -40f;
  float maxVertAngle_temp = 60f;
  float minVertAngle_temp = -40f;
  float horizAimingSpeed_temp;
  float vertAimingSpeed_temp;

  void AdjustCamera()
  {
	if (this.playerState == PlayerControllerState.AIMING) {
		if(gameObject.name.Contains ("Syphen")){
			this.horizAimingSpeed_temp = 60f;
			this.vertAimingSpeed_temp = 60f;
		} else{
			this.horizAimingSpeed_temp = 100f;
			this.vertAimingSpeed_temp = 100f;
		}
	} else{
			this.horizAimingSpeed_temp = this.horizontalAimingSpeed; // defaults
			this.vertAimingSpeed_temp = this.verticalAimingSpeed;
	}
		angleH += this.controllerLookDirection.x * this.horizAimingSpeed_temp * Time.deltaTime;
		angleV += this.controllerLookDirection.y * this.vertAimingSpeed_temp * Time.deltaTime;
	angleV = Mathf.Clamp(angleV, minVertAngle_temp, maxVertAngle_temp);
    Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
    if (this.GetComponent<Rigidbody>().velocity.magnitude > 0.2f)
    {
      aimRotation = Quaternion.Slerp(aimRotation, Quaternion.Euler(this.transform.forward), this.velocityDampingSpeed);
    }
    Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
    this.myCamera.transform.rotation = aimRotation;
    if (this.playerState == PlayerControllerState.AIMING)
    {
      targetPivotOffset = aimPivotOffset;
      targetCamOffset = aimCamOffset;
      targetFOV = aimFOV;
	} else if (this.playerState == PlayerControllerState.ZOOM_OUT)
	{
		targetPivotOffset = new Vector3(0,0,-4);
		targetCamOffset = new Vector3(0,2.5f,-7);
		targetFOV = aimFOV;
		maxVertAngle_temp = 10;
		maxVertAngle_temp = -10;
	}
    /*else if (this.playerState == PlayerControllerState.RUNNING)
    {
      targetPivotOffset = runPivotOffset;
      targetCamOffset = runCamOffset;
      targetFOV = runFOV;
    }*/
    else
    {
      targetPivotOffset = pivotOffset;
      targetCamOffset = camOffset;
      targetFOV = FOV;
    }
	
	if (playerState != PlayerControllerState.ZOOM_OUT) {
			maxVertAngle_temp = maxVerticalAngle;
			minVertAngle_temp = minVerticalAngle;
	}
    this.myCamera.fieldOfView = Mathf.Lerp(this.myCamera.fieldOfView, targetFOV, Time.deltaTime);

    #region Collisions
    Vector3 baseTempPosition = this.transform.position + camYRotation * targetPivotOffset;
    Vector3 tempOffset = targetCamOffset;
    for (float zOffset = targetCamOffset.z; zOffset < 0; zOffset += 0.5f)
    {
      tempOffset.z = zOffset;
      if (DoubleViewingPosCheck(baseTempPosition + aimRotation * tempOffset))
      {
        targetCamOffset.z = tempOffset.z;
        break;
      }
    }
    #endregion

    smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, 10f * Time.deltaTime);
    smoothCamOffset = Vector3.Lerp(smoothCamOffset, targetCamOffset, 10f * Time.deltaTime);

    this.myCamera.transform.position = this.transform.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
  }

  bool DoubleViewingPosCheck(Vector3 checkPos)
  {
    return ViewingPosCheck(checkPos) && ReverseViewingPosCheck(checkPos);
  }

  bool ViewingPosCheck(Vector3 checkPos)
  {
    RaycastHit hit;
    if (Physics.Raycast(checkPos, this.transform.position - checkPos, out hit, relCameraPosMag))
    {
      if (hit.transform != this && !hit.transform.GetComponent<Collider>().isTrigger)
      {
        return false;
      }
    }
    return true;
  }

  bool ReverseViewingPosCheck(Vector3 checkPos)
  {
    RaycastHit hit;
    if (Physics.Raycast(this.transform.position, checkPos - this.transform.position, out hit, relCameraPosMag))
    {
      if (hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
      {
        return false;
      }
    }
    return true;
  }
  #endregion

  #region GUI
  Texture2D temp;
  [HideInInspector]
  public float crosshairSpread;
  [HideInInspector]
  public float minCrosshairSpread = 20.0f;
  [HideInInspector]
  public float maxCrosshairSpread = 40.0f;
  void DrawCrossHair()
  {
    GUIStyle verticalT = new GUIStyle();
    GUIStyle horizontalT = new GUIStyle();
    verticalT.normal.background = verticalTexture;
    horizontalT.normal.background = horizontalTexture;
    crosshairSpread = Mathf.Clamp(crosshairSpread, minCrosshairSpread, maxCrosshairSpread);
    Vector2 pivot = new Vector2(Screen.width / 2, Screen.height / 2);
    GUI.Box(new Rect((Screen.width - 2) / 2, (Screen.height - crosshairSpread) / 2 - 14, 2, 14), temp, horizontalT);
    GUIUtility.RotateAroundPivot(45, pivot);
    GUI.Box(new Rect((Screen.width + crosshairSpread) / 2, (Screen.height - 2) / 2, 14, 2), temp, verticalT);
    GUIUtility.RotateAroundPivot(0, pivot);
    GUI.Box(new Rect((Screen.width - 2) / 2, (Screen.height + crosshairSpread) / 2, 2, 14), temp, horizontalT);
  }

  void OnGUI()
  {
    if (this.playerState == PlayerControllerState.AIMING && this.isThisMachinesPlayer)
    {
      this.DrawCrossHair();
    }
  }
  #endregion
	
  void Update()
  {

    #region TimerMaintenance
    this.jumpCooldownTmp -= Time.deltaTime;
    this.fullSyncRateTmp -= Time.deltaTime;
    #endregion

	if (this.isThisMachinesPlayer)
	{
    #region GatherInput
      if (this.useGamePad)
      {
        this.gamepadState = GamePad.GetState(this.padIndex);
        this.controllerMoveDirection = GamePad.GetAxis(GamePad.Axis.LeftStick, this.padIndex);
        this.controllerLookDirection = GamePad.GetAxis(GamePad.Axis.RightStick, this.padIndex);
        this.controllerMoveDirection.y = Mathf.Pow(this.controllerMoveDirection.y, this.exponentialControllerJoystickModifier);
        this.controllerMoveDirection.x = Mathf.Pow(this.controllerMoveDirection.x, this.exponentialControllerJoystickModifier);
		
		if(controllableHittingFloor && this.controllerLookDirection.y < 0){
			this.controllerLookDirection.y = 0;
		} else{
			this.controllerLookDirection.y = Mathf.Pow(this.controllerLookDirection.y, this.exponentialControllerJoystickModifier);
		}
        this.controllerLookDirection.x = Mathf.Pow(this.controllerLookDirection.x, this.exponentialControllerJoystickModifier);
      }
      else
      {
        Debug.Log("Gathering keyboard.");
        this.controllerMoveDirection = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
        this.controllerLookDirection = new Vector2(Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1), Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1));
      }
    #endregion

    #region SettingPlayerState
    if (this.gamepadState.LeftTrigger > 0.20f){
			if(gameManager == null || gameManager.longRangeUnlocked)
		    {
		      this.playerState = PlayerControllerState.AIMING;
		    }
	}
	else if(this.gamepadState.RightTrigger > 0.20f && gameObject.name.Contains ("Syphen")){
		this.playerState = PlayerControllerState.ZOOM_OUT;
	}
   /* else if (this.gamepadState.LeftStick)
    {
      this.playerState = PlayerControllerState.RUNNING;
    }*/
    else
    {
      this.playerState = PlayerControllerState.WALKING;
    }
    #endregion

    #region RunningActionByState
    Vector3 forward = this.myCamera.transform.TransformDirection(Vector3.forward);
    forward = forward.normalized;
			this.moveDirection = this.controllerMoveDirection.y * new Vector3(forward.x, 0, forward.z) + this.controllerMoveDirection.x * new Vector3(forward.z, 0, -forward.x);
		if (this.moveDirection.x != 0 || this.moveDirection.z != 0)
		{
			lastInput = moveDirection;
		}
    moveDirection *= this.baseSpeed;
    switch (this.playerState)
    {
      case PlayerControllerState.WALKING:
        break;
     /* case PlayerControllerState.RUNNING:
        	moveDirection *= this.runSpeedMultiplier;
        break;*/
      case PlayerControllerState.AIMING:
        moveDirection *= this.aimSpeedMultiplier;
        break;
    }
    #endregion
		if (inverted){
			this.controllerLookDirection.y *= -1;
		}
	}
  }

  void FixedUpdate()
  {

    #region Camera
	if (this.isThisMachinesPlayer){
    	AdjustCamera();
	}
    #endregion

    #region Movement
    this.grounded = this.isGrounded();
    if (this.moveDirection.magnitude > 0.05f)
    {
      switch (this.movementType)
      {
        case MovementType.SIMPLEWALK:
          this.moveDirection.y = 0f;
          float yVelocityTmp = this.GetComponent<Rigidbody>().velocity.y;
				this.GetComponent<Rigidbody>().velocity = this.moveDirection * this.baseSpeed * this.GetComponent<Rigidbody>().mass + new Vector3(0,yVelocityTmp,0);
		
		  // smooth turning
		  Vector3 last_input_without_y = new Vector3(lastInput.x, 0, lastInput.z);
		  Vector3 forward_without_y = new Vector3(transform.forward.x, 0, transform.forward.z);
		  this.transform.forward = Vector3.Lerp(forward_without_y, last_input_without_y, 20f * Time.deltaTime);
         //this.transform.forward = this.moveDirection.normalized;
		  
		  Vector3 angularVelocity = this.GetComponent<Rigidbody>().angularVelocity;
          this.GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(angularVelocity, new Vector3(0,angularVelocity.y,0), 3.0f * Time.deltaTime);
          
		/* Commenting out in case we want this again later; THIS CODE CAUSES FLYING BTW
		  if (this.playerState == PlayerControllerState.RUNNING)
          {
            this.GetComponent<Rigidbody>().velocity *= this.runSpeedMultiplier; // tweaking the velocity vector is a dangerous game
            // ^try to implement this without touching the y component
          }
		*/
          if (this.playerState == PlayerControllerState.AIMING)
          {
            this.GetComponent<Rigidbody>().velocity *= this.aimSpeedMultiplier;
          }
          if (!grounded){
            this.GetComponent<Rigidbody>().velocity += Physics.gravity;
          }
          break;
        case MovementType.IMPULSE:
          Debug.Log(this.moveDirection * this.GetComponent<Rigidbody>().mass * this.impulseDampingSpeed);
          this.GetComponent<Rigidbody>().AddForce(this.moveDirection * this.GetComponent<Rigidbody>().mass * this.impulseDampingSpeed, ForceMode.Impulse);
          this.transform.forward = Vector3.Lerp(this.transform.forward, this.moveDirection, this.velocityDampingSpeed * Time.deltaTime);
		
		  angularVelocity = this.GetComponent<Rigidbody>().angularVelocity;
		  this.GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(angularVelocity, new Vector3(0,angularVelocity.y,0), this.velocityDampingSpeed * Time.deltaTime);
          //this.GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(this.GetComponent<Rigidbody>().angularVelocity, Vector3.zero, this.velocityDampingSpeed * Time.deltaTime);
          break;
        case MovementType.THRUSTERS:
          this.GetComponent<Thrusters>().ActivatePrimaryMovementThrusters(this.moveDirection);
          break;
      }

    }
    #endregion

    #region NetworkUpdate
    if (this.isThisMachinesPlayer)
    {
      Rigidbody rigidbody = this.GetComponent<Rigidbody>();
      PlayerFields fields = this.GetComponent<PlayerFields>();
      if (Network.isServer || Network.isClient)
      {
        if (this.isThisMachinesPlayer)
        {
          if (this.fullSyncRateTmp <= 0.0f)
          {
            this.networkView.RPC("UpdateFullPlayerState", RPCMode.Others, rigidbody.position, rigidbody.rotation, rigidbody.velocity, rigidbody.angularVelocity, fields.health, this.networkView.viewID);
            this.fullSyncRateTmp = this.fullSyncRate;
          }
          else
          {
            this.networkView.RPC("UpdatePartialPlayerState", RPCMode.Others, rigidbody.position, rigidbody.rotation, rigidbody.velocity, this.networkView.viewID);
          }
        }
      }
    }
    #endregion
  }

  public bool isGrounded()
  {
    return Physics.Raycast(this.transform.position, Vector3.down, (this.GetComponent<CapsuleCollider>().height / 2.0f) + 0.2f);
  }


}
