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
				}
			bool requestRequired = (settings == "");

			// Настройка контролов
			DevicesCombo.Items.AddRange (ConcurrentDrawLib.AvailableDevices);
			if (DevicesCombo.Items.Count < 1)
				{
				DevicesCombo.Items.Add ("(no devices)");
				DevicesCombo.Enabled = DevicesLabel.Enabled = false;
				}
			DevicesCombo.SelectedIndex = 0;

			SDPaletteCombo.Items.AddRange (ConcurrentDrawLib.AvailablePalettesNames);
			SDPaletteCombo.SelectedIndex = 0;

			for (int i = 0; i < VisualizationModesChecker.VisualizationModesCount; i++)
				VisualizationCombo.Items.Add (((VisualizationModes)i).ToString ());
			VisualizationCombo.SelectedIndex = 0;

			SDHeight.Minimum = VisHeight.Minimum = ConcurrentDrawLib.MinSpectrogramFrameHeight;
			SDHeight.Maximum = ConcurrentDrawLib.MaxSpectrogramFrameHeight;

			VisWidth.Minimum = ConcurrentDrawLib.MinSpectrogramFrameWidth;
			VisWidth.Maximum = Math.Min (ScreenWidth, ConcurrentDrawLib.MaxSpectrogramFrameWidth);
			VisHeight.Maximum = Math.Min (ScreenHeight, 1024);

			VisWidth.Value = VisWidth.Maximum;
			VisHeight.Value = VisHeight.Maximum;

			VisLeft.Maximum = ScreenWidth;
			VisTop.Maximum = ScreenHeight;

			BDLowEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowEdge;
			BDHighEdge.Value = ConcurrentDrawLib.DefaultPeakEvaluationHighEdge;
			BDLowLevel.Value = ConcurrentDrawLib.DefaultPeakEvaluationLowLevel;
			BDFFTScaleMultiplier.Value = ConcurrentDrawLib.DefaultFFTScaleMultiplier;

			// Разбор настроек
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
					}
				catch
					{
					requestRequired = true;
					}
				}

			// Завершение
			ConcurrentDrawLib.SetPeakEvaluationParameters ((byte)BDLowEdge.Value, (byte)BDHighEdge.Value,
				(byte)BDLowLevel.Value, (byte)BDFFTScaleMultiplier.Value);
			BCancel.Enabled = !requestRequired;
			if (requestRequired)
				this.ShowDialog ();
			}

		// Контроль наличия доступных устройств
		private void ConcurrentDrawParameters_Load (object sender, System.EventArgs e)
			{
			if (!DevicesCombo.Enabled)
				{
				MessageBox.Show ("No compatible audio output devices found", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Close ();
				}
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

				(AlwaysOnTopFlag.Checked ? "1" : "0");

			ConcurrentDrawLib.SetPeakEvaluationParameters ((byte)BDLowEdge.Value, (byte)BDHighEdge.Value,
				(byte)BDLowLevel.Value, (byte)BDFFTScaleMultiplier.Value);

			try
				{
				Registry.SetValue (settingsKey, "", settings);	// Вызовет исключение, если раздел не удалось создать
				}
			catch
				{
				}

			// Завершение
			this.Close ();
			}

		// Отмена настройки
		private void BCancel_Click (object sender, System.EventArgs e)
			{
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
		}
	}
