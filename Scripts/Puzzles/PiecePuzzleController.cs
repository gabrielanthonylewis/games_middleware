using UnityEngine;
using System.Collections;
using UnityEngine.Events;

// The PiecePuzzleController script is the manager script of the piece/picture puzzle.
public class PiecePuzzleController : MonoBehaviour 
{
	struct Position
	{
		public int x;
		public int y;
	}
	
	struct PuzzlePiece
	{
		public Position position;
		public PieceController controller;
	}

	// Event to be invoked upon completion of the puzzle.
	[SerializeField] private UnityEvent OnWinEvent;

	// If true the puzzle is active (the user is playing it).
	[SerializeField] private bool isActive = true;

	// A reference to the Camera that will be active during the duration of the puzzle.
	[SerializeField] private Camera PuzzleCamera = null;

	// Ordered reference to each piece's PieceController.
	[SerializeField] private PieceController[] _goPieces;

	// The current index of the piece/PieceController clicked.
	[SerializeField] private int currIndex = -1;

	// The number of rows the puzzle has.
	[SerializeField] private const int rows = 3;

	// The number of columns the puzzle has.
	[SerializeField] private const int columns = 3;

	// The space between each piece.
	[SerializeField] private const float offset = 0.08f;

	// Ordered reference of each puzzle piece containing more useful data from the "PuzzlePiece" structure.
	private PuzzlePiece[] _BoardPieces;

	// Keeps a reference to the player for purposes of player safety (hide player).
	private Transform _tempPlayer;

	void Awake()
	{
		// If not supposed to active then turn off the camera.
		if (!isActive)
			PuzzleCamera.gameObject.SetActive (false);

		// Inialise _BoardPieces.
		_BoardPieces = new PuzzlePiece[rows * columns ];

		// Populate _BoardPieces[]. 
		int tRow = 0;
		int tCol = 0;
		for (int rowI = 0; rowI < 8; rowI++) 
		{
			// Logic to move along to theoretically move to the next line.
			if(rowI % rows == 0)
			{
				tRow++;
				tCol = 0;
			}
			tCol++;
		
			// Inialise the controller and position information of each piece.
			_BoardPieces[rowI].controller = _goPieces[rowI];
			_BoardPieces[rowI].position.x = tCol - 1;
			_BoardPieces[rowI].position.y = tRow - 1;
		}

		RandomizeBoard ();
	}

	void Update () 
	{
		// If the game is not being played then return.
		if (!isActive)
			return;

		// Exit from the game when Escape is pressed.
		if (Input.GetKeyDown (KeyCode.Escape))
			Stop();

		// [Selecting a piece]
		if (PuzzleCamera) 
		{	
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				RaycastHit hit;
				Ray ray = PuzzleCamera.ScreenPointToRay (Input.mousePosition);
				if(Physics.Raycast(ray, out hit, 10f))
				{
					// If the hit object is a piece...
					if(hit.transform.gameObject.GetComponent<PieceController>())
					{
						PieceController tempPieceController = hit.transform.gameObject.GetComponent<PieceController>();

						// Find the piece selected.
						for(int i = 0; i < _BoardPieces.Length; i++)
						{
							if(tempPieceController == _BoardPieces[i].controller)
							{
								currIndex= i;
								_BoardPieces[currIndex] = _BoardPieces[i];					
								break;
							}
						}

						// Attempt to move the piece into the free space.
						MoveToFreeSpace();

						// Deselect piece.
						currIndex = -1;

						// If the puzzle has been complete then stop the game and invoke the win event.
						if(Complete())
						{
							Stop();
							OnWinEvent.Invoke();
						}
					}
				}
			}
		}
	}

	// Check to see if the puzzle has been complete through comparison.
	private bool Complete()
	{
		for(int i = 0; i < _BoardPieces.Length - 1; i++)
		{
			if(_BoardPieces[i].controller != _goPieces[i])
				return false;
		}

		return true;
	}

	// Hide the player (for safety) and play the puzzle game by enabling the camera and such.
	public bool Play (Transform tempPlayer)
	{
		_tempPlayer = tempPlayer;
		isActive = true;
		PuzzleCamera.gameObject.SetActive (true);
		Screen.showCursor = true;
		this.GetComponent<BoxCollider> ().enabled = false;
		return true;
	}

	// Show the player again and then disable all of the puzzle related game things such as the camera.
	public void Stop()
	{
		isActive = false;
		PuzzleCamera.gameObject.SetActive (false);
		Screen.showCursor = false;
		this.GetComponent<BoxCollider> ().enabled = true;
		_tempPlayer.transform.gameObject.SetActive (true);
		_tempPlayer = null;
	}

	private void MoveToFreeSpace()
	{
		int newIdx = 0;

		// Check in each direction for the free spot and once found, move there (updating the _BoardPiece in the process).
		if (CheckFreeLeft ()) 
		{
			newIdx = (currIndex - 1);
			_BoardPieces [currIndex].controller.Move (PieceController.Direction.Left, offset);
			_BoardPieces [currIndex].position.x -= 1;
		} 
		else if (CheckFreeAbove ()) 
		{
			newIdx = (_BoardPieces [currIndex].position.y - 1) * (rows) + (_BoardPieces [currIndex].position.x); 
			_BoardPieces [currIndex].controller.Move (PieceController.Direction.Up, offset);
			_BoardPieces [currIndex].position.y -= 1;
		} 
		else if (CheckFreeRight ()) 
		{
			newIdx = (currIndex + 1); 
			_BoardPieces [currIndex].controller.Move (PieceController.Direction.Right, offset);
			_BoardPieces [currIndex].position.x += 1;
		} 
		else if (CheckFreeBelow ())
		{
			newIdx = (_BoardPieces [currIndex].position.y + 1) * (rows) + (_BoardPieces [currIndex].position.x); 
			_BoardPieces [currIndex].controller.Move (PieceController.Direction.Down, offset);
			_BoardPieces [currIndex].position.y += 1;
		} 
		else
			return; 

		// Update _BoardPieces with the new positions.
		PuzzlePiece temp = _BoardPieces[currIndex];
		_BoardPieces[currIndex] = _BoardPieces[newIdx];
		_BoardPieces[newIdx] = temp;
		
		currIndex = newIdx;
	}

	private void RandomizeBoard()
	{
		// Attempt to move a random piece 200 times (creates a randomized puzzle).
		for (int i = 0; i < 200; i++)
		{
			int rand = Random.Range (0, (rows * columns));
			currIndex = rand;

			if (_BoardPieces [currIndex].controller) 
			{
				// Locate the free space and move there if there is one.
				MoveToFreeSpace();
			}
		}

		currIndex = -1;
	}

	private bool CheckFreeLeft()
	{
		// Check if the piece is at the far left.
		if(_BoardPieces[currIndex].position.x  == 0)
			return false;
		// Check if there is a piece on the left. 
		else if(_BoardPieces[currIndex - 1].controller)
			return false;

		// If neither then the space is free.
		return true;
	}

	private bool CheckFreeAbove()
	{
		// Check if the piece is at top.
		if(_BoardPieces[currIndex].position.y  == 0)
			return false;
		// Check if there is a piece above. 
		else if(_BoardPieces[(_BoardPieces[currIndex].position.y - 1) * (rows) + ( _BoardPieces[currIndex].position.x)].controller)
			return false;

		// If neither then the space is free.
		return true;
	}

	private bool CheckFreeRight()
	{
		// Check if the piece is at the far right.
		if(_BoardPieces[currIndex].position.x  == columns - 1)
			return false;
		// Check if there is a piece on the right. 
		else if(_BoardPieces[currIndex + 1].controller)
			return false;

		// If neither then the space is free.
		return true;
	}

	private bool CheckFreeBelow()
	{
		// Check if the piece is at bottom.
		if(_BoardPieces[currIndex].position.y  == rows - 1)
			return false;
		// Check if there is a piece below. 
		else if(_BoardPieces[(_BoardPieces[currIndex].position.y + 1)* (rows ) + ( _BoardPieces[currIndex].position.x)].controller)
			return false;

		// If neither then the space is free.
		return true;
	}

}
