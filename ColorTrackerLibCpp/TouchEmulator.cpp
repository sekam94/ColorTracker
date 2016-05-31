#pragma once

#include "windows.h"

#define MAX_NUMBER_OF_CONTACTS 2

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;

namespace ColorTrackerLibCpp
{
	/*namespace Touch
	{
		public ref class TouchContact
		{
		public:
			Drawing::Rectangle Area;
			bool Contact;

			TouchContact(Drawing::Rectangle area, bool contact)
			{
				Area = area;
				Contact = contact;
			}

		private:
			RECT GetRect()
			{
				RECT rect;
				rect.left = Area.Left;
				rect.right = Area.Right;
				rect.top = Area.Top;
				rect.bottom = Area.Bottom;
				return rect;
			}

			POINTER_INFO GetPointerInfo()
			{
				POINTER_INFO pointerInfo;
				pointerInfo.pointerType = PT_TOUCH;

				return pointerInfo;
			}

		internal:
			POINTER_TOUCH_INFO ToPointerTouchInfo()
			{
				POINTER_TOUCH_INFO contact;

				contact.pointerInfo = GetPointerInfo();
				contact.touchFlags = 0;
				contact.touchMask = TOUCH_MASK_NONE;
				contact.rcContact = GetRect();
				contact.rcContactRaw = GetRect();

				return contact;
			}
		};

		public ref class TouchEmulator
		{
			static bool _inited = false;

			static void Init()
			{
				if (!_inited)
					InitializeTouchInjection(MAX_NUMBER_OF_CONTACTS, TOUCH_FEEDBACK_DEFAULT);
			}

			ICollection<TouchContact^>^ _contacts;

		public:
			property ICollection<TouchContact^>^ Contacts
			{
				ICollection<TouchContact^>^ get()
				{
					return _contacts;
				}
				void set(ICollection<TouchContact^>^ contacts)
				{
					_contacts = contacts;
				}
			}

			TouchEmulator()
			{
				Init();
			}

			bool Update()
			{
				if (_contacts != nullptr && _contacts->Count > 0)
				{
					int count = _contacts->Count;
					POINTER_TOUCH_INFO* contacts = new POINTER_TOUCH_INFO[count];
					
					int i = 0;
					for each(TouchContact^ c in _contacts)
						contacts[i++] = c->ToPointerTouchInfo();

					bool result = InjectTouchInput(count, contacts);

					return result;
				}

				return false;
			}
		};
	}*/
}