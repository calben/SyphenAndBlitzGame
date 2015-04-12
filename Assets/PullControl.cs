using UnityEngine;
using System.Collections;

public class PullControl : MonoBehaviour {

	private DeftPlayerController controller;
	private bool alreadyFired;
	public GameObject controlPrefab;

	GameObject _controlledTarget;
	GameObject CP;

	void Start()
	{
		controller = transform.parent.gameObject.GetComponent<DeftPlayerController>();
	}

	[RPC]
	void LaunchControllable()
	{
		CP = Network.Instantiate(controlPrefab, transform.position, Quaternion.identity,1) as GameObject;
	}
	
	[RPC]
	void MoveControllable()
	{/*
		controller.controllerMoveDirection = Vector2.zero;
		//Get controller direction
		if (controller.gamepadState != null)
		{
			controller.controllerMoveDirection = GamePad.GetAxis(GamePad.Axis.LeftStick, _padIndex);
			controller.controllerLookDirection = GamePad.GetAxis(GamePad.Axis.RightStick, _padIndex);
		}
		else
		{
			controller.controllerMoveDirection = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
			controller.controllerLookDirection = new Vector2(Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1), Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1));
		}
		
		// get forward direction
		Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
		Vector3 cameraRight = Camera.main.transform.TransformDirection(Vector3.right).normalized;
		Vector3 move_direction = controller.controllerMoveDirection.y * cameraForward + controller.controllerMoveDirection.x * cameraRight;
		
		//apply movement
		_controlledTarget.transform.position = _controlledTarget.transform.position + move_direction * _magnitude;
		_controlledProjectile.transform.position = Vector3.Lerp(_controlledProjectile.transform.position, _controlledTarget.transform.position, _drag * Time.deltaTime);
	*/}

	void Update()
	{
		if (this.GetComponent<NetworkView>().isMine)
		{
			bool trigger = controller.gamepadState.RightShoulder; //|| (controller.gamepadState.RightTrigger > 0.20f); 
			if (trigger){
				if (!alreadyFired){
					if (Network.isClient || Network.isServer){
						GetComponent<NetworkView>().RPC("LaunchControllable", RPCMode.All);
					}else{
						LaunchControllable();
					}
					alreadyFired = true;
					controller.enabled = false; // freeze the player
				}else{
					if (controlPrefab){
						if (Network.isClient || Network.isServer){
							GetComponent<NetworkView>().RPC("MoveControllable", RPCMode.All);
						}else{
							MoveControllable();
				}	}	}
			}else if (!trigger) {
				if (alreadyFired){
					alreadyFired = false;
					if (controlPrefab) { Destroy(controlPrefab); }
					controller.enabled = true; // unfreeze the player
	}	}	}	}

}
