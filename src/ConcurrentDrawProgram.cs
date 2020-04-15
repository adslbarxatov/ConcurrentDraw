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
		/// <param name="args">Аргументы командной строки</param>
		[STAThread]
		public static void Main (string[] args)
			{
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
			if (!File.Exists (Application.StartupPath + "\\" + ProgramDescription.AssemblyRequirements[0]))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
					ProgramDescription.AssemblyRequirements[0]) + Localization.GetText ("LibraryNotFound_Lib0", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					ProgramDescription.ShowActualAssemblyPage ();
				return;
				}

			if (!File.Exists (Application.StartupPath + "\\" + ProgramDescription.AssemblyRequirements[1]))
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryNotFound", al),
					ProgramDescription.AssemblyRequirements[1]) + Localization.GetText ("LibraryNotFound_Lib1", al),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					ProgramDescription.ShowBASSPage ();
				return;
				}

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyVersion)
				{
				if (MessageBox.Show (string.Format (Localization.GetText ("LibraryIsIncompatible", al),
						ProgramDescription.AssemblyRequirements[0], ConcurrentDrawLib.CDLibVersion, ProgramDescription.AssemblyVersion) +
						Localization.GetText ("LibraryNotFound_Lib0", al), ProgramDescription.AssemblyTitle,
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
					ProgramDescription.ShowActualAssemblyPage ();
				return;
				}

			// Начальная обработка и отображение лого
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new ConcurrentDrawForm ());
			}
		}
	}
