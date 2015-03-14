using UnityEngine;
using System;
using System.Collections;

namespace BlackfireStudio
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class ImageEffects : MonoBehaviour {

		private static int	screenHeight	= -1;
		private static int	screenwidth		= -1;

		private static Texture2D	renderTexture;
		public static Texture2D		RenderTexture
		{
			get
			{
				if (renderTexture == null)
				{
					renderTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
				}
				return renderTexture;
			}
		}

		public static bool IsPro<T>(GameObject go, Type type, Shader s) where T : MonoBehaviour
		{
			bool isPro = Application.HasProLicense();
	#if UNITY_EDITOR
			if (!isPro)
			{
				Debug.LogWarning("Image effects are not supported... But here is a little magic. Unfortunately as nothing is free, it will cost some performance.");
				BlackfireStudio.ImageEffects ie = go.GetComponent<BlackfireStudio.ImageEffects>();
				if (ie == null)
				{
					go.AddComponent<BlackfireStudio.ImageEffects>();
					Debug.LogWarning("Move the ImageEffects component above the " + typeof(T) + " component.");
				}
			}
			else
			{
				DestroyImmediate(go.GetComponent<ImageEffects>());
			}

			// Disable if this shader is not supported
			if (!s || !s.isSupported)
			{
				Debug.Log(type.ToString() + " shader is not supported. I can't do anything about it.");
				go.GetComponent<T>().enabled = false;
			}
	#endif
			return isPro;
		}

		public static void RenderImageEffect(Material m)
		{
			GL.PushMatrix();
			for (int i = 0; i < m.passCount; ++i)
			{
				m.SetPass(i);
				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				GL.Color(new Color(1, 1, 1, 1));
				GL.MultiTexCoord(0, new Vector3(0, 0, 0));
				GL.Vertex3(0, 0, 0);
				GL.MultiTexCoord(0, new Vector3(0, 1, 0));
				GL.Vertex3(0, 1, 0);
				GL.MultiTexCoord(0, new Vector3(1, 1, 0));
				GL.Vertex3(1, 1, 0);
				GL.MultiTexCoord(0, new Vector3(1, 0, 0));
				GL.Vertex3(1, 0, 0);
				GL.End();
			}
			GL.PopMatrix();
		}

		public void OnPostRender()
		{
			if (Screen.width != screenwidth || Screen.height != screenHeight)
			{
				RenderTexture.Resize(Screen.width, Screen.height, TextureFormat.RGB24, false);
				screenwidth		= Screen.width;
				screenHeight	= Screen.height;
			}
			RenderTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			RenderTexture.Apply();
		}
	}
}