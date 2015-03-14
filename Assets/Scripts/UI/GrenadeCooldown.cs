using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrenadeCooldown : MonoBehaviour {

	public string grenadeObjectName;
	private Image image;
	private GrenadeManager grenadeManager;

	// Use this for initialization
	void Start () {
		image = gameObject.GetComponent<Image> ();
		grenadeManager = GameObject.Find (grenadeObjectName).GetComponent<GrenadeManager>();
	}
	// Update is called once per frame
	void Update () {
			float currentFill = (float)(grenadeManager.currentAmmo + ((grenadeManager.rechargeTimer/grenadeManager.recharge)))/grenadeManager.maxAmmo;
//			float currentFill = (float)(grenadeManager.rechargeTimer/grenadeManager.recharge);
			if (currentFill!=image.fillAmount) {
				image.fillAmount = Mathf.MoveTowards (image.fillAmount, currentFill, Time.deltaTime * 0.7f);
		}
	}
}
