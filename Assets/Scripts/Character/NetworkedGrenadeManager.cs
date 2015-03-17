using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Runtime.InteropServices;
using System;

//add to empty game object and attach in front (offset) of Blitz

public struct NetworkedGrenadeThrowParameters
{
  public Vector3 forward;
  public Vector3 position;
  public Quaternion rotation;
}

public class NetworkedGrenadeManager : MonoBehaviour
{
  public int currentAmmo;
  public int maxAmmo;
  public float recharge; // time to recharge each grenade
  public float rechargeTimer = 0; // track current recharge
  public GameObject prefab; //insert grenade prefab here
  public Vector3 offset; //unused

	public float minThrow; // max, min and cur throw power
	public float maxThrow;
	private float curThrow; // track current throw power
	public float maxTime; // maximum time to charge throw
	private float curTime; // current time spent charging throw

  private bool trigger;
  public float triggerCooldown = 1; //adjustable fire rate
  private float cooldown = 0; // temp var for cooldown after trigger
  private RigidbodyNetworkedPlayerController controller;
  public GameObject _controllerObject; 	//This is the game object that has the controller (should be Blitz)
  public AudioManager _audioManager;

  private bool goingToFire = false;
	
  void Start()
  {
	controller = _controllerObject.GetComponent<RigidbodyNetworkedPlayerController>();
  }

  void Update()
  {
    if (networkView.isMine)
    {
      trigger = controller.gamepadState.RightShoulder || (controller.gamepadState.RightTrigger > 0.20f); 
      cooldown -= Time.deltaTime; // reduce cooldown timer

		if (trigger){
			controller.playerState = PlayerControllerState.AIMING; //let blitz aim (even if no ammo)
			goingToFire = true; // bool to indicate intent to fire
			if (curTime<maxTime){ // start charging up
				curTime+=Time.deltaTime; 
				curThrow=Mathf.Lerp (minThrow,maxThrow,curTime/maxTime); // lerp between min and max by percentage
			}
		} else if (!trigger){
			controller.playerState = PlayerControllerState.IDLE; // on trigger release, revert to idle
			if (cooldown <= 0.0 && currentAmmo > 0 && goingToFire) // conditions for firing
			{
				Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
				forward = forward.normalized;
				networkView.RPC("throwGrenade", RPCMode.Others, forward, transform.position, transform.rotation);
				throwGrenade(forward, transform.position, transform.rotation); 
				currentAmmo--; // reduce ammo
				cooldown = triggerCooldown; // reset cooldown
			}
			curTime = 0; // reset charge time
			curThrow = minThrow; // reset throwing power
			goingToFire = false; // always remove intent to fire after releasing trigger
		}
    }
  }

  // method to recharge grenades when under max ammo
  void FixedUpdate()
  {
    if (currentAmmo < maxAmmo)
    {
      if (rechargeTimer > recharge)
      {
        currentAmmo++;
        rechargeTimer = 0;
      }
      rechargeTimer += Time.deltaTime;

    }
  }

	[RPC]
	void throwGrenade(Vector3 forward, Vector3 position, Quaternion rotation)
	{
		GameObject clone;
		clone = Instantiate(prefab, position + forward, rotation) as GameObject;
		clone.rigidbody.velocity = forward * curThrow;
		_audioManager.Play("grenade_toss", 0.0f, false);
	}
}
