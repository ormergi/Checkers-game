using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CheckersGame.Engine;

namespace CheckersGame.GraphicUI
{
	public enum eBlinkColor
	{
		Original,
		Red,
		Green,
		KingRed,
		King,
		KingGreen
	}

	public class PictureBoxTile : PictureBox
	{
		private const int k_TileSize = 80;
		protected const int k_BlinkingCount = 4;
		private const int k_BlinkRedAnimationSpeed = 250;
		protected readonly Timer r_BlinkTimer = new Timer();
		protected int m_BlinkingCounter;
		private Dictionary<eBlinkColor, Image> m_ImagesDictionary;
		protected Image m_BlinkImage;

		protected Coordinate m_Coordinate;

		public PictureBoxTile(Coordinate i_Coordinate)
		{
			m_Coordinate = i_Coordinate;
			Location = CoordinateToLocation(i_Coordinate);
			Size = new Size(k_TileSize, k_TileSize);
			SizeMode = PictureBoxSizeMode.StretchImage;

			SetImage();
			r_BlinkTimer.Interval = k_BlinkRedAnimationSpeed;
			r_BlinkTimer.Tick += blinkTimer_Tick;
		}

		protected virtual void blinkTimer_Tick(object sender, EventArgs e)
		{
			Image = m_BlinkingCounter % 2 == 0 ? m_BlinkImage : m_ImagesDictionary[eBlinkColor.Original];
			m_BlinkingCounter++;
			if (m_BlinkingCounter == k_BlinkingCount)
			{
				r_BlinkTimer.Stop();
			}
		}

		public virtual void Blink(eBlinkColor i_Color)
		{
			m_BlinkImage = m_ImagesDictionary[i_Color];
			m_BlinkingCounter = 0;
			r_BlinkTimer.Start();
		}

		protected virtual void SetImage()
		{
			m_ImagesDictionary = new Dictionary<eBlinkColor, Image>();
			int i = m_Coordinate.Row;
			int j = m_Coordinate.Column;

			if ((i + j) % 2 == 0)
			{
				m_ImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.tile_light);
				m_ImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.tile_light_marked_red);
				m_ImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.tile_light_marked_green);
			}
			else
			{
				m_ImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.tile_dark);
				m_ImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.tile_dark_marked_red);
				m_ImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.tile_dark_marked_green);
			}

			Image = m_ImagesDictionary[eBlinkColor.Original];
		}

		public Coordinate Coordinate
		{
			get
			{
				return m_Coordinate;
			}
		}

		public static int TileSize
		{
			get
			{
				return k_TileSize;
			}
		}

		protected Point CoordinateToLocation(Coordinate i_Coordinate)
		{
			return new Point(i_Coordinate.Column * k_TileSize, i_Coordinate.Row * k_TileSize);
		}
	}
}
