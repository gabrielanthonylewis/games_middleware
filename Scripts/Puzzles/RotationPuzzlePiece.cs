using UnityEngine;
using System.Collections;

// The RotationPuzzlePiece script deals with the clock-wise rotation of the object.
public class RotationPuzzlePiece : MonoBehaviour 
{
	// ID of the piece/ring (0 = inner, 1 = middle, 2 = outer)
	[SerializeField] private int id = -1;

	// Currection direction (0 = North, 1 = East, 2 = South, 3 = West)
	[SerializeField] private int rotation = 0;

	// Reference to the RotationPuzzleParent (manager of the puzzle)
	[SerializeField] RotationPuzzleParent _RotPuzzleParent;

	public void Rotate()
	{
		// Theoretically rotate the piece (increase the rotation).
		rotation++;

		// Reset to North if passed West.
		if(rotation > 3)
			rotation = 0;

		// Call the Rotation on the manager (to deal with puzzle logic)
		_RotPuzzleParent.Rotate (id, rotation);
	}
}
