using UnityEngine;
using System.Collections;

// The Door script provides functionallity for switching levels.
public class Door : MonoBehaviour 
{
	// The level ID that will be switched to.
	[SerializeField] private int _levelID;

	// If the Door is locked it cannot be entered.
	[SerializeField] public bool _isLocked = false;

	// Load level depending on the _levelID (if the door is unlocked).
	public void Enter()
	{
		if (_isLocked) 
			return;

		Application.LoadLevel (_levelID);
	}

	// Load the next level (if the door is unlocked).
	public void ToNextLevel()
	{
		if (_isLocked) 
			return;

		int currI = Application.loadedLevel;
		Application.LoadLevel (currI + 1);
	}
	
	public void SetIsLocked(bool locked)
	{
		_isLocked = locked;
	}
}
