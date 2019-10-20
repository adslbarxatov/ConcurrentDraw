/////////////////////////////////////////////////////
// ����� ������
//#define BASSTEST
//#define EXETEST
//#define SD_DOUBLE_WIDTH

/////////////////////////////////////////////////////
// ������������ ����� � ����������
#include <windows.h>
#include <stdio.h>
#include <math.h>
#include <malloc.h>

#include "BASS/bass.h"
#pragma comment (lib, "BASS/bass.lib")
#pragma comment (lib, "winmm.lib")

/////////////////////////////////////////////////////
// ��������������� �����
#define __u		unsigned

#define schar	__int8
#define sint	__int16
#define slong	__int32
#define sdlong	__int64

#define uchar	__u schar
#define uint	__u sint
#define ulong	__u slong
#define udlong	__u sdlong

#define CD_API(t)	extern __declspec(dllexport) t __stdcall

/////////////////////////////////////////////////////
// ���������
#define MAX_RECORD_DEVICES			10
#define MAX_DEVICE_NAME_LENGTH		128

#define BASS_VERSION				0x02040E00

#define FFT_VALUES_COUNT			1024
#define FFT_MODE					BASS_DATA_FFT2048

#define MINFRAMEWIDTH				128
#define MAXFRAMEWIDTH				2048
#define MINFRAMEHEIGHT				128
#define MAXFRAMEHEIGHT				FFT_VALUES_COUNT

#define CD_BMPINFO_COLORS_COUNT		256

#define PEAK_EVALUATION_LOW_EDGE	0
#define PEAK_EVALUATION_HIGH_EDGE	10
#define PEAK_EVALUATION_LOW_LEVEL	0xF0

#define NAMES_DELIMITER				'\x1'
#define NAMES_DELIMITER2			"\x1"

/////////////////////////////////////////////////////
// ������������ �������
#define SD_FFT_SCALE				765.0f		// 255 * 3, ��� ��������������� �������� ���������
#ifdef SD_DOUBLE_WIDTH
	#define SD_STEP 2							// ������ ���� �������������
#else
	#define SD_STEP 1
#endif

// ������ ������������ ���������������� �������� ���������
#define INBOUND_FFT_VALUE(fftv)	if (fftv > CD_BMPINFO_COLORS_COUNT - 1) fftv = CD_BMPINFO_COLORS_COUNT - 1;

// ������ ��������� �������� ���� ��� ���������� �������
// (�������� �������� ������, ����������� �������� ���������)
#define UPDATE_PEAK(freq,fftv)	if ((freq >= cdFFTPeakEvLowEdge) && (freq <= cdFFTPeakEvHighEdge) && \
								(fftv >= cdFFTPeakEvLowLevel)) cdFFTPeak = 0xFF;

/////////////////////////////////////////////////////
// ���������-��������� ��������� BITMAP
union CD_BITMAPINFO
	{
	struct CD_BMPINFO 
		{
		BITMAPINFOHEADER    header;
		RGBQUAD             colors[CD_BMPINFO_COLORS_COUNT];
		} cd_bmpinfo;
	uchar cd_bmpinfo_ptr [sizeof (struct CD_BMPINFO)];
	};

/////////////////////////////////////////////////////
// ���������� �������
float *GetDataFromStreamEx ();
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);
void FillPalette (RGBQUAD *Palette, uchar PaletteNumber);

/////////////////////////////////////////
// ������� �������

// ������� �������� ����� ��������� ������ ����� (������ �������� �� 128 �� ���)
CD_API(uchar) GetDevicesEx (schar **Devices);

// ������� ��������� ������� ���������� ������ �� ��������� ������
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber);
// -10 - ������������ ������ ����������
// ������������� ���� ������ � ������ -1 (UnknownError) - ������ BASS

// ������� ��������� ������� ����������
CD_API(void) DestroySoundStreamEx ();

// ������� �������������� �������������
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, 
	uchar PaletteNumber, uchar MoveThrough);

// ������� ������� �������� �������������
CD_API(void) DestroySpectrogramEx ();

// ������� ���������� ������� ����� �������������
CD_API(HBITMAP) GetSpectrogramFrameEx ();

// ������� ���������� ������� �������� ���������
CD_API(uchar) GetCurrentPeakEx ();

// ������� ������������� ������� ����������� �������� ��������
CD_API(void) SetPeakEvaluationParametersEx (uchar LowEdge, uchar HighEdge, uchar LowLevel);

// ������� ���������� �������� ���� ������� ������� � ��������� ��������
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness);

// ������� ���������� �������� ��������� ������
CD_API(schar *) GetPalettesNamesEx ();

#ifdef BASSTEST
	// �������� ������� ��� ���������� BASS
	CD_API(void) BASSTest ();
#endif BASSTEST
