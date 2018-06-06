using UnityEngine;
using System.Collections;

// The SceneSetup script instantiates all of the assigned objects.
public class SceneSetup : MonoBehaviour 
{
	// Objects to be Instantiated.
	[SerializeField] private GameObject[] _InstantiateObj;

	// If true the Setup is executed automatically upon the scene loading.
	[SerializeField] private bool _AutoSetup = true;

	// If true the GameObject is destroyed after the Setup is complete.
	[SerializeField] private bool _DestroyOnSetup = false;

	// If true the object is not instantiated but set active.
	[SerializeField] private bool _SetActiveTrue = false;

	void Awake ()
	{
		if (!_AutoSetup)
			return;

		Setup ();
	}

	public void Setup()
	{
		// Instantiated all of the objects withinthe _InstantiateObj array.
		for (int i = 0; i < _InstantiateObj.Length; i++)
		{
			if(_SetActiveTrue)
			{
				_InstantiateObj[i].SetActive(true);
				continue;
			}
			GameObject obj = Instantiate(_InstantiateObj[i], this.transform.position, this.transform.rotation) as GameObject;
			obj.name = _InstantiateObj[i].name; // To avoid "(clone)" in the name.
		}	

		if (_DestroyOnSetup)
			Destroy (this.gameObject);
	}

}
