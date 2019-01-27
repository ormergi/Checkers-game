using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CheckersGame.Engine;
using CheckersGame.Engine.Events;
using CheckersGame.GraphicUI.EventHandlers;

namespace CheckersGame.GraphicUI
{
	public enum eGameType
	{
		PlayerVsPlayer,
		PlayerVsComputer,
		Simulation
	}

	public class PanelCheckers : Panel
	{
		private const int r_TrysUntilGiveHint = 2;
		private readonly GameEngine r_GameEngine;
		private readonly Queue<IGameLogicsEvent> r_LogicsEventsQueue;
		private readonly PictureBoxSoldier[,] r_PictureBoxSoldiersMatrix;
		private readonly PictureBoxTile[,] r_PictureBoxTilesMatrix;
		private int m_TrysUntilGiveHintCounter = 0;
		private PictureBoxSoldier m_SelectedSoldier = null;
		private eGameType m_GameType;

		public event GameEndedEventHanlder GameEnded;

		public event PlayerSwitchEventHanlder PlayerSwitched;

		public PanelCheckers(Settings i_Settings)
		{
			r_GameEngine = new GameEngine(i_Settings.Player1, i_Settings.Player2, i_Settings.BoardSize);
			r_GameEngine.EventOccured += gameEngine_EventOccured;
			r_PictureBoxSoldiersMatrix = new PictureBoxSoldier[i_Settings.BoardSize, i_Settings.BoardSize];
			r_PictureBoxTilesMatrix = new PictureBoxTile[i_Settings.BoardSize, i_Settings.BoardSize];
			r_LogicsEventsQueue = new Queue<IGameLogicsEvent>();
			initializeBoard();
			Size = new Size(i_Settings.BoardSize * PictureBoxTile.TileSize, i_Settings.BoardSize * PictureBoxTile.TileSize);
			m_GameType = i_Settings.GameType;
		}

		private void initializeBoard()
		{
			int boardSize = r_GameEngine.GetBoardSize();

			for (int i = 0; i < boardSize; i++)
			{
				for (int j = 0; j < boardSize; j++)
				{
					PictureBoxTile newPictureBoxTile = new PictureBoxTile(new Coordinate(i, j));

					newPictureBoxTile.Click += tile_Click;
					Controls.Add(newPictureBoxTile);
					r_PictureBoxTilesMatrix[i, j] = newPictureBoxTile;
				}
			}
		}

		public void Restart()
		{
			r_GameEngine.Reset();
			removeSoldiersControls();
			restartSoldiers();
			r_LogicsEventsQueue.Clear();
			StartRound();
		}

		private void StartRound()
		{
			if (m_GameType == eGameType.PlayerVsComputer ||
				m_GameType == eGameType.Simulation)
			{
				Random ranodmMoveIndex = new Random((int)DateTime.Now.Ticks);
                List<Move> currentPlayerMovers = r_GameEngine.GetCurrentPlayerAviableMoves();
                Move randomMove = currentPlayerMovers[ranodmMoveIndex.Next(0, currentPlayerMovers.Count - 1)];

				if (r_GameEngine.GetCurrentPlayerKind() == Player.ePlayerKind.Computer)
				{
					r_GameEngine.TryMove(randomMove);
				}
			}
		}

		private void removeSoldiersControls()
		{
			for (int i = Controls.Count - 1; i >= 0; i--)
			{
				if (Controls[i] is PictureBoxSoldier)
				{
					Controls.Remove(Controls[i]);
				}
			}
		}

		private void restartSoldiers()
		{
			int boardSize = r_GameEngine.GetBoardSize();

			for (int i = 0; i < boardSize; i++)
			{
				for (int j = 0; j < boardSize; j++)
				{
					Coordinate currentCoordinate = new Coordinate(i, j);
					eSymbols symbol = r_GameEngine.GetBoardSquareSymbol(currentCoordinate);

					r_PictureBoxSoldiersMatrix[i, j] = null;
					if (symbol != eSymbols.None)
					{
						PictureBoxSoldier newPictureBoxSolider = new PictureBoxSoldier(symbol, currentCoordinate);

						newPictureBoxSolider.Click += soldier_Click;
						newPictureBoxSolider.AnimationFinished += soldier_AnimationFinished;
						Controls.Add(newPictureBoxSolider);
						r_PictureBoxSoldiersMatrix[i, j] = newPictureBoxSolider;
						newPictureBoxSolider.BringToFront();
					}
				}
			}
		}

		private void soldier_Click(object sender, EventArgs e)
		{
			PictureBoxSoldier clickedSoldier = sender as PictureBoxSoldier;
			if (r_LogicsEventsQueue.Count == 0)
			{
				if (clickedSoldier != null)
				{
					if (r_GameEngine.IsCurrentPlayerSoldier(clickedSoldier.Coordinate))
					{
						if (m_SelectedSoldier == clickedSoldier) 
						{
							// second click on the same soldier - release
							m_SelectedSoldier.SetSelected(false);
							m_SelectedSoldier = null;
						}
						else
						{
							if (m_SelectedSoldier != null)
							{
								m_SelectedSoldier.SetSelected(false);
							}

							clickedSoldier.SetSelected(true);
							m_SelectedSoldier = clickedSoldier;
						}
					}
					else
					{
						clickedSoldier.Blink(eBlinkColor.Red);
					}
				}
			}
		}

		private void tile_Click(object sender, EventArgs e)
		{
			PictureBoxTile clickedTile = sender as PictureBoxTile;
			bool legalMove = false;

			if (m_SelectedSoldier != null)
			{
				Move move = new Move(m_SelectedSoldier.Coordinate, clickedTile.Coordinate);
				legalMove = r_GameEngine.TryMove(move);
				m_TrysUntilGiveHintCounter++;

				if (!legalMove)
				{
					if (r_TrysUntilGiveHint < m_TrysUntilGiveHintCounter &&
						r_LogicsEventsQueue.Count == 0)
					{
						HintBlinkAviableLocations();
						m_TrysUntilGiveHintCounter = 0;
					}
				}
			}
			else if(r_LogicsEventsQueue.Count == 0)
			{
				clickedTile.Blink(eBlinkColor.Red);
			}
		}

		// new Hint Feature S
		private void HintBlinkAviableLocations()
		{
			bool isSelectedSoldierAbleToMove = isSoldierAbleToMove(m_SelectedSoldier);

			if (isSelectedSoldierAbleToMove)
			{
				blinkAbleToReachTiles();
			}
			else 
			{
				blinkAbleToMoveSoldiers();
			}
		}

		private void blinkAbleToReachTiles()
		{
			List<PictureBoxTile> aviableTilesToReachList = getAviableTilesToReachList();

			foreach (PictureBoxTile tile in aviableTilesToReachList)
			{
				tile.Blink(eBlinkColor.Green);
			}
		}

		private void blinkAbleToMoveSoldiers()
		{
			List<PictureBoxSoldier> ableToMoveSoldierList = getCurrentPlayerAbleToMoveSoldiers();

			foreach (PictureBoxSoldier soldier in ableToMoveSoldierList)
			{
				soldier.Blink(eBlinkColor.Green);
			}
		}

		private bool isSoldierAbleToMove(PictureBoxSoldier m_SelectedSoldier)
		{
			bool ableToMove = false;

			foreach (Move currentPlayerMove in r_GameEngine.GetCurrentPlayerAviableMoves())
			{ 
				if (m_SelectedSoldier.Coordinate == currentPlayerMove.FromCoordinate)
				{
					ableToMove = true;
					break;
				}
			}			

			return ableToMove;
		}

		private List<PictureBoxSoldier> getCurrentPlayerAbleToMoveSoldiers()
		{
			List<PictureBoxSoldier> ableToMoveSoldiersList = new List<PictureBoxSoldier>();

			foreach (Move currentPlayerMove in r_GameEngine.GetCurrentPlayerAviableMoves())
			{
				foreach (PictureBoxSoldier soldier in r_PictureBoxSoldiersMatrix)
				{
					if (soldier != null && 
						soldier.Coordinate == currentPlayerMove.FromCoordinate)
					{
						ableToMoveSoldiersList.Add(soldier);
					}
				}
			}

			return ableToMoveSoldiersList;
		}

		private List<PictureBoxTile> getAviableTilesToReachList()
		{
			List<PictureBoxTile> aviableTilesToReachList = new List<PictureBoxTile>();

			foreach (Move currentPlayerMove in r_GameEngine.GetCurrentPlayerAviableMoves())
			{
				foreach(PictureBoxTile tile in r_PictureBoxTilesMatrix)
				{
					if(tile.Coordinate == currentPlayerMove.ToCoordinate &&
						m_SelectedSoldier.Coordinate == currentPlayerMove.FromCoordinate)
					{
						aviableTilesToReachList.Add(tile);
					}
				}
			}

			return aviableTilesToReachList;
		}

		// new feature E
		private void gameEngine_EventOccured(IGameLogicsEvent i_Event)
		{
			bool queueWasEmpty = r_LogicsEventsQueue.Count == 0;

			r_LogicsEventsQueue.Enqueue(i_Event);
			if (queueWasEmpty)
			{
				proceedEvent(r_LogicsEventsQueue.Peek());
			}
		}

		private void soldier_AnimationFinished()
		{
			ContinueToNextEventInQueue();
		}

		private void ContinueToNextEventInQueue()
		{
			r_LogicsEventsQueue.Dequeue();
			if (r_LogicsEventsQueue.Count != 0)
			{
				proceedEvent(r_LogicsEventsQueue.Peek());
			}
		}

		private void proceedEvent(IGameLogicsEvent i_Event)
		{
			SoldierMovedEvent soldierMovedEvent = i_Event as SoldierMovedEvent;
			BecameKingEvent becameKingEvent = i_Event as BecameKingEvent;
			SoldierRemovedEvent soldierRemovedEvent = i_Event as SoldierRemovedEvent;
			PlayerSwitchedEvent playerSwitchedEvent = i_Event as PlayerSwitchedEvent;
			GameEndedEvent gameEndedEvent = i_Event as GameEndedEvent;

			if (soldierMovedEvent != null)
			{
				moveSoldier(soldierMovedEvent.Move);
			}
			else if (becameKingEvent != null)
			{
				becomeKing(becameKingEvent.Coordinate);
			}
			else if (soldierRemovedEvent != null)
			{
				removeSoldier(soldierRemovedEvent.Coordinate);
			}
			else if (playerSwitchedEvent != null)
			{
				OnPlayerSwitched();
			}
			else if (gameEndedEvent != null)
			{
				OnGameEnded();
			}
		}

		private void moveSoldier(Move i_Move)
		{
			PictureBoxSoldier pictureboxSoldier = r_PictureBoxSoldiersMatrix[i_Move.FromCoordinate.Row, i_Move.FromCoordinate.Column];

			r_PictureBoxSoldiersMatrix[i_Move.ToCoordinate.Row, i_Move.ToCoordinate.Column] = pictureboxSoldier;
			r_PictureBoxSoldiersMatrix[i_Move.FromCoordinate.Row, i_Move.FromCoordinate.Column] = null;
			m_SelectedSoldier = null;

			r_PictureBoxTilesMatrix[i_Move.FromCoordinate.Row, i_Move.FromCoordinate.Column].SendToBack();
			pictureboxSoldier.BringToFront();

			pictureboxSoldier.MoveTo(i_Move.ToCoordinate);
		}

		private void becomeKing(Coordinate i_Coordinate)
		{
			PictureBoxSoldier pictureboxSoldier = r_PictureBoxSoldiersMatrix[i_Coordinate.Row, i_Coordinate.Column];

			pictureboxSoldier.BecomeKing();
		}

		private void removeSoldier(Coordinate i_Coordinate)
		{
			PictureBoxSoldier pictureboxSoldier = r_PictureBoxSoldiersMatrix[i_Coordinate.Row, i_Coordinate.Column];

			pictureboxSoldier.Disapear();
			r_PictureBoxSoldiersMatrix[i_Coordinate.Row, i_Coordinate.Column] = null;
		}

		private void OnPlayerSwitched()
		{
			if (PlayerSwitched != null)
			{
				PlayerSwitched.Invoke();
				ContinueToNextEventInQueue();
			}
		}

		private void OnGameEnded()
		{
			if (GameEnded != null)
			{
				GameEnded.Invoke(r_GameEngine.GetPreviousPlayerName(), r_GameEngine.GetPlayerScore(1), r_GameEngine.GetPlayerScore(2));
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			foreach (Control control in Controls)
			{
				PictureBoxTile pictureBox = control as PictureBoxTile;

				if (pictureBox != null)
				{
					e.Graphics.DrawImage(
						pictureBox.Image,
						pictureBox.Location.X,
						pictureBox.Location.Y,
						pictureBox.Width,
						pictureBox.Height);
				}
			}
		}
	}
}
