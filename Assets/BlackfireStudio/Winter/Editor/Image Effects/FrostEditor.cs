using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Frost))]
public class FrostEditor : Editor {
	
	SerializedObject	serializedObj;
	
	SerializedProperty	shader;
	Shader				s;
	List<string>		properties = new List<string>();
	
	SerializedProperty	color;
	
	SerializedProperty	diffuseTex;
	SerializedProperty	bumpTex;
	SerializedProperty	coverageTex;
	
	SerializedProperty	transparency;
	SerializedProperty	refraction;
	SerializedProperty	coverage;
	SerializedProperty	smooth;
	
	
	void OnEnable()
	{
		serializedObj = new SerializedObject(target);
		
		shader = serializedObj.FindProperty("shader");
		
		color = serializedObj.FindProperty("color");
		
		diffuseTex = serializedObj.FindProperty("diffuseTex");
		bumpTex = serializedObj.FindProperty("bumpTex");
		coverageTex = serializedObj.FindProperty("coverageTex");
		
		transparency = serializedObj.FindProperty("transparency");
		refraction = serializedObj.FindProperty("refraction");
		coverage = serializedObj.FindProperty("coverage");
		smooth = serializedObj.FindProperty("smooth");
		
		s = shader.objectReferenceValue as Shader;
		RegisterShaderProperties(s);
	}
	
	private void RegisterShaderProperties(Shader s)
	{
		for (int i = 0; i < ShaderUtil.GetPropertyCount(s); ++i)
		{
			properties.Add(ShaderUtil.GetPropertyName(s, i));
		}
	}
	
	private void GUIShaderRange(string p, SerializedProperty sp)
	{
		float leftValue = ShaderUtil.GetRangeLimits(s, properties.IndexOf(p), 1);
		float rightValue = ShaderUtil.GetRangeLimits(s, properties.IndexOf(p), 2);
		EditorGUILayout.Slider(sp, leftValue, rightValue); 
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeControls();
		
		serializedObj.Update();
		
		EditorGUILayout.PropertyField(shader, new GUIContent("Shader"));
		
		EditorGUILayout.PropertyField(color, new GUIContent("Color (RGB) Screen (A)"));
    	EditorGUILayout.PropertyField(diffuseTex, new GUIContent("Diffuse (RGBA)"));
		EditorGUILayout.PropertyField(bumpTex, new GUIContent("Normal (RGB)"));
		EditorGUILayout.PropertyField(coverageTex, new GUIContent("Coverage (R)"));
		
		GUIShaderRange("_Transparency", transparency);
		GUIShaderRange("_Refraction", refraction);
		GUIShaderRange("_Coverage", coverage);
		GUIShaderRange("_Smooth", smooth);
		
		serializedObj.ApplyModifiedProperties();
	}
}