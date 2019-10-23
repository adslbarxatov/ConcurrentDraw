using Microsoft.Win32;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Поддерживаемые языки приложения
	/// </summary>
	public enum SupportedLanguages
		{
		/// <summary>
		/// Русский
		/// </summary>
		ru_ru,

		/// <summary>
		/// Английский (США)
		/// </summary>
		en_us
		}

	/// <summary>
	/// Класс обеспечивает доступ к языковым настройкам приложения
	/// </summary>
	public static class Localization
		{
		// Название параметра
		private const string LanguageValueName = "Language";

		/// <summary>
		/// Количество доступных языков интерфейса
		/// </summary>
		public const uint AvailableLanguages = 2;

		/// <summary>
		/// Возвращает или задаёт текущий язык интерфейса приложения
		/// </summary>
		public static SupportedLanguages CurrentLanguage
			{
			// Запрос
			get
				{
				// Получение значения
				string lang = "";
				try
					{
					lang = Registry.GetValue (ConcurrentDrawParameters.SettingsKey,
						LanguageValueName, "").ToString ();	// Вызовет исключение при отсутствии ключа
					}
				catch
					{
					}

				// Определение
				switch (lang)
					{
					case "ru_ru":
						return SupportedLanguages.ru_ru;

					default:
						return SupportedLanguages.en_us;
					}
				}

			// Установка
			set
				{
				try
					{
					Registry.SetValue (ConcurrentDrawParameters.SettingsKey,
						LanguageValueName, value.ToString ());	// Вызовет исключение, если раздел не удалось создать
					}
				catch
					{
					}
				}
			}

		/// <summary>
		/// Метод возвращает локализованный текст по указанному идентификатору
		/// </summary>
		/// <param name="TextName">Идентификатор текстового фрагмента</param>
		/// <param name="Language">Требуемый язык локализации</param>
		/// <returns>Локализованный текстовый фрагмент</returns>
		public static string GetText (string TextName, SupportedLanguages Language)
			{
			switch (Language)
				{
				default:
					return ConcurrentDraw_en_us.ResourceManager.GetString (TextName);

				case SupportedLanguages.ru_ru:
					return ConcurrentDraw_ru_ru.ResourceManager.GetString (TextName);
				}
			}
		}
	}
