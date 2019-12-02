using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Paperio {
	public class MainCamera : MonoBehaviour {
		
		public void Start() {
		}

		void Update() {
			if (GameManager.instance.initialized) {
				transform.position = GameManager.instance.player.transform.position + new Vector3(0, 6f, 0);
			}
			if (Input.GetKeyDown(KeyCode.Space)) {
				SceneManager.LoadScene("Game");
			}
		}
	}
}
