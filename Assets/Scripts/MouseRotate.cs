using UnityEngine;
using System.Collections;

public class MouseRotate : MonoBehaviour {
	private Vector3 _prevPosition = Vector3.zero;
	private Gameboy _gameboy = null;

	// Use this for initialization
	void Start () {
		GameObject gameObject = GameObject.Find("Screen");
		if (gameObject == null) {
			return;
		}
		_gameboy = gameObject.GetComponent<Gameboy>();
	}
	
	// Update is called once per frame
	void Update () {
		bool stopFlag = false;
		if (Input.GetMouseButton(0)) {
			stopFlag = true;
			Vector3 diff = Input.mousePosition - _prevPosition;
			transform.Rotate(diff.y, 0, 0);
			transform.Rotate(0, -diff.x, 0);
		} else if (Input.GetMouseButton(1)) {
			stopFlag = true;
			Vector3 diff = Input.mousePosition - _prevPosition;
			transform.Translate(diff.x / 10f, diff.y / 10f, 0, Space.World);
		}
		_prevPosition = Input.mousePosition;

		float wheel = Input.GetAxis("Mouse ScrollWheel");
		if (wheel != 0) {
			stopFlag = true;
			transform.Translate(0, 0, wheel * 5, Space.World);
		}

		_gameboy.SetStopFlag(stopFlag);
	}
}
