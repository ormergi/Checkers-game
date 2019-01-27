namespace CheckersGame.Engine
{
	public struct Coordinate
	{
		private int m_Row;
		private int m_Col;

		public Coordinate(int i_Row, int i_Col)
		{
			m_Row = i_Row;
			m_Col = i_Col;
		}

		public static bool operator ==(Coordinate coordinate1, Coordinate coordinate2)
		{
			return coordinate1.Equals(coordinate2);
		}

		public static bool operator !=(Coordinate coordinate1, Coordinate coordinate2)
		{
			return !coordinate1.Equals(coordinate2);
		}

		public override int GetHashCode()
		{
			return m_Row.GetHashCode() ^ m_Col.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			bool isEqual = false;
			if (obj is Coordinate)
			{
				if ((((Coordinate)obj).Column == this.Column) && (((Coordinate)obj).Row == this.Row))
				{
					isEqual = true; 
				}
			}

			return isEqual;
		}

		public int Row
		{
			get
			{
				return m_Row;
			}
		}

		public int Column
		{
			get
			{
				return m_Col;
			}
		}
	}
}
