using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// The WeaponController script provides/give access to (through input and references) all of the functionallity to operate the current weapon.
// This includes reloading, changing the firing mode, aiming, tilting, throwing grenades, switching weapons etc.
public class WeaponController : MonoBehaviour
{
	// Reference to the current weapon being used.
	[SerializeField] private Weapon	_CurrentWeapon = null;

	// The ParticleSystem to be instantiated upon a bullet hitting an object.
	[SerializeField] private ParticleSystem ObjectHitParticle;

	// Reference to the Hit Marker UI element.
	[SerializeField] private RectTransform HitMarker = null;

	// Firing sound clip.
	[SerializeField] private AudioClip AssaultRifleFireSound;

	// Reloading sound clip.
	[SerializeField] private AudioClip ReloadSound;

	// Fire rate changing sound clip. 
	[SerializeField] private AudioClip FireRateSound;

	// GameObject to be instantiated upon throwing a grenade.
	[SerializeField] private GameObject _GrenadePrefab = null;

	// Traks whether the player is aiming down their sight (ads) and/or tiliting left or right.
	private bool ads = false, tiltRight = false, tiltLeft = false;

	// Tracks whether or not the Fire Coroutine is being used.
	private bool fireRou = false;

	// Reference to the AudioSource component (optimisation purposes).
	private AudioSource _AudioSource;

	// A reference to a potential grenade to be thrown.
	private GameObject tempGrenade = null;

	// A mutliplier used to increase the distance the grenade is thrown.
	private float _grenadeThrowMulti = 250f;

	void Awake()
	{
		if (!HitMarker)
			HitMarker = GameObject.FindGameObjectWithTag("UI_HitMarker").GetComponent<RectTransform>();

		Inventory.instance.UpdateUI ();
	}

	void Start ()
	{
		// Get reference to the AudioSource component.
		_AudioSource = this.GetComponent<AudioSource> ();

		// Add the current weapon to the Inventory.
		Inventory.instance.AddWeapon (_CurrentWeapon.gameObject);
	}
	
	void Update ()
	{
		// If the game is paused then halt all of the behaviour.
		if(Time.timeScale == 0) 
			return;

		// Ready a Grenade if there is one in the Inventory.
		if(Inventory.instance.GetGrenades() > 0)
		{
			if (Input.GetKey (KeyCode.G)) 
			{
				// Ready the grenade.
				if (Input.GetKeyDown (KeyCode.G)) 
				{
					_grenadeThrowMulti = 250f;
					tempGrenade = Instantiate(_GrenadePrefab, this.transform.position + this.transform.forward, _GrenadePrefab.transform.rotation) as GameObject;
					tempGrenade.rigidbody.useGravity = false;
					tempGrenade.transform.GetChild(0).collider.enabled = false;
				}

				// Update the grenades position.
				if(tempGrenade)
					tempGrenade.transform.position = this.transform.position + this.transform.forward /1.6f;

				// Increase the distance of the grenade whilst the player is holding it down. "* (1f / Time.timeScale)" counters the slomo effect affecting the power of the throw.
				_grenadeThrowMulti += 50f * Time.deltaTime * (1f / Time.timeScale);

				// Limit the distance the grenade can be thrown.
				if(_grenadeThrowMulti > 500f)
					_grenadeThrowMulti = 500f;
			}

			// Throw the grenade.
			if(Input.GetKeyUp(KeyCode.G))
			{
				if(tempGrenade)
				{
					tempGrenade.transform.GetChild(0).collider.enabled = true;
					tempGrenade.rigidbody.useGravity = true;
					// Throw the grenade. "* (1f / Time.timeScale)" counters the slomo effect affecting the power of the throw.
					tempGrenade.GetComponent<Rigidbody>().AddForce(this.transform.forward * _grenadeThrowMulti * (1f / Time.timeScale) , ForceMode.Force);
					// Remove one grenade from the Inventory. 
					Inventory.instance.ManipulateGrenades(-1); 
				}
			}
		}

		// Attempt to change weapon depending on the key pressed (1, 2 or 3).
		if (_CurrentWeapon && !_CurrentWeapon.GetAnimation ().isPlaying) 
		{
			if (Input.GetKeyDown (KeyCode.Alpha1) || Input.GetKeyDown (KeyCode.Alpha2) || Input.GetKeyDown (KeyCode.Alpha3))
				fireRou = false;

			if (Input.GetKeyDown (KeyCode.Alpha1))
				Inventory.instance.EquipWeapon (0);
			if (Input.GetKeyDown (KeyCode.Alpha2))
				Inventory.instance.EquipWeapon (1);
			if (Input.GetKeyDown (KeyCode.Alpha3))
				Inventory.instance.EquipWeapon (2);
		}

		// Switch Power Up
		if (Input.GetKeyDown (KeyCode.T))
			_CurrentWeapon.SwitchPowerUp();

		// Return the HitMarker's size back to it's orginal size. 
		HitMarker.sizeDelta = Vector2.Lerp(HitMarker.sizeDelta, new Vector2(4,4), Time.deltaTime * 20f);

		// If there is no Current weapon then weapon behaviour is not possible so return.
		if(_CurrentWeapon == null) return;

		// Drop weapon.
		if (Input.GetKeyDown (KeyCode.X)) 
		{
			if(_CurrentWeapon.GetPowerUpCapable())
				return;

			// If the weapon is dropped then firing has stopped so stop & hide the Muzzle Flash.
			_CurrentWeapon.GetMuzzleFlashPS ().enableEmission = false;
			_CurrentWeapon.GetMuzzleFlashGO ().SetActive (false);

			// If the weapon is a Sniper and scoped then stop looking down the scope.
			if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper && ads)
				_CurrentWeapon.GetScope().SetActive(false);	

			fireRou = false;

			Inventory.instance.DropWeapon (_CurrentWeapon.gameObject);
		}

		// Melee
		if (Input.GetKeyDown (KeyCode.V)) 
		{
			// If idle then can melee.
			if(_CurrentWeapon.GetAnimation ().isPlaying == false)
			{
				fireRou = false;
			
				// Play a different Melee animation depending on whether or not the current weapon is a Sniper.
				if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
					_CurrentWeapon.GetAnimation ().Play ("meleeSniper");
				else
					_CurrentWeapon.GetAnimation ().Play ("melee");

				// If an object is hit then apply force and reduce it's health.
				RaycastHit hit;
				if (Physics.Raycast (this.transform.position, this.transform.forward, out hit, 2f))
				{				
					// Apply force to hit object. "* (1f / Time.timeScale)" counters the slomo effect affecting the power of the throw.
					if (hit.transform.GetComponent<Rigidbody> ())
						hit.transform.GetComponent<Rigidbody> ().AddForce (this.transform.forward * 20000f * Time.deltaTime *  (1f / Time.timeScale));
					
					if (hit.transform.GetComponent<Destructable> ())
					{
						// Increase the size of the HitMarker to show that an object with health has been hit.
						HitMarker.sizeDelta = new Vector2(10,10);
						hit.transform.GetComponent<Destructable> ().ManipulateHealth (5f);
					}
					
				}

				// Aiming is interupted so set it to false.
				ads = false;
			}
		}

		// If weapon exists and NOT reloading...
		if (_CurrentWeapon != null && !_CurrentWeapon.GetAnimation ().IsPlaying ("reloadads") 
		    // (Allows the player the aim down sight whilst shooting the gun but not when doing anything else like changing fire mode)
		    && ((_CurrentWeapon.GetAnimation ().IsPlaying ("recoil") || (_CurrentWeapon.GetAnimation ().IsPlaying ("recoilads"))
		     || !_CurrentWeapon.GetAnimation().isPlaying)))
		{
			// Start/Stop Aiming Down the Gun's Sight depending on the current state.
			if (Input.GetKeyDown (KeyCode.Mouse1))
			{
				ads = !ads;

				// Play animation forwards/backwards depending on the current state.
				if (ads == true)
				{
					_CurrentWeapon.GetAnimation () ["ads"].speed = 1;
					_CurrentWeapon.GetAnimation () ["adsSniper"].speed = 1;
				}
				else
				{
					_CurrentWeapon.GetAnimation () ["ads"].speed = -1;
					_CurrentWeapon.GetAnimation () ["adsSniper"].speed = -1;
				}

				// If the current weapon is a sniper then activate the Scope.
				if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
				{
					_CurrentWeapon.GetScope().SetActive(ads);	
					_CurrentWeapon.GetAnimation ().Play ("adsSniper");	
				}
				else
				{
					_CurrentWeapon.GetAnimation ().Play ("ads");
				}
			}

			// Tilt Right OR back to the normal state depending on current tilt state.
			if (Input.GetKeyDown (KeyCode.E)) 
			{
				tiltRight = !tiltRight;

				// Play backwards/forwards depending on the current tilt state.
				if (tiltRight == true)
					_CurrentWeapon.GetAnimation () ["tiltRight"].speed = 1;
				else
					_CurrentWeapon.GetAnimation () ["tiltRight"].speed = -1;

				_CurrentWeapon.GetAnimation ().Play ("tiltRight");
			}

			// Tilt Left OR back to the normal state depending on current tilt state.
			if (Input.GetKeyDown (KeyCode.Q)) 
			{
				tiltLeft = !tiltLeft;
			
				// Play backwards/forwards depending on the current tilt state.
				if (tiltLeft == true)
					_CurrentWeapon.GetAnimation () ["tiltLeft"].speed = 1;
				else
					_CurrentWeapon.GetAnimation () ["tiltLeft"].speed = -1;
				
				_CurrentWeapon.GetAnimation ().Play ("tiltLeft");
			}
			
			// Change the Fire Type (Fully Automatic, Burst and Single shot).
			if(Input.GetKeyDown(KeyCode.B) && !fireRou) // if not firing
			{
				// Functionallity only availiable for the Assault Rifle.
				if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.AssaultRifle)
				{
					_AudioSource.clip = FireRateSound;
					_AudioSource.Play();
					_CurrentWeapon.NextFireType();

					// Aiming is interupted so set it to false.
					ads = false;
				}
			}

		}

		// Reload on command or automatically if the clip is empty (and if there is enough ammo).
		if (_CurrentWeapon != null && (Input.GetKeyDown (KeyCode.R) || (_CurrentWeapon.GetClip() <= 0)) && Inventory.instance.GetAmmo(_CurrentWeapon.GetWeaponType()) > 0 && !_CurrentWeapon.GetAnimation ().isPlaying) 
		{
			// If has reloaded then play the animation and sound.
			if (_CurrentWeapon.Reload() && !_CurrentWeapon.GetAnimation().isPlaying) 
			{
				fireRou = false;

				// Different reload animation played depending on if Aim down sight or if current weapon is sniper.
				if (ads)
					_CurrentWeapon.GetAnimation ().Play ("reloadads");
				else if(_CurrentWeapon.GetWeaponType() != Weapon.WeaponType.Sniper)
					_CurrentWeapon.GetAnimation ().Play ("reload");
				else if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
					_CurrentWeapon.GetAnimation ().Play ("reloadSniper");

				_AudioSource.clip = ReloadSound;
				_AudioSource.Play();

				// Turn off muzzle flash as not firing.
				_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
			}
		}

		// If the clip is empty then hide the MuzzleFlash (as not firing).
		if (_CurrentWeapon != null && _CurrentWeapon.GetClip() <= 0) 
		{
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
		//	return;
		}

		// Play Muzzle flash when firing.
		if (Input.GetKeyDown (KeyCode.Mouse0)) 
		{
			if (_CurrentWeapon != null && _CurrentWeapon.GetClip() > 0) 
			{
				if(_CurrentWeapon.GetAnimation ().isPlaying == false && !fireRou)
				{
					if (!_CurrentWeapon.GetMuzzleFlashPS ().isPlaying) 
					{
						_CurrentWeapon.GetMuzzleFlashPS ().Play ();
						_CurrentWeapon.GetMuzzleFlashPS().playbackSpeed = 1f *(1f / Time.timeScale); // Counters the Slomo effect.
						_CurrentWeapon.GetMuzzleFlashPS ().enableEmission = true;
						_CurrentWeapon.GetMuzzleFlashGO ().SetActive (true);
					}
				}
			}
		}

		// Shoot gun (if not busy and the Player actually has a gun).
		if (_CurrentWeapon != null && _CurrentWeapon.GetAnimation ().isPlaying == false && !fireRou)
		{
			// If the weapon is fully automatic then Fire the weapon constantly whilst the Mouse is down.
			if (_CurrentWeapon.GetFireType () == Weapon.FireType.Auto) 
			{
				if (Input.GetKey (KeyCode.Mouse0)) 
						StartCoroutine ("Fire");
			} 
			// else If the weapon can only be shot once on Mouse Down (Note that the burst is activated on mouse down). 
			else if (_CurrentWeapon.GetFireType () == Weapon.FireType.Single 
			           || _CurrentWeapon.GetFireType () == Weapon.FireType.Sniper  
			           || _CurrentWeapon.GetFireType() == Weapon.FireType.Burst) 
			{
				if (Input.GetKeyDown (KeyCode.Mouse0)) 
						StartCoroutine ("Fire");
			}
		}
	
		// If not firing then turn off the Muzzle Flash.
		if (!Input.GetKey (KeyCode.Mouse0) && _CurrentWeapon && !fireRou) 
		{
			_CurrentWeapon.GetMuzzleFlashGO().SetActive (false);
		}
	}
	
	public void SetCurrentWeapon (Weapon weapon)
	{
		_CurrentWeapon = weapon;
	}

	public Weapon GetCurrentWeapon()
	{
		return _CurrentWeapon;
	}
	
	IEnumerator Fire()
	{
		fireRou = true;

		// Offset of the ray creating a Recoil effect.
		Vector3 randomVector = Vector3.zero;

		if (_CurrentWeapon.GetFireType () == Weapon.FireType.Burst) 
		{
			// Shoot a burst of 4 bullets.
			for (int i = 0; i < 4; i++) 
			{
				// If not aiming down the sight add some recoil (less than if fully automatic).
				if (!ads)
					randomVector = new Vector3 (Random.Range (-0.03f, 0.03f), Random.Range (-0.03f, 0.03f), Random.Range (-0.03f, 0.0f));

				// Fire a bullet.
				FireBullet(randomVector);

				// Wait for animation to finish before firing again.
				do
				{
					yield return null;
				} 
				while ( _CurrentWeapon.GetAnimation ().isPlaying );

				// Stop emitting the Muzzle Flash (as not firing).
				_CurrentWeapon.GetMuzzleFlashPS ().enableEmission = false;
				_CurrentWeapon.GetMuzzleFlashPS().playbackSpeed = 1f *(1f / Time.timeScale);
			}

			// Turn off the Muzzle Flash as completely done with it.
			_CurrentWeapon.GetMuzzleFlashGO ().SetActive (false);
		} 
		else 
		{
			// If not aiming down the sight add some recoil (more than if burst fire).
			if (!ads)
				randomVector = new Vector3 (Random.Range (-0.05f, 0.05f), Random.Range (-0.05f, 0.05f), Random.Range (-0.05f, 0.05f));

			// Fire a bullet.
			FireBullet(randomVector);

			// Stop emitting the Muzzle Flash (as not firing).
			_CurrentWeapon.GetMuzzleFlashPS ().enableEmission = false;
			_CurrentWeapon.GetMuzzleFlashPS().playbackSpeed = 1f *(1f / Time.timeScale);

			if (_CurrentWeapon && _CurrentWeapon.GetFireType () == Weapon.FireType.Auto) 
			{
				// Wait for animation to finish before firing again.
				do
				{
					yield return null;
				}
				while (_CurrentWeapon && _CurrentWeapon.GetAnimation ().isPlaying );
			}
			else if(_CurrentWeapon.GetFireType() == Weapon.FireType.Sniper)
			{ 
				// Wait an extended period of time (game balance reasons).
				yield return new WaitForSeconds (1.5f *(1f / Time.timeScale));
			}
			// No delay for Assault rifle but delay for a single shot pistol for example.
			else if (_CurrentWeapon.GetWeaponType() != Weapon.WeaponType.AssaultRifle 
			         && _CurrentWeapon.GetFireType () == Weapon.FireType.Single)
			{
				yield return new WaitForSeconds (0.1f *(1f / Time.timeScale));
			}

			// Turn off the Muzzle Flash as completely done with it.
			if(_CurrentWeapon)
				_CurrentWeapon.GetMuzzleFlashGO ().SetActive (false);
		}

		fireRou = false;

		yield return null;
	}

	private void FireBullet(Vector3 randomVector)
	{
		// Different recoil animation played depending on if Aiming or not and if the Sniper is being used (more dramatic).
		if (ads)
		{
			_CurrentWeapon.GetAnimation()["recoilads"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoilads");
		}
		else if(_CurrentWeapon.GetWeaponType() != Weapon.WeaponType.Sniper)
		{
			_CurrentWeapon.GetAnimation()["recoil"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoil");
			
		}
		else if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
		{
			_CurrentWeapon.GetAnimation()["recoilSniper"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoilSniper");
		}


		// Play firing sound clip.
		if (_CurrentWeapon != null && _CurrentWeapon.GetClip () > 0)
		{
			_AudioSource.clip = AssaultRifleFireSound;
			_AudioSource.Play ();
		}

		// Stores the forward vector (optimisation and cleanliness)
		Vector3 forward = this.transform.forward;
		// Stores the Player's position (optimisation and cleanliness)
		Vector3 pos = this.transform.position;

		_CurrentWeapon.FireBullet (randomVector, pos, forward, HitMarker);
	}

	#region bin
	/*
	 * 	// Actually fires a bullet (ray) playing all the appropriate animations and sounds.
	private void FireBullet(Vector3 randomVector)
	{
		// Activate and Play the Muzzle Flash as the gun is now firing.
		_CurrentWeapon.GetMuzzleFlashPS ().Play ();
		_CurrentWeapon.GetMuzzleFlashPS ().enableEmission = true;
		_CurrentWeapon.GetMuzzleFlashPS().playbackSpeed = 1f *(1f / Time.timeScale);
		_CurrentWeapon.GetMuzzleFlashGO ().SetActive (true);

		// Different recoil animation played depending on if Aiming or not and if the Sniper is being used (more dramatic).
		if (ads)
		{
			_CurrentWeapon.GetAnimation()["recoilads"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoilads");
		}
		else if(_CurrentWeapon.GetWeaponType() != Weapon.WeaponType.Sniper)
		{
			_CurrentWeapon.GetAnimation()["recoil"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoil");
			
		}
		else if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
		{
			_CurrentWeapon.GetAnimation()["recoilSniper"].speed = (1f / Time.timeScale);
			_CurrentWeapon.GetAnimation ().Play ("recoilSniper");
		}

		// Stores the forward vector (optimisation and cleanliness)
		Vector3 forward = this.transform.forward;
		// Stores the Player's position (optimisation and cleanliness)
		Vector3 pos = this.transform.position;

		// If the weapon has a scope then fire it from that position.
		if(_CurrentWeapon.GetScope() != null)
		{
			forward =_CurrentWeapon.GetScope().gameObject.transform.forward;
			pos = _CurrentWeapon.GetScope().transform.position;
		}

		// Dramatically bigger offset if the sniper is being used (hip fire).
		if(_CurrentWeapon.GetWeaponType() == Weapon.WeaponType.Sniper)
			randomVector *= 10f;
	
		// If the weapon has a projectile to fire then Fire it.
		if(_CurrentWeapon.transform.GetComponent<FireObject>())
			_CurrentWeapon.transform.GetComponent<FireObject>().Fire(pos);

		// If the gun fires a ray bullet...
		if (_CurrentWeapon.GetUseRayBullet() == true) {
			// If the bullet hits an object add a force and deal damage.
			RaycastHit hit;
			if (Physics.Raycast (pos, forward + randomVector, out hit, 25f)) {
				// Spawn a hit particle (if one exists) where the bullet hit (on the surface).
				if (ObjectHitParticle)
					Instantiate (ObjectHitParticle, hit.point - transform.forward * 0.02f, Quaternion.Euler (hit.normal));

				// Add a forwards force (from the players perspective) to the hit object.
				if (hit.transform.GetComponent<Rigidbody> ())
					hit.transform.GetComponent<Rigidbody> ().AddForce (this.transform.forward * 10000f * Time.deltaTime);
			
				if (hit.transform.GetComponent<Destructable> ()) {
					// Increase the size of the HitMarker to show that an object with health has been hit.
					HitMarker.sizeDelta = new Vector2 (10, 10);
					// Deal damage to the hit object (depends on the damage of the weapon).
					hit.transform.GetComponent<Destructable> ().ManipulateHealth (_CurrentWeapon.GetDamage ());
				}
			}
		}

		// Play firing sound clip.
		_AudioSource.clip = AssaultRifleFireSound;
		_AudioSource.Play();

		// Reduce the current gun's clip by 1.
		_CurrentWeapon.ManipulateClip (-1);

	}*/
	#endregion

}
