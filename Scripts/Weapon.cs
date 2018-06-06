using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// The Weapon script contains weapon specific funtionallity such as reloading and changing the fire type.
// It also stores the information such as the damage of the bullets and the current bullets in the clip.
public class Weapon : MonoBehaviour 
{
	// (Optional) AI ammo count (doesn't have a seperate inventory).
	[SerializeField] private int AiAmmo = 300;

	// Size of clip.
	[SerializeField] private int clipSize = 30;

	// Current ammo in clip.
	[SerializeField] private int currentClip = 0;

	// Reference to MuzzleFlash ParticleSystem.
	[SerializeField] private ParticleSystem MuzzleFlash = null;

	// Reference to Shrink ParticleSystem.
	[SerializeField] private ParticleSystem _ShrinkPS = null;

	// Reference to Transparency ParticleSystem.
	[SerializeField] private ParticleSystem _TransparencyPS = null;

	// Reference to MuzzleFlash GameObject.
	[SerializeField] private GameObject muzzleflashgo = null;

	// Reference to 3D UI TextMesh.	
	[SerializeField] private TextMesh ClipDisplayText = null;

	// (Optional) Reference to a scope.
	[SerializeField] private GameObject _Scope = null;

	// Bullet damage.
	[SerializeField] private int _Damage = 1;

	// If capable of power up.
	[SerializeField] private bool _PowerUpCapable = false;
	
	// Weapon position when picked up by player.
	[SerializeField] private Vector3 PickUpPos = Vector3.zero;

	// Reference to Animation component.
	[SerializeField] private Animation _Animation = null;

	[SerializeField] private AudioClip _PowerUpSound = null;

	// PowerUp
	private enum PowerUp
	{
		NULL, Shrink, Transparency
	};

	private PowerUp _PowerUp = PowerUp.NULL;
	
	// Type of Weapon
	public enum WeaponType
	{
		NULL, AssaultRifle, Shotgun, Pistol, Sniper
	};

	[SerializeField] private WeaponType _WeaponType = WeaponType.NULL;

	// Weapon Fire Type.
	public enum FireType
	{
		NULL, Single, Auto, Burst, Sniper
	};

	[SerializeField] private FireType _FireType = FireType.NULL;

	// Does the gun fire a ray bullet? (e.g. in the case of a projectile crossbow it should not)
	[SerializeField] private bool useRayBullet = true;

	// Dependent on whether the Reload Coroutine is being run.
	private bool reloadRou = false;
	
	// is AI? (decided automatically).
	private bool isAI = false;


	void Start()
	{

		// Automatically decide if weapon is owned by an AI.
		if (this.transform.parent == null)
			isAI = false;
		else if(this.transform.parent.GetComponent<AIWeaponController>())
		   isAI = true;
		else
		   isAI = false;

		// Hide MuzzleFlash
		muzzleflashgo.SetActive(false);
		MuzzleFlash.enableEmission = false;

		// Current Clip fully loaded.
		currentClip = clipSize;

		// Display the current clip.
		if (ClipDisplayText)
			ClipDisplayText.text = currentClip.ToString ();

		// Manipulate ammo (because we initially load the clip).
		if(isAI)
			AIManipulateAmmo(-clipSize);
		else
			Inventory.instance.ManipulateAmmo(_WeaponType, -clipSize);
	}

	// Change fire type to the next one, reseting after single shot.
	public void NextFireType()
	{
		// Play appropriate animation correseponding to the current Fire Type
		if((int)_FireType == 1)
		{
				this.GetAnimation () ["FireRateToSingle"].speed = -1;
				this.GetAnimation ().Play ("FireRateToSingle");
				this.GetAnimation () ["fireRateToSemi"].speed = -1;
				this.GetAnimation ().Play ("fireRateToSemi");
				_FireType = (FireType)((int)_FireType + 1);
		}
		else if((int)_FireType == 2)
		{
				this.GetAnimation () ["fireRateToSemi"].speed = 1;
				this.GetAnimation ().Play ("fireRateToSemi");
				_FireType = (FireType)((int)_FireType + 1);
		}
		else if((int)_FireType == 3)
		{
				this.GetAnimation () ["FireRateToSingle"].speed = 1;
				this.GetAnimation ().Play ("FireRateToSingle");
				_FireType = (FireType)((int)_FireType -2);
		}
		
	}

	public bool Reload()
	{
		// Play Reload animtion.
		if (isAI) {

			if (!reloadRou)
				StartCoroutine ("ReloadRou");

			return true;
		}

		// Return if clip is full or not empty.
		if (!(currentClip < clipSize || currentClip <= 0))
			return false;

		switch (_WeaponType) {
		
		case WeaponType.AssaultRifle:

			// If bullets still in clip, add it back to the ammo.
			if (currentClip > 0) 
				Inventory.instance.ManipulateAmmo(_WeaponType, +currentClip);

			// If there is ammo, add it to the clip (even if can't fill).
			if (Inventory.instance.GetAmmo(_WeaponType) > 0) 
			{
				if(Inventory.instance.GetAmmo(_WeaponType) < clipSize)
				{
					currentClip = Inventory.instance.GetAmmo(_WeaponType);
					Inventory.instance.SetAmmo(_WeaponType, 0);
				}
				else
				{
					currentClip = clipSize;
					Inventory.instance.ManipulateAmmo(_WeaponType, -clipSize);
				}

				// Update clip UI Text element.
				if (ClipDisplayText)
					ClipDisplayText.text = currentClip.ToString ();

			}
			break;

		case WeaponType.Pistol:
			Debug.Log("Weapon.cs/Reload(): TODO - Pistol Case");
			break;

		case WeaponType.Shotgun:
			Debug.Log("Weapon.cs/Reload(): TODO - Shotgun Case");
			break;

		default:
			Debug.Log("Weapon.cs/Reload(): TODO - WeaponType");
			break;

		}

		return true;
	}

	IEnumerator ReloadRou ()
	{
		reloadRou = true;

		// Deactivate muzzle flash.
		muzzleflashgo.SetActive (false);

		// Play Reload animation and wait for it to be complete.
		_Animation.Play ("reload");
		yield return new WaitForSeconds (_Animation ["reload"].length);

		// Update clip and AI Ammo (taking into account the case where AI ammo cannot fill the clip).
		if (AiAmmo < clipSize) 
		{
			currentClip = AiAmmo;
			AiAmmo = 0;
		}
		else 
		{
			currentClip = clipSize;
			AiAmmo -= clipSize;
		}
		
		reloadRou = false;
	}

	// Getter/Setter functions

	public ParticleSystem GetMuzzleFlashPS()
	{
		if (MuzzleFlash == null)
			Debug.LogError ("Weapon.cs/GetMuzzleFlashPS(): MuzzleFlash variable == null");

		return MuzzleFlash;
	}

	public GameObject GetMuzzleFlashGO()
	{
		if (muzzleflashgo == null)
			Debug.LogError ("Weapon.cs/GetMuzzleFlashGO(): muzzleflashgo variable == null");

		return muzzleflashgo;
	}

	public Animation GetAnimation()
	{
		if (_Animation == null)
			Debug.LogError ("Weapon.cs/GetAnimation(): _Animation variable == null");

		return _Animation;
	}

	public void SetAnimation(Animation anim)
	{
		_Animation = anim;
	}

	public WeaponType GetWeaponType()
	{
		return _WeaponType;
	}


	public int GetClip()
	{
		return currentClip;
	}

	public void ManipulateClip(int value)
	{
		currentClip += value;

		// Update clip UI text element.
		if (ClipDisplayText)
			ClipDisplayText.text = currentClip.ToString ();
	}

	public void AIManipulateAmmo(int value)
	{
		AiAmmo += value;
		if (AiAmmo < 0)
			AiAmmo = 0;
	}

	public int AIGetAmmo()
	{
		return AiAmmo;
	}

	public FireType GetFireType()
	{
		return _FireType;
	}
	
	public int GetDamage()
	{
		return _Damage;
	}

	public GameObject GetScope()
	{
		return _Scope;
	}

	public Vector3 GetPickUpPosition()
	{
		return PickUpPos;
	}

	public bool GetUseRayBullet()
	{
		return useRayBullet;
	}

	public void SwitchPowerUp()
	{
		if (!_PowerUpCapable)
			return;

		_ShrinkPS.gameObject.SetActive (false);
		_TransparencyPS.gameObject.SetActive (false);

		if (_PowerUp == PowerUp.NULL) 
		{
			_PowerUp = PowerUp.Shrink;
			_ShrinkPS.gameObject.SetActive (true);
			return;
		}

		if (_PowerUp == PowerUp.Shrink) 
		{
			_PowerUp = PowerUp.Transparency;
			_TransparencyPS.gameObject.SetActive (true);
			return;
		}

		if (_PowerUp == PowerUp.Transparency) 
		{
			_PowerUp = PowerUp.NULL;
			return;
		}

	}

	// Actually fires a bullet (ray) playing all the appropriate animations and sounds.
	public void FireBullet(Vector3 randomVector, Vector3 rayPos, Vector3 forward, RectTransform HitMarker)
	{
		//To update ammo UI
		this.ManipulateClip (0);

		// Activate and Play the Muzzle Flash as the gun is now firing.
		if (GetClip () > 0) 
		{
			this.GetMuzzleFlashPS ().Play ();
			this.GetMuzzleFlashPS ().enableEmission = true;
			this.GetMuzzleFlashPS ().playbackSpeed = 1f * (1f / Time.timeScale);
			this.GetMuzzleFlashGO ().SetActive (true);
		}
		
		// If the weapon has a scope then fire it from that position.
		if(this.GetScope() != null)
		{
			forward =this.GetScope().gameObject.transform.forward;
			rayPos = this.GetScope().transform.position;
		}
		
		// Dramatically bigger offset if the sniper is being used (hip fire).
		if(this.GetWeaponType() == Weapon.WeaponType.Sniper)
			randomVector *= 10f;
		
		// If the weapon has a projectile to fire then Fire it.
		if (GetClip () > 0) 
		{
			if (this.transform.GetComponent<FireObject> ())
				this.transform.GetComponent<FireObject> ().Fire (rayPos);
		}


		// If the gun fires a ray bullet...
		if (this.GetUseRayBullet() == true)
		{
			// If the bullet hits an object add a force and deal damage.
			RaycastHit hit;
			if (Physics.Raycast (rayPos, forward + randomVector, out hit, 25f)) 
			{
				// Spawn a hit particle (if one exists) where the bullet hit (on the surface).

				//UNCOMMENT
				//if (ObjectHitParticle)
				//	Instantiate (ObjectHitParticle, hit.point - transform.forward * 0.02f, Quaternion.Euler (hit.normal));

			
				if (_PowerUp == PowerUp.Shrink) 
				{
					if(Shrink(hit.transform, 1f))
						HitMarker.sizeDelta = new Vector2 (10, 10);
					return;
				}
				if (_PowerUp == PowerUp.Transparency) 
				{
					if(ReduceAlpha(hit.transform, 0.05f))
						HitMarker.sizeDelta = new Vector2 (10, 10);
					return;
				}

				if (GetClip () <= 0) 
					return;

				// Add a forwards force (from the players perspective) to the hit object.
				if (hit.transform.GetComponent<Rigidbody> ())
					hit.transform.GetComponent<Rigidbody> ().AddForce (this.transform.forward * 10000f * Time.deltaTime);

				if (hit.transform.GetComponent<Destructable> ()) 
				{
					// Increase the size of the HitMarker to show that an object with health has been hit.
					HitMarker.sizeDelta = new Vector2 (10, 10);

					// Deal damage to the hit object (depends on the damage of the weapon).
					hit.transform.GetComponent<Destructable> ().ManipulateHealth (this.GetDamage ());
				}
			}
		}

		// Reduce the current gun's clip by 1.
		if(GetClip() > 0)
			this.ManipulateClip (-1);
	}

	private bool Shrink(Transform target, float val)
	{
		if (target.tag != "Resizable")
			return false;

		if (target.GetComponent<Transform> ().localScale.x < 0.2f
			&& target.GetComponent<Transform> ().localScale.y < 0.2f
			&& target.GetComponent<Transform> ().localScale.z < 0.2f
		   ) 
		{
			// Play power up sound to indicate no more can be done.
			if(_PowerUpSound)
			{
				if(Camera.main.GetComponent<AudioSource>())
				{
					Camera.main.GetComponent<AudioSource>().clip = _PowerUpSound;
					Camera.main.GetComponent<AudioSource>().Play();
				}
			}
			return false;
		}

		// Downscale object.
		target.GetComponent<Transform> ().localScale /= 1.1f;

		return true;
	}
	
	private bool ReduceAlpha(Transform target, float val)
	{
		if (target.tag != "Transparent")
			return false;


		Color tempCol = target.GetComponent<MeshRenderer> ().material.color;
		tempCol.a -= val;

		// Disable the collider.
		if (tempCol.a <= 0.4f) 
		{
			target.GetComponent<Collider>().enabled = false;
			// Play power up sound to indicate no more can be done.
			if(_PowerUpSound)
			{
				if(Camera.main.GetComponent<AudioSource>())
				{
					Camera.main.GetComponent<AudioSource>().clip = _PowerUpSound;
					Camera.main.GetComponent<AudioSource>().Play();
				}
			}
			return false;
		}

		target.GetComponent<MeshRenderer> ().material.color = tempCol;
		
		return true;
	}

	public bool GetPowerUpCapable()
	{
		return _PowerUpCapable;
	}

}
