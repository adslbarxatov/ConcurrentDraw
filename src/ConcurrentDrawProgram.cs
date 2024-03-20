using System;
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

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;

			// Проверка наличия обязательных компонентов
			if (!RDGenerics.CheckLibraries (ProgramDescription.AssemblyRequirements, true))
				return;

			// Проверка корреткности версии библиотеки CDLib.dll (BASS проверяется позже)
			if (ConcurrentDrawLib.CDLibVersion != ProgramDescription.AssemblyLibVersion)
				{
				RDGenerics.MessageBox (RDMessageTypes.Error_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.MessageFormat_WrongVersion_Fmt),
					ProgramDescription.AssemblyRequirementsCDL));

				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!RDGenerics.AcceptEULA ())
				return;
			if (!RDGenerics.ShowAbout (true))
				ProgramDescription.RegisterAppExtensions ();

			// Запуск
			Application.Run (new ConcurrentDrawForm ());
			}
		}
	}
