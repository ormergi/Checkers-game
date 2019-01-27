using System.Windows.Forms;

namespace CheckersGame.GraphicUI
{
	public class Launcher
	{
		public static void Start()
		{
			FormGame formGame = null;
			FormSettings formSettings = new FormSettings();

			do
			{
				formSettings.ShowDialog();
				if (formSettings.DialogResult == DialogResult.OK)
				{
					formGame = new FormGame(formSettings.Settings);

					formGame.ShowDialog();
				}
				else
				{
					break;
				}
			}
			while (formSettings.DialogResult != DialogResult.Cancel);
		}
	}
}
