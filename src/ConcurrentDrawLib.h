/////////////////////////////////////////////////////
// Заголовочные файлы и библиотеки
#include <malloc.h>
#include <math.h>
#include <stdio.h>
#include <time.h>
#include <windows.h>

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
#define CD_VERSION					1,20,0,0
#define CD_VERSION_S				"1.20.0.0"
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
#define MAXFRAMEHEIGHT				512
#define POLYMORPH_UPDATE_PAUSE		25

#define CD_HISTO_BAR				(192 * y / AS->sgFrameHeight + 48)
#define CD_HISTO_SPACE				8

#define PEAK_EVALUATION_LOW_EDGE	0
#define PEAK_EVALUATION_HIGH_EDGE	4
#define PEAK_EVALUATION_LOW_LEVEL	0xF8
#define CD_DEFAULT_FFT_SCALE_MULT	40
#define CD_SECOND_FFT_SCALE_MULT	25.5f
#define CD_MIN_FFT_SCALE_MULT		10
#define CD_MAX_FFT_SCALE_MULT		100

#define NAMES_DELIMITER_C			'\x1'
#define NAMES_DELIMITER_S			"\x1"

#define CD_RECORD_FREQ				44100
#define CD_TIMER_TPS				25

/////////////////////////////////////////////////////
// Пререквизиты таймера
#ifdef SD_DOUBLE_WIDTH
	#define SG_STEP 2				// Ширина шага спектрограммы
#else
	#define SG_STEP 1
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
// Структура-описатель состояния программы
struct CDSTATE
	{
	HRECORD cdChannel;				// Дескриптор чтения
	uint cdChannelLength;				// Длина потока (при инициализации из файла будет ненулевой)
	float cdFFT[FFT_VALUES_COUNT];	// Массив значений, получаемый из канала
	MMRESULT cdFFTTimer;			// Дескриптор таймера запроса данных из буфера
	uchar updating;					// Флаг, указывающий на незавершённость последнего процесса обновления FFT

	HBITMAP sgBMP;					// Дескриптор BITMAP спектральной диаграммы
	uchar *sgBuffer;				// Буфер спектральной диаграммы
	uint sgFrameWidth;				// Размеры изображения спектрограммы
	uint sgFrameHeight;
	uint sgCurrentPosition;			// Текущая позиция на статичной спектрограмме
	uchar sgSpectrogramMode;		// Режим спектрограммы (0 - выключена, 1 - с курсором, 
										// 2 - движущаяся, 3 - гистограмма, 4 - симметричная гистограмма)

	float cdFFTScale;				// Масштаб значений FFT
	uint cdHistogramFFTValuesCount;	// Количество значений FFT, используемых для гистограмм
	uchar cdFFTPeak;				// Текущее пиковое значение
	uchar cdFFTPeakEvLowEdge;		// Нижняя граница диапазона определения пика
	uchar cdFFTPeakEvHighEdge;		// Верхняя граница диапазона определения пика
	uchar cdFFTPeakEvLowLevel;		// Наименьшая амплитуда, на которой определяется пик

	union CD_BITMAPINFO sgBMPInfo;	// Данные для инициализации спектрограммы
	union CD_BITMAPINFO sgBeatsInfo;// Палитра для бит-детектора

	RGBQUAD cdPolymorphColors[5];		// Опорные цвета полиморфной палитры
	uint cdPolymorphUpdateCounter;	// Счётчик обновления полиморфной палитры
	uint cdCurrentPalette;			// Текущая палитра
	};

/////////////////////////////////////////////////////
// Внутренние функции
float *GetDataFromStreamEx ();
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);
sint GetRandomValue (sint Min, sint Max);

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

// Функции формируют палитры приложения
void FillPalette_Default (void);
void FillPalette_Sea (void);
void FillPalette_Fire (void);
void FillPalette_Grey (void);
void FillPalette_Sunrise (void);
void FillPalette_Acid (void);
void FillPalette_7MissedCalls (void);
void FillPalette_SailOnTheSea (void);
void FillPalette_Mirror (void);
void FillPalette_Blood (void);
void FillPalette_PolymorphRandom (uchar Polymorph, uchar Monocolor);

CD_API(void) FillPaletteEx (uchar PaletteNumber);

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber);

// Функция возвращает версию данной библиотеки
CD_API(schar *) GetCDLibVersionEx ();

// Функция устанавливает количество значений FFT, которое будет использоваться в гистограммах
CD_API(void) SetHistogramFFTValuesCountEx (uint Count);

// Функция возвращает длину текущего файлового потока (для аудиовыхода всегда 0)
CD_API(uint) GetChannelLengthEx ();

// Функция возвращает ссылку на состояние программы
#define AS	GetAppState()
struct CDSTATE *GetAppState (void);

// Функция инициализирует состояние программы
void InitAppState (void);
