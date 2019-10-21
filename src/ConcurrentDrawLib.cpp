// Общий заголовок
#include "ConcurrentDrawLib.h"

// Общие переменные
HRECORD cdChannel = NULL;			// Дескриптор записи
float cdFFT[FFT_VALUES_COUNT];		// Массив значений, получаемый из канала
MMRESULT cdFFTTimer = NULL;			// Дескриптор таймера запроса данных из буфера

HBITMAP sdBMP = NULL;				// Дескриптор BITMAP спектральной диаграммы
uchar *sdBuffer;					// Буфер спектральной диаграммы
uint sdFrameWidth, sdFrameHeight,	// Размеры изображения спектрограммы
	sdCurrentPosition = 0;			// Текущая позиция на спектрограмме
uchar sdSpectrogramMode = 0;		// Режим спектрограммы (0 - выключена, 1 - с курсором, 
									// 2 - движущаяся, 3 - гистограмма)

float cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT * 
	25.5f;												// Масштаб значений FFT
uchar cdFFTPeak = 0,									// Текущее пиковое значение
	cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE,		// Нижняя граница диапазона определения пика
	cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE,	// Верхняя граница диапазона определения пика
	cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;	// Наименьшая амплитуда, на которой определяется пик

union CD_BITMAPINFO cdBMPInfo;		// Данные для инициализации спектрограммы
uchar cdCurrentPalette = 0;			// Номер текущей палитры

// Функция получает имена устройств вывода звука (массив символов по 128 на имя)
CD_API(uchar) GetDevicesEx (schar **Devices)
	{
	// Переменные
	BASS_DEVICEINFO info;
	uchar i, pos, lastLength;
	uchar devicesCount = 0;

	// Получение количества устройств
	for (devicesCount = 0; devicesCount < MAX_RECORD_DEVICES; devicesCount++)
		{
		if (!BASS_RecordGetDeviceInfo (devicesCount, &info))
			{
			break;
			}
		}

	if (devicesCount == 0)
		return devicesCount;	// Нет доступных устройств

	// Разметка массива имён
	if ((*Devices = (schar *)malloc (devicesCount * MAX_DEVICE_NAME_LENGTH)) == NULL)
		return 0;
	memset (*Devices, 0x00, devicesCount * MAX_DEVICE_NAME_LENGTH);

	// Получение имён
	for (i = pos = 0; i < devicesCount; i++)
		{
		if (!BASS_RecordGetDeviceInfo (i, &info))
			{
			free (*Devices);
			return 0;	// Фигня какая-то
			}

		lastLength = min (strlen (info.name), MAX_DEVICE_NAME_LENGTH - 1);
		memcpy (*Devices + pos, info.name, lastLength);
		*(*Devices + pos + lastLength) = NAMES_DELIMITER_C;
		pos += (lastLength + 1);
		}

	// Завершено
	return devicesCount;
	}

// Функция запускает процесс считывания данных со звукового вывода
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber)
	{
	// Получение доступа к библиотеке
	if (BASS_GetVersion () != BASS_VERSION)
		return -10;

	// Инициализация
	if (!BASS_RecordInit (DeviceNumber))
		return BASS_ErrorGetCode ();

	if (!(cdChannel = BASS_RecordStart (44100, 2, 0x00, NULL, 0)))
		return BASS_ErrorGetCode ();

	// Запуск таймера запроса данных
	cdFFTTimer = timeSetEvent (25, 25, (LPTIMECALLBACK)&UpdateFFT, 0, TIME_PERIODIC);

	// Успешно
	return 0;
	}

// Функция завершает процесс считывания
CD_API(void) DestroySoundStreamEx ()
	{
	// Контроль
	if (!cdChannel)
		return;

	// Закрытие спектрограммы, если есть
	DestroySpectrogramEx ();

	// Закрытие таймера
	if (cdFFTTimer)
		{
		timeKillEvent (cdFFTTimer);
		cdFFTTimer = NULL;
		}

	// Остановка
	BASS_RecordFree ();
	cdChannel = NULL;
	}

// Функция запрашивает данные из канала считывания
float *GetDataFromStreamEx ()
	{
	// Контроль
	ulong v;
	if (!cdChannel)
		return NULL;

	// Получение
	if ((v = BASS_ChannelGetData (cdChannel, &cdFFT, BASS_DATA_AVAILABLE)) < FFT_VALUES_COUNT)
		return NULL;

	if (BASS_ChannelGetData (cdChannel, &cdFFT, FFT_MODE ) < 0)
		return NULL;

	return cdFFT;
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
	v = (uint)(sqrt (cdFFT[FrequencyLevel]) * cdFFTScale);

	// Вписывание в диапазон (uchar)
	if (v > CD_BMPINFO_COLORS_COUNT - 1) 
		v = CD_BMPINFO_COLORS_COUNT - 1;

	// Пересчёт пика
	if ((FrequencyLevel >= cdFFTPeakEvLowEdge) && (FrequencyLevel <= cdFFTPeakEvHighEdge) && 
		(v >= cdFFTPeakEvLowLevel)) 
		cdFFTPeak = 0xFF;

	// Завершено
	return v;
	}

// Функция-таймер перерисовки спектрограммы
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2)
	{
	// Переменные
	uint y, x, v;

	// Заполнение массива (если возможно)
	if (!GetDataFromStreamEx ())
		return;

	// Обновление спектрограммы, если требуется
	switch (sdSpectrogramMode)
		{
		// Без спектрограммы
		default:
		case 0:
			break;

		// С курсором
		case 1:
			for (y = 0; y < sdFrameHeight; y++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (y + 1);

				// Отрисовка
				sdBuffer[y * sdFrameWidth + sdCurrentPosition] =
#ifdef SD_DOUBLE_WIDTH
				sdBuffer[y * sdFrameWidth + (sdCurrentPosition + 1) % sdFrameWidth] = 
#endif
				v;

				// Маркер
				sdBuffer[y * sdFrameWidth + (sdCurrentPosition + SD_STEP) % sdFrameWidth] = 255;
				}

			// Движение маркера
			sdCurrentPosition = (sdCurrentPosition + SD_STEP) % sdFrameWidth;
			break;

		// Движущаяся
		case 2:
			for (y = 0; y < sdFrameHeight; y++)
				{
				// Сдвиг изображения
				for (x = 0; x < sdFrameWidth - SD_STEP; x += SD_STEP)
					{
					sdBuffer[y * sdFrameWidth + x] = sdBuffer[y * sdFrameWidth + x + 1]
#ifdef SD_DOUBLE_WIDTH
					= sdBuffer[y * sdFrameWidth + x + 2]
#endif
					;
					}

				// Получение значения
				v = GetScaledAmplitudeEx (y + 1);

				// Отрисовка
#ifdef SD_DOUBLE_WIDTH
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 2] = 
#endif
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 1] = v;
				}
			break;

		// Гистограмма
		case 3:
			for (x = 0; x < sdFrameWidth; x++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (HISTOGRAM_FFT_VALUES_COUNT * (ulong)x / sdFrameWidth);

				// Отрисовка
				v = sdFrameHeight * (ulong)v / CD_BMPINFO_COLORS_COUNT;	// Перемасштабирование
				for (y = 0; y < v; y++)
					sdBuffer[y * sdFrameWidth + x] = 3 * y / 4 + 64;	// Убираем чёрный низ палитр
				for (y = v; y < sdFrameHeight; y++)
					sdBuffer[y * sdFrameWidth + x] = 0;
				}
			break;
		}
	}

// Функция инициализирует спектрограмму
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, uchar PaletteNumber, uchar SpectrogramMode)
	{
	// Контроль параметров
	if ((FrameWidth < MINFRAMEWIDTH) || (FrameWidth > MAXFRAMEWIDTH) ||
		(FrameHeight < MINFRAMEHEIGHT) || (FrameHeight > MAXFRAMEHEIGHT))
		return -3;
	if (!cdChannel)		// Канал должен быть инициализирован
		return -1;
	if (sdBMP)			// Спектрограмма не должна быть занята
		return -2;

	sdFrameWidth = FrameWidth & 0xFFFC;		// Хрен знает, почему, но CreateDIBSection не понимает размеры,
	if (sdFrameWidth != FrameWidth)
		sdFrameWidth += 4;

	sdFrameHeight = FrameHeight & 0xFFFC;	// которые не делятся на 4
	if (sdFrameHeight != FrameHeight)
		sdFrameHeight += 4;

	sdSpectrogramMode = SpectrogramMode;

	// Инициализация описателя
	memset (cdBMPInfo.cd_bmpinfo_ptr, 0x00, sizeof (union CD_BITMAPINFO));	// Сброс на нули всех значений

	cdBMPInfo.cd_bmpinfo.header.biSize = sizeof (BITMAPINFOHEADER);
	cdBMPInfo.cd_bmpinfo.header.biWidth = sdFrameWidth;
	cdBMPInfo.cd_bmpinfo.header.biHeight = sdFrameHeight;
	cdBMPInfo.cd_bmpinfo.header.biPlanes = 1;
	cdBMPInfo.cd_bmpinfo.header.biBitCount = 8;
	cdBMPInfo.cd_bmpinfo.header.biClrUsed = cdBMPInfo.cd_bmpinfo.header.biClrImportant = CD_BMPINFO_COLORS_COUNT;

	FillPaletteEx (PaletteNumber);

	// Создание BITMAP
	if ((sdBMP = CreateDIBSection (NULL, (BITMAPINFO *)&cdBMPInfo, DIB_RGB_COLORS, (void **)&sdBuffer, NULL, 0)) == NULL)
		return -4;

	// Завершено
	return 0;
	}

// Функция удаляет активную спектрограмму
CD_API(void) DestroySpectrogramEx ()
	{
	// Контроль
	if (!cdChannel)
		return;

	// Сброс
	if (sdBMP)
		{
		sdSpectrogramMode = 0;
		
		DeleteObject (sdBMP);
		sdBMP = NULL;
		}
	}

// Функция возвращает текущий фрейм спектрограммы
CD_API(HBITMAP) GetSpectrogramFrameEx ()
	{
	// Контроль
	if (!sdBMP)
		return NULL;

	// Завершено
	return sdBMP;
	}

// Функция возвращает значение амплитуды на указанном уровне
CD_API(uchar) GetCurrentPeakEx ()
	{
	// Не требует защиты
	if (cdFFTPeak == 0xFF)
		cdFFTPeak--;		// На первый раз прощаем
	else if (cdFFTPeak > 40)
		cdFFTPeak -= 40;	// Далее - затухание
	else
		cdFFTPeak = 0;		// Глушение

	return cdFFTPeak;
	}

// Функция устанавливает метрики определения пикового значения
CD_API(void) SetPeakEvaluationParametersEx (uchar LowEdge, uchar HighEdge, 
	uchar LowLevel, uchar FFTScaleMultiplier)
	{
	// Не требует защиты
	cdFFTPeakEvLowLevel = LowLevel;
	cdFFTPeakEvLowEdge = LowEdge;
	cdFFTPeakEvHighEdge = (HighEdge < LowEdge) ? LowEdge : HighEdge;

	if ((FFTScaleMultiplier >= CD_MIN_FFT_SCALE_MULT) && (FFTScaleMultiplier <= CD_MAX_FFT_SCALE_MULT))
		cdFFTScale = (float)FFTScaleMultiplier;
	else
		cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT;
	cdFFTScale *= 25.5f;
	}

// Функция формирует палитру
CD_API(void) FillPaletteEx (uchar PaletteNumber)
	{
	uint i;
	uint qSize = CD_BMPINFO_COLORS_COUNT / 4;

	switch (PaletteNumber)
		{
		// Стандартная
		default:
		case 0:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 255;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 4 * (qSize - 1 - i);

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}
			cdCurrentPalette = 0;
			break;

		// Море
		case 1:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 0;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 2 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 255;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 0;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 255;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen =
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 255;
				}
			cdCurrentPalette = PaletteNumber;
			break;

		// Огонь
		case 2:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 2 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}
			cdCurrentPalette = PaletteNumber;
			break;

		// Серая
		case 3:
			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = i;
				}
			cdCurrentPalette = PaletteNumber;
			break;

		// Рассвет
		case 4:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 0;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 3 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 2 * (qSize - i);

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 3 * qSize - i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}
			cdCurrentPalette = PaletteNumber;
			break;

		// Кислота
		case 5:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = i;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 64 + i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 128 + 2 * i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 *  i;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				}
			cdCurrentPalette = PaletteNumber;
			break;
		}
	}

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber)
	{
	return 0xFF000000 | (cdBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbRed << 16) |
		(cdBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbGreen << 8) | cdBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbBlue;
	}

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness)
	{
	uint v;

	switch (cdCurrentPalette)
		{
		default:
		case 0:
			return 0xFF000000 | ((Brightness / 2) << 16) | Brightness;

		case 1:
			return 0xFF000000 | Brightness;

		case 2:
			return 0xFF000000 | (Brightness << 16) | ((Brightness / 4) << 8);

		case 3:
			v = (9 * Brightness / 10) & 0xFF;
			return 0xFF000000 | (v << 16) | (v << 8) | v;

		case 4:
			return 0xFF000000 | (Brightness << 8) | Brightness;

		case 5:
			return 0xFF000000 | (Brightness << 8);
		}
	}

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Default (blue-magenta-yellow-white)" NAMES_DELIMITER_S \
		"Sea (blue-cyan-white)" NAMES_DELIMITER_S \
		"Fire (red-orange-yellow-white)" NAMES_DELIMITER_S \
		"Grey" NAMES_DELIMITER_S \
		"Sunshine (blue-green-orange-white)" NAMES_DELIMITER_S \
		"Acid (green-lime-white)")

	return PALETTES_NAMES;
	}

// Функция возвращает ограничивающие размеры фреймов спектрограмм
CD_API(udlong) GetSpectrogramFrameMetricsEx ()
	{
	return ((udlong)MINFRAMEWIDTH << 48) | ((udlong)MAXFRAMEWIDTH << 32) | 
		((udlong)MINFRAMEHEIGHT << 16) | (udlong)MAXFRAMEHEIGHT;
	}

// Функция возвращает стандартные метрики определения пикового значения
CD_API(ulong) GetDefaultPeakEvaluationParametersEx ()
	{
	return (CD_DEFAULT_FFT_SCALE_MULT << 24) | (PEAK_EVALUATION_LOW_EDGE << 16) | 
		(PEAK_EVALUATION_HIGH_EDGE << 8) | PEAK_EVALUATION_LOW_LEVEL;
	}

// Функция возвращает версию данной библиотеки
CD_API(schar *) GetCDLibVersionEx ()
	{
	return CD_VERSION_S;
	}
