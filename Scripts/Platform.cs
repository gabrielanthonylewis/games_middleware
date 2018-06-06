using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour 
{
	// Initial point's transform component.
	[SerializeField] private Transform pointA = null;

	// Final point's transform component.
	[SerializeField] private Transform pointB = null;
	
	// (Optional) "Door" to be opened upon arival.
	[SerializeField] private GameObject Door = null;

	// Speed reduction to slow the speed of the platform if need be.
	[SerializeField] private float _speedReduction = 1f;

	// Possible directions.
	private enum Direction { IDLE, FORWARDS, BACKWARDS };

	// The current direction.
	private Direction _currentDirection = Direction.IDLE;

	// The Platform's position provided by LateUpdate().
	private Vector3 _latePos = Vector3.zero;

	void Update()
	{
		switch (_currentDirection) 
		{
			case Direction.IDLE:
				// Don't move.
				return;
					
			case Direction.FORWARDS:
				// Lerp towards point B (position and rotation).
				this.transform.position = Vector3.Lerp(this.transform.position, pointB.position, (Time.deltaTime / (4f * _speedReduction)));
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, pointB.rotation, Time.deltaTime / 6f);
				break;
					
			case Direction.BACKWARDS:
				// Lerp towards point A (position and rotation).
				this.transform.position = Vector3.Lerp(this.transform.position, pointA.position, Time.deltaTime / (4f * _speedReduction));
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, pointA.rotation, Time.deltaTime / 6f);
				break;
		}

		if (_currentDirection != Direction.IDLE) 
		{
			// Calculate the difference between the current position and the late position.
			Vector3 posDiff = (this.transform.position - _latePos);

			// If the platform has reached its destination...
			if(_currentDirection == Direction.FORWARDS && posDiff.z < 0.001f && _currentDirection == Direction.BACKWARDS && posDiff.z > -0.001f)
			{
				// "Open" Door if present.
				if(Door)
					Door.SetActive(false);

				// Stop the platform.
				_currentDirection = Direction.IDLE;
				return;
			}
		}
	}
	
	void LateUpdate()
	{
		_latePos = this.transform.position;
	}

	// Upon activation, switch to the next state (Forwards/Backwards).
	public void Activate()
	{
		switch (_currentDirection) 
		{
			case Direction.IDLE:
				_latePos = Vector3.zero;
				_currentDirection = Direction.FORWARDS;
				break;

			case Direction.FORWARDS:
				_latePos = Vector3.zero;
				_currentDirection = Direction.BACKWARDS;
				break;

			case Direction.BACKWARDS:
				_latePos = Vector3.zero;
				_currentDirection = Direction.FORWARDS;
				break;
		}
	}

	// To prevent the player from falling straight off the platform, set it's parent to the platform (temp)
	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") 
			other.transform.SetParent(this.transform);
	}

	// On exit, remove the player object's parent transform (returning the object to it's original state). 
	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") 
			other.transform.parent = null;
	}
}
