// Общий заголовок
#include "ConcurrentDrawLib.h"

// Состояние программы
static struct CDSTATE appState;
static uchar appStateInited = 0;

// Функция возвращает ссылку на состояние программы
struct CDSTATE *GetAppState (void)
	{
	return &appState;
	}

// Функция инициализирует состояние программы
void InitAppState (void)
	{
	// Контроль
	if (appStateInited)
		return;

	// Запуск ГПСЧ
	srand ((uint)time (NULL));

	// Установка начальных значений
	appState.cdChannel = NULL;
	appState.cdChannelLength = 0;
	appState.cdFFTTimer = NULL;
	appState.updating = 0;
	appState.sgBMP = NULL;
	
	appState.sgCurrentPosition = 0;
	appState.sgSpectrogramMode = 0;
	
	appState.cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT * CD_SECOND_FFT_SCALE_MULT;
	appState.cdHistogramFFTValuesCount = DEFAULT_FFT_VALUES_COUNT;
	
	appState.cdFFTPeak = 0;
	appState.cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE;
	appState.cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE;
	appState.cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;
	
	appState.cdPolymorphUpdateCounter = 0;
	appState.cdCurrentPalette = 0;
	
	appState.cdPolymorphColors[4].rgbRed = appState.cdPolymorphColors[4].rgbGreen = 
		appState.cdPolymorphColors[4].rgbBlue = CD_BMPINFO_MAXCOLOR;
	appState.cdPolymorphColors[0].rgbRed = appState.cdPolymorphColors[0].rgbGreen = 
		appState.cdPolymorphColors[0].rgbBlue = 0;

	// Завершено
	appStateInited = 1;
	}
