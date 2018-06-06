using UnityEngine;
using System.Collections;

// The BeamCollision script deals with beam chaining upon collision.
public class BeamCollision : MonoBehaviour 
{
	// The length of the ray in units.
	[SerializeField] private float _RayLength = 6.0F;

	// Reference to the hit object's Beamcontroller component.
	private BeamController _HitBeamController;

	void LateUpdate()
	{
		RaycastHit hit;
		if (Physics.Raycast (this.transform.position, this.transform.forward, out hit, _RayLength))
		{
			// If the hit object has a beam controller component...
			if (hit.transform.gameObject.GetComponent<BeamController> ()) 
			{
				// Return if the hit object's children contrain a beam collision component.
				if(hit.transform.gameObject.GetComponentInChildren<BeamCollision>())
					return;

				_HitBeamController = hit.transform.gameObject.GetComponent<BeamController> ();
				// Activate it's beam.
				_HitBeamController.ActivateBeam ();	
			}
		}
		else 
		{
			// If no tower is being hit then stop it.
			Stop();
		}
	}

	public void Stop()
	{
		// Deacivate the hit towers beam.
		if( _HitBeamController)
			_HitBeamController.DeactivateBeam ();

		_HitBeamController = null;
	}
}
