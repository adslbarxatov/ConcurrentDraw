using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Форма обеспечивает доступ к стандартным размерам окна приложения
	/// </summary>
	public partial class WindowSizeForm: Form
		{
		// Доступные размеры
		private List<Point> availableSizes = new List<Point> ();

		/// <summary>
		/// Возвращает флаг, указывающий на успешное задание размера
		/// </summary>
		public bool Selected
			{
			get
				{
				return selected;
				}
			}
		private bool selected = false;

		/// <summary>
		/// Возвращает требуемый размер окна
		/// </summary>
		public Size WindowSize
			{
			get
				{
				return windowSize;
				}
			}
		private Size windowSize = new Size (0, 0);

		/// <summary>
		/// Конструктор. Запускает форму выбора размера
		/// </summary>
		/// <param name="MaxHeight">Максимальная высота окна</param>
		/// <param name="MaxWidth">Максимальная ширина окна</param>
		public WindowSizeForm (uint MaxWidth, uint MaxHeight)
			{
			// Инициализация
			InitializeComponent ();

			this.Text = RDLocale.GetText ("CDP_WindowSize");

			BOK.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK);
			this.AcceptButton = BOK;

			BCancel.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel);
			this.CancelButton = BCancel;

			availableSizes.Clear ();
			availableSizes.Add (new Point (640, 360));
			availableSizes.Add (new Point (640, 480));
			availableSizes.Add (new Point (800, 450));
			availableSizes.Add (new Point (800, 600));
			availableSizes.Add (new Point (960, 540));
			availableSizes.Add (new Point (960, 720));
			availableSizes.Add (new Point (1280, 720));
			availableSizes.Add (new Point (1024, 768));
			availableSizes.Add (new Point ((int)MaxWidth, 128));
			availableSizes.Add (new Point ((int)MaxWidth, (int)MaxHeight));

			for (int i = 0; i < availableSizes.Count; i++)
				SizesCombo.Items.Add (availableSizes[i].X.ToString () + " × " +
					availableSizes[i].Y.ToString () + " px");
			SizesCombo.SelectedIndex = 0;

			// Запуск
			this.ShowDialog ();
			}

		// Выбор размера
		private void BOK_Click (object sender, EventArgs e)
			{
			windowSize = new Size (availableSizes[SizesCombo.SelectedIndex].X,
					availableSizes[SizesCombo.SelectedIndex].Y);
			selected = true;

			this.Close ();
			}

		// Отмена
		private void BCancel_Click (object sender, EventArgs e)
			{
			this.Close ();
			}
		}
	}
