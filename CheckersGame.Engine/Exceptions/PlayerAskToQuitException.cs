using System;
using System.Runtime.Serialization;

namespace CheckersGame.Engine.Exceptions
{
    [Serializable]
    public class PlayerAskToQuitException : Exception
    {
		public PlayerAskToQuitException()
        {
        }

        public PlayerAskToQuitException(string message) : base(message)
        {
        }

        public PlayerAskToQuitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PlayerAskToQuitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
