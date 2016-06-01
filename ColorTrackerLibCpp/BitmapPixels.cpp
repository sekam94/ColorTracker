#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;

namespace ColorTrackerLibCpp
{
	public ref class BitmapPixels : public IDisposable
	{
		int _height;
		int _width;
		bool _locked;

		Bitmap^ _bitmap;
		BitmapData^ _bmData;
		unsigned char* _imagePointer;

		unsigned char* GetPixelPtr(int x, int y)
		{
			if (!PosMatches(x, y))
				throw gcnew ArgumentOutOfRangeException();
			auto offset = y * _bmData->Stride + x * 4;
			return _imagePointer + offset;
		}

	public:
		BitmapPixels(Bitmap^ bitmap)
		{
			if (bitmap->PixelFormat != PixelFormat::Format32bppArgb)
				throw gcnew FormatException("Pixel format is not 32bppArgb");

			_locked = true;
			_bitmap = bitmap;
			_height = bitmap->Height;
			_width = bitmap->Width;
			_bmData = bitmap->LockBits(Rectangle(0, 0, _width, _height), ImageLockMode::ReadWrite, bitmap->PixelFormat);
			_imagePointer = (unsigned char*)_bmData->Scan0.ToPointer();
		}

		bool PosMatches(int x, int y)
		{
			return x >= 0 &&
				x < _width &&
				y >= 0 &&
				y < _height;
		}

		Color GetPixel(int x, int y)
		{
			auto pixelPtr = GetPixelPtr(x, y);

			auto a = *(pixelPtr + 3);
			auto r = *(pixelPtr + 2);
			auto g = *(pixelPtr + 1);
			auto b = *(pixelPtr);

			return Color::FromArgb(a, r, g, b);
		}

		void SetPixel(int x, int y, Color color)
		{
			auto pixelPtr = GetPixelPtr(x, y);

			*(pixelPtr + 3) = color.A;
			*(pixelPtr + 2) = color.R;
			*(pixelPtr + 1) = color.G;
			*(pixelPtr) = color.B;
		}

		~BitmapPixels()
		{
			_bitmap->UnlockBits(_bmData);
			_locked = false;
		}

		property int Width
		{
			int get() { return _width; }
		}
		
		property int Height
		{
			int get() { return _height; }
		}
	};
}
