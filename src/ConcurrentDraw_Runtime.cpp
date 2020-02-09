// Общий заголовок
#include "ConcurrentDrawLib.h"

// Функция запрашивает данные из канала считывания
float *GetDataFromStreamEx ()
	{
	// Отмена выгрузки при закрытом канале
	if (!AS->cdChannel)
		return NULL;

	// Получение (вариант предельной выгрузки)
	if (BASS_ChannelGetData (AS->cdChannel, &AS->cdFFT, BASS_DATA_AVAILABLE) < FFT_VALUES_COUNT)
	// Этот вызов призван отсекать заполнение массива FFT неполными (на рисунке – дырявыми) сетами.
	// Однако отменять обновление фрейма здесь, как это было ранее, нет смысла: задвоенные сеты
	// выглядят куда лучше движущегося рывками изображения.
	// К тому же эта отсечка сильно тормозила обновление полиморфных палитр
		return AS->cdFFT;
	
	BASS_ChannelGetData (AS->cdChannel, &AS->cdFFT, FFT_MODE);	// Сколько выгрузит, столько и отрисуем

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
	if (v > CD_BMPINFO_MAXCOLOR)
		v = CD_BMPINFO_MAXCOLOR;

	// Пересчёт пика
	if ((AS->cdFFTPeakEvLowEdge | AS->cdFFTPeakEvHighEdge) == 0)	// Состояние отключения
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
	uint y, x, v, i;
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
				v = GetScaledAmplitudeEx (SD_SCALE * y / AS->sgFrameHeight + 1);

				// Отрисовка (делаем так, чтобы исключить лишнюю арифметику на первом шаге)
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = v;
				for (i = 1; i < AS->sgSpectrogramStep; i++)
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = v;

				// Маркер
				AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = CD_BMPINFO_MAXCOLOR;
				}

			// Движение маркера
			AS->sgCurrentPosition = (AS->sgCurrentPosition + AS->sgSpectrogramStep) % AS->sgFrameWidth;
			break;

		// Движущаяся спектрограмма
		case 2:
			for (y = 0; y < AS->sgFrameHeight; y++)
				{
				// Сдвиг изображения
				for (x = 0; x < AS->sgFrameWidth - AS->sgSpectrogramStep; x += AS->sgSpectrogramStep)
					{
					for (i = AS->sgSpectrogramStep; i > 0; i--)
						AS->sgBuffer[y * AS->sgFrameWidth + x + i - 1] = AS->sgBuffer[y * AS->sgFrameWidth + x + i];
					}

				// Получение значения
				v = GetScaledAmplitudeEx (SD_SCALE * y / AS->sgFrameHeight + 1);

				// Отрисовка
				for (i = 1; i <= AS->sgSpectrogramStep; i++)
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = v;
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
						AS->sgBuffer[y * AS->sgFrameWidth + x] = AS->cdBackgroundColorNumber;
					}
				else
					{
					for (y = 0; y < v; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = CD_HISTO_BAR;	// Обрезаем края палитр

					for (y = v; y < AS->sgFrameHeight; y++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = AS->cdBackgroundColorNumber;
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
					{
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = AS->cdBackgroundColorNumber;
					for (i = 1; i < AS->sgSpectrogramStep; i++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = 
							AS->cdBackgroundColorNumber;
					}

				for (y = (AS->sgFrameHeight - v2) / 2; y < (AS->sgFrameHeight + v2) / 2; y++)
					{
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = v;
					for (i = 1; i < AS->sgSpectrogramStep; i++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = v;
					}

				for (y = (AS->sgFrameHeight + v2) / 2; y < AS->sgFrameHeight; y++)
					{
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = AS->cdBackgroundColorNumber;
					for (i = 1; i < AS->sgSpectrogramStep; i++)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = 
							AS->cdBackgroundColorNumber;
					}

				// Маркер
				for (y = 0; y < AS->sgFrameHeight; y++)
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + AS->sgSpectrogramStep) % 
						AS->sgFrameWidth] = CD_BMPINFO_MAXCOLOR;

				// Движение маркера
				AS->sgCurrentPosition = (AS->sgCurrentPosition + AS->sgSpectrogramStep) % AS->sgFrameWidth;
				}
			else
				{
				for (y = 0; y < AS->sgFrameHeight; y++)
					{
					// Сдвиг изображения
					for (x = 0; x < AS->sgFrameWidth - AS->sgSpectrogramStep; x += AS->sgSpectrogramStep)
						{
						for (i = AS->sgSpectrogramStep; i > 0; i--)
							AS->sgBuffer[y * AS->sgFrameWidth + x + i - 1] = AS->sgBuffer[y * AS->sgFrameWidth + x + i];
						}
					}

				// Отрисовка
				for (y = 0; y < (AS->sgFrameHeight - v2) / 2; y++)
					{
					for (i = 1; i <= AS->sgSpectrogramStep; i++)
						AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = AS->cdBackgroundColorNumber;
					}

				for (y = (AS->sgFrameHeight - v2) / 2; y < (AS->sgFrameHeight + v2) / 2; y++)
					{
					for (i = 1; i <= AS->sgSpectrogramStep; i++)					
						AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = v;
					}

				for (y = (AS->sgFrameHeight + v2) / 2; y < AS->sgFrameHeight; y++)
					{
					for (i = 1; i <= AS->sgSpectrogramStep; i++)
						AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = AS->cdBackgroundColorNumber;
					}
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
