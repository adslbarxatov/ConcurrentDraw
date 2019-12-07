using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

#if AUDIO
using System.Runtime.InteropServices;
#endif
#if VIDEO
using System.ComponentModel;
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
		private bool logoFirstShowMade = false;					// Флаг, указывающий на выполненное первое отображение лого
		private uint steps = 0;									// Счётчик шагов отрисовки

		private ConcurrentDrawParameters cdp;					// Параметры работы программы
		private SupportedLanguages al = Localization.CurrentLanguage;		// Язык интерфейса приложения

		// Графика
		private LogoDrawerLayer mainLayer;						// Базовый слой изображения
		private ColorMatrix[] colorMatrix = new ColorMatrix[2];				// Полупрозрачные цветовые матрицы для спектрограмм
		private ImageAttributes[] sgAttributes = new ImageAttributes[2];	// Атрибуты изображений спектрограмм

		private List<Graphics> gr = new List<Graphics> ();		// Объекты-отрисовщики
		private List<SolidBrush> brushes = new List<SolidBrush> ();
		private List<Bitmap> logo = new List<Bitmap> ();

		private const int logoIdleSpeed = 2;					// Наименьшая скорость вращения лого
		private int logoSpeedImpulse = 50,						// Импульс скорости
			currentLogoArcDelta = 0;							// Текущий угол приращения поворота лого
		private double currentHistogramArc = 0.0;				// Текущий угол поворота гистограммы-бабочки
		private uint logoHeight;								// Диаметр лого

		private byte peak;										// Пиковое значение для расчёта битовых порогов
		private const byte peakTrigger = 0xF0;					// Значение пика, превышение которого является триггером
		private uint cumulativeCounter;							// Накопитель, обеспечивающий изменение фона
		private const uint cumulationDivisor = 100,				// Границы накопителя
			cumulationLimit = 255 * cumulationDivisor;

		private int[] histoX = new int[4],
			histoY = new int[4];								// Координаты линий гистограммы
		private const double histoDensity = 2.75;				// Плотность гистограммы-бабочки
		// (даёт полный угол чуть более 90°; 90° <=> 2.84)

		private int rad, amp;									// Вспомогательные переменные
		private Bitmap b;
		private Pen p;
		private Random rnd = new Random ();

		// Аудио
#if AUDIO
		AudioManager am = new AudioManager (Application .StartupPath + "\\5.wav", false);

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
		private const double fps = 23.4375;						// Частота кадров видео 
		// определена по аудио как 48000 Hz * 2 ch * 16 bps / 
		// (8 * sizeof (float) * 2048 fftv)

		private VideoManager vm = new VideoManager ();			// Видеофайл (балластная инициализация)
		private uint savingLayersCounter = 0;					// Счётчик сохранений

		private Font demoFont;									// Объекты поддержки текстовых подписей на рендере
		private string[] demoNames = new string[] { "SERAPHIM PROJECT", "СРЕДЬ ТЁМНЫХ УЛИЦ" };
		private SizeF[] demoSizes = new SizeF[2];
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

			// Спецкоманды перед визуализацией
			PreVisualization = 4,

			// Визуализация
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
			cumulativeCounter = cdp.CumulationSpeed;

			// Подготовка к отрисовке
			mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);

			colorMatrix[0] = new ColorMatrix ();
			colorMatrix[0].Matrix33 = 0.9f;
			colorMatrix[1] = new ColorMatrix ();
			colorMatrix[1].Matrix33 = 0.5f;

			sgAttributes[0] = new ImageAttributes ();
			sgAttributes[0].SetColorMatrix (colorMatrix[0], ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			sgAttributes[1] = new ImageAttributes ();
			sgAttributes[1].SetColorMatrix (colorMatrix[1], ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			// Инициализация видеопотока
#if VIDEO
			SFVideo.Title = "Select placement of new video file";
			SFVideo.Filter = "Audio-Video Interchange video format (*.avi)|*.avi";
			SFVideo.FileName = "NewVideo.avi";

			OFAudio.Title = "Select audio file for rendering";
			OFAudio.Filter = "Windows PCM audio files (*.wav)|*.wav";

			switch (MessageBox.Show ("Write frames to AVI ('No' opens audio only)?", ProgramDescription.AssemblyTitle,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3))
				{
				case DialogResult.Yes:
					if ((SFVideo.ShowDialog () == DialogResult.OK) && (OFAudio.ShowDialog () == DialogResult.OK))
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
					break;

				case DialogResult.No:
					if (OFAudio.ShowDialog () != DialogResult.OK)
						OFAudio.FileName = "";
					break;
				}
#endif

			// Запуск аудиоканала
			switch (InitializeAudioStream ())
				{
				case 1:
					cdp.ShowDialog ();
					this.Close ();
					return;

				case -1:
					this.Close ();
					return;
				}

			// Настройка окна
			gr.Add (Graphics.FromHwnd (this.Handle));
			ResetLogo ();

			// Формирование кистей
			brushes.Add (new SolidBrush (ProgramDescription.MasterBackColor));			// Фон
			brushes.Add (new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor ()));	// Лого и beat-детектор
			brushes.Add (new SolidBrush (Color.FromArgb (20, brushes[0].Color)));		// Fade out

#if VIDEO
			// Подготовка параметров
			demoFont = new Font ("a_GroticNr", this.Width / 50);
			for (int i = 0; i < demoNames.Length; i++)
				{
				demoSizes[i] = gr[0].MeasureString (demoNames[i], demoFont);
				}

			// Запуск рендеринга
			if (vm.IsInited)
				{
				this.TopMost = false;
				logoSpeedImpulse += 10;	// Из-за низкого FPS приходится ускорять
				HardWorkExecutor hwe = new HardWorkExecutor (RenderVideo, "Total count of frames", "Rendering...");

				// Без выхода в основной режим
				this.Close ();
				return;
				}
			else
#endif
			// Запуск таймера
				{
				ExtendedTimer.Enabled = true;
				}
				this.Activate ();
			}

		// Метод инициализирует аудиоканал
		private int InitializeAudioStream ()
			{
			string err = "";
			int result = -1;
			SoundStreamInitializationErrors ssie;
#if VIDEO
			if (vm.IsInited || (OFAudio.FileName != ""))
				ssie = ConcurrentDrawLib.InitializeSoundStream (OFAudio.FileName);
			else
#endif
			ssie = ConcurrentDrawLib.InitializeSoundStream (cdp.DeviceNumber);
			switch (ssie)
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
				case SoundStreamInitializationErrors.BASS_RecordAlreadyRunning:
					throw new Exception ("Application failure. Debug required at point 1");

				case SoundStreamInitializationErrors.BASS_ERROR_NO3D:
				case SoundStreamInitializationErrors.BASS_ERROR_UNKNOWN:
					// Возникает при выборе стереомикшера при включённом микрофоне (почему-то)
					err = Localization.GetText ("DeviceBehaviorIsInvalid", al);
					result = 1;		// Запросить настройку приложения
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_ILLPARAM:
				case SoundStreamInitializationErrors.BASS_ERROR_SPEAKER:
					throw new Exception ("Application failure. Debug required at point 2");

				case SoundStreamInitializationErrors.BASS_InvalidDLLVersion:
					err = string.Format (Localization.GetText ("LibraryIsIncompatible", al),
						ProgramDescription.AssemblyRequirements[1]);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FILEOPEN:
					err = string.Format (Localization.GetText ("BASS_ERROR_FILEOPEN", al), OFAudio.FileName);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FILEFORM:
				case SoundStreamInitializationErrors.BASS_ERROR_CODEC:
					err = string.Format (Localization.GetText ("BASS_ERROR_CODEC", al), OFAudio.FileName);
					break;

				case SoundStreamInitializationErrors.BASS_OK:
					break;
				}

			if (err != "")
				{
				MessageBox.Show (err, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return result;
				}

			// Отмена инициализации спектрограммы, если она не требуется
			if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) ==
				SpectrogramModes.NoSpectrogram)
				{
				// Ручное заполнение палитры и выход
				ConcurrentDrawLib.FillPalette (cdp.PaletteNumber);
				return 0;
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
					throw new Exception ("Application failure. Debug required at point 3");
				}

			if (err != "")
				{
				MessageBox.Show (err, ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return result;
				}

			// Успешно
			return 0;
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
#endif
					PrepareLayers ();
					break;

				// Отрисовка фрагментов лого
				case Phases.LogoInbound:
					DrawingLogo ();
					break;

				// Вращение лого
				case Phases.LogoRotation:
					if (logoFirstShowMade)
						currentPhase = Phases.PreVisualization;

					RotatingLogo ();
					break;

				// Спецкоманды
				case Phases.PreVisualization:
#if AUDIO
					am.PlayAudio ();
#endif
					if (cdp.TransparentLogo)
						{
						logo[0].MakeTransparent (ProgramDescription.MasterBackColor);
						gr[1].CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
						}

					logoFirstShowMade = true;
					currentPhase++;
					break;

				// Основной режим
				case Phases.Visualization:
					DrawingVisualization ();
					break;
				}

			// Отрисовка изображения
			DrawFrame ();
			}

#if VIDEO
		// Метод рендеринга видеофайла
		private void RenderVideo (object sender, DoWorkEventArgs e)
			{
			// Запрос длины потока
			uint length = (uint)(ConcurrentDrawLib.ChannelLength * fps +
				((cdp.VisualizationMode == VisualizationModes.Butterfly_histogram) ? 250 : 350));

			// Собственно, выполняемый процесс
			for (int i = 0; i < length; i++)
				{
				ExtendedTimer_Tick (null, null);

				((BackgroundWorker)sender).ReportProgress (100 * i / (int)length,
					"Rendered frames: " + i.ToString () + " out of " + length.ToString ());
				// Возврат прогресса
				// Отмена запрещена
				}

			// Завершено
			e.Result = 0;
			}
#endif

		// Метод отрисовывает сформированный кадр
		private void DrawFrame ()
			{
			// Отрисовка
#if VIDEO
			if (vm.IsInited)
				{
				b = (Bitmap)mainLayer.Layer.Clone ();
				vm.AddFrame (b);
				b.Dispose ();
				savingLayersCounter++;
				}
			else
#endif
				{
				gr[0].DrawImage (mainLayer.Layer, mainLayer.Left, mainLayer.Top);
				}
			}

		// Поворачивает и отрисовывает лого
		private void RotateAndDrawLogo (bool PushBrakes)
			{
			// Торможение вращения
			if (PushBrakes)
				currentLogoArcDelta = (currentLogoArcDelta - logoIdleSpeed) / 2;

			// Отрисовка
			gr[1].RotateTransform (currentLogoArcDelta);

			gr[1].DrawImage (logo[0], -3 * logoHeight / 5, -3 * logoHeight / 5);
			mainLayer.Descriptor.DrawImage (logo[1], (this.Width - logo[1].Width) / 2,
				((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
				SpectrogramModes.NoSpectrogram) ? 0 : (this.Height - logo[1].Height) / 2));
			}

		// Первичное вращение лого
		private void RotatingLogo ()
			{
			// Отрисовка
			currentLogoArcDelta = (int)(++steps);
			RotateAndDrawLogo (false);

			if (steps >= 170)
				{
				currentLogoArcDelta = -logoIdleSpeed;
				steps = 0;
				currentPhase++;
				}
			}

		// Метод формирует лого
		private void DrawingLogo ()
			{
			// Задний круг
			gr[1].FillEllipse (brushes[1], logoHeight / 10, logoHeight / 10, logoHeight, logoHeight);

			// Передний эллипс
			gr[1].FillEllipse (brushes[0], logoHeight / 10 + steps, logoHeight / 10 - steps,
				logoHeight - 2 * steps, logoHeight + 2 * steps);

			// Отрисовка
			mainLayer.Descriptor.DrawImage (logo[0], (this.Width - logo[0].Width) / 2,
				((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
				SpectrogramModes.NoSpectrogram) ? 0 : (this.Height - logo[0].Height) / 2));

			steps++;
			if (steps >= logoHeight / 27)
				{
				gr[1].Dispose ();
				gr.RemoveAt (1);

				logo.Add (new Bitmap (6 * (int)logoHeight / 5, 6 * (int)logoHeight / 5));
				gr.Add (Graphics.FromImage (logo[1]));

				gr[1].TranslateTransform (3 * logoHeight / 5, 3 * logoHeight / 5);
				steps = 0;
				currentPhase++;
				}
			}

		// Отрисовка фрагментов лого
		private void DrawingVisualization ()
			{
#if VIDEO
			// Ручное обновление кадра при записи
			if (vm.IsInited || (OFAudio.FileName != ""))
				ConcurrentDrawLib.UpdateFFTData ();
#endif

			// Запрос пикового значения
			peak = ConcurrentDrawLib.CurrentPeak;

			// Отрисовка гистограммы-бабочки при необходимости (исключает спектрограмму)
			if (cdp.VisualizationMode == VisualizationModes.Butterfly_histogram)
				{
				// Обработка кумулятивного значения
				uint oldCC = cumulativeCounter;
				if (cumulativeCounter > cdp.DecumulationSpeed)
					cumulativeCounter -= cdp.DecumulationSpeed;
				if ((peak > peakTrigger) && (cumulativeCounter < cumulationLimit))
					cumulativeCounter += cdp.CumulationSpeed;
				if ((cumulativeCounter / cumulationDivisor) != (oldCC / cumulationDivisor))
					brushes[2].Color = Color.FromArgb (15,
						ConcurrentDrawLib.GetMasterPaletteColor ((byte)(cumulativeCounter / cumulationDivisor)));

				// Сброс изображения
				mainLayer.Descriptor.FillEllipse (brushes[2], (this.Width - 3 * logo[1].Width) / 2 - 1,
					(this.Height - 3 * logo[1].Height) / 2 - 1, 3 * logo[1].Width + 2, 3 * logo[1].Height + 2);

				// Обработка вращения гистограммы
				if (cdp.HistoRotAccordingToBeats)
					currentHistogramArc += (cdp.HistoRotSpeedDelta * currentLogoArcDelta / logoSpeedImpulse);
				else
					currentHistogramArc += cdp.HistoRotSpeedDelta;

				if (currentHistogramArc > 360.0)
					currentHistogramArc -= 360.0;
				if (currentHistogramArc < 0.0)
					currentHistogramArc += 360.0;

				// Отрисовка
				for (int i = 0; i < 256; i++)
					{
					// Получаем амплитуду
					amp = ConcurrentDrawLib.GetScaledAmplitude ((uint)(cdp.HistogramFFTValuesCount * i) >> 8);	// Вместо /256

					// Получаем цвет
					p = new Pen (ConcurrentDrawLib.GetColorFromPalette ((byte)(3 * amp / 4)), logoHeight / 80);

					// Определяем координаты линий
					rad = logo[1].Width / 2 + (int)((uint)(logo[1].Width * amp) >> 8);	// Вместо /256
					histoX[0] = histoX[2] = this.Width / 2 + (int)(rad * Math.Cos (ArcToRad (i / histoDensity + currentHistogramArc)));
					histoX[1] = histoX[3] = this.Width - histoX[0];
					histoY[0] = histoY[3] = this.Height / 2 + (int)(rad * Math.Sin (ArcToRad (i / histoDensity + currentHistogramArc)));
					histoY[1] = histoY[2] = this.Height - histoY[0];

					// Рисуем
					mainLayer.Descriptor.DrawLine (p, histoX[0], histoY[0], histoX[1], histoY[1]);
					mainLayer.Descriptor.DrawLine (p, histoX[2], histoY[2], histoX[3], histoY[3]);

					// Завершено
					p.Dispose ();
					}
				}

			// Отрисовка лого при необходимости
			if (VisualizationModesChecker.ContainsLogo (cdp.VisualizationMode))
				{
				// Лого
				RotateAndDrawLogo (true);
				if (peak > peakTrigger)
					{
					currentLogoArcDelta = -logoSpeedImpulse;
					}

				// Бит-детектор
				p = new Pen (ConcurrentDrawLib.GetMasterPaletteColor (peak), logoHeight / 50);
				rad = 650 * logo[1].Height / (1950 - peak);
				if (cdp.ShakingBitDetector)
					amp = (int)((logoHeight * peak) >> 15);

				mainLayer.Descriptor.DrawEllipse (p, (this.Width - rad) / 2 +
					(cdp.ShakingBitDetector ? rnd.Next (-amp, amp) : 0),

					(((VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
					SpectrogramModes.NoSpectrogram) ? logo[1].Height : this.Height) - rad) / 2 +
					(cdp.ShakingBitDetector ? rnd.Next (-amp, amp) : 0),

					rad, rad);

#if VIDEO
				/*if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) == SpectrogramModes.NoSpectrogram)
					{
					mainLayer.Descriptor.DrawString (demoNames[0], demoFont, br, (this.Width - demoSizes[0].Width) / 2, 20);
					mainLayer.Descriptor.DrawString (demoNames[1], demoFont, br, (this.Width - demoSizes[1].Width) / 2,
						this.Height - demoSizes[1].Height - 20);
					mainLayer.Descriptor.DrawString ((cumulativeCounter / cumulationDivisor).ToString (), demoFont, brushes[0],
						(this.Width - rad) / 2, this.Height / 2 - 18);
					}
				else
					{
					mainLayer.Descriptor.DrawString (demoNames[0], demoFont, br, 20, logoHeight / 2);
					mainLayer.Descriptor.DrawString (demoNames[1], demoFont, br, this.Width - demoSizes[1].Width - 20,
						logoHeight / 2);
					}*/
#endif

				p.Dispose ();
				}

			// Отрисовка спектрограммы при необходимости
			if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) !=
				SpectrogramModes.NoSpectrogram)
				{
				b = ConcurrentDrawLib.CurrentSpectrogramFrame;

				if ((cdp.VisualizationMode == VisualizationModes.Static_spectrogram) ||
					(cdp.VisualizationMode == VisualizationModes.Moving_spectrogram))
					{
					mainLayer.Descriptor.DrawImage (b, new Rectangle (0, this.Height - b.Height, b.Width, b.Height),
						0, 0, b.Width, b.Height, GraphicsUnit.Pixel, sgAttributes[0]);
					}
				else
					{
					mainLayer.Descriptor.DrawImage (b, new Rectangle (0, this.Height - b.Height, b.Width, b.Height),
						0, 0, b.Width, b.Height, GraphicsUnit.Pixel, sgAttributes[1]);
					}
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
			logo.Add (new Bitmap (6 * (int)logoHeight / 5, 6 * (int)logoHeight / 5));
			gr.Add (Graphics.FromImage (logo[0]));

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
				brushes[i].Dispose ();
			brushes.Clear ();

			for (int i = 0; i < gr.Count; i++)
				gr[i].Dispose ();
			gr.Clear ();

			for (int i = 0; i < logo.Count; i++)
				logo[i].Dispose ();
			logo.Clear ();

			if (mainLayer != null)
				mainLayer.Dispose ();
			if (cdp != null)
				cdp.Dispose ();

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
			// Простой сброс логотипа
			if ((e.Button == MouseButtons.Left) && (e.Clicks == 2))
				{
				// Остановка отрисовки и сброс спектрограммы/гистограммы
				ExtendedTimer.Enabled = false;
				ConcurrentDrawLib.DestroySpectrogram ();	// Объединяет функционал

				// Перезапуск
				if (VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode) ==
					SpectrogramModes.NoSpectrogram)
					{
					// Перезаполнение палитры (без сброса поля отрисовки)
					ConcurrentDrawLib.FillPalette (cdp.PaletteNumber);
					}
				else
					{
					// Пересоздание спектрограммы / гистограммы
					ConcurrentDrawLib.InitializeSpectrogram ((uint)this.Width, cdp.SpectrogramHeight,
						cdp.PaletteNumber, VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode));
					}

				brushes[1].Color = ConcurrentDrawLib.GetMasterPaletteColor ();
				ResetLogo ();
				ExtendedTimer.Enabled = true;

				return;
				}

			// Реинициализация окна
			if (e.Button != MouseButtons.Right)
				return;

			do
				{
				// Остановка отрисовки и сброс слоя
				ExtendedTimer.Enabled = false;
				ConcurrentDrawLib.DestroySoundStream ();	// Объединяет функционал

				if (mainLayer != null)
					mainLayer.Dispose ();
				if (gr[0] != null)
					{
					gr[0].Dispose ();
					gr.RemoveAt (0);
					}
				this.TopMost = false;						// Разрешает отображение окна параметров

				// Перезапрос параметров
				cdp.ShowDialog ();

				// Переопределение размера окна
				this.Width = (int)cdp.VisualizationWidth;
				this.Height = (int)cdp.VisualizationHeight;
				this.Left = (int)cdp.VisualizationLeft;
				this.Top = (int)cdp.VisualizationTop;
				this.TopMost = cdp.AlwaysOnTop;
				} while (InitializeAudioStream () != 0);

			// Пересоздание кисти лого и поля отрисовки
			brushes[1].Color = ConcurrentDrawLib.GetMasterPaletteColor ();
			mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);
			mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, this.Width, this.Height);
			gr.Insert (0, Graphics.FromHwnd (this.Handle));

			// Реинициализация лого (при необходимости)
			if (cdp.ReselLogo)
				ResetLogo ();

			// Перезапуск
			ExtendedTimer.Enabled = true;
			}

		// Метод реинициализирует лого, вызывая его повторную отрисовку
		private void ResetLogo ()
			{
			// Сброс дескрипторов
			if (gr.Count > 1)
				{
				gr[1].Dispose ();
				gr.RemoveAt (1);
				}

			for (int i = 0; i < logo.Count; i++)
				logo[i].Dispose ();
			logo.Clear ();

			currentHistogramArc = 0;

			// Установка главного расчётного размера
			logoHeight = (uint)(Math.Min (this.Width, this.Height) * cdp.LogoHeight);

			// Перезапуск алгоритма таймера
			currentPhase = Phases.LayersPrecache;
			}
		}
	}
