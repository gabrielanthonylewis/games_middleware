using UnityEngine;
using System.Collections;

// The DestroyOnTouch script damages objects with health when they are inside the trigger.
public class DestroyOnTouch : MonoBehaviour 
{

	void OnTriggerStay(Collider other)
	{
		// If the object has an animation component and it's not playing return.
		// (Used in the case of the boss level where damage should only be dealt when the water rises)
		if(this.animation && !animation.isPlaying)
			return;

		// Only affect objects that have health...
		if (!other.GetComponent<Destructable> ())
			return;

		other.gameObject.GetComponent<Destructable> ().ManipulateHealth (50 * Time.deltaTime);
	}
}
