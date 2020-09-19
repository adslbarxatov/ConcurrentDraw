// Общий заголовок
#include "ConcurrentDrawLib.h"

// Общие константы
#define FP_QMAX		64
#define FP_HMAX		128
#define FP_AMAX		192
#define FP_MAX		CD_BMPINFO_MAXCOLOR

// Макроподстановки
#define FP_PALETTE(a,b,c,d,m,g)	for (i = 0; i < FP_QMAX; i++) {\
								FP_Q1 a FP_Q2 b FP_Q3 c FP_Q4 d }\
								for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++) {\
								FP_B m }\
								AS->cdBackgroundColorNumber = g;

#define FP_Q1(r,g,b)	AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = r;\
						AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = g;\
						AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = b;
#define FP_QM(d,r,g,b)	AS->sgBMPInfo.cd_bmpinfo.colors[d + i].rgbRed = r;\
						AS->sgBMPInfo.cd_bmpinfo.colors[d + i].rgbGreen = g;\
						AS->sgBMPInfo.cd_bmpinfo.colors[d + i].rgbBlue = b;
#define FP_Q2(r,g,b)	FP_QM(FP_QMAX,r,g,b)
#define FP_Q3(r,g,b)	FP_QM(FP_HMAX,r,g,b)
#define FP_Q4(r,g,b)	FP_QM(FP_AMAX,r,g,b)

#define FP_B(r,g,b)		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = r;\
						AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = g;\
						AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = b;

#define FPPR_UPDATE(member,limit)	member += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);\
									if (member < FPPR_RND_LIMIT) member = FP_MAX;\
									if (member < limit) member = limit;
#define FPPR_ENLIGHT	while ((AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed + AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen +\
							AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue) < FPPR_CLR_MIN_SUMMA) {\
							AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed += ((AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed < CD_BMPINFO_MAXCOLOR) ? 1 : 0);\
							AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen += ((AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen < CD_BMPINFO_MAXCOLOR) ? 1 : 0);\
							AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue += ((AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue < CD_BMPINFO_MAXCOLOR) ? 1 : 0); }

// Функции, инициализирующие палитры
#define FPPR_RND_LIMIT		10
#define FPPR_CLR_MIN_MONO	32
#define FPPR_CLR_MIN_POLY	64
#define FPPR_CLR_MIN_SUMMA	0x1E0

#define FPPR_HIGH_INDEX		Reversed ? 1 : 3
#define FPPR_LOW_INDEX		Reversed ? 3 : 1
#define FPPR_MID_INDEX		2
#define FPPR_TOP_INDEX		Reversed ? 0 : 4
#define FPPR_BOTTOM_INDEX	Reversed ? 4 : 0

void FillPalette_PolymorphRandom (uchar Reversed, uchar Polymorph, uchar Monocolor)
	{
	uint i, j;

	// Смещение опорных цветов
	AS->cdPolymorphColors[FPPR_BOTTOM_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_BOTTOM_INDEX].rgbGreen = 
		AS->cdPolymorphColors[FPPR_BOTTOM_INDEX].rgbBlue = 0;
	AS->cdPolymorphColors[FPPR_TOP_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_TOP_INDEX].rgbGreen = 
		AS->cdPolymorphColors[FPPR_TOP_INDEX].rgbBlue = CD_BMPINFO_MAXCOLOR;

	if (Polymorph)
		{
		if (Monocolor)
			{
			// Смещение
			FPPR_UPDATE (AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed, FPPR_CLR_MIN_MONO);
			FPPR_UPDATE (AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen, FPPR_CLR_MIN_MONO);
			FPPR_UPDATE (AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue, FPPR_CLR_MIN_MONO);

			// Осветление
			FPPR_ENLIGHT

			// Распространение
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed / 2;
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbGreen = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen / 2;
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbBlue = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue / 2;

			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed / 4;
			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbGreen = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen / 4;
			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbBlue = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue / 4;
			}
		else
			{
			for (i = 1; i < 4; i++)
				{
				FPPR_UPDATE (AS->cdPolymorphColors[i].rgbRed, FPPR_CLR_MIN_POLY);
				FPPR_UPDATE (AS->cdPolymorphColors[i].rgbGreen, FPPR_CLR_MIN_POLY);
				FPPR_UPDATE (AS->cdPolymorphColors[i].rgbBlue, FPPR_CLR_MIN_POLY);
				}
			}
		}

	// Обновление опорных цветов
	else
		{
		if (Monocolor)
			{
			// Генерация
			AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed = (BYTE)GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX);
			AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen = (BYTE)GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX);
			AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue = (BYTE)GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX);

			// Осветление
			FPPR_ENLIGHT

			// Распространение
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed / 2;
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbGreen = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen / 2;
			AS->cdPolymorphColors[FPPR_MID_INDEX].rgbBlue = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue / 2;

			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbRed = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed / 4;
			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbGreen = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen / 4;
			AS->cdPolymorphColors[FPPR_LOW_INDEX].rgbBlue = AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue / 4;
			}
		else
			{
			for (i = 1; i < 4; i++)
				{
				AS->cdPolymorphColors[i].rgbRed = (BYTE)GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX);
				AS->cdPolymorphColors[i].rgbGreen = (BYTE)GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX);
				AS->cdPolymorphColors[i].rgbBlue = (BYTE)GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX);
				}
			}
		}

	// Заполнение остальных цветов
	for (i = 0; i < FP_QMAX; i++)
		{
		for (j = 0; j < 4; j++)
			{
			AS->sgBMPInfo.cd_bmpinfo.colors[j * FP_QMAX + i].rgbRed = ((FP_QMAX - 1 - i) * AS->cdPolymorphColors[j].rgbRed +
				i * AS->cdPolymorphColors[j + 1].rgbRed) / FP_QMAX;
			AS->sgBMPInfo.cd_bmpinfo.colors[j * FP_QMAX + i].rgbGreen = ((FP_QMAX - 1 - i) * AS->cdPolymorphColors[j].rgbGreen +
				i * AS->cdPolymorphColors[j + 1].rgbGreen) / FP_QMAX;
			AS->sgBMPInfo.cd_bmpinfo.colors[j * FP_QMAX + i].rgbBlue = ((FP_QMAX - 1 - i) * AS->cdPolymorphColors[j].rgbBlue +
				i * AS->cdPolymorphColors[j + 1].rgbBlue) / FP_QMAX;
			}
		}

	// Заполнение цветов бит-детектора
	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		FP_B ((Reversed ? (CD_BMPINFO_MAXCOLOR - i) : i) * AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbRed / CD_BMPINFO_COLORS_COUNT, 
			(Reversed ? (CD_BMPINFO_MAXCOLOR - i) : i) * AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbGreen / CD_BMPINFO_COLORS_COUNT,
			(Reversed ? (CD_BMPINFO_MAXCOLOR - i) : i) * AS->cdPolymorphColors[FPPR_HIGH_INDEX].rgbBlue / CD_BMPINFO_COLORS_COUNT);
		}

	// Цвет фона спектрограмм
	AS->cdBackgroundColorNumber = Reversed ? 224 : 8;
	}

// Функция формирует палитру приложения
#define FP_SPECIAL_PALETTES	case 12:\
							case 13:\
							case 14:\
							case 15:\
							case 16:\
							case 17:\
							case 18:\
							case 19:
#define FP_SP_BASE			19

CD_API(void) FillPaletteEx (uchar PaletteNumber)
	{
	// Установка параметров
	uint i;
	uchar polymorphResetNotRequired = (AS->cdPolymorphUpdateCounter >= POLYMORPH_UPDATE_PAUSE) * 2;
	AS->cdPolymorphUpdateCounter = 0;
	AS->cdCurrentPalette = PaletteNumber;

	// Выбор палитры
	switch (PaletteNumber)
		{
		// Стандартная
		default:
		case 0:
			FP_PALETTE (
				(0, 0, 2 * i), 
				(4 * i, 0, 2 * (FP_QMAX + i)),
				(FP_MAX, 4 * i, 4 * (FP_QMAX - 1 - i)),
				(FP_MAX, FP_MAX, 4 * i),
				(i / 2, 0, i), 8)
			AS->cdCurrentPalette = 0;
			break;

		// Море
		case 1:
			FP_PALETTE (
				(0, 0, 3 * i),
				(0, 2 * i, FP_AMAX + i),
				(0, 2 * (FP_QMAX + i), FP_MAX),
				(4 * i, FP_MAX, FP_MAX),
				(0, 2 * i / 3, i), 8)
			break;

		// Огонь
		case 2:
			FP_PALETTE (
				(3 * i, 0, 0),
				(FP_AMAX + i, 2 * i, 0),
				(FP_MAX, 2 * (FP_QMAX + i), 0),
				(FP_MAX, FP_MAX, 4 * i),
				(i, 2 * i / 3, 0), 8)
			break;

		// Серая
		case 3:
			FP_PALETTE (
				(i / 2, i / 2, i / 2),
				((FP_QMAX + i) / 2, (FP_QMAX + i) / 2, (FP_QMAX + i) / 2),
				(FP_QMAX + i, FP_QMAX + i, FP_QMAX + i),
				(2 * (FP_QMAX + i), 2 * (FP_QMAX + i), 2 * (FP_QMAX + i)),
				(4 * i / 5, 4 * i / 5, 4 * i / 5), 8)
			break;

		// Рассвет
		case 4:
			FP_PALETTE (
				(0, 0, 2 * i),
				(0, 3 * i, 2 * (FP_QMAX - i)),
				(4 * i, FP_AMAX - i, 0),
				(FP_MAX, 2 * (FP_QMAX + i), 4 * i),
				(0, 3 * i / 4, 0), 8)
			break;

		// Кислота
		case 5:
			FP_PALETTE (
				(0, i, 0),
				(0, FP_QMAX + i, 0),
				(0, 2 * (FP_QMAX + i), 0),
				(4 * i, FP_MAX, 4 * i),
				(0, i, 0), 8)
			break;

		// 7 пропущенных
		case 6:
			FP_PALETTE (
				(3 * i, 0, 2 * i),
				(3 * (FP_QMAX - i), 3 * i / 2, FP_HMAX + i),
				(2 * i, (FP_AMAX + 5 * i) / 2, FP_AMAX - i),
				(2 * (FP_QMAX + i), FP_MAX, 2 * (FP_QMAX + i)),
				(i, 0, 3 * i / 4), 8)
			break;

		// Парус
		case 7:
			FP_PALETTE (
				(0, 0, 3 * i),
				(2 * i, 0, FP_AMAX + i),
				(2 * (FP_QMAX + i), 0, 4 * (FP_QMAX - 1 - i)),
				(FP_MAX, 4 * i, 4 * i),
				(i, 0, 0), 8)
			break;

		// Зеркало
		case 8:
			FP_PALETTE (
				(i, i, i),
				(FP_QMAX + i, FP_QMAX + i, FP_QMAX + i),
				(2 * (FP_QMAX + i), 2 * (FP_QMAX + i), 2 * (FP_QMAX - i)),
				(FP_MAX, FP_MAX, 4 * i),
				(3 * i / 4, 3 * i / 4, 3 * i / 4), 8)
			break;

		// Кровь
		case 9:
			FP_PALETTE (
				(i, 0, 0),
				(FP_QMAX + i, 0, 0),
				(2 * (FP_QMAX + i), 0, 0),
				(FP_MAX, 4 * i, 4 * i),
				(i, 0, 0), 8)
			break;

		// Лимон
		case 10:
			FP_PALETTE (
				(i, i, 0),
				(FP_QMAX + i, FP_QMAX + i, 0),
				(2 * (FP_QMAX + i), 2 * (FP_QMAX + i), 0),
				(FP_MAX, FP_MAX, 4 * i),
				(i, i, 0), 8)
			break;

		// Ла фиеста
		case 11:
			FP_PALETTE (
				(0, 2 * i, i),
				(0, FP_HMAX + i, FP_QMAX - i),
				(4 * i, FP_AMAX + i, 0),
				(FP_MAX, FP_MAX, 4 * i),
				(0, i, i / 2), 8)
			break;

		// Негатив
		case FP_SP_BASE + 1:
			FP_PALETTE (
				(FP_MAX - i, FP_MAX - i, FP_MAX - i),
				(FP_AMAX - i, FP_AMAX - i, FP_AMAX - i),
				(FP_HMAX - i, FP_HMAX - i, FP_HMAX - i),
				(FP_QMAX - i, FP_QMAX - i, FP_QMAX - i),
				(FP_MAX - i, FP_MAX - i, FP_MAX - i), 224)
			break;

		// Огонь обратный
		case FP_SP_BASE + 2:
			FP_PALETTE (
				(FP_MAX, FP_MAX, FP_AMAX - 3 * i),
				(FP_MAX, FP_MAX - 2 * i, 0),
				(FP_MAX - 2 * i, FP_HMAX - 2 * i, 0),
				(FP_HMAX - 2 * i, 0, 0),
				(FP_MAX - 3 * i / 4, FP_AMAX - 3 * i / 4, FP_HMAX - i / 2), 224)
			break;

		// Море обратное
		case FP_SP_BASE + 3:
			FP_PALETTE (
				(FP_AMAX - 3 * i, FP_MAX, FP_MAX),
				(0, FP_MAX - 2 * i, FP_MAX),
				(0, FP_HMAX - 2 * i, FP_MAX - 2 * i),
				(0, 0, FP_HMAX - 2 * i),
				(FP_HMAX - i / 2, FP_AMAX - 3 * i / 4, FP_MAX - 3 * i / 4), 224)
			break;

		// Пурпурная
		case FP_SP_BASE + 4:
			FP_PALETTE (
				(FP_MAX - 2 * i, FP_AMAX - 3 * i, FP_MAX),
				(FP_HMAX - i, 0, FP_MAX - i),
				(FP_QMAX - i / 2, 0, FP_AMAX - i),
				(32 - i / 2, 0, FP_HMAX - 2 * i),
				(FP_AMAX - 3 * i / 4, FP_HMAX - i / 2, FP_MAX - 3 * i / 4), 224)
			break;

		// Кровь обратная
		case FP_SP_BASE + 5:
			FP_PALETTE (
				(FP_MAX, FP_AMAX - 3 * i, FP_AMAX - 3 * i),
				(FP_MAX - 2 * i, 0, 0),
				(FP_HMAX - i, 0, 0),
				(FP_QMAX - i, 0, 0),
				(FP_MAX - 3 * i / 4, FP_HMAX - i / 2, FP_HMAX - i / 2), 224)
			break;

		// Кислота обратная
		case FP_SP_BASE + 6:
			FP_PALETTE (
				(FP_AMAX - 3 * i, FP_MAX, FP_AMAX - 3 * i),
				(0, FP_MAX - 2 * i, 0),
				(0, FP_HMAX - i, 0),
				(0, FP_QMAX - i, 0),
				(FP_HMAX - i / 2, FP_MAX - 3 * i / 4, FP_HMAX - i / 2), 224)
			break;

		// Лимон обратная
		case FP_SP_BASE + 7:
			FP_PALETTE (
				(FP_MAX, FP_MAX, FP_AMAX - 3 * i),
				(FP_MAX - 2 * i, FP_MAX - 2 * i, 0),
				(FP_HMAX - i, FP_HMAX - i, 0),
				(FP_QMAX - i, FP_QMAX - i, 0),
				(FP_MAX - 3 * i / 4, FP_MAX - 3 * i / 4, FP_HMAX - i / 2), 224)
			break;

		// Полиморфная и случайная
		FP_SPECIAL_PALETTES
			FillPalette_PolymorphRandom (0x4 - (PaletteNumber & 0x4), PaletteNumber & 0x2 & polymorphResetNotRequired, PaletteNumber & 0x1);
			AS->cdPolymorphUpdateCounter = PaletteNumber & 0x2;
			break;
		}
	}

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Adobe Audition" NAMES_DELIMITER_S \
		"Sea" NAMES_DELIMITER_S \
		"Fire" NAMES_DELIMITER_S \
		"Grey" NAMES_DELIMITER_S \
		"Sunrise" NAMES_DELIMITER_S \
		"Acid" NAMES_DELIMITER_S \
		"7 missed calls" NAMES_DELIMITER_S \
		"Sail on the sea" NAMES_DELIMITER_S \
		"Mirror" NAMES_DELIMITER_S \
		"Blood" NAMES_DELIMITER_S \
		\
		"Lemon" NAMES_DELIMITER_S \
		"La fiesta" NAMES_DELIMITER_S \
		"Random" NAMES_DELIMITER_S \
		"Random monocolor" NAMES_DELIMITER_S \
		"Polymorphic" NAMES_DELIMITER_S \
		"Polymorphic monocolor" NAMES_DELIMITER_S \
		"Random (reversed)" NAMES_DELIMITER_S \
		"Random monocolor (reversed)" NAMES_DELIMITER_S \
		"Polymorphic (reversed)" NAMES_DELIMITER_S \
		"Polymorphic monocolor (reversed)" NAMES_DELIMITER_S \
		\
		"Negative" NAMES_DELIMITER_S \
		"Fire (reversed)" NAMES_DELIMITER_S \
		"Sea (reversed)" NAMES_DELIMITER_S \
		"Purple" NAMES_DELIMITER_S \
		"Blood (reversed)" NAMES_DELIMITER_S \
		"Acid (reversed)" NAMES_DELIMITER_S \
		"Lemon (reversed)")

	return PALETTES_NAMES;
	}

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber)
	{
	return 0xFF000000 | 
		(AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbRed << 16) |
		(AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbGreen << 8) | 
		AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbBlue;
	}

// Функция получает цвет фона текущей палитры
CD_API(ulong) GetPaletteBackgroundColorEx ()
	{
	return 0xFF000000 | 
		(AS->sgBMPInfo.cd_bmpinfo.colors[AS->cdBackgroundColorNumber].rgbRed << 16) |
		(AS->sgBMPInfo.cd_bmpinfo.colors[AS->cdBackgroundColorNumber].rgbGreen << 8) | 
		AS->sgBMPInfo.cd_bmpinfo.colors[AS->cdBackgroundColorNumber].rgbBlue;
	}

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness)
	{
	return ((ulong)Brightness << 24) | 
		(AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbRed << 16) |
		(AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbGreen << 8) | 
		AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbBlue;
	}

// Функция возвращает рекомендацию на сброс лого по признаку спецпалитр
CD_API(uchar) PaletteRequiresResetEx (uchar PaletteNumber)
	{
	switch (PaletteNumber)
		{
		FP_SPECIAL_PALETTES
			return 1;

		default:
			return 0;
		}
	}

// Функция возвращает псевдослучайное число между Min и Max, включая границы
sint GetRandomValue (sint Min, sint Max)
	{
	return (sint)((Max - Min + 1) * (sdlong)rand () / (RAND_MAX + 1) + Min);
	}
