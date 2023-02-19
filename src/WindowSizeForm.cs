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
				return new Size (availableSizes[SizesCombo.SelectedIndex].X,
					availableSizes[SizesCombo.SelectedIndex].Y);
				}
			}

		/// <summary>
		/// Конструктор. Запускает форму выбора размера
		/// </summary>
		/// <param name="MaxHeight">Максимальная высота окна</param>
		/// <param name="MaxWidth">Максимальная ширина окна</param>
		public WindowSizeForm (uint MaxWidth, uint MaxHeight/*, SupportedLanguages InterfaceLanguage*/)
			{
			// Инициализация
			InitializeComponent ();
			this.AcceptButton = BOK;
			this.CancelButton = BCancel;

			this.Text = Localization.GetText ("CDP_WindowSize");
			BOK.Text = Localization.GetText ("ConcurrentDrawParameters_BOK");
			BCancel.Text = Localization.GetText ("ConcurrentDrawParameters_BCancel");

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
				SizesCombo.Items.Add (availableSizes[i].X.ToString () + " x " +
					availableSizes[i].Y.ToString () + " px");
			SizesCombo.SelectedIndex = 0;

			// Запуск
			this.ShowDialog ();
			}

		// Выбор размера
		private void BOK_Click (object sender, EventArgs e)
			{
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
