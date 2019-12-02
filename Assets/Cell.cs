using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperio;
using System.Linq;

namespace Paperio {

	public class Cell : MonoBehaviour {

		public CellState state;
		public Movement movement;

		public static void SetCellState(Transform cell, CellState state, Player player) {
			switch (state) {
				case CellState.Home:
					cell.SetParent(player.homeCellsParent);
					player.homeCells.Add(cell);
					cell.GetComponent<Cell>().state = CellState.Home;
					cell.gameObject.tag = "Home";
					cell.GetComponent<MeshRenderer>().enabled = true;
					cell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Home");
					break;
				case CellState.Track:
					cell.SetParent(player.trackCellsParent);
					player.trackCells.Add(cell);
					cell.GetComponent<Cell>().state = CellState.Track;
					cell.gameObject.tag = "Track";
					cell.GetComponent<MeshRenderer>().enabled = true;
					cell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Track");
					break;
				case CellState.Default:
					cell.SetParent(GameManager.instance.cellsParent);
					if (cell.gameObject.tag == "Home") {
						player.homeCells.Remove(cell);
					} else if (cell.gameObject.tag == "Track") {
						player.trackCells.Remove(cell);
					}
					cell.GetComponent<Cell>().state = CellState.Default;
					cell.gameObject.tag = "Untagged";
					cell.GetComponent<MeshRenderer>().enabled = false;
					cell.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Cell");
					break;
			}
		}

		public static bool IsInnerCell(Transform cell, Movement move) {
			int trackHitsCount = 0;
			string side;

			RaycastHit[] hits;

			if (move == Movement.Left || move == Movement.Right) {
				hits = Physics.RaycastAll(cell.position, cell.up, 20f);
				side = "up";
				if (hits.Where(hit => hit.collider.tag == "Track").Count() == 0 || hits.Where(hit => hit.collider.tag == "Home").Count() != 0) {
					hits = Physics.RaycastAll(cell.position, -cell.up, 20f);
					side = "down";
				}
			}
			else {
				hits = Physics.RaycastAll(cell.position, cell.right, 20f);
				side = "right";
				if (hits.Where(hit => hit.collider.tag == "Track").Count() == 0 || hits.Where(hit => hit.collider.tag == "Home").Count() != 0) {
					hits = Physics.RaycastAll(cell.position, -cell.right, 20f);
					side = "left";
				}
			}
			if (hits.Where(hit => hit.collider.tag == "Track").Count() == 0 || hits.Where(hit => hit.collider.tag == "Home").Count() != 0) {
				return false;
			}

			List<RaycastHit> allHits = new List<RaycastHit>();
			foreach (var hit in hits) {
				if (hit.collider.tag != "Track") {
					continue;
				}
				if (allHits.Where(hit1 =>
						Mathf.Abs(Vector3.Distance((hit1).collider.transform.position, hit.collider.transform.position)) <= hit.collider.transform.lossyScale.x + 0.1f &&
						 Mathf.Abs(Vector3.Distance((hit1).collider.transform.position, hit.collider.transform.position)) >= hit.collider.transform.lossyScale.x - 0.1f &&
						(hit1).collider.GetComponent<Cell>().movement == hit.collider.GetComponent<Cell>().movement).Count() != 0) {
					return false;

				}
				allHits.Add(hit);
				trackHitsCount++;
			}
			//Debug.Log("Hits count: " + trackHitsCount + "; " + side);
			return trackHitsCount % 2 != 0;
		}

		public static bool HomeAround(Transform cell) {
			RaycastHit[] hitUp, hitDown, hitLeft, hitRight;

			hitUp = Physics.RaycastAll(cell.position, cell.up, 20f, ~(1 << 8));
			if (hitUp.Where(hit => hit.collider.CompareTag("Home")).Count() == 0) {
				return false;
			}
			hitDown = Physics.RaycastAll(cell.position, -cell.up, 20f, ~(1 << 8));
			if (hitDown.Where(hit => hit.collider.CompareTag("Home")).Count() == 0) {
				return false;
			}
			hitRight = Physics.RaycastAll(cell.position, cell.right, 20f, ~(1 << 8));
			if (hitRight.Where(hit => hit.collider.CompareTag("Home")).Count() == 0) {
				return false;
			}
			hitLeft = Physics.RaycastAll(cell.position, -cell.right, 20f, ~(1 << 8));
			if (hitLeft.Where(hit => hit.collider.CompareTag("Home")).Count() == 0) {
				return false;
			}

			return true;
		}

		/*public static bool AllAroundWalk(Transform cell) {
			if (cell.CompareTag("Track") || cell.CompareTag("Home") || cell.CompareTag("HomeAround")) {
				return true;
			}
			if (cell.CompareTag("Border")) {
				return false;
			}
			if (!HomeAround(cell)) {
				return false;
			}
			cell.tag = "HomeAround";
			GameObject.Find("Player").GetComponent<Player>().SetCellState(cell, CellState.HomeAround);

			RaycastHit hitUp, hitDown, hitLeft, hitRight;

			hitUp = Physics.RaycastAll(cell.position, cell.up, 0.5f, ~(1 << 8))[0];
			if (!HomeAround(hitUp.collider.transform)) {
				return false;
			}
			hitDown = Physics.RaycastAll(cell.position, -cell.up, 0.5f, ~(1 << 8))[0];
			if (!HomeAround(hitDown.collider.transform)) {
				return false;
			}
			hitRight = Physics.RaycastAll(cell.position, cell.right, 0.5f, ~(1 << 8))[0];
			if (!HomeAround(hitRight.collider.transform)) {
				return false;
			}
			hitLeft = Physics.RaycastAll(cell.position, -cell.right, 0.5f, ~(1 << 8))[0];
			if (!HomeAround(hitLeft.collider.transform)) {
				return false;
			}

			return true;
		}*/

		public static Transform GetNeighborCell(Transform cell, Movement side) {

			RaycastHit[] hits = null;

			switch (side) {
				case Movement.Up:
					hits = Physics.RaycastAll(cell.position, cell.up, 0.5f, ~(1 << 8));
					break;
				case Movement.Down:
					hits = Physics.RaycastAll(cell.position, -cell.up, 0.5f, ~(1 << 8));
					break;
				case Movement.Left:
					hits = Physics.RaycastAll(cell.position, -cell.right, 0.5f, ~(1 << 8));
					break;
				case Movement.Right:
					hits = Physics.RaycastAll(cell.position, cell.right, 0.5f, ~(1 << 8));
					break;
			}

			IEnumerable<RaycastHit> cellsHits = hits.Where(hit => hit.collider.name == "Cell(Clone)" && !hit.collider.CompareTag("Home") && !hit.collider.CompareTag("Track"));
			if (cellsHits.Count() == 0) {
				return null;
			}

			return cellsHits.First().collider.transform;

		}

		public static void Fill(Transform cell, Player player, bool start = false) {
			if (cell.CompareTag("Track") || cell.CompareTag("Home") || cell.CompareTag("Border")) {
				return;
			}

			SetCellState(cell, CellState.Home, player);

			RaycastHit hitUp, hitDown, hitLeft, hitRight;

			hitUp = Physics.RaycastAll(cell.position, cell.up, 0.5f, ~(1 << 8))[0];
			Fill(hitUp.collider.transform, player);

			hitDown = Physics.RaycastAll(cell.position, -cell.up, 0.5f, ~(1 << 8))[0];
			Fill(hitDown.collider.transform, player);

			hitRight = Physics.RaycastAll(cell.position, cell.right, 0.5f, ~(1 << 8))[0];
			Fill(hitRight.collider.transform, player);

			hitLeft = Physics.RaycastAll(cell.position, -cell.right, 0.5f, ~(1 << 8))[0];
			Fill(hitLeft.collider.transform, player);
		}

		void Start() {
			//movement = null;
			state = CellState.Default;
		}

		void Update() {

		}

	}

	public enum CellState {
		Default,
		Track,
		Home
	}

}
