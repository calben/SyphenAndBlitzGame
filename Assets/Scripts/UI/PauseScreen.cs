using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PauseScreen : MonoBehaviour {

	public GameObject pauseMenu;
	private bool isActive;
	public GameObject eventSystemObject;
	public GameObject firstSelectedObject;
	private EventSystem eventSystem;

	// Use this for initialization
	void Start () {
		isActive = false;
		pauseMenu.SetActive (isActive);
		Time.timeScale = 1;
		eventSystem = eventSystemObject.GetComponent<EventSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("joystick button 7"))
		{
			isActive = !isActive;

			
			//Stop animations
			if (isActive)
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = 1;
			}
			pauseMenu.SetActive(isActive);
			if (isActive) eventSystem.SetSelectedGameObject(firstSelectedObject);
		}
	}
}
