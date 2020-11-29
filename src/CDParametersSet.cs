using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace RD_AAOW
	{
	/// <summary>
	/// Предоставляет доступ к сохранённым параметрам программы
	/// </summary>
	public class CDParametersSet
		{
		// Параметры
		private char[] splitter = new char[] { '|', ';' };
		private const string DefaultSetName = "\x1";
		private const string SavedSetName = "\x2";

		/// <summary>
		/// Возвращает или задаёт флаг, требующий расположения окна поверх остальных
		/// </summary>
		public bool AlwaysOnTop
			{
			get
				{
				return alwaysOnTop;
				}
			set
				{
				alwaysOnTop = value;
				}
			}
		private bool alwaysOnTop = false;   // Здесь и далее исходные значения представляют настройки по умолчанию

		/// <summary>
		/// Возвращает или задаёт скорость накопления кумулятивного эффекта
		/// </summary>
		public byte CumulationSpeed
			{
			get
				{
				return cumulationSpeed;
				}
			set
				{
				cumulationSpeed = value;
				}
			}
		private byte cumulationSpeed = 70;

		/// <summary>
		/// Возвращает или задаёт множитель скорости ослабления кумулятивного эффекта
		/// </summary>
		public byte DecumulationMultiplier
			{
			get
				{
				return decumulationMultiplier;
				}
			set
				{
				decumulationMultiplier = value;
				}
			}
		private byte decumulationMultiplier = 15;

		/// <summary>
		/// Возвращает или задаёт флаг применения кумулятивного эффекта к остальным параметрам отображения
		/// </summary>
		public bool ExtendedCumulativeEffect
			{
			get
				{
				return extendedCumulativeEffect;
				}
			set
				{
				extendedCumulativeEffect = value;
				}
			}
		private bool extendedCumulativeEffect = false;

		/// <summary>
		/// Константа, определяющая максимум для шкалы ослабления кумулятивного эффекта
		/// </summary>
		public const byte DecumulationMultiplierMaximum = 20;

		/// <summary>
		/// Возвращает или задаёт номер выбранного устройства
		/// </summary>
		public byte DeviceNumber
			{
			get
				{
				return deviceNumber;
				}
			set
				{
				deviceNumber = value;
				}
			}
		private byte deviceNumber = 0;

		/// <summary>
		/// Возвращает или задаёт максимальную частоту выбранного диапазона гистограмм в герцовом масштабе
		/// </summary>
		public uint HistogramRangeMaximum
			{
			get
				{
				return histogramRangeMaximum;
				}
			set
				{
				histogramRangeMaximum = value;
				}
			}
		private uint histogramRangeMaximum = 6000 / HistogramRangeSettingIncrement;

		/// <summary>
		/// Константа, содержащая максимальную частоту гистограммы в герцах, получаемую из БПФ
		/// </summary>
		public const uint HistogramFrequencyMaximum = 22050;

		/// <summary>
		/// Константа, содержащая максимальную частоту гистограммы в герцах, обрабатываемую программой
		/// </summary>
		public const uint HistogramUsedFrequencyMaximum = 11025;

		/// <summary>
		/// Константа, содержащая максимальную частоту гистограммы, 
		/// масштабированную в количество значений FFT с учётом масштаба
		/// </summary>
		public const uint HistogramScaledFrequencyMaximum = 4096 * HistogramRangeSettingIncrement;

		/// <summary>
		/// Константа, содержащая шаг изменения границы диапазона частот гистограммы
		/// </summary>
		public const uint HistogramRangeSettingIncrement = 250;

		/// <summary>
		/// Возвращает или задаёт скорость изменения угла поворота гистограммы
		/// </summary>
		public int HistoRotationSpeedDelta
			{
			get
				{
				return histoRotationSpeedDelta;
				}
			set
				{
				histoRotationSpeedDelta = value;
				}
			}
		private int histoRotationSpeedDelta = 0;

		/// <summary>
		/// Возвращает или задаёт флаг вращения в соответствии с детектором битов
		/// </summary>
		public bool HistoRotationAccToBeats
			{
			get
				{
				return histoRotationAccToBeats;
				}
			set
				{
				histoRotationAccToBeats = value;
				}
			}
		private bool histoRotationAccToBeats = false;

		/// <summary>
		/// Возвращает или задаёт начальный угол поворота гистограммы
		/// </summary>
		public uint HistoRotInitialAngle
			{
			get
				{
				return histoRotInitialAngle;
				}
			set
				{
				histoRotInitialAngle = value;
				}
			}
		private uint histoRotInitialAngle = 0;

		/// <summary>
		/// Возвращает или задаёт высоту лого в процентах от высоты окна
		/// </summary>
		public byte LogoHeightPercentage
			{
			get
				{
				return logoHeightPercentage;
				}
			set
				{
				logoHeightPercentage = value;
				}
			}
		private byte logoHeightPercentage = 60;

		/// <summary>
		/// Возвращает или задаёт номер выбранной палитры
		/// </summary>
		public byte PaletteNumber
			{
			get
				{
				return paletteNumber;
				}
			set
				{
				paletteNumber = value;
				}
			}
		private byte paletteNumber = 0;

		/// <summary>
		/// Возвращает или задаёт силу тряски
		/// </summary>
		public uint ShakeEffect
			{
			get
				{
				return shakeEffect;
				}
			set
				{
				shakeEffect = value;
				}
			}
		private uint shakeEffect = 0;

		/// <summary>
		/// Возвращает или задаёт высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return spectrogramHeight;
				}
			set
				{
				//if ((value & 0xFFFC) == value)
				spectrogramHeight = value & 0xFFFC;         // Исправление, связанное с внутренней корректировкой высоты фрейма
															//else
															//	spectrogramHeight = (value & 0xFFFC) + 4;	// (см. текст InitializeSpectrogramEx)
				}
			}
		private uint spectrogramHeight = 256;

		/// <summary>
		/// Возвращает или задаёт смещение изображения диаграммы от верха окна
		/// </summary>
		public uint SpectrogramTopOffset
			{
			get
				{
				return spectrogramTopOffset;
				}
			set
				{
				spectrogramTopOffset = value;
				}
			}
		private uint spectrogramTopOffset = 0;  // Определяется интерфейсом настроек

		/// <summary>
		/// Возвращает или задаёт высоту окна визуализации
		/// </summary>
		public uint VisualizationHeight
			{
			get
				{
				return visualizationHeight;
				}
			set
				{
				//if ((value & 0xFFFC) == value)
				visualizationHeight = value & 0xFFFC;   // Исправление, связанное с внутренней корректировкой высоты фрейма
														//else
														//	visualizationHeight = (value & 0xFFFC) + 4;
				}
			}
		private uint visualizationHeight = 0;   // Определяется интерфейсом настроек

		/// <summary>
		/// Возвращает или заадёт левый отступ окна визуализации
		/// </summary>
		public uint VisualizationLeft
			{
			get
				{
				return visualizationLeft;
				}
			set
				{
				visualizationLeft = value;
				}
			}
		private uint visualizationLeft = 0;

		/// <summary>
		/// Возвращает или задаёт режим визуализации
		/// </summary>
		public int VisualizationMode
			{
			get
				{
				return visualizationMode;
				}
			set
				{
				visualizationMode = value;
				}
			}
		private int visualizationMode = (int)VisualizationModes.Butterfly_histogram;

		/// <summary>
		/// Возвращает или задаёт верхний отступ окна визуализации
		/// </summary>
		public uint VisualizationTop
			{
			get
				{
				return visualizationTop;
				}
			set
				{
				visualizationTop = value;
				}
			}
		private uint visualizationTop = 0;

		/// <summary>
		/// Возвращает или задаёт ширину окна визуализации
		/// </summary>
		public uint VisualizationWidth
			{
			get
				{
				return visualizationWidth;
				}
			set
				{
				visualizationWidth = value & 0xFFFC;  // Исправление, связанное с внутренней корректировкой высоты фрейма
				}
			}
		private uint visualizationWidth = 0;

		/// <summary>
		/// Возвращает или задаёт нижнюю границу диапазона детекции битов
		/// </summary>
		public uint BeatsDetectorLowEdge
			{
			get
				{
				return beatsDetectorLowEdge;
				}
			set
				{
				beatsDetectorLowEdge = value;
				}
			}
		private uint beatsDetectorLowEdge = ConcurrentDrawLib.DefaultPeakEvaluationLowEdge;

		/// <summary>
		/// Возвращает или задаёт верхнюю границу диапазона детекции битов
		/// </summary>
		public uint BeatsDetectorHighEdge
			{
			get
				{
				return beatsDetectorHighEdge;
				}
			set
				{
				beatsDetectorHighEdge = value;
				}
			}
		private uint beatsDetectorHighEdge = ConcurrentDrawLib.DefaultPeakEvaluationHighEdge;

		/// <summary>
		/// Возвращает или задаёт порог амплитуды детекции битов
		/// </summary>
		public byte BeatsDetectorLowLevel
			{
			get
				{
				return beatsDetectorLowLevel;
				}
			set
				{
				beatsDetectorLowLevel = value;
				}
			}
		private byte beatsDetectorLowLevel = ConcurrentDrawLib.DefaultPeakEvaluationLowLevel;

		/// <summary>
		/// Возвращает или задаёт множитель амплитуды детектора битов
		/// </summary>
		public byte FFTScaleMultiplier
			{
			get
				{
				return fftScaleMultiplier;
				}
			set
				{
				fftScaleMultiplier = value;
				}
			}
		private byte fftScaleMultiplier = ConcurrentDrawLib.DefaultFFTScaleMultiplier;

		/// <summary>
		/// Возвращает или задаёт флаг двойной ширины спектрограммы
		/// </summary>
		public bool SpectrogramDoubleWidth
			{
			get
				{
				return spectrogramDoubleWidth;
				}
			set
				{
				spectrogramDoubleWidth = value;
				}
			}
		private bool spectrogramDoubleWidth = false;

		/// <summary>
		/// Возвращает или задаёт относительную абсциссу центра поля отрисовки лого
		/// </summary>
		public uint LogoCenterX
			{
			get
				{
				return logoCenterX;
				}
			set
				{
				logoCenterX = value;
				}
			}
		private uint logoCenterX = 50;

		/// <summary>
		/// Возвращает или задаёт относительную ординату центра поля отрисовки лого
		/// </summary>
		public uint LogoCenterY
			{
			get
				{
				return logoCenterY;
				}
			set
				{
				logoCenterY = value;
				}
			}
		private uint logoCenterY = 50;

		/// <summary>
		/// Возвращает или задаёт флаг, указывающий на качание гистограммы вместо вращения
		/// </summary>
		public bool SwingingHistogram
			{
			get
				{
				return swingingHistogram;
				}
			set
				{
				swingingHistogram = value;
				}
			}
		private bool swingingHistogram = false;

		/// <summary>
		/// Возвращает или задаёт флаг волн бит-детектора
		/// </summary>
		public bool BeatDetectorWaves
			{
			get
				{
				return beatDetectorWaves;
				}
			set
				{
				beatDetectorWaves = value;
				}
			}
		private bool beatDetectorWaves = false;

		/// <summary>
		/// Возвращает или задаёт метрики генерации дополнительных графических объектов
		/// </summary>
		public LogoDrawerObjectMetrics ParticlesMetrics
			{
			get
				{
				return particlesMetrics;
				}
			set
				{
				particlesMetrics = value;
				}
			}
		private LogoDrawerObjectMetrics particlesMetrics;

		/// <summary>
		/// Конструктор. Инициализирует экземпляр настройками по умолчанию
		/// </summary>
		/// <param name="DefaultSettings">Флаг указывает, следует ли загрузить стандартные настройки
		/// или последние сохранённые настройки</param>
		public CDParametersSet (bool DefaultSettings)
			{
			if (DefaultSettings)
				InitParametersSet (DefaultSetName);
			else
				InitParametersSet (SavedSetName);

			GetSettingsNames ();
			}

		/// <summary>
		/// Конструктор. Инициализирует экземпляр сохранёнными настройками
		/// </summary>
		/// <param name="SetName">Название набора настроек</param>
		public CDParametersSet (string SetName)
			{
			InitParametersSet (SetName);
			}

		// Метод загружает настройки программы
		private void InitParametersSet (string SetName)
			{
			// Инициализация несохраняемых параметров
			particlesMetrics.MaxSpeed = 3;
			particlesMetrics.MinSpeed = 1;
			particlesMetrics.MinSize = 5;
			particlesMetrics.MaxSize = 10;
			particlesMetrics.PolygonsSidesCount = 6;

			particlesMetrics.Acceleration = 0;
			particlesMetrics.AsStars = true;
			particlesMetrics.Enlarging = 0;
			//particlesMetrics.KeepTracks = false;
			particlesMetrics.MaxRed = 255;
			particlesMetrics.MaxGreen = 255;
			particlesMetrics.MaxBlue = 255;
			particlesMetrics.MinRed = 128;
			particlesMetrics.MinGreen = 128;
			particlesMetrics.MinBlue = 128;
			particlesMetrics.ObjectsCount = 0;
			particlesMetrics.ObjectsType = LogoDrawerObjectTypes.RotatingStars;
			particlesMetrics.Rotation = true;
			particlesMetrics.StartupPosition = LogoDrawerObjectStartupPositions.Top;
			particlesMetrics.MaxSpeedFluctuation = 0;

			// Возврат стандартного набора настроек
			if (SetName == DefaultSetName)
				return;

			// Запрос
			string settings = "";

			// Возврат последнего сохранённого набора настроек
			try
				{
				if (SetName == SavedSetName)
					{
					settings = Registry.GetValue (ProgramDescription.AssemblySettingsKey, "", "").ToString ();
					}
				else
					{
					settings = Registry.GetValue (ProgramDescription.AssemblySettingsKey, SetName, "").ToString ();
					}
				}
			catch
				{
				}

			if (settings == "")
				{
				initFailure = true;
				return;
				}

			string[] values;
			try
				{
				// Разбор сохранённых настроек
				values = settings.Split (splitter, StringSplitOptions.RemoveEmptyEntries);

				deviceNumber = byte.Parse (values[0]);
				paletteNumber = byte.Parse (values[1]);

				visualizationMode = int.Parse (values[2]);
				if (Math.Abs (visualizationMode) >= VisualizationModesChecker.VisualizationModesCount)
					visualizationMode = (int)VisualizationModes.Butterfly_histogram;

				spectrogramHeight = uint.Parse (values[3]);
				visualizationWidth = uint.Parse (values[4]);
				visualizationHeight = uint.Parse (values[5]);
				visualizationLeft = uint.Parse (values[6]);
				visualizationTop = uint.Parse (values[7]);

				spectrogramDoubleWidth = (values[8] != "0");
				alwaysOnTop = (values[9] != "0");

				histogramRangeMaximum = uint.Parse (values[10]);
				decumulationMultiplier = byte.Parse (values[11]);
				cumulationSpeed = byte.Parse (values[12]);
				logoHeightPercentage = byte.Parse (values[13]);
				histoRotationSpeedDelta = int.Parse (values[14]);
				shakeEffect = uint.Parse (values[15]);

				beatsDetectorLowEdge = byte.Parse (values[16]);
				beatsDetectorHighEdge = byte.Parse (values[17]);
				beatsDetectorLowLevel = byte.Parse (values[18]);
				fftScaleMultiplier = byte.Parse (values[19]);

				logoCenterX = uint.Parse (values[20]);
				logoCenterY = uint.Parse (values[21]);

				swingingHistogram = (values[22] != "0");
				spectrogramTopOffset = uint.Parse (values[23]);
				beatDetectorWaves = (values[24] != "0");

				histoRotInitialAngle = uint.Parse (values[25]);
				}
			catch
				{
				initFailure = true;
				return;
				}

			// Отдельный разбор новых настроек с игнорированием ошибок
			try
				{
				particlesMetrics.MaxSpeed = uint.Parse (values[26]);
				particlesMetrics.MinSpeed = uint.Parse (values[27]);
				particlesMetrics.MinSize = uint.Parse (values[28]);
				particlesMetrics.MaxSize = uint.Parse (values[29]);
				particlesMetrics.PolygonsSidesCount = byte.Parse (values[30]);
				particlesMetrics.AsStars = (values[32] != "0");
				particlesMetrics.Enlarging = int.Parse (values[33]);
				particlesMetrics.MaxRed = byte.Parse (values[34]);
				particlesMetrics.MaxGreen = byte.Parse (values[35]);
				particlesMetrics.MaxBlue = byte.Parse (values[36]);
				particlesMetrics.MinRed = byte.Parse (values[37]);
				particlesMetrics.MinGreen = byte.Parse (values[38]);
				particlesMetrics.MinBlue = byte.Parse (values[39]);
				particlesMetrics.ObjectsCount = byte.Parse (values[40]);
				particlesMetrics.ObjectsType = (LogoDrawerObjectTypes)byte.Parse (values[41]);
				particlesMetrics.Rotation = (values[42] != "0");
				particlesMetrics.StartupPosition = (LogoDrawerObjectStartupPositions)byte.Parse (values[43]);
				particlesMetrics.MaxSpeedFluctuation = uint.Parse (values[44]);
				histoRotationAccToBeats = (values[45] != "0");
				extendedCumulativeEffect = (values[46] != "0");

				// Новая для версии 1.50
				particlesMetrics.Acceleration = uint.Parse (values[31]);
				}
			catch
				{
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на ошибку инициализации параметров
		/// </summary>
		public bool InitFailure
			{
			get
				{
				return initFailure;
				}
			}
		private bool initFailure = false;

		/// <summary>
		/// Метод сохраняет указанные настройки как выбранные
		/// </summary>
		public void SaveSettings ()
			{
			SaveSettings (SavedSetName);
			}

		/// <summary>
		/// Метод сохраняет указанные настройки под указанным именем
		/// </summary>
		/// <param name="SetName">Название настроек</param>
		public void SaveSettings (string SetName)
			{
			if ((SetName == null) || (SetName == "") || (SetName == Localization.LanguageValueName) ||
				(SetName == AboutForm.LastShownVersionKey))
				return;

			// Сборка строки настроек
			string settings = deviceNumber.ToString () + splitter[0].ToString () +
				paletteNumber.ToString () + splitter[0].ToString () +
				visualizationMode.ToString () + splitter[0].ToString () +
				spectrogramHeight.ToString () + splitter[0].ToString () +

				visualizationWidth.ToString () + splitter[0].ToString () +
				visualizationHeight.ToString () + splitter[0].ToString () +
				visualizationLeft.ToString () + splitter[0].ToString () +
				visualizationTop.ToString () + splitter[0].ToString () +

				(spectrogramDoubleWidth ? "SDW" : "0") + splitter[0].ToString () +
				(alwaysOnTop ? "AOT" : "0") + splitter[0].ToString () +

				histogramRangeMaximum.ToString () + splitter[0].ToString () +
				decumulationMultiplier.ToString () + splitter[0].ToString () +
				cumulationSpeed.ToString () + splitter[0].ToString () +
				logoHeightPercentage.ToString () + splitter[0].ToString () +
				histoRotationSpeedDelta.ToString () + splitter[0].ToString () +

				shakeEffect.ToString () + splitter[0].ToString () +

				beatsDetectorLowEdge.ToString () + splitter[0].ToString () +
				beatsDetectorHighEdge.ToString () + splitter[0].ToString () +
				beatsDetectorLowLevel.ToString () + splitter[0].ToString () +
				fftScaleMultiplier.ToString () + splitter[0].ToString () +

				logoCenterX.ToString () + splitter[0].ToString () +
				logoCenterY.ToString () + splitter[0].ToString () +
				(swingingHistogram ? "SH" : "0") + splitter[0].ToString () +
				spectrogramTopOffset.ToString () + splitter[0].ToString () +
				(beatDetectorWaves ? "BW" : "0") + splitter[0].ToString () +
				histoRotInitialAngle.ToString ();

			settings += (splitter[1].ToString () + particlesMetrics.MaxSpeed.ToString () + splitter[0].ToString () +
				particlesMetrics.MinSpeed.ToString () + splitter[0].ToString () +
				particlesMetrics.MinSize.ToString () + splitter[0].ToString () +
				particlesMetrics.MaxSize.ToString () + splitter[0].ToString () +
				particlesMetrics.PolygonsSidesCount.ToString () + splitter[0].ToString () +
				particlesMetrics.Acceleration.ToString () + splitter[0].ToString () +
				(particlesMetrics.AsStars ? "AS" : "0") + splitter[0].ToString () +
				particlesMetrics.Enlarging.ToString () + splitter[0].ToString () +
				particlesMetrics.MaxRed.ToString () + splitter[0].ToString () +
				particlesMetrics.MaxGreen.ToString () + splitter[0].ToString () +
				particlesMetrics.MaxBlue.ToString () + splitter[0].ToString () +
				particlesMetrics.MinRed.ToString () + splitter[0].ToString () +
				particlesMetrics.MinGreen.ToString () + splitter[0].ToString () +
				particlesMetrics.MinBlue.ToString () + splitter[0].ToString () +
				particlesMetrics.ObjectsCount.ToString () + splitter[0].ToString () +
				((byte)particlesMetrics.ObjectsType).ToString () + splitter[0].ToString () +
				(particlesMetrics.Rotation ? "R" : "0") + splitter[0].ToString () +
				((byte)particlesMetrics.StartupPosition).ToString () + splitter[0].ToString () +
				particlesMetrics.MaxSpeedFluctuation.ToString ());

			settings += (splitter[1].ToString () + (histoRotationAccToBeats ? "RAB" : "0") +
				splitter[0].ToString () + (extendedCumulativeEffect ? "ECE" : "0"));

			// Запись
			try
				{
				if (SetName == SavedSetName)
					{
					Registry.SetValue (ProgramDescription.AssemblySettingsKey, "", settings);
					}
				else
					{
					Registry.SetValue (ProgramDescription.AssemblySettingsKey, SetName, settings);
					}
				}
			catch
				{
				}
			}

		/// <summary>
		/// Метод возвращает список имён наборов настроек
		/// </summary>
		/// <returns>Список имён наборов настроек</returns>
		public static string[] GetSettingsNames ()
			{
			// Получение ключа реестра
			RegistryKey rk = null;
			try
				{
				rk = Registry.LocalMachine.OpenSubKey
					(ProgramDescription.AssemblySettingsKey.Replace (Registry.LocalMachine.Name + "\\", ""));
				}
			catch
				{
				}
			if (rk == null)
				return new string[] { };

			// Получение списка
			List<string> s = null;
			try
				{
				s = new List<string> (rk.GetValueNames ());
				}
			catch
				{
				return new string[] { };
				}
			s.Remove ("");
			s.Remove (Localization.LanguageValueName);
			s.Remove (AboutForm.LastShownVersionKey);

			// Возврат
			rk.Dispose ();
			return s.ToArray ();
			}

		/// <summary>
		/// Метод удаляет сохранённые настройки
		/// </summary>
		/// <param name="SetName">Удаляемый набор настроек</param>
		public static void RemoveSettings (string SetName)
			{
			// Контроль
			if ((SetName == null) || (SetName == "") || (SetName == Localization.LanguageValueName) ||
				(SetName == AboutForm.LastShownVersionKey))
				return;

			// Получение ключа реестра
			RegistryKey rk = null;
			try
				{
				rk = Registry.LocalMachine.OpenSubKey
					(ProgramDescription.AssemblySettingsKey.Replace (Registry.LocalMachine.Name + "\\", ""), true);
				}
			catch
				{
				}
			if (rk == null)
				return;

			// Удаление
			try
				{
				rk.DeleteValue (SetName);
				}
			catch
				{
				}

			// Завершено
			rk.Dispose ();
			}
		}
	}
