using UnityEngine;
using System.Collections;

// The SequenceButton scripts deals the button puzzle piece behaviour.
public class SequenceButton : MonoBehaviour 
{
	// Material to breifly flash.
	[SerializeField] private Material FlashMat;

	// Reference to the sequence controller component (the puzzle manager).
	[SerializeField] private SequenceController _SequenceController;

	// Position of the button, 0 being left and 1 being right (in this case, could go on to 3 or 4..)
	[SerializeField] private int position;

	// The original material of the button.
	private Material _OriginalMat;

	// When true input/pushing is prevented.
	// An example of when this would be the case is when the sequence is beng shown.
	private bool busy;

	void Awake()
	{
		_OriginalMat = this.GetComponent<MeshRenderer> ().material;
	}

	public void UserPush()
	{
		// If not bust then add the press to the attempt sequence.
		if (!busy)
			_SequenceController.AttemptAdd(position);

		// Flash the button (show that it has been presented).
		Flash ();
	}

	public bool Flash()
	{
		StartCoroutine ("FlashRou");
		return true;
	}

	// Change material to the flash material breifly, and then return back.
	IEnumerator FlashRou()
	{
		this.GetComponent<MeshRenderer> ().material = FlashMat;
		yield return new WaitForSeconds(0.4f);
		this.GetComponent<MeshRenderer> ().material = _OriginalMat;
	}

	public void SetBusy(bool val)
	{
		busy = val;
	}

	public bool GetBusy()
	{
		return busy;
	}
}
