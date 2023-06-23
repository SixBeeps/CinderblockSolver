using System;
using System.Text;
using Cinderblock;
using Cinderblock.Imaging;
using SixLabors.ImageSharp;

public class Generator {
  const int W = 5, H = 5;
  public static void Main(string[] args) {
    Random rng = new();
    StringBuilder sb = new();

    // Spit out random data
    // TODO: generates too many unsolvable boards
    for (int y = 0; y < H; y++) {
      for (int x = 0; x < W; x++) {
        sb.Append((int)Math.Round(Math.Pow(rng.NextDouble(),4)*2));
      }
      if (y != H - 1) sb.AppendLine();
    }

    // Output board to console
    Console.WriteLine(sb.ToString());

    // Create image from data
    Board b = Board.FromString(sb.ToString());
    Image img = BoardRenderer.RenderBoard(b);
    img.SaveAsPng("./random.png");
    img.Dispose();
  }
}