using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageFollow : MonoBehaviour {

	public GameObject followObject;
	public Vector3 offset = Vector3.up;
	public float viewDepth = 50f;//The max depth of object in scene before text is disabled
	public bool background;
	private Camera camera;
	private Image image;
	private AI_Mover enemy;
	private bool dead;

	// Use this for initialization
	void Start () {
		camera = Camera.main;
		image = gameObject.GetComponent<Image>();
		image.enabled = true;
		enemy = followObject.GetComponent<AI_Mover> ();
		dead = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!background) {
			float currentHealth = (float)enemy.health / 100;
			if (currentHealth < image.fillAmount) {
				image.fillAmount = Mathf.MoveTowards (image.fillAmount, currentHealth, Time.deltaTime * 0.5f);
			}
		} else {
			//If enemy died, deactivate the background health bar
			if (!dead && enemy.health <= 0) {
				gameObject.SetActive(false);
				dead = true;
			}
		}
		Vector3 viewport = camera.WorldToViewportPoint (followObject.transform.position + offset);
		//If object is less than a certain selfDistance, show its image
		if (viewport.z < viewDepth && viewport.x >=0 && viewport.y>=0 && viewport.z>=0 && followObject.activeSelf == true) {
			transform.position = new Vector3 (viewport.x * Screen.width, viewport.y * Screen.height, 0f);
			image.enabled = true;
		} else {
			image.enabled = false;
		}
	}
}
