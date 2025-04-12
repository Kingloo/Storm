using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StormDesktop.Gui.StreamViews
{
#pragma warning disable CA1010 // Generic interface should also be implemented
	public partial class Twitch : ResourceDictionary
#pragma warning restore CA1010 // Generic interface should also be implemented
	{
		public Twitch()
		{
			InitializeComponent();
		}

		private void OnGameIdDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				TextBlock gameNameOrIdTextBlock = (TextBlock)sender;

				if (gameNameOrIdTextBlock.Text is string { Length: > 0 } gameNameOrId)
				{
					Clipboard.SetText(gameNameOrId);
				}
			}
		}
	}
}
