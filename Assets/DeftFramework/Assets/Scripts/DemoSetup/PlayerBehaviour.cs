using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        turn = 1.0f;
    }

    float turn;

    // Update is called once per frame
    void FixedUpdate()
    {
        this.rigidbody.AddForce(this.transform.forward * 50.0f);
        if (turn > 0)
        {
            turn -= Time.deltaTime;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(this.transform.rotation, Random.rotation, 1.0f);
            turn = 1.0f;
        }
    }
}
