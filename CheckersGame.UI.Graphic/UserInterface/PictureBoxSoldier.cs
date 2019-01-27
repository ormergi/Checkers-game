using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CheckersGame.Engine;
using CheckersGame.GraphicUI.EventHandlers;

namespace CheckersGame.GraphicUI
{ 
    public class PictureBoxSoldier : PictureBoxTile
	{
		private const int k_MovingAnimationSpeed = 10;
		private const int k_MovingStepSize = 4;
		private const int k_DisapearBlinkingAnimationSpeed = 250;
		private const int k_DisapearBlinkingCount = 2;
		private const int k_BecomeKingAnimationSpeed = 50;
		private const int k_BecomeKingGrowingAmplitude = 7;
		private readonly Timer r_MovingTimer = new Timer();
		private readonly Timer r_DisapearTimer = new Timer();
		private readonly Timer r_BecomeKingTimer = new Timer();
		private Point m_LocationToMove;
		private int m_MovingDirectionX;
		private int m_MovingDirectionY;
		private int m_MovingDistance;
		private int m_DisapearBlinkingCounter;
		private int m_KingGrowingCounter;
		private eSymbols m_Symbol;
		private Dictionary<eBlinkColor, Image> m_SoldierImagesDictionary;
		private Dictionary<eBlinkColor, Image> m_KingImagesDictionary;
		private Dictionary<eBlinkColor, Image> m_PictureBoxImages;

		public event SoldierAnimationFinishedEventHandler AnimationFinished;

		public PictureBoxSoldier(eSymbols i_Symbol, Coordinate i_Coordinate) : base(i_Coordinate)
		{
			m_Symbol = i_Symbol;
			BackColor = Color.Transparent;
			SetImage();
			initTimers();
		}

		private void initTimers()
		{
			r_MovingTimer.Interval = k_MovingAnimationSpeed;
			r_MovingTimer.Tick += movingTimer_Tick;
			r_DisapearTimer.Interval = k_DisapearBlinkingAnimationSpeed;
			r_DisapearTimer.Tick += disapearTimer_Tick;
			r_BecomeKingTimer.Interval = k_BecomeKingAnimationSpeed;
			r_BecomeKingTimer.Tick += becomeKingTimer_Tick;
		}

		private void disapearTimer_Tick(object sender, EventArgs e)
		{
			const bool v_BeVisible = true;

			Visible = m_DisapearBlinkingCounter % 2 == 0 ? !v_BeVisible : v_BeVisible;
			m_DisapearBlinkingCounter++;
			if (m_DisapearBlinkingCounter == (k_DisapearBlinkingCount * 2) + 1)
			{
				r_DisapearTimer.Stop();
				OnAnimationFinished();
			}
		}

		private void becomeKingTimer_Tick(object sender, EventArgs e)
		{
			if (m_KingGrowingCounter < k_BecomeKingGrowingAmplitude)
			{
				Width += 2;
				Height += 2;
				Left--;
				Top--;
			}
			else
			{
				Width -= 2;
				Height -= 2;
				Left++;
				Top++;
			}

			m_KingGrowingCounter++;
			if (m_KingGrowingCounter == k_BecomeKingGrowingAmplitude * 2)
			{
				r_BecomeKingTimer.Stop();
				OnAnimationFinished();
			}
		}

		public void SetSelected(bool i_IsSelected)
		{
			BackColor = i_IsSelected ? Color.DarkBlue : Color.Transparent;
		}

		protected override void blinkTimer_Tick(object sender, EventArgs e)
		{
			Image = m_BlinkingCounter % 2 == 0 ? m_BlinkImage : m_PictureBoxImages[eBlinkColor.Original];
			m_BlinkingCounter++;
			if (m_BlinkingCounter == k_BlinkingCount)
			{
				r_BlinkTimer.Stop();
			}
		}

		public override void Blink(eBlinkColor i_Color)
		{
			m_BlinkImage = m_PictureBoxImages[i_Color];
			m_BlinkingCounter = 0;
			r_BlinkTimer.Start();
		}

		protected override void SetImage()
		{
			m_SoldierImagesDictionary = new Dictionary<eBlinkColor, Image>();
			m_KingImagesDictionary = new Dictionary<eBlinkColor, Image>();

			switch (m_Symbol)
			{
				case eSymbols.Black:
					m_SoldierImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.checker_black);
					m_SoldierImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.checker_black_marked_red);
					m_SoldierImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.checker_black_marked_green);
					m_KingImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.checker_black_king);
					m_KingImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.checker_black_king_marked_red);
					m_KingImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.checker_black_king_marked_green);
					break;
				case eSymbols.White:
					m_SoldierImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.checker_white);
					m_SoldierImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.checker_white_marked_red);
					m_SoldierImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.checker_white_marked_green);
					m_KingImagesDictionary.Add(eBlinkColor.Original, Properties.Resources.checker_white_king);
					m_KingImagesDictionary.Add(eBlinkColor.Red, Properties.Resources.checker_white_king_marked_red);
					m_KingImagesDictionary.Add(eBlinkColor.Green, Properties.Resources.checker_white_king_marked_green);
					break;
			}

			m_PictureBoxImages = m_SoldierImagesDictionary;
			Image = m_PictureBoxImages[eBlinkColor.Original];
		}

		private void movingTimer_Tick(object sender, EventArgs e)
		{
			Left += m_MovingDirectionX;
			Top += m_MovingDirectionY;
			if (Math.Abs(Location.X - m_LocationToMove.X) >= m_MovingDistance / 2)
			{
				Height++;
				Width++;
			}
			else
			{
				Height--;
				Width--;
			}

			if (Location == m_LocationToMove)
			{
				r_MovingTimer.Stop();
				OnAnimationFinished();
			}
		}

		private void OnAnimationFinished()
		{
			if (AnimationFinished != null)
			{
				AnimationFinished.Invoke();
			}
		}

		public void MoveTo(Coordinate i_Coordinate)
		{
			m_MovingDirectionX = i_Coordinate.Column > m_Coordinate.Column ? k_MovingStepSize : -k_MovingStepSize;
			m_MovingDirectionY = i_Coordinate.Row > m_Coordinate.Row ? k_MovingStepSize : -k_MovingStepSize;
			SetSelected(false);
			m_LocationToMove = CoordinateToLocation(i_Coordinate);
			m_MovingDistance = Math.Abs(Location.X - m_LocationToMove.X);
			BringToFront();
			r_MovingTimer.Start();
			m_Coordinate = i_Coordinate;
		}

		public void BecomeKing()
		{
			m_PictureBoxImages = m_KingImagesDictionary;
			Image = m_PictureBoxImages[eBlinkColor.Original];
			BecomeKingAnimate();
		}

		private void BecomeKingAnimate()
		{
			m_KingGrowingCounter = 0;
			r_BecomeKingTimer.Start();
		}

		public void Disapear()
		{
			m_DisapearBlinkingCounter = 0;
			r_DisapearTimer.Start();
		}
	}
}
