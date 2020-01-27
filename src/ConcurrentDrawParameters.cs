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
			VisHeight.Maximum = Math.Min (ScreenHeight, 1024);

			VisWidth.Value = (int)(9 * VisWidth.Maximum / 16);
			parameters[DSN].VisualizationWidth = (uint)VisWidth.Value;
			VisHeight.Value = (int)(9 * VisHeight.Maximum / 16);	// По умолчанию - (9 / 16) размера экрана
			parameters[DSN].VisualizationHeight = (uint)VisHeight.Value;

			// Позиция визуализации
			VisLeft.Maximum = ScreenWidth;
			VisLeft.Value = ScreenWidth - VisWidth.Value;	// По умолчанию - верхняя правая четверть экрана
			parameters[DSN].VisualizationLeft = (uint)VisLeft.Value;
			VisTop.Maximum = ScreenHeight;

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

			SDDoubleWidthFlag.Text = Localization.GetText ("CDP_SDDoubleWidthFlag", al);
			AlwaysOnTopFlag.Text = Localization.GetText ("CDP_AlwaysOnTopFlag", al);

			GenericTab.Text = Localization.GetText ("CDP_GenericGroup", al);
			BeatsTab.Text = Localization.GetText ("CDP_BeatsGroup", al);

			BOK.Text = Localization.GetText ("CDP_OK", al);
			BCancel.Text = Localization.GetText ("CDP_Cancel", al);
			LanguageLabel.Text = Localization.GetText ("CDP_LanguageLabel", al);

			BDLowEdge_ValueChanged (BDLowEdge, null);

			CumulationTab.Text = Localization.GetText ("CDP_CumulationGroup", al);

			CESpeed_ValueChanged (null, null);

			RotationTab.Text = Localization.GetText ("CDP_HistoRotGroup", al);
			HistoRotAccToBeats.Text = Localization.GetText ("CDP_HistoRotAccToBeats", al);
			HistoRotSpeed.Text = Localization.GetText ("CDP_HistoRotSpeed", al);

			ShakeFlag.Text = Localization.GetText ("CDP_ShakeFlag", al);

			ProfileLabel.Text = Localization.GetText ("CDP_ProfileLabel", al);
			ProfileCombo.Items[DSN] = Localization.GetText ("CDP_ProfileDefault", al);
			ProfileCombo.Items[SSN] = Localization.GetText ("CDP_ProfileSaved", al);
			}

		// Контроль наличия доступных устройств
		private void ConcurrentDrawParameters_Load (object sender, System.EventArgs e)
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
		private void BOK_Click (object sender, System.EventArgs e)
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
		private void BCancel_Click (object sender, System.EventArgs e)
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
			WindowSizeForm wsf = new WindowSizeForm ((uint)VisLeft.Maximum, (uint)VisTop.Maximum, al);

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
			if (CEDecumulationMultiplier.Value == 0)
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
			CumulationTab.Enabled = RotationTab.Enabled = !VisualizationModesChecker.ContainsSGHGorWF (mode);

			HGRangeLabel.Enabled = HistogramRangeCombo.Enabled = !VisualizationModesChecker.ContainsSGonly (mode);
			SDDoubleWidthFlag.Enabled = VisualizationModesChecker.ContainsSGorWF (mode);
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
		}
	}
