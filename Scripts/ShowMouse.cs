using UnityEngine;
using System.Collections;

// This script is used to show the mouse cursor on awake.
public class ShowMouse : MonoBehaviour 
{
	void Awake () 
	{
		Screen.showCursor = true;
	}
}
