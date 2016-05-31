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
		int _length;
		bool _locked;

		Bitmap^ _bitmap;
		BitmapData^ _bmData;
		char* _imagePointer;
	public:
		BitmapPixels(Bitmap^ bitmap)
		{
			_locked = true;
			_bitmap = bitmap;
			_height = bitmap->Height;
			_width = bitmap->Width;
			_length = _height * _width;
			_bmData = bitmap->LockBits(Rectangle(0, 0, _width, _height), ImageLockMode::ReadWrite, bitmap->PixelFormat);
			_imagePointer = (char*)_bmData->Scan0.ToPointer();
		}

		bool PosMatches(int x, int y)
		{
			return x >= 0 &&
				x < _width &&
				y >= 0 &&
				y < _height;
		}

		Color GetPixel(int index)
		{
			if (index < 0 || index >= _length)
				throw gcnew	ArgumentOutOfRangeException;

			char* pixelPtr = _imagePointer + (index * 3);

			return Color::FromArgb(
				(unsigned char)*(pixelPtr + 2),
				(unsigned char)*(pixelPtr + 1),
				(unsigned char)*(pixelPtr));
		}

		Color GetPixel(int x, int y)
		{
			if (!PosMatches(x, y))
				throw gcnew ArgumentOutOfRangeException();

			int pos = _width * y + x;
			return GetPixel(pos);
		}

		void SetPixel(int index, Color color)
		{
			if (index < 0 || index >= _length)
				throw gcnew	ArgumentOutOfRangeException;

			char* pixelPtr = _imagePointer + index * 3;

			*(pixelPtr + 2) = color.R;
			*(pixelPtr + 1) = color.G;
			*(pixelPtr) = color.B;
		}

		void SetPixel(int x, int y, Color color)
		{
			if (!PosMatches(x, y))
				throw gcnew ArgumentOutOfRangeException();

			int pos = _width * y + x;
			SetPixel(pos, color);
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

		property int Length
		{
			int get() { return _length; }
		}

		property Color default[int]
		{
			Color get(int index)
			{
				return GetPixel(index);
			}

			void set(int index, Color value)
			{
				SetPixel(index, value);
			}
		}
	};
}
