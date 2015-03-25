//From the tutorial: http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	//Server creation
	private const string _projectName = "4f5re3"; //Should be unique to this project
	private const string _roomName = "15e6fs"; //Can be any name (possible suggestion: player name)
	private const int _maxPlayers = 4;
	private const int _portNumber = 2500;

	//Server joining
	private HostData[] _hostList;
	private bool _refreshHostList = false;
	public GameObject _joinAvailablePanel;		//The panel which becomes the parent of the buttons.
	public GameObject _joinButtonPrefab;		//The prefab of the button to create instances of.
	public GameObject _currentMenu;				//The menu that this button lives on.
	public GameObject _connectedMenu;			//The menu that the player is redirected to after a successful connection.
	private List<GameObject> _sessionButtons = new List<GameObject>();

// --- INTERFACE CREATION --- //
	#region CREATE INTERFACE

	/*
	//Create a GUI display
	void OnGUI()
	{
		//If this game instance has not connect to a server nor created one yet
		if(!Network.isClient && !Network.isServer)
		{
			if(GUI.Button(new Rect(100,100,250,100), "Start Server")){
				StartServer();
			}
			if(GUI.Button(new Rect(100,250,250,100), "Refresh Hosts")){
				RefreshHostList();
			}
			if(hostList != null){
				for(int i = 0; i < hostList.Length; ++i){
					if(GUI.Button(new Rect(400,100+(110*i),300,100), hostList[i].gameName)){
						JoinServer(hostList[i]);
					}
				}
			}
		}
	}
	*/

	void Start(){

	}

	void Update(){
		if(_refreshHostList){
			RefreshHostList();
			if(_hostList != null){
				int buttonOffset = -40;
				for(int i = 0; i < _hostList.Length; ++i){
					GameObject sessionButton = (GameObject)Instantiate(_joinButtonPrefab, _joinButtonPrefab.GetComponent<RectTransform>().position, Quaternion.identity);
					sessionButton.transform.SetParent( _joinAvailablePanel.transform );
					sessionButton.GetComponent<RectTransform>().anchoredPosition3D = _joinButtonPrefab.GetComponent<RectTransform>().anchoredPosition3D + (Vector3.up * buttonOffset * i);
					Button b = sessionButton.GetComponent<Button>();
					b.onClick.AddListener(() => {
						JoinServer(_hostList[0]); 
						_currentMenu.SetActive(false);
						_connectedMenu.SetActive(true);
						_refreshHostList = false;
					});
					Text t = sessionButton.transform.GetChild(0).GetComponent<Text>();
					t.text = _hostList[i].gameName;
					_sessionButtons.Add ( sessionButton );
//					if(GUI.Button(new Rect(400,100+(110*i),300,100), _hostList[i].gameName)){
//						JoinServer(_hostList[i]);
//					}
				}
				_refreshHostList = false;
			}
		}
	}

	public void StartRefreshingHostList(){
		_refreshHostList = true;
	}

	public void StopRefreshingHostList(){
		_refreshHostList = false;
	}

	#endregion CREATE INTERFACE

	#region CREATE SERVER

	//Create a server and register it to the Master Server
	public void StartServer(){
		if(!Network.isClient && !Network.isServer){
			Network.InitializeServer(_maxPlayers, _portNumber, !Network.HavePublicAddress());
			MasterServer.RegisterHost(_projectName, _roomName, "This is a comment");
		}
	}

	//Confirmation that the server has indeed been created.
	void OnServerInitialized(){
		Debug.Log("Server Initialized");
	}

	#endregion CREATE SERVER

	#region JOIN SERVER

	//Get the host information that matches the gameTypeName.
	public void RefreshHostList(){
		MasterServer.RequestHostList(_projectName);
		Debug.Log(MasterServer.PollHostList().Length);
	}

	//Event triggered on Master Server
	void OnMasterServerEvent(MasterServerEvent msEvent){
		//Confirm the server was registered
		if(msEvent == MasterServerEvent.RegistrationSucceeded){
			Debug.Log("RegistrationSucceeded");
		}

		Debug.Log("msEvent: " + msEvent.ToString());
		//Obtain data required to join the host server
		if(msEvent == MasterServerEvent.HostListReceived){
			_hostList = MasterServer.PollHostList();
			Debug.Log("HostListReceived");
		}
	}

	//Join the server belonging to the given hostData
	public void JoinServer(HostData hostData){
		Network.Connect(hostData);
		Debug.Log("trying to join " + hostData.gameName);
	}

	//Confirmation that the server has indeed been created.
	void OnConnectedToServer(){
		Debug.Log("Server Joined");
	}

	#endregion JOIN SERVER

	public void Disconnect(){
		if(Network.isServer){
			Network.Disconnect();
			MasterServer.UnregisterHost();
			Debug.Log("Disconnected Server");
		}
		else if(Network.isClient){
			Network.Disconnect();
			Debug.Log("Disconnected from Server");
		}
	}

}
