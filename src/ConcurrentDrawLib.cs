using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RD_AAOW
	{
	/// <summary>
	/// Возможные ошибки инициализации звукового потока
	/// </summary>
	public enum SoundStreamInitializationErrors
		{
		// Общие ошибки приложения

		/// <summary>
		/// Неподдерживаемая версия библиотеки BASS
		/// </summary>
		BASS_InvalidDLLVersion = -10,

		/// <summary>
		/// Дескриптор записи уже занят другой функцией
		/// </summary>
		BASS_RecordAlreadyRunning = -11,

		/// <summary>
		/// Инициализация выполнена успешно
		/// </summary>
		BASS_OK = 0,

		// Ошибки функции BASS_RecordInit

		/// <summary>
		/// A sufficient version of DirectX (or ALSA) is not installed
		/// </summary>
		BASS_ERROR_DX = 39,

		/// <summary>
		/// Device is invalid
		/// </summary>
		BASS_ERROR_DEVICE = 23,

		/// <summary>
		/// The device has already been initialized
		/// </summary>
		BASS_ERROR_ALREADY = 14,

		/// <summary>
		/// There is no available device driver
		/// </summary>
		BASS_ERROR_DRIVER = 3,

		// Ошибки функции BASS_RecordStart

		/// <summary>
		/// BASS_RecordInit has not been successfully called
		/// </summary>
		BASS_ERROR_INIT = 8,

		/// <summary>
		/// The device is busy. An existing recording may need to be stopped before starting another one
		/// </summary>
		BASS_ERROR_BUSY = 46,

		/// <summary>
		/// The recording device is not available. 
		/// Another application may already be recording with it, or it could be a half-duplex device 
		/// that is currently being used for playback
		/// </summary>
		BASS_ERROR_NOTAVAIL = 37,

		/// <summary>
		/// The requested format is not supported. 
		/// If using the BASS_SAMPLE_FLOAT flag, it could be that floating-point recording is not supported
		/// </summary>
		BASS_ERROR_FORMAT = 6,

		/// <summary>
		/// There is insufficient memory
		/// </summary>
		BASS_ERROR_MEM = 1,

		/// <summary>
		/// Some other mystery problem of BASS
		/// </summary>
		BASS_ERROR_UNKNOWN = -1,

		/// <summary>
		/// Could not initialize 3D support
		/// </summary>
		BASS_ERROR_NO3D = 21,

		/// <summary>
		/// Incorrect function call.
		/// The length must be specified when streaming from memory
		/// </summary>
		BASS_ERROR_ILLPARAM = 20,

		/// <summary>
		/// The file could not be opened
		/// </summary>
		BASS_ERROR_FILEOPEN = 2,

		/// <summary>
		/// The file's format is not recognised/supported
		/// </summary>
		BASS_ERROR_FILEFORM = 41,

		/// <summary>
		/// The file uses a codec that is not available/supported. 
		/// This can apply to WAV and AIFF files, and also MP3 files 
		/// when using the "MP3-free" BASS version
		/// </summary>
		BASS_ERROR_CODEC = 44,

		/// <summary>
		/// The specified SPEAKER flags are invalid. 
		/// The device/drivers do not support them, they are attempting to assign
		/// a stereo stream to a mono speaker or 3D functionality is enabled
		/// </summary>
		BASS_ERROR_SPEAKER = 42
		}

	/// <summary>
	/// Возможные ошибки инициализации спектрограммы
	/// </summary>
	public enum SpectrogramInitializationErrors
		{
		/// <summary>
		/// Спектрограмма успешно инициализирована
		/// </summary>
		InitOK = 0,

		/// <summary>
		/// Звуковой поток не инициализирован
		/// </summary>
		SoundStreamNotInitialized = -1,

		/// <summary>
		/// Спектрограмма уже инициализирована
		/// </summary>
		SpectrogramAlreadyInitialized = -2,

		/// <summary>
		/// Ширина или высота изображения находятся вне допустимого диапазона
		/// </summary>
		InvalidFrameSize = -3,

		/// <summary>
		/// Недостаточно памяти для работы со спектрограммой
		/// </summary>
		NotEnoughMemory = -4
		}

	/// <summary>
	/// Возможные режимы спектрограммы
	/// </summary>
	public enum SpectrogramModes
		{
		/// <summary>
		/// Без спектрограммы
		/// </summary>
		NoSpectrogram = 0,

		/// <summary>
		/// Статичная спектрограмма с курсором
		/// </summary>
		StaticSpectrogram,

		/// <summary>
		/// Движущаяся спектрограмма
		/// </summary>
		MovingSpectrogram,

		/// <summary>
		/// Симметричная вижущаяся спектрограмма
		/// </summary>
		SymmetricMovingSpectrogram,

		/// <summary>
		/// Гистограмма без симметрии
		/// </summary>
		HistogramWithNoSymmetry,

		/// <summary>
		/// Гистограмма с горизонтальной симметрией
		/// </summary>
		HistogramWithHorizontalSymmetry,

		/// <summary>
		/// Гистограмма с вертикальной симметрией
		/// </summary>
		HistogramWithVerticalSymmetry,

		/// <summary>
		/// Гистограмма с полной симметрией
		/// </summary>
		HistogramWithFullSymmetry,

		/// <summary>
		/// Статичная волновая форма
		/// </summary>
		StaticAmplitude,

		/// <summary>
		/// Движущаяся волновая форма
		/// </summary>
		MovingAmplitude
		}

	/// <summary>
	/// Класс обеспечивает доступ к функционалу библиотеки ConcurrentDrawLib
	/// </summary>
	public class ConcurrentDrawLib
		{
		// Переменные
		private static char[] splitter = new char[] { '\x1' };

		/// <summary>
		/// Функция получает имена устройств вывода звука (массив символов по 128 на имя)
		/// </summary>
		/// <param name="Devices">Названия устройств</param>
		/// <returns>Количество доступных устройств</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Byte GetDevicesEx (ref IntPtr Devices);

		/// <summary>
		/// Возвращает список доступных устройств считывания
		/// </summary>
		public static string[] AvailableDevices
			{
			get
				{
				// Запрос устройств
				IntPtr devs = IntPtr.Zero;
				if (GetDevicesEx (ref devs) == 0)
					return new string[] { };

				string devices = Marshal.PtrToStringAnsi (devs);
				return devices.Split (splitter, StringSplitOptions.RemoveEmptyEntries);
				}
			}

		// Статусы инициализации хранятся в DLL; здесь не дублируются

		/// <summary>
		/// Функция запускает процесс считывания данных со звукового вывода
		/// </summary>
		/// <param name="DeviceNumber">Номер звукового выхода</param>
		/// <returns>-10 - некорректная версия BASS, положительные коды ошибок и -1 - ошибки BASS</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Int16 InitializeSoundStreamEx (Byte DeviceNumber);

		/// <summary>
		/// Функция запускает процесс считывания данных из звукового файла
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Int16 InitializeFileStreamEx (string FileName);

		/// <summary>
		/// Метод инициализирует звуковой поток из устройства
		/// </summary>
		/// <param name="DeviceNumber">Номер устройства – звукового выхода</param>
		/// <returns>Возвращает состояния инициализации</returns>
		public static SoundStreamInitializationErrors InitializeSoundStream (uint DeviceNumber)
			{
			return (SoundStreamInitializationErrors)InitializeSoundStreamEx ((Byte)DeviceNumber);
			}

		/// <summary>
		/// Метод инициализирует звуковой поток из файла
		/// </summary>
		/// <param name="FileName">Имя файла</param>
		/// <returns>Возвращает состояния инициализации</returns>
		public static SoundStreamInitializationErrors InitializeSoundStream (string FileName)
			{
			return (SoundStreamInitializationErrors)InitializeFileStreamEx (FileName);
			}

		/// <summary>
		/// Функция завершает процесс считывания
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void DestroySoundStreamEx ();

		/// <summary>
		/// Метод освобождает звуковой выход
		/// </summary>
		public static void DestroySoundStream ()
			{
			DestroySoundStreamEx ();
			}

		/// <summary>
		/// Функция выполняет ручное обновление данных FFT вместо встроенного таймера
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void UpdateFFTDataEx ();

		/// <summary>
		/// Метод выполняет ручное обновление данных FFT вместо встроенного таймера
		/// </summary>
		public static void UpdateFFTData ()
			{
			UpdateFFTDataEx ();
			}

		/// <summary>
		/// Функция инициализирует спектрограмму
		/// </summary>
		/// <param name="FrameWidth">Ширина изображения спектрограммы</param>
		/// <param name="FrameHeight">Высота изображения спектрограммы</param>
		/// <param name="PaletteNumber">Номер палитры спектрограммы</param>
		/// <param name="SpectrogramMode">Режим спектрограммы</param>
		/// <param name="Flags">Флаги инициализации: b0 = double width</param>
		/// <returns>0 в случае успеха или отрицательный код ошибки</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Int16 InitializeSpectrogramEx (UInt16 FrameWidth, UInt16 FrameHeight,
			Byte PaletteNumber, Byte SpectrogramMode, Byte Flags);

		/// <summary>
		/// Метод инициализирует спектрограмму
		/// </summary>
		/// <param name="FrameWidth">Ширина изображения спектрограммы</param>
		/// <param name="FrameHeight">Высота изображения спектрограммы</param>
		/// <param name="PaletteNumber">Номер палитры спектрограммы</param>
		/// <param name="Mode">Режим спектрограммы</param>
		/// <param name="DoubleWidth">Флаг двойной ширины спектрограммы</param>
		/// <returns>Возвращает результат инициализации</returns>
		public static SpectrogramInitializationErrors InitializeSpectrogram (uint FrameWidth, uint FrameHeight,
			byte PaletteNumber, SpectrogramModes Mode, bool DoubleWidth)
			{
			return (SpectrogramInitializationErrors)InitializeSpectrogramEx ((UInt16)FrameWidth, (UInt16)FrameHeight,
				(Byte)PaletteNumber, (Byte)Mode, (byte)(DoubleWidth ? 1 : 0));
			}

		/// <summary>
		/// Функция удаляет активную спектрограмму
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void DestroySpectrogramEx ();

		/// <summary>
		/// Метод удаляет активную спектрограмму
		/// </summary>
		public static void DestroySpectrogram ()
			{
			DestroySpectrogramEx ();
			}

		/// <summary>
		/// Функция возвращает текущий фрейм спектрограммы
		/// </summary>
		/// <returns>Возвращает HBITMAP или NULL в случае ошибки</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern IntPtr GetSpectrogramFrameEx ();

		/// <summary>
		/// Возвращает текущее изображение спектрограммы или null при отсутствии инициализации
		/// </summary>
		public static Bitmap CurrentSpectrogramFrame
			{
			get
				{
				// Запрос фрейма
				IntPtr bmp = GetSpectrogramFrameEx ();
				if (bmp == IntPtr.Zero)
					return null;

				// Формирование изображения
				return Bitmap.FromHbitmap (bmp);
				}
			}

		/// <summary>
		/// Функция возвращает текущее значение пика
		/// </summary>
		/// <returns>Значение пика</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Byte GetCurrentPeakEx ();

		/// <summary>
		/// Возвращает текущее значение пика 
		/// </summary>
		public static byte CurrentPeak
			{
			get
				{
				return GetCurrentPeakEx ();
				}
			}

		/// <summary>
		/// Функция устанавливает метрики определения пикового значения
		/// </summary>
		/// <param name="LowEdge">Нижняя граница диапазона определения пика</param>
		/// <param name="HighEdge">Верхняя граница диапазона определения пика</param>
		/// <param name="LowLevel">Наименьшая амплитуда, на которой определяется пик</param>
		/// <param name="FFTScaleMultiplier">Множитель масштаба FFT-значений</param>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void SetPeakEvaluationParametersEx (UInt16 LowEdge, UInt16 HighEdge,
			Byte LowLevel, Byte FFTScaleMultiplier);

		/// <summary>
		/// Метод устанавливает метрики определения пикового значения
		/// </summary>
		/// <param name="LowEdge">Нижняя граница диапазона определения пика</param>
		/// <param name="HighEdge">Верхняя граница диапазона определения пика</param>
		/// <param name="LowLevel">Наименьшая амплитуда, на которой определяется пик</param>
		/// <param name="FFTScaleMultiplier">Множитель масштаба FFT-значений</param>
		public static void SetPeakEvaluationParameters (uint LowEdge, uint HighEdge, byte LowLevel,
			byte FFTScaleMultiplier)
			{
#if VIDEO
			SetPeakEvaluationParametersEx ((UInt16)LowEdge, (UInt16)HighEdge, LowLevel, (byte)(3 * FFTScaleMultiplier / 4));
#else
			SetPeakEvaluationParametersEx ((UInt16)LowEdge, (UInt16)HighEdge, LowLevel, FFTScaleMultiplier);
#endif
			}

		/// <summary>
		/// Функция возвращает основной цвет текущей палитры с указанной яркостью
		/// </summary>
		/// <param name="Brightness">Требуемая яркость</param>
		/// <returns>Цвет в представлении ARGB</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern UInt32 GetMasterPaletteColorEx (Byte Brightness);

		/// <summary>
		/// Метод возвращает основной цвет текущей палитры с указанной яркостью
		/// </summary>
		/// <param name="Brightness">Требуемая яркость</param>
		/// <returns>Основной цвет текущей палитры</returns>
		public static Color GetMasterPaletteColor (byte Brightness)
			{
			return Color.FromArgb ((int)GetMasterPaletteColorEx (Brightness));
			}

		/// <summary>
		/// Метод возвращает основной цвет текущей палитры с максимальной яркостью
		/// </summary>
		/// <returns>Основной цвет текущей палитры</returns>
		public static Color GetMasterPaletteColor ()
			{
			return Color.FromArgb ((int)GetMasterPaletteColorEx (255));
			}

		/// <summary>
		/// Функция возвращает названия доступных палитр
		/// </summary>
		/// <returns>Названия палитр</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern string GetPalettesNamesEx ();

		/// <summary>
		/// Возвращает список доступных палитр
		/// </summary>
		public static string[] AvailablePalettesNames
			{
			get
				{
				string names = GetPalettesNamesEx ();
				return names.Split (splitter, StringSplitOptions.RemoveEmptyEntries);
				}
			}

		/// <summary>
		/// Функция возвращает ограничивающие размеры фреймов спектрограмм
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern UInt64 GetSpectrogramFrameMetricsEx ();

		/// <summary>
		/// Возвращает минимально допустимую ширину фрейма спектрограммы
		/// </summary>
		public static uint MinSpectrogramFrameWidth
			{
			get
				{
				UInt64 v = GetSpectrogramFrameMetricsEx ();
				return (uint)((GetSpectrogramFrameMetricsEx () >> 48) & 0xFFFF);
				}
			}

		/// <summary>
		/// Возвращает максимально допустимую ширину фрейма спектрограммы
		/// </summary>
		public static uint MaxSpectrogramFrameWidth
			{
			get
				{
				return (uint)((GetSpectrogramFrameMetricsEx () >> 32) & 0xFFFF);
				}
			}

		/// <summary>
		/// Возвращает минимально допустимую высоту фрейма спектрограммы
		/// </summary>
		public static uint MinSpectrogramFrameHeight
			{
			get
				{
				return (uint)((GetSpectrogramFrameMetricsEx () >> 16) & 0xFFFF);
				}
			}

		/// <summary>
		/// Возвращает максимально допустимую высоту фрейма спектрограммы
		/// </summary>
		public static uint MaxSpectrogramFrameHeight
			{
			get
				{
				return (uint)(GetSpectrogramFrameMetricsEx () & 0xFFFF);
				}
			}

		/// <summary>
		/// Функция возвращает стандартные метрики определения пикового значения
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern UInt32 GetDefaultPeakEvaluationParametersEx ();

		/// <summary>
		/// Возвращает стандартную нижнюю границу диапазона частот определения пика
		/// </summary>
		public static byte DefaultPeakEvaluationLowEdge
			{
			get
				{
				return (byte)((GetDefaultPeakEvaluationParametersEx () >> 16) & 0xFF);
				}
			}

		/// <summary>
		/// Возвращает стандартную верхнюю границу диапазона частот определения пика
		/// </summary>
		public static byte DefaultPeakEvaluationHighEdge
			{
			get
				{
				return (byte)((GetDefaultPeakEvaluationParametersEx () >> 8) & 0xFF);
				}
			}

		/// <summary>
		/// Возвращает стандартную минимальную амплитуду определения пика
		/// </summary>
		public static byte DefaultPeakEvaluationLowLevel
			{
			get
				{
				return (byte)(GetDefaultPeakEvaluationParametersEx () & 0xFF);
				}
			}

		/// <summary>
		/// Возвращает стандартный множитель масштаба FFT-значений
		/// </summary>
		public static byte DefaultFFTScaleMultiplier
			{
			get
				{
				return (byte)((GetDefaultPeakEvaluationParametersEx () >> 24) & 0xFF);
				}
			}

		/// <summary>
		/// Функция возвращает масштабированное значение амплитуды на указанной частоте
		/// </summary>
		/// <param name="FrequencyLevel">Уровень, соответствующий требуемой частоте в масштабе</param>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Byte GetScaledAmplitudeEx (UInt16 FrequencyLevel);

		/// <summary>
		/// Метод возвращает масштабированное значение амплитуды на указанной частоте
		/// </summary>
		/// <param name="FrequencyLevel">Уровень, соответствующий требуемой частоте в масштабе</param>
		public static byte GetScaledAmplitude (uint FrequencyLevel)
			{
			return GetScaledAmplitudeEx ((UInt16)FrequencyLevel);
			}

		/// <summary>
		/// Функция формирует палитру (вручную, если спектрограмма не используется)
		/// </summary>
		/// <param name="PaletteNumber">Номер палитры</param>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void FillPaletteEx (Byte PaletteNumber);

		/// <summary>
		/// Метод формирует палитру (вручную, если спектрограмма не используется)
		/// </summary>
		/// <param name="PaletteNumber">Номер палитры</param>
		public static void FillPalette (byte PaletteNumber)
			{
			FillPaletteEx (PaletteNumber);
			}

		/// <summary>
		/// Функция получает указанный цвет из текущей палитры
		/// </summary>
		/// <param name="ColorNumber">Номер цвета в палитре</param>
		/// <returns>Цвет в представлении ARGB</returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern UInt32 GetColorFromPaletteEx (Byte ColorNumber);

		/// <summary>
		/// Метод возвращает указанный цвет из текущей палитры
		/// </summary>
		/// <param name="ColorNumber">Номер цвета в палитре</param>
		/// <returns>Требуемый цвет</returns>
		public static Color GetColorFromPalette (byte ColorNumber)
			{
			return Color.FromArgb ((int)GetColorFromPaletteEx (ColorNumber));
			}

		/// <summary>
		/// Функция возвращает версию библиотеки CDLib.dll
		/// </summary>
		/// <returns></returns>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern string GetCDLibVersionEx ();

		/// <summary>
		/// Возвращает версию библиотеки CDLib.dll
		/// </summary>
		public static string CDLibVersion
			{
			get
				{
				return GetCDLibVersionEx ();
				}
			}

		/// <summary>
		/// Функция устанавливает количество значений FFT, 
		/// которое будет использоваться в гистограммах
		/// </summary>
		/// <param name="Count">Количество значений</param>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern void SetHistogramFFTValuesCountEx (UInt16 Count);

		/// <summary>
		/// Метод устанавливает количество значений FFT, 
		/// которое будет использоваться в гистограммах
		/// </summary>
		/// <param name="Count">Количество значений</param>
		public static void SetHistogramFFTValuesCount (uint Count)
			{
			SetHistogramFFTValuesCountEx ((UInt16)Count);
			}

		/// <summary>
		/// Функция возвращает длину текущего файлового потока (для аудиовыхода всегда 0)
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern UInt16 GetChannelLengthEx ();

		/// <summary>
		/// Возвращает длину текущего файлового потока (для аудиовыхода всегда 0)
		/// </summary>
		public static uint ChannelLength
			{
			get
				{
				return GetChannelLengthEx ();
				}
			}

		/// <summary>
		/// Функция возвращает рекомендацию на сброс лого по признаку спецпалитр
		/// </summary>
		[DllImport (ProgramDescription.AssemblyRequirementsCDL)]
		private static extern Byte PaletteRequiresResetEx (Byte PaletteNumber);

		/// <summary>
		/// Метод возвращает рекомендацию на сброс лого по признаку спецпалитр
		/// </summary>
		/// <param name="PaletteNumber">Номер палитры</param>
		public static bool PaletteRequiresReset (byte PaletteNumber)
			{
			return (PaletteRequiresResetEx (PaletteNumber) != 0);
			}
		}
	}
