using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает заставку приложения
	/// </summary>
	public partial class ConcurrentDrawLogo:Form
		{
		/// <summary>
		/// Конструктор. Запускает заставку
		/// </summary>
		public ConcurrentDrawLogo ()
			{
			// Инициализация
			InitializeComponent ();

			// Настройка контролов
			this.BackgroundImage = Properties.CDResources.ConcurrentDraw;
			this.Width = Properties.CDResources.ConcurrentDraw.Width;
			this.Height = Properties.CDResources.ConcurrentDraw.Height;

			AboutLabel.Text = ProgramDescription.AssemblyTitle + "\n\n" + ProgramDescription.AssemblyDescription + "\n" +
				"by " + ProgramDescription.AssemblyCopyright + "\nUpdated: " + ProgramDescription.AssemblyLastUpdate;
			AboutLabel.Left = this.Width - 24 - AboutLabel.Width;
			AboutLabel.Top = this.Height - 24 - AboutLabel.Height;

			// Запуск
			MainTimer.Interval = 2500;
			MainTimer.Enabled = true;
			this.ShowDialog ();
			}

		// Завершение
		private void MainTimer_Tick (object sender, System.EventArgs e)
			{
			this.Close ();
			}
		}
	}
