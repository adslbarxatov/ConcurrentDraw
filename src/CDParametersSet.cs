﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Предоставляет доступ к сохранённым параметрам программы
	/// </summary>
	public class CDParametersSet
		{
		// Параметры
		private char[] splitter = new char[] { ';' };
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
		private bool alwaysOnTop = false;	// Здесь и далее исходные значения представляют настройки по умолчанию

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
		private byte decumulationMultiplier = 16;

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
		/// Возвращает или задаёт количество значений FFT, которые используются при формировании гистограммы
		/// </summary>
		public uint HistogramFFTValuesCount
			{
			get
				{
				if (histogramFFTValuesCount < HistogramFFTValuesCountMinimum)
					return HistogramFFTValuesCountMinimum;

				return histogramFFTValuesCount;
				}
			set
				{
				if (value < HistogramFFTValuesCountMinimum)
					histogramFFTValuesCount = HistogramFFTValuesCountMinimum;
				else
					histogramFFTValuesCount = value;
				}
			}
		private uint histogramFFTValuesCount = 128;

		/// <summary>
		/// Константа, определяющая минимальное количество значений FFT
		/// </summary>
		public const uint HistogramFFTValuesCountMinimum = 32;

		/// <summary>
		/// Возвращает или задаёт скорость изменения угла поворота гистограммы
		/// </summary>
		public int HistoRotSpeedDelta
			{
			get
				{
				return histoRotSpeedDelta;
				}
			set
				{
				histoRotSpeedDelta = value;
				}
			}
		private int histoRotSpeedDelta = 0;

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
		private byte logoHeightPercentage = 30;

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
		/// Возвращает или задаёт флаг, указывающий на эффект тряски
		/// </summary>
		public bool ShakeEffect
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
		private bool shakeEffect = false;

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
				if ((value & 0xFFFC) == value)
					spectrogramHeight = value & 0xFFFC;			// Исправление, связанное с внутренней корректировкой высоты фрейма
				else
					spectrogramHeight = (value & 0xFFFC) + 4;	// (см. текст InitializeSpectrogramEx)
				}
			}
		private uint spectrogramHeight = 256;

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
				visualizationHeight = value;
				}
			}
		private uint visualizationHeight = 0;	// Определяется интерфейсом настроек

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
		public VisualizationModes VisualizationMode
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
		private VisualizationModes visualizationMode = VisualizationModes.Butterfly_histogram;

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
				visualizationWidth = value;
				}
			}
		private uint visualizationWidth = 0;

		/// <summary>
		/// Возвращает или задаёт нижнюю границу диапазона детекции битов
		/// </summary>
		public byte BeatsDetectorLowEdge
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
		private byte beatsDetectorLowEdge = ConcurrentDrawLib.DefaultPeakEvaluationLowEdge;

		/// <summary>
		/// Возвращает или задаёт верхнюю границу диапазона детекции битов
		/// </summary>
		public byte BeatsDetectorHighEdge
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
		private byte beatsDetectorHighEdge = ConcurrentDrawLib.DefaultPeakEvaluationHighEdge;

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
		public byte BeatsDetectorFFTScaleMultiplier
			{
			get
				{
				return beatsDetectorFFTScaleMultiplier;
				}
			set
				{
				beatsDetectorFFTScaleMultiplier = value;
				}
			}
		private byte beatsDetectorFFTScaleMultiplier = ConcurrentDrawLib.DefaultFFTScaleMultiplier;

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

			// Разбор сохранённых настроек
			try
				{
				string[] values = settings.Split (splitter, StringSplitOptions.RemoveEmptyEntries);

				deviceNumber = byte.Parse (values[0]);
				paletteNumber = byte.Parse (values[1]);

				visualizationMode = (VisualizationModes)int.Parse (values[2]);
				if ((uint)visualizationMode >= VisualizationModesChecker.VisualizationModesCount)
					visualizationMode = VisualizationModes.Butterfly_histogram;

				visualizationWidth = uint.Parse (values[4]);
				visualizationHeight = uint.Parse (values[5]);
				visualizationLeft = uint.Parse (values[6]);
				visualizationTop = uint.Parse (values[7]);
				spectrogramHeight = uint.Parse (values[3]);		// Установка размеров окна определяет максимум SDHeight

				spectrogramDoubleWidth = (values[8] != "0");
				alwaysOnTop = (values[9] != "0");

				histogramFFTValuesCount = uint.Parse (values[10]);
				decumulationMultiplier = byte.Parse (values[11]);
				cumulationSpeed = byte.Parse (values[12]);
				logoHeightPercentage = byte.Parse (values[13]);
				histoRotSpeedDelta = int.Parse (values[14]);
				shakeEffect = (values[15] != "0");

				beatsDetectorLowEdge = byte.Parse (values[16]);
				beatsDetectorHighEdge = byte.Parse (values[17]);
				beatsDetectorLowLevel = byte.Parse (values[18]);
				beatsDetectorFFTScaleMultiplier = byte.Parse (values[19]);

				logoCenterX = uint.Parse (values[20]);
				logoCenterY = uint.Parse (values[21]);
				}
			catch
				{
				initFailure = true;
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
			// Сборка строки настроек
			string settings = deviceNumber.ToString () + splitter[0].ToString () +
				paletteNumber.ToString () + splitter[0].ToString () +
				((uint)visualizationMode).ToString () + splitter[0].ToString () +
				spectrogramHeight.ToString () + splitter[0].ToString () +

				visualizationWidth.ToString () + splitter[0].ToString () +
				visualizationHeight.ToString () + splitter[0].ToString () +
				visualizationLeft.ToString () + splitter[0].ToString () +
				visualizationTop.ToString () + splitter[0].ToString () +

				(spectrogramDoubleWidth ? "SDW" : "0") + splitter[0].ToString () +
				(alwaysOnTop ? "AOT" : "0") + splitter[0].ToString () +

				histogramFFTValuesCount.ToString () + splitter[0].ToString () +
				decumulationMultiplier.ToString () + splitter[0].ToString () +
				cumulationSpeed.ToString () + splitter[0].ToString () +
				logoHeightPercentage.ToString () + splitter[0].ToString () +
				histoRotSpeedDelta.ToString () + splitter[0].ToString () +

				(shakeEffect ? "SE" : "0") + splitter[0].ToString () +

				beatsDetectorLowEdge.ToString () + splitter[0].ToString () +
				beatsDetectorHighEdge.ToString () + splitter[0].ToString () +
				beatsDetectorLowLevel.ToString () + splitter[0].ToString () +
				beatsDetectorFFTScaleMultiplier.ToString () + splitter[0].ToString () +

				logoCenterX.ToString () + splitter[0].ToString () +
				logoCenterY.ToString ();

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
			if ((SetName == null) || (SetName == "") || (SetName == Localization.LanguageValueName))
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
		}
	}