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
			SupportedLanguages al = Localization.CurrentLanguage;
			if (!Localization.IsXPRClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsThisInstanceUnique (al == SupportedLanguages.ru_ru))
				return;

			// Проверка наличия обязательных компонентов
			for (int i = 0; i < ProgramDescription.AssemblyRequirements.Length; i++)
				if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyRequirements[i]))
					{
					if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
						ProgramDescription.AssemblyRequirements[i]) +
						Localization.GetText ("LibraryNotFound_Lib" + i.ToString (), al),
						ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) ==
						DialogResult.Yes)
						{
						AboutForm af = new AboutForm (i == 0 ? null : "http://un4seen.com");
						}

					return;
					}

			/*if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyRequirements[1]))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
					ProgramDescription.AssemblyRequirements[1]) + Localization.GetText ("LibraryNotFound_Lib1", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) ==
					DialogResult.Yes)
					{
					AboutForm af = new AboutForm ("http://un4seen.com");
					}
				return;
				}*/

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyLibVersion)
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryIsIncompatible", al),
					ProgramDescription.AssemblyRequirements[0], "(" + ConcurrentDrawLib.CDLibVersion + ") ",
					" (" + ProgramDescription.AssemblyLibVersion + ")") +
					Localization.GetText ("LibraryNotFound_Lib0", al), ProgramDescription.AssemblyTitle,
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
					AboutForm af = new AboutForm (null);
					}

				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!ProgramDescription.AcceptEULA ())
				return;
			ProgramDescription.ShowAbout (true);

			// Запуск
			Application.Run (new ConcurrentDrawForm ());
			}
		}
	}
