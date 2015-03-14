using UnityEngine;
using System.Collections;

public class PlayerControllerNetworkSetup : MonoBehaviour
{
  enum NetworkRole { SEARCHING, HOST, CLIENT }

  const string typeName = "DeftNetwork";
  const string gameName = "12345648";
  NetworkRole role;

  void Start()
  {
    Application.runInBackground = true;
    if (System.Environment.CommandLine.Contains("host"))
    {
      this.role = NetworkRole.HOST;
    }
    else if (System.Environment.CommandLine.Contains("client"))
    {
      this.role = NetworkRole.SEARCHING;
    }
  }

  void OnGUI()
  {
    if (!Network.isClient && !Network.isServer)
    {
      if (GUI.Button(new Rect(100, 100, 300, 100), "Start Server"))
      {
        this.role = NetworkRole.HOST;
      }
      if (GUI.Button(new Rect(100, 250, 300, 100), "Refresh Hosts to Join"))
      {
        this.role = NetworkRole.SEARCHING;
        MasterServer.RequestHostList(typeName);
      }
    }
  }

  private void HostServer()
  {

  }

  void OnServerInitialized()
  {
    GameObject.Find("MapMaker").GetComponent<MakeMap>().PlaceCubes();
    GameObject.Find("RigidBodyManager").GetComponent<RigidBodyManager>().ResetTrackedObjects();
  }

  void FixedUpdate()
  {
    if (!Network.isClient && !Network.isServer)
    {
      if (this.role == NetworkRole.HOST)
      {
        Network.InitializeServer(16, 25000, false);
        MasterServer.RegisterHost(typeName, gameName);
      }
      if (this.role == NetworkRole.SEARCHING && MasterServer.PollHostList().Length > 0)
      {
        HostData[] hostList = MasterServer.PollHostList();
        if (hostList != null)
          for (int i = 0; i < hostList.Length; i++)
          {
            Network.Connect(hostList[i]);
          }
      }
    }
  }


  void OnConnectedToServer()
  {
    this.GetComponent<PlayerSelect>().SpawnPlayer();
  }

}
