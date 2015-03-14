// References: http://docs.unity3d.com/ScriptReference/RaycastHit-normal.html

using UnityEngine;
using System.Collections;

/// <summary>
/// The owner of this script will attach a given object to a surface detected by a Raycast.
/// </summary>
public class attachToSurface : MonoBehaviour {
	public GameObject surfaceObject;
	public LayerMask layerMask; //Layers for raycast to detect.

	public Vector3 positionOffset;
	public float followSmoothing = 1.0f;

	public float rotationMultiplier;
	public Vector3 rotationOffset; 

	public bool attachToTriangle = false;
	public bool spawnObjectOnStart = false;
	
	public struct Triangle{
		public Vector3 position;
		public Vector3 normal;
	} 
	
	private Triangle m_hitTriangle;
	
	// Use this for initialization
	void Start () {
		if(spawnObjectOnStart){
			spawnSurfaceObject();
		}
	}
	
	// Update is called once per frame
	void Update() {
		if( surfaceObject ){
			attach();
		}
	}
	
	void attach(){
		RaycastHit hit; 
		//Check for water surface below transform position
		if ( Physics.Raycast( transform.position, -transform.up, out hit, 500.0f, layerMask ) ) { 
			if(attachToTriangle){
				m_hitTriangle = updateTriangleHit( hit, m_hitTriangle );
				surfaceObject.transform.position = Vector3.Lerp( surfaceObject.transform.position, m_hitTriangle.position + positionOffset, followSmoothing*Time.deltaTime);
				surfaceObject.transform.eulerAngles = m_hitTriangle.normal * rotationMultiplier;
				//Debug.Log("Tri:" + m_hitTriangle.position);
				displaySurfaceRays( hit );
			}
			else{
				surfaceObject.transform.position = Vector3.Lerp( surfaceObject.transform.position, hit.point + positionOffset, followSmoothing*Time.deltaTime);
				surfaceObject.transform.eulerAngles = (hit.normal + rotationOffset)* rotationMultiplier;
				displaySurfaceRays( hit );
			}
		}
	}
	
	void displaySurfaceRays(RaycastHit hit){
		Vector3 incomingVec = hit.point - transform.position;
		Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);
		//Debugging
		Debug.DrawLine(transform.position, hit.point, Color.red); 
		Debug.DrawRay(hit.point, reflectVec, Color.green);
		Debug.DrawRay(hit.point, hit.normal, Color.yellow);
	}
	
	//Reference: http://docs.unity3d.com/ScriptReference/RaycastHit-triangleIndex.html
	Triangle updateTriangleHit( RaycastHit hit, Triangle hitTriangle ){
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null)
			return hitTriangle;
		
		hitTriangle = new Triangle();
		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
		Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
		Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
		Transform hitTransform = hit.collider.transform;
		p0 = hitTransform.TransformPoint(p0);
		p1 = hitTransform.TransformPoint(p1);
		p2 = hitTransform.TransformPoint(p2);
		Debug.DrawLine(p0, p1);
		Debug.DrawLine(p1, p2);
		Debug.DrawLine(p2, p0);
		
		hitTriangle.position = p0;
		hitTriangle.normal = Vector3.Cross(p1 - p0, p2 - p0);
		Debug.DrawRay(hitTriangle.position, hitTriangle.normal);
		return hitTriangle;
	}

	void spawnSurfaceObject(){
		surfaceObject = Instantiate( surfaceObject, transform.position, transform.rotation ) as GameObject;
	}
}
