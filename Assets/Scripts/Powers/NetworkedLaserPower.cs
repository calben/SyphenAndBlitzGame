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
  public GameObject _controllerObject; 	//This is the game object that has the controller (Typically Blitz or Syphen)
  public GameObject _parent;			//This is the game object the weapon should be attached to
  public GameObject _projectile;		//This is the bullet that gets fired from the weapon
  public GameObject _otherGun;			//This is a reference to disable the other gun while this is active
  public AudioManager _audioManager;	//This stores the audio clips that need to be played
  public string _audioClipName = "suction";
  public Vector3 _offset;
  public ArcReactorDemoGunControllerEdit _arcReactor;

  //Rate of fire
  public float _cooldown = 1;
  float _cooldownTimer;

  //Controller properties
  public ProjectileTriggerButton _projectileButton = ProjectileTriggerButton.LEFT;
  GamePad.Index _padIndex = GamePad.Index.One;
  float _triggerThreshold = 0.20f;
  RigidbodyNetworkedPlayerController _controller;

  //Controllable Projectile
  bool _alreadyFired = false;
  GameObject _controlledProjectile = null;


  GameManager _gameManager;

  // Use this for initialization
  void Start()
  {
	GameObject gameManager = GameObject.Find ("GameManager");
	if(gameManager){
		_gameManager = gameManager.GetComponent<GameManager>();
	}

    _cooldownTimer = _cooldown;
    _controller = _controllerObject.GetComponent<RigidbodyNetworkedPlayerController>();
    if (_parent)
    {
      this.transform.parent = _parent.transform;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (_controller.isThisMachinesPlayer)
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
		if ((leftTriggerHeld && _projectileButton == ProjectileTriggerButton.LEFT && (_gameManager == null || _gameManager.longRangeUnlocked))
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
			if(Network.isClient || Network.isServer){
            	this.GetComponent<NetworkView>().RPC("DeactivatePower", RPCMode.Others);
			}
            DeactivatePower();
          }
        }
      }
    }
  }

  [RPC]
  void ActivatePower(Vector3 startPosition, Vector3 direction)
  {
	_arcReactor.StartLaunch();
    _controlledProjectile = Instantiate(_projectile, startPosition, Quaternion.identity) as GameObject;
  }

  [RPC]
  void DeactivatePower()
  {
	_arcReactor.EndLaunch();
    _audioManager.Stop(_audioClipName, 6.5f);
    if (_controlledProjectile)
    {
      Destroy(_controlledProjectile);
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
		if(Network.isClient || Network.isServer){
			this.GetComponent<NetworkView>().RPC("ActivatePower", RPCMode.Others, this.transform.position, new Vector3(0, 0, 0));
		}
      ActivatePower(this.transform.position, new Vector3(0, 0, 0));
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

    if (_controller.isThisMachinesPlayer)
    {
		if(Network.isClient || Network.isServer){
	    	GetComponent<NetworkView>().RPC("UpdatePowerPosition", RPCMode.Others, _controlledProjectile.transform.position,
	                      _controlledProjectile.transform.rotation,
	                      _controlledProjectile.transform.localScale);
		}
		else{
			UpdatePowerPosition(_controlledProjectile.transform.position,
			                    _controlledProjectile.transform.rotation,
			                    _controlledProjectile.transform.localScale);
		}
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