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
		Static_spectrogram = 0,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, с лого
		/// </summary>
		Moving_spectrogram = 1,

		/// <summary>
		/// Гистограмма, с лого
		/// </summary>
		Histogram = 2,

		/// <summary>
		/// Симметричная гистограмма, с лого
		/// </summary>
		Symmetric_histogram = 3,

		/// <summary>
		/// Гистограмма «бабочка» со встроенным лого
		/// </summary>
		Butterfly_histogram = 4,

		/*
		/// <summary>
		/// Статическая спектрограмма с курсором, без лого
		/// </summary>
		Static_spectrogram_without_logo = 5,

		/// <summary>
		/// Движущаяся спектрограмма с курсором, без лого
		/// </summary>
		Moving_spectrogram_without_logo = 6,

		/// <summary>
		/// Гистограмма, без лого
		/// </summary>
		Histogram_without_logo = 7,

		/// <summary>
		/// Симметричная гистограмма, без лого
		/// </summary>
		Symmetric_histogram_without_logo = 8,

		/// <summary>
		/// Только логотип с реакцией на частотные маркеры
		/// </summary>
		Logo_only = 9
		*/
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Количество доступных режимов визуализации
		/// </summary>
		public const uint VisualizationModesCount = 5;

		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки лого
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если лого необходимо</returns>
		public static bool ContainsLogo (VisualizationModes Mode)
			{
			return //(Mode == VisualizationModes.Logo_only) ||
				(Mode == VisualizationModes.Moving_spectrogram) ||
				(Mode == VisualizationModes.Static_spectrogram) ||
				(Mode == VisualizationModes.Histogram) ||
				(Mode == VisualizationModes.Symmetric_histogram) ||
				(Mode == VisualizationModes.Butterfly_histogram);
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
				//case VisualizationModes.Histogram_without_logo:
				case VisualizationModes.Histogram:
					return SpectrogramModes.Histogram;

				default:
					return SpectrogramModes.NoSpectrogram;

				//case VisualizationModes.Moving_spectrogram_without_logo:
				case VisualizationModes.Moving_spectrogram:
					return SpectrogramModes.MovingSpectrogram;

				//case VisualizationModes.Static_spectrogram_without_logo:
				case VisualizationModes.Static_spectrogram:
					return SpectrogramModes.StaticSpectrogram;

				//case VisualizationModes.Symmetric_histogram_without_logo:
				case VisualizationModes.Symmetric_histogram:
					return SpectrogramModes.SymmetricHistogram;
				}
			}
		}
	}
