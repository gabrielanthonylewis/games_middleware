using UnityEngine;
using System.Collections;

// The Rotatable script provides rotation in any axis.
public class Rotatable : MonoBehaviour 
{
	// The axis to rotate around.
	[SerializeField] private Vector3 axis = Vector3.up;

	// "auto" is where the rotation "snaps" to the next rotation, there is no lerp. 
	[SerializeField] private bool auto = false;

	// When true the object will rotate.
	private bool _Rotate = false;

	// Reference to RotationPuzzlePiece component (optimisation).
	private RotationPuzzlePiece _RotPiece;


	void Start()
	{
		_RotPiece = this.GetComponent<RotationPuzzlePiece> ();
	}

	void Update()
	{
		// Must be ready to rotate and be auto.
		if (!_Rotate || !auto)
			return;

		// Rotate/Snap.
		this.transform.Rotate(0f,0f,-90f, Space.World);

		// If a RotationalPuzzlePiece is present, then "rotate" it.
		if (_RotPiece)
			_RotPiece.Rotate();

		// Disable rotation.
		_Rotate = false;
	}

	public void Rotate()
	{
		// If auto is true then allow rotation and return.
		if (auto) 
		{
			_Rotate = true;
			return;
		}

		// Rotate around axis.
		this.transform.RotateAround (this.transform.position, axis, 50f * Time.deltaTime);
	}

	public bool GetAuto()
	{
		return auto;
	}
}
