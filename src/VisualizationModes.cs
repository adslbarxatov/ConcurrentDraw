namespace ESHQSetupStub
	{
	/// <summary>
	/// Варианты работы визуализатора
	/// </summary>
	public enum VisualizationModes
		{
		/// <summary>
		/// Статическая спектрограмма с курсором, с лого
		/// </summary>
		Static_spectrogram_with_logo = 0,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, с лого
		/// </summary>
		Moving_spectrogram_with_logo = 1,

		/// <summary>
		/// Гистограмма, с лого
		/// </summary>
		Histogram_with_logo = 2,

		/// <summary>
		/// Гистограмма «бабочка» со встроенным лого
		/// </summary>
		Butterfly_histogram_with_logo = 3,

		/// <summary>
		/// Статическая спектрограмма с курсором, без лого
		/// </summary>
		Static_spectrogram = 4,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, без лого
		/// </summary>
		Moving_spectrogram = 5,

		/// <summary>
		/// Гистограмма, без лого
		/// </summary>
		Histogram = 6,

		/// <summary>
		/// Только логотип с реакцией на частотные маркеры
		/// </summary>
		Logo_only = 7,
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Количество доступных режимов визуализации
		/// </summary>
		public const uint VisualizationModesCount = 4;

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки лого
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если лого необходимо</returns>
		public static bool ContainsLogo (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.Logo_only) ||
				(Mode == VisualizationModes.Moving_spectrogram_with_logo) ||
				(Mode == VisualizationModes.Static_spectrogram_with_logo) ||
				(Mode == VisualizationModes.Histogram_with_logo) ||
				(Mode == VisualizationModes.Butterfly_histogram_with_logo);
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
				case VisualizationModes.Histogram:
				case VisualizationModes.Histogram_with_logo:
					return SpectrogramModes.Histogram;

				default:
					return SpectrogramModes.NoSpectrogram;

				case VisualizationModes.Moving_spectrogram:
				case VisualizationModes.Moving_spectrogram_with_logo:
					return SpectrogramModes.MovingSpectrogram;

				case VisualizationModes.Static_spectrogram:
				case VisualizationModes.Static_spectrogram_with_logo:
					return SpectrogramModes.StaticSpectrogram;
				}
			}
		}
	}
