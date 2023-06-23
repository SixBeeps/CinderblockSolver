#pragma warning disable CS0659

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Cinderblock {
	public class Rules {
		public enum PieceType {
			EMPTY,
			CINDER,
			DYNAMO
		}

		public enum Direction {
			LEFT,
			RIGHT,
			UP,
			DOWN
		}

		public static Vector2 DirectionOffset(Direction direction) {
			return direction switch {
					Direction.LEFT  => new Vector2(-1, 0),
					Direction.RIGHT => new Vector2(1, 0),
					Direction.UP    => new Vector2(0, -1),
					Direction.DOWN  => new Vector2(0, 1),
					_               => new Vector2(0, 0)
			};
		}
	}

	public class Piece {
		public Rules.PieceType Type {get; private set;}
		public int Id {get; private set;}
		public Vector2 Position;

		public Piece(Rules.PieceType type, int id, int x, int y) {
			Type = type;
			Id = id;
			Position = new Vector2(x, y);
		}

		// Duplicates a given piece instance
		public Piece(Piece p) {
			Type = p.Type;
			Id = p.Id;
			Position = p.Position;
		}

		public override string ToString() {
			return $"{Type} [{Id}] at {Position.ToString()}";
		}

		public override bool Equals(object obj) {
			Piece other = obj as Piece;
			
			// Id shouldn't matter
			return this.Type == other.Type && this.Position == other.Position;
		}
	}

	public class Board {
		public Dictionary<int, Piece> Pieces;
		public Size BoardSize;

		public bool IsWin => GetPiecesOfType(Rules.PieceType.DYNAMO).Length < 2;
		public int Width => BoardSize.Width;
		public int Height => BoardSize.Height;

		// Creates a board with width/height
		public Board(int w, int h) {
			Pieces = new Dictionary<int, Piece>();
			BoardSize = new Size(w, h);
		}

		// Duplicates a board instance
		public Board(Board b) {
			Pieces = new Dictionary<int, Piece>();
			BoardSize = b.BoardSize;
			foreach (Piece bp in b.Pieces.Values) {
				Piece p = new Piece(bp);
				Pieces.Add(p.Id, p);
			}
		}

		// Creates a board instance from the classical string format
		public static Board FromString(string data) {
			string[] rows = data.Split('\n');
			
			Board b = new Board(rows[0].Length, rows.Length);
			
			for (int y = 0; y < rows.Length; y++) {
				for (int x = 0; x < rows[0].Length; x++) {
					// Convert current character digit to piece type
					Rules.PieceType type = rows[y][x] switch {
						'0' => Rules.PieceType.EMPTY,
						'1' => Rules.PieceType.CINDER,
						'2' => Rules.PieceType.DYNAMO,
						_   => throw new NotSupportedException("Building board from string only implements digits 0-2, found " + rows[y][x]),
					};

					// Place new piece if not a 0
					if (type != Rules.PieceType.EMPTY) b.PlacePiece(type, x, y);
				}
			}
		
			return b;
		}

		// Put a new piece on the board
		public void PlacePiece(Rules.PieceType type, int x, int y) {
			if (x >= Width || y >= Height) {
				throw new ArgumentOutOfRangeException();
			}
		
			Piece p = new Piece(type, Pieces.Count, x, y);
			Pieces.Add(p.Id, p);
		}

		// Get a list of pieces on this board of a given type
		public Piece[] GetPiecesOfType(Rules.PieceType type) {
			return (from piece in Pieces.Values where piece.Type == type select piece).ToArray();
		}

		// Attempt to move a piece on this board somewhere
		public bool MovePiece(int pid, Rules.Direction direction) {
			Piece me = Pieces[pid];
			Vector2 offset = Rules.DirectionOffset(direction);

			// Can't go off edge of board
			if (me.Position.X + offset.X * 2 >= Width
        || me.Position.Y + offset.Y * 2 >= Height
        || me.Position.X + offset.X * 2 < 0
        || me.Position.Y + offset.Y * 2 < 0) return false;

			// Look for adjacent tether piece
			Piece tether = null;
			foreach (Piece p in Pieces.Values) {
				if (p.Position == me.Position + offset) tether = p;
			}

			if (tether == null) return false;

			// Look for pieces already in target space
			foreach (Piece p in Pieces.Values) {
				if (p.Position == me.Position + offset * 2) return false;
			}
		
			me.Position += offset * 2;
			if (tether.Type == Rules.PieceType.DYNAMO) Pieces.Remove(tether.Id);
			return true;
		}

		// String representation of this board, dependent only on the pieces
		public string UniqueIdentifier() {
			string ret = "";
			var dynamos = from piece in GetPiecesOfType(Rules.PieceType.DYNAMO)
										orderby piece.Position.X, piece.Position.Y
										select piece;
			var cinders = from piece in GetPiecesOfType(Rules.PieceType.CINDER)
										orderby piece.Position.X, piece.Position.Y
										select piece;
			foreach (Piece p in dynamos) {
				ret += $"D{p.Position.X}|{p.Position.Y}";
			}
			foreach (Piece p in cinders) {
				ret += $"C{p.Position.X}|{p.Position.Y}";
			}
			return ret;
		}

		public override bool Equals(object obj) {
			Board other = obj as Board;
			foreach (Piece myPiece in Pieces.Values) {
				if (!other.Pieces.ContainsValue(myPiece)) return false;
			}
			return true;
		}
	}
}