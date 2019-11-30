// Общий заголовок
#include "ConcurrentDrawLib.h"

// Общие переменные
HRECORD cdChannel = NULL;			// Дескриптор чтения
uint channelLength = 0;				// Длина потока (при инициализации из файла будет ненулевой)
float cdFFT[FFT_VALUES_COUNT];		// Массив значений, получаемый из канала
MMRESULT cdFFTTimer = NULL;			// Дескриптор таймера запроса данных из буфера
uchar updating = 0;					// Флаг, указывающий на незавершённость последнего процесса обновления FFT

HBITMAP sgBMP = NULL;				// Дескриптор BITMAP спектральной диаграммы
uchar *sgBuffer;					// Буфер спектральной диаграммы
uint sgFrameWidth, sgFrameHeight,	// Размеры изображения спектрограммы
	sgCurrentPosition = 0;			// Текущая позиция на спектрограмме
uchar sgSpectrogramMode = 0;		// Режим спектрограммы (0 - выключена, 1 - с курсором, 
									// 2 - движущаяся, 3 - гистограмма, 4 - симметричная гистограмма)

float cdFFTScale =
	(float)CD_DEFAULT_FFT_SCALE_MULT * 25.5f;			// Масштаб значений FFT
uint cdHistogramFFTValuesCount = 
	DEFAULT_FFT_VALUES_COUNT;							// Количество значений FFT, используемых для гистограмм
uchar cdFFTPeak = 0,									// Текущее пиковое значение
	cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE,		// Нижняя граница диапазона определения пика
	cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE,	// Верхняя граница диапазона определения пика
	cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;	// Наименьшая амплитуда, на которой определяется пик

union CD_BITMAPINFO cdBMPInfo,		// Данные для инициализации спектрограммы
	cdDummyInfo;					// Вспомогательная палитра для бит-детектора

RGBQUAD polymorphColors[5];			// Опорные цвета полиморфной палитры
uint polymorphUpdateCounter = 0;	// Счётчик обновления полиморфной палитры

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
	// Контроль
	if (cdChannel)
		return -11;

	// Получение доступа к библиотеке
	if (BASS_GetVersion () != BASS_VERSION)
		return -10;

	// Инициализация
	if (!BASS_RecordInit (DeviceNumber))
		return BASS_ErrorGetCode ();

	if (!(cdChannel = BASS_RecordStart (44100, 2, 0x00, NULL, NULL)))
		return BASS_ErrorGetCode ();

	// Запуск таймера запроса данных
	cdFFTTimer = timeSetEvent (25, 25, (LPTIMECALLBACK)&UpdateFFT, 0, TIME_PERIODIC);

	// Успешно
	channelLength = 0;
	return 0;
	}

// Функция запускает процесс считывания данных из звукового файла
CD_API(sint) InitializeFileStreamEx (schar *FileName)
	{
	BASS_CHANNELINFO info;
	ulong streamLength = 0;

	// Контроль
	if (cdChannel)
		return -11;

	// Получение доступа к библиотеке
	if (BASS_GetVersion () != BASS_VERSION)
		return -10;

	// Инициализация
	if (!BASS_Init (-1, 44100, BASS_DEVICE_STEREO, NULL, NULL))
		return BASS_ErrorGetCode ();

	if (!(cdChannel = BASS_StreamCreateFile (FALSE, FileName, 0, 0, BASS_STREAM_DECODE)))
		return BASS_ErrorGetCode ();

	// Получение длины потока (в миллисекундах)
	BASS_ChannelGetInfo (cdChannel, &info);
	streamLength = BASS_ChannelGetLength (cdChannel, BASS_POS_BYTE);
	channelLength = 8 * streamLength / (info.chans * info.freq * info.origres);

	// Успешно
	return 0;
	}

// Функция завершает процесс считывания
CD_API(void) DestroySoundStreamEx ()
	{
	// Контроль
	if (!cdChannel)
		return;

	// Закрытие таймера
	if (cdFFTTimer)
		{
		timeKillEvent (cdFFTTimer);
		cdFFTTimer = NULL;
		}

	// Закрытие спектрограммы, если есть
	DestroySpectrogramEx ();

	// Остановка
	if (channelLength)
		BASS_Free ();
	else
		BASS_RecordFree ();
	cdChannel = NULL;
	}

// Функция запрашивает данные из канала считывания
float *GetDataFromStreamEx ()
	{
	// Контроль
	if (!cdChannel)
		return NULL;

	// Получение
	if (BASS_ChannelGetData (cdChannel, &cdFFT, BASS_DATA_AVAILABLE) < FFT_VALUES_COUNT)
		return NULL;

	if (BASS_ChannelGetData (cdChannel, &cdFFT, FFT_MODE) < 0)
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

	// Переход в режим обновления
	updating = 1;

	// Обновление полиморфной палитры
	if (polymorphUpdateCounter != 0)	// Нулевое значение используется как блокировка
		{
		if (polymorphUpdateCounter++ >= POLYMORPH_UPDATE_PAUSE)
			{
			FillPaletteEx (10);
			}
		}

	// Обновление спектрограммы, если требуется
	switch (sgSpectrogramMode)
		{
		// Без спектрограммы
		default:
		case 0:
			break;

		// С курсором
		case 1:
			for (y = 0; y < sgFrameHeight; y++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (y + 1);

				// Отрисовка
				sgBuffer[y * sgFrameWidth + sgCurrentPosition] =
#ifdef SG_DOUBLE_WIDTH
				sgBuffer[y * sgFrameWidth + (sgCurrentPosition + 1) % sgFrameWidth] = 
#endif
				v;

				// Маркер
				sgBuffer[y * sgFrameWidth + (sgCurrentPosition + SG_STEP) % sgFrameWidth] = 255;
				}

			// Движение маркера
			sgCurrentPosition = (sgCurrentPosition + SG_STEP) % sgFrameWidth;
			break;

		// Движущаяся
		case 2:
			for (y = 0; y < sgFrameHeight; y++)
				{
				// Сдвиг изображения
				for (x = 0; x < sgFrameWidth - SG_STEP; x += SG_STEP)
					{
					sgBuffer[y * sgFrameWidth + x] = sgBuffer[y * sgFrameWidth + x + 1]
#ifdef SG_DOUBLE_WIDTH
					= sgBuffer[y * sgFrameWidth + x + 2]
#endif
					;
					}

				// Получение значения
				v = GetScaledAmplitudeEx (y + 1);

				// Отрисовка
#ifdef SG_DOUBLE_WIDTH
				sgBuffer[y * sgFrameWidth + sgFrameWidth - 2] = 
#endif
				sgBuffer[y * sgFrameWidth + sgFrameWidth - 1] = v;
				}
			break;

		// Гистограмма и симметричная гистограмма
		case 3:
		case 4:
			for (x = 0; x < sgFrameWidth; x++)
				{
				// Получение значения
				v = GetScaledAmplitudeEx (cdHistogramFFTValuesCount * (ulong)x / sgFrameWidth);

				// Перемасштабирование
				v = sgFrameHeight * (ulong)v / CD_BMPINFO_COLORS_COUNT;	

				if (sgSpectrogramMode == 3)
					{
					for (y = 0; y < v; y++)
						sgBuffer[y * sgFrameWidth + x] = CD_HISTO_BAR;	// Обрезаем края палитр
					for (y = v; y < sgFrameHeight; y++)
						sgBuffer[y * sgFrameWidth + x] = CD_HISTO_SPACE;
					}
				else
					{
					for (y = 0; y < v; y++)
						sgBuffer[y * sgFrameWidth + (sgFrameWidth + x) / 2] =
						sgBuffer[y * sgFrameWidth + (sgFrameWidth - x) / 2] = CD_HISTO_BAR;	// Обрезаем края палитр

					for (y = v; y < sgFrameHeight; y++)
						sgBuffer[y * sgFrameWidth + (sgFrameWidth + x) / 2] =
						sgBuffer[y * sgFrameWidth + (sgFrameWidth - x) / 2] = CD_HISTO_SPACE;
					}
				}
			break;
		}

	// Обновление завершено
	updating = 0;
	}

// Функция выполняет ручное обновление данных FFT вместо встроенного таймера
CD_API(void) UpdateFFTDataEx ()
	{
	UpdateFFT (0, 0, 0, 0, 0);
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
	if (sgBMP)			// Спектрограмма не должна быть занята
		return -2;

	sgFrameWidth = FrameWidth & 0xFFFC;		// Хрен знает, почему, но CreateDIBSection не понимает размеры,
	if (sgFrameWidth != FrameWidth)
		sgFrameWidth += 4;

	sgFrameHeight = FrameHeight & 0xFFFC;	// которые не делятся на 4
	if (sgFrameHeight != FrameHeight)
		sgFrameHeight += 4;

	sgSpectrogramMode = SpectrogramMode;

	// Инициализация описателя
	memset (cdBMPInfo.cd_bmpinfo_ptr, 0x00, sizeof (union CD_BITMAPINFO));	// Сброс на нули всех значений

	cdBMPInfo.cd_bmpinfo.header.biSize = sizeof (BITMAPINFOHEADER);
	cdBMPInfo.cd_bmpinfo.header.biWidth = sgFrameWidth;
	cdBMPInfo.cd_bmpinfo.header.biHeight = sgFrameHeight;
	cdBMPInfo.cd_bmpinfo.header.biPlanes = 1;
	cdBMPInfo.cd_bmpinfo.header.biBitCount = 8;
	cdBMPInfo.cd_bmpinfo.header.biClrUsed = cdBMPInfo.cd_bmpinfo.header.biClrImportant = CD_BMPINFO_COLORS_COUNT;

	FillPaletteEx (PaletteNumber);

	// Создание BITMAP
	if ((sgBMP = CreateDIBSection (NULL, (BITMAPINFO *)&cdBMPInfo, DIB_RGB_COLORS, (void **)&sgBuffer, NULL, 0)) == NULL)
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

	// Запрет на инвалидацию спектрограммы при активном процессе обновления
	while (updating);

	// Сброс
	if (sgBMP)
		{
		sgSpectrogramMode = 0;
		
		DeleteObject (sgBMP);
		sgBMP = NULL;
		}
	}

// Функция возвращает текущий фрейм спектрограммы
CD_API(HBITMAP) GetSpectrogramFrameEx ()
	{
	// Контроль
	if (!sgBMP)
		return NULL;

	// Завершено
	return sgBMP;
	}

// Функция возвращает значение амплитуды на указанном уровне
CD_API(uchar) GetCurrentPeakEx ()
	{
	// Не требует защиты
	if (cdFFTPeak > 0xFD)
		cdFFTPeak--;		// "Эхо"
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
	uint i, j;
	uint qSize = CD_BMPINFO_COLORS_COUNT / 4;
	polymorphUpdateCounter = 0;

	switch (PaletteNumber)
		{
		// Стандартная
		default:
		case 0:
			// Основная палитра
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 2 * (qSize + i);

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 4 * i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 4 * (qSize - 1 - i);

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}

			// Палитра бит-детектора
			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = i / 2; 
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = i;
				}
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

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = i;
				}
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

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = i; 
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = i / 4;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
				}
			break;

		// Серая
		case 3:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = i / 2;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = (qSize + i) / 2;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = qSize + i;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 2 * (qSize + i);
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed =  
					cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i / 5;
				}
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

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = 3 * i / 4;
				}
			break;

		// Кислота
		case 5:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = i;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = qSize + i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 *  i;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = i;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
				}
			break;

		// 7 пропущенных
		case 6:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 3 * i;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 3 * (qSize - i);
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 3 * i / 2;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 2 * qSize + i;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 2 * i;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = (3 * qSize + 5 * i) / 2;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 3 * qSize - i;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 2 * (qSize + i);
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = 3 * i / 4;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = i / 2;
				}
			break;

		// Парус
		case 7:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 2 * i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 255;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 0;
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 4 * (qSize - 1 - i);

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = i;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
				}
			break;

		// Зеркало
		case 8:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = i;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen =
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = qSize + i;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed =
					cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 2 * (qSize - i);

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen = 255;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 * i;
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 3 * i / 4;
				}
			break;

		// Кровь
		case 9:
			for (i = 0; i < qSize; i++) 
				{
				cdBMPInfo.cd_bmpinfo.colors[i].rgbRed = i;
				cdBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
					cdBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbRed = qSize + i;
				cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbGreen =
					cdBMPInfo.cd_bmpinfo.colors[qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbRed = 2 * (qSize + i);
				cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbGreen =
					cdBMPInfo.cd_bmpinfo.colors[2 * qSize + i].rgbBlue = 0;

				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbGreen =
					cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbBlue = 4 *  i;
				cdBMPInfo.cd_bmpinfo.colors[3 * qSize + i].rgbRed = 255;
				}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = i;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen =
					cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
				}
			break;

		// Полиморфная и случайная
		case 10:
		case 11:
			// Обновление цветов
			if ((polymorphColors[4].rgbRed != 255) || (PaletteNumber == 11))
				{
				srand((uint)time (NULL));
				//polymorphColors[0].rgbRed = polymorphColors[0].rgbGreen = polymorphColors[0].rgbBlue = 0;
				polymorphColors[4].rgbRed = polymorphColors[4].rgbGreen = polymorphColors[4].rgbBlue = 255;

				for (i = 1; i < 4; i++)
					{
					polymorphColors[i].rgbRed = GetRandomValue (64, 256);
					polymorphColors[i].rgbGreen = GetRandomValue (64, 256);
					polymorphColors[i].rgbBlue = GetRandomValue (64, 256);
					}
				}

			for (i = 1; i < 4; i++)
				{
				polymorphColors[i].rgbRed += GetRandomValue (-10, 10);
				if (polymorphColors[i].rgbRed < 10) polymorphColors[i].rgbRed = 255;
				if (polymorphColors[i].rgbRed < 64) polymorphColors[i].rgbRed = 64;

				polymorphColors[i].rgbGreen += GetRandomValue (-10, 10);
				if (polymorphColors[i].rgbGreen < 10) polymorphColors[i].rgbGreen = 255;
				if (polymorphColors[i].rgbGreen < 64) polymorphColors[i].rgbGreen = 64;

				polymorphColors[i].rgbBlue += GetRandomValue (-10, 10);
				if (polymorphColors[i].rgbBlue < 10) polymorphColors[i].rgbBlue = 255;
				if (polymorphColors[i].rgbBlue < 64) polymorphColors[i].rgbBlue = 64;
				}

			// Заполнение
			for (i = 0; i < qSize; i++)
				for (j = 0; j < 4; j++)
					{
					cdBMPInfo.cd_bmpinfo.colors[j * qSize + i].rgbRed = ((qSize - i) * polymorphColors[j].rgbRed +
						i * polymorphColors[j + 1].rgbRed) / qSize;
					cdBMPInfo.cd_bmpinfo.colors[j * qSize + i].rgbGreen = ((qSize - i) * polymorphColors[j].rgbGreen +
						i * polymorphColors[j + 1].rgbGreen) / qSize;
					cdBMPInfo.cd_bmpinfo.colors[j * qSize + i].rgbBlue = ((qSize - i) * polymorphColors[j].rgbBlue +
						i * polymorphColors[j + 1].rgbBlue) / qSize;
					}

			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
				{
				cdDummyInfo.cd_bmpinfo.colors[i].rgbRed = i * polymorphColors[2].rgbRed / CD_BMPINFO_COLORS_COUNT;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbGreen = i * polymorphColors[2].rgbGreen / CD_BMPINFO_COLORS_COUNT;
				cdDummyInfo.cd_bmpinfo.colors[i].rgbBlue = i * polymorphColors[2].rgbBlue / CD_BMPINFO_COLORS_COUNT;
				}

			if (PaletteNumber == 10)
				polymorphUpdateCounter = 1;
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
	return 0xFF000000 | (cdDummyInfo.cd_bmpinfo.colors[Brightness].rgbRed << 16) |
		(cdDummyInfo.cd_bmpinfo.colors[Brightness].rgbGreen << 8) | 
		cdDummyInfo.cd_bmpinfo.colors[Brightness].rgbBlue;
	}

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Default (Adobe Audition)" NAMES_DELIMITER_S \
		"Sea" NAMES_DELIMITER_S \
		"Fire" NAMES_DELIMITER_S \
		"Grey" NAMES_DELIMITER_S \
		"Sunrise" NAMES_DELIMITER_S \
		"Acid" NAMES_DELIMITER_S \
		"7 missed calls" NAMES_DELIMITER_S \
		"Sail on the sea" NAMES_DELIMITER_S \
		"Mirror" NAMES_DELIMITER_S \
		"Blood" NAMES_DELIMITER_S \
		"Polymorph" NAMES_DELIMITER_S \
		"Random")

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

// Функция устанавливает количество значений FFT, которое будет использоваться в гистограммах
CD_API(void) SetHistogramFFTValuesCountEx (uint Count)
	{
	cdHistogramFFTValuesCount = Count;

	if ((Count < 64) || (Count > FFT_VALUES_COUNT))
		cdHistogramFFTValuesCount = DEFAULT_FFT_VALUES_COUNT;
	}

// Функция возвращает длину текущего файлового потока в миллисекундах (для аудиовыхода всегда 0)
CD_API(uint) GetChannelLengthEx ()
	{
	if (!cdChannel)
		return 0;

	return channelLength;
	}

// Функция возвращает ПСЗ из диапазона [Min; Max)
sint GetRandomValue (sint Min, sint Max)
	{
	return (double)rand () / (RAND_MAX + 1.0) * (Max - Min) + Min;
	}
