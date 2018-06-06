using UnityEngine;
using System.Collections;

// The PauseMenu script provides fuctionallity to activate/deactive it so the player can use the buttons on it.
// The game is paused by setting the timescale to 0.
public class PauseMenu : MonoBehaviour 
{
	//  Reference to the Pause Menu
	[SerializeField] private GameObject _PauseMenu = null;

	// Tracks whether or not the Pause Menu is being shown.
	[SerializeField] private bool _isShowing = false;

	void Awake()
	{
		if (!_PauseMenu)
		{
			_PauseMenu = GameObject.FindGameObjectWithTag("UI_PauseMenu");
		}
	}

	void Start()
	{
		// If the pause menu shouldn't be showing, hide it.
		if(_isShowing == false)
			this.SetisShowing(false);
	}

	void Update () 
	{
		// When "Escape" is pressed, show/hide the Pause Menu (depending on it's current state).
		if(Input.GetKeyDown(KeyCode.Escape))
			this.SetisShowing(!this.GetisShowing());
	}
	
	public bool GetisShowing()
	{
		return _isShowing;		
	}
	
	public void SetisShowing(bool show)
	{
		_PauseMenu.SetActive(show);

		// Freeze time if showing, resume time if not showing.
		Time.timeScale = System.Convert.ToInt32(!show);

		// Show/Hide Cursor depending on whether the PauseMenu is being shown or not.
		Screen.showCursor = show;

		_isShowing = show;
	}

	// OnDestruction of the Pause menu, resume time.
	void OnDestroy()
	{
		Time.timeScale = 1f;
	}
}
