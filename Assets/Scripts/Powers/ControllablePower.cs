//Reference: http://docs.unity3d.com/ScriptReference/Object.Instantiate.html

using UnityEngine;
using System.Collections;
using GamepadInput;

/**
* Spawn a rigid body GameObject with an initial velocity when triggered. 
* Constraints: The projectile must contain a rigid body.
*/
public class ControllablePower : MonoBehaviour
{
	//Launch properties
	public GameObject _parent;
	public GameObject _projectile;
	public GameObject _otherGun;
	public AudioManager _audioManager;
	public Vector3 _offset;
	public float _drag = 5;
	public float _controlPullSpeed = 0.1f;
	public float _distanceFromPlayer = 4f;
	public float _distanceFromFloor = 0.7f;
	
	public LayerMask layerMask;
	
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
	GameObject _controlledTarget = null;
	GameObject _controlledProjectile = null;
	
	
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
						if (!_alreadyFired)
						{
							if (Network.isClient || Network.isServer)
							{
								networkView.RPC("LaunchControllable", RPCMode.All);
							}
							else
							{
								LaunchControllable();
							}
							_audioManager.PlaySFX("suction", 1.0f, false);
							_alreadyFired = true;
							_controller.state = PlayerState.aiming; //Zoom the camera in
							_controller.enabled = false; // freeze the player
							_otherGun.SetActive(false);  // freeze the other gun
						}
						else
						{
							if (_controlledProjectile)
							{
								if (Network.isClient || Network.isServer)
								{
									networkView.RPC("MoveControllable", RPCMode.All);
								}
								else
								{
									MoveControllable();
								}
							}
						}
					_cooldownTimer = _cooldown;
				}
				//----NOT FIRING----//
				else
				{
					if (_alreadyFired)
					{
						_audioManager.StopSFX("suction", 1.0f);
						_alreadyFired = false;
						DeactivatePower ();
					}
				}
			}
		}
	}
	void ActivatePower(Vector3 startPosition){
		if(Network.isClient || Network.isServer){
			_controlledProjectile = Network.Instantiate(_projectile, startPosition, transform.rotation, 1) as GameObject;
		}
		else{
			_controlledProjectile = Instantiate(_projectile, startPosition, transform.rotation) as GameObject;
		}

		_controlledTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_controlledTarget.transform.position = startPosition;
		if (_controlledTarget.collider) { _controlledTarget.collider.enabled = false; }
		_controlledTarget.renderer.enabled = false;
		_controlledTarget.transform.parent = Camera.main.transform;
	}


	void DeactivatePower(){
		if (_controlledProjectile) { Destroy(_controlledProjectile); }
		if (_controlledTarget) { Destroy(_controlledTarget); }
		_controller.enabled = true; // unfreeze the player
		_otherGun.SetActive(true);  // unfreeze the other gun
	}
	
	[RPC]
	void LaunchControllable()
	{
		RaycastHit hit;
		float distance = 20;
		Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
		if (Physics.Raycast(transform.position + _offset, cameraForward, out hit, distance))
		{
			distance = hit.distance;
		}

		ActivatePower (transform.position + _offset + (cameraForward * distance));
	}
	
	[RPC]
	void MoveControllable()
	{
		_controller.controllerMoveDirection = Vector2.zero;
		
		bool pull_closer;
		float pull = 0;
		//Get controller direction
		if (_controller.gamepadState != null)
		{
			_controller.controllerMoveDirection = GamePad.GetAxis(GamePad.Axis.LeftStick, _padIndex);
			_controller.controllerLookDirection = GamePad.GetAxis(GamePad.Axis.RightStick, _padIndex);
			
			// feels better (can't lose control or the controllable and scatter all the items)
			// but also makes the cam move slow enough to allow the later raycast to not miss
			_controller.controllerLookDirection.x = 0.15f * _controller.controllerLookDirection.x*_controller.controllerLookDirection.x*_controller.controllerLookDirection.x;
			_controller.controllerLookDirection.y = 0.15f * _controller.controllerLookDirection.y*_controller.controllerLookDirection.y*_controller.controllerLookDirection.y;
			
			pull_closer = GamePad.GetTrigger(GamePad.Trigger.RightTrigger, _padIndex) > 0.2f;
		}
		else
		{
			_controller.controllerMoveDirection = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
			_controller.controllerLookDirection = new Vector2(Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1), Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1));
			pull_closer = false;
		}
		// make controllable stop in front of player (player is the parent of the parent of this script)
		float dist = Vector3.Distance (_controlledTarget.transform.position, _parent.transform.parent.transform.position);//Camera.main.transform.position);
		
		if (pull_closer && dist > _distanceFromPlayer) {
			pull = -1 * _controlPullSpeed;
		}
		
		// get forward direction
		Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
		Vector3 cameraRight = Camera.main.transform.TransformDirection(Vector3.right).normalized;
		Vector3 move_direction = pull * cameraForward;
		
		// if the controllable hits the floor, keep the player from going lower into the floor
		if (Physics.Raycast (_controlledTarget.transform.position, -_controlledTarget.transform.up, _distanceFromFloor, layerMask)) {
			if (_controller.controllerLookDirection.y < 0)
				_controller.controllerLookDirection.y = 0;	
		}
		
		//apply movement
		_controlledTarget.transform.position = _controlledTarget.transform.position + move_direction;

		MovePower (_controlledProjectile.transform.position, _controlledTarget.transform.position, new Vector3(0,0,0));

	}

	void MovePower(Vector3 currentPosition, Vector3 newPosition, Vector3 velocity){
		_controlledProjectile.transform.position = Vector3.Lerp(currentPosition, newPosition, _drag * Time.deltaTime);
	}
}