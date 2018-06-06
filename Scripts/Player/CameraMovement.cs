using UnityEngine;
using System.Collections;

// The CameraMovement script allows the recieving object (the camera is the intended object)
// to rotate depending on the mouse input. The X and Y axis can be independtly locked.
public class CameraMovement : MonoBehaviour
{
	// If true X axis rotation is locked/prevented.
	[SerializeField] private bool LockX = false;
    
	// If true Y axis rotation is locked/prevented.
	[SerializeField] private bool LockY = false;
    
	// Speed of vertical rotation.
	[SerializeField] private float _VerticalSpeed = 2.5f;
    
	// Speed of horizontal rotation.
	[SerializeField] private float _HorizontalSpeed = 2.5f;

	// Lower bound vertical limit. 
    private float _VertLimitLow = 0.5f;
    
	// Higher bound vertical limit.
	private float _VertLimitHigh = -0.4f;

	// Reference to Transform component (optimisation purposes).
    private Transform _Transform = null;

	public Camera ThirdPersonCamera = null;

	public Camera LayerCamera = null;

//	private bool isThirdPerson = false;

    void Awake()
    {
        _Transform = this.transform;
    }

    void Update()
    {
		// If paused (timeScale == 0), stop behaviour (return).
		if(Time.timeScale == 0)
			return;
		     
		// If X axis isn't locked then update the Vertical Rotation accordingly.
        if (!LockX)
            UpdateHorizontalRot();

		// If Y axis isn't locked then update the Vertical Rotation accordingly.
        if (!LockY) {
			UpdateVerticalRot ();

			/* Third person.
			if(Input.GetKeyDown(KeyCode.I))
			{
				if(ThirdPersonCamera)
				{
				
					isThirdPerson = !isThirdPerson;
					ThirdPersonCamera.gameObject.SetActive(isThirdPerson);
					ThirdPersonCamera.transform.gameObject.SetActive(isThirdPerson);
					this.camera.enabled = !isThirdPerson;
					LayerCamera.enabled = !isThirdPerson;
					this.GetComponent<WeaponController>().enabled = !isThirdPerson;

				}
				else
				{
					if(this.GetComponent<WeaponController>())
					{
						this.GetComponent<WeaponController>().GetCurrentWeapon().gameObject.SetActive(true);
					}
				}
			}*/

		}
    }   

    private void UpdateHorizontalRot()
    {
		float mouseX = Input.GetAxis ("Mouse X") * _HorizontalSpeed;

		Screen.lockCursor = (mouseX != 0);

		// Prevent the "Slomo effect" from slowing down horizontal rotation. "* (1f / Time.timeScale)" counters the slomo effect.
		if (Time.timeScale != 1)
			mouseX *= Time.deltaTime * (1f / Time.timeScale) * 200f ;

		// Get horizontal rotation freezing x and z axis.
        Quaternion horizontalRot = _Transform.rotation * Quaternion.Euler(new Vector3(0f, mouseX, 0f)) ;
        horizontalRot.x = 0f;
        horizontalRot.z = 0f;

        _Transform.rotation = horizontalRot;
    }

    private void UpdateVerticalRot()
    {
        float mouseY = Input.GetAxis("Mouse Y") * _VerticalSpeed;

		Screen.lockCursor = (mouseY != 0);

		// Prevent the "Slomo effect" from slowing down vertical rotation. "* (1f / Time.timeScale)" counters the slomo effect.
		if (Time.timeScale != 1)
			mouseY *= Time.deltaTime * (1f / Time.timeScale)  * 200f;

		// Get horizontal rotation freezing y and z axis.
        Quaternion verticalRot = Quaternion.Euler(new Vector3(-mouseY, 0f, 0f));
        verticalRot.x = Mathf.Clamp(verticalRot.x, Quaternion.Euler(-65f, 0, 0).x, Quaternion.Euler(65f, 0, 0).x);
        verticalRot.y = 0f;
        verticalRot.z = 0f;

		// Enforce upper and lower bound limits preventing the camera from rotating past these limits.
        if (_Transform.localRotation.x + verticalRot.x > _VertLimitLow || _Transform.localRotation.x + verticalRot.x < _VertLimitHigh)
            return;

        _Transform.rotation *= verticalRot;
    }

	void OnTriggerStay(Collider other)
	{
		//this.transform.position = Vector3.Lerp (this.transform.position, this.transform.position + this.transform.forward, Time.deltaTime);
		
	}


}
