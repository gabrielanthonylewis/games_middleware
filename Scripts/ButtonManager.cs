using UnityEngine;
using System.Collections;

// The ButtonManager script provides many different functions that will be used upon button press.
// Examples including reloading the level, loading a level and quitting the game.
public class ButtonManager : MonoBehaviour 
{
	
	public void LoadLevel(int levelNo)
	{
		// The Inventory is static throught levels/rooms so only reset when on the main menu.
		if (levelNo == 0)
		{
			// Reset Inventory to inital state.
			Inventory.instance.Initialise ();
		}

		Time.timeScale = 1f;

		// Load level "levelNo", e.g. level 0 = the Main Menu.
		Application.LoadLevel(levelNo);
	}
	
	public void ReloadLevel()
	{
		// Reset Inventory to inital state.
		Inventory.instance.Initialise ();

		Time.timeScale = 1f;

		// Reload the current level.
		Application.LoadLevel(Application.loadedLevel);
	}

	public void Show(GameObject obj)
	{
		obj.SetActive (true);
	}

	public void Hide(GameObject obj)
	{
		obj.SetActive (false);
	}
	

	public void QuitApplication()
	{
		// Exit the game/application.
		Application.Quit();
	}
}
