using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Класс описывает форму доступа к параметрам программы
	/// </summary>
	public partial class ConcurrentDrawParameters:Form
		{
		// Константы и переменные
		private SupportedLanguages al = Localization.CurrentLanguage;				// Текущий язык интерфейса
		private List<CDParametersSet> parameters = new List<CDParametersSet> ();	// Наборы сохранённых параметров

		private const int defaultSettingsNumber = 0;
		private const int savedSettingsNumber = 1;
		private const int DSN = defaultSettingsNumber;
		private const int SSN = savedSettingsNumber;

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
				DevicesCombo.Items.Add (Localization.GetText ("CDP_NoDevices", al));
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
			VisualizationCombo.SelectedIndex = (int)parameters[DSN].VisualizationMode;

			// Высота спектрограммы
			SDHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SDHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;
			SDHeight.Value = parameters[DSN].SpectrogramHeight;

			// Размеры визуализации 
			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = ScreenHeight;

			VisWidth.Value = (int)(9 * VisWidth.Maximum / 16);
			parameters[DSN].VisualizationWidth = (uint)VisWidth.Value;
			VisHeight.Value = (int)(9 * VisHeight.Maximum / 16);	// По умолчанию - (9 / 16) размера экрана
			parameters[DSN].VisualizationHeight = (uint)VisHeight.Value;

			// Позиция визуализации
			VisLeft.Value = ScreenWidth - VisWidth.Value;	// По умолчанию - верхняя правая четверть экрана
			parameters[DSN].VisualizationLeft = (uint)VisLeft.Value;
			// Максимумы теперь зависят от размеров окна визуализации; задаются в соответствующем обработчике

			// Параметры детектора битов (получаются из DLL)
			BDLowEdge.Value = parameters[DSN].BeatsDetectorLowEdge;
			BDHighEdge.Value = parameters[DSN].BeatsDetectorHighEdge;
			BDLowLevel.Value = parameters[DSN].BeatsDetectorLowLevel;
			BDFFTScaleMultiplier.Value = parameters[DSN].BeatsDetectorFFTScaleMultiplier;

			// Плотность гистограммы
			for (int i = 1; i <= CDParametersSet.HistogramFFTValuesCountMinimum; i *= 2)
				{
				HistogramRangeCombo.Items.Add ("0 – " +
					(i * 22050.0 / (double)CDParametersSet.HistogramFFTValuesCountMinimum).ToString ());
				}
			HistogramRangeCombo.SelectedIndex = (int)Math.Log (parameters[DSN].HistogramFFTValuesCount /
				CDParametersSet.HistogramFFTValuesCountMinimum, 2.0);

			// Кумулятивный эффект
			CEDecumulationMultiplier.Maximum = (int)CDParametersSet.DecumulationMultiplierMaximum;
			CEDecumulationMultiplier.Value = parameters[DSN].DecumulationMultiplier;

			CECumulationSpeed.Value = parameters[DSN].CumulationSpeed;
			LogoHeightPercentage.Value = parameters[DSN].LogoHeightPercentage;

			// Скорость вращения гистограммы
			HistoRotSpeedArc.Value = parameters[DSN].HistoRotSpeedDelta;
			HistoRotSpeed.Checked = true;

			// Флаги
			AlwaysOnTopFlag.Checked = parameters[DSN].AlwaysOnTop;
			ShakeFlag.Checked = parameters[DSN].ShakeEffect;
			SDDoubleWidthFlag.Checked = parameters[DSN].SpectrogramDoubleWidth;

			// Язык интерфейса
			for (int i = 0; i < Localization.AvailableLanguages; i++)
				LanguageCombo.Items.Add (((SupportedLanguages)i).ToString ());
			LanguageCombo.SelectedIndex = (int)al;			// По умолчанию - язык системы или английский

			// Запрос настроек
			bool requestRequired = GetSettings (SSN);

			// Установка настроек
			ConcurrentDrawLib.SetPeakEvaluationParameters (parameters[SSN].BeatsDetectorLowEdge,
				parameters[SSN].BeatsDetectorHighEdge, parameters[SSN].BeatsDetectorLowLevel,
#if VIDEO
 (byte)(3 * parameters[SSN].BeatsDetectorFFTScaleMultiplier / 4));
#else
 parameters[SSN].BeatsDetectorFFTScaleMultiplier);
#endif
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[SSN].HistogramFFTValuesCount);

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
					BHelp_Click (null, null);	// Справка на случай первого запуска
					req = true;
					}
				}

			// Разбор сохранённых настроек
			try
				{
				DevicesCombo.SelectedIndex = parameters[psn].DeviceNumber;
				SDPaletteCombo.SelectedIndex = parameters[psn].PaletteNumber;

				if ((uint)parameters[psn].VisualizationMode >= VisualizationModesChecker.VisualizationModesCount)
					parameters[psn].VisualizationMode = VisualizationModes.Butterfly_histogram;
				VisualizationCombo.SelectedIndex = (int)parameters[psn].VisualizationMode;

				VisWidth.Value = parameters[psn].VisualizationWidth;
				VisHeight.Value = parameters[psn].VisualizationHeight;
				VisLeft.Value = parameters[psn].VisualizationLeft;
				VisTop.Value = parameters[psn].VisualizationTop;

				SDHeight.Value = parameters[psn].SpectrogramHeight;	// Установка размеров окна определяет максимум SDHeight

				BDLowEdge.Value = parameters[psn].BeatsDetectorLowEdge;
				BDHighEdge.Value = parameters[psn].BeatsDetectorHighEdge;
				BDLowLevel.Value = parameters[psn].BeatsDetectorLowLevel;
				BDFFTScaleMultiplier.Value = parameters[psn].BeatsDetectorFFTScaleMultiplier;

				SDDoubleWidthFlag.Checked = parameters[psn].SpectrogramDoubleWidth;
				AlwaysOnTopFlag.Checked = parameters[psn].AlwaysOnTop;
				HistogramRangeCombo.SelectedIndex = (int)Math.Log (parameters[psn].HistogramFFTValuesCount /
					CDParametersSet.HistogramFFTValuesCountMinimum, 2.0);

				CEDecumulationMultiplier.Value = parameters[psn].DecumulationMultiplier;
				CECumulationSpeed.Value = parameters[psn].CumulationSpeed;

				LogoHeightPercentage.Value = parameters[psn].LogoHeightPercentage;
				LogoCenterXTrack.Value = (int)parameters[psn].LogoCenterX;
				LogoCenterYTrack.Value = (int)(LogoCenterYTrack.Maximum - parameters[psn].LogoCenterY);

				if (parameters[psn].HistoRotSpeedDelta < 0)
					HistoRotAccToBeats.Checked = true;
				else
					HistoRotSpeed.Checked = true;
				HistoRotSpeedArc.Value = (decimal)Math.Abs (parameters[psn].HistoRotSpeedDelta / 10.0);

				ShakeFlag.Checked = parameters[psn].ShakeEffect;
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
			this.Text = ProgramDescription.AssemblyMainName + " – " + Localization.GetText ("CDPName", al);

			DevicesLabel.Text = Localization.GetText ("CDP_DevicesLabel", al);

			VisTypeLabel.Text = Localization.GetText ("CDP_VisTypeLabel", al);
			VisSizeLabel.Text = Localization.GetText ("CDP_VisSizeLabel", al);
			VisLeftTopLabel.Text = Localization.GetText ("CDP_VisLeftTopLabel", al);

			PaletteLabel.Text = Localization.GetText ("CDP_PaletteLabel", al);
			SGHGHeightLabel.Text = Localization.GetText ("CDP_SGHGHeightLabel", al);
			HGRangeLabel.Text = Localization.GetText ("CDP_HGRangeLabel", al);
			HzLabel.Text = Localization.GetText ("CDP_Hz", al);
			LogoHeightLabel.Text = Localization.GetText ("CDP_LogoHeightLabel", al);
			LogoCenterButton.Text = Localization.GetText ("CDP_LogoCenterButton", al);
			LogoInfoLabel.Text = Localization.GetText ("CDP_LogoInfoText", al);

			SDDoubleWidthFlag.Text = Localization.GetText ("CDP_SDDoubleWidthFlag", al);
			AlwaysOnTopFlag.Text = Localization.GetText ("CDP_AlwaysOnTopFlag", al);

			GenericTab.Text = Localization.GetText ("CDP_GenericGroup", al);
			LogoTab.Text = Localization.GetText ("CDP_LogoGroup", al);
			BeatsTab.Text = Localization.GetText ("CDP_BeatsGroup", al);
			RotationTab.Text = Localization.GetText ("CDP_HistoRotGroup", al);
			CumulationTab.Text = Localization.GetText ("CDP_CumulationGroup", al);

			BOK.Text = Localization.GetText ("CDP_OK", al);
			BCancel.Text = Localization.GetText ("CDP_Cancel", al);
			LanguageLabel.Text = Localization.GetText ("CDP_LanguageLabel", al);

			BDLowEdge_ValueChanged (BDLowEdge, null);
			CESpeed_ValueChanged (null, null);
			LogoCenterXTrack_ValueChanged (null, null);

			HistoRotAccToBeats.Text = Localization.GetText ("CDP_HistoRotAccToBeats", al);
			HistoRotSpeed.Text = Localization.GetText ("CDP_HistoRotSpeed", al);

			ShakeFlag.Text = Localization.GetText ("CDP_ShakeFlag", al);

			CEInfo.Text = Localization.GetText ("CDP_CEInfoText", al);

			ProfileLabel.Text = Localization.GetText ("CDP_ProfileLabel", al);
			ProfileCombo.Items[DSN] = Localization.GetText ("CDP_ProfileDefault", al);
			ProfileCombo.Items[SSN] = Localization.GetText ("CDP_ProfileSaved", al);
			}

		// Контроль наличия доступных устройств
		private void ConcurrentDrawParameters_Load (object sender, EventArgs e)
			{
			// Контроль возможности запуска
			if (!DevicesCombo.Enabled)
				{
				MessageBox.Show (Localization.GetText ("NoCompatibleDevices", al), ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Close ();
				return;
				}

			// Перезапрос настроек (если предыдущие были отменены)
			ProfileCombo.SelectedIndex = SSN;
			if (BCancel.Enabled)
				GetSettings (SSN);

			// Отмена реинициализации, которая выставляется при загрузке (кроме спецпалитр)
			logoResetFlag = ConcurrentDrawLib.PaletteRequiresReset (parameters[SSN].PaletteNumber);
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
				return parameters[SSN].VisualizationMode;
				}
			}

		/// <summary>
		/// Возвращает выбранную высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return parameters[SSN].SpectrogramHeight;
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
				parameters[SSN].BeatsDetectorFFTScaleMultiplier);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[SSN].HistogramFFTValuesCount);

			// Завершение
			BCancel.Enabled = true;
			if (this.Visible)	// Исключает инвалидацию при вызове из обработчика горячих клавиш
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
			parameters[psn].VisualizationMode = (VisualizationModes)VisualizationCombo.SelectedIndex;
			parameters[psn].SpectrogramHeight = (uint)SDHeight.Value;

			parameters[psn].VisualizationWidth = (uint)VisWidth.Value;
			parameters[psn].VisualizationHeight = (uint)VisHeight.Value;
			parameters[psn].VisualizationLeft = (uint)VisLeft.Value;
			parameters[psn].VisualizationTop = (uint)VisTop.Value;

			parameters[psn].SpectrogramDoubleWidth = SDDoubleWidthFlag.Checked;
			parameters[psn].AlwaysOnTop = AlwaysOnTopFlag.Checked;
			parameters[psn].HistogramFFTValuesCount = (uint)(Math.Pow (2.0, HistogramRangeCombo.SelectedIndex) *
				CDParametersSet.HistogramFFTValuesCountMinimum);

			parameters[psn].DecumulationMultiplier = (byte)CEDecumulationMultiplier.Value;
			parameters[psn].CumulationSpeed = (byte)CECumulationSpeed.Value;
			parameters[psn].LogoHeightPercentage = (byte)LogoHeightPercentage.Value;
			parameters[psn].LogoCenterX = (uint)LogoCenterXTrack.Value;
			parameters[psn].LogoCenterY = (uint)(LogoCenterYTrack.Maximum - LogoCenterYTrack.Value);

			if (HistoRotAccToBeats.Checked)
				parameters[psn].HistoRotSpeedDelta = (int)(-HistoRotSpeedArc.Value * 10);
			else
				parameters[psn].HistoRotSpeedDelta = (int)(HistoRotSpeedArc.Value * 10);

			parameters[psn].ShakeEffect = ShakeFlag.Checked;

			parameters[psn].BeatsDetectorFFTScaleMultiplier = (byte)BDFFTScaleMultiplier.Value;
			parameters[psn].BeatsDetectorHighEdge = (byte)BDHighEdge.Value;
			parameters[psn].BeatsDetectorLowEdge = (byte)BDLowEdge.Value;
			parameters[psn].BeatsDetectorLowLevel = (byte)BDLowLevel.Value;

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

		// Изменение настроек детекции бита
		private void BDLowEdge_ValueChanged (object sender, EventArgs e)
			{
			if ((((TrackBar)sender).Name == "BDHighEdge") && (BDLowEdge.Value > BDHighEdge.Value))
				BDLowEdge.Value = BDHighEdge.Value;
			if ((((TrackBar)sender).Name == "BDLowEdge") && (BDHighEdge.Value < BDLowEdge.Value))
				BDHighEdge.Value = BDLowEdge.Value;

			BDSettings.Text = string.Format (Localization.GetText ("CDP_BDSettingsText", al),
				(44100 * BDLowEdge.Value / 2048).ToString (),
				(44100 * BDHighEdge.Value / 2048).ToString (),
				(100 * BDLowLevel.Value / 255).ToString (), BDFFTScaleMultiplier.Value.ToString ());
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
					VisTop.Value = VisTop.Maximum - VisHeight.Value;
					break;

				case "AlignToLeft":
					VisLeft.Value = 0;
					break;

				case "AlignToRight":
					VisLeft.Value = VisLeft.Maximum - VisWidth.Value;
					break;
				}

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

		// Установка реинициализации лого при изменении параметров, от которых зависит его вид
		private void SDWindowsSize_Changed (object sender, EventArgs e)
			{
			SDHeight.Maximum = Math.Min (ConcurrentDrawLib.MaxSpectrogramFrameHeight, VisHeight.Value);
			VisLeft.Maximum = VisWidth.Maximum - VisWidth.Value;
			VisTop.Maximum = VisHeight.Maximum - VisHeight.Value;

			logoResetFlag = true;
			}

		/// <summary>
		/// Возвращает количество значений FFT, которые используются при формировании гистограммы
		/// </summary>
		public uint HistogramFFTValuesCount
			{
			get
				{
				return parameters[SSN].HistogramFFTValuesCount;
				}
			}

		// Метод отображает быструю справку по использованию
		private void BHelp_Click (object sender, EventArgs e)
			{
			ConcurrentDrawLogo cdl = new ConcurrentDrawLogo ();
			cdl.Dispose ();

			MessageBox.Show (Localization.GetText ("HelpText", al), ProgramDescription.AssemblyTitle,
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			MessageBox.Show (Localization.GetText ("HelpKeysText", al), ProgramDescription.AssemblyTitle,
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

		// Изменение языка интерфейса
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			Localization.CurrentLanguage = al = (SupportedLanguages)LanguageCombo.SelectedIndex;
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
			WindowSizeForm wsf = new WindowSizeForm ((uint)VisWidth.Maximum, (uint)VisHeight.Maximum, al);

			// Перенос
			if (wsf.Selected)
				{
				VisWidth.Value = wsf.WindowSize.Width;
				VisHeight.Value = wsf.WindowSize.Height;
				}
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
			}

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

		// Изменение настроек детекции бита
		private void CESpeed_ValueChanged (object sender, EventArgs e)
			{
			if (CEDecumulationMultiplier.Value == CEDecumulationMultiplier.Maximum)
				{
				CESettings.Text = Localization.GetText ("CDP_CENo", al);
				}
			else
				{
				CESettings.Text = string.Format (Localization.GetText ("CDP_CESettingsText", al),
					CECumulationSpeed.Value.ToString (), (CEDecumulationMultiplier.Value /
					(double)CEDecumulationMultiplier.Maximum).ToString ());
				}
			}

		// Изменение режима отображения
		private void VisualizationCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			VisualizationModes mode = (VisualizationModes)VisualizationCombo.SelectedIndex;

			SGHGHeightLabel.Enabled = SDHeight.Enabled = VisualizationModesChecker.ContainsSGHGorWF (mode);
			RotationTab.Enabled = !VisualizationModesChecker.ContainsSGHGorWF (mode);

			HGRangeLabel.Enabled = HistogramRangeCombo.Enabled = !VisualizationModesChecker.ContainsSGonly (mode);
			SDDoubleWidthFlag.Enabled = VisualizationModesChecker.ContainsSGorWF (mode);

			LogoHeightPercentage.Enabled = LogoCenterXTrack.Enabled = LogoCenterYTrack.Enabled =
				LogoCenterLabel.Enabled = VisualizationModesChecker.ContainsLogo (mode);

			LogoCenterXTrack.Enabled = LogoCenterYTrack.Enabled =
				LogoCenterXTrack.Visible = LogoCenterYTrack.Visible = !VisualizationModesChecker.IsPerspective (mode);
			LogoCenterXTrack_ValueChanged (null, null);
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
				if (VisualizationModesChecker.IsPerspective (parameters[SSN].VisualizationMode))
					return 0.5;	// Принудительная центровка

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
				if (VisualizationModesChecker.IsPerspective (parameters[SSN].VisualizationMode))
					return 0.5;	// Принудительная центровка

				return parameters[SSN].LogoCenterY / 100.0;
				}
			}

		/// <summary>
		/// Возвращает скорость изменения угла поворота гистограммы
		/// </summary>
		public double HistoRotSpeedDelta
			{
			get
				{
				return Math.Abs (parameters[SSN].HistoRotSpeedDelta / 10.0);
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на режим синхронизации поворота гистограммы с бит-детектором
		/// </summary>
		public bool HistoRotAccordingToBeats
			{
			get
				{
				return (parameters[SSN].HistoRotSpeedDelta < 0);
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на эффект тряски
		/// </summary>
		public bool ShakeEffect
			{
			get
				{
				return parameters[SSN].ShakeEffect;
				}
			}

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
				MessageBox.Show (Localization.GetText ("CDP_ProfileError", al), ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

			if (MessageBox.Show (Localization.GetText ("CDP_ProfileRemove", al), ProgramDescription.AssemblyTitle,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
				return;

			// Удаление
			CDParametersSet.RemoveSettings (ProfileCombo.Items[ProfileCombo.SelectedIndex].ToString ());
			parameters.RemoveAt (ProfileCombo.SelectedIndex);
			ProfileCombo.Items.RemoveAt (ProfileCombo.SelectedIndex);
			ProfileCombo.SelectedIndex = SSN;
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

		// Изменение координат центра лого
		private void LogoCenterXTrack_ValueChanged (object sender, EventArgs e)
			{
			LogoCenterLabel.Text = string.Format (Localization.GetText ("CDP_LogoCenterText", al),
				LogoHeightPercentage.Value.ToString (),
				LogoCenterXTrack.Enabled ? (LogoCenterXTrack.Value / 100.0).ToString () : "0.5",
				LogoCenterYTrack.Enabled ? ((LogoCenterYTrack.Maximum - LogoCenterYTrack.Value) / 100.0).ToString () : "0.5");
			logoResetFlag = true;
			}

		// Выравнивание по центру
		private void LogoCenterButton_Click (object sender, EventArgs e)
			{
			LogoCenterXTrack.Value = LogoCenterXTrack.Maximum / 2;
			LogoCenterYTrack.Value = LogoCenterYTrack.Maximum / 2;
			}

		/// <summary>
		/// Возвращает текущий язык интерфейса
		/// </summary>
		public SupportedLanguages CurrentInterfaceLanguage
			{
			get
				{
				return al;
				}
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
			Keys.H,	// 2

			Keys.Up,
			Keys.Down,
			Keys.Left,
			Keys.Right,	// 6

			Keys.S,
			Keys.W,
			Keys.A,
			Keys.D,	// 10

			Keys.T,
			Keys.K,	// 12

			Keys.F17,
			Keys.F18,
			Keys.F19,
			Keys.F20,	// 16

			Keys.F21,
			Keys.F22,
			Keys.F23,
			Keys.F24,	// 20

			Keys.C,
			Keys.OemQuestion,
			Keys.I
		};

		/// <summary>
		/// Метод преобразует некоторые сочетания клавиш в специальные коды
		/// </summary>
		public static Keys AdaptHotKey (Keys OldKey, Keys Modifiers)
			{
			if (Modifiers == Keys.Control)
				{
				switch (OldKey)
					{
					// Для стрелок
					case Keys.Up:
						return Keys.F17;

					case Keys.Down:
						return Keys.F18;

					case Keys.Left:
						return Keys.F19;

					case Keys.Right:
						return Keys.F20;

					// Для WASD
					case Keys.S:
						return Keys.F21;

					case Keys.W:
						return Keys.F22;

					case Keys.A:
						return Keys.F23;

					case Keys.D:
						return Keys.F24;
					}
				}

			// Все остальные случаи
			return OldKey;
			}


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
			switch (allowedHotKeys.IndexOf (HotKey))
				{
				// Смена режима
				case 0:
					if (VisualizationCombo.SelectedIndex == VisualizationCombo.Items.Count - 1)
						VisualizationCombo.SelectedIndex = 0;
					else
						VisualizationCombo.SelectedIndex++;
					hotKeyResult = VisTypeLabel.Text + " " + VisualizationCombo.Text;
					break;

				// Смена палитры
				case 1:
					if (SDPaletteCombo.SelectedIndex == SDPaletteCombo.Items.Count - 1)
						SDPaletteCombo.SelectedIndex = 0;
					else
						SDPaletteCombo.SelectedIndex++;
					hotKeyResult = PaletteLabel.Text + " " + SDPaletteCombo.Text;
					break;

				// Изменение диапазона гистограмм
				case 2:
					if (HistogramRangeCombo.SelectedIndex == HistogramRangeCombo.Items.Count - 1)
						HistogramRangeCombo.SelectedIndex = 0;
					else
						HistogramRangeCombo.SelectedIndex++;
					hotKeyResult = HGRangeLabel.Text + " " + HistogramRangeCombo.Text + " " + HzLabel.Text;
					break;

				// Изменение расположения окна
				case 3:
					if (VisTop.Value != VisTop.Minimum)
						VisTop.Value--;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 13:
					VisTop.Value = VisTop.Minimum;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 4:
					if (VisTop.Value != VisTop.Maximum)
						VisTop.Value++;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 14:
					VisTop.Value = VisTop.Maximum;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 5:
					if (VisLeft.Value != VisLeft.Minimum)
						VisLeft.Value--;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 15:
					VisLeft.Value = VisLeft.Minimum;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 6:
					if (VisLeft.Value != VisLeft.Maximum)
						VisLeft.Value++;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				case 16:
					VisLeft.Value = VisLeft.Maximum;
					hotKeyResult = VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " +
						VisTop.Value.ToString () + " px";
					break;

				// Изменение расположения лого
				case 7:
					if (LogoCenterYTrack.Value != LogoCenterYTrack.Minimum)
						LogoCenterYTrack.Value--;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 17:
					LogoCenterYTrack.Value = LogoCenterYTrack.Minimum;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 8:
					if (LogoCenterYTrack.Value != LogoCenterYTrack.Maximum)
						LogoCenterYTrack.Value++;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 18:
					LogoCenterYTrack.Value = LogoCenterYTrack.Maximum;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 9:
					if (LogoCenterXTrack.Value != LogoCenterXTrack.Minimum)
						LogoCenterXTrack.Value--;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 19:
					LogoCenterXTrack.Value = LogoCenterXTrack.Minimum;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 10:
					if (LogoCenterXTrack.Value != LogoCenterXTrack.Maximum)
						LogoCenterXTrack.Value++;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				case 20:
					LogoCenterXTrack.Value = LogoCenterXTrack.Maximum;
					hotKeyResult = LogoCenterLabel.Text;
					break;

				// Изменение флага Always on top
				case 11:
					AlwaysOnTopFlag.Checked = !AlwaysOnTopFlag.Checked;
					hotKeyResult = AlwaysOnTopFlag.Text + " = " + (AlwaysOnTopFlag.Checked ? "1" : "0");
					break;

				// Изменение флага Shake
				case 12:
					ShakeFlag.Checked = !ShakeFlag.Checked;
					hotKeyResult = ShakeFlag.Text + " = " + (ShakeFlag.Checked ? "1" : "0");
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

				// Запрос всех настроек
				case 23:
					MessageBox.Show (DevicesLabel.Text + " " + DevicesCombo.Text + "\n" +
						VisTypeLabel.Text + " " + VisualizationCombo.Text + "\n" +
						VisSizeLabel.Text + " " + VisWidth.Value.ToString () + " x " + VisHeight.Value.ToString () + " px\n" +
						VisLeftTopLabel.Text + " " + VisLeft.Value.ToString () + " x " + VisTop.Value.ToString () + " px\n" +
						PaletteLabel.Text + " " + SDPaletteCombo.Text + "\n" +
						SGHGHeightLabel.Text + " " + SDHeight.Value.ToString () + " px" +
						(SDDoubleWidthFlag.Checked ? ("; " + SDDoubleWidthFlag.Text) : "") + "\n" +
						HGRangeLabel.Text + " " + HistogramRangeCombo.Text + " " + HzLabel.Text + "\n" +
						(ShakeFlag.Checked ? (ShakeFlag.Text + "\n") : "") +
						(AlwaysOnTopFlag.Checked ? (AlwaysOnTopFlag.Text + "\n") : "") +
						"\n" + LogoCenterLabel.Text + "\n\n" +
						BDSettings.Text + "\n\n" +
						(HistoRotAccToBeats.Checked ? HistoRotAccToBeats.Text : HistoRotSpeed.Text) + " " +
						HistoRotSpeedArc.Value.ToString () + "°\n\n" +
						CESettings.Text,
						ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
					break;
				}

			// Применение новой настройки
			BOK_Click (null, null);
			return hotKeyResult;
			}
		}
	}
