using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// The SequenceController script is the manager script of the sequence puzzle.
public class SequenceController : MonoBehaviour 
{
	// Event to be Invoked upon completion of the puzzle.
	public UnityEvent OnFinish;
	
	// NOTE: could have an array of button (left to right)

	// Reference to the left SequenceButton.
	[SerializeField] private SequenceButton _LeftButton;

	// Reference to the right SequenceButton.
	[SerializeField] private SequenceButton _RightButton;

	// The Sequence itself, 0 represents left, 1 represents right.
	[SerializeField] private int[] sequence;

	// The attempt.
	[SerializeField] private int[] sequenceAttempt;

	// Current attempt index.
	[SerializeField] private int currAttemptI = 0;

	// Desired length of the sequence.
	[SerializeField] private int sequenceLength;

	// If true the puzzle has been successfully finished.
	[SerializeField] private bool finished = false;

	// Material to be set to upon success (as indication).
	[SerializeField] private Material finishedMat;

	// If true then accept no input (most likely displaying the sequence).
	bool busy = false;

	void Awake()
	{
		// Inistialise sequence arrays to a default value of -1.
		sequence = new int[sequenceLength];
		sequenceAttempt = new int[sequenceLength];
		for (int i = 0; i < sequenceLength; i++) 
		{
			sequence[i] = -1;
			sequenceAttempt[i] = -1;
		}
	}

	void Start()
	{
		GenerateSequence ();
	}

	// Generate a random sequence depending on the desired length.
	private void GenerateSequence()
	{
		// Reset attempt index.
		currAttemptI = 0;
		for (int i = 0; i < sequenceLength; i++)
			sequenceAttempt[i] = -1;

		for (int i = 0; i < sequenceLength; i++) 
			sequence[i] = Random.Range(0,2);
	}

	public void PlaySequence()
	{
		if (busy || finished)
			return;

		busy = true;
		StartCoroutine ("PlaySequenceRou");
	}

	public void AttemptAdd(int pos)
	{
		if (finished)
			return;

		// Add to the attempt.
		sequenceAttempt [currAttemptI] = pos;

		// If the attempt is correct...
		if(sequenceAttempt[currAttemptI] == sequence[currAttemptI])
		{
			// if the attempt has been finished...
			if(currAttemptI == sequenceLength - 1)
			{		
				finished = true;
				this.GetComponent<MeshRenderer>().material = finishedMat;

				if(OnFinish != null)
					OnFinish.Invoke();
			}
			
			currAttemptI++;
		}
		else
		{
			// Reset the attempt (indicated through flash of buttons).
			for (int i = 0; i < 2; i++) 
			{
				if (sequence[i] == 0)
					_LeftButton.Flash ();
				else if (sequence[i] == 1)
					_RightButton.Flash ();
			}

			// Generate a new sequence and finally, play/visualise it.
			GenerateSequence();
			PlaySequence();
		}
	}

	IEnumerator PlaySequenceRou()
	{
		yield return new WaitForSeconds (1f);

		// Set each button busy.
		for (int i = 0; i < sequenceLength; i++)
		{
			if (sequence[i] == 0)
				_LeftButton.SetBusy(true);
			else if (sequence[i] == 1)
				_RightButton.SetBusy(true);
		}

		// Play the sequence (flash then wait).
		for (int i = 0; i < sequenceLength; i++) 
		{
			if (sequence[i] == 0)
				_LeftButton.Flash ();
			else if (sequence[i] == 1)
				_RightButton.Flash ();

			yield return new WaitForSeconds (0.8f);
		}
	
		// Set each button to not be busy. (play sequence over).
		for (int i = 0; i < sequenceLength; i++)
		{
			if (sequence[i] == 0)
				_LeftButton.SetBusy(false);
			else if (sequence[i] == 1)
				_RightButton.SetBusy(false);
		}

		busy = false;
	}
	

}
