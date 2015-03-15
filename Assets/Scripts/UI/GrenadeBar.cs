using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GrenadeBar : MonoBehaviour {
	public bool active;
	private bool firstFill;
	public string grenadeObjectName;
	private Image image;
	private GrenadeManager grenadeManager;
	// Use this for initialization
	void Start () {
		active = false;
		firstFill = true;
		image = gameObject.GetComponent<Image> ();
	}
	public void StartGrenadeUI() {
		active = true;
		grenadeManager = GameObject.Find (grenadeObjectName).GetComponent<GrenadeManager>();
		//Activate cooldown bar
		GameObject.Find ("GrenadeBarCoolDown").GetComponent<GrenadeCooldown> ().enabled = true;
		GameObject.Find ("GrenadeBarBG").GetComponent<Image> ().enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (active && firstFill) {
			image.fillAmount = Mathf.MoveTowards (image.fillAmount, 1.0f, Time.deltaTime * 0.8f);
			if (image.fillAmount == 1f)
				firstFill = false;
		}
		if (active && !firstFill) {
			float currentAmmo = (float)grenadeManager.currentAmmo/grenadeManager.maxAmmo;
			if (currentAmmo!=image.fillAmount) {
//				image.fillAmount = Mathf.MoveTowards (image.fillAmount, currentAmmo, Time.deltaTime * 0.7f);
				image.fillAmount = currentAmmo;
			}
		}
	}
}
