using UnityEngine;
using System.Collections;

// The Disable Audio script is used to disable the audio.. It is used on the Main Menu for dramatic effect.
[RequireComponent (typeof (AudioSource))]
public class DisableAudio : MonoBehaviour 
{

	void Start () 
	{
		// If on the Main Menu then disable the audio.
		if(Application.loadedLevel == 0)
			this.GetComponent<AudioSource>().enabled = false;
	}

}
