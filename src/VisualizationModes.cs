namespace RD_AAOW
	{
	/// <summary>
	/// Варианты работы визуализатора
	/// </summary>
	public enum VisualizationModes
		{
		/// <summary>
		/// Только лого 
		/// </summary>
		Logo_only = 0,

		/// <summary>
		/// Статичная спектрограмма с курсором
		/// </summary>
		Static_spectrogram,

		/// <summary>
		/// Движущаяся спектрограмма
		/// </summary>
		Moving_spectrogram,

		/// <summary>
		/// Симметричная движущаяся спектрограмма
		/// </summary>
		Symmetric_moving_spectrogram,

		/// <summary>
		/// Гистограмма без симметрии
		/// </summary>
		Histogram_with_no_symmetry,

		/// <summary>
		/// Гистограмма с горизонтальной симметрией
		/// </summary>
		Histogram_with_horizontal_symmetry,

		/// <summary>
		/// Гистограмма с вертикальной симметрией
		/// </summary>
		Histogram_with_vertical_symmetry,

		/// <summary>
		/// Гистограмма с полной симметрией
		/// </summary>
		Histogram_with_full_symmetry,

		/// <summary>
		/// Гистограмма-бабочка с полной симметрией
		/// </summary>
		Butterfly_histogram_with_full_symmetry,

		/// <summary>
		/// Гистограмма-бабочка с вертикальной симметрией
		/// </summary>
		Butterfly_histogram_with_vertical_symmetry,

		/// <summary>
		/// Гистограмма-бабочка без симметрии
		/// </summary>
		Snail_histogram,

		/// <summary>
		/// Гистограмма-перспектива
		/// </summary>
		Perspective_histogram,

		/// <summary>
		/// Статичная амплитудная гистограмма с курсором
		/// </summary>
		Static_amplitude,

		/// <summary>
		/// Движущаяся амплитудная гистограмма
		/// </summary>
		Moving_amplitude
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Количество доступных режимов визуализации
		/// </summary>
		public const uint VisualizationModesCount = 12;

		/// <summary>
		/// Метод возвращает режим спектрограммы по режиму визуализации
		/// </summary>
		/// <param name="Mode">Режим для преобразования</param>
		/// <returns>Возвращает режим спектрограммы</returns>
		public static SpectrogramModes VisualizationModeToSpectrogramMode (VisualizationModes Mode)
			{
			switch (Mode)
				{
				case VisualizationModes.Static_spectrogram:
					return SpectrogramModes.StaticSpectrogram;

				case VisualizationModes.Moving_spectrogram:
					return SpectrogramModes.MovingSpectrogram;

				case VisualizationModes.Symmetric_moving_spectrogram:
					return SpectrogramModes.SymmetricMovingSpectrogram;

				case VisualizationModes.Histogram_with_no_symmetry:
					return SpectrogramModes.HistogramWithNoSymmetry;

				case VisualizationModes.Histogram_with_horizontal_symmetry:
					return SpectrogramModes.HistogramWithHorizontalSymmetry;

				case VisualizationModes.Histogram_with_vertical_symmetry:
					return SpectrogramModes.HistogramWithVerticalSymmetry;

				case VisualizationModes.Histogram_with_full_symmetry:
					return SpectrogramModes.HistogramWithFullSymmetry;

				case VisualizationModes.Static_amplitude:
					return SpectrogramModes.StaticAmplitude;

				case VisualizationModes.Moving_amplitude:
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
				(VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.StaticSpectrogram) ||
				(VisualizationModeToSpectrogramMode (Mode) == SpectrogramModes.SymmetricMovingSpectrogram);
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
		/// <returns>Возвращает true в случае, если предполагается гистограмма-бабочка или её аналог</returns>
		public static bool IsButterflyOrSnail (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Butterfly_histogram_with_full_symmetry) ||
				(Mode == VisualizationModes.Butterfly_histogram_with_vertical_symmetry) ||
				(Mode == VisualizationModes.Snail_histogram);
			}

		/// <summary>
		/// Метод возвращает true, если режим предполагает гистограмму-перспективу
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если предполагается гистограмма-перспектива</returns>
		public static bool IsPerspective (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Perspective_histogram);
			}
		}
	}
