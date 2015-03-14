using UnityEngine;
using System.Collections;

public class AI_Mover : MonoBehaviour {


	protected NavMeshAgent agent;
	public Transform waypoint;
	protected bool interested;
	protected Transform prevWaypoint;
	public int health;
	public int damageTaken;


	public bool interest()
	{

		return this.interested;

	}


	public virtual void isInterested() {}

	protected virtual void react() {}


	public Transform getPosition()
	{
		
		return this.transform;
		
	}


	public void updateWaypoint(Transform nextWaypoint)
	{

		this.prevWaypoint = this.waypoint;
		
		this.waypoint = nextWaypoint;

	}
	

	protected void move()
	{

		if(this.waypoint == null)
		{

			return;

		}

		this.agent.SetDestination (Vector3.Lerp (transform.position, this.waypoint.position, 0.5f));
	
	}

}
