using UnityEngine;
using System.Collections;

// The Spawner script deals with spawning any inputted object.
// It has the functionallity to spawn such an object every x seconds if defined through a recursive coroutine.
public class Spawner : MonoBehaviour 
{
	// Entity to Spawn.
	[SerializeField] private GameObject Entity = null;

	// How many Entitys are spawned each time.
	[SerializeField] private int Quantity = 1;

	// (Optional) If true then an Entity will spawn/instantiate frequently (defined by player)
	[SerializeField] private bool SpawnEverySecondsBool = false;

	// (Optional) The ammount of seconds before an Entity is spawned (after first Entity is spawned).
	[SerializeField] private int SpawnEverySeconds = 0;

	void Start () 
	{
		// Spawn Entity.
		Spawn(Quantity);

		// If true, spawn an Entity after a number of seconds has passed.
		if(SpawnEverySecondsBool)
			StartCoroutine(SpawnEvery(SpawnEverySeconds));
	}
	
	public void Spawn(int quantity)
	{
		// Spawn/Instantiate "quanitity" number of Entitys.
		for(int i = 0; i < quantity; i++)
			Instantiate(Entity, this.transform.position, this.transform.rotation);
	}
	
	// Recursive Coroutine that spawns an Entity after a specific number of seconds.
	IEnumerator SpawnEvery(int seconds)
	{
		yield return new WaitForSeconds(seconds);
		
		Spawn(1);

		StartCoroutine(SpawnEvery(SpawnEverySeconds));
	}
}
