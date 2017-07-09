extern "C"
{
	__declspec(dllexport)
	void __stdcall loadRom(int size, unsigned char* dat);
	
	__declspec(dllexport)
	void __stdcall nextFrame();
	
	__declspec(dllexport)
	void __stdcall initTgbDual();
	
	__declspec(dllexport)
	void __stdcall freeTgbDual();
	
	__declspec(dllexport)
	unsigned char* __stdcall getBytes();

	__declspec(dllexport)
	short* __stdcall getSoundBytes(int size);

	__declspec(dllexport)
	void __stdcall setKeys(int down, int up, int left, int right, int a, int b, int select, int start);
}