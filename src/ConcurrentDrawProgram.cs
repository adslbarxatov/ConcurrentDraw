using System;
using System.IO;
using System.Threading;
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

			// Запрос языка приложения
			SupportedLanguages al = Localization.CurrentLanguage;

			// Проверка запуска единственной копии
			bool result;
			Mutex instance = new Mutex (true, ProgramDescription.AssemblyTitle, out result);
			if (!result)
				{
				MessageBox.Show (string.Format (Localization.GetText ("AlreadyStarted", al), ProgramDescription.AssemblyTitle),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Проверка наличия обязательных компонентов
			if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyRequirements[0]))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
					ProgramDescription.AssemblyRequirements[0]) + Localization.GetText ("LibraryNotFound_Lib0", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
					AboutForm af = new AboutForm (null);
					}
				return;
				}

			if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.AssemblyRequirements[1]))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
					ProgramDescription.AssemblyRequirements[1]) + Localization.GetText ("LibraryNotFound_Lib1", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					{
					AboutForm af = new AboutForm ("http://www.un4seen.com");
					}
				return;
				}

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyLibVersion)
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryIsIncompatible", al),
						ProgramDescription.AssemblyRequirements[0], ConcurrentDrawLib.CDLibVersion,
						ProgramDescription.AssemblyLibVersion) +
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
