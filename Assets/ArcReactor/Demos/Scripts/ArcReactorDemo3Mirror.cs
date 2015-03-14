﻿using UnityEngine;
using System.Collections;

public class ArcReactorDemo3Mirror : MonoBehaviour {

	public GameObject hitParticles;
	public Vector3 rotationSpeed;
	public float temperature;
	public Gradient colorGrad;

	private GameObject partSystem;
	private bool mirrorHit;
	private bool oldMirrorHit;

	const float rotationSpeedCoef = 20;
	const float dissipateRate = 0.25f;
	const float heatRate = 0.5f;



	public void ArcReactorReflection(ArcReactorHitInfo hit)
	{
		//Debug.Log(hit.raycastHit.transform.gameObject.name);
		mirrorHit = true;

		temperature = Mathf.Clamp01(temperature + heatRate * Time.deltaTime);

		if (!partSystem.activeSelf)
			partSystem.SetActive(true);

		if (!partSystem.particleSystem.enableEmission)
			partSystem.particleSystem.enableEmission = true;

		partSystem.transform.position = hit.raycastHit.point;
		partSystem.transform.LookAt(hit.raycastHit.point + hit.raycastHit.normal);
	}

	// Use this for initialization
	void Start () {
		transform.rotation = UnityEngine.Random.rotation;
		rotationSpeed.x = UnityEngine.Random.Range(-rotationSpeedCoef,rotationSpeedCoef);
		rotationSpeed.y = UnityEngine.Random.Range(-rotationSpeedCoef,rotationSpeedCoef);
		rotationSpeed.z = UnityEngine.Random.Range(-rotationSpeedCoef,rotationSpeedCoef);

		partSystem = (GameObject)GameObject.Instantiate(hitParticles);
		partSystem.transform.parent = transform;
		partSystem.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		//If ray stopped hitting mirror
		if (!mirrorHit && !oldMirrorHit && partSystem.particleSystem.enableEmission)
		{
			partSystem.particleSystem.enableEmission = false;
		}
		oldMirrorHit = mirrorHit;
		mirrorHit = false;

		if (!partSystem.particleSystem.enableEmission && partSystem.activeSelf && !partSystem.particleSystem.IsAlive())
		{		
			partSystem.SetActive(false);
		}

		transform.Rotate ( Vector3.left * ( rotationSpeed.x * Time.deltaTime ) );
		transform.Rotate ( Vector3.up * ( rotationSpeed.y * Time.deltaTime ) );
		transform.Rotate ( Vector3.forward * ( rotationSpeed.z * Time.deltaTime ) );

		temperature = Mathf.Clamp01(temperature - dissipateRate * Time.deltaTime);

		renderer.material.color = colorGrad.Evaluate(temperature);
	}
}
