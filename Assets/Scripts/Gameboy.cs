using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Gameboy : MonoBehaviour {
	[DllImport ("TGB_Dual")]
	private static extern void initTgbDual();

	[DllImport ("TGB_Dual")]
	private static extern void freeTgbDual();
	
	[DllImport ("TGB_Dual")]
	private static extern void loadRom(int size, byte[] dat);
	
	[DllImport ("TGB_Dual")]
	private static extern void nextFrame();
	
	[DllImport ("TGB_Dual")]
	private static extern IntPtr getBytes();
	
	[DllImport ("TGB_Dual")]
	private static extern IntPtr getSoundBytes(int size);
	
	[DllImport ("TGB_Dual")]
	private static extern void setKeys(int down, int up, int left, int right, int a, int b, int select, int start);

	private bool _stopFlag = false;
	private bool _loadedFlag = false;

	private Buffer2D _screenBuffer;
	
	private AudioSource _audioSource;
	private List<short[]> _soundBuffers;
	private int _soundBufferCount = 2;
	private int _soundBufferIndex = 0;
	
	const string PRELOAD_FILE_PATH = "";
	//const string PRELOAD_FILE_PATH = @"Tetris.gb";
	//const string PRELOAD_FILE_PATH = @"Alleyway.gb";

	const int SCREEN_REFRESH_RATE = 60;
	const int SCREEN_WIDTH = 160;
	const int SCREEN_HEIGHT = 144;
	const int SOUND_FREQUENCY = 44100;
	const int SOUND_BUFFER_LENGTH = 2646;

	public void SetStopFlag(bool value) {
		_stopFlag = value;
	}
	
	public void LoadROM(string path) {
		int size = 0;
		byte[] bytes = new byte[1];
		//Debug.Log(File.Exists(path));
		
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
			try {
				size = (int)reader.BaseStream.Length;
				bytes = reader.ReadBytes(size);
				//Debug.Log("Length: " + size);
				//for(int i = 0; i < 10; i++) {
				//	Debug.Log(String.Format("{0:x2}", bytes[i]));
				//}
			} catch (Exception) {
				Debug.Log("LoadROM: Error");
				return;
			}
		}
		
		_loadedFlag = true;
		loadRom(size, bytes);
	}

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = SCREEN_REFRESH_RATE;

		_screenBuffer = new Buffer2D(SCREEN_WIDTH, SCREEN_HEIGHT);
		Renderer renderer = GetComponent<Renderer>();
		renderer.material.mainTexture = _screenBuffer.texture;
		//#if !UNITY_UV_STARTS_AT_TOP
		Vector3 scale = transform.localScale;
		transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
		//#endif

		_soundBuffers = new List<short[]>();
		for (int i = 0; i < _soundBufferCount; i++) {
			_soundBuffers.Add(new short[SOUND_BUFFER_LENGTH * 2]);
		}
		
		//AudioSettings.outputSampleRate = SOUND_FREQUENCY;
		//AudioSettings.SetDSPBufferSize(SOUND_BUFFER_LENGTH, 2);

		AudioConfiguration config = new AudioConfiguration();
		config.sampleRate = SOUND_FREQUENCY;
		config.dspBufferSize = SOUND_BUFFER_LENGTH;
		config.speakerMode = AudioSpeakerMode.Stereo;
		AudioSettings.Reset(config);
		
		//int bufferLength = 0;
		//int numBuffers = 0;
		//AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
		//Debug.Log("bufferLength: " + bufferLength);
		//Debug.Log("numBuffers: " + numBuffers);
		_audioSource = GetComponent<AudioSource>();
		_audioSource.Play();

		initTgbDual();
		if (PRELOAD_FILE_PATH.Length > 0) {
			LoadROM(PRELOAD_FILE_PATH);
		}
	}

	void OnApplicationQuit() {
		//Debug.Log("OnApplicationQuit");
		freeTgbDual();
	}
	
	// Update is called once per frame
	void Update () {
		if (_stopFlag == true || _loadedFlag == false) {
			return;
		}
		nextFrame();
		_updateScreen();
		_setKeyInfo();
	}
	
	void OnAudioFilterRead(float[] data, int channels) {
		if (_stopFlag == true || _loadedFlag == false) {
			return;
		}
		_readSoundData();
		
		//if (!_Buffer) {
		int length = data.Length;
		int index = _soundBufferIndex - 1;
		if (index < 0) {
			index += _soundBufferCount;
		}

		short[] buffer = _soundBuffers[index];
		for (int i = 0; i < length; i = i + channels) {
			data[i] = ((float)buffer[i]) / 32768f;
			if (channels == 2) {
				data[i + 1] = ((float)buffer[i + 1]) / 32768f;
			}
		}
	}

	private void _updateScreen() {
		int size = SCREEN_WIDTH * SCREEN_HEIGHT * 4;
		byte[] bytes = new byte[size];
		IntPtr pointer = getBytes();
		
		Marshal.Copy(pointer, bytes, 0, size);
		
		Color32[] pixels = _screenBuffer.pixels;
		
		int width = _screenBuffer.width;
		int i = 0;
		for (int y = 0; y < SCREEN_HEIGHT; y++) {
			for (int x = 0; x < SCREEN_WIDTH; x++) {
				int index = x + y * width;
				pixels[index].r = bytes[i + 2];
				pixels[index].g = bytes[i + 1];
				pixels[index].b = bytes[i + 0];
				pixels[index].a = 255;
				i += 4;
			}
		}
		_screenBuffer.Update();
	}

	private void _setKeyInfo() {
		int down = 0, up = 0, left = 0, right = 0, start = 0, select = 0, b = 0, a = 0;
		
		if (Input.GetKey(KeyCode.DownArrow)) {
			down = 1;
		}
		if (Input.GetKey(KeyCode.UpArrow)) {
			up = 1;
		}
		if (Input.GetKey(KeyCode.LeftArrow)) {
			left = 1;
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			right = 1;
		}
		if (Input.GetKey(KeyCode.A)) {
			select = 1;
		}
		if (Input.GetKey(KeyCode.S)) {
			start = 1;
		}
		if (Input.GetKey(KeyCode.Z)) {
			b = 1;
		}
		if (Input.GetKey(KeyCode.X)) {
			a = 1;
		}
		
		setKeys(down, up, left, right, a, b, select, start);
	}
	
	private void _readSoundData() {
		IntPtr ptr2 = getSoundBytes(SOUND_BUFFER_LENGTH);
		
		if (ptr2 != IntPtr.Zero) {
			Marshal.Copy(ptr2, _soundBuffers[_soundBufferIndex], 0, SOUND_BUFFER_LENGTH * 2);
		} else {
			Debug.Log("_readSoundData: Error");
		}
		
		_soundBufferIndex++;
		if (_soundBufferIndex >= _soundBufferCount) {
			_soundBufferIndex = 0;
		}
	}
	
	private void Log(string text) {
		//using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Debug2.txt", true))
		//{
			//file.WriteLine(text);
		//}
	}
}
