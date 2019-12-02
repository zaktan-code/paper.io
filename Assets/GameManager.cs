using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Paperio {
	public class GameManager : MonoBehaviour {

		public static GameManager instance = null;
		public bool initialized { get; private set; }

		public int homePercentsVal {
			get {
				return _homePercentsVal;
			}
			set {
				_homePercentsVal = value;
				homePercentsText.text = value.ToString() + "%";
			}
		}
		public Text homePercentsText;

		public Transform[] cells { get; private set; }
		public Transform cellsParent { get; private set; }

		public Player player { get; private set; }

		void Awake() {
			initialized = false;
			instance = this;

			//DontDestroyOnLoad(gameObject);
			Initialize();
			player = Instantiate(Resources.Load<Player>("Prefabs/Player"));

			initialized = true;
		}

		private void Initialize() {
			cellsParent = GameObject.Find("Cells").transform;
			HashSet<Transform> cellsHashSet = new HashSet<Transform>(cellsParent.GetComponentsInChildren<Transform>());
			cellsHashSet.Remove(cellsParent);
			cells = cellsHashSet.ToArray();
		}

		void Update() {

		}

		private int _homePercentsVal;
	}
}
