// Общий заголовок
#include "ConcurrentDrawLib.h"

// Состояние программы
static union CDSTATE appState;
static uchar appStateInited = 0;

// Функция возвращает ссылку на состояние программы
union CDSTATE *GetAppState (void)
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
	appState.cdState_St.cdChannel = NULL;
	appState.cdState_St.channelLength = 0;
	appState.cdState_St.cdFFTTimer = NULL;
	appState.cdState_St.updating = 0;
	appState.cdState_St.sgBMP = NULL;
	
	appState.cdState_St.sgCurrentPosition = 0;
	appState.cdState_St.sgSpectrogramMode = 0;
	
	appState.cdState_St.cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT * CD_SECOND_FFT_SCALE_MULT;
	appState.cdState_St.cdHistogramFFTValuesCount = DEFAULT_FFT_VALUES_COUNT;
	
	appState.cdState_St.cdFFTPeak = 0;
	appState.cdState_St.cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE;
	appState.cdState_St.cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE;
	appState.cdState_St.cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;
	
	appState.cdState_St.polymorphUpdateCounter = 0;
	appState.cdState_St.cdCurrentPalette = 0;
	
	appState.cdState_St.polymorphColors[4].rgbRed = AS.polymorphColors[4].rgbGreen = AS.polymorphColors[4].rgbBlue = 255;
	appState.cdState_St.polymorphColors[0].rgbRed = AS.polymorphColors[0].rgbGreen = AS.polymorphColors[0].rgbBlue = 0;

	// Завершено
	appStateInited = 1;
	}
