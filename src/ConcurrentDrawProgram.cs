using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Класс описывает основное приложение
	/// </summary>
	public class CodeShow
		{
		/// <summary>
		/// Точка входа программы
		/// </summary>
		/// <param name="args">Аргументы командной строки</param>
		[STAThread]
		public static void Main (string[] args)
			{
			// Проверка запуска единственной копии
			bool result;
			Mutex instance = new Mutex (true, ProgramDescription.AssemblyTitle, out result);
			if (!result)
				{
				MessageBox.Show (ProgramDescription.AssemblyTitle + " already started",
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Проверка наличия обязательных компонентов
			for (int i = 0; i < ProgramDescription.AssemblyRequirements.Length; i++)
				{
				if (!File.Exists (Application.StartupPath + "\\" + ProgramDescription.AssemblyRequirements[i]))
					{
					MessageBox.Show ("Библиотека «" + ProgramDescription.AssemblyRequirements[i] + "» не найдена или недоступна. " +
						"Работа программы невозможна", ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
					}
				}

#if BASSTEST
			ConcurrentDrawLib.BASSTest ();
#endif

			// Начальная обработка и отображение лого
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new ConcurrentDrawForm ());
			}
		}
	}
