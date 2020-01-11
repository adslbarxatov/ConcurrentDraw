namespace ESHQSetupStub
	{
	/// <summary>
	/// Варианты работы визуализатора
	/// </summary>
	public enum VisualizationModes
		{
		/// <summary>
		/// Статичная спектрограмма с курсором со встроенным лого
		/// </summary>
		Static_spectrogram = 0,

		/// <summary>
		/// Движущаяся спектрограмма со встроенным лого
		/// </summary>
		Moving_spectrogram = 1,

		/// <summary>
		/// Гистограмма со встроенным лого
		/// </summary>
		Histogram = 2,

		/// <summary>
		/// Симметричная гистограмма со встроенным лого
		/// </summary>
		Symmetric_histogram = 3,

		/// <summary>
		/// Гистограмма «бабочка» со встроенным лого
		/// </summary>
		Butterfly_histogram = 4,

		/// <summary>
		/// Гистограмма-перспектива со встроенным лого
		/// </summary>
		Perspective_histogram = 5,

		/// <summary>
		/// Статичная амплитудная с курсором со встроенным лого
		/// </summary>
		Static_amplitude = 6,

		/// <summary>
		/// Движущаяся амплитудная со встроенным лого
		/// </summary>
		Moving_amplitude = 7,

		/// <summary>
		/// Статичная спектрограмма с курсором без лого
		/// </summary>
		Static_spectrogram_without_logo = 8,

		/// <summary>
		/// Движущаяся спектрограмма без лого
		/// </summary>
		Moving_spectrogram_without_logo = 9,

		/// <summary>
		/// Гистограмма без лого
		/// </summary>
		Histogram_without_logo = 10,

		/// <summary>
		/// Симметричная гистограмма без лого
		/// </summary>
		Symmetric_histogram_without_logo = 11,

		/// <summary>
		/// Гистограмма «бабочка» без лого
		/// </summary>
		Butterfly_histogram_without_logo = 12,

		/// <summary>
		/// Гистограмма-перспектива без лого
		/// </summary>
		Perspective_histogram_without_logo = 13,

		/// <summary>
		/// Статичная амплитудная с курсором без лого
		/// </summary>
		Static_amplitude_without_logo = 14,

		/// <summary>
		/// Движущаяся амплитудная без лого
		/// </summary>
		Moving_amplitude_without_logo = 15,

		/// <summary>
		/// Только лого 
		/// </summary>
		Logo_only = 16
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Количество доступных режимов визуализации
		/// </summary>
#if ALL_MODES
		public const uint VisualizationModesCount = 17;
#else
		public const uint VisualizationModesCount = 8;
#endif

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки лого
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если лого необходимо</returns>
		public static bool ContainsLogo (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Logo_only) ||
				(Mode <= VisualizationModes.Moving_amplitude);
			}

		/// <summary>
		/// Метод возвращает режим спектрограммы по режиму визуализации
		/// </summary>
		/// <param name="Mode">Режим для преобразования</param>
		/// <returns>Возвращает режим спектрограммы</returns>
		public static SpectrogramModes VisualizationModeToSpectrogramMode (VisualizationModes Mode)
			{
			switch (Mode)
				{
				case VisualizationModes.Static_spectrogram_without_logo:
				case VisualizationModes.Static_spectrogram:
					return SpectrogramModes.StaticSpectrogram;

				case VisualizationModes.Moving_spectrogram_without_logo:
				case VisualizationModes.Moving_spectrogram:
					return SpectrogramModes.MovingSpectrogram;

				case VisualizationModes.Histogram_without_logo:
				case VisualizationModes.Histogram:
					return SpectrogramModes.Histogram;

				case VisualizationModes.Symmetric_histogram_without_logo:
				case VisualizationModes.Symmetric_histogram:
					return SpectrogramModes.SymmetricHistogram;

				case VisualizationModes.Static_amplitude:
				case VisualizationModes.Static_amplitude_without_logo:
					return SpectrogramModes.StaticAmplitude;

				case VisualizationModes.Moving_amplitude:
				case VisualizationModes.Moving_amplitude_without_logo:
					return SpectrogramModes.MovingAmplitude;

				default:
					return SpectrogramModes.NoSpectrogram;
				}
			}

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки спектрограммы, гистограммы или волновые формы
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если спектрограмма необходима</returns>
		public static bool ContainsSGHGorWF (VisualizationModes Mode)
			{
			return VisualizationModeToSpectrogramMode (Mode) != SpectrogramModes.NoSpectrogram;
			}

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки только спектрограммы
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если спектрограмма необходима</returns>
		public static bool ContainsSGonly (VisualizationModes Mode)
			{
			return (VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.MovingSpectrogram) ||
				(VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.StaticSpectrogram);
			}

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки только спектрограммы
		/// или амплитудной гистограммы
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если спектрограмма необходима</returns>
		public static bool ContainsSGorWF (VisualizationModes Mode)
			{
			return ContainsSGonly (Mode) ||
				(VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.StaticAmplitude) ||
				(VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.MovingAmplitude);
			}

		/// <summary>
		/// Метод возвращает true, если режим предполагает гистограмму-бабочку
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если предполагается гистограмма-бабочка</returns>
		public static bool IsButterfly (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Butterfly_histogram) ||
				(Mode == VisualizationModes.Butterfly_histogram_without_logo);
			}

		/// <summary>
		/// Метод возвращает true, если режим предполагает гистограмму-перспективу
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если предполагается гистограмма-перспектива</returns>
		public static bool IsPerspective (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Perspective_histogram) ||
				(Mode == VisualizationModes.Perspective_histogram_without_logo);
			}
		}
	}
