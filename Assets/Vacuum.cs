using UnityEngine;
using System.Collections;
using GamepadInput;

public class Vacuum : MonoBehaviour
{

  public float magnitude;
  private float trigger;
  private DeftPlayerController controller;
  public bool affectEverything;
  private ParticleEmitter effect;
  public bool active;

  void Start()
  {
    controller = GameObject.FindGameObjectWithTag("Player").GetComponent<DeftPlayerController>();
    effect = GetComponentInChildren<ParticleEmitter>();
    effect.emit = false;
  }


  [RPC]
  void Activate()
  {
    this.active = true;
  }

  [RPC]
  void DeActivate()
  {
    this.active = false;
  }

  void Update()
  {
    if (this.networkView.isMine)
    {
      if (controller.gamepadState.RightTrigger > 0.20f && !this.active)
      {
        this.networkView.RPC("Activate", RPCMode.Others);
        this.Activate();
      }
      else if (controller.gamepadState.RightTrigger > 0.20f && this.active)
      {
      }
      else
      {
        this.networkView.RPC("DeActivate", RPCMode.Others);
        this.DeActivate();
      }
    }
    //Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
    //this.transform.LookAt (Camera.main.transform.position);
    //transform.rotation = Quaternion.LookRotation (-Camera.main.transform.forward, Camera.main.transform.right);
  }

  void OnTriggerStay(Collider other)
  {
    if (active)
      if (other.rigidbody!=null && other.rigidbody.useGravity && other.tag != "Player")
      {
        PhysicsStatus status = other.gameObject.GetComponent<PhysicsStatus>();
        Vector3 direction = Vector3.Normalize(other.transform.position - this.transform.parent.position);
        if (status == null || status.pullable || affectEverything)
        {
          other.rigidbody.AddForce(direction * (-1 * magnitude), ForceMode.Impulse);
        }
      }
  }

}
