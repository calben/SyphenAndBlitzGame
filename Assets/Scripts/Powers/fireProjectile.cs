//Reference: http://docs.unity3d.com/ScriptReference/Object.Instantiate.html

using UnityEngine;
using System.Collections;
using GamepadInput;

public enum ProjectileAction { THROW, BEAM, ATTACH_TO_SELF, REMOTE_CTRL }
public enum ProjectileTriggerButton { LEFT, RIGHT }

/**
* Spawn a rigid body GameObject with an initial velocity when triggered. 
* Constraints: The projectile must contain a rigid body.
*/
public class fireProjectile : MonoBehaviour
{
	//Launch properties
	public GameObject _parent;
	public GameObject _projectile;
	public GameObject _otherGun;
	public Vector3 _offset;
	public Vector3 _trajectory = Vector3.forward;
	public float _magnitude = 50;
	public float _drag = 5;
	public bool _makeChild = false;
	public ProjectileAction _projectileAction = ProjectileAction.THROW;
	public float _controlPullSpeed = 0.1f;

	public LayerMask layerMask;
	
	//Rate of fire
	//public float _powerUpTime = 1.0f;
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
					switch (_projectileAction)
					{
					case ProjectileAction.THROW:
						if(!_alreadyFired) 
						{
							Debug.Log("i'm gonna launch it");
							if (Network.isClient || Network.isServer)
							{
								networkView.RPC("LaunchProjectile", RPCMode.All, _offset, _magnitude, _makeChild);
							}
							else
							{
								LaunchProjectile(_offset, _magnitude, _makeChild);
							}
						}
						break;
						
					case ProjectileAction.BEAM: 
						FireBeam();
						break;
						
					case ProjectileAction.REMOTE_CTRL:
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
							_alreadyFired = true;
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
						break;
						
					default: break;
					}
					_cooldownTimer = _cooldown;
				}
				//----NOT FIRING----//
				else
				{
					switch (_projectileAction)
					{
					case ProjectileAction.THROW:
						
						break;
						
					case ProjectileAction.BEAM:
						if (_alreadyFired)
						{
							_alreadyFired = false;
							if (_controlledProjectile) { Destroy(_controlledProjectile); }
						}
						break;
						
					case ProjectileAction.REMOTE_CTRL:
						if (_alreadyFired)
						{
							_alreadyFired = false;
							if (_controlledProjectile) { Destroy(_controlledProjectile); }
							if (_controlledTarget) { Destroy(_controlledTarget); }
							_controller.enabled = true; // unfreeze the player
							_otherGun.SetActive(true);  // unfreeze the other gun
						}
						break;
						
					default: break;
					}
				}
			}
		}
	}
	
	[RPC]
	void LaunchProjectile(Vector3 offset, float magnitude, bool makeChild)
	{
		GameObject clone;
		clone = Instantiate(_projectile, transform.position + offset, transform.rotation) as GameObject;
		//clone.rigidbody.velocity = transform.TransformDirection( trajectory * magnitude );
		
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward = forward.normalized;
		clone.rigidbody.velocity = (new Vector3(forward.x * magnitude, 0, forward.z * magnitude));
		
		if (makeChild)
		{
			clone.transform.parent = this.transform;
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
			if(Network.isClient || Network.isServer ){
				_controlledProjectile = Network.Instantiate(_projectile, this.transform.position, Quaternion.identity, 1) as GameObject;
			}
			else{
				_controlledProjectile = Instantiate(_projectile, this.transform.position, Quaternion.identity) as GameObject;
			}
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
			networkView.RPC("LaserBeam", RPCMode.Others, _controlledProjectile.transform.position,
			                _controlledProjectile.transform.rotation,
			                _controlledProjectile.transform.localScale);
			
			LaserBeam(_controlledProjectile.transform.position,
			          _controlledProjectile.transform.rotation,
			          _controlledProjectile.transform.localScale);
		}
	}
	
	[RPC]
	void LaserBeam(Vector3 position, Quaternion rotationAngles, Vector3 scale)
	{
		_controlledProjectile.transform.position = position;
		_controlledProjectile.transform.rotation = rotationAngles;
		_controlledProjectile.transform.localScale = scale;
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
		if(Network.isClient || Network.isServer){
			_controlledProjectile = Network.Instantiate(_projectile, transform.position + _offset + (cameraForward * distance), transform.rotation, 1) as GameObject;
		}
		else{
			_controlledProjectile = Instantiate(_projectile, transform.position + _offset + (cameraForward * distance), transform.rotation) as GameObject;
		}
		_controlledTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_controlledTarget.transform.position = transform.position + _offset + (cameraForward * distance);
		if (_controlledTarget.collider) { _controlledTarget.collider.enabled = false; }
		_controlledTarget.renderer.enabled = false;
		_controlledTarget.transform.parent = Camera.main.transform;
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
		
		if (pull_closer && dist > 4f) {
			pull = -1 * _controlPullSpeed;
		}
		
		// get forward direction
		Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
		Vector3 cameraRight = Camera.main.transform.TransformDirection(Vector3.right).normalized;
		Vector3 move_direction = pull * cameraForward;
		
		// if the controllable hits the floor, keep the player from going lower into the floor
		if (Physics.Raycast (_controlledTarget.transform.position, -_controlledTarget.transform.up, 0.7f, layerMask)) {
			if (_controller.controllerLookDirection.y < 0)
				_controller.controllerLookDirection.y = 0;	
		}
		
		//apply movement
		_controlledTarget.transform.position = _controlledTarget.transform.position + move_direction;
		_controlledProjectile.transform.position = Vector3.Lerp(_controlledProjectile.transform.position, _controlledTarget.transform.position, _drag * Time.deltaTime);
	}
}