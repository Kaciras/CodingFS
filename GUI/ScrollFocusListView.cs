namespace CodingFS.GUI;

class ScrollFocusListView : ListView
{
	const int WM_HSCROLL = 0x114;
	const int WM_VSCROLL = 0x115;
	const int MOUSEWHEEL = 0x020A;

	// Make ComboBox to lose focus on scroll.
	protected override void WndProc(ref Message msg)
	{
		switch (msg.Msg)
		{
			case WM_HSCROLL:
			case WM_VSCROLL:
			case MOUSEWHEEL:
				Focus();
				break;
		}
		base.WndProc(ref msg);
	}
}
