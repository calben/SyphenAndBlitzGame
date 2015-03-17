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
    Application.LoadLevel(gameSceneName);
    //		GameObject.Find ("GameManager").GetComponent<DeftNetwork> ().enabled = true;
  }

  public void InvertControls()
  {
    string name = DeftClientServer.GetComponent<PlayerSelect>().selectedPlayer.name;
    GameObject player = null;
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
    {
      if (obj.name.Contains(name))
      {
        player = obj;
      }
    }
    if (player != null) {
			player.GetComponent<RigidbodyNetworkedPlayerController> ().inverted = !player.GetComponent<RigidbodyNetworkedPlayerController> ().inverted;
	}
  }
}
