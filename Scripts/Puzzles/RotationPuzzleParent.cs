using UnityEngine;
using System.Collections;

// The RotationPuzzleParent script deals with the puzzle logic of the Rotation Puzzle.
public class RotationPuzzleParent : MonoBehaviour 
{
	// The sequence to be achieved.
	[SerializeField] private int[] Sequence= new int[3];

	// The attempted sequence made by the player.
	[SerializeField] private int[] attempt = new int[3];

	// Reference to an animation that will be played upon completion.
	[SerializeField] private Animation PlayAnim;


	void Awake () 
	{
		// Generate the sequence.
		GenerateSequence ();
		// By slim chance, the sequence may have already been solved.
		CheckSequence ();
	}
	

	private void GenerateSequence()
	{
		// Generate a random sequence.
		for(int i = 0; i < Sequence.Length; i++)
			Sequence[i] = Random.Range(0, 4 );
	}

	private void CheckSequence()
	{
		bool correct = true;
		// Compare the sequence and the attemted sequence for correctness..
		for(int i = 0; i < Sequence.Length; i++)
		{
			if(Sequence[i] != attempt[i])
				correct = false;
		}

		// If the attempted sequence is correct then play the animation (open door).
		if(correct) 
		{
			if(PlayAnim)
				PlayAnim.Play();
		}
	}

	// Updates the attempted sequence depending on the piece (inner, middle, outer) and value (North, East, South, West).
	public void Rotate(int piece, int val)
	{
		attempt [piece] = val;
		// Check to see if the sequence is right.
		CheckSequence ();
	}
	
	public int[] GetSequence()
	{
		return Sequence;
	}

}
