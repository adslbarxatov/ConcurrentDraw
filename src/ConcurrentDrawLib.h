/////////////////////////////////////////////////////
// Режим работы
//#define BASSTEST
//#define SD_DOUBLE_WIDTH

/////////////////////////////////////////////////////
// Заголовочные файлы и библиотеки
#include <windows.h>
#include <stdio.h>
#include <math.h>
#include <malloc.h>

#include "BASS/bass.h"
#pragma comment (lib, "BASS/bass.lib")
#pragma comment (lib, "winmm.lib")

/////////////////////////////////////////////////////
// Переопределения типов
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
// Константы
#define BASS_VERSION				0x02040E00
#define CD_VERSION					1,12,0,0
#define CD_VERSION_S				"1.12.0.0"
#define CD_PRODUCT					"ConcurrentDraw visualization tool's BASS adapter"
#define CD_COMPANY					"RD AAOW"

#define MAX_RECORD_DEVICES			10
#define MAX_DEVICE_NAME_LENGTH		128

#define FFT_VALUES_COUNT			1024
#define DEFAULT_FFT_VALUES_COUNT	128
#define FFT_MODE					BASS_DATA_FFT2048

#define MINFRAMEWIDTH				128
#define MAXFRAMEWIDTH				2048
#define MINFRAMEHEIGHT				128
#define CD_BMPINFO_COLORS_COUNT		256
#define MAXFRAMEHEIGHT				CD_BMPINFO_COLORS_COUNT	// до 11025 Гц

#define PEAK_EVALUATION_LOW_EDGE	0
#define PEAK_EVALUATION_HIGH_EDGE	4
#define PEAK_EVALUATION_LOW_LEVEL	0xF8
#define CD_DEFAULT_FFT_SCALE_MULT	40
#define CD_MIN_FFT_SCALE_MULT		10
#define CD_MAX_FFT_SCALE_MULT		100

#define NAMES_DELIMITER_C			'\x1'
#define NAMES_DELIMITER_S			"\x1"

/////////////////////////////////////////////////////
// Пререквизиты таймера
#ifdef SD_DOUBLE_WIDTH
	#define SD_STEP 2				// Ширина шага спектрограммы
#else
	#define SD_STEP 1
#endif

/////////////////////////////////////////////////////
// Структура-описатель заголовка BITMAP
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
// Внутренние функции
float *GetDataFromStreamEx ();
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);

/////////////////////////////////////////
// Внешние функции

// Функция получает имена устройств вывода звука (массив символов по 128 на имя)
CD_API(uchar) GetDevicesEx (schar **Devices);

// Функция запускает процесс считывания данных со звукового вывода
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber);
// -10 - недопустимая версия библиотеки, -11 - дескриптор уже занят
// Положительные коды ошибок и ошибка -1 (UnknownError) - ошибки BASS

// Функция запускает процесс считывания данных из звукового файла
CD_API(sint) InitializeFileStreamEx (schar *FileName);

// Функция завершает процесс считывания
CD_API(void) DestroySoundStreamEx ();

// Функция выполняет ручное обновление данных FFT вместо встроенного таймера
CD_API(void) UpdateFFTDataEx ();

// Функция инициализирует спектрограмму
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, 
	uchar PaletteNumber, uchar SpectrogramMode);

// Функция удаляет активную спектрограмму
CD_API(void) DestroySpectrogramEx ();

// Функция возвращает текущий фрейм спектрограммы
CD_API(HBITMAP) GetSpectrogramFrameEx ();

// Функция возвращает текущее значение амплитуды
CD_API(uchar) GetCurrentPeakEx ();

// Функция устанавливает метрики определения пикового значения
CD_API(void) SetPeakEvaluationParametersEx (uchar LowEdge, uchar HighEdge, 
	uchar LowLevel, uchar FFTScaleMultiplier);

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness);

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ();

// Функция возвращает ограничивающие размеры фреймов спектрограмм
CD_API(udlong) GetSpectrogramFrameMetricsEx ();

// Функция возвращает стандартные метрики определения пикового значения
CD_API(ulong) GetDefaultPeakEvaluationParametersEx ();

// Функция возвращает масштабированное значение амплитуды на указанной частоте
CD_API(uchar) GetScaledAmplitudeEx (uint FrequencyLevel);

// Функция формирует палитру
CD_API(void) FillPaletteEx (uchar PaletteNumber);

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber);

// Функция возвращает версию данной библиотеки
CD_API(schar *) GetCDLibVersionEx ();

// Функция устанавливает количество значений FFT, которое будет использоваться в гистограммах
CD_API(void) SetHistogramFFTValuesCountEx (uint Count);

// Функция возвращает длину текущего файлового потока (для аудиовыхода всегда 0)
CD_API(uint) GetChannelLengthEx ();

#ifdef BASSTEST
	// Тестовая функция для библиотеки BASS
	CD_API(void) BASSTest ();
#endif
