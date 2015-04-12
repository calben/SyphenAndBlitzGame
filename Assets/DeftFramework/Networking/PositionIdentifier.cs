using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class PositionIdentifier : MonoBehaviour
{

  public string MakeIdentifierForObject(GameObject obj)
  {
    string x = obj.transform.position.x.ToString("F2");
    string y = obj.transform.position.y.ToString("F2");
    string z = obj.transform.position.z.ToString("F2");
    string result = x + ":" + y + ":" + z;
    return result;
  }

}
