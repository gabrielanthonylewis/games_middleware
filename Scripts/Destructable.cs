using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

// The Destructable script provides health and destruction functionally upon 0 health (unless specified not to).
// Destruction is achieved by instantiating a prefab of smaller objects.
public class Destructable : MonoBehaviour 
{
	// Fragments to instantiate when object is destroyed.
	[SerializeField] private GameObject Fragments = null;

	// The current health of the object.
	[SerializeField] private float Health = 1f;

	// The maximum health (default at 0f signifing the max health will be = to the current health).
	[SerializeField] private float _MaxHealth = 0f;

	// (Optional) Camera to activate upon destruction.
	// e.g. the Death camera when the player dies.
	[SerializeField] private GameObject Cam = null;

	// (Optional) Reference to the UI Health element to manipulate.
	// e.g. the Player's health bar decreasing.
	[SerializeField] private RectTransform UIhealth = null;

	// (Optional) Reference to a UI Text element to manipulate.
	// e.g. the Bosses health bar decreasing.
	[SerializeField] private Text UIText = null;

	// (Optional) Objects to drop upon death. e.g. Droping a weapon.
	[SerializeField] private List<GameObject> DropList = new List<GameObject>();

	// Does the object destroy itself upon reaching 0 health?
	[SerializeField] private bool DestroyOnNoHealth = true;

	// (Optional) Animation to be played upon death.
	[SerializeField] private AnimationClip DeathAnim = null;

	// (Optional) Animation to be played upon getting damaged.
	[SerializeField] private AnimationClip HitAnim = null;

	// (Optional - For use on Boss enemy) reference to a OnDeathBossEvent script to be invoked.
	[SerializeField] private OnDeathBossEvent _OnDeathBossEvent = null;


	// A scaled value (representing 1 health) for use of scaling the UI Bar.
	private float _UIScaledUnit = 0f;
	
	void Start()
	{
		// If a reference to a UI element exists then calculate a scaled value.
		if (UIhealth)
			_UIScaledUnit = UIhealth.rect.width / Health;

		// If the Max Health is 0f (default) then set the Max Health to the current Health.
		if(_MaxHealth == 0f)
			_MaxHealth = Health;

		// If a reference to a UI text element exists then output health/maxhealth.
		if (UIText)
			UIText.text = Health.ToString () + "/" + _MaxHealth.ToString ();
	}

	public bool ManipulateHealth(float val)
	{
		// If trying to increase the health when the health is already at maximum then return.
		if(Health == _MaxHealth && val < 0) return false;

		Health -= val;

		// If there is a Hit animation clip then play it.
		if (HitAnim) 
		{
			this.GetComponent<Animation> ().clip = HitAnim;
			this.GetComponent<Animation>().Play();
		}

		// Limit the health from passing the upper bound (Max health).
		if(Health > _MaxHealth)
			Health = _MaxHealth;

		if (Health < 0)
			Health = 0;

		// If a UI element is set then set it to the new value of health.
		if (UIhealth)
			UIhealth.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 18f, Health * _UIScaledUnit);

		// If a UI text element exists then output health/maxhealth.
		if (UIText)
			UIText.text = Health.ToString () + "/" + _MaxHealth.ToString ();

		// If the current Health is < 0 then the object dies/explodes.
		if (Health <= 0 && DestroyOnNoHealth)
			StartCoroutine("Explode");
		
		return true;
	}

	IEnumerator Explode()
	{
		if (DeathAnim)
		{
			this.GetComponent<Animation> ().clip = DeathAnim;
			this.GetComponent<Animation>().Play();
			yield return new WaitForSeconds (this.GetComponent<Animation> ().clip.length);
		}

		// Instantiate fragments and detach all child objects.
		if (Fragments) 
		{
			GameObject _fragments = Instantiate (Fragments, transform.position, transform.rotation) as GameObject;
			_fragments.transform.DetachChildren ();
		}

		// "Drop" all objects in the Droplist and add physics.
		for (int i = 0; i < DropList.Count; i++) 
		{
			DropList[i].transform.SetParent(null);
			DropList[i].AddComponent<Rigidbody>();
			// Assign Pick up layer (8) to the object.
			DropList[i].layer = 8;
		}

		// If a Camera is assigned then activate it.
		if (Cam) 
		{
			// Begin slomo effect
			Time.timeScale = 0.4f;
			Time.fixedDeltaTime = (Time.fixedDeltaTime * 0.4f);

			// Set the cameras postion to the objects position (so the animation plays at the right position)
			Cam.transform.GetChild(0).transform.position = this.transform.position;

			// Show crusor so that the player can use the UI Buttons.
			Screen.showCursor = true;

			Cam.SetActive (true);
		}

		if (_OnDeathBossEvent)
			_OnDeathBossEvent.InvokeEvent();

		Destroy(this.gameObject);
	}

}
