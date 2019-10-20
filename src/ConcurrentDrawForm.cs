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

		// Графика
		private LogoDrawerLayer mainLayer;						// Базовый слой изображения

		private Graphics gr, gl;								// Объекты-отрисовщики
		private List<SolidBrush> brushes = new List<SolidBrush> ();
		private List<Font> fonts = new List<Font> ();
		private Bitmap logo1a, logo1b, b;
		private SolidBrush br;

		private const int logoIdleSpeed = 2;					// Наименьшая скорость вращения лого
		private const int logoSpeedImpulse = 50;				// Импульс скорости
		private int currentArc = 0;								// Текущий угол приращения поворота лого
		private uint logoHeight;								// Диаметр лого

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

		// Видео
#if VIDEO
		private const float fps = 30.0f;						// Частота кадров видео
		private VideoManager vm = new VideoManager ();			// Видеофайл (балластная инициализация)
		private uint savingLayersCounter = 0;					// Счётчик сохранений
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
			Spectrogram = 5
			}

		/// <summary>
		/// Конструктор. Инициализирует экземпляр отрисовщика
		/// </summary>
		public ConcurrentDrawForm ()
			{
			InitializeComponent ();
			}

		private void CSDrawer_Shown (object sender, EventArgs e)
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
			logoHeight = (uint)(this.Height * 2) / 3;

			// Настройка диалогов
			SFVideo.Title = "Select placement of new video";
			SFVideo.Filter = "Audio-Video Interchange video format (*.avi)|(*.avi)";

			// Формирование шрифтов и кистей
			brushes.Add (new SolidBrush (Color.FromArgb (0, 0, 0)));
			brushes.Add (new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor ()));

			fonts.Add (new Font ("Consolas", 22, FontStyle.Regular));

			// Подготовка к записи в видеопоток
			mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);

			// Инициализация видеопотока
#if VIDEO
			SFVideo.FileName = "NewVideo.avi";
			if ((MessageBox.Show ("Write frames to AVI?", ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) &&
				(SFVideo.ShowDialog () == DialogResult.OK))
				{
				vm = new VideoManager (SFVideo.FileName, fps, mainLayer.Layer, true);

				if (!vm.IsInited)
					{
					MessageBox.Show ("Failed to initialize AVI stream", ProgramDescription.AssemblyTitle,
						 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					this.Close ();
					return;
					}
				}
#endif

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
				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_ALREADY:
				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_BUSY:
				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_NOTAVAIL:
					err = "Requested device is unavailable, busy or in use by another application";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_DEVICE:
					err = "Requested device is invalid or incompatible";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_DRIVER:
					err = "No compatible driver found for requested device";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_DX:
					err = "A sufficient version of DirectX is not installed";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_FORMAT:
					err = "Specified device doesn't support required audio mode";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_MEM:
					err = "No enough memory. Restart required";
					break;

				default:
				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_INIT:
				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_ERROR_UNKNOWN:
					throw new Exception ("Application failure. Debug required at point 1");

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_InvalidDLLVersion:
					err = "Version of " + ProgramDescription.AssemblyRequirements[1] + " is incompatible " +
						"with this application";
					break;

				case ConcurrentDrawLib.SoundStreamInitializationErrors.BASS_OK:
					break;
				}

			if (err != "")
				{
				MessageBox.Show (err, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
				}

			// Запуск спектрограммы, если требуется
			if (!VisualizationModesChecker.ContainsSpectrogram (cdp.VisualizationMode))
				return true;

			switch (ConcurrentDrawLib.InitializeSpectrogram ((uint)this.Width, cdp.SpectrogramHeight,
				cdp.PaletteNumber, VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode)))
				{
				case ConcurrentDrawLib.SpectrogramInitializationErrors.InitOK:
					break;

				case ConcurrentDrawLib.SpectrogramInitializationErrors.InvalidFrameSize:
					err = "Incorrect spectrogram image size";
					break;

				case ConcurrentDrawLib.SpectrogramInitializationErrors.NotEnoughMemory:
					err = "Not enough memory";
					break;

				default:
				case ConcurrentDrawLib.SpectrogramInitializationErrors.SoundStreamNotInitialized:
				case ConcurrentDrawLib.SpectrogramInitializationErrors.SpectrogramAlreadyInitialized:
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
					mainLayer.Descriptor.FillRectangle (brushes[0], 0, this.Height - (cdp.SpectrogramHeight * steps / 100),
						this.Width, cdp.SpectrogramHeight * steps / 100);

					if (++steps > 100)
						{
						steps = 0;
						currentPhase++;
						}
					break;

				case Phases.Spectrogram:
					DrawingSpectrogram ();
					break;
				}

			// Отрисовка слоёв
			DrawLayers ();
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
			mainLayer.Descriptor.DrawImage (logo1b, (this.Width - logo1b.Width) / 2, 0);
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
			mainLayer.Descriptor.DrawImage (logo1a, (this.Width - logo1a.Width) / 2, 0);

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
		private void DrawingSpectrogram ()
			{
			byte v = ConcurrentDrawLib.CurrentPeak;

			// Отрисовка лого при необходимости
			if (VisualizationModesChecker.ContainsLogo (cdp.VisualizationMode))
				{
				RotateAndDrawLogo (true);
				if (v > 0xF0)
					currentArc = -logoSpeedImpulse;

				br = new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor (v));
				mainLayer.Descriptor.FillEllipse (br, (this.Width - logo1b.Width / 3) / 2,
					(logo1b.Height - logo1b.Height / 3) / 2, logo1b.Width / 3, logo1b.Height / 3);
				br.Dispose ();
				}

			// Отрисовка спектрограммы при необходимости
			if (VisualizationModesChecker.ContainsSpectrogram (cdp.VisualizationMode))
				{
				b = ConcurrentDrawLib.CurrentSpectrogramFrame;
				mainLayer.Descriptor.DrawImage (b, 0, this.Height - b.Height);
				b.Dispose ();
				}
			}

		// Создание и подготовка слоёв и лого
		private void PrepareLayers ()
			{
			// Подготовка слоёв
			this.BackColor = brushes[0].Color;
			mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, mainLayer.Layer.Width, mainLayer.Layer.Height);

			// Первичная отрисовка
			DrawLayers ();

			// Инициализация лого
			logo1a = new Bitmap ((int)(logoHeight * 1.2), (int)(logoHeight * 1.2));
			gl = Graphics.FromImage (logo1a);

			// Переход к следующему обработчику
			steps = 0;
			currentPhase++;
			}

		// Отрисовка слоёв
		private void DrawLayers ()
			{
			// Отрисовка
#if VIDEO
			if (vm.IsInited)
				{
				b = (Bitmap)mainLayer.Layer.Clone ();
				vm.AddFrame (b);
				b.Dispose ();
				savingLayersCounter++;

				string s = "- Rendering -\nPhase: " + currentPhase.ToString () + "\nFrames: " + savingLayersCounter.ToString ();
				gr.DrawString ("- Rendering -\nPhase: ████████████████\nFrames: █████", fonts[0], brushes[0], 0, 0);
				gr.DrawString (s, fonts[0], brushes[1], 0, 0);
				}
			else
				{
#endif
			gr.DrawImage (mainLayer.Layer, mainLayer.Left, mainLayer.Top);
#if VIDEO
				}
#endif
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

			for (int i = 0; i < fonts.Count; i++)
				{
				fonts[i].Dispose ();
				}
			fonts.Clear ();

			if (gr != null)
				gr.Dispose ();
			if (gl != null)
				gl.Dispose ();
			if (logo1a != null)
				logo1a.Dispose ();
			if (logo1b != null)
				logo1b.Dispose ();
			mainLayer.Dispose ();

#if VIDEO
			vm.Dispose ();
#endif
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
					if (VisualizationModesChecker.ContainsSpectrogram (cdp.VisualizationMode))
						ConcurrentDrawLib.DestroySoundStream ();	// Объединяет функционал
					mainLayer.Dispose ();
					gr.Dispose ();

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
				mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, mainLayer.Layer.Width, mainLayer.Layer.Height);

				gr = Graphics.FromHwnd (this.Handle);

				// Перезапуск
				ExtendedTimer.Enabled = true;
				}
			}
		}
	}
