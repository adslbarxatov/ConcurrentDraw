using System.Drawing;
using System.Windows.Forms;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Форма обеспечивает доступ к стандартным размерам окна приложения
	/// </summary>
	public partial class WindowSizeForm:Form
		{
		// Доступные размеры
		private Point[] availableSizes = new Point[] {
			new Point (640, 480),
			new Point (640, 360),
			new Point (960, 720),
			new Point (960, 540),
			new Point (1024, 768),
			new Point (1280, 720)
			};

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
		/// <param name="InterfaceLanguage">Язык интерфейса</param>
		public WindowSizeForm (SupportedLanguages InterfaceLanguage)
			{
			// Инициализация
			InitializeComponent ();

			this.Text = Localization.GetText ("CDPWindowSize", InterfaceLanguage);
			BOK.Text = Localization.GetText ("CDP_OK", InterfaceLanguage);
			BCancel.Text = Localization.GetText ("CDP_Cancel", InterfaceLanguage);

			for (int i = 0; i < availableSizes.Length; i++)
				SizesCombo.Items.Add (availableSizes[i].X.ToString () + " x " + availableSizes[i].Y.ToString () + " px");
			SizesCombo.SelectedIndex = 0;

			// Запуск
			this.ShowDialog ();
			}

		// Выбор размера
		private void BOK_Click (object sender, System.EventArgs e)
			{
			selected = true;
			this.Close ();
			}

		// Отмена
		private void BCancel_Click (object sender, System.EventArgs e)
			{
			this.Close ();
			}
		}
	}
