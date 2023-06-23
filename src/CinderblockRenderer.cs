#pragma warning disable CS0659

using Cinderblock.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Cinderblock.Imaging {
	public class BoardRenderer {

		public const int CELL_SIZE = 64, MARGIN = 16, PIECE_RADIUS = 28;
		public static Rgba32 BgColor = new Rgba32(98/255f, 156/255f, 113/255f),
												 FgColor = new Rgba32(211/255f, 232/255f, 222/255f),
												 DynamoColor = new Rgba32(0, 0, 0),
												 CinderColor = new Rgba32(1, 1, 1);
		public static IPen pen = Pens.Solid(FgColor, 2);
		
		// Renders a game board to an ImageSharp image
		public static Image RenderBoard(Board board) {
			int imgWidth = board.Width * CELL_SIZE + MARGIN * 2;
			int imgHeight = board.Height * CELL_SIZE + MARGIN * 2;
			Image<Rgba32> image = new(imgWidth, imgHeight, BgColor);

			// Render the grid
			for (int x = 0; x < board.Width; x++) {
				for (int y = 0; y < board.Height; y++) {
					var rect = new RectangularPolygon(x * CELL_SIZE + MARGIN, y * CELL_SIZE + MARGIN, CELL_SIZE, CELL_SIZE);
					image.Mutate( x => x.Draw(pen, rect));
				}
			}

			// Render each piece on the board
			foreach (Piece p in board.Pieces.Values) {
				var circ = new EllipsePolygon(CenterOfCell((int)p.Position.X, (int)p.Position.Y), PIECE_RADIUS);
				var col = p.Type switch {
						Rules.PieceType.DYNAMO => Color.Black,
						Rules.PieceType.CINDER => Color.White,
						_                      => Color.Red
				};
				image.Mutate( x => x.Fill(col, circ) );
			}

			return image;
		}

		public static PointF CenterOfCell(int cellX, int cellY) {
			int x = MARGIN + (int)((cellX + 0.5f) * CELL_SIZE);
			int y = MARGIN + (int)((cellY + 0.5f) * CELL_SIZE);
			return new PointF(x, y);
		}
	}

	public class SolutionRenderer {

		public static Image RenderSolution(BoardState solvedState) {
			BoardState curState = solvedState;
			Image<Rgba32> gif = (Image<Rgba32>)BoardRenderer.RenderBoard(solvedState.board);
			while (curState != null) {
				Image<Rgba32> frame = (Image<Rgba32>)BoardRenderer.RenderBoard(curState.board);
				gif.Frames.InsertFrame(0, frame.Frames[0]);

				gif.Frames[0].Metadata.GetGifMetadata().FrameDelay = 50;
				
				curState = curState.parent;
			}

			gif.Metadata.GetGifMetadata().RepeatCount = 0;
			
			return gif;
		}
		
	}
}