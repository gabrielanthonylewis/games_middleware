using UnityEngine;
using System.Collections;

// The PlayerRaycast script provides optional behaviour to Interact with objects (have to have an Interact layer)
// and Pick up objects (have to have a PickUp layer).
public class PlayerRaycast : MonoBehaviour 
{
	// Delegate Implementation (I wanted to experiment with it).
	delegate void MainDelegate(RaycastHit hit);
	MainDelegate MyDelegate;

	// Layer/layers to be seen by the Ray.
	[SerializeField] LayerMask layermask;

	// Sound clip played when picking up an Item.
	[SerializeField] private AudioClip _PickUpSound;

	// If true enables the Pick up behaviour.
	[SerializeField] bool _pickUp = false;

	// If true enables the Interaction behaviour.
	[SerializeField] bool _interact = false;

	// Reference to AudioSource component (optimisation purposes).
	private AudioSource _AudioSource = null;

	// Setter used to add/remove Pick Up behaviour from the delegate.
	public bool PickUp
	{
		get{ return _pickUp; }
		set{ 
			_pickUp = PickUp;
			if(_pickUp)
			{
				MyDelegate += PickupRay;
				if(this.enabled == false)
					this.enabled = true;
			}
			else
			{
				MyDelegate -= PickupRay;
				if(MyDelegate == null)
					this.enabled = false;
			}
		}
	}

	// Setter used to add/remove Interaction behaviour from the delegate.
	public bool Interact
	{
		get{ return _interact; }
		set{ 
			_interact = Interact;
			if(_interact)
			{
				MyDelegate += InteractRay;
				if(this.enabled == false)
					this.enabled = true;
			}
			else
			{
				MyDelegate -= InteractRay;
				if(MyDelegate == null)
					this.enabled = false;
			}
		}
	}

	void Awake () 
	{
		// Add Pick up behaviour to the delegate if true.
		if(_pickUp)
			MyDelegate += PickupRay;

		// Add Interaction behaviour to the delegate if true.
		if (_interact)
			MyDelegate += InteractRay;
	}

	void Start()
	{
		// Get reference to the  AudioSource component.
		_AudioSource = this.GetComponent<AudioSource>();

		// Hide cursor (A hit marker is present).
		Screen.showCursor = false;
	}

	void OnEnable()
	{
		// If both the Interaction and Pick Up behaviour is not going to be used,
		// there is not point in the script being active.
		if (MyDelegate == null)
			this.enabled = false;
	}

	void Update () 
	{
		// Check to see if anything within the layerMask is hit.
		RaycastHit hit;
		if (Physics.Raycast (this.transform.position, this.transform.forward, out hit, 4f, layermask)) {
			MyDelegate (hit);
		}
	}

	void PickupRay(RaycastHit hit)
	{
		// If the hit object cannot be picked up then return.
		if(hit.transform.gameObject.layer != 8) return;
		
		if (Input.GetKeyDown (KeyCode.F))
		{
			// If the hit object is a Weapon, add it to the Inventory.
			if(hit.transform.gameObject.GetComponent<Weapon>())
				Inventory.instance.AddWeapon(hit.transform.gameObject);

			// If the script has a bonus... (it must be value based)
			if(hit.transform.gameObject.GetComponent<PickUpBonus>())
			{
				// If ammo is rewarded, add the ammo to the Inventory and destroy the pick up object.
				if(hit.transform.gameObject.GetComponent<PickUpBonus>().ammo > 0)
				{
					Inventory.instance.ManipulateAmmo(Weapon.WeaponType.AssaultRifle, hit.transform.gameObject.GetComponent<PickUpBonus>().ammo);
					Destroy(hit.transform.gameObject);
				}

				// If health is rewarded, add the health to the Player and destroy the pick up object.
				if(hit.transform.gameObject.GetComponent<PickUpBonus>().health > 0)
				{
					if(this.transform.parent.GetComponent<Destructable>().ManipulateHealth(-hit.transform.gameObject.GetComponent<PickUpBonus>().health))
						Destroy(hit.transform.gameObject);
					else
						return; // return so the pick up sound clip isn't played.
				}

				// If grenades are rewarded, add the grenades to the Inventory and destroy the pick up object.
				if(hit.transform.gameObject.GetComponent<PickUpBonus>().grenades > 0)
				{
					Inventory.instance.ManipulateGrenades(hit.transform.gameObject.GetComponent<PickUpBonus>().grenades);
					Destroy(hit.transform.gameObject);
				}
			}

			// Play the pick up sound clip.
			_AudioSource.clip = _PickUpSound;
			_AudioSource.Play();
		}
	}

	void InteractRay(RaycastHit hit)
	{	
		// If the hit object cannot be interacted with then return.
		if(hit.transform.gameObject.layer != 9) return;

		if(Input.GetKeyDown(KeyCode.F))
		{
			// If the hit object is a button, "push" it.
			if(hit.transform.GetComponent<ButtonBehaviour>())
			{
				hit.transform.GetComponent<ButtonBehaviour>().Push();
			}

			// If the hit object has an OpenGate component, open the corresponding gate.
			if(hit.transform.gameObject.GetComponent<OpenGate>())
				hit.transform.gameObject.GetComponent<OpenGate>().Open();

			// If the hit object has a PiecePuzzleController component, "play" the puzzle and remove self from the game temporarily.
			if(hit.transform.gameObject.GetComponent<PiecePuzzleController>())
			{
				if(hit.transform.gameObject.GetComponent<PiecePuzzleController>().Play(this.transform.parent))
					this.transform.parent.gameObject.SetActive(false);
			}

			// If a Sequence Controller is hit, play it's sequence.
			if(hit.transform.gameObject.GetComponent<SequenceController>())
				hit.transform.gameObject.GetComponent<SequenceController>().PlaySequence();

			// If a Sequence button itself is hit and it's not busy, "push" it.
			if(hit.transform.gameObject.GetComponent<SequenceButton>())
			{
				if(!hit.transform.gameObject.GetComponent<SequenceButton>().GetBusy())
					hit.transform.gameObject.GetComponent<SequenceButton>().UserPush();
			}

			// If a hit object is rotatable and snaps (auto), rotate the object to the next rotation.
			if(hit.transform.gameObject.GetComponent<Rotatable>())
			{	
				if(hit.transform.gameObject.GetComponent<Rotatable>().GetAuto())
					hit.transform.gameObject.GetComponent<Rotatable>().Rotate();
			}
		
			// If the hit object is a Door, etner it.
			if(hit.transform.gameObject.GetComponent<Door>())
				hit.transform.gameObject.GetComponent<Door>().Enter();
		}

		// If the F key is active and the hit object is rotatable and doesn't snap (!auto), rotate the object.
		if (Input.GetKey (KeyCode.F))
		{
			if(hit.transform.gameObject.GetComponent<Rotatable>())
			{
				if(!hit.transform.gameObject.GetComponent<Rotatable>().GetAuto())
					hit.transform.gameObject.GetComponent<Rotatable>().Rotate();
			}
		}
	}
}
