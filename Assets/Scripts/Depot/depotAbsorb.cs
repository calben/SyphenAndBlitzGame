using UnityEngine;
using System.Collections;

public class depotAbsorb : MonoBehaviour
{

  public int depotNumber;
  public string supplyItemTag;

  public float animGrowRate = 0.1f;
  //Link to game manager to update stats
  public GameObject gameManager;
  public AudioManager _audioManager;

  private GameManager gameStats;
  private bool depotFull;
  private int capacity;
  private int resourceVal;
  private Animator myAnim;


  void Start()
  {
    myAnim = this.GetComponent<Animator>();
    myAnim.SetBool("isStopped", true);
    myAnim.SetBool("isPlaying", false);
    myAnim.speed = 0.0f;
    gameStats = gameManager.GetComponent<GameManager>();
    depotFull = false;
    capacity = gameStats.depotCapacity[depotNumber];
    resourceVal = gameStats.depotResourceValue[depotNumber];
  }

  [RPC]
  void absorbAction(NetworkViewID id)
  {
    if ((gameStats.depotCurrentStock[depotNumber]) < capacity)
    {
      gameStats.increaseResourceCount(depotNumber);
      //Destroy resource
      DeftLayerSyncManager syncManager = GameObject.Find("Layer10SyncManager").GetComponent<DeftLayerSyncManager>();
      GameObject obj = syncManager.objectsInLayer[id];
      Destroy(obj);
      syncManager.objectsInLayer.Remove(id);
      myAnim.SetBool("isPlaying", true);
      myAnim.SetBool("isStopped", false);
      myAnim.Play("depotGrow", 0, myAnim.GetCurrentAnimatorStateInfo(0).length * gameStats.depotCurrentStock[depotNumber] / gameStats.depotCapacity[depotNumber] * animGrowRate);
      _audioManager.Play("depot_absorb", 0.01f, false);
    }
    else if (!depotFull)
    {
      depotFull = true;
    }
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.tag == supplyItemTag && !depotFull)
    {
      this.networkView.RPC("absorbAction", RPCMode.All, other.networkView.viewID);
    }
  }

}
