using UnityEngine;
using System.Collections;

// The FlashingLight script provides an ambient flashing of a light component.
[RequireComponent(typeof (Light))]
public class FlashingLight : MonoBehaviour 
{
	// The speed multiplier of the transition.
	[SerializeField] private float _SpeedMultiplier = 1f;

	// The maximum intensity to transition to.
	[SerializeField] private float _MaxIntensity = 1f;

	// The initial intensity to return back to.
	private float _InitialIntensity = 0f;

	// The amount of range (plus the inital range) to transition to.
	private float _AdditionalRange = 0.5f;

	// The inital range to return back to.
	private float _InitialRange = 0f;

	// Reference to the light component (optimisation).
	private Light _Light = null;

	// When true increase intensity and range;
	// otherwise, decrease back to the intial state.
	private bool _Increase = true;

	void Awake()
	{
		_Light = this.GetComponent<Light> ();

		// Initalisation.
		_InitialIntensity = _Light.intensity;
		_InitialRange = _Light.range;
		_AdditionalRange += _Light.range;
	}

	void Update()
	{
		if (_Increase) 
		{
			// Increase intensity and range of the Light component.
			_Light.intensity = Mathf.Lerp (_Light.intensity, _MaxIntensity, Time.deltaTime * _SpeedMultiplier);
			_Light.range = Mathf.Lerp (_Light.range, _AdditionalRange, Time.deltaTime * _SpeedMultiplier);

			// When the target intensity is reached, stop increasing the intensity and range (and begin decreasing).
			if(_Light.intensity >= _MaxIntensity - 0.01f)
				_Increase = false;
		}
		else 
		{
			// Decrease intensity and range of the Light component.
			_Light.intensity = Mathf.Lerp (_Light.intensity, _InitialIntensity, Time.deltaTime * _SpeedMultiplier);
			_Light.range = Mathf.Lerp (_Light.range, _InitialRange, Time.deltaTime * _SpeedMultiplier);

			// When the initial intensity is reached, stop decreasing the intensity and range (and begin increasing).
			if(_Light.intensity <= _InitialIntensity + 0.01f)
				_Increase = true;
		}
	}

}
