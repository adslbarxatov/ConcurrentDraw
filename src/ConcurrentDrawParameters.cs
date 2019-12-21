using Microsoft.Win32;
using System;
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
		private char[] splitter = new char[] { ';' };
		private SupportedLanguages al = Localization.CurrentLanguage;

		// Название параметра с настройками
		private const string SettingsValueName = "";

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

			// Настройка контролов

			// Устройства
			DevicesCombo.Items.AddRange (ConcurrentDrawLib.AvailableDevices);
			if (DevicesCombo.Items.Count < 1)
				{
				DevicesCombo.Items.Add (Localization.GetText ("CDP_NoDevices", al));
				DevicesCombo.Enabled = DevicesLabel.Enabled = false;
				}
			DevicesCombo.SelectedIndex = 0;			// По умолчанию - первое
			deviceNumber = 0;

			// Палитра
			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = 0;		// По умолчанию - первая
			paletteNumber = 0;

			// Режим
			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				{
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ().Replace ('_', ' '));
				}
			visualizationMode = VisualizationModes.Butterfly_histogram;		// По умолчанию - бабочка
			VisualizationCombo.SelectedIndex = (int)visualizationMode;

			// Высота спектрограммы
			SDHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SDHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;
			SDHeight.Value = 256;					// По умолчанию - 256 px
			sdHeight = (uint)SDHeight.Value;

			// Размеры визуализации 
			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = Math.Min (ScreenHeight, 1024);

			VisWidth.Value = (int)(9 * VisWidth.Maximum / 16);
			VisHeight.Value = (int)(9 * VisHeight.Maximum / 16);	// По умолчанию - (9 / 16) размера экрана

			visualizationWidth = (uint)VisWidth.Value;
			visualizationHeight = (uint)VisHeight.Value;

			// Позиция визуализации
			VisLeft.Maximum = ScreenWidth;
			VisLeft.Value = ScreenWidth - VisWidth.Value;	// По умолчанию - верхняя правая четверть экрана
			VisTop.Maximum = ScreenHeight;

			visualizationLeft = (uint)VisLeft.Value;
			visualizationTop = (uint)VisTop.Value;

			// Параметры детектора битов (получаются из DLL)
			BDLowEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowEdge;			// По умолчанию - 0 - 86 Hz, peak = 97%, FFTm = 40
			BDHighEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationHighEdge;
			BDLowLevel.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowLevel;
			BDFFTScaleMultiplier.Value = ConcurrentDrawLib.DefaultFFTScaleMultiplier;

			// Плотность гистограммы
			for (int i = 1; i <= 32; i *= 2)
				{
				HistogramRangeCombo.Items.Add ("0 – " + (i * 22050.0 / 32.0).ToString () +
					" " + Localization.GetText ("CDP_Hz", al));
				}
			histogramFFTValuesCountShift = HistogramRangeCombo.SelectedIndex = 2;			// По умолчанию - до 2,7 кГц

			// Кумулятивный эффект
			CEDecumulationMultiplier.Value = 8;									// По умолчанию - 0,8
			decumulationMultiplier = (uint)CEDecumulationMultiplier.Value;
			CECumulationSpeed.Value = 70;										// По умолчанию - 70
			cumulationSpeed = (uint)CECumulationSpeed.Value;
			LogoHeightPercentage.Value = 30;									// По умолчанию - 30%
			logoHeight = (uint)LogoHeightPercentage.Value;

			// Скорость вращения гистограммы
			histoRotSpeedArc = 0;							// По умолчанию - без вращения
			HistoRotSpeedArc.Value = 0;
			HistoRotSpeed.Checked = true;

			// Язык интерфейса
			for (int i = 0; i < Localization.AvailableLanguages; i++)
				LanguageCombo.Items.Add (((SupportedLanguages)i).ToString ());
			LanguageCombo.SelectedIndex = (int)al;			// По умолчанию - язык системы или английский

			// Запрос настроек
			bool requestRequired = GetSavedSettings ();

			// Установка настроек
			ConcurrentDrawLib.SetPeakEvaluationParameters ((byte)BDLowEdge.Value, (byte)BDHighEdge.Value,
				(byte)BDLowLevel.Value, (byte)BDFFTScaleMultiplier.Value);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (this.HistogramFFTValuesCount);

			// Запуск окна немедленно, ести требуется
			BCancel.Enabled = !requestRequired;
			if (requestRequired)
				this.ShowDialog ();
			}

		// Метод получает настройки из реестра; возвращает true, если настройки требуется ввести вручную
		private bool GetSavedSettings ()
			{
			string settings = "";
			try
				{
				settings = Registry.GetValue (ProgramDescription.AssemblySettingsKey, SettingsValueName, "").ToString ();
				}
			catch
				{
				}
			if (settings == "")
				{
				BHelp_Click (null, null);	// Справка на случай первого запуска
				return true;
				}

			// Разбор сохранённых настроек
			string[] values = settings.Split (splitter, System.StringSplitOptions.RemoveEmptyEntries);

			try
				{
				DevicesCombo.SelectedIndex = int.Parse (values[0]);
				deviceNumber = (uint)DevicesCombo.SelectedIndex;

				SDPaletteCombo.SelectedIndex = int.Parse (values[1]);
				paletteNumber = (byte)SDPaletteCombo.SelectedIndex;

				VisualizationCombo.SelectedIndex = int.Parse (values[2]);
				visualizationMode = (VisualizationModes)VisualizationCombo.SelectedIndex;

				VisWidth.Value = decimal.Parse (values[4]);
				visualizationWidth = (uint)VisWidth.Value;

				VisHeight.Value = decimal.Parse (values[5]);
				visualizationHeight = (uint)VisHeight.Value;

				VisLeft.Value = decimal.Parse (values[6]);
				visualizationLeft = (uint)VisLeft.Value;

				VisTop.Value = decimal.Parse (values[7]);
				visualizationTop = (uint)VisTop.Value;

				SDHeight.Value = decimal.Parse (values[3]);		// Установка размеров окна определяет максимум SDHeight
				sdHeight = (uint)SDHeight.Value;

				uint bdSettings = uint.Parse (values[8]);
				BDLowEdge.Value = (int)(bdSettings & 0xFF);
				BDHighEdge.Value = (int)((bdSettings >> 8) & 0xFF);
				BDLowLevel.Value = (int)((bdSettings >> 16) & 0xFF);
				BDFFTScaleMultiplier.Value = (int)((bdSettings >> 24) & 0xFF);

				alwaysOnTop = AlwaysOnTopFlag.Checked = (values[9] != "0");
				histogramFFTValuesCountShift = HistogramRangeCombo.SelectedIndex = int.Parse (values[10]);

				CEDecumulationMultiplier.Value = int.Parse (values[11]);
				decumulationMultiplier = (uint)CEDecumulationMultiplier.Value;
				CECumulationSpeed.Value = int.Parse (values[12]);
				cumulationSpeed = (uint)CECumulationSpeed.Value;
				LogoHeightPercentage.Value = int.Parse (values[13]);
				logoHeight = (uint)LogoHeightPercentage.Value;

				histoRotSpeedArc = int.Parse (values[14]);
				if (histoRotSpeedArc < 0)
					HistoRotAccToBeats.Checked = true;
				else
					HistoRotSpeed.Checked = true;
				HistoRotSpeedArc.Value = (decimal)Math.Abs (histoRotSpeedArc / 10.0);

				transparentLogo = TransparentFlag.Checked = (values[15] != "0");
				shakingBitDetector = ShakeFlag.Checked = (values[16] != "0");
				}
			catch
				{
				return true;
				}

			// Успешно
			return false;
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
			LogoHeightLabel.Text = Localization.GetText ("CDP_LogoHeightLabel", al);

			AlwaysOnTopFlag.Text = Localization.GetText ("CDP_AlwaysOnTopFlag", al);

			BeatsGroup.Text = Localization.GetText ("CDP_BeatsGroup", al);

			BOK.Text = Localization.GetText ("CDP_OK", al);
			BCancel.Text = Localization.GetText ("CDP_Cancel", al);
			LanguageLabel.Text = Localization.GetText ("CDP_LanguageLabel", al);

			BDLowEdge_ValueChanged (BDLowEdge, null);

			CumulationGroup.Text = Localization.GetText ("CDP_CumulationGroup", al);

			CESpeed_ValueChanged (null, null);

			HistoRotGroup.Text = Localization.GetText ("CDP_HistoRotGroup", al);
			HistoRotAccToBeats.Text = Localization.GetText ("CDP_HistoRotAccToBeats", al);
			HistoRotSpeed.Text = Localization.GetText ("CDP_HistoRotSpeed", al);

			TransparentFlag.Text = Localization.GetText ("CDP_TransparentFlag", al);
			ShakeFlag.Text = Localization.GetText ("CDP_ShakeFlag", al);
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
			if (BCancel.Enabled)
				GetSavedSettings ();

			// Отмена реинициализации, которая выставляется при загрузке
			logoResetFlag = (SDPaletteCombo.SelectedIndex >= 10) && (SDPaletteCombo.SelectedIndex <= 13);
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
		/// Номер выбранного устройства
		/// </summary>
		public uint DeviceNumber
			{
			get
				{
				return deviceNumber;
				}
			}
		private uint deviceNumber;

		/// <summary>
		/// Номер выбранной палитры
		/// </summary>
		public byte PaletteNumber
			{
			get
				{
				return paletteNumber;
				}
			}
		private byte paletteNumber;

		/// <summary>
		/// Возвращает флаг, указывающий вариант спектрограммы
		/// </summary>
		public VisualizationModes VisualizationMode
			{
			get
				{
				return visualizationMode;
				}
			}
		private VisualizationModes visualizationMode;

		/// <summary>
		/// Возвращает выбранную высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return sdHeight;
				}
			}
		private uint sdHeight;

		// Сохранение настроек
		private void BOK_Click (object sender, System.EventArgs e)
			{
			// Закрепление настроек
			deviceNumber = (uint)DevicesCombo.SelectedIndex;
			paletteNumber = (byte)SDPaletteCombo.SelectedIndex;
			visualizationMode = (VisualizationModes)VisualizationCombo.SelectedIndex;
			sdHeight = (uint)SDHeight.Value;

			visualizationWidth = (uint)VisWidth.Value;
			visualizationHeight = (uint)VisHeight.Value;
			visualizationLeft = (uint)VisLeft.Value;
			visualizationTop = (uint)VisTop.Value;

			alwaysOnTop = AlwaysOnTopFlag.Checked;
			histogramFFTValuesCountShift = HistogramRangeCombo.SelectedIndex;

			decumulationMultiplier = (uint)CEDecumulationMultiplier.Value;
			cumulationSpeed = (uint)CECumulationSpeed.Value;
			logoHeight = (uint)LogoHeightPercentage.Value;

			if (HistoRotAccToBeats.Checked)
				histoRotSpeedArc = (int)(-HistoRotSpeedArc.Value * 10);
			else
				histoRotSpeedArc = (int)(HistoRotSpeedArc.Value * 10);

			transparentLogo = TransparentFlag.Checked && TransparentFlag.Enabled;
			shakingBitDetector = ShakeFlag.Checked;

			// Сохранение
			string settings = deviceNumber.ToString () + splitter[0].ToString () +
				paletteNumber.ToString () + splitter[0].ToString () +
				((uint)visualizationMode).ToString () + splitter[0].ToString () +
				sdHeight.ToString () + splitter[0].ToString () +
				visualizationWidth.ToString () + splitter[0].ToString () +
				visualizationHeight.ToString () + splitter[0].ToString () +
				visualizationLeft.ToString () + splitter[0].ToString () +
				visualizationTop.ToString () + splitter[0].ToString () +

				(((BDFFTScaleMultiplier.Value & 0xFF) << 24) | ((BDLowLevel.Value & 0xFF) << 16) |
				((BDHighEdge.Value & 0xFF) << 8) | (BDLowEdge.Value & 0xFF)).ToString () + splitter[0].ToString () +

				(alwaysOnTop ? "1" : "0") + splitter[0].ToString () +
				histogramFFTValuesCountShift.ToString () + splitter[0].ToString () +
				decumulationMultiplier.ToString () + splitter[0].ToString () +
				cumulationSpeed.ToString () + splitter[0].ToString () +
				logoHeight.ToString () + splitter[0].ToString () +
				histoRotSpeedArc.ToString () + splitter[0].ToString () +

				(transparentLogo ? "1" : "0") + splitter[0].ToString () +
				(shakingBitDetector ? "1" : "0");

			try
				{
				Registry.SetValue (ProgramDescription.AssemblySettingsKey, SettingsValueName, settings);
				}
			catch
				{
				}

			// Установка параметров
			ConcurrentDrawLib.SetPeakEvaluationParameters ((byte)BDLowEdge.Value, (byte)BDHighEdge.Value,
				(byte)BDLowLevel.Value, (byte)BDFFTScaleMultiplier.Value);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (this.HistogramFFTValuesCount);

			// Завершение
			BCancel.Enabled = true;
			this.Close ();
			}

		// Отмена настройки
		private void BCancel_Click (object sender, System.EventArgs e)
			{
			logoResetFlag = false;	// Перерисовка при отмене бессмысленна
			this.Close ();
			}

		/// <summary>
		/// Возвращает ширину окна визуализации
		/// </summary>
		public uint VisualizationWidth
			{
			get
				{
				return visualizationWidth;
				}
			}
		private uint visualizationWidth;

		/// <summary>
		/// Возвращает высоту окна визуализации
		/// </summary>
		public uint VisualizationHeight
			{
			get
				{
				return visualizationHeight;
				}
			}
		private uint visualizationHeight;

		/// <summary>
		/// Возвращает левый отступ окна визуализации
		/// </summary>
		public uint VisualizationLeft
			{
			get
				{
				return visualizationLeft;
				}
			}
		private uint visualizationLeft;

		/// <summary>
		/// Возвращает верхний отступ окна визуализации
		/// </summary>
		public uint VisualizationTop
			{
			get
				{
				return visualizationTop;
				}
			}
		private uint visualizationTop;

		/// <summary>
		/// Возвращает флаг, требующий расположения окна поверх остальных
		/// </summary>
		public bool AlwaysOnTop
			{
			get
				{
				return alwaysOnTop;
				}
			}
		private bool alwaysOnTop = false;

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
				return 32u << histogramFFTValuesCountShift;
				}
			}
		private int histogramFFTValuesCountShift;

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
			WindowSizeForm wsf = new WindowSizeForm (al);

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
		/// Скорость ослабления кумулятивного эффекта
		/// </summary>
		public uint DecumulationSpeed
			{
			get
				{
				return decumulationMultiplier * cumulationSpeed / (uint)CEDecumulationMultiplier.Maximum;
				}
			}
		private uint decumulationMultiplier;

		/// <summary>
		/// Скорость накопления кумулятивного эффекта
		/// </summary>
		public uint CumulationSpeed
			{
			get
				{
				return cumulationSpeed;
				}
			}
		private uint cumulationSpeed;

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
			CumulationGroup.Enabled = HistoRotGroup.Enabled = !VisualizationModesChecker.ContainsSGHGorWF (mode);

			HGRangeLabel.Enabled = HistogramRangeCombo.Enabled = !VisualizationModesChecker.ContainsSGonly (mode);

			TransparentFlag.Enabled = !VisualizationModesChecker.ContainsSGHGorWF (mode);
			TransparentFlag.Checked &= TransparentFlag.Enabled;
			}

		/// <summary>
		/// Высота лого в процентах от высоты окна
		/// </summary>
		public double LogoHeight
			{
			get
				{
				return logoHeight / 100.0;
				}
			}
		private uint logoHeight;

		/// <summary>
		/// Возвращает скорость изменения угла поворота гистограммы
		/// </summary>
		public double HistoRotSpeedDelta
			{
			get
				{
				return Math.Abs (histoRotSpeedArc / 10.0);
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на режим синхронизации поворота гистограммы с бит-детектором
		/// </summary>
		public bool HistoRotAccordingToBeats
			{
			get
				{
				return (histoRotSpeedArc < 0);
				}
			}
		private int histoRotSpeedArc;

		/// <summary>
		/// Возвращает флаг, указывающий на прозрачность лого
		/// </summary>
		public bool TransparentLogo
			{
			get
				{
				return transparentLogo && TransparentFlag.Enabled;
				}
			}
		private bool transparentLogo = false;

		/// <summary>
		/// Возвращает флаг, указывающий на дребезг бит-детектора
		/// </summary>
		public bool ShakingBitDetector
			{
			get
				{
				return shakingBitDetector;
				}
			}
		private bool shakingBitDetector = false;
		}
	}
