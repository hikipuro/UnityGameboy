using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Windows.Forms;

public class UI : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnGUI() {
		if (GUI.Button(new Rect(10, UnityEngine.Screen.height - 35, 100, 25), "Load")) {
			GameObject gameObject = GameObject.Find("Screen");
			if (gameObject == null) {
				return;
			}
			Gameboy gameboy = gameObject.GetComponent<Gameboy>();
			if (gameboy == null) {
				return;
			}

			gameboy.SetStopFlag(true);
			string path = "";
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Gameboy (*.gb;*.gbc)|*.gb;*.gbc|すべてのファイル(*.*)|*.*";
			if (dialog.ShowDialog() == DialogResult.OK) {
				path = dialog.FileName;
			}
			//string path = EditorUtility.OpenFilePanel("Gameboy ROM File", "", "gb;*.gbc");
			gameboy.SetStopFlag(false);

			if (path.Length != 0) {
				gameboy.LoadROM(path);
			}
		}
		
		if (GUI.Button(new Rect(120, UnityEngine.Screen.height - 35, 100, 25), "Reset")) {
			GameObject gameObject = GameObject.Find("GB");
			if (gameObject == null) {
				return;
			}
			gameObject.transform.position = new Vector3(0, 0.83f, -8.76f);
			gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
		}
	}
}
