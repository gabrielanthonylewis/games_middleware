using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// The AI Weapon Controller script manages the weapon specfic behaviour automatiaclly for the AI.
// It deal with Reloading and firing as well as Spotting the targets through multiple ray casts.
public class AIWeaponController : MonoBehaviour 
{
	// Reference to the current weapon being used.
	[SerializeField] private Weapon _CurrentWeapon = null;

	// The ParticleSystem to be instantiated upon hitting an object.
	[SerializeField] private ParticleSystem ObjectHitParticle;

	// Reference to the Player/Target
	[SerializeField] private GameObject Player = null;

	// There could potentially be more than one target (store in this variable)
	[SerializeField] private GameObject[] targets;

	// Tag of the Target (so that AI could potentially fight other AI/"Freindlys")
	[SerializeField] private string TargetTag = "Player";

	// Sound clip played when shooting.
	[SerializeField] private AudioClip AssaultRifleFireSound;

	// Sound clip played when reloading.
	[SerializeField] private AudioClip ReloadSound;

	// How far the AI can see (units/meters)
	[SerializeField] private float _SightRange = 25f;

	[SerializeField] private float _RandomVectorRange = 0.12f;

	// If true then the player/target has been spotted (and will shoot).
	private bool SpottedPlayer = false;

	// Current index of the target in focus (corresponds with the "targets" array).
	private	int targetIndex = -1;

	// If true then the behaviour will be halted at a point. Used when AI is reacting (waits a moment of time). 
	private bool waitFor = false;

	// Prevents behaviour from executing if true.
	private bool stop = false;

	// Reference to the AudioSource component (optimisation purposes).
	private AudioSource _AudioSource = null;

	
	void Start () 
	{
		// Get reference to AudioSource component (optimisation purposes).
		_AudioSource = this.GetComponent<AudioSource> ();

		// If a Player target hasn't been set then find one.
		if(Player == null)
			Player = GameObject.FindGameObjectWithTag("Player");

		// Find all Gameobjects with the corresponding tag.
		targets = GameObject.FindGameObjectsWithTag(TargetTag);
			
		// Find the index of the current player/target in the array.
		for(int i = 0; i < targets.Length; i++)
		{
			if(targets[i] == Player)
				targetIndex = i;
		}

	}

	void Update ()
	{
		// if stop == true, halt behaviour.
		if(stop)
			return;

		// If there isn't a current target then stop firing.
		if (targetIndex < 0 || targetIndex > targets.Length || targets[targetIndex] == null)
		{
			// If there are no targets, try find one (or more) again.
			if(targets.Length == 0)
				targets = GameObject.FindGameObjectsWithTag(TargetTag);

			// Hide muzzle flash.
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
		
			// Randomly pick a new target from the possible targets. (done 50 times in the case the target is null)
			for(int i = 0; i < 50; i++)
			{
				if(targets.Length == 0)
					break;

				targetIndex = Random.Range(0, targets.Length);

				// If a target is found, check to see if the AI (self) can see the target.
				if(targets[targetIndex] != null)
				{
					RaycastHit hit;
					if (Physics.Raycast (this.transform.position, targets[targetIndex].transform.position - this.transform.position, out hit, _SightRange))
					{
						if(hit.transform.tag == TargetTag)
							break;
					}
				}
			}

			// If no target is found, stop behaviour.
			if(targets.Length > 0 && targets[targetIndex] == null)
				stop = true;

			// The target is not spotted by default.
			SpottedPlayer = false;
		
			return;
		}

		// "Look" (aim gun) towards the player.
		this.transform.LookAt (targets[targetIndex].transform.position, transform.up);

		// If waitFor is true (in the case of reaction time), hault behaviour.
		if(waitFor)
			return;
		
		// Create 3 different rays (so that the AI has can spot targets in obscure postions such as crouched)

		RaycastHit hitBottom; // Lowest ray hit (stores hit data)
		RaycastHit hitMid; // Medium ray hit (stores hit data)
		RaycastHit hitTop; // Top ray hit (stores hit data)

		#region Debugging
		// hitTop
		Debug.DrawRay(this.transform.position, (this.transform.forward + (this.transform.up * 0.075f)) * _SightRange, Color.green);
		// hitMid
		Debug.DrawRay(this.transform.position, (this.transform.forward + (this.transform.up * 0.05f)) * _SightRange, Color.green);
		// hitBottom
		Debug.DrawRay(this.transform.position, this.transform.forward * _SightRange, Color.green);
		#endregion

		// Has the ray hit an object?
		bool hitMidBool = Physics.Raycast (this.transform.position, (this.transform.forward + (this.transform.up * 0.05f)), out hitMid, _SightRange);
		bool hitTopBool = Physics.Raycast (this.transform.position, (this.transform.forward + (this.transform.up * 0.075f)), out hitTop, _SightRange);
		bool hitBottomBool = Physics.Raycast (this.transform.position , this.transform.forward, out hitBottom, _SightRange);

		// If none of the rays have hit an object, return and hide the muzzle flash (stopped firing).
		if (!hitBottomBool && !hitMidBool && !hitTopBool) 
		{
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
			return;
		}
		// Else if ANY of the three rays have hit the player/a target.
		else if (hitTop.transform != null && (hitTop.transform.tag == TargetTag || (hitTop.transform.GetComponent<Destructable> () && SpottedPlayer))
		    || hitBottom.transform != null && (hitBottom.transform.tag == TargetTag || (hitBottom.transform.GetComponent<Destructable> () && SpottedPlayer))
			|| hitMid.transform != null && (hitMid.transform.tag == TargetTag || (hitMid.transform.GetComponent<Destructable> () && SpottedPlayer)))
		{			
			// If the player hasn't already been spotted.
			if(SpottedPlayer == false)
			{
				// React (wait for a moment of time).
				StartCoroutine("Reaction");
				SpottedPlayer = true;
				return;
			}
		}
		// Else if another object  (not a player/target) return and hide the muzzle flash (stopped firing).
		else 
		{
			//TODO NEXT BUILD // ATEMPT TO SEE TARGET BY CROUCHING OR LEANING
			
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
			return;
		}

		// If the Muzzle Flash is not playing then activate it.
		if (!_CurrentWeapon.GetMuzzleFlashPS().isPlaying) {
			
			_CurrentWeapon.GetMuzzleFlashPS().Play ();
			_CurrentWeapon.GetMuzzleFlashPS().enableEmission = true;
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (true);
		}

		// If the weapon's clip is empty.
		if (_CurrentWeapon.GetClip() <= 0) 
		{
			// If there is Ammo left then reload.
			if (_CurrentWeapon.AIGetAmmo() > 0)
			{
				// Play Reload sound.
				if(_AudioSource)
				{
					_AudioSource.clip = ReloadSound;
					
					if(_AudioSource.isPlaying == false)
					{
						if(_AudioSource.enabled == true)
							_AudioSource.Play();
					}
				}

				//Reload weapon.
				_CurrentWeapon.Reload();
			}

			// Activate/show Muzzle flash.
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
			return;
		}

		// If no animation is playing (weapon is idle/not being used)
		if (this.GetComponent<Animation> ().isPlaying == false)
		{
			// Recoil effect implemented using random vector to be used as an offset.
			Vector3 randomVector = new Vector3 (Random.Range (-_RandomVectorRange, _RandomVectorRange), Random.Range (-_RandomVectorRange, _RandomVectorRange), Random.Range (-_RandomVectorRange, _RandomVectorRange)); 
			
			Debug.DrawRay(this.transform.position + this.transform.up * 0.5f, (this.transform.forward   + randomVector) * _SightRange, Color.blue);

			// If the ray (bullet) has hit an object.
			RaycastHit hit;
			if (Physics.Raycast (this.transform.position, this.transform.forward + randomVector, out hit, _SightRange)) 
			{
				// Play recoil animation
				this.GetComponent<Animation> ().Play ("recoil");

				// Play shoot/fire sound.
				if(_AudioSource)
				{
					_AudioSource.clip = AssaultRifleFireSound;
					
					if(_AudioSource.enabled == true)
						_AudioSource.Play();
				}

				// Activate/Show muzzle flash.
				_CurrentWeapon.GetMuzzleFlashGO().SetActive (true);

				// Reduce clip size by 1 bullet.
				_CurrentWeapon.ManipulateClip(-1);


				// Apply force to the hit object (if it has a Rigidbody component).
				if (hit.transform.GetComponent<Rigidbody> ())
					hit.transform.GetComponent<Rigidbody> ().AddForce (this.transform.forward * 10000f * Time.deltaTime);


			
				// Reduce object's health by 1 (if it has a Destructable component).
				if (hit.transform.GetComponent<Destructable> ())
					hit.transform.GetComponent<Destructable> ().ManipulateHealth (_CurrentWeapon.GetDamage());

			}
			
		}

	}

	// On destruction deactivate the Muzzle Flash.
	void OnDestroy ()
	{
		_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
	}

	// Wait a random amount of time before reacting.
	IEnumerator Reaction()
	{
		waitFor = true;

		float randomTime = Random.Range(0f,1.5f);
		yield return new WaitForSeconds(randomTime);

		waitFor = false;
	}
	
	public GameObject GetTarget()
	{
		if (targetIndex < 0 || targetIndex > targets.Length)
			return null;

		return targets[targetIndex];
	}

	public bool GetSpottedPlayer()
	{
		return SpottedPlayer;
	}



}
