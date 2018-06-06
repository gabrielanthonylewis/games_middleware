using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// The PlayerMovement script moves the object depending on the player inputs, also providing crouch and prone functionality.
// The script also provides the Slow Motion effect functionality by changing the timescale.
public class PlayerMovement : MonoBehaviour
{
	// Vertical Speed multiplier
    [SerializeField] private float _VerticalSpeed = 6f;
    
	// Horizontal Speed multiplier
	[SerializeField] private float _HorizontalSpeed = 6f;
    
	// Reference to the Slomo Bar UI element.
	[SerializeField] private RectTransform _SlomoBar;

	// Reference to the Slomo Fill Image.
	[SerializeField] private RawImage _SlomoFillImg;

	// Total Slomo value in seconds.
	[SerializeField] private float _TotalSlomo = 7f;

	// Current Slomo value.
	[SerializeField] private float _CurrentSlomo;

	// A scaled value (representing 1 slomo unit) for use of scaling the UI Bar.
	private float scaledUnit = 0f;

	// Is the Player Crouched?
	private bool isCrouching = false;

	// Is the player Prone?
	private bool isProne = false;

	void Start()
	{
		if (!_SlomoBar) 
		{
			_SlomoBar = GameObject.FindGameObjectWithTag ("UI_SlomoBar").GetComponent<RectTransform> ();
			// Calculate a scaled value.
			scaledUnit = _SlomoBar.rect.width / _TotalSlomo;
		}
		
		if (!_SlomoFillImg)
			_SlomoFillImg = GameObject.FindGameObjectWithTag ("SlomoFill").GetComponent<RawImage> ();
		

		// Set the current slomo value to the total slomo value.
		_CurrentSlomo = _TotalSlomo;
	}

	void Update()
	{
		if (!_SlomoBar) 
		{
			_SlomoBar = GameObject.FindGameObjectWithTag ("UI_SlomoBar").GetComponent<RectTransform> ();
			// Calculate a scaled value.
			scaledUnit = _SlomoBar.rect.width / _TotalSlomo;
		}

		if (!_SlomoFillImg)
			_SlomoFillImg = GameObject.FindGameObjectWithTag ("SlomoFill").GetComponent<RawImage> ();


		// If there is Slomo time left...
		if(_CurrentSlomo > 0f)
		{
			if (Input.GetKeyUp (KeyCode.Space)) 
			{
				// Reset timescale back to normal.
				Time.timeScale = 1f;
				//Time.fixedDeltaTime = (Time.fixedDeltaTime * 2.5f);	
				Time.fixedDeltaTime = 0.02F * Time.timeScale;

				_SlomoFillImg.enabled = false;
				return;
			}
			
			if (Input.GetKeyDown (KeyCode.Space)) 
			{
				// Slow time down.
				Time.timeScale = 0.4f;
				//Time.fixedDeltaTime = (Time.fixedDeltaTime * 0.4f);
				Time.fixedDeltaTime = 0.02F * Time.timeScale;

				_SlomoFillImg.enabled = true;
			}

			if(Input.GetKey(KeyCode.Space))
			{
				// Reduce the current Slomo time every second. "* (1f / Time.timeScale)" is to counter the slowed time.
				_CurrentSlomo -= Time.deltaTime * (1f / Time.timeScale);
				// Adjust the Slomo Bar accordingly.
				_SlomoBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 18f, _CurrentSlomo * scaledUnit);
			}

			// If there is no more slomo left then reset the timescale back to normal.
			if(_CurrentSlomo <= 0)
			{
				Time.timeScale = 1f;
				//Time.fixedDeltaTime = (Time.fixedDeltaTime * 2.5f);
				Time.fixedDeltaTime = 0.02F * Time.timeScale;

				_SlomoFillImg.enabled = false;
			}
		}

		// Crouch/Stand back up depending on the current stance.
		if (Input.GetKeyDown(KeyCode.C))
		{
			if(isCrouching)
			{
				// return to original size
				this.GetComponent<CapsuleCollider>().center = new Vector3(0f,0f,0f);
				this.GetComponent<CapsuleCollider>().height = 2f;
				isCrouching = false;
				return;
			}

			// New size (Crouch)
			// NOTE: Crouching works by simply changing the size of the collider and moving it up 
			// letting the player fall beneath the floor more so than before.
			this.GetComponent<CapsuleCollider>().height = 1.6f;
			this.GetComponent<CapsuleCollider>().center = new Vector3(0f,0.2f,0f);
			isCrouching = true;
			isProne = false;
		}

		// Prone/Stand back up depending on the current stance.
		if(Input.GetKeyDown(KeyCode.LeftAlt))
		{
			if(isProne)
			{
				// return to original size
				this.GetComponent<CapsuleCollider>().center = new Vector3(0f,0f,0f);
				this.GetComponent<CapsuleCollider>().height = 2f;
				isProne = false;
				return;
			}

			// New size (Crouch)
			// NOTE: Prone works by simply changing the size of the collider and moving it up 
			// letting the player fall beneath the floor more so than before.
			this.GetComponent<CapsuleCollider>().height /= 10f;
			this.GetComponent<CapsuleCollider>().center = new Vector3(0f,0.5f,0f);
			isProne = true;
			isCrouching = false;
			return;
		}
		
	}

    void FixedUpdate()
    {
		// Record horizontal and vertical movement multiplying each by their corresponding multiplier.
        float horizontal = Input.GetAxis("Horizontal") * _HorizontalSpeed;
        float vertical = Input.GetAxis("Vertical") * _VerticalSpeed;

		// Move/Translate the player on each axis.
        transform.Translate(new Vector3(horizontal, 0f, 0f) * Time.fixedDeltaTime); 
        transform.Translate(new Vector3(0, 0f, vertical) * Time.fixedDeltaTime);

    }
	
}
