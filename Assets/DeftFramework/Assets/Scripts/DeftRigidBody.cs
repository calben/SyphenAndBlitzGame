using UnityEngine;
using System.Collections;

public class DeftRigidBody : MonoBehaviour
{

  public enum DeftRigidBodyType { PLAYER, PHYSICSOBJECT };

  public DeftRigidBodyType type;
  public double m_InterpolationBackTime = 0.1;
  public double m_ExtrapolationLimit = 0.5;

  public string controllerScript;

  internal struct State
  {
    internal double timestamp;
    internal Vector3 pos;
    internal Vector3 velocity;
    internal Quaternion rot;
    internal Vector3 angularVelocity;
  }

  void OnNetworkInstantiate(NetworkMessageInfo msg)
  {
    Debug.Log("Instantiating deft rigid body of type " + this.type.ToString());
    if (this.type == DeftRigidBodyType.PLAYER)
    {
      if (GetComponent<NetworkView>().isMine)
      {
      }
      else
      {
        if (controllerScript != null)
          Destroy(gameObject.GetComponent(controllerScript));
        // destroy any other controllers or enable/disable them
      }
    }
    if (this.type == DeftRigidBodyType.PHYSICSOBJECT)
    {
      // todo
    }
  }

  State[] m_BufferedState = new State[20];
  int m_TimestampCount;

  void Start()
  {
    foreach (NetworkView n in GetComponents<NetworkView>())
      n.observed = this;
  }

  void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
  {
    if (stream.isWriting)
    {
      Vector3 pos = GetComponent<Rigidbody>().position;
      Quaternion rot = GetComponent<Rigidbody>().rotation;
      Vector3 velocity = GetComponent<Rigidbody>().velocity;
      Vector3 angularVelocity = GetComponent<Rigidbody>().angularVelocity;

      stream.Serialize(ref pos);
      stream.Serialize(ref velocity);
      stream.Serialize(ref rot);
      stream.Serialize(ref angularVelocity);
    }
    else
    {
      Vector3 pos = Vector3.zero;
      Vector3 velocity = Vector3.zero;
      Quaternion rot = Quaternion.identity;
      Vector3 angularVelocity = Vector3.zero;
      stream.Serialize(ref pos);
      stream.Serialize(ref velocity);
      stream.Serialize(ref rot);
      stream.Serialize(ref angularVelocity);

      for (int i = m_BufferedState.Length - 1; i >= 1; i--)
      {
        m_BufferedState[i] = m_BufferedState[i - 1];
      }

      State state;
      state.timestamp = info.timestamp;
      state.pos = pos;
      state.velocity = velocity;
      state.rot = rot;
      state.angularVelocity = angularVelocity;
      m_BufferedState[0] = state;

      m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

      for (int i = 0; i < m_TimestampCount - 1; i++)
        if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
          Debug.Log("State inconsistent");
    }
  }

  void Update()
  {
    // This is the target playback time of the rigid body
    double interpolationTime = Network.time - m_InterpolationBackTime;

    // Use interpolation if the target playback time is present in the buffer
    if (m_BufferedState[0].timestamp > interpolationTime)
    {
      // Go through buffer and find correct state to play back
      for (int i = 0; i < m_TimestampCount; i++)
      {
        if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
        {
          // The state one slot newer (<100ms) than the best playback state
          State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
          // The best playback state (closest to 100 ms old (default time))
          State lhs = m_BufferedState[i];

          // Use the time between the two slots to determine if interpolation is necessary
          double length = rhs.timestamp - lhs.timestamp;
          float t = 0.0f;
          // As the time difference gets closer to 100 ms t gets closer to 1 in 
          // which case rhs is only used
          // Example:
          // Time is 10.000, so sampleTime is 9.900 
          // lhs.time is 9.910 rhs.time is 9.980 length is 0.070
          // t is 9.900 - 9.910 / 0.070 = 0.14. So it uses 14% of rhs, 86% of lhs
          if (length > 0.0001f)
            t = (float)((interpolationTime - lhs.timestamp) / length);

          // if t=0 => lhs is used directly
          transform.localPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
          transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
          return;
        }
      }
      // Use extrapolation
    }
    else
    {
      State latest = m_BufferedState[0];

      float extrapolationLength = (float)(interpolationTime - latest.timestamp);
      // Don't extrapolation for more than 500 ms, you would need to do that carefully
      if (extrapolationLength < m_ExtrapolationLimit)
      {
        float axisLength = extrapolationLength * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
        Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

        GetComponent<Rigidbody>().position = latest.pos + latest.velocity * extrapolationLength;
        GetComponent<Rigidbody>().rotation = angularRotation * latest.rot;
        GetComponent<Rigidbody>().velocity = latest.velocity;
        GetComponent<Rigidbody>().angularVelocity = latest.angularVelocity;
      }
    }
  }

}