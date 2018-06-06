using UnityEngine;
using System.Collections;

// The Pillar script deals with elevation of the pillar by a set amount.
public class Pillar : MonoBehaviour 
{
	// The amout of units to move upwards.
	[SerializeField] private float _ElevationVal = 21.0F;

	// The new position to elevate to.
	private Vector3 _newPos = Vector3.zero;

	// If true the pillar will elevate to the new position.
	private bool _bElevate = false;

	public void Elevate()
	{
		// Set up new position to be itself + the elevation value in the y direction.
		_newPos = this.transform.position;
		_newPos.y += _ElevationVal;

		// Allow elevation...
		_bElevate = true;
	}

	void Update()
	{
		if (!_bElevate)
			return;

		// Elevate.
		this.transform.position = Vector3.Lerp (this.transform.position, _newPos, Time.deltaTime / 15f);
	}
}
