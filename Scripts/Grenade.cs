using UnityEngine;
using System.Collections;

// The Grenade script "explodes" the object after a specified amount of time.
// The damage is done by activating a trigger collider that deals damage to any objects inside it.
public class Grenade : MonoBehaviour 
{
	// Time until explosion.
	[SerializeField] private float waitTime = 5f;

	[SerializeField] private SphereCollider triggerCollider = null;
	
	void Start()
	{
		// Start the explotion coroutine.
		StartCoroutine(WaitThenExplode(waitTime));	

		triggerCollider = this.GetComponent<SphereCollider> ();
		triggerCollider.enabled = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		// Add and explosion force to the object within the trigger.
		if(other.GetComponent<Rigidbody>())
			other.GetComponent<Rigidbody>().AddExplosionForce(1000f * Time.deltaTime, this.transform.position, 2f);

		// Damage the object within the trigger by 10.
		if(other.GetComponent<Destructable>())
			other.GetComponent<Destructable>().ManipulateHealth(10f);
	}

	// Wait "seconds" seconds and the explode the grenade.
	IEnumerator WaitThenExplode(float seconds)
	{
		triggerCollider.enabled = false;

		yield return new WaitForSeconds(seconds);

		// Play the explosion sound clip.
		if(this.GetComponent<AudioSource>())
			this.GetComponent<AudioSource>().Play();

		// enable trigger collider so that objects inside it can take damage.
		triggerCollider.enabled = true;

		// Wait a small period of time so that the trigger can deal damage and force
		// to the surrounding objects.
		yield return new WaitForSeconds(0.5f);

		// If object has a parent (e.g. in the case of the exploding arrow),
		// delete it (and therefore the grenade aswell)
		if(this.transform.parent != null)
			Destroy(this.transform.parent.gameObject);
		
		Destroy(this.gameObject);
	}
}
