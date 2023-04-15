using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает форму доступа к параметрам программы
	/// </summary>
	public partial class ConcurrentDrawParameters: Form
		{
		// Константы и переменные
		private List<CDParametersSet> parameters = new List<CDParametersSet> ();    // Наборы сохранённых параметров

		private const int defaultSettingsNumber = 0;
		private const int savedSettingsNumber = 1;
		private const int DSN = defaultSettingsNumber;
		private const int SSN = savedSettingsNumber;

		private uint histoRange = 1;

		#region Interface masters

		/// <summary>
		/// Конструктор. Позволяет запросить параметры из реестра.
		/// При отсутствии записи в реестре будет вызвано окно настроек
		/// </summary>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="ScreenWidth">Ширина экрана</param>
		public ConcurrentDrawParameters (uint ScreenWidth, uint ScreenHeight)
			{
			// Инициализация
			InitializeComponent ();

			// Кнопки по умолчанию
			this.AcceptButton = BOK;
			this.CancelButton = BCancel;

			// Запрос стандартных настроек и балластное заполнение второго поля
			parameters.Add (new CDParametersSet (true));
			parameters.Add (new CDParametersSet (true));
			ProfileCombo.Items.Add ("0");
			ProfileCombo.Items.Add ("1");

			// Запрос сохранённых настроек
			string[] s = CDParametersSet.GetSettingsNames ();
			for (int i = 0; i < s.Length; i++)
				{
				parameters.Add (new CDParametersSet (s[i]));
				ProfileCombo.Items.Add (s[i]);
				}

			// Настройка контролов

			// Устройства
			DevicesCombo.Items.AddRange (ConcurrentDrawLib.AvailableDevices);
			if (DevicesCombo.Items.Count < 1)
				{
				DevicesCombo.Items.Add (Localization.GetText ("CDP_NoDevices"));
				DevicesCombo.Enabled = DevicesLabel.Enabled = false;
				}
			DevicesCombo.SelectedIndex = parameters[DSN].DeviceNumber;

			// Палитра
			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = parameters[DSN].PaletteNumber;

			// Режим
			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				{
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ().Replace ('_', ' '));
				}
			VisualizationCombo.SelectedIndex = Math.Abs (parameters[DSN].VisualizationMode);
			WithLogoFlag.Checked = (parameters[DSN].VisualizationMode >= 0);
			ShakeValue.Value = parameters[DSN].ShakeEffect;

			// Высота спектрограммы
			SGHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SGHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;
			SGHeight.Value = parameters[DSN].SpectrogramHeight;

			// Размеры визуализации 
			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = ScreenHeight;

			VisWidth.Value = (int)(9 * VisWidth.Maximum / 16);
			parameters[DSN].VisualizationWidth = (uint)VisWidth.Value;
			VisHeight.Value = (int)(9 * VisHeight.Maximum / 16);    // По умолчанию - (9 / 16) размера экрана
			parameters[DSN].VisualizationHeight = (uint)VisHeight.Value;

			// Позиция визуализации
			VisLeft.Value = ScreenWidth - VisWidth.Value;   // По умолчанию - верхняя правая четверть экрана
			parameters[DSN].VisualizationLeft = (uint)VisLeft.Value;

			// Смещение спектрограммы / гистограммы
			SGTopOffset.Minimum = 0;    // Сбивается из-за вызова обработчика изменения SGHeight до установки ограничений на окно
			SGTopOffset.Value = SGTopOffset.Maximum;
			parameters[DSN].SpectrogramTopOffset = (uint)SGTopOffset.Value;
			// Максимумы теперь зависят от размеров окна визуализации; задаются в соответствующем обработчике

			// Параметры детектора битов (получаются из DLL)
			BDLowEdge.Value = (int)parameters[DSN].BeatsDetectorLowEdge;
			BDHighEdge.Value = (int)parameters[DSN].BeatsDetectorHighEdge;
			BDLowLevel.Value = parameters[DSN].BeatsDetectorLowLevel;
			FFTScaleMultiplier.Value = parameters[DSN].FFTScaleMultiplier;

			// Плотность гистограммы
			histoRange = parameters[DSN].HistogramRangeMaximum;

			// Кумулятивный эффект
			CEDecumulationMultiplier.Maximum = (int)CDParametersSet.DecumulationMultiplierMaximum;
			CEDecumulationMultiplier.Value = parameters[DSN].DecumulationMultiplier;

			CECumulationSpeed.Value = parameters[DSN].CumulationSpeed;
			LogoHeightPercentage.Value = parameters[DSN].LogoHeightPercentage;

			// Скорость вращения гистограммы
			HistoRotSpeedAngle.Value = parameters[DSN].HistoRotationSpeedDelta;
			if (parameters[DSN].HistoRotationAccToBeats)
				HistoRotAccToBeats.Checked = true;
			else
				HistoRotSpeed.Checked = true;
			HistoRotInitialAngle.Value = parameters[DSN].HistoRotInitialAngle;

			// Флаги
			AlwaysOnTopFlag.Checked = parameters[DSN].AlwaysOnTop;
			SDDoubleWidthFlag.Checked = parameters[DSN].SpectrogramDoubleWidth;
			SwingingHistogramFlag.Checked = parameters[DSN].SwingingHistogram;
			BeatWavesFlag.Checked = parameters[DSN].BeatDetectorWaves;

			// Язык интерфейса
			LanguageCombo.Items.AddRange (Localization.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)Localization.CurrentLanguage;
				// По умолчанию - язык системы или английский
				}
			catch
				{
				LanguageCombo.SelectedIndex = 0;
				}

			// Метрики объектов
			for (int i = 0; i < LogoDrawerSupport.ObjectTypesCount; i++)
				ObjectsTypeCombo.Items.Add (((LogoDrawerObjectTypes)i).ToString ());
			ObjectsTypeCombo.SelectedIndex = (int)parameters[DSN].ParticlesMetrics.ObjectsType;

			ObjectsCountField.Maximum = LogoDrawerSupport.MaxObjectsCount;
			ObjectsCountField.Value = parameters[DSN].ParticlesMetrics.ObjectsCount;

			ObjectsSidesCountField.Minimum = LogoDrawerSupport.MinPolygonsSidesCount;
			ObjectsSidesCountField.Maximum = LogoDrawerSupport.MaxPolygonsSidesCount;
			ObjectsSidesCountField.Value = parameters[DSN].ParticlesMetrics.PolygonsSidesCount;

			for (int i = 0; i < LogoDrawerSupport.ObjectStartupPositionsCount; i++)
				ObjectsStartupSideCombo.Items.Add (((LogoDrawerObjectStartupPositions)i).ToString ());
			ObjectsStartupSideCombo.SelectedIndex = (int)parameters[DSN].ParticlesMetrics.StartupPosition;

			ObjectsEnlargingCoeffField.Minimum = -LogoDrawerSupport.MaxEnlarge;
			ObjectsEnlargingCoeffField.Maximum = LogoDrawerSupport.MaxEnlarge;
			ObjectsEnlargingCoeffField.Value = parameters[DSN].ParticlesMetrics.Enlarging;

			ObjectsAccelerationField.Minimum = 0;
			ObjectsAccelerationField.Maximum = LogoDrawerSupport.MaxAcceleration;
			ObjectsAccelerationField.Value = parameters[DSN].ParticlesMetrics.Acceleration;

			ObjectsMinSpeedField.Minimum = ObjectsMaxSpeedField.Minimum = LogoDrawerSupport.MinObjectSpeed;
			ObjectsMinSpeedField.Maximum = ObjectsMaxSpeedField.Maximum =
				ObjectsSpeedFluctuationField.Maximum = LogoDrawerSupport.MaxObjectSpeed;
			ObjectsMaxSpeedField.Value = parameters[DSN].ParticlesMetrics.MaxSpeed;
			ObjectsMinSpeedField.Value = parameters[DSN].ParticlesMetrics.MinSpeed;
			ObjectsSpeedFluctuationField.Value = parameters[DSN].ParticlesMetrics.MaxSpeedFluctuation;

			ObjectsMinSizeField.Minimum = ObjectsMaxSizeField.Minimum = LogoDrawerSupport.MinObjectSize;
			ObjectsMinSizeField.Maximum = ObjectsMaxSizeField.Maximum = LogoDrawerSupport.MaxObjectSize;
			ObjectsMaxSizeField.Value = parameters[DSN].ParticlesMetrics.MaxSize;
			ObjectsMinSizeField.Value = parameters[DSN].ParticlesMetrics.MinSize;

			//ObjectsKeepTracksFlag.Checked = parameters[DSN].ParticlesMetrics.KeepTracks;

			ObjectsMaxColor.BackColor = Color.FromArgb (parameters[DSN].ParticlesMetrics.MaxRed,
				parameters[DSN].ParticlesMetrics.MaxGreen, parameters[DSN].ParticlesMetrics.MaxBlue);
			ColorPicker_Click (ObjectsMaxColor, null);

			ObjectsMinColor.BackColor = Color.FromArgb (parameters[DSN].ParticlesMetrics.MinRed,
				parameters[DSN].ParticlesMetrics.MinGreen, parameters[DSN].ParticlesMetrics.MinBlue);
			ColorPicker_Click (ObjectsMinColor, null);

			// Запрос настроек
			bool requestRequired = GetSettings (SSN);

			// Установка настроек
			ConcurrentDrawLib.SetPeakEvaluationParameters (parameters[SSN].BeatsDetectorLowEdge,
				parameters[SSN].BeatsDetectorHighEdge, parameters[SSN].BeatsDetectorLowLevel,
				parameters[SSN].FFTScaleMultiplier);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[SSN].HistogramRangeMaximum *
				CDParametersSet.HistogramScaledFrequencyMaximum / CDParametersSet.HistogramFrequencyMaximum,
				parameters[SSN].ReverseFreqOrder);

			// Запуск окна немедленно, ести требуется
			BCancel.Enabled = !requestRequired;

			if (requestRequired)
				this.ShowDialog ();
			}

		// Метод получает настройки из реестра; возвращает true, если настройки требуется ввести вручную
		private bool GetSettings (uint ParametersSetNumber)
			{
			// Переменные
			bool req = false;
			if (ParametersSetNumber >= parameters.Count)
				return true;
			int psn = (int)ParametersSetNumber;

			// Запрос сохранённых параметров
			if (psn == SSN)
				{
				parameters[psn] = new CDParametersSet (false);
				if (parameters[psn].InitFailure)
					{
					ConcurrentDrawLogo cdl = new ConcurrentDrawLogo ();
					cdl.Dispose ();

					RDGenerics.ShowAbout (true);    // Справка на случай первого запуска
					req = true;
					}
				}

			// Разбор сохранённых настроек
			try
				{
				SDPaletteCombo.SelectedIndex = parameters[psn].PaletteNumber;

				if (Math.Abs (parameters[psn].VisualizationMode) >= VisualizationModesChecker.VisualizationModesCount)
					parameters[psn].VisualizationMode = (int)VisualizationModes.Butterfly_histogram_with_full_symmetry;
				VisualizationCombo.SelectedIndex = Math.Abs (parameters[psn].VisualizationMode);
				WithLogoFlag.Checked = (parameters[psn].VisualizationMode >= 0);

				BDLowEdge.Value = (int)parameters[psn].BeatsDetectorLowEdge;
				BDHighEdge.Value = (int)parameters[psn].BeatsDetectorHighEdge;
				BDLowLevel.Value = parameters[psn].BeatsDetectorLowLevel;
				FFTScaleMultiplier.Value = parameters[psn].FFTScaleMultiplier;

				SDDoubleWidthFlag.Checked = parameters[psn].SpectrogramDoubleWidth;
				AlwaysOnTopFlag.Checked = parameters[psn].AlwaysOnTop;
				SwingingHistogramFlag.Checked = parameters[psn].SwingingHistogram;

				CEDecumulationMultiplier.Value = parameters[psn].DecumulationMultiplier;
				CECumulationSpeed.Value = parameters[psn].CumulationSpeed;
				ExtendedCumulation.Checked = parameters[psn].ExtendedCumulativeEffect;

				LogoHeightPercentage.Value = parameters[psn].LogoHeightPercentage;
				LogoCenterXTrack.Value = (int)parameters[psn].LogoCenterX;
				LogoCenterYTrack.Value = (int)(LogoCenterYTrack.Maximum - parameters[psn].LogoCenterY);

				if (parameters[psn].HistoRotationAccToBeats)
					HistoRotAccToBeats.Checked = true;
				else
					HistoRotSpeed.Checked = true;
				HistoRotSpeedAngle.Value = (decimal)(parameters[psn].HistoRotationSpeedDelta / 10.0);
				HistoRotInitialAngle.Value = parameters[psn].HistoRotInitialAngle;

				ShakeValue.Value = parameters[psn].ShakeEffect;
				BeatWavesFlag.Checked = parameters[psn].BeatDetectorWaves;
				ReverseFreqOrderFlag.Checked = parameters[psn].ReverseFreqOrder;

				// Эти параметры перемещены в конец, т.к. могут вызывать ошибки при запусках, не зависящие от программы
				histoRange = parameters[psn].HistogramRangeMaximum;
				HistogramRangeField_ValueChanged (null, null);
				DevicesCombo.SelectedIndex = parameters[psn].DeviceNumber;

				VisWidth.Value = parameters[psn].VisualizationWidth;
				VisHeight.Value = parameters[psn].VisualizationHeight;
				VisLeft.Value = parameters[psn].VisualizationLeft;
				VisTop.Value = parameters[psn].VisualizationTop;

				SGHeight.Value = parameters[psn].SpectrogramHeight;         // Установка размеров окна определяет максимум SGHeight
				SGTopOffset.Value = parameters[psn].SpectrogramTopOffset;   // Установка SGHeight определяет максимум SGTopOffset

				// Эти параметры теперь тоже сохраняются
				ObjectsMaxSpeedField.Value = parameters[psn].ParticlesMetrics.MaxSpeed;
				ObjectsMinSpeedField.Value = parameters[psn].ParticlesMetrics.MinSpeed;
				ObjectsMaxSizeField.Value = parameters[psn].ParticlesMetrics.MaxSize;
				ObjectsMinSizeField.Value = parameters[psn].ParticlesMetrics.MinSize;
				ObjectsSidesCountField.Value = parameters[psn].ParticlesMetrics.PolygonsSidesCount;

				ObjectsAccelerationField.Value = parameters[psn].ParticlesMetrics.Acceleration;
				ObjectsEnlargingCoeffField.Value = parameters[psn].ParticlesMetrics.Enlarging;

				ObjectsMaxColor.BackColor = Color.FromArgb (parameters[psn].ParticlesMetrics.MaxRed,
					parameters[psn].ParticlesMetrics.MaxGreen, parameters[psn].ParticlesMetrics.MaxBlue);
				ColorPicker_Click (ObjectsMaxColor, null);
				ObjectsMinColor.BackColor = Color.FromArgb (parameters[psn].ParticlesMetrics.MinRed,
					parameters[psn].ParticlesMetrics.MinGreen, parameters[psn].ParticlesMetrics.MinBlue);
				ColorPicker_Click (ObjectsMinColor, null);

				ObjectsCountField.Value = parameters[psn].ParticlesMetrics.ObjectsCount;
				ObjectsTypeCombo.SelectedIndex = (int)parameters[psn].ParticlesMetrics.ObjectsType;
				ObjectsStartupSideCombo.SelectedIndex = (int)parameters[psn].ParticlesMetrics.StartupPosition;
				ObjectsSpeedFluctuationField.Value = parameters[psn].ParticlesMetrics.MaxSpeedFluctuation;
				}
			catch
				{
				req = true;
				}

			// Успешно
			return req;
			}

		// Применение языковой настройки к окну
		private void LocalizeForm ()
			{
			this.Text = ProgramDescription.AssemblyMainName + " – " + Localization.GetText ("CDP_Name");

			GenericTab.Text = Localization.GetText ("MainTabControl_GenericTab");
			Localization.SetControlsText (GenericTab);
			FFTScaleMultiplier_ValueChanged (null, null);

			HistoTab.Text = Localization.GetText ("MainTabControl_HistoTab");
			Localization.SetControlsText (HistoTab);
			HistogramRangeField_ValueChanged (null, null);

			LogoTab.Text = Localization.GetText ("MainTabControl_LogoTab");
			Localization.SetControlsText (LogoTab);
			LogoCenterXTrack_ValueChanged (null, null);

			BeatsTab.Text = Localization.GetText ("MainTabControl_BeatsTab");
			BDLowEdge_ValueChanged (BDLowEdge, null);

			CumulationTab.Text = Localization.GetText ("MainTabControl_CumulationTab");
			Localization.SetControlsText (CumulationTab);
			CESpeed_ValueChanged (null, null);

			ParticlesTab.Text = Localization.GetText ("MainTabControl_ParticlesTab");
			Localization.SetControlsText (ParticlesTab);

			Localization.SetControlsText (this);
			ProfileCombo.Items[DSN] = Localization.GetText ("CDP_ProfileDefault");
			ProfileCombo.Items[SSN] = Localization.GetText ("CDP_ProfileSaved");
			}

		// Контроль наличия доступных устройств
		private void ConcurrentDrawParameters_Load (object sender, EventArgs e)
			{
			// Контроль возможности запуска
			if (!DevicesCombo.Enabled)
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning, "NoCompatibleDevices");
				this.Close ();
				return;
				}

			// Перезапрос настроек (если предыдущие были отменены)
			ProfileCombo.SelectedIndex = SSN;
			if (BCancel.Enabled)
				GetSettings (SSN);

			// Отмена реинициализации, которая выставляется при загрузке (кроме спецпалитр)
			logoResetFlag = ConcurrentDrawLib.PaletteRequiresReset (parameters[SSN].PaletteNumber);

			// Загрузка снимка спектра для бит-детектора
			Bitmap b = new Bitmap (BeatsDetectorImage.Width, BeatsDetectorImage.Height);
			Graphics g = Graphics.FromImage (b);

			for (uint i = 0; i < b.Width; i++)
				{
				Pen p = new Pen (ConcurrentDrawLib.GetColorFromPalette
					(ConcurrentDrawLib.GetScaledAmplitude (i)));
				g.DrawLine (p, i, 0, i, BeatsDetectorImage.Height);
				p.Dispose ();
				}

			g.Dispose ();

			if (BeatsDetectorImage.BackgroundImage != null)
				BeatsDetectorImage.BackgroundImage.Dispose ();
			BeatsDetectorImage.BackgroundImage = b;
			}

		/// <summary>
		/// Возвращает флаг, указывающий на возможность работы программы
		/// </summary>
		public bool HasAvailableDevices
			{
			get
				{
				return DevicesCombo.Enabled;
				}
			}

		// Сохранение настроек
		private void BOK_Click (object sender, EventArgs e)
			{
			// Закрепление настроек и сохранение
			SetSettings (SSN, "");

			// Установка параметров
			ConcurrentDrawLib.SetPeakEvaluationParameters (parameters[SSN].BeatsDetectorLowEdge,
				parameters[SSN].BeatsDetectorHighEdge, parameters[SSN].BeatsDetectorLowLevel,
				parameters[SSN].FFTScaleMultiplier);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[SSN].HistogramRangeMaximum *
				CDParametersSet.HistogramScaledFrequencyMaximum / CDParametersSet.HistogramFrequencyMaximum,
				parameters[SSN].ReverseFreqOrder);

			// Завершение
			BCancel.Enabled = true;
			if (this.Visible)   // Исключает инвалидацию при вызове из обработчика горячих клавиш
				this.Close ();
			}

		private void SetSettings (uint ParametersSetNumber, string ParametersSetName)
			{
			// Контроль
			if (ParametersSetNumber < 1)
				return;
			int psn = (int)ParametersSetNumber;

			// Закрепление настроек
			parameters[psn].DeviceNumber = (byte)DevicesCombo.SelectedIndex;
			parameters[psn].PaletteNumber = (byte)SDPaletteCombo.SelectedIndex;
			parameters[psn].VisualizationMode = VisualizationCombo.SelectedIndex * (WithLogoFlag.Checked ? 1 : -1);
			parameters[psn].SpectrogramHeight = (uint)SGHeight.Value;
			parameters[psn].SpectrogramTopOffset = (uint)SGTopOffset.Value;

			parameters[psn].VisualizationWidth = (uint)VisWidth.Value;
			parameters[psn].VisualizationHeight = (uint)VisHeight.Value;
			parameters[psn].VisualizationLeft = (uint)VisLeft.Value;
			parameters[psn].VisualizationTop = (uint)VisTop.Value;

			parameters[psn].SpectrogramDoubleWidth = SDDoubleWidthFlag.Checked;
			parameters[psn].AlwaysOnTop = AlwaysOnTopFlag.Checked;
			parameters[psn].SwingingHistogram = SwingingHistogramFlag.Checked;
			parameters[psn].HistogramRangeMaximum = histoRange;

			parameters[psn].DecumulationMultiplier = (byte)CEDecumulationMultiplier.Value;
			parameters[psn].CumulationSpeed = (byte)CECumulationSpeed.Value;
			parameters[psn].ExtendedCumulativeEffect = ExtendedCumulation.Checked;

			parameters[psn].LogoHeightPercentage = (byte)LogoHeightPercentage.Value;
			parameters[psn].LogoCenterX = (uint)LogoCenterXTrack.Value;
			parameters[psn].LogoCenterY = (uint)(LogoCenterYTrack.Maximum - LogoCenterYTrack.Value);

			parameters[psn].HistoRotationAccToBeats = HistoRotAccToBeats.Checked;
			parameters[psn].HistoRotationSpeedDelta = (int)(HistoRotSpeedAngle.Value * 10);
			parameters[psn].HistoRotInitialAngle = (uint)HistoRotInitialAngle.Value;

			parameters[psn].ShakeEffect = (uint)ShakeValue.Value;
			parameters[psn].BeatDetectorWaves = BeatWavesFlag.Checked;
			parameters[psn].ReverseFreqOrder = ReverseFreqOrderFlag.Checked;

			parameters[psn].FFTScaleMultiplier = (byte)FFTScaleMultiplier.Value;
			parameters[psn].BeatsDetectorHighEdge = (uint)BDHighEdge.Value;
			parameters[psn].BeatsDetectorLowEdge = (uint)BDLowEdge.Value;
			parameters[psn].BeatsDetectorLowLevel = (byte)BDLowLevel.Value;

			LogoDrawerObjectMetrics ldom = parameters[psn].ParticlesMetrics;

			ldom.MaxSpeed = (uint)ObjectsMaxSpeedField.Value;
			ldom.MinSpeed = (uint)ObjectsMinSpeedField.Value;
			ldom.MaxSize = (uint)ObjectsMaxSizeField.Value;
			ldom.MinSize = (uint)ObjectsMinSizeField.Value;
			ldom.PolygonsSidesCount = (byte)ObjectsSidesCountField.Value;

			ldom.Acceleration = (uint)ObjectsAccelerationField.Value;
			ldom.Enlarging = (int)ObjectsEnlargingCoeffField.Value;
			ldom.MaxRed = ObjectsMaxColor.BackColor.R;
			ldom.MaxGreen = ObjectsMaxColor.BackColor.G;
			ldom.MaxBlue = ObjectsMaxColor.BackColor.B;
			ldom.MinRed = ObjectsMinColor.BackColor.R;
			ldom.MinGreen = ObjectsMinColor.BackColor.G;
			ldom.MinBlue = ObjectsMinColor.BackColor.B;
			ldom.ObjectsCount = (byte)ObjectsCountField.Value;
			ldom.ObjectsType = (LogoDrawerObjectTypes)ObjectsTypeCombo.SelectedIndex;
			ldom.AsStars = ((ldom.ObjectsType == LogoDrawerObjectTypes.RotatingStars) ||
				(ldom.ObjectsType == LogoDrawerObjectTypes.Stars));
			ldom.Rotation = ((ldom.ObjectsType == LogoDrawerObjectTypes.RotatingLetters) ||
				(ldom.ObjectsType == LogoDrawerObjectTypes.RotatingPictures) ||
				(ldom.ObjectsType == LogoDrawerObjectTypes.RotatingPolygons) ||
				(ldom.ObjectsType == LogoDrawerObjectTypes.RotatingStars));
			ldom.StartupPosition = (LogoDrawerObjectStartupPositions)ObjectsStartupSideCombo.SelectedIndex;
			ldom.MaxSpeedFluctuation = (uint)ObjectsSpeedFluctuationField.Value;

			parameters[psn].ParticlesMetrics = LogoDrawerSupport.AlingMetrics (ldom);

			// Сохранение
			if (psn == 1)
				parameters[psn].SaveSettings ();
			else
				parameters[psn].SaveSettings (ParametersSetName);
			}

		// Отмена настройки
		private void BCancel_Click (object sender, EventArgs e)
			{
			// Перерисовка при отмене бессмысленна (кроме спецпалитр)
			logoResetFlag = ConcurrentDrawLib.PaletteRequiresReset (parameters[SSN].PaletteNumber);
			this.Close ();
			}

		/// <summary>
		/// Возвращает флаг, требующий реинициализации лого
		/// </summary>
		public bool ReselLogo
			{
			get
				{
				return logoResetFlag;
				}
			}
		private bool logoResetFlag = false;

		// Метод отображает быструю справку по использованию
		private void BHelp_Click (object sender, EventArgs e)
			{
			ConcurrentDrawLogo cdl = new ConcurrentDrawLogo ();
			cdl.Dispose ();

			RDGenerics.ShowAbout (false);
			}

		// Изменение языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			Localization.CurrentLanguage = (SupportedLanguages)LanguageCombo.SelectedIndex;
			LocalizeForm ();
			}

		// Задание размера и позиции окна визуализации мышью
		private void SpecifyWindowPosition_Click (object sender, EventArgs e)
			{
			// Запрос размеров и позиции
			ScreenShooterForm ssf = new ScreenShooterForm ((uint)VisWidth.Minimum, (uint)VisWidth.Maximum,
				(uint)VisHeight.Minimum, (uint)VisHeight.Maximum);

			// Перенос
			if (ssf.Selected)
				{
				VisWidth.Value = ssf.WindowSize.Width;
				VisHeight.Value = ssf.WindowSize.Height;
				VisLeft.Value = ssf.LeftTopPoint.X;
				VisTop.Value = ssf.LeftTopPoint.Y;
				}
			}

		// Задание размера окна визуализации вариантом из списка
		private void SelectWindowSize_Click (object sender, EventArgs e)
			{
			// Запрос размеров
			WindowSizeForm wsf = new WindowSizeForm ((uint)VisWidth.Maximum, (uint)VisHeight.Maximum);

			// Перенос
			if (wsf.Selected)
				{
				VisWidth.Value = wsf.WindowSize.Width;
				VisHeight.Value = wsf.WindowSize.Height;
				}
			}

		#endregion

		#region Defaults

		/// <summary>
		/// Возвращает номер выбранного устройства
		/// </summary>
		public uint DeviceNumber
			{
			get
				{
				return parameters[SSN].DeviceNumber;
				}
			}

		/// <summary>
		/// Возвращает номер выбранной палитры
		/// </summary>
		public byte PaletteNumber
			{
			get
				{
				return parameters[SSN].PaletteNumber;
				}
			}

		/// <summary>
		/// Возвращает режим визуализации
		/// </summary>
		public VisualizationModes VisualizationMode
			{
			get
				{
				return (VisualizationModes)Math.Abs (parameters[SSN].VisualizationMode);
				}
			}

		// Изменение режима отображения
		private void VisualizationCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			VisualizationModes mode = (VisualizationModes)VisualizationCombo.SelectedIndex;
			WithLogoFlag.Enabled = (mode != VisualizationModes.Logo_only);

			SGHeightLabel.Enabled = SGHeight.Enabled = SGHeightPxLabel.Enabled =
				SGTopOffsetLabel.Enabled = SGTopOffset.Enabled = SGTopOffsetPxLabel.Enabled =
				SGTopOffsetMin.Enabled = SGTopOffsetMid.Enabled = SGTopOffsetMax.Enabled =
				SGHeightMax.Enabled = SGHeightMin.Enabled =
				VisualizationModesChecker.ContainsSGHGorWF (mode);
			HistoRotAccToBeats.Enabled = HistoRotSpeed.Enabled = HistoRotSpeedAngle.Enabled =
				HistoRotSpeedLabel.Enabled = ResetRotation.Enabled = SwingingHistogramFlag.Enabled =
				HistoRotInitialAngle.Enabled = HistoRotInitialAngleLabel.Enabled = HistoRotInitialLabel.Enabled =
				ResetInitialAngle.Enabled = !VisualizationModesChecker.ContainsSGHGorWF (mode);
			BeatWavesFlag.Enabled = VisualizationModesChecker.ContainsSGHGorWF (mode) || (mode == VisualizationModes.Logo_only);

			HGRangeLabel.Enabled = HistoRangeUp.Enabled = HistoRangeDown.Enabled = HzLabel.Enabled = ReverseFreqOrderFlag.Enabled =
				(mode != VisualizationModes.Logo_only);
			SDDoubleWidthFlag.Enabled = VisualizationModesChecker.ContainsSGorWF (mode);

			LogoCenterXTrack.Enabled = LogoCenterYTrack.Enabled =
				LogoCenterXTrack.Visible = LogoCenterYTrack.Visible = !VisualizationModesChecker.IsPerspective (mode);
			LogoCenterXTrack_ValueChanged (null, null);
			BeatWavesFlag.Enabled = WithLogoFlag.Checked;
			}

		/// <summary>
		/// Возвращает флаг наличия в режиме визуализации лого
		/// </summary>
		public bool VisualizationContainsLogo
			{
			get
				{
				return (parameters[SSN].VisualizationMode >= 0);
				}
			}

		/// <summary>
		/// Возвращает ширину окна визуализации
		/// </summary>
		public uint VisualizationWidth
			{
			get
				{
				return parameters[SSN].VisualizationWidth;
				}
			}

		/// <summary>
		/// Возвращает высоту окна визуализации
		/// </summary>
		public uint VisualizationHeight
			{
			get
				{
				return parameters[SSN].VisualizationHeight;
				}
			}

		/// <summary>
		/// Возвращает левый отступ окна визуализации
		/// </summary>
		public uint VisualizationLeft
			{
			get
				{
				return parameters[SSN].VisualizationLeft;
				}
			}

		/// <summary>
		/// Возвращает верхний отступ окна визуализации
		/// </summary>
		public uint VisualizationTop
			{
			get
				{
				return parameters[SSN].VisualizationTop;
				}
			}

		/// <summary>
		/// Возвращает флаг, требующий расположения окна поверх остальных
		/// </summary>
		public bool AlwaysOnTop
			{
			get
				{
				return parameters[SSN].AlwaysOnTop;
				}
			}

		// Выравнивание окна по экрану
		private void AlignTo_Click (object sender, EventArgs e)
			{
			switch (((Button)sender).Name)
				{
				case "AlignToTop":
					VisTop.Value = 0;
					break;

				case "AlignToBottom":
					VisTop.Value = VisHeight.Maximum - VisHeight.Value;
					break;

				case "AlignToLeft":
					VisLeft.Value = 0;
					break;

				case "AlignToRight":
					VisLeft.Value = VisWidth.Maximum - VisWidth.Value;
					break;
				}
			}

		// Установка реинициализации лого при изменении параметров, от которых зависит его вид
		private void SDWindowsSize_Changed (object sender, EventArgs e)
			{
			SGHeight.Maximum = Math.Min (ConcurrentDrawLib.MaxSpectrogramFrameHeight, VisHeight.Value);
			VisLeft.Maximum = VisWidth.Maximum - VisWidth.Value;
			VisTop.Maximum = VisHeight.Maximum - VisHeight.Value;
			SGTopOffset.Maximum = VisHeight.Value - SGHeight.Value;

			logoResetFlag = true;
			}

		// Выбор палитры
		private void SDPaletteCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Предложение реинициализации
			logoResetFlag = true;

			// Сборка палитры
			ConcurrentDrawLib.FillPalette ((byte)SDPaletteCombo.SelectedIndex);

			Bitmap b = new Bitmap (PaletteImageBox.Width, PaletteImageBox.Height);
			Graphics g = Graphics.FromImage (b);

			for (int i = 0; i < b.Width; i++)
				{
				Pen p = new Pen (ConcurrentDrawLib.GetColorFromPalette ((byte)(256 * i / b.Width)));
				g.DrawLine (p, i, 0, i, PaletteImageBox.Height / 2 - 1);
				p.Dispose ();

				p = new Pen (ConcurrentDrawLib.GetMasterPaletteColor ((byte)(256 * i / b.Width)));
				g.DrawLine (p, i, PaletteImageBox.Height / 2, i, PaletteImageBox.Height - 1);
				p.Dispose ();
				}

			g.Dispose ();

			// Отображение палитры
			if (PaletteImageBox.BackgroundImage != null)
				PaletteImageBox.BackgroundImage.Dispose ();
			PaletteImageBox.BackgroundImage = b;
			//b.Save ("C:\\1\\" + SDPaletteCombo.SelectedIndex.ToString ("D02") + ".png", System.Drawing.Imaging.ImageFormat.Png);
			}

		/// <summary>
		/// Возвращает силу тряски
		/// </summary>
		public uint ShakeEffect
			{
			get
				{
				return parameters[SSN].ShakeEffect + 1; // Значение 1 не имеет реального эффекта
				}
			}

		// Изменение множителя значений БПФ
		private void FFTScaleMultiplier_ValueChanged (object sender, EventArgs e)
			{
			FFTScaleLabel.Text = string.Format (Localization.GetText ("CDP_FFTScaleLabel"),
				FFTScaleMultiplier.Value);
			}

		#endregion

		#region Histograms

		/// <summary>
		/// Возвращает высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return parameters[SSN].SpectrogramHeight;
				}
			}

		// Установка размера поля спектро-/гистограммы
		private void SGHeightMax_Click (object sender, EventArgs e)
			{
			SGHeight.Value = SGHeight.Maximum;
			}

		private void SGHeightMin_Click (object sender, EventArgs e)
			{
			SGHeight.Value = SGHeight.Minimum;
			}

		/// <summary>
		/// Возвращает смещение изображения диаграммы от верха окна
		/// </summary>
		public uint SpectrogramTopOffset
			{
			get
				{
				return parameters[SSN].SpectrogramTopOffset;
				}
			}

		// Установка смещения поля спектро-/гистограммы
		private void SGTopOffsetMid_Click (object sender, EventArgs e)
			{
			SGTopOffset.Value = (uint)SGTopOffset.Maximum / 2;
			}

		private void SGTopOffsetMax_Click (object sender, EventArgs e)
			{
			SGTopOffset.Value = SGTopOffset.Maximum;
			}

		private void SGTopOffsetMin_Click (object sender, EventArgs e)
			{
			SGTopOffset.Value = SGTopOffset.Minimum;
			}

		/// <summary>
		/// Возвращает флаг, указывающий на качание гистограммы вместо вращения
		/// </summary>
		public bool SwingingHistogram
			{
			get
				{
				return parameters[SSN].SwingingHistogram;
				}
			}

		/// <summary>
		/// Возвращает количество значений FFT, которые используются при формировании гистограммы
		/// </summary>
		public uint HistogramFFTValuesCount
			{
			get
				{
				return parameters[SSN].HistogramRangeMaximum * CDParametersSet.HistogramScaledFrequencyMaximum /
					CDParametersSet.HistogramFrequencyMaximum;
				}
			}

		// Задание высоты спектрограммы
		private void SGHeight_ValueChanged (object sender, EventArgs e)
			{
			SGTopOffset.Maximum = VisHeight.Value - SGHeight.Value;
			}

		/// <summary>
		/// Возвращает скорость изменения угла поворота гистограммы
		/// </summary>
		public double HistoRotSpeedDelta
			{
			get
				{
				return parameters[SSN].HistoRotationSpeedDelta / 10.0;
				}
			}

		/// <summary>
		/// Возвращает начальный угол поворота гистограммы
		/// </summary>
		public uint HistoRotStartAngle
			{
			get
				{
				return parameters[SSN].HistoRotInitialAngle;
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на режим синхронизации поворота гистограммы с бит-детектором
		/// </summary>
		public bool HistoRotAccordingToBeats
			{
			get
				{
				return parameters[SSN].HistoRotationAccToBeats;
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на двойную ширину спектрограммы
		/// </summary>
		public bool SpectrogramDoubleWidth
			{
			get
				{
				return parameters[SSN].SpectrogramDoubleWidth;
				}
			}

		// Сброс вращения гистограммы
		private void ResetRotation_Click (object sender, EventArgs e)
			{
			HistoRotSpeedAngle.Value = 0;
			}

		// Сброс начального угла поворота гистограммы
		private void ResetInitialAngle_Click (object sender, EventArgs e)
			{
			HistoRotInitialAngle.Value = HistoRotInitialAngle.Minimum;
			}

		// Изменение значения диапазона гистограммы
		private void HistogramRangeField_ValueChanged (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Button b = (Button)sender;
				if ((b.Name == "HistoRangeUp") && (++histoRange > CDParametersSet.HistogramUsedFrequencyMaximum /
					CDParametersSet.HistogramRangeSettingIncrement))
					{
					histoRange = 1;
					}
				else if ((b.Name == "HistoRangeDown") && (--histoRange < 1))
					{
					histoRange = CDParametersSet.HistogramUsedFrequencyMaximum / CDParametersSet.HistogramRangeSettingIncrement;
					}
				}

			HzLabel.Text = string.Format (Localization.GetText ("HistoTab_HzLabelText"),
				CDParametersSet.HistogramRangeSettingIncrement * histoRange);
			}

		#endregion

		#region Logo

		/// <summary>
		/// Возвращает флаг, указывающий на наличие волн бит-детектора
		/// </summary>
		public bool BeatDetectorWaves
			{
			get
				{
				return parameters[SSN].BeatDetectorWaves && BeatWavesFlag.Enabled;
				}
			}

		/// <summary>
		/// Возвращает высоту лого в долях от высоты окна
		/// </summary>
		public double LogoHeight
			{
			get
				{
				return parameters[SSN].LogoHeightPercentage / 100.0;
				}
			}

		/// <summary>
		/// Возвращает относительную абсиссу центра лого
		/// </summary>
		public double LogoCenterX
			{
			get
				{
				if (VisualizationModesChecker.IsPerspective (this.VisualizationMode))
					return 0.5; // Принудительная центровка

				return parameters[SSN].LogoCenterX / 100.0;
				}
			}

		/// <summary>
		/// Возвращает относительную ординату центра лого
		/// </summary>
		public double LogoCenterY
			{
			get
				{
				if (VisualizationModesChecker.IsPerspective (this.VisualizationMode))
					return 0.5; // Принудительная центровка

				return parameters[SSN].LogoCenterY / 100.0;
				}
			}

		// Изменение координат центра лого
		private void LogoCenterXTrack_ValueChanged (object sender, EventArgs e)
			{
			LogoCenterLabel.Text = string.Format (Localization.GetText ("CDP_LogoCenterText"),
				LogoHeightPercentage.Value.ToString (),
				LogoCenterXTrack.Enabled ? (LogoCenterXTrack.Value / 100.0).ToString () : "0.5",
				LogoCenterYTrack.Enabled ? ((LogoCenterYTrack.Maximum - LogoCenterYTrack.Value) /
				100.0).ToString () : "0.5");
			logoResetFlag = true;
			}

		// Выравнивание по центру
		private void LogoCenterButton_Click (object sender, EventArgs e)
			{
			LogoCenterXTrack.Value = LogoCenterXTrack.Maximum / 2;
			LogoCenterYTrack.Value = LogoCenterYTrack.Maximum / 2;
			}

		#endregion

		#region Beats detector

		// Изменение настроек детекции бита
		private void BDLowEdge_ValueChanged (object sender, EventArgs e)
			{
			if ((((TrackBar)sender).Name == "BDHighEdge") && (BDLowEdge.Value > BDHighEdge.Value))
				BDLowEdge.Value = BDHighEdge.Value;
			if ((((TrackBar)sender).Name == "BDLowEdge") && (BDHighEdge.Value < BDLowEdge.Value))
				BDHighEdge.Value = BDLowEdge.Value;

			if ((BDLowEdge.Value == BDHighEdge.Value) && (BDHighEdge.Value == 0))
				BDSettings.Text = Localization.GetText ("CDP_BDNo");
			else
				BDSettings.Text = string.Format (Localization.GetText ("CDP_BDSettingsText"),
					(CDParametersSet.HistogramFrequencyMaximum * BDLowEdge.Value /
					// Используется только первая четверть

					(CDParametersSet.HistogramScaledFrequencyMaximum /
					CDParametersSet.HistogramRangeSettingIncrement)).ToString (),

					(CDParametersSet.HistogramFrequencyMaximum * BDHighEdge.Value /
					(CDParametersSet.HistogramScaledFrequencyMaximum /
					CDParametersSet.HistogramRangeSettingIncrement)).ToString (),

					(100 * BDLowLevel.Value / 255).ToString ());
			}

		/// <summary>
		/// Возвращает порог срабатывания бит-детектора
		/// </summary>
		public byte BeatsDetectorLowLevel
			{
			get
				{
				return parameters[SSN].BeatsDetectorLowLevel;
				}
			}

		#endregion

		#region Cumulation effect

		/// <summary>
		/// Возвращает скорость ослабления кумулятивного эффекта
		/// </summary>
		public uint DecumulationSpeed
			{
			get
				{
				return (uint)parameters[SSN].DecumulationMultiplier * (uint)parameters[SSN].CumulationSpeed /
					(uint)CEDecumulationMultiplier.Maximum;
				}
			}

		/// <summary>
		/// Возвращает скорость накопления кумулятивного эффекта
		/// </summary>
		public uint CumulationSpeed
			{
			get
				{
				return parameters[SSN].CumulationSpeed;
				}
			}

		/// <summary>
		/// Возвращает флаг расширенного кумулятивного эффекта
		/// </summary>
		public bool ExtendedCumulationEffect
			{
			get
				{
				return parameters[SSN].ExtendedCumulativeEffect;
				}
			}

		// Изменение настроек детекции бита
		private void CESpeed_ValueChanged (object sender, EventArgs e)
			{
			if (CEDecumulationMultiplier.Value == CEDecumulationMultiplier.Maximum)
				{
				CESettings.Text = Localization.GetText ("CDP_CENo");
				}
			else
				{
				CESettings.Text = string.Format (Localization.GetText ("CDP_CESettingsText"),
					CECumulationSpeed.Value.ToString (), (CEDecumulationMultiplier.Value /
					(double)CEDecumulationMultiplier.Maximum).ToString ());
				}
			}

		#endregion

		#region Particles

		/// <summary>
		/// Возвращает метрики генерации дополнительных графических объектов
		/// </summary>
		public LogoDrawerObjectMetrics ParticlesMetrics
			{
			get
				{
				return parameters[SSN].ParticlesMetrics;
				}
			}

		/// <summary>
		/// Возвращает флаг, разрешающий отображение дополнительных графических объектов
		/// </summary>
		public bool AllowParticles
			{
			get
				{
				return (parameters[SSN].ParticlesMetrics.ObjectsCount > 0);
				}
			}

		// Выбор цветов объектов
		private void ColorPicker_Click (object sender, EventArgs e)
			{
			Button btn = (Button)sender;

			if (e != null)
				{
				ColorPicker.Color = btn.BackColor;
				ColorPicker.ShowDialog ();
				btn.BackColor = ColorPicker.Color;
				}

			btn.ForeColor = (btn.BackColor.R + btn.BackColor.G + btn.BackColor.B > 128 * 3) ? Color.FromArgb (0, 0, 0) :
				Color.FromArgb (255, 255, 255);
			}

		#endregion

		#region Profiling

		// Выбор профиля
		private void ProfileCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			RemoveProfile.Enabled = ((ProfileCombo.SelectedIndex != DSN) && (ProfileCombo.SelectedIndex != SSN));
			}

		// Запрос настроек из профиля
		private void ApplyProfile_Click (object sender, EventArgs e)
			{
			GetSettings ((ProfileCombo.SelectedIndex < 0) ? SSN : (uint)ProfileCombo.SelectedIndex);
			}

		// Сохранение набора настроек
		private void AddProfile_Click (object sender, EventArgs e)
			{
			// Контроль
			if (ProfileCombo.Items.Contains (ProfileCombo.Text) || (ProfileCombo.Text == ""))
				{
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Warning, "CDP_ProfileError");
				return;
				}

			// Создание профиля
			parameters.Add (new CDParametersSet (true));
			ProfileCombo.Items.Add (ProfileCombo.Text);

			// Заполнение профиля
			SetSettings ((uint)parameters.Count - 1, ProfileCombo.Text);
			ProfileCombo.SelectedIndex = parameters.Count - 1;
			}

		// Удаление набора настроек
		private void RemoveProfile_Click (object sender, EventArgs e)
			{
			// Контроль
			if (ProfileCombo.SelectedIndex <= SSN)
				return;

			if (RDGenerics.LocalizedMessageBox (RDMessageTypes.Question, "CDP_ProfileRemove",
				LzDefaultTextValues.Button_YesNoFocus, LzDefaultTextValues.Button_No) == RDMessageButtons.ButtonTwo)
				return;

			// Удаление
			CDParametersSet.RemoveSettings (ProfileCombo.Items[ProfileCombo.SelectedIndex].ToString ());
			parameters.RemoveAt (ProfileCombo.SelectedIndex);
			ProfileCombo.Items.RemoveAt (ProfileCombo.SelectedIndex);
			ProfileCombo.SelectedIndex = SSN;
			}

		#endregion

		#region Keyboard

		/// <summary>
		/// Метод проверяет, доступен ли для указанной клавиши обработчик в окне параметров
		/// </summary>
		/// <param name="HotKey">Горячая клавиша</param>
		/// <returns>Возвращает true, если доступен</returns>
		public static bool IsHotKeyAllowed (Keys HotKey)
			{
			return allowedHotKeys.Contains (HotKey);
			}

		// Список доступных клавиш
		private static List<Keys> allowedHotKeys = new List<Keys>
			{
			Keys.M,
			Keys.P,
			Keys.H,						// 2

			Keys.Up,
			Keys.Down,
			Keys.Left,
			Keys.Right,					// 6

			Keys.Up | Keys.Shift,
			Keys.Down | Keys.Shift,
			Keys.Left | Keys.Shift,
			Keys.Right | Keys.Shift,	// 10

			Keys.T,
			Keys.K,						// 12

			Keys.Up | Keys.Control,
			Keys.Down | Keys.Control,
			Keys.Left | Keys.Control,
			Keys.Right | Keys.Control,	// 16
			
			Keys.Up | Keys.Control | Keys.Shift,
			Keys.Down | Keys.Control | Keys.Shift,
			Keys.Left | Keys.Control | Keys.Shift,
			Keys.Right | Keys.Control | Keys.Shift,	// 20
			
			Keys.C | Keys.Shift,
			Keys.OemQuestion,
			Keys.I,							// 23

			Keys.Oemcomma,
			Keys.OemPeriod,
			Keys.Oemcomma | Keys.Shift,
			Keys.OemPeriod | Keys.Shift,	// 27

			Keys.M | Keys.Shift,
			Keys.P | Keys.Shift,
			Keys.H | Keys.Shift,			// 30

			Keys.W,
			Keys.L,
			Keys.V,							// 33

			Keys.PageUp | Keys.Shift,
			Keys.PageDown | Keys.Shift,		// 35

			Keys.C,
			Keys.PageUp,
			Keys.PageDown,					// 38

			Keys.K | Keys.Shift,
			Keys.E,
			Keys.Tab,
			Keys.F,							// 42

			Keys.F13,
			Keys.F14						// 44

			// Клавиши, обрабатываемые в основном интерфейсе
			// Keys.R,
			// Keys.S,
			// Keys.Escape,
			// Keys.Space,
		};

		/// <summary>
		/// Метод обрабатывает нажатие горячей клавиши на главном экране
		/// </summary>
		/// <param name="HotKey">Горячая клавиша</param>
		/// <returns>Результат вызова горячей клавиши</returns>
		public string ProcessHotKey (Keys HotKey)
			{
			// Переменные
			string hotKeyResult = "";

			// Контроль
			if (!IsHotKeyAllowed (HotKey))
				return hotKeyResult;

			// Отмена реинициализации, которая выставляется при загрузке (кроме спецпалитр)
			logoResetFlag = ConcurrentDrawLib.PaletteRequiresReset (parameters[SSN].PaletteNumber);

			// Обработка клавиш
			int i = allowedHotKeys.IndexOf (HotKey);
			switch (i)
				{
				#region Смена режима
				case 0:
				case 28:
					if (i == 0)
						{
						if (VisualizationCombo.SelectedIndex == VisualizationCombo.Items.Count - 1)
							VisualizationCombo.SelectedIndex = 0;
						else
							VisualizationCombo.SelectedIndex++;
						}
					else
						{
						if (VisualizationCombo.SelectedIndex == 0)
							VisualizationCombo.SelectedIndex = VisualizationCombo.Items.Count - 1;
						else
							VisualizationCombo.SelectedIndex--;
						}

					hotKeyResult = VisTypeLabel.Text + " " + VisualizationCombo.Text;
					break;
				#endregion

				#region Смена палитры
				case 1:
				case 29:
					if (i == 1)
						{
						if (SDPaletteCombo.SelectedIndex == SDPaletteCombo.Items.Count - 1)
							SDPaletteCombo.SelectedIndex = 0;
						else
							SDPaletteCombo.SelectedIndex++;
						}
					else
						{
						if (SDPaletteCombo.SelectedIndex == 0)
							SDPaletteCombo.SelectedIndex = SDPaletteCombo.Items.Count - 1;
						else
							SDPaletteCombo.SelectedIndex--;
						}

					hotKeyResult = PaletteLabel.Text + " " + SDPaletteCombo.Text;
					break;
				#endregion

				#region Изменение диапазона гистограмм
				case 2:
				case 30:
					if (i == 2)
						{
						if (++histoRange > CDParametersSet.HistogramUsedFrequencyMaximum /
										CDParametersSet.HistogramRangeSettingIncrement)
							histoRange = 1;
						}
					else
						{
						if (--histoRange < 1)
							histoRange = CDParametersSet.HistogramUsedFrequencyMaximum / CDParametersSet.HistogramRangeSettingIncrement;
						}

					hotKeyResult = HGRangeLabel.Text + " 0 – " + (histoRange *
						CDParametersSet.HistogramRangeSettingIncrement).ToString () + " " +
						HzLabel.Text.Substring (HzLabel.Text.Length - 2);
					break;
				#endregion

				#region Изменение расположения окна
				case 3:
				case 13:
				case 4:
				case 14:
				case 5:
				case 15:
				case 6:
				case 16:
					switch (i)
						{
						case 3:
							if (VisTop.Value != VisTop.Minimum)
								VisTop.Value--;
							break;

						case 13:
							VisTop.Value = VisTop.Minimum;
							break;

						case 4:
							if (VisTop.Value != VisTop.Maximum)
								VisTop.Value++;
							break;

						case 14:
							VisTop.Value = VisTop.Maximum;
							break;

						case 5:
							if (VisLeft.Value != VisLeft.Minimum)
								VisLeft.Value--;
							break;

						case 15:
							VisLeft.Value = VisLeft.Minimum;
							break;

						case 6:
							if (VisLeft.Value != VisLeft.Maximum)
								VisLeft.Value++;
							break;

						case 16:
							VisLeft.Value = VisLeft.Maximum;
							break;
						}

					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;
				#endregion

				#region Изменение расположения лого
				case 7:
				case 17:
				case 8:
				case 18:
				case 9:
				case 19:
				case 10:
				case 20:
					switch (i)
						{
						case 7:
							if (LogoCenterYTrack.Value != LogoCenterYTrack.Maximum)
								LogoCenterYTrack.Value++;
							break;

						case 17:
							LogoCenterYTrack.Value = LogoCenterYTrack.Maximum;
							break;

						case 8:
							if (LogoCenterYTrack.Value != LogoCenterYTrack.Minimum)
								LogoCenterYTrack.Value--;
							break;

						case 18:
							LogoCenterYTrack.Value = LogoCenterYTrack.Minimum;
							break;

						case 9:
							if (LogoCenterXTrack.Value != LogoCenterXTrack.Minimum)
								LogoCenterXTrack.Value--;
							break;

						case 19:
							LogoCenterXTrack.Value = LogoCenterXTrack.Minimum;
							break;

						case 10:
							if (LogoCenterXTrack.Value != LogoCenterXTrack.Maximum)
								LogoCenterXTrack.Value++;
							break;

						case 20:
							LogoCenterXTrack.Value = LogoCenterXTrack.Maximum;
							break;
						}

					hotKeyResult = LogoCenterLabel.Text;
					break;
				#endregion

				// Изменение значения силы тряски
				case 12:
				case 39:
					if (i == 12)
						{
						if (ShakeValue.Value < ShakeValue.Maximum)
							ShakeValue.Value++;
						else
							ShakeValue.Value = ShakeValue.Minimum;
						}
					else
						{
						if (ShakeValue.Value > ShakeValue.Minimum)
							ShakeValue.Value--;
						else
							ShakeValue.Value = ShakeValue.Maximum;
						}
					hotKeyResult = ShakeLabel.Text + " " + ShakeValue.Value.ToString ();
					break;

				// Выравнивание лого по центру
				case 21:
					LogoCenterButton_Click (null, null);
					hotKeyResult = LogoCenterLabel.Text;
					break;

				// Вызов справки
				case 22:
					BHelp_Click (null, null);
					// Установка hotKeyResult не требуется
					break;

				#region Запрос всех настроек
				case 23:
					RDGenerics.MessageBox (RDMessageTypes.Information,
						DevicesLabel.Text + " " + DevicesCombo.Text + "\n" +
						VisTypeLabel.Text + " " + VisualizationCombo.Text +
						(WithLogoFlag.Checked ? (" + " + WithLogoFlag.Text + "\n") : "\n") +
						VisSizeLabel.Text + " " + VisWidth.Value.ToString () + " x " +
							VisHeight.Value.ToString () + " px\n" +
						VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
							VisTop.Value.ToString () + " px\n" +
						PaletteLabel.Text + " " + SDPaletteCombo.Text + "\n" +
						ShakeLabel.Text + " " + ShakeValue.Value.ToString () + "\n" +
						(AlwaysOnTopFlag.Checked ? (AlwaysOnTopFlag.Text + "\n") : "") +
						FFTScaleLabel.Text + "\n\n" +

						HGRangeLabel.Text +
							(ReverseFreqOrderFlag.Checked ? " " : " 0 – ") +
							(histoRange * CDParametersSet.HistogramRangeSettingIncrement).ToString () +
							(ReverseFreqOrderFlag.Checked ? " – 0 " : " ") +
							HzLabel.Text.Substring (HzLabel.Text.Length - 2) + "\n" +
						SGHeightLabel.Text + " " + SGHeight.Value.ToString () + " px" +
						(SDDoubleWidthFlag.Checked ? ("; " + SDDoubleWidthFlag.Text) : "") + "\n" +
						SGTopOffsetLabel.Text + " " + SGTopOffset.Value.ToString () + " px\n" +
						(HistoRotAccToBeats.Checked ? HistoRotAccToBeats.Text : HistoRotSpeed.Text) + " " +
						HistoRotSpeedAngle.Value.ToString () + "°\n" +
						(SwingingHistogramFlag.Checked ? (SwingingHistogramFlag.Text + "\n") : "") +

						"\n" + LogoCenterLabel.Text +
						(BeatWavesFlag.Checked ? ("\n" + BeatWavesFlag.Text) : "") + "\n\n" +

						BDSettings.Text + "\n\n" +

						CESettings.Text);
					break;
				#endregion

				#region Увеличение / уменьшение скорости вращения гистограммы
				case 24:
				case 25:
				case 26:
				case 27:
					if (i < 26)
						{
						HistoRotSpeed.Checked = true;
						hotKeyResult = HistoRotSpeed.Text;
						}
					else
						{
						HistoRotAccToBeats.Checked = true;
						hotKeyResult = HistoRotAccToBeats.Text;
						}

					if (i % 2 == 0)
						{
						if (HistoRotSpeedAngle.Value > HistoRotSpeedAngle.Minimum)
							HistoRotSpeedAngle.Value -= HistoRotSpeedAngle.Increment;
						}
					else
						{
						if (HistoRotSpeedAngle.Value < HistoRotSpeedAngle.Maximum)
							HistoRotSpeedAngle.Value += HistoRotSpeedAngle.Increment;
						}

					hotKeyResult += (" " + HistoRotSpeedAngle.Value.ToString () + "°");
					break;
				#endregion

				#region Флаги

				// Изменение флага Always on top
				case 11:
					AlwaysOnTopFlag.Checked = !AlwaysOnTopFlag.Checked;
					hotKeyResult = AlwaysOnTopFlag.Text + " = " + (AlwaysOnTopFlag.Checked ? "1" : "0");
					break;

				// Изменение флага Swinging histogram
				case 31:
					SwingingHistogramFlag.Checked = !SwingingHistogramFlag.Checked;
					hotKeyResult = SwingingHistogramFlag.Text + " = " + (SwingingHistogramFlag.Checked ? "1" : "0");
					break;

				// Изменение флага With logo
				case 32:
					WithLogoFlag.Checked = !WithLogoFlag.Checked;
					hotKeyResult = WithLogoFlag.Text + " = " + (WithLogoFlag.Checked ? "1" : "0");
					break;

				// Изменение флага Beatwaves
				case 33:
					BeatWavesFlag.Checked = !BeatWavesFlag.Checked;
					hotKeyResult = BeatWavesFlag.Text + " = " + (BeatWavesFlag.Checked ? "1" : "0");
					break;

				// Изменение флага Reverse freq order
				case 42:
					ReverseFreqOrderFlag.Checked = !ReverseFreqOrderFlag.Checked;
					hotKeyResult = ReverseFreqOrderFlag.Text + " = " + (ReverseFreqOrderFlag.Checked ? "1" : "0");
					break;

				// Переключение флага расширенного кумулятивного эффекта
				case 40:
					ExtendedCumulation.Checked = !ExtendedCumulation.Checked;
					hotKeyResult = ExtendedCumulation.Text + " = " + (ExtendedCumulation.Checked ? "1" : "0");
					break;

				// Запрос текущего значения накопителя кумулятивного эффекта
				case 41:
					hotKeyResult = "!CC = 000000";  // Обрабатывается в основном интерфейсе
					break;

				#endregion

				#region Изменение размера лого
				case 34:
				case 35:
					if (i == 34)
						{
						if (LogoHeightPercentage.Value < LogoHeightPercentage.Maximum)
							LogoHeightPercentage.Value++;
						}
					else
						{
						if (LogoHeightPercentage.Value > LogoHeightPercentage.Minimum)
							LogoHeightPercentage.Value--;
						}

					hotKeyResult = LogoCenterLabel.Text;
					break;
				#endregion

				// Выравниваие гистограммы по центру
				case 36:
					SGTopOffsetMid_Click (null, null);
					hotKeyResult = HistoTab.Text + ": " + SGTopOffsetLabel.Text + " " +
						SGTopOffset.Value.ToString () + SGTopOffsetPxLabel.Text;
					break;

				#region Изменение размера поля гистограммы

				case 37:
				case 38:
					if (i == 37)
						{
						if (SGHeight.Value < SGHeight.Maximum)
							SGHeight.Value++;
						}
					else
						{
						if (SGHeight.Value > SGHeight.Minimum)
							SGHeight.Value--;
						}

					hotKeyResult = HistoTab.Text + ": " + SGHeightLabel.Text + " " +
						SGHeight.Value.ToString () + SGHeightPxLabel.Text;
					break;

				#endregion

				#region Изменение чувствительности БПФ (псевдоклавиши, подаются из события мыши)

				case 43:
				case 44:
					if (i == 43)
						{
						if (FFTScaleMultiplier.Value < FFTScaleMultiplier.Maximum)
							FFTScaleMultiplier.Value += 2;
						}
					else
						{
						if (FFTScaleMultiplier.Value > FFTScaleMultiplier.Minimum)
							FFTScaleMultiplier.Value -= 2;
						}
					hotKeyResult = FFTScaleLabel.Text;
					break;

					#endregion
				}

			// Применение новой настройки
			BOK_Click (null, null);
			return hotKeyResult;
			}

		#endregion
		}
	}
