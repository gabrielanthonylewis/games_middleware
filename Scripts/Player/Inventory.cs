using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// The Inventory script is a Singleton implementation that holds the ammo count, an array of guns, grenade count etc.
// Also providing Accessors to these values and functionality to drop, add or equipt a weapon.
public class Inventory 
{
	// Singleton pattern implementation so that there can only be ONE inventory used throughout the game.
	#region Singleton Pattern implementation
	protected Inventory() { }
	
	private static Inventory _instance = null;
	
	public static Inventory instance
	{
		get
		{
			if (Inventory._instance == null)
			{
				Inventory._instance = new Inventory();
			}
			return Inventory._instance;
		}
	}
	#endregion
	
	// List of guns (maximum of 3)
	[SerializeField] private GameObject[] Guns = new GameObject[3];

	// Total ammount of Assault Rifle ammo (NOTE that this represents all ammo at this point).
	[SerializeField] private int _AR_ammo = 300;

	// Reference to the Player's WeaponController component.
	[SerializeField] private WeaponController _WeaponController = null;

	// Ammount of grenades (default at 3)
	private int _Grenades = 3;

	// Reference to the Main Camera's Transform component (optimisation purposes).
	private Transform _MainCamera = null;

	// Reference to the Grendades UI text component (to show the player how many grenades they have).
	private Text _GrenadesUIText = null;

	// Reference to the Clips UI text component (to show the player how many clips/mags they have).
	private Text _ClipsUIText = null;

	// Initalisation function (cannot use Start() or Awake() as not inherited from MonoBehaviour)  
	public void Initialise()
	{
		// Default Values.
		_AR_ammo = 300;
		_Grenades = 3;

		// Get reference to the Grenades UI Text component.
		if (!_GrenadesUIText)
			_GrenadesUIText = GameObject.FindGameObjectWithTag ("GrenadesText").GetComponent<Text>();

		// Get reference to the Clips UI Text component.
		if (!_ClipsUIText)
			_ClipsUIText = GameObject.FindGameObjectWithTag ("ClipsText").GetComponent<Text>();

		UpdateUI ();
	}

	public void UpdateUI()
	{
		// Get reference to the Grenades UI Text component.
		if (!_GrenadesUIText)
			_GrenadesUIText = GameObject.FindGameObjectWithTag ("GrenadesText").GetComponent<Text>();

		// Get reference to the Clips UI Text component.
		if (!_ClipsUIText)
			_ClipsUIText = GameObject.FindGameObjectWithTag ("ClipsText").GetComponent<Text>();

		// Display how many grenades the player has.
		_GrenadesUIText.text = _Grenades.ToString();

		// Calculates how many full clips there are..
		_ClipsUIText.text = Mathf.RoundToInt((_AR_ammo + 30) / 30f).ToString();
	}

	public bool EquipWeapon(int slotIndex)
	{
		// If no gun exists in requested slot then return.
		if (Guns [slotIndex] == null)
			return false;

		// If the gun is already active then return.
		if (Guns [slotIndex].activeSelf == true)
			return false;
	
		// If a reference to the Main Camera is non-existant then get it (optimisation reasons).
		if (_MainCamera == null)
			_MainCamera = Camera.main.transform;

		// Turn off held guns and re-parent (to avoid a potential bug).
		for (int i = 0; i < Guns.Length; i++) 
		{
			if(Guns [i] != null)
			{
				Guns[i].transform.gameObject.SetActive (false);
				Guns[i].transform.SetParent(_MainCamera.parent);
			}
		}

		// Optimisation reasons as ".GetComponent<Weapon>()" is called multiple times. 
		Weapon gunWeaponComponent = Guns [slotIndex].GetComponent<Weapon> ();

		// Activate/show new gun, parenting and positioning the gun in the correct position.
		Guns[slotIndex].transform.gameObject.SetActive(true);
		Guns[slotIndex].transform.SetParent(_MainCamera);
		Guns [slotIndex].transform.localPosition = Vector3.zero;
		Guns [slotIndex].transform.localPosition = gunWeaponComponent.GetPickUpPosition();
		Guns [slotIndex].transform.localRotation = Quaternion.Euler (Vector3.zero);

		// If a reference to the player's WeaponController doesn't exist, get it.
		if (_WeaponController == null)
			_WeaponController = _MainCamera.GetComponent<WeaponController> ();

		// Set the current weapon to the new one.
		_WeaponController.SetCurrentWeapon (gunWeaponComponent);
	
		// "Refresh" the camera to avoid a past animation bug.
		_MainCamera.gameObject.SetActive (false);
		_MainCamera.gameObject.SetActive (true);

		return true;
	}

	public bool AddWeapon(GameObject weapon)
	{
		// If a reference to the Main Camera is non-existant then get it (optimisation reasons).
		if (_MainCamera == null)
			_MainCamera = Camera.main.transform;

		// If the weapon is already in the Inventory then don't add it!
		for (int i = 0; i < Guns.Length; i++) {
			if(Guns[i] == weapon) return false;
		}

		// Check through all of the Guns until an empty spot is found.
		for(int i = 0; i < Guns.Length; i++)
		{
			// If spot in Inventory is empty.
			if(Guns[i] == null)
			{
				// Add weapon to spot, parent it and position it correctly.
				Guns[i] = weapon;

				weapon.transform.SetParent(_MainCamera.transform);
			
				weapon.transform.rotation = Guns[0].transform.rotation;
				weapon.transform.SetParent(Guns[0].transform.parent);
				weapon.transform.position = Guns[0].transform.position;

				weapon.name = "Gun";

				// Set weapon's position to custom position (was implemented due to the Sniper having to be positioned uniquely).
				weapon.transform.localPosition = weapon.GetComponent<Weapon>().GetPickUpPosition();
	
				// Turn off colliders and physics.
				if(weapon.transform.GetComponent<BoxCollider>())
					weapon.transform.GetComponent<BoxCollider>().enabled = false;
				if(weapon.transform.GetComponent<Rigidbody>())
					weapon.transform.GetComponent<Rigidbody>().isKinematic = true;

				// Assign reference to the Player's animation component.
				weapon.GetComponent<Weapon>().SetAnimation(_MainCamera.GetComponent<Animation>());

				// Set the weapon's children's layers to "GunLayer" so that the gun will not clip through objects (from the player's perspective).
				Transform[] children = weapon.GetComponentsInChildren<Transform>();
				for(int j = 0; j < children.Length; j++)
				{
					children[j].gameObject.layer = 10;
				}

				// Hide weapon (not equiped on default).
				weapon.SetActive(false);

				// Automatically equip weapon if the Player hasn't already got a weapon equipted.
				bool isEmpty = true; // Records whether or not the Inventory is empty.
				for(int j = 1; j < Guns.Length; j++)
				{
					if(Guns[j] != null)
						isEmpty = false;
				}

				if(isEmpty && i == 0) // If no weapon in the inventory and first spot free, equip weapon.
					EquipWeapon(0);

				return true;
			}
		}

		// An empty spot hasn't been found so return false (weapon not added).
		return false;
	}
	
	public bool DropWeapon(GameObject weapon)
	{
		// If the weapon doesn't exist, it can't be dropped, so return false.
		if(weapon == null) return false;

		// If a reference to the Main Camera is non-existant then get it (optimisation reasons).
		if (_MainCamera == null)
			_MainCamera = Camera.main.transform;

		// Check guns in the Inventory for the corresponding weapon.
		int idx = -1; // "-1" acts as out of range.
		for(int i = 0; i < Guns.Length; i++)
		{
			if(Guns[i] == weapon)
			{
				// Deparent the weapon and re-enable the colliders and physics.
				weapon.transform.SetParent(null);
			
				if(weapon.transform.GetComponent<BoxCollider>())
					weapon.transform.GetComponent<BoxCollider>().enabled = true;

				if(weapon.transform.GetComponent<Rigidbody>())
				{
					weapon.transform.GetComponent<Rigidbody>().isKinematic = false;
					// "Throw" weapon forwards.
					weapon.transform.GetComponent<Rigidbody>().AddForce(_MainCamera.forward * 10000f * Time.deltaTime);
				}

				// Set the weapon's children's layers to 0 (default) so that the player cannot see the weapon through objects.
				Transform[] children = weapon.GetComponentsInChildren<Transform>();
				for(int j = 0; j < children.Length; j++)
				{
					children[j].gameObject.layer = 0;
				}

				// Sets the weapon's layer to "PickUp" so that the player can pick it back up.
				weapon.layer = 8;

				// Inventory slot is empty.
				Guns[i] = null;

				// Keep track of the slot emptied.
				idx = i;
			}
			
		}

		// If no corresponding gun was found in the Inventory then return false (couldn't drop). 
		if(idx == -1)
			return false;

		// If another gun is in the Inventory then equip it.
		for(int i = 0; i < Guns.Length; i++)
		{
			if(Guns[i] != null)
			{
				// possible TODO ?: Move all guns to the left/right.. (fill in the gap in the array)
				EquipWeapon(i);
				return true;
			}
		}

		// If no reference to the player's WeaponController component is present, get it. 
		if (_WeaponController == null)
			_WeaponController = _MainCamera.GetComponent<WeaponController> ();

		// No weapon was found in the Inventory.
		_WeaponController.SetCurrentWeapon (null);
		
		return true;
	}

	public int GetAmmo(Weapon.WeaponType weaponType)
	{	// Calculates how many full clips there are..
		if(_ClipsUIText)
			_ClipsUIText.text = Mathf.RoundToInt((_AR_ammo + 30) / 30f).ToString();	
		switch (weaponType) {
		case Weapon.WeaponType.AssaultRifle:
			return _AR_ammo;
			
		case Weapon.WeaponType.Pistol:
			Debug.Log("Inventory.cs/GetAmmo(): TODO - Pistol Case");
			break;
			
		case Weapon.WeaponType.Shotgun:
			Debug.Log("Inventory.cs/GetAmmo(): TODO - Shotgun Case");
			break;
			
		case Weapon.WeaponType.Sniper:
			Debug.Log("Inventory.cs/SetAmmo(): TODO - Sniper Case");
			return _AR_ammo;
			
			
		default:
			Debug.LogError("Inventory.cs/GetAmmo(): Invalid Weapon Type!");
			return -1;
		}

		return -1;
	}

	public void SetAmmo(Weapon.WeaponType weaponType, int value)
	{
		switch (weaponType) {
		case Weapon.WeaponType.AssaultRifle:

			_AR_ammo = value;

			// Enforce lower bound limit.
			if(_AR_ammo < 0)
				_AR_ammo = 0;

			break;
			
		case Weapon.WeaponType.Pistol:
			Debug.Log("Inventory.cs/SetAmmo(): TODO - Pistol Case");
			break;
			
		case Weapon.WeaponType.Shotgun:
			Debug.Log("Inventory.cs/SetAmmo(): TODO - Shotgun Case");
			break;
			
		case Weapon.WeaponType.Sniper:
			Debug.Log("Inventory.cs/SetAmmo(): TODO - Sniper Case");

			_AR_ammo = value;

			// Enforce lower bound limit.
			if(_AR_ammo < 0)
				_AR_ammo = 0;

			break;
			
		default:
			Debug.LogError("Inventory.cs/SetAmmo(): Invalid Weapon Type!");
			break;
		}

		// Get reference to the Clips UI Text component.
		if (!_ClipsUIText)
			_ClipsUIText = GameObject.FindGameObjectWithTag ("ClipsText").GetComponent<Text>();
		
		// Calculates how many full clips there are..
		_ClipsUIText.text = Mathf.RoundToInt((_AR_ammo + 30) / 30f).ToString();
	}

	public void ManipulateAmmo(Weapon.WeaponType weaponType, int value)
	{

		switch (weaponType) 
		{
			case Weapon.WeaponType.AssaultRifle:
		
				_AR_ammo += value;

				// Enforce lower bound limit.
				if (_AR_ammo < 0)
				{
					_AR_ammo = 0;
					_ClipsUIText.text = "0"; // Ensure Clips Text displays 0.
				}

				break;
				
			case Weapon.WeaponType.Pistol:
				Debug.Log("Inventory.cs/ManupulateAmmo(): TODO - Pistol Case");
				break;
				
			case Weapon.WeaponType.Shotgun:
				Debug.Log("Inventory.cs/ManupulateAmmo(): TODO - Shotgun Case");
				break;
				
			case Weapon.WeaponType.Sniper:
				//Debug.Log("Inventory.cs/ManupulateAmmo(): TODO - Sniper Case");

				_AR_ammo += value;

				// Enforce lower bound limit.
				if (_AR_ammo < 0)
				{
					_AR_ammo = 0;
					_ClipsUIText.text = "0"; // Ensure Clips Text displays 0.
				}
				break;
				
			default:
				Debug.LogError("Inventory.cs/ManupulateAmmo(): Invalid Weapon Type!");
				break;
		}

		// Get reference to the Clips UI Text component.
		if (!_ClipsUIText) 
		{
			if(GameObject.FindGameObjectWithTag ("ClipsText"))
				_ClipsUIText = GameObject.FindGameObjectWithTag ("ClipsText").GetComponent<Text> ();
		}

		// Calculates how many full clips there are..
		if(_ClipsUIText)
			_ClipsUIText.text = Mathf.RoundToInt((_AR_ammo + 30) / 30f).ToString();	
	}
	
	public int GetGrenades()
	{
		return _Grenades;
	}
	
	public void SetGrenades(int value)
	{
		_Grenades = value;

		// Get reference to the Grenades UI Text component.
		if (!_GrenadesUIText)
			_GrenadesUIText = GameObject.FindGameObjectWithTag ("GrenadesText").GetComponent<Text>();
		
		// Display how many grenades the player has.
		_GrenadesUIText.text = _Grenades.ToString();
	}
	
	public void ManipulateGrenades(int value)
	{
		_Grenades += value;

		// Enforce lower bound limit.
		if(_Grenades < 0)
			_Grenades = 0;

		// Get reference to the Grenades UI Text component.
		if (!_GrenadesUIText)
			_GrenadesUIText = GameObject.FindGameObjectWithTag ("GrenadesText").GetComponent<Text>();
		
		// Display how many grenades the player has.
		_GrenadesUIText.text = _Grenades.ToString();
	}
	
}
