using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

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
		// Доступные фазы отрисовки
		private enum VisualizationPhases
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

		// Общие переменные и константы
		private VisualizationPhases currentPhase =
			VisualizationPhases.LayersPrecache;					// Текущая фаза отрисовки
		private bool logoFirstShowMade = false;					// Флаг, указывающий на выполненное первое отображение лого
		private uint steps = 0;									// Счётчик шагов отрисовки
		private Random rnd = new Random ();						// ГПСЧ
		private ConcurrentDrawParameters cdp;					// Параметры работы программы
		private const string screenshotsDir = "CDScreenshots";	// Папка для скриншотов

		// Графика
		private LogoDrawerLayer mainLayer;						// Базовый слой изображения
		private ColorMatrix[] colorMatrix =
			new ColorMatrix[3];									// Полупрозрачные цветовые матрицы для спектрограмм
		private ImageAttributes[] sgAttributes =
			new ImageAttributes[3];								// Атрибуты изображений спектрограмм

		private List<Graphics> gr = new List<Graphics> ();		// Объекты-отрисовщики
		private List<SolidBrush> brushes = new List<SolidBrush> ();
		private List<Bitmap> logo = new List<Bitmap> ();

		// Бит-детектор
		private const int logoIdleSpeed = 2;					// Наименьшая скорость вращения лого
#if VIDEO
		private const int logoSpeedImpulse = 65;				// Импульс скорости при пиковом значении
#else
		private const int logoSpeedImpulse = 50;
#endif
		private const float beatsDetAngleMultiplier =
			150.0f / logoSpeedImpulse;							// Множитель для расчёта угла дуги бит-детектора

		// Лого
		private int currentLogoAngleDelta = 0,					// Текущий угол приращения поворота лого
			currentLogoAngle = 0;								// Текущий угол поворота лого (для бит-детектора)
		private double currentHistogramAngle = 0.0;				// Текущий угол поворота гистограммы-бабочки
		private uint logoHeight,								// Текущий диаметр лого
			logoCenterX, logoCenterY;							// Текущие координаты центра лого
		private const int fillingOpacity = 15;					// Непрозрачность кумулятивного эффекта и эффекта fadeout
		private bool firstFilling = true;						// Флаг, указывающий на необходимость инициализации кисти фона

		// Кумулятивный эффект
		private byte peak;										// Пиковое значение для расчёта битовых порогов
		private uint cumulationCounter;							// Накопитель, обеспечивающий кумулятивный эффект
		private const uint cumulationDivisor = 100,				// Граница и масштаб накопителя
			cumulationLimit = 255 * cumulationDivisor;

		// Метрики гистограмм
		private int[] histoX = new int[4],
			histoY = new int[4];								// Координаты линий гистограмм
		private const double butterflyDensity = 2.75;			// Плотность гистограммы-бабочки
		private const double perspectiveDensity = 3.15;			// Плотность гистограммы-перспективы
		// (даёт полный угол чуть более 90°; 90° <=> 2.84; 80° <=> 3.2)

#if OBJECTS
		// Дополнительные графические объекты
		private List<ILogoDrawerObject> objects =
			new List<ILogoDrawerObject> ();						// Визуальные объекты
		private LogoDrawerObjectMetrics objectsMetrics;			// Метрики генерируемых объектов
		private LogoDrawerLayer objectsLayer;					// Слой визуальных объектов
#endif

		// Вспомогательные переменные
		private int rad, amp;
		private double angle1, angle2;
		private Bitmap firstBMP;
		private Pen p;

		// Субтитры
		private Font[] subtitlesFonts = new Font[2];			// Объекты поддержки текстовых подписей на рендере
		private SizeF[] subtitlesSizes = new SizeF[2];

#if !VIDEO
		private string hotKeyResultText = "";					// Замена субтитрам, позволяющая отображать
		private uint hotKeyResultTextShowCounter = 0;			// результат настройки горячими клавишами
		private const uint hotKeyResultCounterLimit = 100;
		private const int hotKeyTextFontNumber = 0;
#else
		private bool showSubtitlesNow = false;					// Флаг фазы отрисовки текстовых подписей

		// Видео
		private const double fps = 23.4375;						// Частота кадров видео 
		// определена по аудио как (48000 Hz * 2 ch * 16 bps) / (8 * sizeof (float) * 2048 fftv)

		private VideoManager vm = new VideoManager ();			// Видеофайл (балластная инициализация)
		private AudioManager amv;								// Аудиодорожка видео
		private const uint fadeOutLength = 40;					// Длина эффекта fade out в кадрах
		private Bitmap secondBMP;								// Отрисовочный кадр
		private List<Bitmap> backgrounds = new List<Bitmap> ();	// Фоновые фреймы (если представлены)
		private int backgroundsCounter = 0;						// Текущий фрейм видеофона

		private ParametersPicker pp =
			new ParametersPicker (false);						// Интерфейс запроса параметров рендеринга
#endif

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
			cumulationCounter = cdp.CumulationSpeed;

			// Подготовка к отрисовке
			mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);

			colorMatrix[0] = new ColorMatrix ();
			colorMatrix[0].Matrix33 = 0.9f;						// Спектрограмма
			colorMatrix[1] = new ColorMatrix ();
			colorMatrix[1].Matrix33 = 0.5f;						// Простая гистограмма
			colorMatrix[2] = new ColorMatrix ();
			colorMatrix[2].Matrix33 = fillingOpacity / 50.0f;	// Фоновое изображение

			for (int i = 0; i < colorMatrix.Length; i++)
				{
				sgAttributes[i] = new ImageAttributes ();
				sgAttributes[i].SetColorMatrix (colorMatrix[i], ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				}

			// Формирование кистей
			brushes.Add (new SolidBrush (ConcurrentDrawLib.GetColorFromPalette (0)));			// Фон
			brushes.Add (new SolidBrush (ConcurrentDrawLib.GetMasterPaletteColor ()));			// Лого и beat-детектор
			brushes.Add (new SolidBrush (Color.FromArgb (fillingOpacity, brushes[0].Color)));	// Fade out (переопределяется далее)

			// Начальная инициализация слоёв (первый кадр)
			this.BackColor = brushes[0].Color;
			mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, this.Width, this.Height);


#if VIDEO
			// Запрос параметров рендеринга
			this.TopMost = false;
			pp.ShowDialog ();

			// Настройка диалогов
			SFVideo.Title = "Select placement of new video file";
			SFVideo.Filter = "Audio-Video Interchange video format|*.avi";
			SFVideo.FileName = pp.SubtitlesStrings[1] + ".avi";

			OFAudio.Title = "Select audio file for rendering";
			OFAudio.Filter = "Windows PCM audio files|*.wav";

			OFBackground.Title = "Select background file for rendering";
			OFBackground.Filter = "Image file|*.png|Image files set, including this one|*.png|" + SFVideo.Filter;

			// Инициализация фона
			if (pp.LoadBackground && (OFBackground.ShowDialog () == DialogResult.OK))
				{
				switch (OFBackground.FilterIndex)
					{
					// Статичный фрейм
					case 1:
						try
							{
							secondBMP = (Bitmap)Bitmap.FromFile (OFBackground.FileName);
							backgrounds.Add ((Bitmap)secondBMP.Clone ());
							secondBMP.Dispose ();
							}
						catch
							{
							MessageBox.Show ("Failed to load background image", ProgramDescription.AssemblyTitle,
								 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							}
						break;

					// Набор статичных фреймов
					case 2:
						// Запрос списка
						string[] files = null;

						try
							{
							files = Directory.GetFiles (Path.GetDirectoryName (OFBackground.FileName),
								"*.png", SearchOption.TopDirectoryOnly);
							}
						catch
							{
							MessageBox.Show ("Failed to load background images set", ProgramDescription.AssemblyTitle,
								 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							break;
							}

						// Загрузка фреймов
						for (int i = 0; i < files.Length; i++)
							{
							try
								{
								secondBMP = (Bitmap)Bitmap.FromFile (files[i]);
								backgrounds.Add ((Bitmap)secondBMP.Clone ());
								secondBMP.Dispose ();
								}
							catch
								{
								// Пропускаем
								}
							}
						break;

					// Видеофайл
					case 3:
						vm = new VideoManager (OFBackground.FileName);

						if (!vm.IsOpened)
							{
							MessageBox.Show ("Failed to load background video", ProgramDescription.AssemblyTitle,
								 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							break;
							}

						// Считывание кадров
						for (uint i = 0; i < vm.FramesCount; i++)
							backgrounds.Add (vm.GetFrame (i));

						// Завершение
						vm.Dispose ();
						break;
					}
				}

			// Инициализация видеопотока
			if (pp.WriteFramesToAVI)
				{
				if ((SFVideo.ShowDialog () == DialogResult.OK) && (OFAudio.ShowDialog () == DialogResult.OK))
					{
					vm = new VideoManager (SFVideo.FileName, fps, mainLayer.Layer, true);

					if (!vm.IsCreated)
						{
						MessageBox.Show ("Failed to initialize AVI stream", ProgramDescription.AssemblyTitle,
							 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						this.Close ();
						return;
						}
					}
				else
					{
					this.Close ();
					return;
					}
				}
			else
				{
				if (OFAudio.ShowDialog () != DialogResult.OK)
					OFAudio.FileName = "";
				}

#endif

			// Запуск аудиозахвата
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
#if VIDEO
			if (vm.IsCreated)
				{
				secondBMP = new Bitmap (this.Width, this.Height);
				gr.Add (Graphics.FromImage (secondBMP));
				}
			else
#endif
				{
				gr.Add (Graphics.FromHwnd (this.Handle));
				}
				ResetLogo ();

				// Подготовка параметров
				subtitlesFonts[0] = new Font ("Arial Narrow", this.Width / 55, FontStyle.Bold);
				subtitlesFonts[1] = new Font ("Arial", this.Width / 45, FontStyle.Bold);

#if VIDEO
			for (int i = 0; i < pp.SubtitlesStrings.Length; i++)
				subtitlesSizes[i] = gr[0].MeasureString (pp.SubtitlesStrings[i], subtitlesFonts[i]);

			// Запуск рендеринга
			if (vm.IsCreated)
				{
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
			if (vm.IsCreated || (OFAudio.FileName != ""))
				ssie = ConcurrentDrawLib.InitializeSoundStream (OFAudio.FileName);
			else
#endif
			ssie = ConcurrentDrawLib.InitializeSoundStream (cdp.DeviceNumber);
			switch (ssie)
				{
				case SoundStreamInitializationErrors.BASS_ERROR_ALREADY:
				case SoundStreamInitializationErrors.BASS_ERROR_BUSY:
					err = Localization.GetText ("BASS_ERROR_BUSY", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_NOTAVAIL:
					err = Localization.GetText ("BASS_ERROR_NOTAVAIL", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DEVICE:
					err = Localization.GetText ("BASS_ERROR_DEVICE", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DRIVER:
					err = Localization.GetText ("BASS_ERROR_DRIVER", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_DX:
					err = Localization.GetText ("BASS_ERROR_DX", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FORMAT:
					err = Localization.GetText ("BASS_ERROR_FORMAT", cdp.CurrentInterfaceLanguage);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_MEM:
					err = Localization.GetText ("BASS_ERROR_MEM", cdp.CurrentInterfaceLanguage);
					break;

				default:
				case SoundStreamInitializationErrors.BASS_ERROR_INIT:
				case SoundStreamInitializationErrors.BASS_RecordAlreadyRunning:
					throw new Exception ("Application failure. Debug required at point 1");

				case SoundStreamInitializationErrors.BASS_ERROR_NO3D:
				case SoundStreamInitializationErrors.BASS_ERROR_UNKNOWN:
					// Возникает при выборе стереомикшера при включённом микрофоне (почему-то)
					err = Localization.GetText ("DeviceBehaviorIsInvalid", cdp.CurrentInterfaceLanguage);
					result = 1;		// Запросить настройку приложения
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_ILLPARAM:
				case SoundStreamInitializationErrors.BASS_ERROR_SPEAKER:
					throw new Exception ("Application failure. Debug required at point 2");

				case SoundStreamInitializationErrors.BASS_InvalidDLLVersion:
					err = string.Format (Localization.GetText ("LibraryIsIncompatible", cdp.CurrentInterfaceLanguage),
						ProgramDescription.AssemblyRequirements[1]);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FILEOPEN:
					err = string.Format (Localization.GetText ("BASS_ERROR_FILEOPEN", cdp.CurrentInterfaceLanguage),
						OFAudio.FileName);
					break;

				case SoundStreamInitializationErrors.BASS_ERROR_FILEFORM:
				case SoundStreamInitializationErrors.BASS_ERROR_CODEC:
					err = string.Format (Localization.GetText ("BASS_ERROR_CODEC", cdp.CurrentInterfaceLanguage),
						OFAudio.FileName);
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
			if (!VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode))
				{
				// Ручное заполнение палитры и выход
				ConcurrentDrawLib.FillPalette (cdp.PaletteNumber);
				return 0;
				}

			// Запуск спектрограммы, если требуется
			switch (ConcurrentDrawLib.InitializeSpectrogram ((uint)this.Width, cdp.SpectrogramHeight,
				cdp.PaletteNumber, VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode),
				cdp.SpectrogramDoubleWidth))
				{
				case SpectrogramInitializationErrors.InitOK:
					break;

				case SpectrogramInitializationErrors.NotEnoughMemory:
					err = Localization.GetText ("BASS_ERROR_MEM", cdp.CurrentInterfaceLanguage);
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
				case VisualizationPhases.LayersPrecache:
					PrepareLayers ();
					break;

				// Отрисовка фрагментов лого
				case VisualizationPhases.LogoInbound:
					DrawingLogo ();
					break;

				// Вращение лого
				case VisualizationPhases.LogoRotation:
					if (logoFirstShowMade)
						currentPhase = VisualizationPhases.PreVisualization;

					RotatingLogo ();
					break;

				// Спецкоманды
				case VisualizationPhases.PreVisualization:
					// Донастройка отрисовщика (если установлен флаг прозрачности)
					logo[0].MakeTransparent (ConcurrentDrawLib.GetColorFromPalette (0));
					gr[1].CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

					// Перекрытие остатка лого при его отсутствии
					if (!cdp.VisualizationContainsLogo &&
						VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode) &&
						(this.Height - cdp.SpectrogramHeight > 0))
						{
						mainLayer.Descriptor.FillRectangle (brushes[0], (this.Width - logo[1].Width) / 2, 0,
							logo[1].Width, this.Height - cdp.SpectrogramHeight);
						}

					logoFirstShowMade = true;
					currentLogoAngleDelta = -logoIdleSpeed;
					currentPhase++;
					break;

				// Основной режим
				case VisualizationPhases.Visualization:
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
			uint length = (uint)(ConcurrentDrawLib.ChannelLength * fps + 250);

			// Собственно, выполняемый процесс
			for (int i = 0; i < length; i++)
				{
				// Отрисовка
				ExtendedTimer_Tick (null, null);

				showSubtitlesNow = (i >= 325) && (i <= 475);

				// Возврат прогресса
				((BackgroundWorker)sender).ReportProgress ((int)HardWorkExecutor.ProgressBarSize * i / (int)length,
					"Rendered frames: " + i.ToString () + " out of " + length.ToString ());
				}

			// Fade out
			brushes[2] = new SolidBrush (Color.FromArgb (2 * fillingOpacity, brushes[0].Color));
			for (int i = 0; i < fadeOutLength; i++)
				{
				gr[0].FillRectangle (brushes[2], 0, 0, this.Width, this.Height);
				firstBMP = (Bitmap)secondBMP.Clone ();
				vm.AddFrame (firstBMP);
				firstBMP.Dispose ();

				// Возврат прогресса
				((BackgroundWorker)sender).ReportProgress ((int)HardWorkExecutor.ProgressBarSize * i / (int)fadeOutLength,
					"Fadeout frames: " + i.ToString () + " out of " + fadeOutLength.ToString ());
				}

			// Завершено
			e.Result = 0;
			}
#endif

#if OBJECTS
		// Отрисовка визуальных объектов
		private void DrawObjects ()
			{
			// Затенение предыдущих элементов
			if (!objectsMetrics.KeepTracks || (objectsLayer == null))
				{
				if (objectsLayer != null)
					objectsLayer.Dispose ();
				objectsLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);
				}

			// Отрисовка объектов со смещением
			uint cumulation;
			if (VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode))
				cumulation = 255;
			else
				cumulation = cumulationCounter / cumulationDivisor;

			for (int i = 0; i < objects.Count; i++)
				{
				objects[i].Move (objectsMetrics.Acceleration, objectsMetrics.Enlarging);

				if (!objects[i].IsInited)
					{
					objects[i].Dispose ();

					// Обновление зависимых параметров
					objectsMetrics.MaxSpeed = 3;
					objectsMetrics.MinSpeed = 1;
					objectsMetrics.MaxSize = 1;
					objectsMetrics.PolygonsSidesCount = (byte)rnd.Next (5, 8);

					switch (objectsMetrics.ObjectsType)
						{
						default:
						//case LogoDrawerObjectTypes.Pictures:
						//case LogoDrawerObjectTypes.RotatingPictures:
						case LogoDrawerObjectTypes.Spheres:
							objects[i] = new LogoDrawerSphere ((uint)this.Width, (uint)this.Height, rnd, objectsMetrics);
							break;

						case LogoDrawerObjectTypes.Polygons:
						case LogoDrawerObjectTypes.Stars:
						case LogoDrawerObjectTypes.RotatingPolygons:
						case LogoDrawerObjectTypes.RotatingStars:
							objects[i] = new LogoDrawerSquare ((uint)this.Width, (uint)this.Height, rnd, objectsMetrics);
							break;

						case LogoDrawerObjectTypes.Letters:
						case LogoDrawerObjectTypes.RotatingLetters:
							objects[i] = new LogoDrawerLetter ((uint)this.Width, (uint)this.Height, rnd, objectsMetrics);
							break;
						}
					}

				objectsLayer.Descriptor.DrawImage (objects[i].Image, objects[i].X - objects[i].Image.Width / 2,
					objects[i].Y - objects[i].Image.Height / 2);
				}
			}
#endif

		// Метод отрисовывает сформированный кадр
		private void DrawFrame ()
			{
			if (cdp.ShakeEffect)
				{
				amp = mainLayer.Layer.Width * peak / 0xA000;

				gr[0].DrawImage (mainLayer.Layer, mainLayer.Left + rnd.Next (-amp, amp),
					mainLayer.Top + rnd.Next (-amp, amp));
				}
			else
				{
				gr[0].DrawImage (mainLayer.Layer, mainLayer.Left, mainLayer.Top);
				}

			// Отрисовка
#if VIDEO
			if (vm.IsCreated)
				{
				firstBMP = (Bitmap)secondBMP.Clone ();
				vm.AddFrame (firstBMP);
				firstBMP.Dispose ();
				}
#endif
			}

		// Обновляет углы поворота лого
		private void UpdateAngles (bool PushBrakes)
			{
			// Обновление приращения угла
			if (peak > cdp.BeatsDetectorLowLevel)
				currentLogoAngleDelta = -logoSpeedImpulse;

			// Торможение вращения
			if (PushBrakes)
				currentLogoAngleDelta = (7 * currentLogoAngleDelta - logoIdleSpeed) / 8;

			// Обновление угла
			currentLogoAngle = (currentLogoAngle + currentLogoAngleDelta / 3) % 360;
			}

		// Отрисовывает лого
		private void RotateAndDrawLogo ()
			{
			// Отрисовка
			gr[1].RotateTransform (currentLogoAngleDelta);
			gr[1].DrawImage (logo[0], -3 * logoHeight / 5, -3 * logoHeight / 5);

			// Обработка режима "только лого"
			if (cdp.VisualizationMode == VisualizationModes.Logo_only)
				{
				// Ручной перебор амплитуд для поддержки бит-детектора
				for (uint i = 0; i < 256; i++)
					ConcurrentDrawLib.GetScaledAmplitude (i);
				}

			// Отрисовка лого
			mainLayer.Descriptor.DrawImage (logo[1], logoCenterX - logo[1].Width / 2, logoCenterY - logo[1].Height / 2);
			}

		// Первичное вращение лого
		private void RotatingLogo ()
			{
			// Отрисовка
			currentLogoAngleDelta = (int)(++steps);
			UpdateAngles (false);
			RotateAndDrawLogo ();

			if (steps >= 170)
				{
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
			mainLayer.Descriptor.DrawImage (logo[0], logoCenterX - logo[0].Width / 2,
				logoCenterY - logo[0].Height / 2);

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

		// Метод обрабатывает кумулятивный эффект и затенение изображения
		private void ApplyCumulativeEffect (bool MaxFilling)
			{
			// Обработка кумулятивного значения
			uint oldCC = cumulationCounter;

			if (cdp.DecumulationSpeed != cdp.CumulationSpeed)
				{
				if (cumulationCounter > cdp.DecumulationSpeed)
					cumulationCounter -= cdp.DecumulationSpeed;
				if ((peak > cdp.BeatsDetectorLowLevel) && (cumulationCounter < cumulationLimit))
					cumulationCounter += cdp.CumulationSpeed;
				}
			else if (cumulationCounter != 0)
				{
				cumulationCounter = 0;
				}

			if (((cumulationCounter / cumulationDivisor) != (oldCC / cumulationDivisor)) ||	// Целочисленное деление обязательно
				firstFilling)	// Отвечает за правильное применение фона при старте программы
				{
				brushes[2].Color = Color.FromArgb (fillingOpacity * (MaxFilling ? 6 : 1),
					ConcurrentDrawLib.GetMasterPaletteColor ((byte)(cumulationCounter / cumulationDivisor)));
				}

			// Затенение изображения / кумулятивный эффект
#if VIDEO
			if ((backgrounds.Count == 0) || firstFilling)
				{
#endif
			if (VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode))
				{
				if (this.Height - cdp.SpectrogramHeight > 0)
					mainLayer.Descriptor.FillRectangle (brushes[2], 0, 0, mainLayer.Layer.Width,
						mainLayer.Layer.Height - cdp.SpectrogramHeight);
				}
			else
				{
				mainLayer.Descriptor.FillRectangle (brushes[2], 0, 0, mainLayer.Layer.Width, mainLayer.Layer.Height);
				}
#if VIDEO
				}
			else
				{
				mainLayer.Descriptor.DrawImage (backgrounds[backgroundsCounter],
								new Rectangle (0, 0, this.Width, this.Height),
								0, 0, backgrounds[backgroundsCounter].Width, backgrounds[backgroundsCounter].Height,
								GraphicsUnit.Pixel, sgAttributes[2]);

				if (++backgroundsCounter >= backgrounds.Count)
					backgroundsCounter = 0;
				}
#endif

			// Завершение
			if (firstFilling)
				firstFilling = false;
			}

		// Метод отрисовывает гистограммы «бабочка» и «перспектива»
		private void DrawButterflyAndPerspective ()
			{
			// Затенение и кумулятивный эффект
			ApplyCumulativeEffect (VisualizationModesChecker.IsPerspective (cdp.VisualizationMode));

			// Обработка вращения гистограммы
			if (cdp.HistoRotAccordingToBeats)
				{
				if (currentLogoAngleDelta < -logoIdleSpeed)
					{
					currentHistogramAngle -= (cdp.HistoRotSpeedDelta * currentLogoAngleDelta / logoSpeedImpulse);
					}
				}
			else
				{
				currentHistogramAngle += cdp.HistoRotSpeedDelta;
				}

			if (currentHistogramAngle > 360.0)
				currentHistogramAngle -= 360.0;

			// Отрисовка
			if (VisualizationModesChecker.IsPerspective (cdp.VisualizationMode))
				rad = (int)Math.Sqrt (this.Width * this.Width + this.Height * this.Height) / 2;		// Радиус для перспективы

			for (int i = 0; i < 256; i++)
				{
				// Получаем амплитуду
				amp = ConcurrentDrawLib.GetScaledAmplitude ((uint)(cdp.HistogramFFTValuesCount * i) / 256);

				// Определяем координаты линий и создаём кисть
				if (VisualizationModesChecker.IsButterfly (cdp.VisualizationMode))
					{
					// Кисть
					p = new Pen (ConcurrentDrawLib.GetColorFromPalette ((byte)(4 * amp / 5)), logoHeight / 80);

					// Радиус и углы поворота по индексу и общему вращению
					rad = logo[1].Width / 2 + (int)((uint)(logo[1].Width * amp) / 256);
					angle1 = ArcToRad (i / butterflyDensity);
					angle2 = ArcToRad (currentHistogramAngle);
					if (cdp.SwingingHistogram)
						angle2 = Math.Sin (angle2) / 2.0;

					// Расчёт координат
					histoX[0] = (int)(logoCenterX + rad * Math.Cos (angle2 + angle1));
					histoX[1] = 2 * (int)logoCenterX - histoX[0];
					histoX[2] = (int)(logoCenterX + rad * Math.Cos (angle2 - angle1));
					histoX[3] = 2 * (int)logoCenterX - histoX[2];
					histoY[0] = (int)(logoCenterY + rad * Math.Sin (angle2 + angle1));
					histoY[1] = 2 * (int)logoCenterY - histoY[0];
					histoY[2] = (int)(logoCenterY + rad * Math.Sin (angle2 - angle1));
					histoY[3] = 2 * (int)logoCenterY - histoY[2];
					}
				else
					{
					// Кисть
					p = new Pen (Color.FromArgb (90, ConcurrentDrawLib.GetColorFromPalette ((byte)(3 * amp / 4))),
						logoHeight / ((i > 252) ? 90 : 40));	// Костыль для обхода шва на перспективе

					// Углы
					// (двойная длина дуги для того же количества линий)
					angle1 = ArcToRad (((255 - i) * ((i % 2 == 0) ? 1 : -1)) / perspectiveDensity);
					angle2 = ArcToRad (currentHistogramAngle + 90);
					if (cdp.SwingingHistogram)
						angle2 = (Math.Sin (angle2) + Math.PI) / 2.0;

					// Координаты
					histoX[0] = (int)(logoCenterX + rad * Math.Cos (angle2 + angle1));
					histoX[1] = (int)(logoCenterX + logoHeight * Math.Cos (angle2) / 20);
					histoX[2] = 2 * (int)logoCenterX - histoX[0];
					histoX[3] = 2 * (int)logoCenterX - histoX[1];
					histoY[0] = (int)(logoCenterY + rad * Math.Sin (angle2 + angle1));
					histoY[1] = (int)(logoCenterY + logoHeight * Math.Sin (angle2) / 20);
					histoY[2] = 2 * (int)logoCenterY - histoY[0];
					histoY[3] = 2 * (int)logoCenterY - histoY[1];
					}

				// Рисуем
				mainLayer.Descriptor.DrawLine (p, histoX[0], histoY[0], histoX[1], histoY[1]);
				mainLayer.Descriptor.DrawLine (p, histoX[2], histoY[2], histoX[3], histoY[3]);

				// Завершено
				p.Dispose ();
				}
			}

		// Отрисовка фрагментов лого
		private void DrawingVisualization ()
			{
#if VIDEO
			// Ручное обновление кадра при записи
			if (vm.IsCreated || (OFAudio.FileName != ""))
				ConcurrentDrawLib.UpdateFFTData ();
#endif

#if OBJECTS
			// Отрисовка объектов
			DrawObjects ();
			mainLayer.Descriptor.DrawImage (objectsLayer.Layer, 0, 0);
#endif

			// Запрос пикового значения 
			peak = ConcurrentDrawLib.CurrentPeak;

			// Отрисовка гистограммы-бабочки при необходимости
			if (VisualizationModesChecker.IsButterfly (cdp.VisualizationMode))
				DrawButterflyAndPerspective ();

			// Затенение и кумулятивный эффект для остальных режимов
			if (VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode) ||
				(cdp.VisualizationMode == VisualizationModes.Logo_only))
				ApplyCumulativeEffect (true);

			// Отрисовка лого при необходимости
			UpdateAngles (true);
			if (cdp.VisualizationContainsLogo)
				{
				// Лого
				RotateAndDrawLogo ();

				// Бит-детектор
				p = new Pen (ConcurrentDrawLib.GetMasterPaletteColor (peak), logoHeight / 50);
				rad = 400 * logo[1].Height / (1200 - peak);

				for (int i = 0; i < 2; i++)
					mainLayer.Descriptor.DrawArc (p, logoCenterX - rad / 2, logoCenterY - rad / 2,
						rad, rad, currentLogoAngle + i * 180, currentLogoAngleDelta * beatsDetAngleMultiplier);

				p.Dispose ();
				}

			// Отрисовка перспективы поверх лого
			if (VisualizationModesChecker.IsPerspective (cdp.VisualizationMode))
				DrawButterflyAndPerspective ();

			// Отрисовка спектрограммы при необходимости
			if (VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode))
				{
				// Получение текущего фрейма спектрограммы
				firstBMP = ConcurrentDrawLib.CurrentSpectrogramFrame;

				// Отрисовка фрейма
				if (VisualizationModesChecker.ContainsSGorWF (cdp.VisualizationMode))
					{
					mainLayer.Descriptor.DrawImage (firstBMP,
						new Rectangle (0, this.Height - firstBMP.Height, firstBMP.Width, firstBMP.Height),
						0, 0, firstBMP.Width, firstBMP.Height, GraphicsUnit.Pixel, sgAttributes[0]);
					}
				else
					{
					mainLayer.Descriptor.DrawImage (firstBMP,
						new Rectangle (0, this.Height - firstBMP.Height, firstBMP.Width, firstBMP.Height),
						0, 0, firstBMP.Width, firstBMP.Height, GraphicsUnit.Pixel, sgAttributes[1]);
					}

				firstBMP.Dispose ();
				}

			// Отрисовка текстовых подписей
#if VIDEO
			if (showSubtitlesNow)
				{
				if (pp.SubtitlesStrings[0] != "")
					mainLayer.Descriptor.DrawString (pp.SubtitlesStrings[0], subtitlesFonts[0], brushes[pp.BackgroundBrush ? 0 : 1],
						(pp.RightStringsAlignment ? (this.Width - subtitlesSizes[0].Width - 50) : 50),
						this.Height - subtitlesSizes[0].Height - subtitlesSizes[1].Height - 30);
				if (pp.SubtitlesStrings[1] != "")
					mainLayer.Descriptor.DrawString (pp.SubtitlesStrings[1], subtitlesFonts[1], brushes[pp.BackgroundBrush ? 0 : 1],
						(pp.RightStringsAlignment ? (this.Width - subtitlesSizes[1].Width - 50) : 50),
						this.Height - subtitlesSizes[1].Height - 30);
				}
#else
			if (hotKeyResultText != "")
				{
				mainLayer.Descriptor.DrawString (hotKeyResultText, subtitlesFonts[hotKeyTextFontNumber],
					brushes[1], this.Width - subtitlesSizes[hotKeyTextFontNumber].Width - 50,
					this.Height - subtitlesSizes[hotKeyTextFontNumber].Height - 30);

				if (hotKeyResultTextShowCounter++ > hotKeyResultCounterLimit)
					{
					hotKeyResultText = "";
					}
				}
#endif
			}

		// Метод пересчитывает градусы в радианы
		private double ArcToRad (double Arc)
			{
			return Math.PI * Arc / 180.0;
			}

		// Создание и подготовка слоёв и лого
		private void PrepareLayers ()
			{
			// Сброс главного слоя
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
			if (firstBMP != null)
				firstBMP.Dispose ();
			if (cdp != null)
				cdp.Dispose ();

#if OBJECTS
			if (objectsLayer != null)
				objectsLayer.Dispose ();
#endif

#if VIDEO
			if (secondBMP != null)
				secondBMP.Dispose ();

			for (int i = 0; i < backgrounds.Count; i++)
				backgrounds[i].Dispose ();
			backgrounds.Clear ();

			// Попытка добавления аудио
			amv = new AudioManager (SFVideo.FileName.Substring (0, SFVideo.FileName.Length - 4) + ".wav", false);
			if (amv.IsInited)
				{
				vm.AddAudio (amv);
				amv.Dispose ();
				}

			// Завершение
			vm.Dispose ();
#endif
			}

		// Принудительный выход (по любой клавише)
		private void LogoDrawer_KeyDown (object sender, KeyEventArgs e)
			{
#if !VIDEO
			switch (e.KeyCode)
				{
				// Закрытие окна
				case Keys.Escape:
					this.Close ();
					break;

				// Вызов настроек
				case Keys.Space:
					ChangeSettingsAndRestart (ChangeSettingsAndRestartModes.CallSettingsWindow, 0);
					break;

				// Реинициализация 
				case Keys.R:
					ChangeSettingsAndRestart (ChangeSettingsAndRestartModes.RestartDrawingOnly, 0);
					break;

				// Сохранение скриншота визуализации
				case Keys.S:
					mainLayer.Descriptor.DrawString (ProgramDescription.AssemblyTitle, subtitlesFonts[hotKeyTextFontNumber],
						brushes[1], 0, 0);

					try
						{
						if (!Directory.Exists (Application.StartupPath + "\\" + screenshotsDir))
							Directory.CreateDirectory (Application.StartupPath + "\\" + screenshotsDir);

						mainLayer.Layer.Save (Application.StartupPath + "\\" + screenshotsDir + "\\" +
							DateTime.Now.ToString ("dd-MM-yyyy_HH-mm-ss") + ".png", ImageFormat.Png);
						}
					catch
						{
						MessageBox.Show (Localization.GetText ("ScreenshotFailure", cdp.CurrentInterfaceLanguage),
							ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					break;

				// Другие настройки (с передачей результата выполнения из окна настроек)
				default:
					hotKeyResultText = ChangeSettingsAndRestart (ChangeSettingsAndRestartModes.SendHotKeyToSettingsWindow,
						e.KeyCode | e.Modifiers);
					break;
				}
#endif
			}

		// Вызов настроек
		private void ConcurrentDrawForm_MouseClick (object sender, MouseEventArgs e)
			{
#if !VIDEO
			// Простой сброс логотипа
			if ((e.Button == MouseButtons.Left) && (e.Clicks == 2))
				ChangeSettingsAndRestart (ChangeSettingsAndRestartModes.RestartDrawingOnly, 0);

			else if (e.Button == MouseButtons.Right)
				ChangeSettingsAndRestart (ChangeSettingsAndRestartModes.CallSettingsWindow, 0);
#endif
			}

#if !VIDEO
		// Режимы работы реинициализатора отрисовки
		private enum ChangeSettingsAndRestartModes
			{
			// Вызов окна настроек
			CallSettingsWindow,

			// Только реинициализация
			RestartDrawingOnly,

			// Отправка горячей клавиши в окно настроек
			SendHotKeyToSettingsWindow,

			// Подрежим, запускающийся в случае сбоя реинициализации после применения настроек
			SendHotKeyFailed
			}

		// Метод обрабатывает перезапуск отрисовки
		private string ChangeSettingsAndRestart (ChangeSettingsAndRestartModes Mode, Keys HotKey)
			{
			// Переменные
			ChangeSettingsAndRestartModes csarMode = Mode;
			string hotKeyResult = "";

			// Контроль
			if (((Mode == ChangeSettingsAndRestartModes.SendHotKeyToSettingsWindow) ||
				(Mode == ChangeSettingsAndRestartModes.SendHotKeyFailed)) &&
				!ConcurrentDrawParameters.IsHotKeyAllowed (HotKey))
				return hotKeyResult;

			// Остановка отрисовки и сброс слоя
			ExtendedTimer.Enabled = false;

			if (csarMode != ChangeSettingsAndRestartModes.RestartDrawingOnly)
				{
				ConcurrentDrawLib.DestroySoundStream ();	// Объединяет функционал

				if (csarMode == ChangeSettingsAndRestartModes.CallSettingsWindow)
					{
					if (mainLayer != null)
						mainLayer.Dispose ();

					if (gr[0] != null)
						{
						gr[0].Dispose ();
						gr.RemoveAt (0);
						}
					}
				}
			else
				{
				ConcurrentDrawLib.DestroySpectrogram ();
				}

			// Выбор варианта перезапуска
			if (csarMode == ChangeSettingsAndRestartModes.RestartDrawingOnly)
				{
				// Перезапуск
				if (!VisualizationModesChecker.ContainsSGHGorWF (cdp.VisualizationMode))
					{
					// Перезаполнение палитры (без сброса поля отрисовки)
					ConcurrentDrawLib.FillPalette (cdp.PaletteNumber);
					}
				else
					{
					// Пересоздание спектрограммы / гистограммы
					ConcurrentDrawLib.InitializeSpectrogram ((uint)this.Width, cdp.SpectrogramHeight,
						cdp.PaletteNumber, VisualizationModesChecker.VisualizationModeToSpectrogramMode (cdp.VisualizationMode),
						cdp.SpectrogramDoubleWidth);
					}
				}
			else
				{
				// Перезапрос параметров
				do
					{
					// Разрешает отображение окна параметров
					this.TopMost = false;

					// Изменение параметров
					if ((csarMode == ChangeSettingsAndRestartModes.CallSettingsWindow) ||
						(csarMode == ChangeSettingsAndRestartModes.SendHotKeyFailed))
						{
						cdp.ShowDialog ();
						}
					else
						{
						hotKeyResult = cdp.ProcessHotKey (HotKey);
						subtitlesSizes[hotKeyTextFontNumber] = gr[0].MeasureString (hotKeyResult,
							subtitlesFonts[hotKeyTextFontNumber]);
						hotKeyResultTextShowCounter = 0;

						csarMode = ChangeSettingsAndRestartModes.SendHotKeyFailed;	// Защита от зацикливания при сбоях
						}

					// Переопределение размера окна
					this.Width = (int)cdp.VisualizationWidth;
					this.Height = (int)cdp.VisualizationHeight;
					this.Left = (int)cdp.VisualizationLeft;
					this.Top = (int)cdp.VisualizationTop;
					this.TopMost = cdp.AlwaysOnTop;
					} while (InitializeAudioStream () != 0);
				}

			// Пересоздание кисти лого и поля отрисовки
			brushes[0].Color = ConcurrentDrawLib.GetColorFromPalette (0);
			brushes[1].Color = ConcurrentDrawLib.GetMasterPaletteColor ();
			if (csarMode == ChangeSettingsAndRestartModes.CallSettingsWindow)
				{
				mainLayer = new LogoDrawerLayer (0, 0, (uint)this.Width, (uint)this.Height);
				mainLayer.Descriptor.FillRectangle (brushes[0], 0, 0, this.Width, this.Height);
				gr.Insert (0, Graphics.FromHwnd (this.Handle));
				}

			// Реинициализация лого (при необходимости)
			if (cdp.ReselLogo || (csarMode == ChangeSettingsAndRestartModes.RestartDrawingOnly))
				ResetLogo ();

			// Перезапуск
			ExtendedTimer.Enabled = true;
			return hotKeyResult;
			}
#endif

		// Метод реинициализирует лого, вызывая его повторную отрисовку
		private void ResetLogo ()
			{
			// Сброс дескрипторов
			if (gr.Count > 1)
				{
				gr[1].Dispose ();
				gr.RemoveAt (1);
				}

#if OBJECTS
			for (int i = 0; i < objects.Count; i++)
				objects[i].Dispose ();
			objects.Clear ();
#endif

			for (int i = 0; i < logo.Count; i++)
				logo[i].Dispose ();
			logo.Clear ();

			currentHistogramAngle = 0;

			// Установка главного расчётного размера и координат лого
			logoHeight = (uint)(Math.Min (this.Width, this.Height) * cdp.LogoHeight);
			logoCenterX = (uint)(this.Width * cdp.LogoCenterX);
			logoCenterY = (uint)(this.Height * cdp.LogoCenterY);

			// Перезапуск алгоритма таймера
			currentPhase = VisualizationPhases.LayersPrecache;

#if OBJECTS
			// Обновление метрик графических объектов
			objectsMetrics.Acceleration = true;
			objectsMetrics.AsStars = false;
			objectsMetrics.Enlarging = 2;
			objectsMetrics.KeepTracks = false;
			objectsMetrics.MaxRed = ConcurrentDrawLib.GetColorFromPalette (255).R;
			objectsMetrics.MaxGreen = ConcurrentDrawLib.GetColorFromPalette (255).G;
			objectsMetrics.MaxBlue = ConcurrentDrawLib.GetColorFromPalette (255).B;
			objectsMetrics.MinRed = ConcurrentDrawLib.GetColorFromPalette (192).R;
			objectsMetrics.MinGreen = ConcurrentDrawLib.GetColorFromPalette (192).G;
			objectsMetrics.MinBlue = ConcurrentDrawLib.GetColorFromPalette (192).B;
			objectsMetrics.MinSize = 1;
			objectsMetrics.ObjectsCount = 4;
			objectsMetrics.ObjectsType = LogoDrawerObjectTypes.Spheres;
			objectsMetrics.Rotation = false;
			objectsMetrics.StartupPosition = LogoDrawerObjectStartupPositions.CenterRandom ;
			objectsMetrics.MaxSpeedFluctuation = 0;

			for (int i = 0; i < objectsMetrics.ObjectsCount; i++)
				objects.Add (new LogoDrawerLetter (0, 0, null, objectsMetrics));
#endif
			}
		}
	}
