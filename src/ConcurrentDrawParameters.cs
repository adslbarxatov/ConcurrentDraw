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
		private const string settingsKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\" + ProgramDescription.AssemblyMainName;
		private char[] splitter = new char[] { ';' };

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
			this.Text = ProgramDescription.AssemblyMainName + " parameters";

			// Запрос настроек
			string settings = "";
			try
				{
				settings = Registry.GetValue (settingsKey, "", "").ToString ();	// Вызовет исключение при отсутствии ключа
				}
			catch
				{
				BHelp_Click (null, null);	// Справка на случай первого запуска
				}
			bool requestRequired = (settings == "");

			// Настройка контролов

			// Устройства
			DevicesCombo.Items.AddRange (ConcurrentDrawLib.AvailableDevices);
			if (DevicesCombo.Items.Count < 1)
				{
				DevicesCombo.Items.Add ("(no devices)");
				DevicesCombo.Enabled = DevicesLabel.Enabled = false;
				}
			DevicesCombo.SelectedIndex = 0;			// По умолчанию - первое

			// Палитра
			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = 0;		// По умолчанию - первая

			// Режим
			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ());
			VisualizationCombo.SelectedIndex = 7;	// По умолчанию - бабочка

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
				HistogramRangeCombo.Items.Add ("0 – " + (i * 22050.0 / 16.0).ToString () + " Hz");
			HistogramRangeCombo.SelectedIndex = 1;			// По умолчанию - до 2,7 кГц

			// Разбор сохранённых настроек
			if (!requestRequired)
				{
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
					requestRequired = true;
					}
				}

			// Завершение
			ConcurrentDrawLib.SetPeakEvaluationParameters ((byte)BDLowEdge.Value, (byte)BDHighEdge.Value,
				(byte)BDLowLevel.Value, (byte)BDFFTScaleMultiplier.Value);
			ConcurrentDrawLib.SetHistogramFFTValuesCount (this.HistogramFFTValuesCount);

			BCancel.Enabled = !requestRequired;
			if (requestRequired)
				this.ShowDialog ();
			}

		// Контроль наличия доступных устройств
		private void ConcurrentDrawParameters_Load (object sender, System.EventArgs e)
			{
			// Контроль возможности запуска
			if (!DevicesCombo.Enabled)
				{
				MessageBox.Show ("No compatible audio output devices found", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Close ();
				}

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
				Registry.SetValue (settingsKey, "", settings);	// Вызовет исключение, если раздел не удалось создать
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

			BDSettings.Text = "Range: " + (44100 * BDLowEdge.Value / 2048).ToString () + " – " +
				(44100 * BDHighEdge.Value / 2048).ToString () + " Hz; amplitude threshold: " +
				BDLowLevel.Value.ToString () + " of 255; FFT scale multiplier: " + BDFFTScaleMultiplier.Value.ToString ();
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
			MessageBox.Show ("Quick user manual\n\n" +
				"Press right mouse button to get to settings window later, ESC key to close the application\n\n" +
				"At first application will start with recommended settings. But you can change:\n" +
				"• Output device for audio data getting (stereo mixer or speakers required);\n" +
				"• Visualization mode (spectrogram, histogram or 'butterfly' histogram for now);\n" +
				"• Window size and placement (not less than 128 x 128 px and not more than 2048 x 1024 px);\n" +
				"• Spectrogram and histogram height (between 128 and 256 px; may load CPU);\n" +
				"• Histogram density (how many frequencies will be shown);\n" +
				"• Frequencies range and amplitude (loudness) threshold for beats detector;\n" +
				"• FFT scale multiplier (contrast) for spectrogram and histogram;\n" +
				"• 'Always on top' window state (turns off when settings window is active).\n" +
				"Settings will be saved by pressing OK button. They can be changed anytime you need"
				, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
