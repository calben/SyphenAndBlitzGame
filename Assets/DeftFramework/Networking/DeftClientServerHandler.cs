using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
	
	public GameObject _joinAvailablePanel;		//The panel which becomes the parent of the buttons.
	public GameObject _joinButtonPrefab;		//The prefab of the button to create instances of.
	private List<GameObject> _sessionButtons = new List<GameObject>();
	private bool _refreshHostList = false;
	
	HostData[] hostdata = new HostData[0];
	
	void Start()
	{
		Application.runInBackground = true;
	}
	
	void OnGUI()
	{
		/*
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
		if(hostdata != null){
			int buttonOffset = -40;
			for(int i = 0; i < hostdata.Length; ++i){
				GameObject sessionButton = (GameObject)Instantiate(_joinButtonPrefab, _joinButtonPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
				sessionButton.transform.SetParent( _joinAvailablePanel.transform );
				sessionButton.GetComponent<RectTransform>().anchoredPosition3D = _joinButtonPrefab.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
				Button b = sessionButton.GetComponent<Button>();
				b.onClick.AddListener(() => {
					Network.Connect(this.hostdata[i]);
					//_refreshHostList = false;
				});
				Text t = sessionButton.transform.GetChild(0).GetComponent<Text>();
				t.text = hostdata[i].gameName;
				_sessionButtons.Add ( sessionButton );
			}
			//_refreshHostList = false;    
		/*
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
*/
	}
	
	public void SetToHost(){
		this.currentRole = DeftNetworkRole.WILLHOST;
	}
	
	public void SetToSearcher(){
		this.currentRole = DeftNetworkRole.SEARCHER;
		_refreshHostList = true;
		_sessionButtons.Clear();
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
				Debug.Log("success 1nce");
				if(_refreshHostList){
					int buttonOffset = -40;
					Debug.Log("success?");
					for(int i = 0; i < hostdata.Length; ++i){
						GameObject sessionButton = (GameObject)Instantiate(_joinButtonPrefab, _joinButtonPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
						sessionButton.transform.SetParent( _joinAvailablePanel.transform );
						sessionButton.GetComponent<RectTransform>().anchoredPosition3D = _joinButtonPrefab.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
						Button b = sessionButton.GetComponent<Button>();
						b.onClick.AddListener(() => {
							Network.Connect(this.hostdata[0]);
							_joinAvailablePanel.SetActive(false);
							_refreshHostList = false;
						});
						Text t = sessionButton.transform.GetChild(0).GetComponent<Text>();
						t.text = hostdata[i].gameName;
						_sessionButtons.Add ( sessionButton );
					}
					_refreshHostList = false;
				}
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
