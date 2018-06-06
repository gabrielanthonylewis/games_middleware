using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OnDeathBossEvent : MonoBehaviour 
{
	// (optional) Animation to be played upon death.
	[SerializeField] private Animation _EventAnimation = null;

	// (optional) UI text displaying the time left.
	[SerializeField] private Text _TimerUIText = null;

	// (optional) GameObjects that will be set active.
	[SerializeField] private GameObject _ObjectToSetActive = null;

	// The total number of seconds the player has to escape.
	[SerializeField] private int _TotalSeconds = 90;

	public void InvokeEvent()
	{
		if(_EventAnimation)
			_EventAnimation.Play ();

		if (_ObjectToSetActive)
			_ObjectToSetActive.SetActive (true);

		StartCoroutine ("Timer");
	}

	IEnumerator Timer()
	{
		// If there is a timer text element then enable it.
		if (_TimerUIText)
		{
			if(_TimerUIText.transform.parent)
				_TimerUIText.transform.parent.gameObject.SetActive(true);
		}

		int currentSeconds = _TotalSeconds;

		while(_TotalSeconds-- > 0)
		{
			// Wait one second and then reduce the current seconds by one.
			yield return new WaitForSeconds (1);
			currentSeconds--;

			// Calculate the equivelent minutes and seconds.
			int minutes = currentSeconds / 60;
			int seconds = currentSeconds - minutes * 60;

			// If the minutes is < 10 then add a 0 before the number of minutes.
			string minExtra = "";
			if (minutes < 10)
				minExtra = "0";

			// If the seconds is < 10 then add a 0 before the number of seconds.
			string secExtra = "";
			if (seconds < 10)
				secExtra = "0";

			// Display the remaining number of minutes and seconds.
			_TimerUIText.text = minExtra + minutes.ToString() + ":" + secExtra + seconds.ToString();
		}
	}
}
