using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

#if AUDIO
using System.Runtime.InteropServices;
#endif

// Классы
namespace ESHQSetupStub
	{
	/// <summary>
	/// Класс обеспечивает отображение визуализации проекта
	/// </summary>
	public partial class ConcurrentDrawForm:Form
		{
		// Общие переменные и константы
		private Phases currentPhase = Phases.LayersPrecache;	// Текущая фаза отрисовки
		private uint steps = 0;									// Счётчик шагов отрисовки

		private ConcurrentDrawParameters cdp;					// Параметры работы программы
		private SupportedLanguages al = Localization.CurrentLanguage;	// Язык интерфейса приложения

		// Графика
		private LogoDrawerLayer mainLayer;						// Базовый слой изображения

		private Graphics gr, gl;								// Объекты-отрисовщики
		private List<SolidBrush> brushes = new List<SolidBrush> ();
		private Bitmap logo1a, logo1b, b;
		private SolidBrush br;

		private const int logoIdleSpeed = 2;					// Наименьшая скорость вращения лого
		private const int logoSpeedImpulse = 50;				// Импульс скорости
		private int currentArc = 0;								// Текущий угол приращения поворота лого
		private uint logoHeight;								// Диаметр лого

		private Pen p;											// Карандаш для линий гистограммы
		private int[] histoX = new int[4],
			histoY = new int[4];								// Координаты линий гистограммы
		private const double histoDensity = 4.0;				// Плотность гистограммы-бабочки

		private byte peak;										// Пиковое значение для расчёта битовых порогов
		private int rad, amp;									// Вспомогательные переменные

		// Аудио
#if AUDIO
		AudioManager am = new AudioManager (Application.StartupPath + "\\4.wav", false);

		// Эта конструкция имитирует нажатие клавиши, запускающей и останавливающей запись
		[DllImport ("user32.dll")]
		private static extern void keybd_event (byte vk, byte scan, int flags, int extrainfo);

		private void TriggerRecord ()
			{
			keybd_event ((byte)Keys.Add, 0, 0, 0);
			keybd_event ((byte)Keys.Add, 0, 2, 0);
			}
#endif

		// Фазы отрисовки
		private enum Phases
			{
			// Подготовка слоёв
			LayersPrecache = 1,

			// Вход лого
			LogoInbound = 2,

			// Вращение лого
			LogoRotation = 3,

			// Пауза после лого
			LogoIntermission = 4,

			// Затенение лого
			Visualization = 5
			}

		/// <summary>
		/// Конструктор. Инициализирует экземпляр отрисовщика
		/// </summary>
		public ConcurrentDrawForm ()
			{
			InitializeComponent ();
			}

		private void ConcurrentDrawForm_Shown (object sender, EventArgs e)
			{
			// Если запрос границ экрана завершается ошибкой, отменяем отображение
			try
				{
				this.Width = Screen.PrimaryScreen.Bounds.Width;
				this.Height = Screen.PrimaryScreen.Bounds.Height;
				}
			catch
				{
				this.Close ();
				return;
				}
			this.Text = ProgramDescription.AssemblyTitle;

			// Запрос параметров (при необходимости вызовет окно настроек)
			cdp = new ConcurrentDrawParameters ((uint)this.Width, (uint)this.Height);
			if (!cdp.HasAvailableDevices)
				{
				this.Close ();
				return;
				}
			this.Width = (int)cdp.VisualizationWidth;
			this.Height = (int)cdp.VisualizationHeight;
			this.Left = (int)cdp.VisualizationLeft;
			this.Top = (int)cdp.VisualizationTop;
			this.TopMost = cdp.AlwaysOnTop;

			// Запуск аудиоканала
			if (!InitializeAudioStream ())
				{
				this.Close ();
				return;
				}

			// Настройка окна
			gr = Graphics.FromHwnd (this.Handle);
			logoHeight = (uint)(Math.Min (this.Width, this.Height) * 7) / 12;

			// Формирование шрифтов и кистей
			brushes.Add (new SolidBrush (Color.FromArgb (0, 0, 0)));
			brushes.Add (new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor ()));
			brushes.Add (new SolidBrush (Color.FromArgb (20, brushes[0].Color)));

			// Подготовка к отрисовке
			mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);

			// Запуск
			ExtendedTimer.Enabled = true;
			this.Activate ();
			}

		// Метод инициализирует аудиоканал
		private bool InitializeAudioStream ()
			{
			string err = "";
			switch (ConcurrentDrawLib.InitializeSoundStream (cdp.DeviceNumber))
				{
				case SoundStreamInitializationErrors.BASS_ERROR_ALREADY:
				case SoundStreamInitializationErrors.BASS_ERROR_BUSY:
					err = Localization.GetText ("BASS_ERROR_BUSY", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_NOTAVAIL:
					err = Localization.GetText ("BASS_ERROR_NOTAVAIL", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DEVICE:
					err = Localization.GetText ("BASS_ERROR_DEVICE", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DRIVER:
					err = Localization.GetText ("BASS_ERROR_DRIVER", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DX:
					err = Localization.GetText ("BASS_ERROR_DX", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FORMAT:
					err = Localization.GetText ("BASS_ERROR_FORMAT", al);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_MEM:
					err = Localization.GetText ("BASS_ERROR_MEM", al);
					break;

				default:
				case SoundStreamInitializationErrors.BASS_ERROR_INIT:
				case SoundStreamInitializationErrors.BASS_ERROR_UNKNOWN:
					throw new Exception ("Application failure. Debug required at point 1");

				case SoundStreamInitializationErrors.BASS_InvalidDLLVersion:
					err = string.Format (Localization.GetText ("LibraryIsIncompatible", al),
						ProgramDescription.AssemblyRequirements[1]);
					break;

				case SoundStreamInitializationErrors.BASS_OK:
					break;
				}

			if (err != "")
				{
				MessageBox.Show (err, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
				}

			// Отмена инициализации спектрограммы, если она не требуется
			if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) ==
				SpectrogramModes.NoSpectrogram)
				{
				// Ручное заполнение палитры и выход
				ConcurrentDrawLib.FillPalette (cdp.PaletteNumber);
				return true;
				}

			// Запуск спектрограммы, если требуется
			switch (ConcurrentDrawLib.InitializeSpectrogram ((uint)this.Width, cdp.SpectrogramHeight,
				cdp.PaletteNumber, VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode)))
				{
				case SpectrogramInitializationErrors.InitOK:
					break;

				case SpectrogramInitializationErrors.NotEnoughMemory:
					err = Localization.GetText ("BASS_ERROR_MEM", al);
					break;

				default:
				case SpectrogramInitializationErrors.InvalidFrameSize:
				case SpectrogramInitializationErrors.SoundStreamNotInitialized:
				case SpectrogramInitializationErrors.SpectrogramAlreadyInitialized:
					throw new Exception ("Application failure. Debug required at point 2");
				}

			if (err != "")
				{
				MessageBox.Show (err, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
				}

			// Успешно
			return true;
			}

		// Таймер расширенного режима отображения
		private void ExtendedTimer_Tick (object sender, EventArgs e)
			{
			switch (currentPhase)
				{
				// Создание фрагментов лого
				case Phases.LayersPrecache:
#if AUDIO
					TriggerRecord ();
					am.PlayAudio ();
#endif
					PrepareLayers ();
					break;

				// Отрисовка фрагментов лого
				case Phases.LogoInbound:
					DrawingLogo ();
					break;

				// Вращение лого
				case Phases.LogoRotation:
					RotatingLogo ();
					break;

				// Пауза
				case Phases.LogoIntermission:
					RotateAndDrawLogo (true);

					if ((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
						SpectrogramModes.NoSpectrogram))
						{
						mainLayer.Descriptor.FillRectangle (brushes[0], 0, this.Height - (cdp.SpectrogramHeight * steps / 100),
							this.Width, cdp.SpectrogramHeight * steps / 100);
						}
					else
						{
						currentPhase++;
						}

					if (++steps > 100)
						{
						steps = 0;
						currentPhase++;
						}
					break;

				case Phases.Visualization:
					DrawingVisualization ();
					break;
				}

			// Отрисовка изображения
			gr.DrawImage (mainLayer.Layer, mainLayer.Left, mainLayer.Top);
			}

		// Поворачивает и отрисовывает лого
		private void RotateAndDrawLogo (bool PushBrakes)
			{
			// Торможение вращения
			if (PushBrakes)
				currentArc = (currentArc - logoIdleSpeed) / 2;

			// Отрисовка
			gl.RotateTransform (currentArc);
			gl.DrawImage (logo1a, -(int)(logoHeight * 0.6), -(int)(logoHeight * 0.6));
			mainLayer.Descriptor.DrawImage (logo1b, (this.Width - logo1b.Width) / 2,
				((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
					SpectrogramModes.NoSpectrogram) ? 0 : (this.Height - logo1b.Height) / 2));
			}

		// Первичное вращение лого
		private void RotatingLogo ()
			{
			// Отрисовка
			currentArc = (int)(++steps);
			RotateAndDrawLogo (false);

			if (steps >= 170)
				{
				currentArc = -logoIdleSpeed;
				steps = 0;
				currentPhase++;
				}
			}

		// Метод формирует лого
		private void DrawingLogo ()
			{
			// Задний круг
			gl.FillEllipse (brushes[1], (int)(logoHeight * 0.1), (int)(logoHeight * 0.1), logoHeight, logoHeight);

			// Передний круг
			gl.FillEllipse (brushes[0], (int)(logoHeight * 0.1) + steps, (int)(logoHeight * 0.1) - steps,
				logoHeight - 2 * steps, logoHeight + 2 * steps);

			// Отрисовка
			mainLayer.Descriptor.DrawImage (logo1a, (this.Width - logo1a.Width) / 2,
				((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
				SpectrogramModes.NoSpectrogram) ? 0 : (this.Height - logo1a.Height) / 2));

			steps++;
			if (steps >= 0.05 * logoHeight)
				{
				gl.Dispose ();
				logo1b = new Bitmap ((int)(logoHeight * 1.2), (int)(logoHeight * 1.2));
				gl = Graphics.FromImage (logo1b);

				gl.TranslateTransform ((int)(logoHeight * 0.6), (int)(logoHeight * 0.6));
				steps = 0;
				currentPhase++;
				}
			}

		// Отрисовка фрагментов лого
		private void DrawingVisualization ()
			{
			// Запрос пикового значения
			peak = ConcurrentDrawLib.CurrentPeak;

			// Отрисовка гистограммы-бабочки при необходимости (исключает спектрограмму)
			if (cdp.VisualizationMode == VisualizationModes.Butterfly_histogram_with_logo)
				{
				// Сброс изображения
				mainLayer.Descriptor.FillEllipse (brushes[2], (this.Width - logo1b.Width) / 2 - 256,
					(this.Height - logo1b.Height) / 2 - 256, logo1b.Width + 512, logo1b.Height + 512);

				// Отрисовка
				for (int i = 0; i < 256; i++)
					{
					// Получаем амплитуду
					amp = ConcurrentDrawLib.GetScaledAmplitude ((uint)(cdp.HistogramFFTValuesCount * i / 256));

					// Получаем цвет
					if (p != null)
						p.Dispose ();
					p = new Pen (ConcurrentDrawLib.GetColorFromPalette ((byte)amp));

					// Определяем координаты линий
					rad = (logo1b.Width) / 2 + amp;
					histoX[0] = histoX[2] = this.Width / 2 + (int)(rad * Math.Cos (ArcToRad (i / histoDensity)));
					histoX[1] = histoX[3] = this.Width / 2 + (int)(rad * Math.Cos (ArcToRad (180.0 + i / histoDensity)));
					histoY[0] = histoY[3] = this.Height / 2 + (int)(rad * Math.Sin (ArcToRad (i / histoDensity)));
					histoY[1] = histoY[2] = this.Height / 2 + (int)(rad * Math.Sin (ArcToRad (180.0 + i / histoDensity)));

					// Рисуем
					mainLayer.Descriptor.DrawLine (p, histoX[0], histoY[0], histoX[1], histoY[1]);
					mainLayer.Descriptor.DrawLine (p, histoX[2], histoY[2], histoX[3], histoY[3]);
					}
				}

			// Отрисовка лого при необходимости
			if (VisualizationModesChecker.ContainsLogo (cdp.VisualizationMode))
				{
				RotateAndDrawLogo (true);
				if (peak > 0xF0)
					currentArc = -logoSpeedImpulse;

				br = new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor (peak));

				rad = 650 * logo1b.Height / (1950 - peak);
				mainLayer.Descriptor.FillEllipse (br, (this.Width - rad) / 2,
					(
					((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
					SpectrogramModes.NoSpectrogram) ? logo1b.Height : this.Height)
					- rad) / 2, rad, rad);

				br.Dispose ();
				}

			// Отрисовка спектрограммы при необходимости
			if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
				SpectrogramModes.NoSpectrogram)
				{
				b = ConcurrentDrawLib.CurrentSpectrogramFrame;
				mainLayer.Descriptor.DrawImage (b, 0, this.Height - b.Height);
				b.Dispose ();
				}
			}

		// Метод пересчитывает градусы в радианы
		private double ArcToRad (double Arc)
			{
			return Math.PI * Arc / 180.0;
			}

		// Создание и подготовка слоёв и лого
		private void PrepareLayers ()
			{
			// Подготовка слоёв
			this.BackColor = brushes[0].Color;
			mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, this.Width, this.Height);

			// Инициализация лого
			logo1a = new Bitmap ((int)(logoHeight * 1.2), (int)(logoHeight * 1.2));
			gl = Graphics.FromImage (logo1a);

			// Переход к следующему обработчику
			steps = 0;
			currentPhase++;
			}

		// Закрытие окна
		private void LogoDrawer_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Остановка всех отрисовок
			ExtendedTimer.Enabled = false;
			ConcurrentDrawLib.DestroySoundStream ();
#if AUDIO
			am.StopAudio ();
			am.Dispose ();
#endif

			// Сброс ресурсов
			for (int i = 0; i < brushes.Count; i++)
				{
				brushes[i].Dispose ();
				}
			brushes.Clear ();

			if (gr != null)
				gr.Dispose ();
			if (gl != null)
				gl.Dispose ();
			if (logo1a != null)
				logo1a.Dispose ();
			if (logo1b != null)
				logo1b.Dispose ();
			if (mainLayer != null)
				mainLayer.Dispose ();
			if (cdp != null)
				cdp.Dispose ();
			}

		// Принудительный выход (по любой клавише)
		private void LogoDrawer_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Escape)
				this.Close ();
			}

		// Вызов настроек
		private void ConcurrentDrawForm_MouseClick (object sender, MouseEventArgs e)
			{
			// Реинициализация
			if (e.Button == MouseButtons.Right)
				{
				do
					{
					// Остановка отрисовки и сброс слоя
					ExtendedTimer.Enabled = false;
					ConcurrentDrawLib.DestroySoundStream ();	// Объединяет функционал
					if (mainLayer != null)
						mainLayer.Dispose ();
					if (gr != null)
						gr.Dispose ();
					this.TopMost = false;	// Разрешает отображение окна параметров

					// Перезапрос параметров
					cdp.ShowDialog ();

					// Переопределение размера окна
					this.Width = (int)cdp.VisualizationWidth;
					this.Height = (int)cdp.VisualizationHeight;
					this.Left = (int)cdp.VisualizationLeft;
					this.Top = (int)cdp.VisualizationTop;
					this.TopMost = cdp.AlwaysOnTop;
					} while (!InitializeAudioStream ());

				// Пересоздание кисти лого и сброс поля отрисовки
				brushes[1].Color = ConcurrentDrawLib.GetMasterPaletteColor ();
				mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);
				mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, this.Width, this.Height);
				gr = Graphics.FromHwnd (this.Handle);

				// Реинициализация лого (при необходимости)
				if (cdp.ReselLogo)
					{
					if (gl != null)
						gl.Dispose ();
					if (logo1a != null)
						logo1a.Dispose ();
					if (logo1b != null)
						logo1b.Dispose ();
					logoHeight = (uint)(Math.Min (this.Width, this.Height) * 7) / 12;
					currentPhase = Phases.LayersPrecache;
					}

				// Перезапуск
				ExtendedTimer.Enabled = true;
				}
			}
		}
	}
