using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Blackfire Studio/Frost")]
public class Frost : MonoBehaviour {

	private bool		isPro;

	public Shader		shader;
	
	public Color		color;
	
	public Texture2D	diffuseTex;
	public Texture2D	bumpTex;
	public Texture2D	coverageTex;
	
	public float		transparency;
	public float		refraction;
	public float		coverage;
	public float		smooth;

	private Material	frostMaterial;
	protected Material	material
	{
		get
		{
			if (frostMaterial == null)
			{
				frostMaterial = new Material(shader);
				frostMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return frostMaterial;
		}
	}

	void Start()
	{
		isPro = BlackfireStudio.ImageEffects.IsPro<Frost>(gameObject, typeof(Frost), shader);
	}

	void Update()
	{
		if (!isPro)
		{
			if (shader != null)
			{
				material.SetTexture("_MainTex", BlackfireStudio.ImageEffects.RenderTexture);

				material.SetColor("_Color", color);
				material.SetFloat("_Transparency", transparency);
				material.SetFloat("_Refraction", refraction);
				material.SetFloat("_Coverage", coverage);
				material.SetFloat("_Smooth", smooth);

				if (diffuseTex != null) { material.SetTexture("_DiffuseTex", diffuseTex); }		else { material.SetTexture("_DiffuseTex", null); }
				if (bumpTex != null) { material.SetTexture("_BumpTex", bumpTex); }				else { material.SetTexture("_BumpTex", null); }
				if (coverageTex != null) { material.SetTexture("_CoverageTex", coverageTex); }	else { material.SetTexture("_CoverageTex", null); }
			}
		}
	}

	void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (shader != null)
		{
			material.SetColor("_Color", color);
			material.SetFloat("_Transparency", transparency);
			material.SetFloat("_Refraction", refraction);
			material.SetFloat("_Coverage", coverage);
			material.SetFloat("_Smooth", smooth);
			
			if (diffuseTex != null) { material.SetTexture("_DiffuseTex", diffuseTex); }		else { material.SetTexture("_DiffuseTex", null); }
			if (bumpTex != null) { material.SetTexture("_BumpTex", bumpTex); }				else { material.SetTexture("_BumpTex", null); }
			if (coverageTex != null) { material.SetTexture("_CoverageTex", coverageTex); }	else { material.SetTexture("_CoverageTex", null); }
			
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	public void OnPostRender()
	{
		if (!isPro)
		{
			BlackfireStudio.ImageEffects.RenderImageEffect(material);
		}
	}

	void OnEnable()
	{
		isPro = BlackfireStudio.ImageEffects.IsPro<Frost>(gameObject, typeof(Frost), shader);
	}

	void OnDisable()
	{
		if (frostMaterial != null)
		{
			DestroyImmediate(frostMaterial);
		}
	}
}