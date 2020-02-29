// Общий заголовок
#include "ConcurrentDrawLib.h"

// Функция проверяет версию библиотеки BASS
ulong BASSVersionIsCorrect ()
	{
	ulong v = BASS_GetVersion ();

	// Перебор допустимых версий
	if ((v == 0x02040E00) ||
		(v == 0x02040F00))
		return v;

	return 0;
	}

// Функция запускает процесс считывания данных со звукового вывода
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber)
	{
	// Контроль
	if (AS->cdChannel)
		return -11;

	// Получение доступа к библиотеке
	if (!BASSVersionIsCorrect ())
		return -10;

	// Установка начального состояния программы (если не выполнено ранее функциями настройки)
	InitAppState ();

	// Инициализация
	if (!BASS_RecordInit (DeviceNumber))
		return BASS_ErrorGetCode ();

	if (!(AS->cdChannel = BASS_RecordStart (CD_RECORD_FREQ, 2, 0x00, NULL, NULL)))
		return BASS_ErrorGetCode ();

	// Запуск таймера запроса данных
	AS->cdFFTTimer = timeSetEvent (CD_TIMER_TPS, CD_TIMER_TPS, (LPTIMECALLBACK)&UpdateFFT, 0, TIME_PERIODIC);

	// Успешно
	AS->cdChannelLength = 0;
	return 0;
	}

// Функция запускает процесс считывания данных из звукового файла
CD_API(sint) InitializeFileStreamEx (schar *FileName)
	{
	BASS_CHANNELINFO info;
	ulong streamLength = 0;

	// Контроль
	if (AS->cdChannel)
		return -11;

	// Получение доступа к библиотеке
	if (!BASSVersionIsCorrect ())
		return -10;

	// Установка начального состояния программы (если не выполнено ранее функциями настройки)
	InitAppState ();

	// Инициализация
	if (!BASS_Init (-1, CD_RECORD_FREQ, BASS_DEVICE_STEREO, NULL, NULL))
		return BASS_ErrorGetCode ();

	if (!(AS->cdChannel = BASS_StreamCreateFile (FALSE, FileName, 0, 0, BASS_STREAM_DECODE)))
		return BASS_ErrorGetCode ();

	// Получение длины потока (в миллисекундах)
	BASS_ChannelGetInfo (AS->cdChannel, &info);
	streamLength = BASS_ChannelGetLength (AS->cdChannel, BASS_POS_BYTE);
	AS->cdChannelLength = 8 * streamLength / (info.chans * info.freq * info.origres);

	// Успешно
	return 0;
	}

// Функция завершает процесс считывания
CD_API(void) DestroySoundStreamEx ()
	{
	// Контроль
	if (!AS->cdChannel)
		return;

	// Закрытие таймера
	if (AS->cdFFTTimer)
		{
		timeKillEvent (AS->cdFFTTimer);
		AS->cdFFTTimer = NULL;
		}

	// Закрытие спектрограммы, если есть
	DestroySpectrogramEx ();

	// Остановка
	if (AS->cdChannelLength)
		BASS_Free ();
	else
		BASS_RecordFree ();
	AS->cdChannel = NULL;
	}

// Функция инициализирует спектрограмму
// Flags: b0 = double width
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, uchar PaletteNumber, 
	uchar SpectrogramMode, uchar Flags)
	{
	// Контроль параметров
	if ((FrameWidth < MINFRAMEWIDTH) || (FrameWidth > MAXFRAMEWIDTH) ||
		(FrameHeight < MINFRAMEHEIGHT) || (FrameHeight > MAXFRAMEHEIGHT))
		return -3;

	if (!AS->cdChannel)		// Канал должен быть инициализирован
		return -1;

	if (AS->sgBMP)			// Спектрограмма не должна быть занята
		return -2;

	AS->sgFrameWidth = FrameWidth & 0xFFFC;		// Хрен знает, почему, но CreateDIBSection не понимает размеры,
	if (AS->sgFrameWidth != FrameWidth)
		AS->sgFrameWidth += 4;

	AS->sgFrameHeight = FrameHeight & 0xFFFC;	// которые не делятся на 4
	if (AS->sgFrameHeight != FrameHeight)
		AS->sgFrameHeight += 4;

	AS->sgSpectrogramMode = SpectrogramMode;
	AS->sgSpectrogramStep = 1 + (Flags & 0x1);

	// Инициализация описателя
	AS->sgBMPInfo.cd_bmpinfo.header.biSize = sizeof (BITMAPINFOHEADER);
	AS->sgBMPInfo.cd_bmpinfo.header.biWidth = AS->sgFrameWidth;
	AS->sgBMPInfo.cd_bmpinfo.header.biHeight = AS->sgFrameHeight;
	AS->sgBMPInfo.cd_bmpinfo.header.biPlanes = 1;
	AS->sgBMPInfo.cd_bmpinfo.header.biBitCount = 8;
	AS->sgBMPInfo.cd_bmpinfo.header.biClrUsed = AS->sgBMPInfo.cd_bmpinfo.header.biClrImportant = CD_BMPINFO_COLORS_COUNT;

	FillPaletteEx (PaletteNumber);

	// Создание BITMAP
	if ((AS->sgBMP = CreateDIBSection (NULL, (BITMAPINFO *)&AS->sgBMPInfo, DIB_RGB_COLORS, (void **)&AS->sgBuffer, NULL, 0)) == NULL)
		return -4;

	// Завершено
	return 0;
	}

// Функция удаляет активную спектрограмму
CD_API(void) DestroySpectrogramEx ()
	{
	// Контроль
	if (!AS->cdChannel)
		return;

	// Запрет на инвалидацию спектрограммы при активном процессе обновления
	while (AS->updating);

	// Сброс
	if (AS->sgBMP)
		{
		AS->sgSpectrogramMode = 0;
		
		DeleteObject (AS->sgBMP);
		AS->sgBMP = NULL;
		}
	}
