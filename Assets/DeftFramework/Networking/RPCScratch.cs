using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class RPCScratch : MonoBehaviour
{

    [StructLayout(LayoutKind.Sequential)]
    struct RPCBodyState
    {
        public double timestamp;
        public Vector3 pos;
        public Vector3 velocity;
        public Quaternion rot;
        public Vector3 angularVelocity;
    }

    byte[] MarshalBodyState(RPCBodyState state)
    {
        int size = Marshal.SizeOf(state);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(state, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    RPCBodyState UnMarshalBodyState(byte[] arr)
    {
        RPCBodyState state = new RPCBodyState();
        int size = Marshal.SizeOf(state);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);
        state = (RPCBodyState)Marshal.PtrToStructure(ptr, state.GetType());
        Marshal.FreeHGlobal(ptr);
        return state;
    }

    public int i;

    [RPC]
    void setI(int newI)
    {
        this.i = newI;
        Debug.Log("Set i to " + this.i);
    }

    [RPC]
    void ReceiveBodyState(byte[] bytes)
    {
        RPCBodyState state = UnMarshalBodyState(bytes);
        Debug.Log("Received state with stamp " + state.timestamp + " and angular velocity " + state.angularVelocity);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        {
            if (Network.isServer)
            {
                RPCBodyState state = new RPCBodyState();
                state.timestamp = i;
                state.angularVelocity = new Vector3(0f, 1.2f, 4f);
                byte[] bytes = MarshalBodyState(state);
                this.GetComponent<NetworkView>().RPC("ReceiveBodyState", RPCMode.Others, bytes);
            }
        }
        i++;
    }
}
