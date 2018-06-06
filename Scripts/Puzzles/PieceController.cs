using UnityEngine;
using System.Collections;

// The PieceController deals with the movement of a "piece".
public class PieceController : MonoBehaviour 
{
	// Direction to move in, Left/Right is in the X axis whilst Up/Down is in the Z axis.
	public enum Direction { Up, Left, Down, Right };

	public bool Move(Direction dir,  float offset)
	{
		Vector3 newPos = this.transform.position;

		// Move according to the direction chosen + the offset. 
		switch (dir) 
		{
			case Direction.Up:
				newPos.z += this.transform.localScale.z + offset;
				break;

			case Direction.Left:
				newPos.x -= this.transform.localScale.x + offset;
				break;

			case Direction.Down:
				newPos.z -= this.transform.localScale.z + offset;
				break;

			case Direction.Right:
				newPos.x += this.transform.localScale.z + offset;
				break;
		}
		this.transform.position = newPos;

		return true;
	}
}
