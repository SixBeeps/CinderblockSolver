#pragma warning disable CS0659

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cinderblock.Solver {
	public class BoardState {
		public Board board;
		public BoardState parent;
		public List<BoardState> children;
		
		public string MoveDescription;
		public bool hasExpanded;

		public bool IsTerminal => hasExpanded && !children.Any();

		public BoardState(Cinderblock.Board board, BoardState parent = null, string desc = "START") {
			this.board = board;
			this.parent = parent;
			children = new List<BoardState>();
			MoveDescription = desc;
		}

		public void ExpandState() {
			// Don't try expanding again
			if (hasExpanded) return;

			// Go through every piece...
			foreach (Piece dynamo in board.GetPiecesOfType(Rules.PieceType.DYNAMO)) {
				
				// Try moving the piece in every direction
				foreach (Rules.Direction dir in Enum.GetValues(typeof(Rules.Direction))) {
					Board newBoard = new Board(board);
					if (newBoard.MovePiece(dynamo.Id, dir) == true) {
						children.Add(new BoardState(newBoard, this, $"Move {dynamo} {Enum.GetName(typeof(Rules.Direction), dir)}"));
					}
				}

				hasExpanded = true;
			}
		}

		// Print out the steps we took to get here
		public void LogSteps() {
			Stack<BoardState> stateStack = new Stack<BoardState>();
			stateStack.Push(this);
			while (stateStack.Peek().parent != null) {
				stateStack.Push(stateStack.Peek().parent);
			}

			// Get rid of start state
			stateStack.Pop();

			Console.WriteLine($"--- HERE IN {stateStack.Count} MOVES ---");

			while (stateStack.Count > 0) {
				Console.WriteLine(stateStack.Pop().MoveDescription);
			}
		}
	}

	public class Solver {
		public BoardState root;
		public int Iterations {get; private set;}
		private List<BoardState> toBeExpanded;
		private List<string> seenBoards;

		public Solver(Cinderblock.Board board) {
			root = new BoardState(board);
			Iterations = 0;
			toBeExpanded = new List<BoardState> { root };
			seenBoards = new List<string> { root.board.UniqueIdentifier() };
		}

		public BoardState Solve(int maxIterations) {
			while (Iterations < maxIterations) {
				// If nothing can be expanded, reached a dead end
				if (toBeExpanded.Count == 0) {
					Console.WriteLine($"Ran out of expansions on {Iterations} iterations");
					return null;
				}
				
				// Create a working list which holds the new terminal states
				List<BoardState> expansions = new List<BoardState>();

				// Expand pre-existing terminals
				Console.WriteLine($"Expanding {toBeExpanded.Count} terminals");
				foreach(BoardState expanding in toBeExpanded) {
					expanding.ExpandState();
					foreach (BoardState child in expanding.children) {
						string uid = child.board.UniqueIdentifier();
						if (!seenBoards.Contains(uid)) {
							expansions.Add(child);
							seenBoards.Add(uid);
						}
					}
				}

				// Newly created terminals should be expanded next turn
				toBeExpanded = expansions;

				// If any of them are winning boards, we solved the board!
				foreach (BoardState state in toBeExpanded) {
					if (state.board.IsWin) return state;
				}

				Iterations++;
			}

			// Reached max iteration length, didn't solve in time
			return null;
		}
	}
}