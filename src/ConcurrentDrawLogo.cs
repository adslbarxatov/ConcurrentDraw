using System;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает заставку приложения
	/// </summary>
	public partial class ConcurrentDrawLogo: Form
		{
		/// <summary>
		/// Конструктор. Запускает заставку
		/// </summary>
		public ConcurrentDrawLogo ()
			{
			// Инициализация
			InitializeComponent ();

			// Настройка контролов
#if DPMODULE

			this.BackgroundImage = Properties.DPModuleResources.DeploymentPackages;
			this.Width = Properties.DPModuleResources.DeploymentPackages.Width;
			this.Height = Properties.DPModuleResources.DeploymentPackages.Height;

#else

			this.BackgroundImage = Properties.CDResources.ConcurrentDraw;
			this.Width = Properties.CDResources.ConcurrentDraw.Width;
			this.Height = Properties.CDResources.ConcurrentDraw.Height;

#endif

			AboutLabel.Text = ProgramDescription.AssemblyTitle +
#if DPMODULE
				"\n" + ProgramDescription.AssemblyDescription +
#else
				"\n\n" + ProgramDescription.AssemblyDescription +
				"\nby " + RDGenerics.AssemblyCopyright +
#endif
				"\nUpdated: " + ProgramDescription.AssemblyLastUpdate;
			AboutLabel.Left = this.Width - 24 - AboutLabel.Width;
			AboutLabel.Top = this.Height - 24 - AboutLabel.Height;

			// Запуск
			MainTimer.Interval = 2500;
			MainTimer.Enabled = true;

#if !DPMODULE
			this.ShowDialog ();
#endif
			}

		// Завершение
		private void MainTimer_Tick (object sender, EventArgs e)
			{
			MainTimer.Enabled = false;
			this.Close ();
			}

		// Досрочное закрытие
		private void ConcurrentDrawLogo_MouseMove (object sender, MouseEventArgs e)
			{
#if DPMODULE
			MainTimer.Enabled = false;
			this.Close ();
#endif
			}
		}
	}
