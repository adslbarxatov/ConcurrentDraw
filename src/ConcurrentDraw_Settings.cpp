// Общий заголовок
#include "ConcurrentDrawLib.h"

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

// Функция возвращает текущий фрейм спектрограммы
CD_API(HBITMAP) GetSpectrogramFrameEx ()
	{
	// Контроль
	if (!AS->sgBMP)
		return NULL;

	// Завершено
	return AS->sgBMP;
	}

// Функция возвращает значение амплитуды на указанном уровне
CD_API(uchar) GetCurrentPeakEx ()
	{
	// Не требует защиты
	if (AS->cdFFTPeak > 0xFD)
		AS->cdFFTPeak--;		// "Эхо"
	else if (AS->cdFFTPeak > 40)
		AS->cdFFTPeak -= 40;	// Далее - затухание
	else
		AS->cdFFTPeak = 0;		// Глушение

	return AS->cdFFTPeak;
	}

// Функция устанавливает метрики определения пикового значения
CD_API(void) SetPeakEvaluationParametersEx (uint LowEdge, uint HighEdge, 
	uchar LowLevel, uchar FFTScaleMultiplier)
	{
	// Установка начального состояния программы (функция срабатывает только один раз
	// за время жизни программы)
	InitAppState ();

	// Не требует защиты
	AS->cdFFTPeakEvLowLevel = LowLevel;
	AS->cdFFTPeakEvLowEdge = LowEdge;
	AS->cdFFTPeakEvHighEdge = (HighEdge < LowEdge) ? LowEdge : HighEdge;

	if ((FFTScaleMultiplier >= CD_MIN_FFT_SCALE_MULT) && (FFTScaleMultiplier <= CD_MAX_FFT_SCALE_MULT))
		AS->cdFFTScale = (float)FFTScaleMultiplier;
	else
		AS->cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT;
	AS->cdFFTScale *= CD_SECOND_FFT_SCALE_MULT;
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
CD_API(void) SetHistogramFFTValuesCountEx (uint Count, uchar Reversed)
	{
	// Установка начального состояния программы (функция срабатывает только один раз
	// за время жизни программы)
	InitAppState ();

	// Установка значений
	AS->cdHistogramFFTValuesCount = Count;

	if ((Count < 16) || (Count > FFT_VALUES_COUNT))
		AS->cdHistogramFFTValuesCount = DEFAULT_FFT_VALUES_COUNT;

	AS->cdReverseFreqOrder = ((Reversed != 0) ? 1 : 0);
	}

// Функция возвращает длину текущего файлового потока в миллисекундах (для аудиовыхода всегда 0)
CD_API(uint) GetChannelLengthEx ()
	{
	if (!AS->cdChannel)
		return 0;

	return AS->cdChannelLength;
	}
