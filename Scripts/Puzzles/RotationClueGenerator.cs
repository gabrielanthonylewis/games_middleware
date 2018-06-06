using UnityEngine;
using System.Collections;

// The RotationClueGenerator script is used to generate the correct 
// colours and sizes for the clues to help the player complete the Rotation Puzzle.
public class RotationClueGenerator : MonoBehaviour 
{
	// Reference to all of the Inner piece prefabs.
	[SerializeField] private GameObject[] InnerPrefabs;

	// Reference to all of the Middle piece prefabs.
	[SerializeField] private GameObject[] MiddlePrefabs;

	// Reference to all of the Outer piece prefabs.
	[SerializeField] private GameObject[] OuterPrefabs;
	
	private enum Ring { Inner, Middle, Outer };

	// Which ring the clue should emulate.
	[SerializeField] private Ring _Ring = Ring.Inner;

	// Reference to the RotationPuzzleParent (puzzle manager).
	[SerializeField] private RotationPuzzleParent _RotPuzzPar = null;


	void Start () 
	{
		if (!_RotPuzzPar)
			return;


		GameObject tempPrefab = null;

		// Assign the tempPrefab to the correct prefab depending on the ring piece to emulate.
		switch (_Ring) 
		{
			case Ring.Inner:
				tempPrefab = InnerPrefabs [_RotPuzzPar.GetSequence () [0]];
				break;

			case Ring.Middle:
				tempPrefab = MiddlePrefabs [_RotPuzzPar.GetSequence () [1]];
				break;

			case Ring.Outer:
				tempPrefab = OuterPrefabs [_RotPuzzPar.GetSequence () [2]];
				break;
		}

		// Change the colour and scale to match that of the correct ring piece.
		this.transform.localScale = tempPrefab.transform.lossyScale;
		this.transform.GetComponent<MeshRenderer> ().material = tempPrefab.GetComponent<MeshRenderer> ().material;
	}

}
