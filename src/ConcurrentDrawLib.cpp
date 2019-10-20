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
uchar sdSpectrogramMode = 0,		// Режим спектрограммы (0 - выключена, 1 - с курсором, 2 - движущаяся)
	cdFFTPeak = 0,					// Текущее пиковое значение
	cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE,		// Нижняя граница диапазона определения пика
	cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE,	// Верхняя граница диапазона определения пика
	cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;	// Наименьшая амплитуда, на которой определяется пик
uchar currentPalette = 0;			// Номер текущей палитры

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
		*(*Devices + pos + lastLength) = NAMES_DELIMITER;
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
	uint v;
	if (!cdChannel)
		return NULL;

	// Получение
	if ((v = BASS_ChannelGetData (cdChannel, &cdFFT, BASS_DATA_AVAILABLE)) < FFT_VALUES_COUNT)
		return NULL;

	if (BASS_ChannelGetData (cdChannel, &cdFFT, FFT_MODE ) < 0)
		return NULL;

	return cdFFT;
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
				// Масштабирование (квадратный корень позволяет лучше видеть нижние значения)
				v = (uint)(sqrt (cdFFT[y + 1]) * SD_FFT_SCALE);

				// Вписывание в диапазон и пересчёт пика
				INBOUND_FFT_VALUE (v)
				UPDATE_PEAK (y, v)

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

				// Масштаб
				v = (int)(sqrt (cdFFT[y + 1]) * SD_FFT_SCALE);

				// Вписывание в диапазон и пересчёт пика
				INBOUND_FFT_VALUE (v)
				UPDATE_PEAK (y, v)

				// Отрисовка
#ifdef SD_DOUBLE_WIDTH
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 2] = 
#endif
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 1] = v;
				}
			break;
		}
	}

// Функция инициализирует спектрограмму
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, uchar PaletteNumber, uchar MoveThrough)
	{
	// Переменные
	union CD_BITMAPINFO info;

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

	sdSpectrogramMode = (MoveThrough ? 2 : 1);

	// Инициализация описателя
	memset (info.cd_bmpinfo_ptr, 0x00, sizeof (union CD_BITMAPINFO));	// Сброс на нули всех значений

	info.cd_bmpinfo.header.biSize = sizeof (BITMAPINFOHEADER);
	info.cd_bmpinfo.header.biWidth = sdFrameWidth;
	info.cd_bmpinfo.header.biHeight = sdFrameHeight;
	info.cd_bmpinfo.header.biPlanes = 1;
	info.cd_bmpinfo.header.biBitCount = 8;
	info.cd_bmpinfo.header.biClrUsed = info.cd_bmpinfo.header.biClrImportant = CD_BMPINFO_COLORS_COUNT;

	FillPalette (info.cd_bmpinfo.colors, PaletteNumber);

	// Создание BITMAP
	if ((sdBMP = CreateDIBSection (NULL, (BITMAPINFO *)&info, DIB_RGB_COLORS, (void **)&sdBuffer, NULL, 0)) == NULL)
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
CD_API(void) SetPeakEvaluationParametersEx (uchar LowEdge, uchar HighEdge, uchar LowLevel)
	{
	// Не требует защиты
	cdFFTPeakEvLowLevel = LowLevel;
	cdFFTPeakEvLowEdge = LowEdge;
	cdFFTPeakEvHighEdge = (HighEdge < LowEdge) ? LowEdge : HighEdge;
	}

// Функция формирует палитру
void FillPalette (RGBQUAD *Palette, uchar PaletteNumber)
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
				Palette[i].rgbBlue = 4 * i;

				Palette[qSize + i].rgbBlue = 255;
				Palette[qSize + i].rgbRed = 4 * i;

				Palette[2 * qSize + i].rgbRed = 255;
				Palette[2 * qSize + i].rgbBlue = 4 * (qSize - 1 - i);
				Palette[2 * qSize + i].rgbGreen = 4 * i;

				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = 0;
			break;

		// Море
		case 1:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbBlue = 4 * i;

				Palette[qSize + i].rgbBlue = 255;
				Palette[qSize + i].rgbGreen = 2 * i;

				Palette[2 * qSize + i].rgbBlue = 255;
				Palette[2 * qSize + i].rgbGreen = 2 * (qSize + i);

				Palette[3 * qSize + i].rgbBlue = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbRed = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;

		// Огонь
		case 2:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbRed = 4 * i;

				Palette[qSize + i].rgbRed = 255;
				Palette[qSize + i].rgbGreen = 2 * i;

				Palette[2 * qSize + i].rgbRed = 255;
				Palette[2 * qSize + i].rgbGreen = 2 * (qSize + i);

				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;

		// Серая
		case 3:
			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++) 
				{
				Palette[i].rgbRed = Palette[i].rgbGreen = Palette[i].rgbBlue = i;
				}
			currentPalette = PaletteNumber;
			break;

		// Рассвет
		case 4:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbBlue = 2 * i;
				Palette[qSize + i].rgbBlue = 128 - 2 * i;
				Palette[qSize + i].rgbGreen = 3 * i;
				Palette[2 * qSize + i].rgbRed = 4 * i;
				Palette[2 * qSize + i].rgbGreen = 192 - i;
				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 128 + 2 * i;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;
		}

	//ACT_SavePaletteEx ("test.act", (union RGBA_Color *)Palette, 256);
	}

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness)
	{
	uint v;

	switch (currentPalette)
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
			return 0xFF000000 | ((3 * Brightness / 4) << 8);
		}
	}

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Default (blue-magenta-yellow-white)" NAMES_DELIMITER2 \
		"Sea (blue-cyan-white)" NAMES_DELIMITER2 \
		"Fire (red-orange-yellow-white)" NAMES_DELIMITER2 \
		"Grey" NAMES_DELIMITER2 \
		"Sunshine (blue-green-orange-white)")

	return PALETTES_NAMES;
	}

#ifdef EXETEST

sint main (void)
	{
	schar *devs;
	float *fftr;
	uchar count = GetDevicesEx (&devs);
	sint y;

	if (count < 1)
		return - 1;

	if (InitializeSoundStream (0) != 0)
		return -2;

	InitializeSpectrogram (200, 0, 0);

	while (1)
		{
		fftr = GetDataFromStream ();
		for (int i = 0; i < 40; i++)
			{
			y = (sint)(sqrt (fftr[i * 3 + 1]) * 3 * 9);
			if (y > 9)
				y = 9;

			printf ("%c", 0x30 + y);
			}
		printf ("\n");
		Sleep (40);
		}

	DestroySoundStream ();
	}

#endif
