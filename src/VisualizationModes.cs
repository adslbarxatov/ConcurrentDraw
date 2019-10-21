namespace ESHQSetupStub
	{
	/// <summary>
	/// Варианты работы визуализатора
	/// </summary>
	public enum VisualizationModes
		{
		/// <summary>
		/// Только логотип с реакцией на частотные маркеры
		/// </summary>
		LogoOnly = 0,

		/// <summary>
		/// Статическая спектрограмма с курсором, с лого
		/// </summary>
		StaticSpectrogramAndLogo = 1,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, с лого
		/// </summary>
		MovingSpectrogramAndLogo = 2,

		/// <summary>
		/// Статическая спектрограмма с курсором, без лого
		/// </summary>
		StaticSpectrogram = 3,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, без лого
		/// </summary>
		MovingSpectrogram = 4,

		/// <summary>
		/// Гистограмма, с лого
		/// </summary>
		HistogramAndLogo = 5,

		/// <summary>
		/// Гистограмма, без лого
		/// </summary>
		Histogram = 6,

		/// <summary>
		/// Гистограмма «бабочка» со встроенным лого
		/// </summary>
		ButterflyHistogram = 7
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Количество доступных режимов визуализации
		/// </summary>
		public const uint VisualizationModesCount = 8;

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки лого
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если лого необходимо</returns>
		public static bool ContainsLogo (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.LogoOnly) ||
				(Mode == VisualizationModes.MovingSpectrogramAndLogo) ||
				(Mode == VisualizationModes.StaticSpectrogramAndLogo) ||
				(Mode == VisualizationModes.HistogramAndLogo) ||
				(Mode == VisualizationModes.ButterflyHistogram);
			}

		/// <summary>
		/// Метод возвращает режим спектрограммы по режиму визуализации
		/// </summary>
		/// <param name="Mode">Режим для преобразования</param>
		/// <returns>Возвращает режим спектрограммы</returns>
		public static ConcurrentDrawLib.SpectrogramModes VisualizationModeToSpectrogramMode (VisualizationModes Mode)
			{
			switch (Mode)
				{
				case VisualizationModes.Histogram:
				case VisualizationModes.HistogramAndLogo:
					return ConcurrentDrawLib.SpectrogramModes.Histogram;

				default:
					// case VisualizationModes.LogoOnly:
					// case VisualizationModes.ButterflyHistogram:
					return ConcurrentDrawLib.SpectrogramModes.NoSpectrogram;

				case VisualizationModes.MovingSpectrogram:
				case VisualizationModes.MovingSpectrogramAndLogo:
					return ConcurrentDrawLib.SpectrogramModes.MovingSpectrogram;

				case VisualizationModes.StaticSpectrogram:
				case VisualizationModes.StaticSpectrogramAndLogo:
					return ConcurrentDrawLib.SpectrogramModes.StaticSpectrogram;
				}
			}
		}
	}
