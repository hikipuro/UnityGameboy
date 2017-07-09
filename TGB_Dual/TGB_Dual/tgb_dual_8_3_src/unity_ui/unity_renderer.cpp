#define _CRT_SECURE_NO_WARNINGS

//#include <AS3/AS3.h>
#include "unity_renderer.h"
#include <stdio.h>
#include <stdarg.h>
#include <time.h>
#include "../../export.h"

unsigned char* bytes;
short* soundBytes;
unsigned int map_24[0x10000];
unsigned char keys;

//sound_renderer *snd_render2;
unity_renderer *self;

unsigned char* __stdcall getBytes();

short* __stdcall getSoundBytes();

void __stdcall setKeys(int down, int up, int left, int right, int a, int b, int select, int start);

unsigned char* __stdcall getBytes() {
	return bytes;
	/*
	inline_as3(
		"var byteArray:ByteArray = new ByteArray();\n"
		//"byteArray.endian = 'littleEndian';\n"
		"CModule.readBytes(%0, %1, byteArray);\n"
		"return byteArray;\n" :: "r"(bytes), "r"(160 * 144 * 4)
	);
	*/
}

short* __stdcall getSoundBytes(int size) {
	//printf("getSoundBytes: \n");
	//int size = 2048;
	if (!self->snd_render) {
		//FILE* log_file = freopen("debug.txt", "a", stdout);
		//fprintf(log_file, "getSoundBytes error 0\n");
		//fclose(log_file);
		/*
		inline_as3(
			"return null;\n"
		);
		*/
		return (short*)0;
	}
	self->snd_render->render((short*)soundBytes, size);

	/*
	inline_as3(
		"var byteArray:ByteArray = new ByteArray();\n"
		"CModule.readBytes(%0, %1, byteArray);\n"
		"return byteArray;\n" :: "r"(soundBytes), "r"(size * 4)
	);
	*/
	return soundBytes;
}

void __stdcall setKeys(int down, int up, int left, int right, int a, int b, int select, int start) {
	/*
	int down = 0, up = 0, left = 0, right = 0;
	int a = 0, b = 0, select = 0, start = 0;
	
	inline_as3(
		"%0 = down;\n"
		"%1 = up;\n"
		"%2 = left;\n"
		"%3 = right;\n"
		"%4 = a;\n"
		"%5 = b;\n"
		"%6 = select;\n"
		"%7 = start;\n"
		: 
		"=r"(down),
		"=r"(up),
		"=r"(left),
		"=r"(right),
		"=r"(a),
		"=r"(b),
		"=r"(select), 
		"=r"(start)
	); 
	*/
	
	if (start > 0) {
		start = 1;
	}
	
	keys = 0;
	keys |=	((down & 1) << 4) |
			((up & 1) << 5) | 
			((left & 1) << 6) | 
			((right & 1) << 7) | 
			((start & 1) << 3) | 
			((select & 1) << 2) |
			((b & 1) << 1) |
			(a & 1);
}

unity_renderer::unity_renderer()
{
	self = this;
	key_state=0;
	cur_time=0;
	color_type=2; 
	
	bytes = (unsigned char*)malloc(160 * 144 * 4);
	soundBytes = (short*)malloc(2048 * 2 * 4); 
	
	//snd_render = NULL;
	//snd_render2 = NULL;
	
	for (int i=0;i<0x10000;i++){
		map_24[i]=0xFF000000 | ((i&0xf800)<<8)|((i&0x7c0)<<5)|((i&0x3f)<<2);
		//map_24[i]=0xFFff0000;
	}
}

unity_renderer::~unity_renderer()
{
	free(bytes);
	free(soundBytes);
}


void unity_renderer::render_screen(byte *buf,int width,int height,int depth)
{
	int i,j;
	word* wbuf = (word*)buf;
	unsigned int* dbytes = (unsigned int*)bytes;
	
	for (i = 0; i < height; i++) {
		for (j = 0; j < width; j++) {
			int index = i * 160 + j;
			dbytes[index] = map_24[*(wbuf++)];
			/*
			*(bytes + index + 2) = ((pixel) & 0x1F) * 8;
			*(bytes + index + 1) = ((pixel >> 5) & 0x1F) * 8;
			*(bytes + index + 0) = ((pixel >> 10) & 0x1F) * 8;
			*/
		}
	}
}

word unity_renderer::map_color(word gb_col)
{
	word r,g,b;
	int r2,g2,b2;

	r=((gb_col>>0)&0x1f)<<3;
	g=((gb_col>>5)&0x1f)<<3;
	b=((gb_col>>10)&0x1f)<<3;

	r2=m_filter.r_def+((r*m_filter.r_r+g*m_filter.r_g+b*m_filter.r_b)/((!m_filter.r_div)?1:m_filter.r_div));
	g2=m_filter.g_def+((r*m_filter.g_r+g*m_filter.g_g+b*m_filter.g_b)/((!m_filter.g_div)?1:m_filter.g_div));
	b2=m_filter.b_def+((r*m_filter.b_r+g*m_filter.b_g+b*m_filter.b_b)/((!m_filter.b_div)?1:m_filter.b_div));

	r2=(r2>255)?255:((r2<0)?0:r2);
	g2=(g2>255)?255:((g2<0)?0:g2);
	b2=(b2>255)?255:((b2<0)?0:b2);

	gb_col=(r>>3)|((g>>3)<<5)|((b>>3)<<10);

	// xBBBBBGG GGGRRRRR から変換
	if (color_type==0) // ->RRRRRGGG GGxBBBBB に変換 (565)
		return ((gb_col&0x1F)<<11)|((gb_col&0x3e0)<<1)|((gb_col&0x7c00)>>10)|((gb_col&0x8000)>>10);
	if (color_type==1) // ->xRRRRRGG GGGBBBBB に変換 (1555)
		return ((gb_col&0x1F)<<10)|(gb_col&0x3e0)|((gb_col&0x7c00)>>10)|(gb_col&0x8000);
	if (color_type==2) // ->RRRRRGGG GGBBBBBx に変換 (5551)
		return ((gb_col&0x1F)<<11)|((gb_col&0x3e0)<<1)|((gb_col&0x7c00)>>9)|(gb_col>>15);
	else
		return gb_col;
}

word unity_renderer::unmap_color(word gb_col)
{
	// xBBBBBGG GGGRRRRR へ変換
	if (color_type==0) // ->RRRRRGGG GGxBBBBB から変換 (565)
		return (gb_col>>11)|((gb_col&0x7c0)>>1)|(gb_col<<10)|((gb_col&0x40)<<10);
	if (color_type==1) // ->xRRRRRGG GGGBBBBB から変換 (1555)
		return ((gb_col&0x7c00)>>10)|(gb_col&0x3e0)|((gb_col&0x1f)<<10)|(gb_col&0x8000);
	if (color_type==2) // ->RRRRRGGG GGBBBBBx から変換 (5551)
		return (gb_col>>11)|((gb_col&0x7c0)>>1)|((gb_col&0x3e)<<9)|(gb_col<<15);
	else
		return gb_col;
}

void unity_renderer::set_pad(int stat)
{
	key_state=stat;
}

int unity_renderer::check_pad()
{
	static bool test = false;
	test = !test;
	
	//if (test) {
		return keys;
	//}
	
	//return key_state;
}

byte unity_renderer::get_time(int type)
{
	dword now=fixed_time-cur_time;

	switch(type){
	case 8: // 秒
		return (byte)(now%60);
	case 9: // 分
		return (byte)((now/60)%60);
	case 10: // 時
		return (byte)((now/(60*60))%24);
	case 11: // 日(L)
		return (byte)((now/(24*60*60))&0xff);
	case 12: // 日(H)
		return (byte)((now/(256*24*60*60))&1);
	}
	return 0;
}

void unity_renderer::set_time(int type,byte dat)
{
	dword now=fixed_time;
	dword adj=now-cur_time;

	switch(type){
	case 8: // 秒
		adj=(adj/60)*60+(dat%60);
		break;
	case 9: // 分
		adj=(adj/(60*60))*60*60+(dat%60)*60+(adj%60);
		break;
	case 10: // 時
		adj=(adj/(24*60*60))*24*60*60+(dat%24)*60*60+(adj%(60*60));
		break;
	case 11: // 日(L)
		adj=(adj/(256*24*60*60))*256*24*60*60+(dat*24*60*60)+(adj%(24*60*60));
		break;
	case 12: // 日(H)
		adj=(dat&1)*256*24*60*60+(adj%(256*24*60*60));
		break;
	}
	cur_time=now-adj;
}

void unity_renderer::output_log(char *mes,...)
{
	va_list vl;
	char buf[256];

	va_start(vl,mes);
	vsprintf(buf,mes,vl);
	
	//FILE* log_file = freopen("debug.txt", "a", stdout);
    //fprintf(log_file, "%s\n", buf);
	//fclose(log_file);
	//printf("%s\n", buf);

	va_end(vl);
	return;
}
