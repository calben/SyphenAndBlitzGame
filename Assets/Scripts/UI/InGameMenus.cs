using UnityEngine;
using System.Collections;

public class InGameMenus : MonoBehaviour
{

  public string mainMenuSceneName;
  public string gameSceneName;
  public GameObject DeftClientServer;

  public void MainMenu()
  {
    Application.LoadLevel(mainMenuSceneName);
  }

  public void RestartGame()
  {
    GameObject.Find("Layer10SyncManager").GetComponent<DeftLayerSyncManager>().LoadLastSavedState();
	if (GameObject.Find ("GameOverWindow") != null) {
		GameObject.Find ("GameOverWindow").SetActive (false);
	}
	if (GameObject.Find ("PauseWindow") != null) {
		GameObject.Find ("PauseWindow").SetActive (false);
	}
  }

  public void InvertControls()
  {
		if(DeftClientServer){
			string name = DeftClientServer.GetComponent<PlayerSelect>().selectedPlayer.name;
			GameObject player = null;
			foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
			{
				if (obj.name.Contains(name))
				{
					player = obj;
				}
			}
			if (player != null)
			{
				player.GetComponent<RigidbodyNetworkedPlayerController>().inverted = !player.GetComponent<RigidbodyNetworkedPlayerController>().inverted;
			}
		}
		else{
			GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
			if (player[0] != null)
			{
				player[0].GetComponent<RigidbodyNetworkedPlayerController>().inverted = !player[0].GetComponent<RigidbodyNetworkedPlayerController>().inverted;
			}
		}
  }
}
