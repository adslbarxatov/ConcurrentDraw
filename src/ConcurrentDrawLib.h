/////////////////////////////////////////////////////
// Заголовочные файлы и библиотеки
#include <malloc.h>
#include <math.h>
#include <stdio.h>
#include <time.h>
#include <windows.h>

#include "../BASS/bass.h"
#pragma comment (lib, "BASS/bass.lib")
#pragma comment (lib, "winmm.lib")

/////////////////////////////////////////////////////
// Переопределения типов
#include "..\\Generics\\CSTypes.h"

#define CD_API(t)	extern __declspec(dllexport) t __stdcall

/////////////////////////////////////////////////////
// Константы
#define CD_VERSION					3,7,0,0
#define CD_VERSION_S				"3.7.0.0"
#define CD_PRODUCT					"BASS adapter for ConcurrentDraw"
#define CD_COMPANY					FDL_COMPANY

#define MAX_RECORD_DEVICES			10
#define MAX_DEVICE_NAME_LENGTH		128

#define DEFAULT_FFT_VALUES_COUNT	256
#define FFT_VALUES_COUNT			8192	// Для отрисовки напрямую
#define FFT_MODE					BASS_DATA_FFT16384
#define FFT_CLEAN_VALUES_COUNT		8192	// Для отрисовки из файла
#define FFT_CLEAN_MODE				BASS_DATA_FFT16384

#define MINFRAMEWIDTH				128
#define MAXFRAMEWIDTH				2048
#define MINFRAMEHEIGHT				128
#define CD_BMPINFO_COLORS_COUNT		256
#define CD_BMPINFO_MAXCOLOR			255
#define MAXFRAMEHEIGHT				540
#define SD_SCALE					MAXFRAMEHEIGHT
#define POLYMORPH_UPDATE_PAUSE		25

#define CD_HISTO_BAR				(192 * y / AS->sgFrameHeight + 32)
#define CD_HISTO_BAR_S				(384 * abs (y - AS->sgFrameHeight / 2) / AS->sgFrameHeight + 32)

#define PEAK_EVALUATION_LOW_EDGE	0
#define PEAK_EVALUATION_HIGH_EDGE	16
#define PEAK_EVALUATION_LOW_LEVEL	0xF8
#define CD_DEFAULT_FFT_SCALE_MULT	100

#define CD_SECOND_FFT_SCALE_MULT	80.0f	
#define CD_FFT_EV_METHOD(v)			v * AS->cdFFTScale

#define CD_MIN_FFT_SCALE_MULT		10
#define CD_MAX_FFT_SCALE_MULT		200

#define NAMES_DELIMITER_C			'\x1'
#define NAMES_DELIMITER_S			"\x1"

#define CD_RECORD_FREQ				44100
#define CD_TIMER_TPS				25
#define CD_VIDEO_FPS				30

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
	uint cdChannelLength;			// Длина потока (при инициализации из файла будет ненулевой)
	udlong cdChannelPosition;		// Счётчик принудительного выравнивания курсора чтения аудиофайла
	udlong cdChannelBPF;			// Число байт на фрейм для текущего канала
	float cdFFT[FFT_VALUES_COUNT];	// Массив значений, получаемый из канала
	MMRESULT cdFFTTimer;			// Дескриптор таймера запроса данных из буфера
	uchar updating;					// Флаг, указывающий на незавершённость последнего процесса обновления FFT

	/*HBITMAP sgBMP;					// Дескриптор BITMAP спектральной диаграммы*/
	/*uchar PaletteNumberOut;			// Номер палитры для вывода изображения*/
	uchar *sgBufferDraw;			// Буфер спектральной диаграммы для отрисовки
	uchar *sgBufferOut;				// Буфер спектральной диаграммы для возврата изображения
	uint sgFrameWidth;				// Размеры изображения спектрограммы
	uint sgFrameHeight;
	uint sgCurrentPosition;			// Текущая позиция на статичной спектрограмме
	uchar sgSpectrogramMode;		// Режим спектрограммы (см. описание InitializeSpectrogramEx)
	uchar sgSpectrogramStep;		// Шаг движения спектрограммы

	float cdFFTScale;				// Масштаб значений FFT
	uint cdHistogramFFTValuesCount;	// Количество значений FFT, используемых для гистограмм
	uchar cdReverseFreqOrder;		// Флаг разворота порядка частот на выходе с БПФ
	uchar cdFFTPeak;				// Текущее пиковое значение
	uint cdFFTPeakEvLowEdge;		// Нижняя граница диапазона определения пика
	uint cdFFTPeakEvHighEdge;		// Верхняя граница диапазона определения пика
	uchar cdFFTPeakEvLowLevel;		// Наименьшая амплитуда, на которой определяется пик

	union CD_BITMAPINFO sgBMPInfo;	// Данные для инициализации спектрограммы
	union CD_BITMAPINFO sgBeatsInfo;// Палитра для бит-детектора

	RGBQUAD cdPolymorphColors[5];	// Опорные цвета полиморфной палитры
	uint cdPolymorphUpdateCounter;	// Счётчик обновления полиморфной палитры
	uchar cdCurrentPalette;			// Текущая палитра
	uchar cdBackgroundColorNumber;	// Цвет текущей палитры, используемый в качестве фона спектрограмм
	};

/////////////////////////////////////////
// Функции

// Функция получает имена устройств вывода звука (массив символов по 128 на имя)
CD_API(uchar) GetDevicesEx (schar **Devices);

// Функция проверяет версию библиотеки BASS
ulong BASSVersionIsCorrect ();

// Функция запускает процесс считывания данных со звукового вывода
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber);
// -10 - недопустимая версия библиотеки, -11 - дескриптор уже занят
// Положительные коды ошибок и ошибка -1 (UnknownError) - ошибки BASS

// Функция запускает процесс считывания данных из звукового файла
CD_API(sint) InitializeFileStreamEx (schar *FileName);

// Функция завершает процесс считывания
CD_API(void) DestroySoundStreamEx ();

// Функция запрашивает данные из аудиобуфера
float *GetDataFromStream (float *CleanData);

// Функции выполняю отрисовку гистограм и спектрограм
void DrawSpectrogram (uchar Mode);
void DrawHistogram (uchar Symmetric);
void DrawAmplitudes (uchar Moving);

// Runtime-updater
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);

// Функция выполняет ручное обновление данных FFT вместо встроенного таймера
CD_API(void) UpdateFFTDataEx ();

// Функция инициализирует спектрограмму
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, 
	uchar PaletteNumber, uchar SpectrogramMode, uchar Flags);

// Функция удаляет активную спектрограмму
CD_API(void) DestroySpectrogramEx ();

// Функция возвращает текущий фрейм спектрограммы
CD_API(HBITMAP) GetSpectrogramFrameEx ();

// Функция возвращает текущее значение амплитуды
CD_API(uchar) GetCurrentPeakEx ();

// Функция устанавливает метрики определения пикового значения
CD_API(void) SetPeakEvaluationParametersEx (uint LowEdge, uint HighEdge, 
	uchar LowLevel, uchar FFTScaleMultiplier);

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness);

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ();

// Функция возвращает рекомендацию на сброс лого по признаку спецпалитр
CD_API(uchar) PaletteRequiresResetEx (uchar PaletteNumber);

// Функция возвращает ограничивающие размеры фреймов спектрограмм
CD_API(udlong) GetSpectrogramFrameMetricsEx ();

// Функция возвращает стандартные метрики определения пикового значения
CD_API(ulong) GetDefaultPeakEvaluationParametersEx ();

// Функция возвращает масштабированное значение амплитуды на указанной частоте
CD_API(uchar) GetScaledAmplitudeEx (uint FrequencyLevel);

// Функции формируют палитры приложения
void FillPalette_PolymorphRandom (uchar Reversed, uchar Polymorph, uchar Monocolor);
sint GetRandomValue (sint Min, sint Max);

CD_API(void) FillPaletteEx (uchar PaletteNumber);

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber);

// Функция получает цвет фона текущей палитры
CD_API(ulong) GetPaletteBackgroundColorEx ();

// Функция возвращает версию данной библиотеки
CD_API(schar *) GetCDLibVersionEx ();

// Функция устанавливает количество значений FFT, которое будет использоваться в гистограммах, и их порядок
CD_API(void) SetHistogramFFTValuesCountEx (uint Count, uchar Reversed);

// Функция возвращает длину текущего файлового потока (для аудиовыхода всегда 0)
CD_API(uint) GetChannelLengthEx ();

// Функция возвращает ссылку на состояние программы
#define AS	GetAppState()
struct CDSTATE *GetAppState (void);

// Функция инициализирует состояние программы
void InitAppState (void);

// Функция выгружает полные данные БПФ в виде сумм амплитуд по частотам в табличный файл
CD_API(sint) DumpSpectrogramFromFileEx (schar *SoundFileName);
