using UnityEngine;
using System.Collections;
using UnityEngine.Events;

// The CinematicCameraEvent script is to be soley used by an animation event. 
public class CinematicCameraEvent : MonoBehaviour
{
	// The event to be invoked.
	[SerializeField] private UnityEvent _Event = null;

	// Invoke the event and destroy the object.
	public void InvokeFunction()
	{
		_Event.Invoke ();
		Destroy (this.gameObject);
	}
}
