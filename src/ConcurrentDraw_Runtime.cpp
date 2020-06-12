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

	// Принудительное выравнивание по потоку при чтении из файла
	if (AS->cdChannelLength)
		{
		BASS_ChannelSetPosition (AS->cdChannel, AS->cdChannelPosition * AS->cdChannelBPF, BASS_POS_BYTE);
		AS->cdChannelPosition = AS->cdChannelPosition + 1;
		}

	return AS->cdFFT;
	}

// Функция возвращает масштабированное значение амплитуды на указанной частоте
CD_API(uchar) GetScaledAmplitudeEx (uint FrequencyLevel)
	{
	// Переменные
	uint v, fl = FrequencyLevel;

	// Контроль
	if (AS->cdChannelLength)
		fl += 3;
	else
		fl++;
	if (fl >= FFT_VALUES_COUNT)
		fl = FFT_VALUES_COUNT - 1;
	
	// Получение (uint, чтобы исключить суммирование с переносом)
	v = CD_FFT_EV_METHOD (AS->cdFFT[fl]);

	// Вписывание в диапазон (uchar)
	if (v > CD_BMPINFO_MAXCOLOR)
		v = CD_BMPINFO_MAXCOLOR;

	// Пересчёт пика
	if ((AS->cdFFTPeakEvLowEdge | AS->cdFFTPeakEvHighEdge) == 0)	// Состояние отключения
		return (uchar)v;

	if ((fl >= AS->cdFFTPeakEvLowEdge) && (fl <= AS->cdFFTPeakEvHighEdge) && (v >= AS->cdFFTPeakEvLowLevel))
		AS->cdFFTPeak = 0xFF;

	// Завершено
	return (uchar)v;
	}

// Функции, отрисовывающие отдельные виды спектрограм
void DrawSpectrogram (uchar Mode)
	{
	uint i, v, y;
	sint x;

	for (y = 0; y < AS->sgFrameHeight; y++)
		{
		// Получение значения
		v = GetScaledAmplitudeEx (SD_SCALE * y / AS->sgFrameHeight);

		switch (Mode)
			{
			// Движущаяся симметричная
			case 2:
				// Сдвиг изображения
				for (x = AS->sgFrameWidth - 2 * AS->sgSpectrogramStep - 4; x >= 0 ; x -= 2 * AS->sgSpectrogramStep)
					{
					for (i = AS->sgSpectrogramStep; i > 0; i--)
						AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2 + i + 1] = 
							AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2 - i - 2] = 
							AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2 + 1];
					}

				// Отрисовка
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth / 2 - 2] = 
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth / 2 + 1] = (uchar)v;
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth / 2] = 
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth / 2 - 1] = CD_BMPINFO_MAXCOLOR;
				break;

			// Движущаяся
			case 1:
				// Сдвиг изображения
				for (x = 0; x < AS->sgFrameWidth - AS->sgSpectrogramStep; x += AS->sgSpectrogramStep)
					{
					for (i = AS->sgSpectrogramStep; i > 0; i--)
						AS->sgBuffer[y * AS->sgFrameWidth + x + i - 1] = AS->sgBuffer[y * AS->sgFrameWidth + x + i];
					}

				// Отрисовка
				for (i = 1; i <= AS->sgSpectrogramStep; i++)
					AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = (uchar)v;
				break;

			// Статичная
			case 0:
			default:
				// Отрисовка (делаем так, чтобы исключить лишнюю арифметику на первом шаге)
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = (uchar)v;
				for (i = 1; i < AS->sgSpectrogramStep; i++)
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = (uchar)v;

				// Маркер
				AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = CD_BMPINFO_MAXCOLOR;

				break;
			}
		}

	// Движение маркера
	if (Mode == 0)
		AS->sgCurrentPosition = (AS->sgCurrentPosition + AS->sgSpectrogramStep) % AS->sgFrameWidth;
	}

// Symmetric - битовое поле; бит 0 - горизонтальная симметрия, бит 1 - вертикальная симметрия
void DrawHistogram (uchar Symmetric)
	{
	uint v, x, y;

	for (x = 0; x < AS->sgFrameWidth; x++)
		{
		// Получение значения
		v = GetScaledAmplitudeEx (AS->cdHistogramFFTValuesCount * (ulong)x / AS->sgFrameWidth);

		// Перемасштабирование
		v = AS->sgFrameHeight * (ulong)v / CD_BMPINFO_COLORS_COUNT;	

		// Симметричная
		for (y = 0; y < AS->sgFrameHeight; y++)
		if (Symmetric & 0x1)
			{
			if (Symmetric & 0x2)
				{
				if ((y > (AS->sgFrameHeight - v) / 2) && (y < (AS->sgFrameHeight + v) / 2))
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = CD_HISTO_BAR_S;	
				else
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = AS->cdBackgroundColorNumber;
				}
			else
				{
				if (y < v)
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = CD_HISTO_BAR;	// Обрезаем края палитр
				else
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth + x) / 2] =
					AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgFrameWidth - x) / 2] = AS->cdBackgroundColorNumber;
				}
			}

		// Простая
		else
			{
			if (Symmetric & 0x2)
				{
				if ((y > (AS->sgFrameHeight - v) / 2) && (y < (AS->sgFrameHeight + v) / 2))
					AS->sgBuffer[y * AS->sgFrameWidth + x] = CD_HISTO_BAR_S;
				else
					AS->sgBuffer[y * AS->sgFrameWidth + x] = AS->cdBackgroundColorNumber;
				}
			else
				{
				if (y < v)
					AS->sgBuffer[y * AS->sgFrameWidth + x] = CD_HISTO_BAR;	// Обрезаем края палитр
				else
					AS->sgBuffer[y * AS->sgFrameWidth + x] = AS->cdBackgroundColorNumber;
				}
			}
		}
	}

void DrawAmplitudes (uchar Moving)
	{
	// Переменные
	uint i, v, x, y;
	ulong v2;

	for (x = v2 = 0; x < AS->cdHistogramFFTValuesCount; x++)
		{
		// Получение значения
		v = GetScaledAmplitudeEx (x);
		v2 += v * v;
		}

	// Перемасштабирование
	v2 = (ulong)sqrt (v2 / AS->cdHistogramFFTValuesCount);
	v = v2;
	v2 = AS->sgFrameHeight * (ulong)v2 / CD_BMPINFO_COLORS_COUNT;

	// Статичная
	if (!Moving)
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
			AS->sgBuffer[y * AS->sgFrameWidth + AS->sgCurrentPosition] = (uchar)v;
			for (i = 1; i < AS->sgSpectrogramStep; i++)
				AS->sgBuffer[y * AS->sgFrameWidth + (AS->sgCurrentPosition + i) % AS->sgFrameWidth] = (uchar)v;
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
	// Движущаяся
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
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = (uchar)v;
			}

		for (y = (AS->sgFrameHeight + v2) / 2; y < AS->sgFrameHeight; y++)
			{
			for (i = 1; i <= AS->sgSpectrogramStep; i++)
				AS->sgBuffer[y * AS->sgFrameWidth + AS->sgFrameWidth - i] = AS->cdBackgroundColorNumber;
			}
		}
	}

// Функция-таймер перерисовки спектрограммы
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2)
	{
	// Заполнение массива (если возможно)
	if (!GetDataFromStreamEx ())
		return;

	// Переход в режим обновления
	AS->updating = 1;

	// Обновление полиморфной палитры
	if (AS->cdPolymorphUpdateCounter != 0)	// Нулевое значение используется как блокировка
		{
		if (AS->cdPolymorphUpdateCounter++ >= POLYMORPH_UPDATE_PAUSE)
			FillPaletteEx (AS->cdCurrentPalette);
		}

	// Обновление спектрограммы, если требуется
	switch (AS->sgSpectrogramMode)
		{
		// Без спектрограммы
		default:
		case 0:
			break;

		// Статичная спектрограмма с курсором и движущаяся спектрограмма
		case 1:
		case 2:
		case 3:
			DrawSpectrogram (AS->sgSpectrogramMode - 1);
			break;

		// Гистограмма в четырёх вариантах симметрии
		case 4:
		case 5:
		case 6:
		case 7:
			DrawHistogram (AS->sgSpectrogramMode - 4);
			break;

		// Статичная и движущаяся амплитудная
		case 8:
		case 9:
			DrawAmplitudes (AS->sgSpectrogramMode - 8);
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
