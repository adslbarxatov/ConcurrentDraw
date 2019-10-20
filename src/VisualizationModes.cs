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
		}

	/// <summary>
	/// Класс описывает вспомогательные методы для обработки режимов работы
	/// </summary>
	public static class VisualizationModesChecker
		{
		/// <summary>
		/// Метод проверяет, требует ли указанный режим отрисовки лого
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если лого необходимо</returns>
		public static bool ContainsLogo (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.LogoOnly) ||
				(Mode == VisualizationModes.MovingSpectrogramAndLogo) ||
				(Mode == VisualizationModes.StaticSpectrogramAndLogo);
			}

		/// <summary>
		/// Метод проверяет, предполагает ли указанный режим движущуюся спектрограмму
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если предполагается движущаяся спектрограмма</returns>
		public static bool ContainsMovingSpectrogram (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.MovingSpectrogram) ||
				(Mode == VisualizationModes.MovingSpectrogramAndLogo);
			}

		/// <summary>
		/// Метод проверяет, предполагает ли указанный режим спектрограмму
		/// </summary>
		/// <param name="Mode">Режим для проверки</param>
		/// <returns>Возвращает true в случае, если предполагается спектрограмма</returns>
		public static bool ContainsSpectrogram (VisualizationModes Mode)
			{
			return (Mode == VisualizationModes.MovingSpectrogram) ||
				(Mode == VisualizationModes.MovingSpectrogramAndLogo) ||
				(Mode == VisualizationModes.StaticSpectrogram) ||
				(Mode == VisualizationModes.StaticSpectrogramAndLogo);
			}
		}
	}
