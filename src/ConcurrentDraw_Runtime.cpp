// Общий заголовок
#include "ConcurrentDrawLib.h"

// Функция запрашивает данные из канала считывания
float *GetDataFromStreamEx ()
	{
	// Контроль
	if (!AS->cdChannel)
		return NULL;

	// Получение
	if (BASS_ChannelGetData (AS->cdChannel, &AS->cdFFT, BASS_DATA_AVAILABLE) < FFT_VALUES_COUNT)
		return NULL;

	if (BASS_ChannelGetData (AS->cdChannel, &AS->cdFFT, FFT_MODE) < 0)
		return NULL;

	return AS->cdFFT;
	}

// Функция возвращает масштабированное значение амплитуды на указанной частоте
CD_API(uchar) GetScaledAmplitudeEx (uint FrequencyLevel)
	{
	// Переменные
	uint v;

	// Контроль
	if (FrequencyLevel >= FFT_VALUES_COUNT)
		return 0;
	
	// Получение (uint, чтобы исключить суммирование с переносом)
	v = (uint)(sqrt (AS->cdFFT[FrequencyLevel]) * AS->cdFFTScale);

	// Вписывание в диапазон (uchar)
	if (v > CD_BMPINFO_COLORS_COUNT - 1)
		v = CD_BMPINFO_COLORS_COUNT - 1;

	// Пересчёт пика
	if (AS->cdFFTPeakEvLowEdge | AS->cdFFTPeakEvHighEdge == 0)	// Состояние отключения
		return v;

	if ((FrequencyLevel >= AS->cdFFTPeakEvLowEdge) && (FrequencyLevel <= AS->cdFFTPeakEvHighEdge) && 
		(v >= AS->cdFFTPeakEvLowLevel))
		{
		AS->cdFFTPeak = 0xFF;
		rand ();	// Привязка поведения ГПСЧ к звуковому потоку
		}

	// Завершено
	return v;
	}

// Функция-таймер перерисовки спектрограммы
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2)
	{
	// Переменные
	uint y, x, v;
	ulong v2;

	// Заполнение массива (если возможно)
	if (!GetDataFromStreamEx ())
		return;

	// Переход в режим обновления
	AS->updating = 1;

	// Обновление полиморфной палитры
	if (AS->cdPolymorphUpdateCounter != 0)	// Нулевое значение используется как блокировка
		{
		if (AS->cdPolymorphUpdateCounter++ >= POLYMORPH_UPDATE_PAUSE)
			{
			FillPaletteEx (AS->cdCurrentPalette);
			}
		}

	// Обновление спектрограммы, если требуется
	switch (AS->sgSpectrogramMode)
		{
		// Без спектрограммы
		default:
		case 0:
			break;

		// Статичная спектрограмма с курсором
		case 1:
			for (y = 0; y < AS->sgFrameHeight; y++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (384 * y / AS->sgFrameHeight);

				// Отрисовка
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] =
#ifdef SG_DOUBLE_WIDTH
				AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + 1) % AS->sgFrameWidth] = 
#endif
				v;

				// Маркер
				AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + SG_STEP) % AS->sgFrameWidth] = 255;
				}

			// Движение маркера
			AS->sgCurrentPosition = (AS->sgCurrentPosition + SG_STEP) % AS->sgFrameWidth;
			break;

		// Движущаяся спектрограмма
		case 2:
			for (y = 0; y < AS->sgFrameHeight; y++)
				{
				// Сдвиг изображения
				for (x = 0; x < AS->sgFrameWidth - SG_STEP; x += SG_STEP)
					{
					AS->sgBuffer[y * AS->sgFrameWidth + x] = AS->sgBuffer[y * AS->sgFrameWidth + x + 1]
#ifdef SG_DOUBLE_WIDTH
					= AS->sgBuffer[y * AS->sgFrameWidth + x + 2]
#endif
					;
					}

				// Получение значения
				v = GetScaledAmplitudeEx (384 * y / AS->sgFrameHeight);

				// Отрисовка
#ifdef SG_DOUBLE_WIDTH
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 2] = 
#endif
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 1] = v;
				}
			break;

		// Гистограмма и симметричная гистограмма
		case 3:
		case 4:
			for (x = 0; x < AS->sgFrameWidth; x++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (AS->cdHistogramFFTValuesCount * (ulong)x / AS->sgFrameWidth);

				// Перемасштабирование
				v = AS->sgFrameHeight * (ulong)v / CD_BMPINFO_COLORS_COUNT;	

				if (AS->sgSpectrogramMode == 3)
					{
					for (y = 0; y < v; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + x] = CD_HISTO_BAR;	// Обрезаем края палитр
					for (y = v; y < AS->sgFrameHeight; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + x] = CD_HISTO_SPACE;
					}
				else
					{
					for (y = 0; y < v; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = CD_HISTO_BAR;	// Обрезаем края палитр

					for (y = v; y < AS->sgFrameHeight; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = CD_HISTO_SPACE;
					}
				}
			break;

		// Статичная и движущаяся амплитудная
		case 5:
		case 6:
			for (x = v2 = 0; x < AS->cdHistogramFFTValuesCount; x++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (x);
				v2 += v * v;
				}

			// Перемасштабирование
			v2 = sqrt(v2 / AS->cdHistogramFFTValuesCount);
			v = v2;
			v2 = AS->sgFrameHeight * (ulong)v2 / CD_BMPINFO_COLORS_COUNT;

			if (AS->sgSpectrogramMode == 5)
				{
				// Линии
				for (y = 0; y < (AS->sgFrameHeight - v2) / 2; y++)
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = 
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + 1) % AS->sgFrameWidth] = 
#endif
					CD_HISTO_SPACE;

				for (y = (AS->sgFrameHeight - v2) / 2; y < (AS->sgFrameHeight + v2) / 2; y++)
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = 
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + 1) % AS->sgFrameWidth] = 
#endif
					v;

				for (y = (AS->sgFrameHeight + v2) / 2; y < AS->sgFrameHeight; y++)
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = 
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + 1) % AS->sgFrameWidth] = 
#endif
					CD_HISTO_SPACE;

				// Маркер
				for (y = 0; y < AS->sgFrameHeight; y++)
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + SG_STEP) % AS->sgFrameWidth] = 255;

				// Движение маркера
				AS->sgCurrentPosition = (AS->sgCurrentPosition + SG_STEP) % AS->sgFrameWidth;
				}
			else
				{
				for (y = 0; y < AS->sgFrameHeight; y++)
					{
					// Сдвиг изображения
					for (x = 0; x < AS->sgFrameWidth - SG_STEP; x += SG_STEP)
						{
						AS->sgBuffer[y * AS->sgFrameWidth + x] = AS->sgBuffer[y * AS->sgFrameWidth + x + 1]
	#ifdef SG_DOUBLE_WIDTH
						= AS->sgBuffer[y * AS->sgFrameWidth + x + 2]
	#endif
						;
						}
					}

				// Отрисовка
				for (y = 0; y < (AS->sgFrameHeight - v2) / 2; y++)
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 2] = 
#endif
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 1] = CD_HISTO_SPACE;

				for (y = (AS->sgFrameHeight - v2) / 2; y < (AS->sgFrameHeight + v2) / 2; y++)
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 2] = 
#endif
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 1] = v;

				for (y = (AS->sgFrameHeight + v2) / 2; y < AS->sgFrameHeight; y++)
#ifdef SG_DOUBLE_WIDTH
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 2] = 
#endif
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - 1] = CD_HISTO_SPACE;
				}
			break;
		}

	// Обновление завершено
	AS->updating = 0;
	}

// Функция выполняет ручное обновление данных FFT вместо встроенного таймера
CD_API(void) UpdateFFTDataEx ()
	{
	UpdateFFT (0, 0, 0, 0, 0);
	}
