// Общий заголовок
#include "ConcurrentDrawLib.h"

// Список поддерживаемых версий BASS
ulong SupportedBASSVersions[] = {
	0x02040E00,
	0x02040F00,
	0x02041007,
	0x02041100,
	//0x02041200,	// Не работает
	};

float FFT[FFT_CLEAN_VALUES_COUNT];
float fftSumma[FFT_CLEAN_VALUES_COUNT];

// Функция проверяет версию библиотеки BASS
ulong BASSVersionIsCorrect ()
	{
	ulong v = BASS_GetVersion ();
	uint i;

	// Перебор допустимых версий
	for (i = 0; i < sizeof (SupportedBASSVersions) / sizeof (ulong); i++)
		if (v == SupportedBASSVersions[i])
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

	// Получение длины потока (в секундах)
	BASS_ChannelGetInfo (AS->cdChannel, &info);
	streamLength = (ulong)BASS_ChannelGetLength (AS->cdChannel, BASS_POS_BYTE);
	AS->cdChannelLength = (uint)((info.origres & BASS_ORIGRES_FLOAT ? 2 : 1) * 8 * streamLength / 
		(info.chans * info.freq * (info.origres & 0xFFFF)));

	AS->cdChannelPosition = 0;
	AS->cdChannelBPF = info.chans * info.freq * (info.origres & 0xFFFF) / (8 * CD_VIDEO_FPS);

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

	// Канал должен быть инициализирован
	if (!AS->cdChannel)
		return -1;

	// Спектрограмма не должна быть занята
	if (AS->sgBufferDraw)
		return -2;

	AS->sgFrameWidth = FrameWidth & 0xFFFC;		// Хрен знает, почему, но CreateDIBSection не понимает размеры,
	AS->sgFrameHeight = FrameHeight & 0xFFFC;	// которые не делятся на 4
	AS->sgSpectrogramMode = SpectrogramMode;
	AS->sgSpectrogramStep = 1 + (Flags & 0x1);

	// Инициализация буфера изображения
	FillPaletteEx (PaletteNumber);

	AS->sgBufferDraw = (void *)malloc (AS->sgFrameWidth * AS->sgFrameHeight);
	if (!AS->sgBufferDraw)
		return -4;

	memset (AS->sgBufferDraw, 0x00, AS->sgFrameWidth * AS->sgFrameHeight);

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
	if (AS->sgBufferDraw)
		{
		AS->sgSpectrogramMode = 0;

		free (AS->sgBufferDraw);
		AS->sgBufferDraw = NULL;
		}
	}

// Функция выгружает полные данные БПФ в виде сумм амплитуд по частотам в табличный файл
CD_API(sint) DumpSpectrogramFromFileEx (schar *SoundFileName)
	{
	// Переменные
	float max = 0.0f;
	sint i;
	FILE *FO;
	schar ColumnName[MAX_DEVICE_NAME_LENGTH];
	schar TableFileName[256];

	// Пробуем открыть файлы
	if (i = InitializeFileStreamEx (SoundFileName))
		return i;

	sprintf (ColumnName, "%s", strrchr (SoundFileName, '\\') + 1);
	ColumnName[strlen (ColumnName) - 4] = '\0';

	sprintf (TableFileName, "%s", SoundFileName);
	i = strlen (TableFileName);
	TableFileName[i - 3] = 'c';
	TableFileName[i - 2] = 's';
	TableFileName[i - 1] = 'v';

	if ((FO = fopen (TableFileName, "wb")) == NULL)
		return -1;

	// Инициализируем массивы
	for (i = 0; i < FFT_CLEAN_VALUES_COUNT; i++)
		FFT[i] = fftSumma[i] = 0.0f;

	// Выполняем считывание
	while (GetDataFromStream (&FFT))
		{
		for (i = 0; i < FFT_CLEAN_VALUES_COUNT; i++)
			fftSumma[i] += FFT[i];
		}

	// Нормализуем значения на одну минуту трека
	for (i = 0; i < FFT_CLEAN_VALUES_COUNT; i++)
		{
		fftSumma[i] = 100.0f * 60.0f * fftSumma[i] / (float)AS->cdChannelLength;
		if (max < fftSumma[i])
			max = fftSumma[i];
		}

	// Нормализуем значения на громкость 1000 у. е.
	for (i = 0; i < FFT_CLEAN_VALUES_COUNT; i++)
		fftSumma[i] = 1000.0f * fftSumma[i] / max;

	// Записываем в файл, указывая соответствующие частоты вместо отсчётов
	fprintf (FO, "Fq;%s\r\n", ColumnName);
	for (i = 0; i < FFT_CLEAN_VALUES_COUNT; i++)
		fprintf (FO, "%u;%f\r\n", 22050 * (ulong)i / FFT_CLEAN_VALUES_COUNT, fftSumma[i]);

	// Завершено
	DestroySoundStreamEx ();
	fclose (FO);
	}
