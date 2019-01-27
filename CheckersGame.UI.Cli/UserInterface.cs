using System;
using System.Text;
using CheckersGame.Engine;
using CheckersGame.Engine.Exceptions;

namespace CheckersGame.CLI
{
	internal class UserInterface
	{		
		private GameEngine m_GameEngine;

		public void Start()
		{
			readUserSettings();
			bool playRound = true;

			do
			{
				m_GameEngine.Reset();
				runSingleRound();
				drawStatistics();
				playRound = askForContinue();
			}
			while (playRound);
		}

		private bool askForContinue()
		{
			string inputString;
			bool validInput;

			Console.Write("Play again? (enter y/n): ");
			do
			{
				inputString = Console.ReadLine();
				validInput = true;
				if (inputString != "y" && inputString != "n")
				{
					validInput = false;
					Console.Write("Error: please enter 'y' / 'n' : ");
				}
			}
			while (!validInput);

			return inputString == "y";
		}

		private void runSingleRound()
		{
			bool validMove;
			bool gameEnded;
            
            Console.Clear();
			drawBoard();
            Console.Write("{0}'s turn: ", m_GameEngine.GetCurrentPlayerName());
			do
			{
				try
				{
					Move currentMove = readPlayerMove();

					do
					{
						validMove = m_GameEngine.TryMove(currentMove);
						if (!validMove)
						{
							Console.Write("Move is invalid, enter new move: ");
							currentMove = readPlayerMove();
						}
					}
					while (!validMove);
                    
                    Console.Clear();             
                    drawBoard();
					drawState();
					gameEnded = m_GameEngine.IsGameEnded();
				}
				catch (PlayerAskToQuitException /*i_Excpetion*/)
				{
					gameEnded = true;
					m_GameEngine.PunishQuitPlayer();
				}
			}
			while (!gameEnded);
		}

		private void drawBoard()
		{
			StringBuilder horizontalHeaderStringBuilder = new StringBuilder();
			StringBuilder lineSeparatorStringBuilder = new StringBuilder();
			int boardSize = m_GameEngine.GetBoardSize();
			char currentColumnLetter = 'A';

			// print board horizontal header and build line separator
			lineSeparatorStringBuilder.Append(" =");
			for (int i = 0; i < boardSize; i++)
			{
				lineSeparatorStringBuilder.Append("====");
				horizontalHeaderStringBuilder.Append(string.Format("   {0}", currentColumnLetter));
				currentColumnLetter++;
			}

			Console.WriteLine(horizontalHeaderStringBuilder.ToString());
			Console.WriteLine(lineSeparatorStringBuilder.ToString());

			// printing board rows saparated with separators
			for (int i = 0; i < boardSize; i++)
			{
				drawBoardLine(boardSize, i);
				Console.WriteLine(lineSeparatorStringBuilder.ToString());
			}
		}

		private void drawBoardLine(int i_BoardSize, int i_CurrentLineNumber)
		{
			char currentRowLetter = 'a';
			StringBuilder lineStringBuilder = new StringBuilder();

			currentRowLetter += (char)i_CurrentLineNumber;
			lineStringBuilder.Append(string.Format("{0}|", currentRowLetter));
			for (int currentCol = 0; currentCol < i_BoardSize; currentCol++)
			{
				Coordinate currentCoordinate = new Coordinate(i_CurrentLineNumber, currentCol);
                eSymbols currentBoardSlotSymbol = m_GameEngine.GetBoardSquareSymbol(currentCoordinate);
                string currentBoardSlotCLISymbol = convertToCliSymbol(currentBoardSlotSymbol);

				if (currentBoardSlotSymbol == eSymbols.None)
				{
					lineStringBuilder.Append("   |");
				}
				else
				{
					lineStringBuilder.Append(string.Format(" {0} |", currentBoardSlotCLISymbol));
				}
			}

			Console.WriteLine(lineStringBuilder.ToString());
		}

		private void drawState()
		{
            string previousPlayerSymbol = convertToCliSymbol(m_GameEngine.GetPreviousPlayerSymbol());
            string activePlayerSymbol = convertToCliSymbol(m_GameEngine.GetCurrentPlayerSymbol());

            Console.WriteLine(
				"{0}'s move was ({1}): {2}",
				m_GameEngine.GetPreviousPlayerName(),
                previousPlayerSymbol,
				m_GameEngine.GetLastMove().ToString());
            Console.Write(
                "{0}'s turn ({1}): ",
                m_GameEngine.GetCurrentPlayerName(),
                activePlayerSymbol);
		}

        private string convertToCliSymbol(eSymbols i_Symbol)
        {
            return i_Symbol == eSymbols.Black ? "X" : "O";
        }

		private void drawStatistics()
		{
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("{0}'s score are: {1}", m_GameEngine.GetPlayerName(1), m_GameEngine.GetPlayerScore(1));
			Console.WriteLine("{0}'s score are: {1}", m_GameEngine.GetPlayerName(2), m_GameEngine.GetPlayerScore(2));
		}

		private Move readPlayerMove()
		{
			Move move;
			bool validInput;

			do
			{
				string inputString = Console.ReadLine();

				if (inputString == "Q")
				{
					throw new PlayerAskToQuitException();
				}

				validInput = Move.TryParse(inputString, out move);
				if (!validInput)
				{
					Console.Write("Error: move syntax invalid (enter COLrow>COLrow): ");
				}
			}
			while (!validInput);

			return move;
		}

		private void readUserSettings()
		{
			Player firstPlayer = readPlayerDetails(1);
			Player secondPlayer;
			int boardSize = readBoardSize();
			eGameMode gameMode = readGameMode();

			if (gameMode == eGameMode.PlayerVsPlayer)
			{
				secondPlayer = readPlayerDetails(2);
			}
			else
			{
				secondPlayer = createComputerPlayer();
			}

            firstPlayer.Symbol = eSymbols.Black;
			secondPlayer.Symbol = eSymbols.White;
			m_GameEngine = new GameEngine(firstPlayer, secondPlayer, boardSize);
		}

		private Player createComputerPlayer()
		{
			Player computerPlayer = new Player();

			computerPlayer.Score = 0;
			computerPlayer.Kind = Player.ePlayerKind.Computer;
			computerPlayer.Name = "Computer";

			return computerPlayer;
		}

		private Player readPlayerDetails(int i_PlayerNum)
		{
			Player newPlayer = new Player();

			newPlayer.Kind = Player.ePlayerKind.Human;
			newPlayer.Name = readPlayerName(i_PlayerNum);
			newPlayer.Score = 0;

			return newPlayer;
		}

		private string readPlayerName(int i_PlayerNum)
		{
			string playerName = "None";
			bool validInput;

			Console.Write(string.Format("Enter player {0} name (no spaces, max 20 characters): ", i_PlayerNum));
			do
			{
				playerName = Console.ReadLine();
				validInput = true;
				if (playerName.Length == 0)
				{
					Console.Write("Error: player name is empty, try again: ");
					validInput = false;
				}
				else if (playerName.Length > Player.k_MaxPlayerNameLength)
				{
					Console.Write("Error: player name max length is {0}, try again: ", Player.k_MaxPlayerNameLength);
					validInput = false;
				}
				else if (playerName.Contains(" "))
				{
					Console.Write("Error: player name cannot contains spaces, try again: ");
					validInput = false;
				}
			}
			while (!validInput);

			return playerName;
		}

		private eGameMode readGameMode()
		{
			eGameMode gameMode = eGameMode.PlayerVsComputer;
			bool validInput;

			Console.Write(
@"Please select game mode (enter 1 / 2):
1. Player Vs. Computer.
2. Player Vs. Player.

Your choice: ");
			do
			{
				string inputString = Console.ReadLine();
				int inputNumber;

				validInput = int.TryParse(inputString, out inputNumber);
				if (validInput)
				{
					switch (inputNumber)
					{
						case 1:
							gameMode = eGameMode.PlayerVsComputer;
							break;
						case 2:
							gameMode = eGameMode.PlayerVsPlayer;
							break;
						default:
							validInput = false;
							break;
					}
				}

				if (!validInput)
				{
					Console.Write("Error: please select 1 / 2: ");
				}
			}
			while (!validInput);

			return gameMode;
		}

		private int readBoardSize()
		{
			int boardSize = 6;
			bool validInput;

			Console.Write(
@"Please select board size (enter 1 / 2 /3):
1. 6x6.
2. 8x8.
3. 10x10.

Your choise: ");
			do
			{
				string inputString = Console.ReadLine();
				int inputNumber;

				validInput = int.TryParse(inputString, out inputNumber);
				if (validInput)
				{
					switch (inputNumber)
					{
						case 1:
							boardSize = 6;
							break;
						case 2:
							boardSize = 8;
							break;
						case 3:
							boardSize = 10;
							break;
						default:
							validInput = false;
							break;
					}
				}

				if (!validInput)
				{
					Console.Write("Error: please select 1 / 2 / 3: ");
				}
			}
			while (!validInput);

			return boardSize;
		}
	}
}
