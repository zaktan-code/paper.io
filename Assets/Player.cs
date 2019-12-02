using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Paperio;

namespace Paperio {

	public class Player : MonoBehaviour {

		private const bool SHOW_START_END_CELLS = false;
		private const bool SHOW_CURRENT_CELL = false;
		private const bool SHOW_FILL_START_CELL = false;
		private const bool SHOW_CHECKED_CELLS = false;

		public float movementSpeed = 0.03f;

		private bool moveLeft = false, moveRight = false, moveUp = false, moveDown = false;
		private bool allowLeft = true, allowRight = true, allowUp = true, allowDown = true;

		private Movement currentMovement;
		private Movement previousMovement;

		private GameObject currentTrack;

		private Transform[] cells;
		private Transform startCell;
		private Transform endCell;
		private Transform currentCell;
		private Transform previousCell;
		public List<Transform> homeCells { get; set; } 
		public List<Transform> trackCells { get; set; }

		public Transform homeCellsParent { get; private set; }
		public Transform trackCellsParent { get; private set; }

		public bool inHome {
			get { return currentCell.gameObject.tag == "Home"; }
		}

		private Transform GetCurrentCell() {
			IEnumerable<Transform> curCellNum = null;
			switch (currentMovement) {
				case Movement.Left:
					curCellNum = cells.Where(cellTransform => cellTransform.position.z == currentCell.position.z &&
						cellTransform.position.x < currentCell.position.x &&
							Mathf.Abs(transform.position.x - cellTransform.position.x) < transform.lossyScale.x * 1f);
					break;
				case Movement.Right:
					curCellNum = cells.Where(cellTransform => cellTransform.position.z == currentCell.position.z &&
						cellTransform.position.x > currentCell.position.x &&
							Mathf.Abs(transform.position.x - cellTransform.position.x) < transform.lossyScale.x * 1f);
					break;
				case Movement.Up:
					curCellNum = cells.Where(cellTransform => cellTransform.position.x == currentCell.position.x &&
						cellTransform.position.z > currentCell.position.z &&
							Mathf.Abs(transform.position.z - cellTransform.position.z) < transform.lossyScale.y * 1f);
					break;
				case Movement.Down:
					curCellNum = cells.Where(cellTransform => cellTransform.position.x == currentCell.position.x &&
						cellTransform.position.z < currentCell.position.z &&
							Mathf.Abs(transform.position.z - cellTransform.position.z) < transform.lossyScale.y * 1f);
					break;
			}

			return (curCellNum.Count() == 0) ? currentCell : curCellNum.First();
		}

		private Transform GetEndCell() {
			IEnumerable<Transform> endCellNum = null;

			switch (currentMovement) {
				case Movement.Left:
					endCellNum = cells.Where(cellTransform => cellTransform.position.z == startCell.position.z &&
						cellTransform.position.x == cells.Select(cellTransform1 => cellTransform1.position.x).Min());
					break;
				case Movement.Right:
					endCellNum = cells.Where(cellTransform => cellTransform.position.z == startCell.position.z &&
						cellTransform.position.x == cells.Select(cellTransform1 => cellTransform1.position.x).Max());
					break;
				case Movement.Up:
					endCellNum = cells.Where(cellTransform => cellTransform.position.x == startCell.position.x &&
						cellTransform.position.z == cells.Select(cellTransform1 => cellTransform1.position.z).Max());
					break;
				case Movement.Down:
					endCellNum = cells.Where(cellTransform => cellTransform.position.x == startCell.position.x &&
						cellTransform.position.z == cells.Select(cellTransform1 => cellTransform1.position.z).Min());
					break;
			}

			return endCellNum.First();
		}

		private void MakeStartHome() {

			IEnumerable<Transform> newHomeCells = cells
				.Where(cell => Mathf.Abs(Vector3.Distance(currentCell.position, cell.position)) <= Mathf.Sqrt(Mathf.Pow(cell.lossyScale.x, 2) + Mathf.Pow(cell.lossyScale.y, 2)));

			foreach (Transform homeCell in newHomeCells) {
				Cell.SetCellState(homeCell, CellState.Home, this);
			}

		}

		void Start() {
			homeCellsParent = GameObject.Find("HomeCells").transform;
			trackCellsParent = GameObject.Find("TrackCells").transform;
			cells = GameManager.instance.cells;

			homeCells = new List<Transform>();
			trackCells = new List<Transform>();

			startCell = cells.Where(cell => cell.gameObject.tag != "NoSpawn").RandomOrDefault();
			currentCell = previousCell = startCell;
			transform.position = startCell.position;

			MakeStartHome();
			SetPercents();

			if (transform.position.x == 0 && transform.position.z == 0) {
				currentMovement = previousMovement = (Movement)Random.Range(0, 3);
			} else if (transform.position.x >= 0 && transform.position.z >= 0) {
				currentMovement = previousMovement = (new Movement[] { Movement.Down, Movement.Left }).RandomOrDefault();
			} else if (transform.position.x >= 0 && transform.position.z <= 0) {
				currentMovement = previousMovement = (new Movement[] { Movement.Up, Movement.Left }).RandomOrDefault();
			} else if (transform.position.x <= 0 && transform.position.z >= 0) {
				currentMovement = previousMovement = (new Movement[] { Movement.Down, Movement.Right }).RandomOrDefault();
			} else if (transform.position.x <= 0 && transform.position.z <= 0) {
				currentMovement = previousMovement = (new Movement[] { Movement.Up, Movement.Right }).RandomOrDefault();
			}
			endCell = GetEndCell();
		}

		void Update() {

			moveLeft = Input.GetKeyDown(KeyCode.LeftArrow);
			moveRight = Input.GetKeyDown(KeyCode.RightArrow);
			moveUp = Input.GetKeyDown(KeyCode.UpArrow);
			moveDown = Input.GetKeyDown(KeyCode.DownArrow);

			if (moveLeft) {
				if (allowLeft && currentMovement != Movement.Left && currentMovement != Movement.Right) {
					currentMovement = Movement.Left;
				}
			} else if (moveRight) {
				if (allowRight && currentMovement != Movement.Right && currentMovement != Movement.Left) {
					currentMovement = Movement.Right;
				}
			} else if (moveUp) {
				if (allowUp && currentMovement != Movement.Up && currentMovement != Movement.Down) {
					currentMovement = Movement.Up;
				}
			} else if (moveDown) {
				if (allowDown && currentMovement != Movement.Down && currentMovement != Movement.Up) {
					currentMovement = Movement.Down;
				}
			}

			currentCell.GetComponent<Cell>().movement = currentMovement;

			if (SHOW_CURRENT_CELL) {
				currentCell.GetComponent<MeshRenderer>().enabled = false;
			}
			previousCell = currentCell;
			currentCell = GetCurrentCell();

			if (currentCell.tag == "Track") {
				EndGame();
			}

			if (!inHome && previousCell != currentCell || inHome) {
				if (previousCell.gameObject.tag != "Home") {
					Cell.SetCellState(previousCell, CellState.Track, this);
				}
			}

			if (inHome && trackCells.Count() != 0) {
				int iterCount = 0;
				Transform[] nearCells = new Transform[2];
				
				foreach (Transform trackCell in trackCells) {

					Movement trackCellMovement = trackCell.GetComponent<Cell>().movement;

					nearCells[0] = Cell.GetNeighborCell(trackCell,
						(trackCell.GetComponent<Cell>().movement == Movement.Up || trackCell.GetComponent<Cell>().movement == Movement.Down) ?
							Movement.Left : Movement.Up);
					nearCells[1] = Cell.GetNeighborCell(trackCell,
						(trackCell.GetComponent<Cell>().movement == Movement.Up || trackCell.GetComponent<Cell>().movement == Movement.Down) ?
							Movement.Right : Movement.Down);


					foreach (Transform nearCell in nearCells) {
						Debug.Log("iter number: " + iterCount);
						iterCount++;
						if (nearCell == null) {
							continue;
						}

						if (Cell.IsInnerCell(nearCell, trackCellMovement)) {
							Cell.Fill(nearCell, this, true);
							if (SHOW_FILL_START_CELL) {
								nearCell.GetComponent<MeshRenderer>().enabled = true;
								nearCell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/RedCell");
							}
						}
						else if (SHOW_CHECKED_CELLS) {
							nearCell.GetComponent<MeshRenderer>().enabled = true;
							nearCell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/YellowCell");
						}
					}
				}

				foreach (Transform cell in trackCells) {
					Cell.SetCellState(cell, CellState.Home, this);
				}
				trackCells.Clear();

				SetPercents();
			}

			if (SHOW_CURRENT_CELL) {
				currentCell.GetComponent<MeshRenderer>().enabled = true;
				currentCell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/RedCell");
				startCell.GetComponent<MeshRenderer>().enabled = true;
				endCell.GetComponent<MeshRenderer>().enabled = true;
			}

			if (currentMovement != previousMovement) {
				transform.position = currentCell.position;

				if (SHOW_START_END_CELLS) {
					startCell.GetComponent<MeshRenderer>().enabled = false;
					endCell.GetComponent<MeshRenderer>().enabled = false;
				}
				previousMovement = currentMovement;
				startCell = currentCell;
				endCell = GetEndCell();
			}

			transform.position = Vector3.MoveTowards(transform.position, endCell.position, movementSpeed);

		}

		private void SetPercents() {
			GameManager.instance.homePercentsVal = (int)((float)homeCells.Count() / (float)cells.Count() * 100f);
		}

		private void EndGame() {
			this.gameObject.SetActive(false);
		}

		private void OnTriggerEnter(Collider other) {
			if (other.name.Length >= 6 && other.name.Substring(0, 6) == "Border") {
				//Debug.Log(currentCell.name);
				//EndGame();
			}
			switch (other.name) {
				case "BorderLeft":
					if (currentMovement == Movement.Left) {
						EndGame();
					}
					allowLeft = false;
					break;
				case "BorderRight":
					if (currentMovement == Movement.Right) {
						EndGame();
					}
					allowRight = false;
					break;
				case "BorderTop":
					if (currentMovement == Movement.Up) {
						EndGame();
					}
					allowUp = false;
					break;
				case "BorderBottom":
					if (currentMovement == Movement.Down) {
						EndGame();
					}
					allowDown = false;
					break;
				default:
					return;
			}
		}

		private void OnTriggerExit(Collider other) {
			switch (other.name) {
				case "BorderLeft":
					allowLeft = true;
					break;
				case "BorderRight":
					allowRight = true;
					break;
				case "BorderTop":
					allowUp = true;
					break;
				case "BorderBottom":
					allowDown = true;
					break;
				default:
					return;
			}
		}
	}

	public enum Movement {
		Left,
		Right,
		Up,
		Down
	}
}
