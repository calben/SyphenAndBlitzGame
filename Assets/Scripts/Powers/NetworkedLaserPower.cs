//Reference: http://docs.unity3d.com/ScriptReference/Object.Instantiate.html

using UnityEngine;
using System.Collections;
using GamepadInput;

/**
* Spawn a rigid body GameObject with an initial velocity when triggered. 
* Constraints: The projectile must contain a rigid body.
*/
public class NetworkedLaserPower : MonoBehaviour
{
	//Launch properties
	public GameObject _parent;
	public GameObject _projectile;
	public Vector3 _offset;
	
	//Rate of fire
	public float _cooldown = 1;
	float _cooldownTimer;
	
	//Controller properties
	public ProjectileTriggerButton _projectileButton = ProjectileTriggerButton.LEFT;
	GamePad.Index _padIndex = GamePad.Index.One;
	float _triggerThreshold = 0.20f;
	DeftPlayerController _controller;
	
	//Controllable Projectile
	bool _alreadyFired = false;
	GameObject _controlledProjectile = null;
	public AudioManager _audioManager;
	
	
	// Use this for initialization
	void Start()
	{
		_cooldownTimer = _cooldown;
		_controller = GameObject.FindGameObjectWithTag("Player").GetComponent<DeftPlayerController>();
		if (_parent)
		{
			this.transform.parent = _parent.transform;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		if (this.networkView.isMine || _controller.singlePlayer)
		{
			_cooldownTimer -= Time.deltaTime;
			
			if (_parent)
			{
				this.transform.position = _parent.transform.position;
			}
			
			bool leftTriggerHeld = (GamePad.GetTrigger(GamePad.Trigger.LeftTrigger, _padIndex) > _triggerThreshold);
			bool rightTriggerHeld = (GamePad.GetTrigger(GamePad.Trigger.RightTrigger, _padIndex) > _triggerThreshold);
			if (_cooldownTimer <= 0.0f)
			{
				//----FIRING----//
				if ((leftTriggerHeld && _projectileButton == ProjectileTriggerButton.LEFT)
				    || (rightTriggerHeld && _projectileButton == ProjectileTriggerButton.RIGHT))
				{
					FireBeam();
					_cooldownTimer = _cooldown;
				}
				//----NOT FIRING----//
				else
				{
					if (_alreadyFired)
					{
						_alreadyFired = false;
						DeactivatePower ();
					}
				}
			}
		}
	}

	void ActivatePower(Vector3 startPosition, Vector3 direction){
		_audioManager.Play("laser_sustain", 0.25f, true);
		if(Network.isClient || Network.isServer ){
			_controlledProjectile = Network.Instantiate(_projectile, startPosition, Quaternion.identity, 1) as GameObject;
		}
		else{
			_controlledProjectile = Instantiate(_projectile, startPosition, Quaternion.identity) as GameObject;
		}
	}

	void DeactivatePower(){
		_audioManager.Stop("laser_sustain", 6.5f);
		if (_controlledProjectile) {
			Destroy (_controlledProjectile);
		}
	}
	
	void FireBeam()
	{
		RaycastHit hit;
		float beamRange = 500;
		float beamDiameter = 3;
		float beamDistanceMagnifier = 5.0f;

		if (!_alreadyFired)
		{
			ActivatePower (this.transform.position, new Vector3(0,0,0));
			_alreadyFired = true;
		}
		_controlledProjectile.transform.position = this.transform.position + _offset;
		
		float currentDistanceFiring = 5;
		Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
		if (Physics.Raycast(Camera.main.transform.position, cameraForward, out hit, beamRange))
		{
			currentDistanceFiring = Vector3.Distance(hit.point, this.transform.position);
			_controlledProjectile.transform.LookAt(hit.point);
			_controlledProjectile.transform.RotateAround(this.transform.position, _controlledProjectile.transform.right, -90);
			_controlledProjectile.transform.localScale = new Vector3(beamDiameter, currentDistanceFiring * beamDistanceMagnifier, beamDiameter);
		}
		else
		{
			_controlledProjectile.transform.LookAt(_controlledProjectile.transform.position + cameraForward);
			_controlledProjectile.transform.RotateAround(this.transform.position, _controlledProjectile.transform.right, -90);
			_controlledProjectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		}
		
		if ((Network.isClient || Network.isServer) && this.networkView.isMine)
		{
			networkView.RPC("UpdatePowerPosition", RPCMode.Others, _controlledProjectile.transform.position,
			                _controlledProjectile.transform.rotation,
			                _controlledProjectile.transform.localScale);
			
			UpdatePowerPosition(_controlledProjectile.transform.position,
			          _controlledProjectile.transform.rotation,
			          _controlledProjectile.transform.localScale);
		}
	}
	
	[RPC]
	void UpdatePowerPosition(Vector3 position, Quaternion rotationAngles, Vector3 scale)
	{
		_controlledProjectile.transform.position = position;
		_controlledProjectile.transform.rotation = rotationAngles;
		_controlledProjectile.transform.localScale = scale;
	}
}