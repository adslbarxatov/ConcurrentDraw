using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает заставку приложения
	/// </summary>
	public partial class ConcurrentDrawLogo: Form
		{
		// Переменные
		private Graphics g, gb;
		private Bitmap b;
		private string title;
		private Font f;
		private SizeF sz;

		/// <summary>
		/// Конструктор. Запускает заставку
		/// </summary>
		public ConcurrentDrawLogo ()
			{
			// Инициализация
			InitializeComponent ();

			// Настройка контролов
#if DPMODULE

			this.Width = Properties.DPModuleResources.DeploymentPackages.Width;
			this.Height = Properties.DPModuleResources.DeploymentPackages.Height;
			this.BackColor = ProgramDescription.MasterBackColor;

			gb = Graphics.FromHwnd (this.Handle);

#else

			this.Width = Properties.CDResources.ConcurrentDraw.Width;
			this.Height = Properties.CDResources.ConcurrentDraw.Height;

#endif

			b = new Bitmap (this.Width, this.Height);
			g = Graphics.FromImage (b);
			g.FillRectangle (new SolidBrush (this.BackColor), 0, 0, this.Width, this.Height);
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

#if DPMODULE
			f = new Font ("Arial", 11.0f, FontStyle.Bold | FontStyle.Italic);
#else
			f = new Font ("Arial", 11.0f, FontStyle.Bold);
#endif

			// Надпись
			title = ProgramDescription.AssemblyTitle +
#if DPMODULE
				"\n" + ProgramDescription.AssemblyDescription +
#else
				"\n\n" + ProgramDescription.AssemblyDescription +
				"\nby " + RDGenerics.AssemblyCopyright +
#endif
				"\nUpdated: " + ProgramDescription.AssemblyLastUpdate;
			sz = g.MeasureString (title, f);


			// Запуск
#if DPMODULE
			MainTimer.Interval = 25;
#else
			MainTimer.Interval = 2500;
#endif

			MainTimer.Enabled = true;

#if !DPMODULE

			// Отрисовка
			g.DrawImage (Properties.CDResources.ConcurrentDraw, 0, 0);

			SolidBrush c = new SolidBrush (Color.FromArgb (32, 32, 32));
			g.DrawString (title, f, c, this.Width - 18 - sz.Width, this.Height - 18 - sz.Height);
			c.Dispose ();

			this.BackgroundImage = b;

			// Запуск
			this.ShowDialog ();
#endif
			}

		// Завершение
#if DPMODULE
		private int pos = 0;
		private SolidBrush br = new SolidBrush (ProgramDescription.MasterTextColor);
#endif

		private void MainTimer_Tick (object sender, EventArgs e)
			{
#if DPMODULE

			// Граничные условия
			if (pos >= 100)  // 2,5 секунды
				{
				MainTimer.Enabled = false;
				this.Close ();
				}

			// Отрисовка
			if (pos < 30)
				{
				// Фон
				int w = this.Width / 10;
				for (int i = 0; i < 10; i++)
					{
					if (i >= pos)
						continue;

					g.FillRectangle (br, i * w, 0, (float)(w * Math.Sin ((Math.PI / 2.0) * (pos / 30.0))),
						this.Height);
					}

				// Подпись
				byte r = (byte)((ProgramDescription.MasterBackColor.R - ProgramDescription.MasterTextColor.R) *
					(pos / 30.0) + ProgramDescription.MasterTextColor.R);
				SolidBrush c = new SolidBrush (Color.FromArgb (r, r, r));

				g.DrawString (title, f, c, this.Width - 18 - sz.Width, this.Height - 18 - sz.Height);
				c.Dispose ();

				// Отрисовка на фоне
				g.DrawImage (Properties.DPModuleResources.DeploymentPackages, 0, 0);
				gb.DrawImage (b, 0, 0);
				}

			// Счётчик автозакрытия
			pos++;

#else

			this.Close ();

#endif
			}

		// Закрытие окна
		private void ConcurrentDrawLogo_FormClosing (object sender, FormClosingEventArgs e)
			{
			MainTimer.Enabled = false;

			if (b != null)
				b.Dispose ();
			if (g != null)
				g.Dispose ();
			if (gb != null)
				gb.Dispose ();
			if (f != null)
				f.Dispose ();

#if DPMODULE
			if (br != null)
				br.Dispose ();
#endif
			}

		// Досрочное закрытие
		private void ConcurrentDrawLogo_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		private void ConcurrentDrawLogo_KeyDown (object sender, KeyEventArgs e)
			{
			this.Close ();
			}
		}
	}
