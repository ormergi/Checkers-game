using System;
using System.Collections.Generic;
using System.Text;
using CheckersGame.Engine;

namespace CheckersGame.GraphicUI
{
	public struct Settings
	{
		private readonly Player r_Player1;
		private readonly Player r_Player2;
		private readonly int r_BoardSize;
		private readonly eGameType r_GameType;

		public Settings(Player i_Player1, Player i_Player2, int i_BoardSize, eGameType i_GameType)
		{
			r_Player1 = i_Player1;
			r_Player2 = i_Player2;
			r_BoardSize = i_BoardSize;
			r_GameType = i_GameType;
		}

		public Player Player1
		{
			get
			{
				return r_Player1;
			}
		}

		public Player Player2
		{
			get
			{
				return r_Player2;
			}
		}

		public int BoardSize
		{
			get
			{
				return r_BoardSize;
			}
		}

		public eGameType GameType
		{
			get
			{
				return r_GameType;
			}
		}
	}
}
