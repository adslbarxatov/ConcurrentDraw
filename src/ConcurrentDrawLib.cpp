// ����� ���������
#include "ConcurrentDrawLib.h"

// ����� ����������
HRECORD cdChannel = NULL;			// ���������� ������
float cdFFT[FFT_VALUES_COUNT];		// ������ ��������, ���������� �� ������
MMRESULT cdFFTTimer = NULL;			// ���������� ������� ������� ������ �� ������

HBITMAP sdBMP = NULL;				// ���������� BITMAP ������������ ���������
uchar *sdBuffer;					// ����� ������������ ���������
uint sdFrameWidth, sdFrameHeight,	// ������� ����������� �������������
	sdCurrentPosition = 0;			// ������� ������� �� �������������
uchar sdSpectrogramMode = 0;		// ����� ������������� (0 - ���������, 1 - � ��������, 2 - ����������)

float cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT * 
	25.5f;												// ������� �������� FFT
uchar cdFFTPeak = 0,									// ������� ������� ��������
	cdFFTPeakEvLowEdge = PEAK_EVALUATION_LOW_EDGE,		// ������ ������� ��������� ����������� ����
	cdFFTPeakEvHighEdge = PEAK_EVALUATION_HIGH_EDGE,	// ������� ������� ��������� ����������� ����
	cdFFTPeakEvLowLevel = PEAK_EVALUATION_LOW_LEVEL;	// ���������� ���������, �� ������� ������������ ���
uchar currentPalette = 0;			// ����� ������� �������

// ������� �������� ����� ��������� ������ ����� (������ �������� �� 128 �� ���)
CD_API(uchar) GetDevicesEx (schar **Devices)
	{
	// ����������
	BASS_DEVICEINFO info;
	uchar i, pos, lastLength;
	uchar devicesCount = 0;

	// ��������� ���������� ���������
	for (devicesCount = 0; devicesCount < MAX_RECORD_DEVICES; devicesCount++)
		{
		if (!BASS_RecordGetDeviceInfo (devicesCount, &info))
			{
			break;
			}
		}

	if (devicesCount == 0)
		return devicesCount;	// ��� ��������� ���������

	// �������� ������� ���
	if ((*Devices = (schar *)malloc (devicesCount * MAX_DEVICE_NAME_LENGTH)) == NULL)
		return 0;
	memset (*Devices, 0x00, devicesCount * MAX_DEVICE_NAME_LENGTH);

	// ��������� ���
	for (i = pos = 0; i < devicesCount; i++)
		{
		if (!BASS_RecordGetDeviceInfo (i, &info))
			{
			free (*Devices);
			return 0;	// ����� �����-��
			}

		lastLength = min (strlen (info.name), MAX_DEVICE_NAME_LENGTH - 1);
		memcpy (*Devices + pos, info.name, lastLength);
		*(*Devices + pos + lastLength) = NAMES_DELIMITER_C;
		pos += (lastLength + 1);
		}

	// ���������
	return devicesCount;
	}

// ������� ��������� ������� ���������� ������ �� ��������� ������
CD_API(sint) InitializeSoundStreamEx (uchar DeviceNumber)
	{
	// ��������� ������� � ����������
	if (BASS_GetVersion () != BASS_VERSION)
		return -10;

	// �������������
	if (!BASS_RecordInit (DeviceNumber))
		return BASS_ErrorGetCode ();

	if (!(cdChannel = BASS_RecordStart (44100, 2, 0x00, NULL, 0)))
		return BASS_ErrorGetCode ();

	// ������ ������� ������� ������
	cdFFTTimer = timeSetEvent (25, 25, (LPTIMECALLBACK)&UpdateFFT, 0, TIME_PERIODIC);

	// �������
	return 0;
	}

// ������� ��������� ������� ����������
CD_API(void) DestroySoundStreamEx ()
	{
	// ��������
	if (!cdChannel)
		return;

	// �������� �������������, ���� ����
	DestroySpectrogramEx ();

	// �������� �������
	if (cdFFTTimer)
		{
		timeKillEvent (cdFFTTimer);
		cdFFTTimer = NULL;
		}

	// ���������
	BASS_RecordFree ();
	cdChannel = NULL;
	}

// ������� ����������� ������ �� ������ ����������
float *GetDataFromStreamEx ()
	{
	// ��������
	ulong v;
	if (!cdChannel)
		return NULL;

	// ���������
	if ((v = BASS_ChannelGetData (cdChannel, &cdFFT, BASS_DATA_AVAILABLE)) < FFT_VALUES_COUNT)
		return NULL;

	if (BASS_ChannelGetData (cdChannel, &cdFFT, FFT_MODE ) < 0)
		return NULL;

	return cdFFT;
	}

// �������-������ ����������� �������������
void CALLBACK UpdateFFT (UINT uTimerID, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2)
	{
	// ����������
	uint y, x, v, xd;

	// ���������� ������� (���� ��������)
	if (!GetDataFromStreamEx ())
		return;

	// ���������� �������������, ���� ���������
	switch (sdSpectrogramMode)
		{
		// ��� �������������
		default:
		case 0:
			break;

		// � ��������
		case 1:
			for (y = 0; y < sdFrameHeight; y++)
				{
				// ��������������� (���������� ������ ��������� ����� ������ ������ ��������)
				v = (uint)(sqrt (cdFFT[y + 1]) * cdFFTScale);

				// ���������� � �������� � �������� ����
				INBOUND_FFT_VALUE (v)
				UPDATE_PEAK (y, v)

				// ���������
				sdBuffer[y * sdFrameWidth + sdCurrentPosition] =
#ifdef SD_DOUBLE_WIDTH
				sdBuffer[y * sdFrameWidth + (sdCurrentPosition + 1) % sdFrameWidth] = 
#endif
				v;

				// ������
				sdBuffer[y * sdFrameWidth + (sdCurrentPosition + SD_STEP) % sdFrameWidth] = 255;
				}

			// �������� �������
			sdCurrentPosition = (sdCurrentPosition + SD_STEP) % sdFrameWidth;
			break;

		// ����������
		case 2:
			for (y = 0; y < sdFrameHeight; y++)
				{
				// ����� �����������
				for (x = 0; x < sdFrameWidth - SD_STEP; x += SD_STEP)
					{
					sdBuffer[y * sdFrameWidth + x] = sdBuffer[y * sdFrameWidth + x + 1]
#ifdef SD_DOUBLE_WIDTH
					= sdBuffer[y * sdFrameWidth + x + 2]
#endif
					;
					}

				// �������
				v = (uint)(sqrt (cdFFT[y + 1]) * cdFFTScale);

				// ���������� � �������� � �������� ����
				INBOUND_FFT_VALUE (v)
				UPDATE_PEAK (y, v)

				// ���������
#ifdef SD_DOUBLE_WIDTH
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 2] = 
#endif
				sdBuffer[y * sdFrameWidth + sdFrameWidth - 1] = v;
				}
			break;

		// �����������
		case 3:
			for (x = 0; x < sdFrameWidth; x++)
				{
				// ��������� �������� � �������
				xd = HISTOGRAM_FFT_VALUES_COUNT * (ulong)x / sdFrameWidth;
				v = (uint)(sqrt (cdFFT[xd]) * cdFFTScale);

				// ���������� � �������� � �������� ����
				INBOUND_FFT_VALUE (v)
				UPDATE_PEAK (xd, v)

				// ���������
				v = sdFrameHeight * (ulong)v / CD_BMPINFO_COLORS_COUNT;
				for (y = 0; y < v; y++)
					sdBuffer[y * sdFrameWidth + x] = 3 * y / 4 + 64;	// ������� ������ ��� �������
				for (y = v; y < sdFrameHeight; y++)
					sdBuffer[y * sdFrameWidth + x] = 0;
				}
			break;
		}
	}

// ������� �������������� �������������
CD_API(sint) InitializeSpectrogramEx (uint FrameWidth, uint FrameHeight, uchar PaletteNumber, uchar SpectrogramMode)
	{
	// ����������
	union CD_BITMAPINFO info;

	// �������� ����������
	if ((FrameWidth < MINFRAMEWIDTH) || (FrameWidth > MAXFRAMEWIDTH) ||
		(FrameHeight < MINFRAMEHEIGHT) || (FrameHeight > MAXFRAMEHEIGHT))
		return -3;
	if (!cdChannel)		// ����� ������ ���� ���������������
		return -1;
	if (sdBMP)			// ������������� �� ������ ���� ������
		return -2;

	sdFrameWidth = FrameWidth & 0xFFFC;		// ���� �����, ������, �� CreateDIBSection �� �������� �������,
	if (sdFrameWidth != FrameWidth)
		sdFrameWidth += 4;

	sdFrameHeight = FrameHeight & 0xFFFC;	// ������� �� ������� �� 4
	if (sdFrameHeight != FrameHeight)
		sdFrameHeight += 4;

	sdSpectrogramMode = SpectrogramMode;

	// ������������� ���������
	memset (info.cd_bmpinfo_ptr, 0x00, sizeof (union CD_BITMAPINFO));	// ����� �� ���� ���� ��������

	info.cd_bmpinfo.header.biSize = sizeof (BITMAPINFOHEADER);
	info.cd_bmpinfo.header.biWidth = sdFrameWidth;
	info.cd_bmpinfo.header.biHeight = sdFrameHeight;
	info.cd_bmpinfo.header.biPlanes = 1;
	info.cd_bmpinfo.header.biBitCount = 8;
	info.cd_bmpinfo.header.biClrUsed = info.cd_bmpinfo.header.biClrImportant = CD_BMPINFO_COLORS_COUNT;

	FillPalette (info.cd_bmpinfo.colors, PaletteNumber);

	// �������� BITMAP
	if ((sdBMP = CreateDIBSection (NULL, (BITMAPINFO *)&info, DIB_RGB_COLORS, (void **)&sdBuffer, NULL, 0)) == NULL)
		return -4;

	// ���������
	return 0;
	}

// ������� ������� �������� �������������
CD_API(void) DestroySpectrogramEx ()
	{
	// ��������
	if (!cdChannel)
		return;

	// �����
	if (sdBMP)
		{
		sdSpectrogramMode = 0;
		
		DeleteObject (sdBMP);
		sdBMP = NULL;
		}
	}

// ������� ���������� ������� ����� �������������
CD_API(HBITMAP) GetSpectrogramFrameEx ()
	{
	// ��������
	if (!sdBMP)
		return NULL;

	// ���������
	return sdBMP;
	}

// ������� ���������� �������� ��������� �� ��������� ������
CD_API(uchar) GetCurrentPeakEx ()
	{
	// �� ������� ������
	if (cdFFTPeak == 0xFF)
		cdFFTPeak--;		// �� ������ ��� �������
	else if (cdFFTPeak > 40)
		cdFFTPeak -= 40;	// ����� - ���������
	else
		cdFFTPeak = 0;		// ��������

	return cdFFTPeak;
	}

// ������� ������������� ������� ����������� �������� ��������
CD_API(void) SetPeakEvaluationParametersEx (uchar LowEdge, uchar HighEdge, 
	uchar LowLevel, uchar FFTScaleMultiplier)
	{
	// �� ������� ������
	cdFFTPeakEvLowLevel = LowLevel;
	cdFFTPeakEvLowEdge = LowEdge;
	cdFFTPeakEvHighEdge = (HighEdge < LowEdge) ? LowEdge : HighEdge;

	if ((FFTScaleMultiplier >= CD_MIN_FFT_SCALE_MULT) && (FFTScaleMultiplier <= CD_MAX_FFT_SCALE_MULT))
		cdFFTScale = (float)FFTScaleMultiplier;
	else
		cdFFTScale = (float)CD_DEFAULT_FFT_SCALE_MULT;
	cdFFTScale *= 25.5f;
	}

// ������� ��������� �������
void FillPalette (RGBQUAD *Palette, uchar PaletteNumber)
	{
	uint i;
	uint qSize = CD_BMPINFO_COLORS_COUNT / 4;

	switch (PaletteNumber)
		{
		// �����������
		default:
		case 0:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbBlue = 4 * i;

				Palette[qSize + i].rgbBlue = 255;
				Palette[qSize + i].rgbRed = 4 * i;

				Palette[2 * qSize + i].rgbRed = 255;
				Palette[2 * qSize + i].rgbBlue = 4 * (qSize - 1 - i);
				Palette[2 * qSize + i].rgbGreen = 4 * i;

				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = 0;
			break;

		// ����
		case 1:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbBlue = 4 * i;

				Palette[qSize + i].rgbBlue = 255;
				Palette[qSize + i].rgbGreen = 2 * i;

				Palette[2 * qSize + i].rgbBlue = 255;
				Palette[2 * qSize + i].rgbGreen = 2 * (qSize + i);

				Palette[3 * qSize + i].rgbBlue = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbRed = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;

		// �����
		case 2:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbRed = 4 * i;

				Palette[qSize + i].rgbRed = 255;
				Palette[qSize + i].rgbGreen = 2 * i;

				Palette[2 * qSize + i].rgbRed = 255;
				Palette[2 * qSize + i].rgbGreen = 2 * (qSize + i);

				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 255;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;

		// �����
		case 3:
			for (i = 0; i < CD_BMPINFO_COLORS_COUNT; i++) 
				{
				Palette[i].rgbRed = Palette[i].rgbGreen = Palette[i].rgbBlue = i & 0xFF;
				}
			currentPalette = PaletteNumber;
			break;

		// �������
		case 4:
			for (i = 0; i < qSize; i++) 
				{
				Palette[i].rgbBlue = 2 * i;
				Palette[qSize + i].rgbBlue = 128 - 2 * i;
				Palette[qSize + i].rgbGreen = 3 * i;
				Palette[2 * qSize + i].rgbRed = 4 * i;
				Palette[2 * qSize + i].rgbGreen = 192 - i;
				Palette[3 * qSize + i].rgbRed = 255;
				Palette[3 * qSize + i].rgbGreen = 128 + 2 * i;
				Palette[3 * qSize + i].rgbBlue = 4 * i;
				}
			currentPalette = PaletteNumber;
			break;
		}

	//ACT_SavePaletteEx ("test.act", (union RGBA_Color *)Palette, 256);
	}

// ������� ���������� �������� ���� ������� ������� � ��������� ��������
CD_API(ulong) GetMasterPaletteColorEx (uchar Brightness)
	{
	uint v;

	switch (currentPalette)
		{
		default:
		case 0:
			return 0xFF000000 | ((Brightness / 2) << 16) | Brightness;

		case 1:
			return 0xFF000000 | Brightness;

		case 2:
			return 0xFF000000 | (Brightness << 16) | ((Brightness / 4) << 8);

		case 3:
			v = (9 * Brightness / 10) & 0xFF;
			return 0xFF000000 | (v << 16) | (v << 8) | v;

		case 4:
			return 0xFF000000 | ((3 * Brightness / 4) << 8);
		}
	}

// ������� ���������� �������� ��������� ������
CD_API(schar *) GetPalettesNamesEx ()
	{
	#define PALETTES_NAMES	("Default (blue-magenta-yellow-white)" NAMES_DELIMITER_S \
		"Sea (blue-cyan-white)" NAMES_DELIMITER_S \
		"Fire (red-orange-yellow-white)" NAMES_DELIMITER_S \
		"Grey" NAMES_DELIMITER_S \
		"Sunshine (blue-green-orange-white)")

	return PALETTES_NAMES;
	}

// ������� ���������� �������������� ������� ������� ������������
CD_API(udlong) GetSpectrogramFrameMetricsEx ()
	{
	return ((udlong)MINFRAMEWIDTH << 48) | ((udlong)MAXFRAMEWIDTH << 32) | 
		((udlong)MINFRAMEHEIGHT << 16) | (udlong)MAXFRAMEHEIGHT;
	}

// ������� ���������� ����������� ������� ����������� �������� ��������
CD_API(ulong) GetDefaultPeakEvaluationParametersEx ()
	{
	return (CD_DEFAULT_FFT_SCALE_MULT << 24) | (PEAK_EVALUATION_LOW_EDGE << 16) | 
		(PEAK_EVALUATION_HIGH_EDGE << 8) | PEAK_EVALUATION_LOW_LEVEL;
	}
