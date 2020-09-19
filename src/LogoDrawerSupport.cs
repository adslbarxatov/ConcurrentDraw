using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает отрисовочный слой
	/// </summary>
	public class LogoDrawerLayer:IDisposable
		{
		// Переменные
		private bool isInited = false;

		/// <summary>
		/// Возвращает изображение слоя
		/// </summary>
		public Bitmap Layer
			{
			get
				{
				return (isInited ? layer : null);
				}
			}
		private Bitmap layer;

		/// <summary>
		/// Возвращает дескриптор для изменения слоя
		/// </summary>
		public Graphics Descriptor
			{
			get
				{
				return (isInited ? descriptor : null);
				}
			}
		private Graphics descriptor;

		/// <summary>
		/// Метод освобождает занятые экземпляром ресурсы
		/// </summary>
		public void Dispose ()
			{
			if (isInited)
				{
				descriptor.Dispose ();
				layer.Dispose ();
				isInited = false;
				}
			}

		/// <summary>
		/// Левое смещение слоя
		/// </summary>
		public uint Left
			{
			get
				{
				return left;
				}
			}
		private uint left;

		/// <summary>
		/// Верхнее смещение слоя
		/// </summary>
		public uint Top
			{
			get
				{
				return top;
				}
			}
		private uint top;

		/// <summary>
		/// Конструктор. Создаёт новый отрисовочный слой
		/// </summary>
		/// <param name="Width">Ширина слоя</param>
		/// <param name="Height">Высота слоя</param>
		/// <param name="LeftOffset">Левое смещение</param>
		/// <param name="TopOffset">Верхнее смещение</param>
		public LogoDrawerLayer (uint LeftOffset, uint TopOffset, uint Width, uint Height)
			{
			left = LeftOffset;
			top = TopOffset;
			layer = new Bitmap ((Width == 0) ? 1 : (int)Width, (Height == 0) ? 1 : (int)Height);
			descriptor = Graphics.FromImage (layer);
			isInited = true;
			}
		}

	/// <summary>
	/// Вспомогательный класс программы
	/// </summary>
	public static class LogoDrawerSupport
		{
		/*/// <summary>
		/// Граница разрешения/запрета ускорения
		/// </summary>
		public const int AccelerationBorder = 0;*/

		/// <summary>
		/// Количество возможных стартовых позиций
		/// </summary>
		public const uint ObjectStartupPositionsCount = 14;

		/// <summary>
		/// Количество возможных типов объектов
		/// </summary>
		public const uint ObjectTypesCount = 9;

		/// <summary>
		/// Доля поля текста
		/// </summary>
		public const double TextFieldPart = 3.0 / 8.0;				// Часть поля отрисовки, занимаемая текстом

		/// <summary>
		/// Метод приводит исходные метрики объекта к допустимым диапазонам
		/// </summary>
		/// <param name="OldMetrics">Исходные метрики</param>
		/// <returns>Приведённые метрики</returns>
		public static LogoDrawerObjectMetrics AlingMetrics (LogoDrawerObjectMetrics OldMetrics)
			{
			LogoDrawerObjectMetrics metrics;

			metrics.MinRed = (OldMetrics.MinRed > OldMetrics.MaxRed) ? OldMetrics.MaxRed : OldMetrics.MinRed;
			metrics.MaxRed = (OldMetrics.MinRed > OldMetrics.MaxRed) ? OldMetrics.MinRed : OldMetrics.MaxRed;
			metrics.MinGreen = (OldMetrics.MinGreen > OldMetrics.MaxGreen) ? OldMetrics.MaxGreen : OldMetrics.MinGreen;
			metrics.MaxGreen = (OldMetrics.MinGreen > OldMetrics.MaxGreen) ? OldMetrics.MinGreen : OldMetrics.MaxGreen;
			metrics.MinBlue = (OldMetrics.MinBlue > OldMetrics.MaxBlue) ? OldMetrics.MaxBlue : OldMetrics.MinBlue;
			metrics.MaxBlue = (OldMetrics.MinBlue > OldMetrics.MaxBlue) ? OldMetrics.MinBlue : OldMetrics.MaxBlue;

			metrics.MinSize = (OldMetrics.MinSize > OldMetrics.MaxSize) ? OldMetrics.MaxSize : OldMetrics.MinSize;
			metrics.MaxSize = (OldMetrics.MinSize > OldMetrics.MaxSize) ? OldMetrics.MinSize : OldMetrics.MaxSize;
			if (metrics.MinSize < MinObjectSize)
				metrics.MinSize = MinObjectSize;
			if (metrics.MinSize > MaxObjectSize)
				metrics.MinSize = MaxObjectSize;
			if (metrics.MaxSize < metrics.MinSize)
				metrics.MaxSize = metrics.MinSize;
			if (metrics.MaxSize > MaxObjectSize)
				metrics.MaxSize = MaxObjectSize;

			metrics.MinSpeed = (OldMetrics.MinSpeed > OldMetrics.MaxSpeed) ? OldMetrics.MaxSpeed : OldMetrics.MinSpeed;
			metrics.MaxSpeed = (OldMetrics.MinSpeed > OldMetrics.MaxSpeed) ? OldMetrics.MinSpeed : OldMetrics.MaxSpeed;
			if (metrics.MinSpeed < MinObjectSpeed)
				metrics.MinSpeed = MinObjectSpeed;
			if (metrics.MinSpeed > MaxObjectSpeed)
				metrics.MinSpeed = MaxObjectSpeed;
			if (metrics.MaxSpeed < metrics.MinSpeed)
				metrics.MaxSpeed = metrics.MinSpeed;
			if (metrics.MaxSpeed > MaxObjectSpeed)
				metrics.MaxSpeed = MaxObjectSpeed;

			metrics.MaxSpeedFluctuation = OldMetrics.MaxSpeedFluctuation;
			if (metrics.MaxSpeedFluctuation > MaxObjectSpeed)
				metrics.MaxSpeedFluctuation = MaxObjectSpeed;
			metrics.StartupPosition = OldMetrics.StartupPosition;

			metrics.ObjectsType = OldMetrics.ObjectsType;

			metrics.ObjectsCount = OldMetrics.ObjectsCount;
			if (metrics.ObjectsCount > MaxObjectsCount)
				metrics.ObjectsCount = (byte)MaxObjectsCount;

			metrics.PolygonsSidesCount = OldMetrics.PolygonsSidesCount;
			if (metrics.PolygonsSidesCount > MaxPolygonsSidesCount)
				metrics.PolygonsSidesCount = (byte)MaxPolygonsSidesCount;
			if (metrics.PolygonsSidesCount < MinPolygonsSidesCount)
				metrics.PolygonsSidesCount = (byte)MinPolygonsSidesCount;

			metrics.KeepTracks = OldMetrics.KeepTracks;
			metrics.AsStars = OldMetrics.AsStars;
			metrics.Rotation = OldMetrics.Rotation;
			metrics.Acceleration = OldMetrics.Acceleration;

			metrics.Enlarging = OldMetrics.Enlarging;
			if (metrics.Enlarging < -MaxEnlarge)
				metrics.Enlarging = -MaxEnlarge;
			if (metrics.Enlarging > MaxEnlarge)
				metrics.Enlarging = MaxEnlarge;

			return metrics;
			}

		/// <summary>
		/// Минимально допустимый размер шрифта
		/// </summary>
		public const uint MinFontSize = 10;

		/// <summary>
		/// Максимально допустимый размер шрифта
		/// </summary>
		public const uint MaxFontSize = 100;

		/// <summary>
		/// Максимально допустимое количество объектов
		/// </summary>
		public const uint MaxObjectsCount = 20;

		/// <summary>
		/// Минимально допустимое количество сторон многоугольников
		/// </summary>
		public const uint MinPolygonsSidesCount = 3;

		/// <summary>
		/// Максимально допустимое количество сторон многоугольников
		/// </summary>
		public const uint MaxPolygonsSidesCount = 16;

		/// <summary>
		/// Максимально допустимый коэффициент увеличения / уменьшения
		/// </summary>
		public const int MaxEnlarge = 10;

		/// <summary>
		/// Максимально допустимый коэффициент ускорения
		/// </summary>
		public const int MaxAcceleration = 10;

		/// <summary>
		/// Минимально допустимая скорость объекта
		/// </summary>
		public const uint MinObjectSpeed = 0;

		/// <summary>
		/// Максимально допустимая скорость объекта
		/// </summary>
		public const uint MaxObjectSpeed = 50;

		/// <summary>
		/// Минимально допустимый размер объекта
		/// </summary>
		public const uint MinObjectSize = 1;

		/// <summary>
		/// Максимально допустимый размер объекта
		/// </summary>
		public const uint MaxObjectSize = 400;

		/// <summary>
		/// Метод переводит градусы в радианы
		/// </summary>
		/// <param name="Φ">Градусная величина угла</param>
		/// <returns>Радианная величина угла</returns>
		public static double D2R (double Φ)
			{
			return Math.PI * Φ / 180.0;
			}

		/// <summary>
		/// Метод возвращает значение синуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Синус угла</returns>
		public static double Sinus (double ArcInDegrees)
			{
			return Math.Sin (D2R (ArcInDegrees));
			}

		/// <summary>
		/// Метод возвращает значение косинуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Косинус угла</returns>
		public static double Cosinus (double ArcInDegrees)
			{
			return Math.Cos (D2R (ArcInDegrees));
			}

		/*/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Flat
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsFlat (LogoDrawerObjectStartupPositions Position)
			{
			return (Position == LogoDrawerObjectStartupPositions.LeftFlat) ||
				(Position == LogoDrawerObjectStartupPositions.RightFlat) ||
				(Position == LogoDrawerObjectStartupPositions.CenterFlat);
			}*/

		/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Left
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsLeft (LogoDrawerObjectStartupPositions Position)
			{
			return (Position == LogoDrawerObjectStartupPositions.Left) /*||
				(Position == LogoDrawerObjectStartupPositions.LeftFlat)*/;
			}

		/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Center
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsCenter (LogoDrawerObjectStartupPositions Position)
			{
			return //(Position == LogoDrawerObjectStartupPositions.CenterFlat) ||
				(Position == LogoDrawerObjectStartupPositions.CenterRandom);
			}

		/// <summary>
		/// Метод рассчитывает вторую координату точки по известным параметрам линейной траектории
		/// </summary>
		/// <param name="MasterX">Абсцисса известной точки прямой</param>
		/// <param name="MasterY">Ордината известной точки прямой</param>
		/// <param name="StepX">Шаг абсциссы до соседней известной точки прямой</param>
		/// <param name="StepY">Шаг ординаты до соседней известной точки прямой</param>
		/// <param name="OppositeValue">Известная координата требуемой точки</param>
		/// <param name="IsAbscissa">Тип известной координаты требуемой точки</param>
		/// <returns>Возвращает вторую координату точки</returns>
		public static int EvaluateLinearCoordinate (int MasterX, int MasterY, int StepX, int StepY, int OppositeValue, bool IsAbscissa)
			{
			if (IsAbscissa)
				{
				if (StepX != 0)
					return (int)((double)(OppositeValue - MasterX) * (double)StepY / (double)StepX + (double)MasterY);
				else
					return MasterY;
				}
			else
				{
				if (StepY != 0)
					return (int)((double)(OppositeValue - MasterY) * (double)StepX / (double)StepY + (double)MasterX);
				else
					return MasterX;
				}
			}
		}

	/// <summary>
	/// Класс описывает строку, предназначенную для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerString
		{
		/// <summary>
		/// Шрифт строки
		/// </summary>
		public Font StringFont
			{
			get
				{
				return stringFont;
				}
			}
		private Font stringFont;

		/// <summary>
		/// Текст строки
		/// </summary>
		public string StringText
			{
			get
				{
				return stringText;
				}
			}
		private string stringText;

		/// <summary>
		/// Пауза до перехода на следующую строку
		/// </summary>
		public uint Pause
			{
			get
				{
				return pause;
				}
			}
		private uint pause = 0;

		/// <summary>
		/// Ширина буквы строки текста
		/// </summary>
		public uint LetterSize
			{
			get
				{
				return letterSize;
				}
			}
		private uint letterSize = 0;

		/// <summary>
		/// Длина строки текста
		/// </summary>
		public uint StringLength
			{
			get
				{
				return stringLength;
				}
			}
		private uint stringLength = 0;

		/// <summary>
		/// Тип строки (дополнительное поле)
		/// </summary>
		public uint StringType
			{
			get
				{
				return stringType;
				}
			}
		private uint stringType = 0;

		/// <summary>
		/// Конструктор. Инициализиует объект-строку (предполагает моноширинный шрифт)
		/// </summary>
		/// <param name="Text">Текст строки</param>
		/// <param name="TextFont">Шрифт строки</param>
		/// <param name="TimeoutPause">Пауза до перехода к следующей строке</param>
		/// <param name="LetterWidth">Ширина отдельной буквы строки</param>
		public LogoDrawerString (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth)
			{
			LogoDrawerStringInit (Text, TextFont, TimeoutPause, LetterWidth, 0);
			}

		/// <summary>
		/// Конструктор. Инициализиует объект-строку (предполагает моноширинный шрифт)
		/// </summary>
		/// <param name="Text">Текст строки</param>
		/// <param name="TextFont">Шрифт строки</param>
		/// <param name="TimeoutPause">Пауза до перехода к следующей строке</param>
		/// <param name="LetterWidth">Ширина отдельной буквы строки</param>
		/// <param name="Type">Тип строки (для поддержки дополнительных функций)</param>
		public LogoDrawerString (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth, uint Type)
			{
			LogoDrawerStringInit (Text, TextFont, TimeoutPause, LetterWidth, Type);
			}

		private void LogoDrawerStringInit (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth, uint Type)
			{
			stringText = Text;
			if ((stringText == null) || (stringText == ""))
				stringText = " ";
			stringLength = (uint)stringText.Length;

			stringFont = TextFont;
			letterSize = LetterWidth;
			pause = TimeoutPause;
			stringType = Type;
			}
		}

	/// <summary>
	/// Структура описывает параметры генерации объектов
	/// </summary>
	public struct LogoDrawerObjectMetrics
		{
		/// <summary>
		/// Начальная позиция движения
		/// </summary>
		public LogoDrawerObjectStartupPositions StartupPosition;

		/// <summary>
		/// Минимальная скорость
		/// </summary>
		public uint MinSpeed;

		/// <summary>
		/// Максимальная скорость
		/// </summary>
		public uint MaxSpeed;

		/// <summary>
		/// Дребезг скорости при движении
		/// </summary>
		public uint MaxSpeedFluctuation;

		/// <summary>
		/// Минимальный размер
		/// </summary>
		public uint MinSize;

		/// <summary>
		/// Максимальный размер
		/// </summary>
		public uint MaxSize;

		/// <summary>
		/// Минимальное значение красного канала
		/// </summary>
		public byte MinRed;

		/// <summary>
		/// Максимальное значение красного канала
		/// </summary>
		public byte MaxRed;

		/// <summary>
		/// Минимальное значение зелёного канала
		/// </summary>
		public byte MinGreen;

		/// <summary>
		/// Максимальное значение зелёного канала
		/// </summary>
		public byte MaxGreen;

		/// <summary>
		/// Минимальное значение синего канала
		/// </summary>
		public byte MinBlue;

		/// <summary>
		/// Максимальное значение синего канала
		/// </summary>
		public byte MaxBlue;

		/// <summary>
		/// Тип объектов
		/// </summary>
		public LogoDrawerObjectTypes ObjectsType;

		/// <summary>
		/// Количество одновременно существующих объектов
		/// </summary>
		public byte ObjectsCount;

		/// <summary>
		/// Количество сторон многоугольников
		/// </summary>
		public byte PolygonsSidesCount;

		/// <summary>
		/// Флаг сохранения следов движения объектов
		/// </summary>
		public bool KeepTracks;

		/// <summary>
		/// Флаг преобразования многоугольников в звёзды
		/// </summary>
		public bool AsStars;

		/// <summary>
		/// Флаг вращения объектов
		/// </summary>
		public bool Rotation;

		/// <summary>
		/// Коэффициент ускорения движения объектов
		/// </summary>
		public uint Acceleration;

		/// <summary>
		/// Коэффициент увеличения / уменьшения объектов при движении
		/// </summary>
		public int Enlarging;
		}

	/// <summary>
	/// Возможные начальные позиции объектов
	/// </summary>
	public enum LogoDrawerObjectStartupPositions
		{
		/// <summary>
		/// Слева
		/// </summary>
		Left = 1,

		/// <summary>
		/// Справа
		/// </summary>
		Right = 2,

		/// <summary>
		/// Сверху
		/// </summary>
		Top = 3,

		/// <summary>
		/// Снизу
		/// </summary>
		Bottom = 4,

		/// <summary>
		/// От центра во все стороны
		/// </summary>
		CenterRandom = 5,

		/*/// <summary>
		/// От центра по горизонтали
		/// </summary>
		CenterFlat = 6,

		/// <summary>
		/// Слева по горизонтали
		/// </summary>
		LeftFlat = 7,

		/// <summary>
		/// Справа по горизонтали
		/// </summary>
		RightFlat = 8,*/

		/// <summary>
		/// Случайная
		/// </summary>
		Random = 0,

		/// <summary>
		/// К центру со всех сторон
		/// </summary>
		ToCenterRandom = 6,

		/// <summary>
		/// Слева сверху
		/// </summary>
		LeftTop = 7,

		/// <summary>
		/// Справа сверху
		/// </summary>
		RightTop = 8,

		/// <summary>
		/// Слева снизу
		/// </summary>
		LeftBottom = 9,

		/// <summary>
		/// Справа снизу
		/// </summary>
		RightBottom = 10
		}

	/// <summary>
	/// Возможные типы генерируемых объектов
	/// </summary>
	public enum LogoDrawerObjectTypes
		{
		/// <summary>
		/// Сферы
		/// </summary>
		Spheres = 0,

		/// <summary>
		/// Многоугольники
		/// </summary>
		Polygons = 4,

		/// <summary>
		/// Звёзды
		/// </summary>
		Stars = 5,

		/// <summary>
		/// Картинки
		/// </summary>
		Pictures = 7,

		/// <summary>
		/// Буквы
		/// </summary>
		Letters = 8,

		/// <summary>
		/// Вращающиеся многоугольники
		/// </summary>
		RotatingPolygons = 1,

		/// <summary>
		/// Вращающиеся звёзды
		/// </summary>
		RotatingStars = 2,

		/// <summary>
		/// Вращающиеся картинки
		/// </summary>
		RotatingPictures = 6,

		/// <summary>
		/// Вращающиеся буквы
		/// </summary>
		RotatingLetters = 3,
		}

	/// <summary>
	/// Класс описывает визуальный объект 'сфера' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerSphere:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private float speedX, initSpeedX;	// Горизонтальная скорость, начальная скорость
		private float speedY, initSpeedY;	// Вертикальная скорость, начальная скорость
		private float ρ;				// Радиус описанной окружности для объекта
		private SolidBrush objectBrush;	// Кисть для отрисовки объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				if (!isInited)
					return null;

				return image;
				}
			}
		private Bitmap image;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (image != null)
				image.Dispose ();
			if (objectBrush != null)
				objectBrush.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		/// <param name="ScreenCenterX">Абсцисса изменённого центра экрана</param>
		/// <param name="ScreenCenterY">Ордината изменённого центра экрана</param>
		public LogoDrawerSphere (uint ScreenWidth, uint ScreenHeight, uint ScreenCenterX, uint ScreenCenterY,
			Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = LogoDrawerSupport.AlingMetrics (Metrics);
			if (Randomizer == null)
				return;

			// Получение изображения
			rnd = Randomizer;
			maxFluctuation = (int)metrics.MaxSpeedFluctuation;
			ρ = rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize);

			objectBrush = new SolidBrush (Color.FromArgb (10, rnd.Next (metrics.MinRed, metrics.MaxRed + 1),
				rnd.Next (metrics.MinGreen, metrics.MaxGreen + 1), rnd.Next (metrics.MinBlue, metrics.MaxBlue + 1)));

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				//case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
					//case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -(int)ρ;
						endX = (int)(ScreenWidth + ρ);
						}
					else
						{
						x = (int)(ScreenWidth + ρ);
						endX = -(int)ρ;
						speedX *= -1;
						}

					/*if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))*/
					endY = y = rnd.Next ((int)(ScreenHeight + ρ)) - (int)(ρ / 2);
					/*else
						endY = y = (int)(ScreenHeight2 * (1.0 - LogoDrawerSupport.TextFieldPart));*/
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -(int)ρ;
						endY = (int)(ScreenHeight + ρ);
						}
					else
						{
						y = (int)(ScreenHeight + ρ);
						endY = -(int)ρ;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)(ScreenWidth + ρ)) - (int)(ρ / 2);
					break;

				case LogoDrawerObjectStartupPositions.LeftTop:
				case LogoDrawerObjectStartupPositions.LeftBottom:
				case LogoDrawerObjectStartupPositions.RightTop:
				case LogoDrawerObjectStartupPositions.RightBottom:
					while ((speedX == 0) && (speedY == 0))
						{
						speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.RightTop))
						{
						y = -2 * (int)ρ;
						endY = (int)(ScreenHeight + 2 * ρ);
						}
					else
						{
						y = (int)(ScreenHeight + 2 * ρ);
						endY = -2 * (int)ρ;
						speedY *= -1;
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftBottom))
						{
						x = -2 * (int)ρ;
						endX = (int)(ScreenWidth + 2 * ρ);
						}
					else
						{
						x = (int)(ScreenWidth + 2 * ρ);
						endX = -2 * (int)ρ;
						speedX *= -1;
						}
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				//case LogoDrawerObjectStartupPositions.CenterFlat:
				case LogoDrawerObjectStartupPositions.ToCenterRandom:
				default:
					while ((speedX == 0) && (speedY == 0) /*||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0)*/)
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						/*if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else*/
						speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenCenterX;	// (int)ScreenWidth / 2;
						/*if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight2 * (1.0 - LogoDrawerSupport.TextFieldPart));
						else*/
						y = (int)ScreenCenterY;	// (int)(ScreenHeight / 2);
						}
					else if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (int)ScreenCenterX;	// (int)ScreenWidth / 2;
						endY = (int)ScreenCenterY;	// (int)ScreenHeight / 2;

						if (Math.Abs (speedX) > Math.Abs (speedY))
							{
							x = (speedX < 0) ? ((int)(ScreenWidth + ρ + speedX) + maxFluctuation) :
								(-(int)(ρ + speedX) - maxFluctuation);
							y = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, x, true);
							}
						else
							{
							y = (speedY < 0) ? ((int)(ScreenHeight + ρ + speedY) + maxFluctuation) :
								(-(int)(ρ + speedY) - maxFluctuation);
							x = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, y, false);
							}
						}
					else
						{
						x = rnd.Next ((int)(ScreenWidth + ρ)) - (int)(ρ / 2);
						y = rnd.Next ((int)(ScreenHeight + ρ)) - (int)(ρ / 2);
						}

					if (metrics.StartupPosition != LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (speedX > 0) ? ((int)(ScreenWidth + ρ + speedX) + maxFluctuation) :
							(-(int)(ρ + speedX) - maxFluctuation);		// Фикс против преждевременного
						endY = (speedY > 0) ? ((int)(ScreenHeight + ρ + speedY) + maxFluctuation) :
							(-(int)(ρ + speedY) - maxFluctuation);		// 'перепрыгивания' порога
						}

					break;
				}

			// Успешно
			initSpeedX = Math.Sign (speedX);
			initSpeedY = (speedX * speedY == 0.0f) ? 1 : (speedY / Math.Abs (speedX));

			isInited = true;
			Move (0, 0);		// Генерация изображения
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Коэффициент ускорения</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		public void Move (uint Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += ((int)speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			speedX += Acceleration * initSpeedX / 10.0f;

			y += ((int)speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			//if ((speedY > LogoDrawerSupport.AccelerationBorder) || (speedY < -LogoDrawerSupport.AccelerationBorder))
			speedY += Acceleration * initSpeedY / 10.0f;	// Пропорциональное смещение

			if ((Enlarging > 0) || (Enlarging < 0) && (ρ > -Enlarging + 2))
				ρ += Enlarging / 10.0f;

			// Отрисовка
			if (image != null)
				image.Dispose ();

			if (ρ > 2)
				{
				image = new Bitmap ((int)ρ, (int)ρ);
				Graphics g = Graphics.FromImage (image);

				for (int i = (int)ρ / 2; i >= 0; i -= 3)
					g.FillEllipse (objectBrush, ρ / 2 - i, ρ / 2 - i, 2 * i, 2 * i);

				g.Dispose ();
				}
			else
				image = new Bitmap (2, 2);

			// Контроль
			if ((speedX > 0) && (x > endX) || (speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) || (speedY < 0) && (y < endY))
				isInited = false;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'многоугольник' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerSquare:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private float speedX, initSpeedX;	// Скорость горизонтального смещения, начальная скорость
		private float speedY, initSpeedY;	// Скорость вертикального смещения, начальная скорость
		private int speedOfRotation;	// Скорость вращения
		private uint sidesCount;		// Количество сторон многоугольника
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private bool star;				// Флаг преобразования многоугольника в звезду
		private float ρ;				// Радиус описанной окружности для объекта
		private int φ;					// Угол поворота объекта
		private SolidBrush objectBrush;	// Кисть для отрисовки объекта
		private bool rotation;			// Флаг вращения объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return image;
				}
			}
		private Bitmap image;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (image != null)
				image.Dispose ();
			if (objectBrush != null)
				objectBrush.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'правильный многоугольник'
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		/// <param name="ScreenCenterX">Абсцисса изменённого центра экрана</param>
		/// <param name="ScreenCenterY">Ордината изменённого центра экрана</param>
		public LogoDrawerSquare (uint ScreenWidth, uint ScreenHeight, uint ScreenCenterX, uint ScreenCenterY,
			Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = Metrics;	// LogoDrawerSupport.AlingMetrics (Metrics);
			if (Randomizer == null)
				return;

			// Генерация параметров изображения
			rnd = Randomizer;
			star = metrics.AsStars;
			rotation = metrics.Rotation;
			sidesCount = metrics.PolygonsSidesCount;
			ρ = rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize + 1);

			if (rotation)
				{
				φ = rnd.Next (0, 360);
				speedOfRotation = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				}
			else
				{
				φ = (360 * 3) / (metrics.PolygonsSidesCount * 4);	// Ориентация вверх
				speedOfRotation = 0;
				}

			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			objectBrush = new SolidBrush (Color.FromArgb (255, rnd.Next (metrics.MinRed, metrics.MaxRed + 1),
				rnd.Next (metrics.MinGreen, metrics.MaxGreen + 1), rnd.Next (metrics.MinBlue, metrics.MaxBlue + 1)));

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				//case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
					//case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -2 * (int)ρ;
						endX = (int)(ScreenWidth + 2 * ρ);
						}
					else
						{
						x = (int)(ScreenWidth + 2 * ρ);
						endX = -2 * (int)ρ;
						speedX *= -1;
						}

					/*if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))*/
					endY = y = rnd.Next ((int)(ScreenHeight + 2 * ρ)) - (int)ρ;
					/*else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));*/
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -2 * (int)ρ;
						endY = (int)(ScreenHeight + 2 * ρ);
						}
					else
						{
						y = (int)(ScreenHeight + 2 * ρ);
						endY = -2 * (int)ρ;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)(ScreenWidth + 2 * ρ)) - (int)ρ;
					break;

				case LogoDrawerObjectStartupPositions.LeftTop:
				case LogoDrawerObjectStartupPositions.LeftBottom:
				case LogoDrawerObjectStartupPositions.RightTop:
				case LogoDrawerObjectStartupPositions.RightBottom:
					while ((speedX == 0) && (speedY == 0))
						{
						speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.RightTop))
						{
						y = -2 * (int)ρ;
						endY = (int)(ScreenHeight + 2 * ρ);
						}
					else
						{
						y = (int)(ScreenHeight + 2 * ρ);
						endY = -2 * (int)ρ;
						speedY *= -1;
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftBottom))
						{
						x = -2 * (int)ρ;
						endX = (int)(ScreenWidth + 2 * ρ);
						}
					else
						{
						x = (int)(ScreenWidth + 2 * ρ);
						endX = -2 * (int)ρ;
						speedX *= -1;
						}
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				//case LogoDrawerObjectStartupPositions.CenterFlat:
				case LogoDrawerObjectStartupPositions.ToCenterRandom:
				default:
					while ((speedX == 0) && (speedY == 0) /*||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0)*/)
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						/*if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else*/
						speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenCenterX;	// ScreenWidth / 2;
						/*if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else*/
						y = (int)ScreenCenterY;	// (ScreenHeight / 2);
						}
					else if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (int)ScreenCenterX;	// ScreenWidth / 2;
						endY = (int)ScreenCenterY;	// ScreenHeight / 2;

						if (Math.Abs (speedX) > Math.Abs (speedY))
							{
							x = (speedX < 0) ? ((int)(ScreenWidth + 4 * ρ + speedX) + maxFluctuation) :
								(-4 * (int)(ρ + speedX) - maxFluctuation);
							y = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, x, true);
							}
						else
							{
							y = (speedY < 0) ? ((int)(ScreenHeight + 4 * ρ + speedY) + maxFluctuation) :
								(-4 * (int)(ρ + speedY) - maxFluctuation);
							x = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, y, false);
							}
						}
					else
						{
						x = rnd.Next ((int)(ScreenWidth + 2 * ρ)) - (int)ρ;
						y = rnd.Next ((int)(ScreenHeight + 2 * ρ)) - (int)ρ;
						}

					if (metrics.StartupPosition != LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (speedX > 0) ? ((int)(ScreenWidth + 8 * ρ + speedX) + maxFluctuation) :
							(-8 * (int)(ρ + speedX) - maxFluctuation);
						endY = (speedY > 0) ? ((int)(ScreenHeight + 8 * ρ + speedY) + maxFluctuation) :
							(-8 * (int)(ρ + speedY) - maxFluctuation);
						}
					break;
				}

			// Успешно
			initSpeedX = Math.Sign (speedX);
			initSpeedY = (speedX * speedY == 0.0f) ? 1 : (speedY / Math.Abs (speedX));

			isInited = true;
			Move (0, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		public void Move (uint Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += ((int)speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			speedX += Acceleration * initSpeedX / 10.0f;

			y += ((int)speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			//if ((speedY > LogoDrawerSupport.AccelerationBorder) || (speedY < -LogoDrawerSupport.AccelerationBorder))
			speedY += Acceleration * initSpeedY / 10.0f;

			if (rotation)
				{
				φ += speedOfRotation;
				while (φ < 0)
					φ += 360;
				while (φ > 359)
					φ -= 360;
				}

			if ((Enlarging > 0) || (Enlarging < 0) && (ρ > -Enlarging))
				ρ += Enlarging / 10.0f;

			// Сброс предыдущего изображения
			if (image != null)
				image.Dispose ();

			// Сборка фрейма
			List<Point> points = new List<Point> ();
			for (int i = 0; i < sidesCount; i++)
				{
				points.Add (new Point ((int)(ρ - ρ * LogoDrawerSupport.Cosinus ((double)φ +
					(double)i * 360.0 / (double)sidesCount)),
					(int)(ρ - ρ * LogoDrawerSupport.Sinus ((double)φ +
					(double)i * 360.0 / (double)sidesCount))));

				if (star)
					points.Add (new Point ((int)(ρ - ρ * 0.25 * LogoDrawerSupport.Cosinus ((double)φ +
						((double)i + 0.5) * 360.0 / (double)sidesCount)),
						(int)(ρ - ρ * 0.25 * LogoDrawerSupport.Sinus ((double)φ +
						((double)i + 0.5) * 360.0 / (double)sidesCount))));
				}

			Point[] res = points.ToArray ();
			points.Clear ();

			// Формирование изображения
			if (ρ == 0)
				image = new Bitmap (2, 2);
			else
				image = new Bitmap (2 * (int)ρ, 2 * (int)ρ);
			Graphics g = Graphics.FromImage (image);
			if (ρ != 0)
				g.FillPolygon (objectBrush, res);
			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) || (speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) || (speedY < 0) && (y < endY))
				isInited = false;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'буква' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerLetter:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private float speedX, initSpeedX;	// Скорость горизонтального смещения, начальная скорость
		private float speedY, initSpeedY;	// Скорость вертикального смещения, начальная скорость
		private int speedOfRotation;	// Скорость вращения
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private int φ;					// Угол поворота объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return resultImage;
				}
			}
		private Bitmap resultImage, sourceImage;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (resultImage != null)
				resultImage.Dispose ();
			if (sourceImage != null)
				sourceImage.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'правильный многоугольник'
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		/// <param name="ScreenCenterX">Абсцисса изменённого центра экрана</param>
		/// <param name="ScreenCenterY">Ордината изменённого центра экрана</param>
		public LogoDrawerLetter (uint ScreenWidth, uint ScreenHeight, uint ScreenCenterX, uint ScreenCenterY,
			Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = Metrics;	// LogoDrawerSupport.AlingMetrics (Metrics);
			if (Randomizer == null)
				return;

			// Генерация параметров изображения
			rnd = Randomizer;

			φ = rnd.Next (0, 360);
			if (metrics.Rotation)
				{
				speedOfRotation = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				}
			else
				{
				speedOfRotation = 0;
				}
			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			// Генерация изображения
			SolidBrush sb = new SolidBrush (Color.FromArgb (rnd.Next (128, 256), rnd.Next (metrics.MinRed, metrics.MaxRed + 1),
				rnd.Next (metrics.MinGreen, metrics.MaxGreen + 1), rnd.Next (metrics.MinBlue, metrics.MaxBlue + 1)));
			int size = rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize + 1);

			sourceImage = new Bitmap (size * 2, size * 2);
			Graphics g = Graphics.FromImage (sourceImage);

			Font f = new Font ("Arial Black", size, FontStyle.Bold);
			string s = Encoding.GetEncoding (1251).GetString (new byte[] { (byte)rnd.Next (192, 192 + 32) });
			SizeF sz = g.MeasureString (s, f);

			g.DrawString (s, f, sb, (sourceImage.Width - sz.Width) / 2, (sourceImage.Height - sz.Height) / 2);

			g.Dispose ();
			f.Dispose ();
			sb.Dispose ();

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				//case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
					//case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}

					/*if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))*/
					endY = y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
					/*else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));*/
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
					break;

				case LogoDrawerObjectStartupPositions.LeftTop:
				case LogoDrawerObjectStartupPositions.LeftBottom:
				case LogoDrawerObjectStartupPositions.RightTop:
				case LogoDrawerObjectStartupPositions.RightBottom:
					while ((speedX == 0) && (speedY == 0))
						{
						speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.RightTop))
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftBottom))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				//case LogoDrawerObjectStartupPositions.CenterFlat:
				case LogoDrawerObjectStartupPositions.ToCenterRandom:
				default:
					while ((speedX == 0) && (speedY == 0) /*||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0)*/)
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						/*if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else*/
						speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenCenterX;	// ScreenWidth / 2;
						/*if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else*/
						y = (int)ScreenCenterY;	// (ScreenHeight / 2);
						}
					else if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (int)ScreenCenterX;	// ScreenWidth / 2;
						endY = (int)ScreenCenterY;	// ScreenHeight / 2;

						if (Math.Abs (speedX) > Math.Abs (speedY))
							{
							x = (speedX < 0) ? ((int)(ScreenWidth + speedX) + sourceImage.Width + maxFluctuation) :
								(-sourceImage.Width + (int)speedX - maxFluctuation);
							y = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, x, true);
							}
						else
							{
							y = (speedY < 0) ? ((int)(ScreenHeight + speedY) + sourceImage.Height + maxFluctuation) :
								(-sourceImage.Height + (int)speedY - maxFluctuation);
							x = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, y, false);
							}
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
						y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
						}

					if (metrics.StartupPosition != LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (speedX > 0) ? ((int)(ScreenWidth + speedX) + sourceImage.Width + maxFluctuation) :
							(-sourceImage.Width + (int)speedX - maxFluctuation);
						endY = (speedY > 0) ? ((int)(ScreenHeight + speedY) + sourceImage.Height + maxFluctuation) :
							(-sourceImage.Height + (int)speedY - maxFluctuation);
						}

					break;
				}

			// Успешно
			initSpeedX = Math.Sign (speedX);
			initSpeedY = (speedX * speedY == 0.0f) ? 1 : (speedY / Math.Abs (speedX));

			isInited = true;
			Move (0, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении (неактивен)</param>
		public void Move (uint Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += ((int)speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			speedX += Acceleration * initSpeedX / 10.0f;

			y += ((int)speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			//if ((speedY > LogoDrawerSupport.AccelerationBorder) || (speedY < -LogoDrawerSupport.AccelerationBorder))
			speedY += Acceleration * initSpeedY / 10.0f;

			φ += speedOfRotation;
			while (φ < 0)
				φ += 360;
			while (φ > 359)
				φ -= 360;

			// Перерисовка
			if (resultImage != null)
				resultImage.Dispose ();

			resultImage = new Bitmap (sourceImage.Width, sourceImage.Height);
			Graphics g = Graphics.FromImage (resultImage);

			g.TranslateTransform (sourceImage.Width / 2, sourceImage.Height / 2);			// Центровка поворота
			g.RotateTransform (φ);
			g.DrawImage (sourceImage, -sourceImage.Width / 2, -sourceImage.Height / 2);
			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) || (speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) || (speedY < 0) && (y < endY))
				isInited = false;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'картинка' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerPicture:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private float speedX, initSpeedX;	// Скорость горизонтального смещения, начальная скорость
		private float speedY, initSpeedY;	// Скорость вертикального смещения, начальная скорость
		private int speedOfRotation;	// Скорость вращения
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private int φ;					// Угол поворота объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return resultImage;
				}
			}
		private Bitmap resultImage, sourceImage;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (resultImage != null)
				resultImage.Dispose ();
			if (sourceImage != null)
				sourceImage.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'картинка' на основе случайно выбранного изображения
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		/// <param name="PicturesPath">Директория с изображениями</param>
		/// <param name="ScreenCenterX">Абсцисса изменённого центра экрана</param>
		/// <param name="ScreenCenterY">Ордината изменённого центра экрана</param>
		public LogoDrawerPicture (uint ScreenWidth, uint ScreenHeight, uint ScreenCenterX, uint ScreenCenterY,
			Random Randomizer, LogoDrawerObjectMetrics Metrics, string PicturesPath)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = Metrics;	// LogoDrawerSupport.AlingMetrics (Metrics);
			if (Randomizer == null)
				return;

			// Генерация параметров изображения
			rnd = Randomizer;

			if (metrics.Rotation)
				{
				speedOfRotation = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				φ = rnd.Next (0, 360);
				}
			else
				{
				speedOfRotation = 0;
				φ = 0;
				}

			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			// Получение изображения
			try
				{
				string[] files = Directory.GetFiles (PicturesPath);
				Bitmap b = (Bitmap)Bitmap.FromFile (files[(files.Length > 1) ? rnd.Next (files.Length) : 0]);
				int size = rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize);
				Bitmap b2 = new Bitmap (b, (int)((double)size * (double)b.Width / (double)b.Height), size);

				sourceImage = new Bitmap (2 * b2.Width, 2 * b2.Height);
				Graphics g = Graphics.FromImage (sourceImage);

				g.DrawImage (b2, (sourceImage.Width - b2.Width) / 2, (sourceImage.Height - b2.Height) / 2);

				g.Dispose ();
				b.Dispose ();
				b2.Dispose ();
				}
			catch
				{
				sourceImage = new Bitmap (2, 2);
				}

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				//case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
					//case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}

					/*if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))*/
					endY = y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
					/*else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));*/
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
					break;

				case LogoDrawerObjectStartupPositions.LeftTop:
				case LogoDrawerObjectStartupPositions.LeftBottom:
				case LogoDrawerObjectStartupPositions.RightTop:
				case LogoDrawerObjectStartupPositions.RightBottom:
					while ((speedX == 0) && (speedY == 0))
						{
						speedX = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						speedY = rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.RightTop))
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					if ((metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftTop) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.LeftBottom))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				//case LogoDrawerObjectStartupPositions.CenterFlat:
				case LogoDrawerObjectStartupPositions.ToCenterRandom:
				default:
					while ((speedX == 0) && (speedY == 0) /*||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0)*/)
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						/*if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else*/
						speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenCenterX;	// ScreenWidth / 2;
						/*if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else*/
						y = (int)ScreenCenterY;	// (ScreenHeight / 2);
						}
					else if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (int)ScreenCenterX;	// ScreenWidth / 2;
						endY = (int)ScreenCenterY;	// ScreenHeight / 2;

						if (Math.Abs (speedX) > Math.Abs (speedY))
							{
							x = (speedX < 0) ? ((int)(ScreenWidth + speedX) + sourceImage.Width + maxFluctuation) :
								(-sourceImage.Width + (int)speedX - maxFluctuation);
							y = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, x, true);
							}
						else
							{
							y = (speedY < 0) ? ((int)(ScreenHeight + speedY) + sourceImage.Height + maxFluctuation) :
								(-sourceImage.Height + (int)speedY - maxFluctuation);
							x = LogoDrawerSupport.EvaluateLinearCoordinate (endX, endY, (int)speedX, (int)speedY, y, false);
							}
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
						y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
						}

					if (metrics.StartupPosition != LogoDrawerObjectStartupPositions.ToCenterRandom)
						{
						endX = (speedX > 0) ? ((int)(ScreenWidth + speedX) + sourceImage.Width + maxFluctuation) :
							(-sourceImage.Width + (int)speedX - maxFluctuation);
						endY = (speedY > 0) ? ((int)(ScreenHeight + speedY) + sourceImage.Height + maxFluctuation) :
							(-sourceImage.Height + (int)speedY - maxFluctuation);
						}

					break;
				}

			// Успешно
			initSpeedX = Math.Sign (speedX);
			initSpeedY = (speedX * speedY == 0.0f) ? 1 : (speedY / Math.Abs (speedX));

			isInited = true;
			Move (0, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении (неактивен)</param>
		public void Move (uint Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += ((int)speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			speedX += Acceleration * initSpeedX / 10.0f;

			y += ((int)speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			//if ((speedY > LogoDrawerSupport.AccelerationBorder) || (speedY < -LogoDrawerSupport.AccelerationBorder))
			speedY += Acceleration * initSpeedY / 10.0f;

			φ += speedOfRotation;
			while (φ < 0)
				φ += 360;
			while (φ > 359)
				φ -= 360;

			// Перерисовка
			if (resultImage != null)
				resultImage.Dispose ();

			resultImage = new Bitmap (sourceImage.Width, sourceImage.Height);
			Graphics g = Graphics.FromImage (resultImage);

			g.TranslateTransform (sourceImage.Width / 2, sourceImage.Height / 2);			// Центровка поворота
			g.RotateTransform (φ);

			g.DrawImage (sourceImage, -sourceImage.Width / 2, -sourceImage.Height / 2);
			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) || (speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) || (speedY < 0) && (y < endY))
				isInited = false;
			}
		}

	/// <summary>
	/// Интерфейс описывает визуальный объект
	/// </summary>
	public interface ILogoDrawerObject
		{
		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		void Dispose ();

		/// <summary>
		/// Изображение объекта
		/// </summary>
		Bitmap Image
			{
			get;
			}

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		bool IsInited
			{
			get;
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Коэффициент ускорения</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		void Move (uint Acceleration, int Enlarging);

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		int X
			{
			get;
			}

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		int Y
			{
			get;
			}
		}
	}
