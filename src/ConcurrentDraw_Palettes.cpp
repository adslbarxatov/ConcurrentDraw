// Общий заголовок
#include "ConcurrentDrawLib.h"

// Функции, инициализирующие отдельные палитры
#define FP_QMAX		64
#define FP_HMAX		128
#define FP_AMAX		192
#define FP_MAX		255

void FillPalette_Default (void)
	{
	uint i;

	// Основная палитра
	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;
		
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 4 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = 2 * (FP_QMAX + i);

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 4 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 4 * (FP_QMAX - 1 - i);

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 * i;
		}

	// Палитра бит-детектора
	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = i / 2; 
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = i;
		}
	}

void FillPalette_Sea (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 2 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = FP_MAX;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = FP_MAX;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = 4 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = FP_MAX;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = i;
		}
	}

void FillPalette_Fire (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 4 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 2 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 * i;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = i; 
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = i / 4;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
		}
	}

void FillPalette_Grey (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = i / 2;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = (FP_QMAX + i) / 2;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = FP_QMAX + i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 2 * (FP_QMAX + i);
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed =  
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i / 5;
		}
	}

void FillPalette_Sunrise (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 3 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = 2 * (FP_QMAX - i);

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 4 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = FP_AMAX - i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 * i;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = 3 * i / 4;
		}
	}

void FillPalette_Acid (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = i;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = FP_QMAX + i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 *  i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = FP_MAX;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = i;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
		}
	}

void FillPalette_7MissedCalls (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 3 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 2 * i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 3 * (FP_QMAX - i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 3 * i / 2;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = FP_HMAX + i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 2 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = (FP_AMAX + 5 * i) / 2;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = FP_AMAX - i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = FP_MAX;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = 3 * i / 4;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = i / 2;
		}
	}

void FillPalette_SailOnTheSea (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 4 * i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 2 * i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = FP_MAX;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 0;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 4 * (FP_QMAX - 1 - i);

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 * i;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = i;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
		}
	}

void FillPalette_Mirror (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = FP_QMAX + i;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 2 * (FP_QMAX - i);

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = 
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen = FP_MAX;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 * i;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 3 * i / 4;
		}
	}

void FillPalette_Blood (void)
	{
	uint i;

	for (i = 0; i < FP_QMAX; i++) 
		{
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbRed = i;
		AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbGreen = 
			AS->sgBMPInfo.cd_bmpinfo.colors[i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbRed = FP_QMAX + i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbGreen =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_QMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbRed = 2 * (FP_QMAX + i);
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbGreen =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_HMAX + i].rgbBlue = 0;

		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbGreen =
			AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbBlue = 4 *  i;
		AS->sgBMPInfo.cd_bmpinfo.colors[FP_AMAX + i].rgbRed = FP_MAX;
		}

	for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++)
		{
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = i;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen =
			AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = 0;
		}
	}

#define FPPR_RND_LIMIT		10
#define FPPR_CLR_MIN_MONO	32
#define FPPR_CLR_MIN_POLY	32

void FillPalette_PolymorphRandom (uchar Polymorph, uchar Monocolor)
	{
	uint i, j;

	// Смещение опорных цветов
	if (Polymorph)
		{
		if (Monocolor)
			{
			AS->cdPolymorphColors[3].rgbRed += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
			if (AS->cdPolymorphColors[3].rgbRed < FPPR_RND_LIMIT)
				AS->cdPolymorphColors[3].rgbRed = FP_MAX;
			if (AS->cdPolymorphColors[3].rgbRed < FPPR_CLR_MIN_MONO)
				AS->cdPolymorphColors[3].rgbRed = FPPR_CLR_MIN_MONO;

			AS->cdPolymorphColors[3].rgbGreen += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
			if (AS->cdPolymorphColors[3].rgbGreen < FPPR_RND_LIMIT)
				AS->cdPolymorphColors[3].rgbGreen = FP_MAX;
			if (AS->cdPolymorphColors[3].rgbGreen < FPPR_CLR_MIN_MONO)
				AS->cdPolymorphColors[3].rgbGreen = FPPR_CLR_MIN_MONO;

			AS->cdPolymorphColors[3].rgbBlue += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
			if (AS->cdPolymorphColors[3].rgbBlue < FPPR_RND_LIMIT)
				AS->cdPolymorphColors[3].rgbBlue = FP_MAX;
			if (AS->cdPolymorphColors[3].rgbBlue < FPPR_CLR_MIN_MONO)
				AS->cdPolymorphColors[3].rgbBlue = FPPR_CLR_MIN_MONO;

			AS->cdPolymorphColors[2].rgbRed = AS->cdPolymorphColors[3].rgbRed / 2;
			AS->cdPolymorphColors[2].rgbGreen = AS->cdPolymorphColors[3].rgbGreen / 2;
			AS->cdPolymorphColors[2].rgbBlue = AS->cdPolymorphColors[3].rgbBlue / 2;

			AS->cdPolymorphColors[1].rgbRed = AS->cdPolymorphColors[3].rgbRed / 4;
			AS->cdPolymorphColors[1].rgbGreen = AS->cdPolymorphColors[3].rgbGreen / 4;
			AS->cdPolymorphColors[1].rgbBlue = AS->cdPolymorphColors[3].rgbBlue / 4;
			}
		else
			{
			for (i = 1; i < 4; i++)
				{
				AS->cdPolymorphColors[i].rgbRed += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
				if (AS->cdPolymorphColors[i].rgbRed < FPPR_RND_LIMIT)
					AS->cdPolymorphColors[i].rgbRed = FP_MAX;
				if (AS->cdPolymorphColors[i].rgbRed < FPPR_CLR_MIN_POLY)
					AS->cdPolymorphColors[i].rgbRed = FPPR_CLR_MIN_POLY;

				AS->cdPolymorphColors[i].rgbGreen += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
				if (AS->cdPolymorphColors[i].rgbGreen < FPPR_RND_LIMIT)
					AS->cdPolymorphColors[i].rgbGreen = FP_MAX;
				if (AS->cdPolymorphColors[i].rgbGreen < FPPR_CLR_MIN_POLY)
					AS->cdPolymorphColors[i].rgbGreen = FPPR_CLR_MIN_POLY;

				AS->cdPolymorphColors[i].rgbBlue += GetRandomValue (-FPPR_RND_LIMIT, FPPR_RND_LIMIT);
				if (AS->cdPolymorphColors[i].rgbBlue < FPPR_RND_LIMIT)
					AS->cdPolymorphColors[i].rgbBlue = FP_MAX;
				if (AS->cdPolymorphColors[i].rgbBlue < FPPR_CLR_MIN_POLY)
					AS->cdPolymorphColors[i].rgbBlue = FPPR_CLR_MIN_POLY;
				}
			}
		}

	// Обновление опорных цветов
	else
		{
		if (Monocolor)
			{
			AS->cdPolymorphColors[3].rgbRed = GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX + 1);
			AS->cdPolymorphColors[3].rgbGreen = GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX + 1);
			AS->cdPolymorphColors[3].rgbBlue = GetRandomValue (FPPR_CLR_MIN_MONO, FP_MAX + 1);

			AS->cdPolymorphColors[2].rgbRed = AS->cdPolymorphColors[3].rgbRed / 2;
			AS->cdPolymorphColors[2].rgbGreen = AS->cdPolymorphColors[3].rgbGreen / 2;
			AS->cdPolymorphColors[2].rgbBlue = AS->cdPolymorphColors[3].rgbBlue / 2;

			AS->cdPolymorphColors[1].rgbRed = AS->cdPolymorphColors[3].rgbRed / 4;
			AS->cdPolymorphColors[1].rgbGreen = AS->cdPolymorphColors[3].rgbGreen / 4;
			AS->cdPolymorphColors[1].rgbBlue = AS->cdPolymorphColors[3].rgbBlue / 4;
			}
		else
			{
			for (i = 1; i < 4; i++)
				{
				AS->cdPolymorphColors[i].rgbRed = GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX + 1);
				AS->cdPolymorphColors[i].rgbGreen = GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX + 1);
				AS->cdPolymorphColors[i].rgbBlue = GetRandomValue (FPPR_CLR_MIN_POLY, FP_MAX + 1);
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
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbRed = i * AS->cdPolymorphColors[3].rgbRed / CD_BMPINFO_COLORS_COUNT;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbGreen = i * AS->cdPolymorphColors[3].rgbGreen / CD_BMPINFO_COLORS_COUNT;
		AS->sgBeatsInfo.cd_bmpinfo.colors[i].rgbBlue = i * AS->cdPolymorphColors[3].rgbBlue / CD_BMPINFO_COLORS_COUNT;
		}
	}

// Функция формирует палитру приложения
CD_API(void) FillPaletteEx (uchar PaletteNumber)
	{
	// Установка параметров
	uchar polymorphResetNotRequired = (AS->cdPolymorphUpdateCounter >= POLYMORPH_UPDATE_PAUSE) * 2;
	AS->cdPolymorphUpdateCounter = 0;

	AS->cdCurrentPalette = PaletteNumber;

	// Выбор палитры
	switch (PaletteNumber)
		{
		// Стандартная
		default:
		case 0:
			FillPalette_Default ();
			AS->cdCurrentPalette = 0;
			break;

		// Море
		case 1:
			FillPalette_Sea ();
			break;

		// Огонь
		case 2:
			FillPalette_Fire ();
			break;

		// Серая
		case 3:
			FillPalette_Grey ();
			break;

		// Рассвет
		case 4:
			FillPalette_Sunrise ();
			break;

		// Кислота
		case 5:
			FillPalette_Acid ();
			break;

		// 7 пропущенных
		case 6:
			FillPalette_7MissedCalls ();
			break;

		// Парус
		case 7:
			FillPalette_SailOnTheSea ();
			break;

		// Зеркало
		case 8:
			FillPalette_Mirror ();
			break;

		// Кровь
		case 9:
			FillPalette_Blood ();
			break;

		// Полиморфная и случайная
		case 10:
		case 11:
		case 12:
		case 13:
			FillPalette_PolymorphRandom (PaletteNumber & 0x2 & polymorphResetNotRequired, PaletteNumber & 0x1);
			AS->cdPolymorphUpdateCounter = PaletteNumber & 0x2;
			break;
		}
	}

// Функция получает указанный цвет из текущей палитры
CD_API(ulong) GetColorFromPaletteEx (uchar ColorNumber)
	{
	return 0xFF000000 | 
		(AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbRed << 16) |
		(AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbGreen << 8) | 
		AS->sgBMPInfo.cd_bmpinfo.colors[ColorNumber].rgbBlue;
	}

// Функция возвращает основной цвет текущей палитры с указанной яркостью
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness)
	{
	return 0xFF000000 | 
		(AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbRed << 16) |
		(AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbGreen << 8) | 
		AS->sgBeatsInfo.cd_bmpinfo.colors[Brightness].rgbBlue;
	}

// Функция возвращает названия доступных палитр
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Default (Adobe Audition)" NAMES_DELIMITER_S \
		"Sea" NAMES_DELIMITER_S \
		"Fire" NAMES_DELIMITER_S \
		"Grey" NAMES_DELIMITER_S \
		"Sunrise" NAMES_DELIMITER_S \
		"Acid" NAMES_DELIMITER_S \
		"7 missed calls" NAMES_DELIMITER_S \
		"Sail on the sea" NAMES_DELIMITER_S \
		"Mirror" NAMES_DELIMITER_S \
		"Blood" NAMES_DELIMITER_S \
		"Polymorph" NAMES_DELIMITER_S \
		"Polymorph monocolor" NAMES_DELIMITER_S \
		"Random" NAMES_DELIMITER_S \
		"Random monocolor")

	return PALETTES_NAMES;
	}

// Функция возвращает псевдослучайное число между Min и Max
sint GetRandomValue (sint Min, sint Max)
	{
	return (double)rand () / (RAND_MAX + 1.0) * (Max - Min) + Min;
	}