using UnityEngine;
using System.Collections;

public class DeftClientServerHandler : MonoBehaviour
{
  public enum DeftNetworkRole { SEARCHER, CLIENT, WILLHOST, UNASSIGNED, HOST };
  public DeftNetworkRole currentRole = DeftNetworkRole.UNASSIGNED;

  public bool LAN = true;
  public bool spawnPlayer = false;
  public int numberConnections = 4;
  public int port = 2500;

  public string gameName = "4f5re3";
  public string roomName = "15e6fs";
  public string serverAddress = "127.0.0.1";

  HostData[] hostdata = new HostData[0];

  void Start()
  {
    Application.runInBackground = true;
  }

  void OnGUI()
  {
    if (!Network.isClient && !Network.isServer)
    {
      if (GUI.Button(new Rect(10, 10, 150, 50), "Join Server"))
      {
        this.currentRole = DeftNetworkRole.SEARCHER;
      }
      if (GUI.Button(new Rect(10, 70, 150, 50), "Host Server"))
      {
        this.currentRole = DeftNetworkRole.WILLHOST;
      }
      if (this.currentRole == DeftNetworkRole.SEARCHER)
      {
        for (int i = 0; i < hostdata.Length; i++)
        {
          if (GUI.Button(new Rect(160, 10 + 50 * i, 150, 50), hostdata[i].gameName))
          {
            Network.Connect(this.hostdata[i]);
          }
        }
      }
    }
  }

  void Update()
  {
    switch (this.currentRole)
    {
      case DeftNetworkRole.WILLHOST:
        Network.InitializeServer(this.numberConnections, this.port, !this.LAN);
        MasterServer.RegisterHost(this.gameName, this.roomName);
        break;
      case DeftNetworkRole.SEARCHER:
        MasterServer.RequestHostList(this.gameName);
        hostdata = MasterServer.PollHostList();
        if (hostdata.Length > 0)
        {
          //Debug.Log("Connecting to " + hostdata[0].ToString());
          //Network.Connect(hostdata[0]);
        }
        break;
    }
  }


  void OnFailedToConnect(NetworkConnectionError error)
  {
    Debug.Log("Could not connect to server: " + error);
  }

  void OnServerInitialized()
  {
    Debug.Log("Successfully initialized server.");
    this.currentRole = DeftNetworkRole.HOST;
    if (spawnPlayer)
    {
      this.GetComponent<PlayerSelect>().selectedPlayer = this.GetComponent<PlayerSelect>().playerTypeA;
      this.GetComponent<PlayerSelect>().SpawnPlayer();
    }
  }

  void OnConnectedToServer()
  {
    Debug.Log("Successfully connected to server.");
    this.currentRole = DeftNetworkRole.CLIENT;
    if (spawnPlayer)
    {
      this.GetComponent<PlayerSelect>().selectedPlayer = this.GetComponent<PlayerSelect>().playerTypeB;
      this.GetComponent<PlayerSelect>().SpawnPlayer();
    }
  }
}
