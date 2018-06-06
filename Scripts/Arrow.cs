using UnityEngine;
using System.Collections;

// The Arrow script deal with the projectile behaviour of an arrow,
// stopping when it hits an object and dealing damage.
public class Arrow : MonoBehaviour {
	
	// Reference to Rigidbody component (optimisation)
	private Rigidbody _Rigidbody = null;

	void Start()
	{
		// Assign reference to Rigidbody component.
		_Rigidbody = this.GetComponent<Rigidbody> ();
	}

	void OnTriggerEnter(Collider other)
	{
		// Stick the arrow in the hit object.
		_Rigidbody.isKinematic = true;
		_Rigidbody.useGravity = false;
			
		// Deal 5 damage to the object if it's destructable.
		if (other.GetComponent<Destructable> ()) 
		{
			other.GetComponent<Destructable> ().ManipulateHealth (5f);
			
			Vector3 pos = this.transform.position;
		//	this.transform.SetParent (other.transform);
			this.transform.position = pos;
		}

		if (other.transform.localScale == Vector3.one)
			this.transform.SetParent (other.transform);

		// Turn off the collider.
		this.GetComponent<Collider> ().enabled = false;
	}
}
