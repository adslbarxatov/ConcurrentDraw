using System;
using System.Runtime.InteropServices;
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
			RDLocale.InitEncodings ();

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;

			// Проверка наличия обязательных компонентов
			if (!RDGenerics.CheckLibrariesExistence (ProgramDescription.AssemblyLibraries, true))
				return;

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			/*if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyLibVersion)
				{
				RDInterface.MessageBox (RDMessageTypes.Error_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.MessageFormat_WrongVersion_Fmt),
					ProgramDescription.AssemblyRequirementsCDL));

				return;
				}*/
			if (!RDGenerics.CheckLibraryVersion (ProgramDescription.AssemblyLibraries[0][0],
				ProgramDescription.AssemblyLibraries[0][1], GetCDLibVersionEx)
#if CHECK_BASS
				|| !RDGenerics.CheckLibraryVersion (ProgramDescription.AssemblyLibraries[1][0],
				ProgramDescription.AssemblyLibraries[1][1])
#endif
				)
				{
				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDInterface.AcceptEULA ())
				return;
			if (!RDInterface.ShowAbout (true))
				RDGenerics.RegisterFileAssociations (true);

			// Запуск
			Application.Run (new ConcurrentDrawForm ());
			}

		/// <summary>
		/// Функция возвращает версию библиотеки CDLib.dll
		/// </summary>
		/// <returns></returns>
		[DllImport (ProgramDescription.AssemblyCDL)]
		private static extern IntPtr GetCDLibVersionEx ();
		}
	}
