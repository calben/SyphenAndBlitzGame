using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Runtime.InteropServices;
using System;

//add to empty game object and attach in front (offset) of Blitz

public struct GrenadeThrowParameters
{
  public Vector3 forward;
  public Vector3 position;
  public Quaternion rotation;
}

public class GrenadeManager : MonoBehaviour
{
  public int currentAmmo;
  public int maxAmmo;
  public float recharge;
  public float rechargeTimer = 0;
  public GameObject prefab; //insert grenade prefab here
  public Vector3 offset; //unused

  public float minThrow; // max, min and cur throw power
  public float maxThrow;
  public float curThrow;
  public float maxTime; // maximum time to charge throw
  private float curTime; // current time spent charging throw

  private bool trigger;
  public float triggerCooldown = 1; //adjustable fire rate
  private float cooldown = 0; // temp var for cooldown after trigger
  private DeftPlayerController controller;
  public AudioManager _audioManager;

  private bool goingToFire = false;
	
  void Start()
  {
    controller = transform.parent.gameObject.GetComponent<DeftPlayerController>();
  }

  void Update()
  {
    if (GetComponent<NetworkView>().isMine)
    {
      trigger = controller.gamepadState.RightShoulder || (controller.gamepadState.RightTrigger > 0.20f); 
      cooldown -= Time.deltaTime; // reduce cooldown timer

		if (trigger){
			controller.state = PlayerState.aiming; //let blitz aim (even if no ammo)
			goingToFire = true; // bool to indicate intent to fire
			if (curTime<maxTime){ // start charging up
				curTime+=Time.deltaTime; 
				curThrow=Mathf.Lerp (minThrow,maxThrow,curTime/maxTime); // lerp between min and max by percentage
			}
		} else if (!trigger){
			controller.state = PlayerState.idle; // on trigger release, revert to idle
			if (cooldown <= 0.0 && currentAmmo > 0 && goingToFire) // conditions for firing
			{
				Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
				forward = forward.normalized;
				GetComponent<NetworkView>().RPC("throwGrenade", RPCMode.Others, forward, transform.position, transform.rotation);
				throwGrenade(forward, transform.position, transform.rotation, curThrow); 
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
  void throwGrenade(Vector3 forward, Vector3 position, Quaternion rotation, float magnitude)
  {
    GameObject clone;
    clone = Instantiate(prefab, position + forward, rotation) as GameObject;
		clone.GetComponent<Rigidbody>().velocity = forward * magnitude;
	_audioManager.Play("grenade_toss", 0.0f, false);
  }
}
