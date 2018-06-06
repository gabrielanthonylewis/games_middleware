using UnityEngine;
using System.Collections;

// The AI script provides all of the non-weapon behaviours such as looking at the target,
// following a Roam Path or even charging towards the player depending on what is required.
public class AI : MonoBehaviour 
{
	// Reference to the Player/Target
	[SerializeField] private GameObject Player = null;

	// Tag of the Target (so that AI could potentially fight other AI/"Freindlys")
	[SerializeField] private string TargetTag = "Player";

	// (Optional) If true signifies that the AI will proceed towards the player once spotted.
	[SerializeField] private bool isRoamer = false;

	// (Optional) If true signifies that the AI has a roam path (Point A & B) which it will follow.
	[SerializeField] private bool hasRoamPath = false;

	// (Optional) If true signifies that the AI Roam Path is vertical so the y axis will be modified.
	[SerializeField] private bool verticalRoamPath = false;

	// Reference to Point A
	[SerializeField] private GameObject _PointA = null;

	// Reference to Point B
	[SerializeField] private GameObject _PointB = null;

	// Keeps track of which point the AI will/is moving towards.
	[SerializeField] private bool goingB = true;
	
	// If true the AI object will face the player.
	[SerializeField] bool LookAtTarget = true;

	// Reference to AIWeaponController component.
	private AIWeaponController _AIWeaponController = null;

	// There could potentially be more than one target (store in this variable)
	private GameObject[] targets;
	
	void Start()
	{
		// If a Player target hasn't been set then find one.
		if(Player == null)
			Player = GameObject.FindGameObjectWithTag(TargetTag);

		// Get AIWeaponController component.
		_AIWeaponController = this.GetComponentInChildren<AIWeaponController> ();
	}

	void Update () 
	{
		// If the AI has a Roam Path move towards the correct point (e.g. A to B)
		if(hasRoamPath)
		{
			if(goingB)
			{
				if(!_PointB)
					return;
				
				// New position to move towards..
				Vector3 bPos = Vector3.MoveTowards(transform.position, _PointB.transform.position, Time.deltaTime);
				
				// If there isn't a vertical roam path then there is no need to adjust the Y axis.
				if(!verticalRoamPath)
					bPos.y = this.transform.position.y;
				
				this.transform.position = bPos;
				
				// If at/close enough to Point B, head to Point A.
				if((this.transform.position.x - _PointB.transform.position.x) > -0.2f && (this.transform.position.z - _PointB.transform.position.z) < 0.5f)
					goingB = false;
			}
			else
			{
				if(!_PointA)
					return;
				
				// New position to move towards..
				Vector3 aPos = Vector3.MoveTowards(transform.position, _PointA.transform.position, Time.deltaTime);
				
				// If there isn't a vertical roam path then there is no need to adjust the Y axis.
				if(!verticalRoamPath)
					aPos.y = this.transform.position.y;
				
				this.transform.position = aPos;
				
				
				// If at/close enough to Point A, head to Point B.
				if((this.transform.position.x - _PointA.transform.position.x) < 0.2f && (this.transform.position.z - _PointA.transform.position.z) < 0.5f)
					goingB = true;
			}
		}

		// If the Player/Target isn't set, get the new target.
		if (Player == null) 
		{

			if(this.transform.GetChild(0).GetComponent<AIWeaponController>())
				Player = this.transform.GetChild(0).GetComponent<AIWeaponController>().GetTarget();

			return;
		}

		// Look towards the target (but preventing the body rotating upwards and downwards) 
		if (LookAtTarget)
		{
			Vector3 targetPos = Player.transform.position;
			targetPos.y = this.transform.position.y;
			this.transform.LookAt (targetPos, transform.up);
		}

		// If the AI is a roamer/charger move towards the enemy.
		if (isRoamer) 
		{
			// Move towards player if spotted.
			if(_AIWeaponController && _AIWeaponController.GetSpottedPlayer())
				this.transform.position = Vector3.MoveTowards (transform.position, Player.transform.position, Time.deltaTime);
		}



	}
}
