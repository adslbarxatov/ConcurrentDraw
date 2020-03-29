using System.Drawing;
using System.Windows.Forms;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Начальная форма программы
	/// </summary>
	public partial class ScreenShooterForm:Form
		{
		// Параметры
		private Point start;

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
				return MainSelection.Size;
				}
			}

		/// <summary>
		/// Возвращает верхнюю левую точку выбранного поля
		/// </summary>
		public Point LeftTopPoint
			{
			get
				{
				return MainSelection.Location;
				}
			}

		/// <summary>
		/// Конструктор. Запускает визуальный выбор размеров и позиции окна
		/// </summary>
		/// <param name="MaxHeight">Максимальная высота окна</param>
		/// <param name="MaxWidth">Максимальная ширина окна</param>
		/// <param name="MinHeight">Минимальная высота окна</param>
		/// <param name="MinWidth">Минимальная ширина окна</param>
		public ScreenShooterForm (uint MinWidth, uint MaxWidth, uint MinHeight, uint MaxHeight)
			{
			// Инициализация
			InitializeComponent ();

			// Если запрос границ экрана завершается ошибкой, отменяем отображение
			try
				{
				this.Width = Screen.PrimaryScreen.Bounds.Width;
				this.Height = Screen.PrimaryScreen.Bounds.Height;
				}
			catch
				{
				this.Close ();
				return;
				}

			// Настройка контролов
			this.Text = ProgramDescription.AssemblyTitle;
			MainSelection.MinimumSize = new Size ((int)MinWidth, (int)MinHeight);
			MainSelection.MaximumSize = new Size ((int)MaxWidth, (int)MaxHeight);

			// Запуск
			this.ShowDialog ();
			}

		// Нажатие мыши
		private void MainForm_MouseDown (object sender, MouseEventArgs e)
			{
			// Обработка выделения области
			if (e.Button != MouseButtons.Left)
				return;

			if (!MainSelection.Visible)
				MainSelection.Visible = true;

			// Фиксация начальной точки
			MainSelection.Location = start = e.Location;
			}

		// Движение мыши
		private void MainForm_MouseMove (object sender, MouseEventArgs e)
			{
			if (e.Button != MouseButtons.Left)
				return;

			// Обновление рамки выделения
			if (e.X >= start.X)
				{
				MainSelection.Left = start.X;
				MainSelection.Width = e.X - start.X + 1;
				}
			else
				{
				MainSelection.Left = e.X;
				MainSelection.Width = start.X - e.X + 1;
				}

			if (e.Y >= start.Y)
				{
				MainSelection.Top = start.Y;
				MainSelection.Height = e.Y - start.Y + 1;
				}
			else
				{
				MainSelection.Top = e.Y;
				MainSelection.Height = start.Y - e.Y + 1;
				}

			// Отображение координат и размеров
			MainSelection.Text = "(" + MainSelection.Left.ToString () + "; " + MainSelection.Top.ToString () + ") (" +
				MainSelection.Width.ToString () + " x " + MainSelection.Height.ToString () + ")";
			}

		// Завершение выделения
		private void MainForm_MouseUp (object sender, MouseEventArgs e)
			{
			// Выбор сделан
			if (e.Button == MouseButtons.Left)
				{
				selected = true;
				this.Close ();
				}
			}

		// Команды рамки выделения
		private void MainSelection_MouseDown (object sender, MouseEventArgs e)
			{
			if (MainSelection.Visible)
				MainSelection.Visible = false;
			}

		// Обработка клавиатуры
		private void MainForm_KeyDown (object sender, KeyEventArgs e)
			{
			switch (e.KeyCode)
				{
				// Выход
				case Keys.Escape:
				case Keys.X:
				case Keys.Q:
					this.Close ();
					break;
				}
			}
		}
	}
