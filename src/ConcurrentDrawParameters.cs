using Microsoft.Win32;
using System;
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

		/// <summary>
		/// Возвращает ключ реестра, в котором хранятся настройки приложения
		/// </summary>
		public const string SettingsKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\" + ProgramDescription.AssemblyMainName;

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

			// Палитра
			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = 0;		// По умолчанию - первая

			// Режим
			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				{
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ().Replace ('_', ' '));
				}
			VisualizationCombo.SelectedIndex = (int)VisualizationModesChecker.VisualizationModesCount - 1;	// По умолчанию - бабочка

			// Высота спектрограммы
			SDHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SDHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;
			SDHeight.Value = 220;					// По умолчанию - 220 px

			// Размеры визуализации 
			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = Math.Min (ScreenHeight, 1024);

			VisWidth.Value = 9 * VisWidth.Maximum / 16;
			VisHeight.Value = 9 * VisHeight.Maximum / 16;	// По умолчанию - (9 / 16) размера экрана

			// Позиция визуализации
			VisLeft.Maximum = ScreenWidth;
			VisLeft.Value = ScreenWidth - VisWidth.Value;	// По умолчанию - верхний правый угол
			VisTop.Maximum = ScreenHeight;

			// Параметры детектора битов (получаются из DLL)
			BDLowEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowEdge;
			BDHighEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationHighEdge;
			BDLowLevel.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowLevel;
			BDFFTScaleMultiplier.Value = ConcurrentDrawLib.DefaultFFTScaleMultiplier;

			// Плотность гистограммы
			for (int i = 1; i <= 16; i *= 2)
				{
				HistogramRangeCombo.Items.Add ("0 – " + (i * 22050.0 / 16.0).ToString () +
					" " + Localization.GetText ("CDP_Hz", al));
				}
			HistogramRangeCombo.SelectedIndex = 1;			// По умолчанию - до 2,7 кГц

			// Язык интерфейса
			for (int i = 0; i < Localization.AvailableLanguages; i++)
				{
				LanguageCombo.Items.Add (((SupportedLanguages)i).ToString ());
				}
			LanguageCombo.SelectedIndex = (int)al;			// По умолчанию - английский

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
				settings = Registry.GetValue (SettingsKey,
					SettingsValueName, "").ToString ();
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
				SDPaletteCombo.SelectedIndex = int.Parse (values[1]);
				VisualizationCombo.SelectedIndex = int.Parse (values[2]);
				SDHeight.Value = decimal.Parse (values[3]);
				VisWidth.Value = decimal.Parse (values[4]);
				VisHeight.Value = decimal.Parse (values[5]);
				VisLeft.Value = decimal.Parse (values[6]);
				VisTop.Value = decimal.Parse (values[7]);

				uint bdSettings = uint.Parse (values[8]);
				BDLowEdge.Value = (int)(bdSettings & 0xFF);
				BDHighEdge.Value = (int)((bdSettings >> 8) & 0xFF);
				BDLowLevel.Value = (int)((bdSettings >> 16) & 0xFF);
				BDFFTScaleMultiplier.Value = (int)((bdSettings >> 24) & 0xFF);

				AlwaysOnTopFlag.Checked = (values[9] != "0");
				HistogramRangeCombo.SelectedIndex = int.Parse (values[10]);
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

			AlwaysOnTopFlag.Text = Localization.GetText ("CDP_AlwaysOnTopFlag", al);
			LogoResetFlag.Text = Localization.GetText ("CDP_LogoResetFlag", al);

			BeatsGroup.Text = Localization.GetText ("CDP_BeatsGroup", al);

			BOK.Text = Localization.GetText ("CDP_OK", al);
			BCancel.Text = Localization.GetText ("CDP_Cancel", al);
			LanguageLabel.Text = Localization.GetText ("CDP_LanguageLabel", al);

			BDLowEdge_ValueChanged (BDLowEdge, null);
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
			LogoResetFlag.Checked = false;
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
				return (uint)DevicesCombo.SelectedIndex;
				}
			}

		/// <summary>
		/// Номер выбранной палитры
		/// </summary>
		public byte PaletteNumber
			{
			get
				{
				return (byte)SDPaletteCombo.SelectedIndex;
				}
			}

		/// <summary>
		/// Возвращает флаг, указывающий вариант спектрограммы
		/// </summary>
		public VisualizationModes VisualizationMode
			{
			get
				{
				return (VisualizationModes)VisualizationCombo.SelectedIndex;
				}
			}

		/// <summary>
		/// Возвращает выбранную высоту изображения диаграммы
		/// </summary>
		public uint SpectrogramHeight
			{
			get
				{
				return (uint)SDHeight.Value;
				}
			}

		// Сохранение настроек
		private void BOK_Click (object sender, System.EventArgs e)
			{
			// Сохранение
			string settings = DevicesCombo.SelectedIndex.ToString () + splitter[0].ToString () +
				SDPaletteCombo.SelectedIndex.ToString () + splitter[0].ToString () +
				VisualizationCombo.SelectedIndex.ToString () + splitter[0].ToString () +
				SDHeight.Value.ToString () + splitter[0].ToString () +
				VisWidth.Value.ToString () + splitter[0].ToString () +
				VisHeight.Value.ToString () + splitter[0].ToString () +
				VisLeft.Value.ToString () + splitter[0].ToString () +
				VisTop.Value.ToString () + splitter[0].ToString () +

				(((BDFFTScaleMultiplier.Value & 0xFF) << 24) | ((BDLowLevel.Value & 0xFF) << 16) |
				((BDHighEdge.Value & 0xFF) << 8) | (BDLowEdge.Value & 0xFF)).ToString () + splitter[0].ToString () +

				(AlwaysOnTopFlag.Checked ? "1" : "0") + splitter[0].ToString () +
				HistogramRangeCombo.SelectedIndex.ToString ();

			try
				{
				Registry.SetValue (SettingsKey,
					SettingsValueName, settings);	// Вызовет исключение, если раздел не удалось создать
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
			LogoResetFlag.Checked = false;	// Перерисовка при отмене бессмысленна
			this.Close ();
			}

		/// <summary>
		/// Возвращает ширину окна визуализации
		/// </summary>
		public uint VisualizationWidth
			{
			get
				{
				return (uint)VisWidth.Value;
				}
			}

		/// <summary>
		/// Возвращает высоту окна визуализации
		/// </summary>
		public uint VisualizationHeight
			{
			get
				{
				return (uint)VisHeight.Value;
				}
			}

		/// <summary>
		/// Возвращает левый отступ окна визуализации
		/// </summary>
		public uint VisualizationLeft
			{
			get
				{
				return (uint)VisLeft.Value;
				}
			}

		/// <summary>
		/// Возвращает верхний отступ окна визуализации
		/// </summary>
		public uint VisualizationTop
			{
			get
				{
				return (uint)VisTop.Value;
				}
			}

		/// <summary>
		/// Возвращает флаг, требующий расположения окна поверх остальных
		/// </summary>
		public bool AlwaysOnTop
			{
			get
				{
				return AlwaysOnTopFlag.Checked;
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
				BDLowLevel.Value.ToString (), BDFFTScaleMultiplier.Value.ToString ());
			}

		// Выравнивание окна по экрану
		private void AlignToTop_Click (object sender, EventArgs e)
			{
			VisTop.Value = 0;
			}

		private void AlignToBottom_Click (object sender, EventArgs e)
			{
			VisTop.Value = VisHeight.Maximum - VisHeight.Value;
			}

		private void AlignToLeft_Click (object sender, EventArgs e)
			{
			VisLeft.Value = 0;
			}

		private void AlignToRight_Click (object sender, EventArgs e)
			{
			VisLeft.Value = VisWidth.Maximum - VisWidth.Value;
			}

		/// <summary>
		/// Возвращает флаг, требующий реинициализации лого
		/// </summary>
		public bool ReselLogo
			{
			get
				{
				return LogoResetFlag.Checked;
				}
			}

		// Установка реинициализации лого при изменении параметров, от которых зависит его вид
		private void SDPaletteCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			LogoResetFlag.Checked = true;
			}

		/// <summary>
		/// Возвращает количество значений FFT, которые используются при формировании гистограммы
		/// </summary>
		public uint HistogramFFTValuesCount
			{
			get
				{
				return 64u << HistogramRangeCombo.SelectedIndex;
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
		}
	}
