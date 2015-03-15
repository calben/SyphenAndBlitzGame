using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum PlayerCharacter {  Syphen, Blitz };

public class GameManager : MonoBehaviour
{
  public GameObject DeftClientServer;
  public int[] playerCurrentHealth;
  public int[] playerTotalHealth;
  public int numbersOfDepots;
  public int[] depotCapacity;
  public int[] depotCurrentStock;
  public int[] depotResourceValue;
  public GameObject[] depots;
  public int killerAttackDamage;
  public int feederAttackDamage;
  public GameObject[] depotUI_objects;
  public GameObject eventSystemObject;
  public GameObject gameOverFirstSelected;
  public GameObject gameOverWindow;
  public GameObject winText;
  public GameObject loseText;
  public GameObject SyphenPowerUnlock;
  public GameObject BlitzPowerUnlock;

  private int depotsFull;
  private EventSystem eventSystem;

  void Start()
  {
    gameOverWindow.SetActive(false);
	SyphenPowerUnlock.SetActive (false);
	BlitzPowerUnlock.SetActive(false);
    winText.SetActive(false);
    loseText.SetActive(false);
    eventSystem = eventSystemObject.GetComponent<EventSystem>();
    depotsFull = 0;
    //Disable all depots except the first one
    foreach (GameObject d in depots)
    {
      d.SetActive(false);
    }
    foreach (GameObject d in depotUI_objects)
    {
      d.SetActive(false);
    }
    depots[0].SetActive(true);
    depotUI_objects[0].SetActive(true);
  }
  public void StartHealthBar(string name) {
		GameObject hb = GameObject.Find ("HealthBar");
		GameObject hs = GameObject.Find ("HealthStats");
		if (name.Contains ("Syphen")) {
			hb.GetComponent<HealthBar> ().StartHealthBar (0);
			hs.GetComponent<HealthStats> ().StartStats (0);
		} else {
			hb.GetComponent<HealthBar> ().StartHealthBar (1);
			hs.GetComponent<HealthStats> ().StartStats (1);
		}
  }
  public void StartGrenadeBar() {
		GameObject.Find ("GrenadeBar").GetComponent<GrenadeBar>().StartGrenadeUI();
  }

  public void decreaseHealth(string targetPlayerName, string attackerName)
  {
    int damage;
    if (attackerName.Contains("Feeder"))
    {
      damage = feederAttackDamage;
    }
    else
    {
      damage = killerAttackDamage;
    }
    if (targetPlayerName.Contains("Syphen"))
    {
      playerCurrentHealth[0] -= damage;
    }
    else
    {
      playerCurrentHealth[1] -= damage;
    }
    if (playerCurrentHealth[0] <= 0 || playerCurrentHealth[1] <= 0)
    {
      gameOver();
    }
  }

  //Increases resource count of a certain depot
  public void increaseResourceCount(int depotNumber)
  {
		if (depotCurrentStock[depotNumber] < depotCapacity[depotNumber]) {
			depotCurrentStock[depotNumber] += depotResourceValue[depotNumber];
			if (depotCurrentStock [depotNumber] >= depotCapacity[depotNumber]) {
				depotFull ();
				Debug.Log ("called DepotFull()");
			}
		}
  }
  public void depotFull()
  {
    Debug.Log("A Depot is FULL");
    depotsFull++;
    if (depotsFull == numbersOfDepots)
    {
      lastDepotFull();
    }
    else
    {
		//Activate the Power Unlock window 
		string name = DeftClientServer.GetComponent<PlayerSelect>().selectedPlayer.name;
		if (name.Contains("Blitz")) {
			BlitzPowerUnlock.SetActive(true);
		} else {
			SyphenPowerUnlock.SetActive(true);
		}
    }
  }
  public void activateNextDepot() 
  {
	//Activate next depot
	depots[depotsFull].SetActive(true);
	depotUI_objects[depotsFull].SetActive(true);
  }
  public void lastDepotFull()
  {
    Debug.Log("YOU WIN.");
    gameOverWindow.SetActive(true);
    winText.SetActive(true);
    eventSystem.SetSelectedGameObject(gameOverFirstSelected);
  }
  public void gameOver()
  {
    Debug.Log("YOU DIED.");
    gameOverWindow.SetActive(true);
    loseText.SetActive(true);
    eventSystem.SetSelectedGameObject(gameOverFirstSelected);
  }


}
