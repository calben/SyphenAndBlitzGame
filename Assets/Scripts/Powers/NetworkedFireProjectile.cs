using UnityEngine;
using System.Collections;
using GamepadInput;

// DEPRECATED, DO NOT USE

public enum NetworkedProjectileAction { THROW, BEAM, ATTACHTOSELF, REMOTECTRL }
public enum NetworkedProjectileTrigger { LEFTRIGGER, RIGHTTRIGGER };

/**
* Spawn a rigid body GameObject with an initial velocity when triggered. 
* Constraints: The projectile must contain a rigid body.
*/
public class NetworkedFireProjectile : MonoBehaviour
{
  public GameObject parent;
  public GameObject projectile;
  public GameObject otherGun;
  public Vector3 offset;
  public Vector3 trajectory = Vector3.forward;
  public float magnitude = 50;
  public float drag = 5;
  public bool makeChild = false;
  public NetworkedProjectileAction NetworkedProjectileAction = NetworkedProjectileAction.THROW;
  public NetworkedProjectileTrigger projectileTrigger;
  public float controlPullSpeed = 0.1f;

  //Rate of fire
  //public float powerUpTime = 1.0f;
  public float cooldown = 1;
  float cooldownTimer;

  //Controller properties
  public GamePad.Trigger projectileButton = GamePad.Trigger.LeftTrigger;
  GamePad.Index padIndex = GamePad.Index.One;
  float triggerThreshold = 0.20f;
  RigidbodyNetworkedPlayerController controller;

  //Controllable Projectile
  bool alreadyFired = false;
  GameObject controlledTarget = null;
  GameObject controlledProjectile = null;


  // Use this for initialization
  void Start()
  {
    cooldownTimer = cooldown;
    controller = this.gameObject.transform.parent.gameObject.GetComponent<RigidbodyNetworkedPlayerController>();
    if (parent)
    {
      this.transform.parent = parent.transform;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (this.GetComponent<NetworkView>().isMine || controller.isThisMachinesPlayer)
    {
      cooldownTimer -= Time.deltaTime;

      if (parent)
      {
        this.transform.position = parent.transform.position;
      }

      bool leftTriggerHeld = (GamePad.GetTrigger(GamePad.Trigger.LeftTrigger, padIndex) > triggerThreshold);
      bool rightTriggerHeld = (GamePad.GetTrigger(GamePad.Trigger.RightTrigger, padIndex) > triggerThreshold);
      if (cooldownTimer <= 0.0f)
      {
        //----FIRING----//
        if ((leftTriggerHeld && projectileTrigger == NetworkedProjectileTrigger.LEFTRIGGER)
            || (rightTriggerHeld && projectileTrigger == NetworkedProjectileTrigger.RIGHTTRIGGER))
        {
          switch (NetworkedProjectileAction)
          {
            case NetworkedProjectileAction.THROW:
              if (!alreadyFired)
              {
                if (Network.isClient || Network.isServer)
                {
                  GetComponent<NetworkView>().RPC("LaunchProjectile", RPCMode.All, offset, magnitude, makeChild);
                }
                else
                {
                  LaunchProjectile(offset, magnitude, makeChild);
                }
              }
              break;

            case NetworkedProjectileAction.BEAM:
              FireBeam();
              break;

            case NetworkedProjectileAction.REMOTECTRL:
              if (!alreadyFired)
              {
                if (Network.isClient || Network.isServer)
                {
                  GetComponent<NetworkView>().RPC("LaunchControllable", RPCMode.All);
                }
                else
                {
                  LaunchControllable();
                }
                alreadyFired = true;
                controller.enabled = false; // freeze the player
                otherGun.SetActive(false);  // freeze the other gun
              }
              else
              {
                if (controlledProjectile)
                {
                  if (Network.isClient || Network.isServer)
                  {
                    GetComponent<NetworkView>().RPC("MoveControllable", RPCMode.All);
                  }
                  else
                  {
                    MoveControllable();
                  }
                }
              }
              break;

            default: break;
          }
          cooldownTimer = cooldown;
        }
        //----NOT FIRING----//
        else
        {
          switch (NetworkedProjectileAction)
          {
            case NetworkedProjectileAction.THROW:
              break;

            case NetworkedProjectileAction.BEAM:
              if (alreadyFired)
              {
                alreadyFired = false;
                if (controlledProjectile) { Destroy(controlledProjectile); }
              }
              break;

            case NetworkedProjectileAction.REMOTECTRL:
              if (alreadyFired)
              {
                alreadyFired = false;
                if (controlledProjectile) { Destroy(controlledProjectile); }
                if (controlledTarget) { Destroy(controlledTarget); }
                controller.enabled = true;
                otherGun.SetActive(true);
              }
              break;

            default: break;
          }
        }
      }
    }
  }

  void LaunchProjectile(Vector3 offset, float magnitude, bool makeChild)
  {
    GameObject clone = Instantiate(projectile, transform.position + offset, transform.rotation) as GameObject;
    Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
    forward = forward.normalized;
    clone.GetComponent<Rigidbody>().velocity = (new Vector3(forward.x * magnitude, 0, forward.z * magnitude));
    if (makeChild)
    {
      clone.transform.parent = this.transform;
    }
  }

  [RPC]
  void FireSimpleProjectile(Vector3 launchFrom, Vector3 direction, float magnitude, ForceNodeType type, ForceNodeBehaviour behaviour)
  {
    GameObject projectile = new GameObject();

  }

  void FireBeam()
  {
    RaycastHit hit;
    float beamRange = 500;
    float beamDiameter = 3;
    float beamDistanceMagnifier = 5.0f;

    if (!alreadyFired)
    {
      if (Network.isClient || Network.isServer)
      {
        controlledProjectile = Network.Instantiate(projectile, this.transform.position, Quaternion.identity, 1) as GameObject;
      }
      else
      {
        controlledProjectile = Instantiate(projectile, this.transform.position, Quaternion.identity) as GameObject;
      }
      alreadyFired = true;
    }
    controlledProjectile.transform.position = this.transform.position + offset;

    float currentDistanceFiring = 5;
    Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
    if (Physics.Raycast(Camera.main.transform.position, cameraForward, out hit, beamRange))
    {
      currentDistanceFiring = Vector3.Distance(hit.point, this.transform.position);
      controlledProjectile.transform.LookAt(hit.point);
      controlledProjectile.transform.RotateAround(this.transform.position, controlledProjectile.transform.right, -90);
      controlledProjectile.transform.localScale = new Vector3(beamDiameter, currentDistanceFiring * beamDistanceMagnifier, beamDiameter);
    }
    else
    {
      controlledProjectile.transform.LookAt(controlledProjectile.transform.position + cameraForward);
      controlledProjectile.transform.RotateAround(this.transform.position, controlledProjectile.transform.right, -90);
      controlledProjectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    if ((Network.isClient || Network.isServer) && this.GetComponent<NetworkView>().isMine)
    {
    }
  }

  void LaserBeam(Vector3 position, Quaternion rotationAngles, Vector3 scale)
  {
    controlledProjectile.transform.position = position;
    controlledProjectile.transform.rotation = rotationAngles;
    controlledProjectile.transform.localScale = scale;
  }

  void LaunchControllable()
  {
    RaycastHit hit;
    float distance = 20;
    Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
    if (Physics.Raycast(transform.position + offset, cameraForward, out hit, distance))
    {
      distance = hit.distance;
    }
    if (Network.isClient || Network.isServer)
    {
      controlledProjectile = Network.Instantiate(projectile, transform.position + offset + (cameraForward * distance), transform.rotation, 1) as GameObject;
    }
    else
    {
      controlledProjectile = Instantiate(projectile, transform.position + offset + (cameraForward * distance), transform.rotation) as GameObject;
    }
    controlledTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    controlledTarget.transform.position = transform.position + offset + (cameraForward * distance);
    if (controlledTarget.GetComponent<Collider>()) { controlledTarget.GetComponent<Collider>().enabled = false; }
    controlledTarget.GetComponent<Renderer>().enabled = false;
    controlledTarget.transform.parent = Camera.main.transform;
  }

  void MoveControllable()
  {
    controller.controllerMoveDirection = Vector2.zero;

    bool pullcloser;
    float pull = 0;
    //Get controller direction
    if (controller.gamepadState != null)
    {
      controller.controllerMoveDirection = GamePad.GetAxis(GamePad.Axis.LeftStick, padIndex);
      controller.controllerLookDirection = GamePad.GetAxis(GamePad.Axis.RightStick, padIndex);
      pullcloser = GamePad.GetTrigger(GamePad.Trigger.RightTrigger, padIndex) > 0.2f;
    }
    else
    {
      controller.controllerMoveDirection = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
      controller.controllerLookDirection = new Vector2(Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1), Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1));
      pullcloser = false;
    }
    if (pullcloser)
    {
      pull = -1 * controlPullSpeed;
    }

    // get forward direction
    Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
    Vector3 cameraRight = Camera.main.transform.TransformDirection(Vector3.right).normalized;
    Vector3 movedirection = pull * cameraForward;

    //apply movement
    controlledTarget.transform.position = controlledTarget.transform.position + movedirection;
    controlledProjectile.transform.position = Vector3.Lerp(controlledProjectile.transform.position, controlledTarget.transform.position, drag * Time.deltaTime);
  }
}