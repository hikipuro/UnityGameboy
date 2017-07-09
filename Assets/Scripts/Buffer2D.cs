using UnityEngine;
using System.Collections;

public class Buffer2D {
	private int _width = 0;
	private int _height = 0;
	private Texture2D _texture;
	private Color32[] _pixels;

	public Buffer2D(int width, int height) {
		_texture = new Texture2D(width, height);
		_width = _texture.width;
		_height = _texture.height;
		_initPixels();
	}

	public int width { get {return _width;} }
	public int height { get {return _height;} }
	public Color32[] pixels { get {return _pixels;} }
	public Texture2D texture { get {return _texture;} }
	
	public void SetPixel(int index, byte r, byte g, byte b, byte a = 255) {
		_pixels[index].r = r;
		_pixels[index].g = g;
		_pixels[index].b = b;
		_pixels[index].a = a;
	}

	public void Update() {
		_texture.SetPixels32(_pixels);
		_texture.Apply();
	}

	private void _initPixels() {
		_pixels = new Color32[_width * _height];

		int i = 0;
		for (int y = 0; y < _height; y++) {
			for (int x = 0; x < _width; x++) {
				_pixels[i++] = new Color32();
			}
		}
	}

}
