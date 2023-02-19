using System;
using System.IO;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает основное приложение
	/// </summary>
	public class ConcurrentDrawProgram
		{
		/// <summary>
		/// Точка входа программы
		/// </summary>
		[STAThread]
		public static void Main ()
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			// Язык интерфейса и контроль XPR
			/*SupportedLanguages al = Localization.CurrentLanguage;*/
			if (!Localization.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsThisInstanceUnique (Localization.IsCurrentLanguageRuRu))
				return;

			// Проверка наличия обязательных компонентов
			for (int i = 0; i < ProgramDescription.AssemblyRequirements.Length; i++)
				if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyRequirements[i]))
					{
					if (RDGenerics.MessageBox (RDMessageTypes.Question,
						string.Format (Localization.GetText ("LibraryNotFound"),
						ProgramDescription.AssemblyRequirements[i]) +
						Localization.GetText ("LibraryNotFound_Lib" + i.ToString ()),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.Yes),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.No)) ==
						RDMessageButtons.ButtonOne)
						{
						AboutForm af = new AboutForm (i == 0 ? null : "http://un4seen.com");
						}

					return;
					}

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyLibVersion)
				{
				if (RDGenerics.MessageBox (RDMessageTypes.Question,
					string.Format (Localization.GetText ("LibraryIsIncompatible"),
					ProgramDescription.AssemblyRequirements[0], "(" + ConcurrentDrawLib.CDLibVersion + ") ",
					" (" + ProgramDescription.AssemblyLibVersion + ")") +
					Localization.GetText ("LibraryNotFound_Lib0"),
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Yes),
					Localization.GetDefaultButtonName (Localization.DefaultButtons.No)) ==
					RDMessageButtons.ButtonOne)
					{
					AboutForm af = new AboutForm (null);
					}

				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDGenerics.AcceptEULA ())
				return;
			RDGenerics.ShowAbout (true);

			// Запуск
			Application.Run (new ConcurrentDrawForm ());
			}
		}
	}
