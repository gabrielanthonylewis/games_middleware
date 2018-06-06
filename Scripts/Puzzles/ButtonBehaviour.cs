using UnityEngine;
using System.Collections;

public class ButtonBehaviour : MonoBehaviour 
{
	// (optional) Reference to a Platform component to be activated upon button push.
	[SerializeField] private Platform platform = null;

	public void Push()
	{
		if (this.GetComponent<Animation> ()) 
			this.GetComponent<Animation>().Play();

		if (platform) 
			platform.Activate();
	}
}
