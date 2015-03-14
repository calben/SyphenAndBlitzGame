using UnityEngine;
using System.Collections;
using GamepadInput;

public class Thrusters : MonoBehaviour
{

  public float thrusterPower = 0.2f;
  public float velocityDampingSpeed = 1.0f;
  public float rotationRightingSpeed = 1.0f;
  public float maxThrusterTime = 5.0f;
  public float thrusterTimeAvailable = 5.0f;
  public float verticalThrusterMultiplier = 3.0f;
  public float recoveryRateMultiplier = 2.0f;

  public bool debug = true;


  NetworkView networkView;
  bool thrustersWereActivated = false;

  void Awake()
  {
    this.networkView = this.GetComponent<NetworkView>();
  }

  void FixedUpdate()
  {
    GamepadState state = this.GetComponent<RigidbodyNetworkedPlayerController>().gamepadState;
    Vector3 direction = new Vector3();
    if (state != null)
    {
      if (state.A)
      {
        Quaternion currentRotation = this.GetComponent<Rigidbody>().transform.rotation;
        this.GetComponent<Rigidbody>().transform.rotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(currentRotation.x, 90, currentRotation.z), this.rotationRightingSpeed * Time.deltaTime);
        this.GetComponent<Rigidbody>().velocity = Vector3.Lerp(this.GetComponent<Rigidbody>().velocity, Vector3.zero, this.velocityDampingSpeed * Time.deltaTime);
        this.GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(this.GetComponent<Rigidbody>().angularVelocity, Vector3.zero, this.velocityDampingSpeed * Time.deltaTime);
        this.thrusterTimeAvailable = Mathf.Lerp(this.thrusterTimeAvailable, 0f, 0.5f * Time.deltaTime);
        return;
      }
      if (state.LeftShoulder)
      {
        direction = this.GetComponent<Rigidbody>().transform.up * this.verticalThrusterMultiplier;
      }
      else if (state.RightShoulder)
      {
        direction = this.GetComponent<Rigidbody>().transform.up * -1 * this.verticalThrusterMultiplier;
      }
      else
      {
        if (!thrustersWereActivated)
        {
          this.thrusterTimeAvailable += this.recoveryRateMultiplier * Time.deltaTime;
          this.thrusterTimeAvailable = Mathf.Clamp(this.thrusterTimeAvailable, 0f, this.maxThrusterTime);
        }
        thrustersWereActivated = false;
        return;
      }
    }
    if (this.thrusterTimeAvailable > 0.1f)
    {

      this.thrustersWereActivated = true;
      if (Network.isServer || Network.isClient)
      {
        networkView.RPC("ActivateThrusters", RPCMode.AllBuffered, direction);
      }
      else
      {
        this.ActivateThrusters(direction);
      }
    }
  }

  public void ActivatePrimaryMovementThrusters(Vector3 direction)
  {
    if (this.thrusterTimeAvailable > 0.1f)
    {
      this.thrustersWereActivated = true;
      if (Network.isServer || Network.isClient)
      {
        this.GetComponent<NetworkView>().RPC("PrimaryMovementThrusters", RPCMode.All, direction);
      }
      else
      {
        this.PrimaryMovementThrusters(direction);
      }
    }
  }

  [RPC]
  public void PrimaryMovementThrusters(Vector3 direction)
  {
    this.GetComponent<Rigidbody>().AddForce(direction * this.GetComponent<Rigidbody>().mass * this.thrusterPower * Time.deltaTime, ForceMode.Impulse);
    this.transform.forward = Vector3.Lerp(this.transform.forward, direction, this.velocityDampingSpeed * Time.deltaTime);
    this.GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(this.GetComponent<Rigidbody>().angularVelocity, Vector3.zero, this.velocityDampingSpeed * Time.deltaTime);
    this.thrusterTimeAvailable -= Time.deltaTime;
  }

  [RPC]
  public void ActivateThrusters(Vector3 direction)
  {
    this.gameObject.GetComponent<Rigidbody>().AddForce(direction * this.gameObject.GetComponent<Rigidbody>().mass * thrusterPower * Time.deltaTime, ForceMode.Impulse);
    this.thrusterTimeAvailable -= Time.deltaTime;
  }

}
