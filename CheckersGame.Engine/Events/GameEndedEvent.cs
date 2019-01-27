namespace CheckersGame.Engine.Events
{
    public class GameEndedEvent : IGameLogicsEvent
	{
		private Player m_Player;

		public GameEndedEvent(Player i_Player)
		{
			m_Player = i_Player;
		}

		public Player Winner
		{
			get
			{
				return m_Player;
			}
		}
	}
}