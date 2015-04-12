using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MakeMap : MonoBehaviour
{

    public GameObject cube;
    public int numberOfCubes;
    public int min, max;
    public string layer;
    public Color cubecolor;

    public void PlaceCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            GameObject c = (GameObject) Network.Instantiate(cube, GeneratedPosition(), Quaternion.identity, 5);
            c.layer = LayerMask.NameToLayer(layer);
        }
    }

    Vector3 GeneratedPosition()
    {
        int x, y, z;
        x = UnityEngine.Random.Range(min, max);
        y = UnityEngine.Random.Range(min, max);
        z = UnityEngine.Random.Range(min, max);
        return new Vector3(x, y, z);
    }

    [RPC]
    public void SyncCubeColor()
    {
        foreach (GameObject obj in GameObject.Find("RigidBodyManager").GetComponent<RigidBodyManager>().objectsToSync)
        {
            obj.GetComponent<Renderer>().material.color = cubecolor;
        }
    }

}
