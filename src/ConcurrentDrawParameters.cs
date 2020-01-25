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
		private SupportedLanguages al = Localization.CurrentLanguage;
		private List<CDParametersSet> parameters = new List<CDParametersSet> ();

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

			// Настройка контролов

			// Устройства
			DevicesCombo.Items.AddRange (ConcurrentDrawLib.AvailableDevices);
			if (DevicesCombo.Items.Count < 1)
				{
				DevicesCombo.Items.Add (Localization.GetText ("CDP_NoDevices", al));
				DevicesCombo.Enabled = DevicesLabel.Enabled = false;
				}
			DevicesCombo.SelectedIndex = parameters[0].DeviceNumber;

			// Палитра
			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = parameters[0].PaletteNumber;

			// Режим
			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				{
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ().Replace ('_', ' '));
				}
			VisualizationCombo.SelectedIndex = (int)parameters[0].VisualizationMode;

			// Высота спектрограммы
			SDHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SDHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;
			SDHeight.Value = parameters[0].SpectrogramHeight;

			// Размеры визуализации 
			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = Math.Min (ScreenHeight, 1024);

			VisWidth.Value = (int)(9 * VisWidth.Maximum / 16);
			VisHeight.Value = (int)(9 * VisHeight.Maximum / 16);	// По умолчанию - (9 / 16) размера экрана

			// Позиция визуализации
			VisLeft.Maximum = ScreenWidth;
			VisLeft.Value = ScreenWidth - VisWidth.Value;	// По умолчанию - верхняя правая четверть экрана
			VisTop.Maximum = ScreenHeight;

			// Параметры детектора битов (получаются из DLL)
			BDLowEdge.Value = parameters[0].BeatsDetectorLowEdge;
			BDHighEdge.Value = parameters[0].BeatsDetectorHighEdge;
			BDLowLevel.Value = parameters[0].BeatsDetectorLowLevel;
			BDFFTScaleMultiplier.Value = parameters[0].BeatsDetectorFFTScaleMultiplier;

			// Плотность гистограммы
			for (int i = 1; i <= CDParametersSet.HistogramFFTValuesCountMinimum; i *= 2)
				{
				HistogramRangeCombo.Items.Add ("0 – " +
					(i * 22050.0 / (double)CDParametersSet.HistogramFFTValuesCountMinimum).ToString () +
					" " + Localization.GetText ("CDP_Hz", al));
				}
			HistogramRangeCombo.SelectedIndex = (int)Math.Log (parameters[0].HistogramFFTValuesCount /
				CDParametersSet.HistogramFFTValuesCountMinimum, 2.0);

			// Кумулятивный эффект
			CEDecumulationMultiplier.Maximum = (int)CDParametersSet.DecumulationMultiplierMaximum;
			CEDecumulationMultiplier.Value = parameters[0].DecumulationMultiplier;

			CECumulationSpeed.Value = parameters[0].CumulationSpeed;
			LogoHeightPercentage.Value = parameters[0].LogoHeightPercentage;

			// Скорость вращения гистограммы
			HistoRotSpeedArc.Value = parameters[0].HistoRotSpeedDelta;
			HistoRotSpeed.Checked = true;

			// Флаги
			AlwaysOnTopFlag.Checked = parameters[0].AlwaysOnTop;
			ShakeFlag.Checked = parameters[0].ShakeEffect;

			// Язык интерфейса
			for (int i = 0; i < Localization.AvailableLanguages; i++)
				LanguageCombo.Items.Add (((SupportedLanguages)i).ToString ());
			LanguageCombo.SelectedIndex = (int)al;			// По умолчанию - язык системы или английский

			// Запрос настроек
			bool requestRequired = GetSavedSettings ();

			// Установка настроек
			ConcurrentDrawLib.SetPeakEvaluationParameters (parameters[1].BeatsDetectorLowEdge,
				parameters[1].BeatsDetectorHighEdge, parameters[1].BeatsDetectorLowLevel,
				parameters[1].BeatsDetectorFFTScaleMultiplier);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[1].HistogramFFTValuesCount);

			// Запуск окна немедленно, ести требуется
			BCancel.Enabled = !requestRequired;
			if (requestRequired)
				this.ShowDialog ();
			}

		// Метод получает настройки из реестра; возвращает true, если настройки требуется ввести вручную
		private bool GetSavedSettings ()
			{
			// Переменные
			bool req = false;

			// Запрос сохранённых параметров
			parameters[1] = new CDParametersSet (false);
			if (parameters[1].InitFailure)
				{
				BHelp_Click (null, null);	// Справка на случай первого запуска
				req = true;
				}

			// Разбор сохранённых настроек
			try
				{
				DevicesCombo.SelectedIndex = parameters[1].DeviceNumber;
				SDPaletteCombo.SelectedIndex = parameters[1].PaletteNumber;

				if ((uint)parameters[1].VisualizationMode >= VisualizationModesChecker.VisualizationModesCount)
					parameters[1].VisualizationMode = VisualizationModes.Butterfly_histogram;
				VisualizationCombo.SelectedIndex = (int)parameters[1].VisualizationMode;

				VisWidth.Value = parameters[1].VisualizationWidth;
				VisHeight.Value = parameters[1].VisualizationHeight;
				VisLeft.Value = parameters[1].VisualizationLeft;
				VisTop.Value = parameters[1].VisualizationTop;

				SDHeight.Value = parameters[1].SpectrogramHeight;		// Установка размеров окна определяет максимум SDHeight

				BDLowEdge.Value = parameters[1].BeatsDetectorLowEdge;
				BDHighEdge.Value = parameters[1].BeatsDetectorHighEdge;
				BDLowLevel.Value = parameters[1].BeatsDetectorLowLevel;
				BDFFTScaleMultiplier.Value = parameters[1].BeatsDetectorFFTScaleMultiplier;

				AlwaysOnTopFlag.Checked = parameters[1].AlwaysOnTop;
				HistogramRangeCombo.SelectedIndex = (int)Math.Log (parameters[1].HistogramFFTValuesCount /
					CDParametersSet.HistogramFFTValuesCountMinimum, 2.0);

				CEDecumulationMultiplier.Value = parameters[1].DecumulationMultiplier;
				CECumulationSpeed.Value = parameters[1].CumulationSpeed;
				LogoHeightPercentage.Value = parameters[1].LogoHeightPercentage;

				if (parameters[1].HistoRotSpeedDelta < 0)
					HistoRotAccToBeats.Checked = true;
				else
					HistoRotSpeed.Checked = true;
				HistoRotSpeedArc.Value = (decimal)Math.Abs (parameters[1].HistoRotSpeedDelta / 10.0);

				ShakeFlag.Checked = parameters[1].ShakeEffect;
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
			LogoHeightLabel.Text = Localization.GetText ("CDP_LogoHeightLabel", al);

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
		/// Возвращает номер выбранного устройства
		/// </summary>
		public uint DeviceNumber
			{
			get
				{
				return parameters[1].DeviceNumber;
				}
			}

		/// <summary>
		/// Возвращает номер выбранной палитры
		/// </summary>
		public byte PaletteNumber
			{
			get
				{
				return parameters[1].PaletteNumber;
				}
			}

		/// <summary>
		/// Возвращает режим визуализации
		/// </summary>
		public VisualizationModes VisualizationMode
			{
			get
				{
				return parameters[1].VisualizationMode;
				}
			}

		/// <summary>
		/// Возвращает выбранную высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return parameters[1].SpectrogramHeight;
				}
			}

		// Сохранение настроек
		private void BOK_Click (object sender, System.EventArgs e)
			{
			// Закрепление настроек
			parameters[1].DeviceNumber = (byte)DevicesCombo.SelectedIndex;
			parameters[1].PaletteNumber = (byte)SDPaletteCombo.SelectedIndex;
			parameters[1].VisualizationMode = (VisualizationModes)VisualizationCombo.SelectedIndex;
			parameters[1].SpectrogramHeight = (uint)SDHeight.Value;

			parameters[1].VisualizationWidth = (uint)VisWidth.Value;
			parameters[1].VisualizationHeight = (uint)VisHeight.Value;
			parameters[1].VisualizationLeft = (uint)VisLeft.Value;
			parameters[1].VisualizationTop = (uint)VisTop.Value;

			parameters[1].AlwaysOnTop = AlwaysOnTopFlag.Checked;
			parameters[1].HistogramFFTValuesCount = (uint)(Math.Pow (2.0, HistogramRangeCombo.SelectedIndex) *
				CDParametersSet.HistogramFFTValuesCountMinimum);

			parameters[1].DecumulationMultiplier = (byte)CEDecumulationMultiplier.Value;
			parameters[1].CumulationSpeed = (byte)CECumulationSpeed.Value;
			parameters[1].LogoHeightPercentage = (byte)LogoHeightPercentage.Value;

			if (HistoRotAccToBeats.Checked)
				parameters[1].HistoRotSpeedDelta = (int)(-HistoRotSpeedArc.Value * 10);
			else
				parameters[1].HistoRotSpeedDelta = (int)(HistoRotSpeedArc.Value * 10);

			parameters[1].ShakeEffect = ShakeFlag.Checked;

			parameters[1].BeatsDetectorFFTScaleMultiplier = (byte)BDFFTScaleMultiplier.Value;
			parameters[1].BeatsDetectorHighEdge = (byte)BDHighEdge.Value;
			parameters[1].BeatsDetectorLowEdge = (byte)BDLowEdge.Value;
			parameters[1].BeatsDetectorLowLevel = (byte)BDLowLevel.Value;

			// Сохранение
			parameters[1].SaveSettings ();

			// Установка параметров
			ConcurrentDrawLib.SetPeakEvaluationParameters (parameters[1].BeatsDetectorLowEdge,
				parameters[1].BeatsDetectorHighEdge, parameters[1].BeatsDetectorLowLevel,
#if VIDEO
				(byte)(3 * parameters[1].BeatsDetectorFFTScaleMultiplier / 4));
#else
				parameters[1].BeatsDetectorFFTScaleMultiplier);
#endif
			ConcurrentDrawLib.SetHistogramFFTValuesCount (parameters[1].HistogramFFTValuesCount);

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
				return parameters[1].VisualizationWidth;
				}
			}

		/// <summary>
		/// Возвращает высоту окна визуализации
		/// </summary>
		public uint VisualizationHeight
			{
			get
				{
				return parameters[1].VisualizationHeight;
				}
			}

		/// <summary>
		/// Возвращает левый отступ окна визуализации
		/// </summary>
		public uint VisualizationLeft
			{
			get
				{
				return parameters[1].VisualizationLeft;
				}
			}

		/// <summary>
		/// Возвращает верхний отступ окна визуализации
		/// </summary>
		public uint VisualizationTop
			{
			get
				{
				return parameters[1].VisualizationTop;
				}
			}

		/// <summary>
		/// Возвращает флаг, требующий расположения окна поверх остальных
		/// </summary>
		public bool AlwaysOnTop
			{
			get
				{
				return parameters[1].AlwaysOnTop;
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
				return parameters[1].HistogramFFTValuesCount;
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
				return (uint)parameters[1].DecumulationMultiplier * (uint)parameters[1].CumulationSpeed /
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
				return parameters[1].CumulationSpeed;
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
			}

		/// <summary>
		/// Возвращает высоту лого в долях от высоты окна
		/// </summary>
		public double LogoHeight
			{
			get
				{
				return parameters[1].LogoHeightPercentage / 100.0;
				}
			}

		/// <summary>
		/// Возвращает скорость изменения угла поворота гистограммы
		/// </summary>
		public double HistoRotSpeedDelta
			{
			get
				{
				return Math.Abs (parameters[1].HistoRotSpeedDelta / 10.0);
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на режим синхронизации поворота гистограммы с бит-детектором
		/// </summary>
		public bool HistoRotAccordingToBeats
			{
			get
				{
				return (parameters[1].HistoRotSpeedDelta < 0);
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий на эффект тряски
		/// </summary>
		public bool ShakeEffect
			{
			get
				{
				return parameters[1].ShakeEffect;
				}
			}
		}
	}
