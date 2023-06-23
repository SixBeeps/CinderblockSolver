using System;
using System.IO;
using System.Diagnostics;
using Cinderblock;
using Cinderblock.Solver;
using Cinderblock.Imaging;
using SixLabors.ImageSharp;

class Program {  
  public static void Main (string[] args) {
		// Load a board from file
    string levelData = File.ReadAllText("../boards/square1.txt");
		Board board = Board.FromString(levelData);

		// Create instances for the solver and timer
		Solver solver = new Solver(board);
		Stopwatch timer = new Stopwatch();

		// Run solve
		Console.WriteLine("Finding solution...");
		timer.Start();
		BoardState solved = solver.Solve(25);
		timer.Stop();

		Console.WriteLine($"Calculation took {timer.ElapsedMilliseconds}ms");
		
		if (solved != null) {
			solved.LogSteps();
      Console.WriteLine("Rendering solution to GIF...");
  		Image render = SolutionRenderer.RenderSolution(solved);
  		render.SaveAsGif("./solution.gif");
  		render.Dispose();
		} else {
			Console.WriteLine("Board not solved");
		}
  }
}