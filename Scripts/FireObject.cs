using UnityEngine;
using System.Collections;

// The FireObject script provides the functionallity to fire (instantiate and add force) an object.
public class FireObject : MonoBehaviour 
{
	// Projectile to fire.
	[SerializeField] private GameObject Projectile = null;

	public void Fire(Vector3 spawnPos)
	{
		// Fire the projectile at the position "spawnPos".
		GameObject projectile = Instantiate(Projectile, spawnPos + this.transform.forward, Camera.main.transform.rotation) as GameObject;

		// Add force to the object using Unity's Physics engine to simulate Projectile Motion.
		// "* (1f / Time.timeScale)" counters the slomo effect affecting the power of the launch.
		projectile.GetComponent<Rigidbody>().AddForce(this.transform.forward * 400000f * Time.deltaTime *(1f / Time.timeScale));

	}

}
