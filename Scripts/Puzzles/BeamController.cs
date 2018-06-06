using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// The BeamController script is the manager script of the beam tower.
public class BeamController : MonoBehaviour
{
	// (optional) The event to be invoked when powered.
	[SerializeField] private UnityEvent OnPowered;

	// A reference to the particle system that represents the beam.
	[SerializeField] private ParticleSystem _Beam;

	// (optional) If true then the beam is automatically active and cannot be deactivated. 
	[SerializeField] private bool constantlyOn;

	// (optional) If true then an "action" will be performed (an animation or event). 
	[SerializeField] private bool _PerformAction = false;

	// (optional) Animation to be played when powered.
	[SerializeField] private Animation _ActionAnimation = null;

	void Awake()
	{
		if (constantlyOn)
			ActivateBeam ();
	}

	public void ActivateBeam()
	{
		// Perform the action if there is one.
		if (_PerformAction)
			PerformAction();

		// Activate beam if not already active.
		if (_Beam) 
		{
			if (_Beam.gameObject.activeSelf)
				return;
		}
		else
			return;

		_Beam.gameObject.SetActive (true);
	}

	public void DeactivateBeam()
	{
		if (constantlyOn)
			return;

		if (_Beam == null)
			return;

		// Deactivate the beam.
		_Beam.GetComponent<BeamCollision> ().Stop ();
	
		_Beam.gameObject.SetActive (false);
	}

	public void PerformAction()
	{
		// If there is an animation, play it.
		if (_ActionAnimation)
			_ActionAnimation.Play();

		// If there is an event to be invoked, invoke it.
		if(OnPowered != null)
			OnPowered.Invoke();

		_PerformAction = false;
	}
}
