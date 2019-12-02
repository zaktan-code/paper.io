using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorScript : Editor {

	[MenuItem("Paper.io/Set Cells")]
	public static void SetCells() {

		GameObject cell = Resources.Load<GameObject>("Prefabs/Cell");

		GameObject floor = GameObject.Find("Floor");
		int x = -(int)(floor.transform.localScale.x * 10f);
		int z = -(int)(floor.transform.localScale.z * 10f);

		for (int i = x; i <= Mathf.Abs(x); i++) {
			for (int j = z; j <= Mathf.Abs(z); j++) {
				GameObject newCell = Instantiate(cell, new Vector3(i / 2f, 0.5f, j / 2f), Quaternion.Euler(90, 0, 0)).gameObject;
				if (Mathf.Abs(i) == Mathf.Abs(x) || Mathf.Abs(j) == Mathf.Abs(z)) {
					newCell.tag = "NoSpawn";
				}
			}
		}
	}
}
