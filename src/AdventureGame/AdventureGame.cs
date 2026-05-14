using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";

	private const char Wall = '#';
	private const string DungeonFilePath = "../../../res/DungeonTemplate.txt";

	private Adventurer adventurer = null!;
	private Room[,] dungeon = null!;

	private int rows;
	private int cols;

	private int aRow;
	private int aCol;

	private int exitRow;
	private int exitCol;

	private int lampRow;
	private int lampCol;

	private int keyRow;
	private int keyCol;

	private int chestRow;
	private int chestCol;

	private int grueRow;
	private int grueCol;

	private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool isAdventureAlive;
	private bool isGrueChasing;

	private string lastDirection = string.Empty;

	public AdventureGame()
	{
	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();
				input = GetInput();
			}
			while (!IsValidInput(input));

			ProcessInput(input);
			UpdateGameState();
		}
		while (!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		adventurer = new Adventurer();

		LoadDungeon(DungeonFilePath);

		var start = FindFirstTraversableTile();
		aRow = start.row;
		aCol = start.col;

		isChestOpen = false;
		hasPlayerQuit = false;
		isAdventureAlive = true;
		isGrueChasing = false;
		lastDirection = string.Empty;
	}

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		Room r = dungeon[aRow, aCol];

		if (adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
			Console.WriteLine($"Player: ({aRow}, {aCol})");
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
			+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
			+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
			+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()?.Trim().ToUpperInvariant() ?? string.Empty;
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs =
		{
			GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST,
			GET_LAMP, GET_KEY, OPEN_CHEST, QUIT
		};

		if (!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if (!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			Console.WriteLine("You got eaten alive by the Grue!");
			isAdventureAlive = false;
		}
		else if (input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if (input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if (input == GO_EAST)
		{
			GoEast(r);
		}
		else if (input == GO_WEST)
		{
			GoWest(r);
		}
		else if (input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if (input == GET_KEY)
		{
			GetKey(r);
		}
		else if (input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else
		{
			Quit();
		}
	}

	private void UpdateGameState()
	{
		if (!isAdventureAlive || hasPlayerQuit || !isChestOpen || !isGrueChasing)
		{
			return;
		}

		if (grueRow == aRow && grueCol == aCol)
		{
			Console.WriteLine("The Grue caught you!");
			isAdventureAlive = false;
			return;
		}

		MoveGrueTowardAdventurer();
		Console.WriteLine($"Grue Position: ({grueRow}, {grueCol})");
		Console.WriteLine($"Player Position: ({aRow}, {aCol})");

		if (grueRow == aRow && grueCol == aCol)
		{
			Console.WriteLine("The Grue caught you!");
			isAdventureAlive = false;
		}
	}

	private bool IsGameOver()
	{
		return hasPlayerQuit || !isAdventureAlive || HasWon();
	}

	private bool HasWon()
	{
		return isChestOpen && aRow == exitRow && aCol == exitCol && isAdventureAlive;
	}

	private void ShowGameOverScreen()
	{
		if (HasWon())
		{
			Console.WriteLine("You escaped the dungeon!");
		}
		else if (!isAdventureAlive)
		{
			Console.WriteLine("Game Over! The Grue got you!");
		}
		else if (hasPlayerQuit)
		{
			Console.WriteLine("You quit the game!");
		}

		Console.WriteLine();
		Console.WriteLine("Press ENTER to play again...");
		Console.ReadLine();

		RestartGame();
	}

	private void RestartGame()
	{
		Start();
	}

	private void GoNorth(Room r)
	{
		if (r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if (r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if (r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if (r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if (r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if (r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
	{
		if (r.HasChest())
		{
			if (adventurer.HasKey())
			{
				Console.WriteLine("You got the treasure!");
				isChestOpen = true;
				isGrueChasing = true;
				Console.WriteLine("The Grue has awakened!");
			}
			else
			{
				Console.WriteLine("You do not have the key!");
			}
		}
		else
		{
			Console.WriteLine("There is no chest in this room.");
		}
	}

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}

	private void LoadDungeon(string filePath)
	{
		string[] lines = File.ReadAllLines(filePath);

		rows = int.Parse(lines[0]);
		cols = int.Parse(lines[1]);

		exitRow = int.Parse(lines[2]);
		exitCol = int.Parse(lines[3]);

		lampRow = int.Parse(lines[4]);
		lampCol = int.Parse(lines[5]);

		keyRow = int.Parse(lines[6]);
		keyCol = int.Parse(lines[7]);

		chestRow = int.Parse(lines[8]);
		chestCol = int.Parse(lines[9]);

		grueRow = int.Parse(lines[10]);
		grueCol = int.Parse(lines[11]);

		int layoutStart = 12;
		int descriptionsStart = layoutStart + rows;

		if (lines.Length < descriptionsStart)
		{
			throw new FormatException("File does not contain enough layout rows.");
		}

		dungeon = new Room[rows, cols];
		List<(int row, int col)> traversableTiles = new();

		for (int row = 0; row < rows; row++)
		{
			string layoutLine = lines[layoutStart + row];

			if (layoutLine.Length != cols)
			{
				throw new FormatException($"Layout row {row} must contain exactly {cols} characters.");
			}

			for (int col = 0; col < cols; col++)
			{
				if (layoutLine[col] != Wall)
				{
					dungeon[row, col] = new Room();
					traversableTiles.Add((row, col));
				}
			}
		}

		int descriptionCount = lines.Length - descriptionsStart;

		if (descriptionCount != traversableTiles.Count)
		{
			throw new FormatException(
				$"Description count ({descriptionCount}) must match traversable tile count ({traversableTiles.Count})."
			);
		}

		for (int i = 0; i < traversableTiles.Count; i++)
		{
			string[] parts = lines[descriptionsStart + i].Split('|', 2);

			if (parts.Length != 2)
			{
				throw new FormatException($"Invalid room description line: {lines[descriptionsStart + i]}");
			}

			bool isLit = parts[0] switch
			{
				"1" => true,
				"0" => false,
				_ => throw new FormatException("Room lit value must be 1 or 0.")
			};

			string description = parts[1];

			var (row, col) = traversableTiles[i];
			Room room = dungeon[row, col];

			room.SetLit(isLit);
			room.SetDescription(description);

			room.SetLamp(row == lampRow && col == lampCol);
			room.SetKey(row == keyRow && col == keyCol);
			room.SetChest(row == chestRow && col == chestCol);

			room.SetNorth(IsTraversable(row - 1, col));
			room.SetSouth(IsTraversable(row + 1, col));
			room.SetEast(IsTraversable(row, col + 1));
			room.SetWest(IsTraversable(row, col - 1));
		}

		ValidateTraversableTile(exitRow, exitCol, "exit");
		ValidateTraversableTile(lampRow, lampCol, "lamp");
		ValidateTraversableTile(keyRow, keyCol, "key");
		ValidateTraversableTile(chestRow, chestCol, "chest");
		ValidateTraversableTile(grueRow, grueCol, "grue");
	}

	private bool IsTraversable(int row, int col)
	{
		return row >= 0 &&
			   row < rows &&
			   col >= 0 &&
			   col < cols &&
			   dungeon[row, col] != null;
	}

	private void ValidateTraversableTile(int row, int col, string name)
	{
		if (!IsTraversable(row, col))
		{
			throw new FormatException($"The {name} position must be on a traversable tile.");
		}
	}

	private (int row, int col) FindFirstTraversableTile()
	{
		for (int row = 0; row < rows; row++)
		{
			for (int col = 0; col < cols; col++)
			{
				if (dungeon[row, col] != null)
				{
					return (row, col);
				}
			}
		}

		throw new InvalidOperationException("The dungeon does not contain any traversable tiles.");
	}

	private void MoveGrueTowardAdventurer()
	{
		List<(int row, int col)> path = FindShortestPath(grueRow, grueCol, aRow, aCol);

		if (path.Count >= 2)
		{
			(grueRow, grueCol) = path[1];
		}
	}

	private List<(int row, int col)> FindShortestPath(int startRow, int startCol, int targetRow, int targetCol)
	{
		var path = new List<(int row, int col)>();

		if (startRow == targetRow && startCol == targetCol)
		{
			path.Add((startRow, startCol));
			return path;
		}

		// Distancia de cada nodo
		int[,] distance = new int[rows, cols];
		int[,] prevRow = new int[rows, cols];
		int[,] prevCol = new int[rows, cols];

		// Inicializar distancias en infinito
		for (int r = 0; r < rows; r++)
		{
			for (int c = 0; c < cols; c++)
			{
				distance[r, c] = int.MaxValue;
				prevRow[r, c] = -1;
				prevCol[r, c] = -1;
			}
		}

		var priorityQueue = new PriorityQueue<(int row, int col), int>();

		distance[startRow, startCol] = 0;
		priorityQueue.Enqueue((startRow, startCol), 0);

		while (priorityQueue.Count > 0)
		{
			var current = priorityQueue.Dequeue();

			if (current.row == targetRow && current.col == targetCol)
			{
				break;
			}

			foreach (var neighbor in GetNeighbors(current.row, current.col))
			{
				int newDistance = distance[current.row, current.col] + 1;

				if (newDistance < distance[neighbor.row, neighbor.col])
				{
					distance[neighbor.row, neighbor.col] = newDistance;
					prevRow[neighbor.row, neighbor.col] = current.row;
					prevCol[neighbor.row, neighbor.col] = current.col;
					priorityQueue.Enqueue(neighbor, newDistance);
				}
			}
		}

		if (distance[targetRow, targetCol] == int.MaxValue)
		{
			return path;
		}

		// Reconstruir el path
		int row2 = targetRow;
		int col2 = targetCol;

		while (!(row2 == startRow && col2 == startCol))
		{
			path.Add((row2, col2));
			int pr = prevRow[row2, col2];
			int pc = prevCol[row2, col2];
			row2 = pr;
			col2 = pc;
		}

		path.Add((startRow, startCol));
		path.Reverse();

		return path;
	}

	private IEnumerable<(int row, int col)> GetNeighbors(int row, int col)
	{
		Room room = dungeon[row, col];

		if (room.HasNorth() && IsTraversable(row - 1, col))
		{
			yield return (row - 1, col);
		}

		if (room.HasSouth() && IsTraversable(row + 1, col))
		{
			yield return (row + 1, col);
		}

		if (room.HasEast() && IsTraversable(row, col + 1))
		{
			yield return (row, col + 1);
		}

		if (room.HasWest() && IsTraversable(row, col - 1))
		{
			yield return (row, col - 1);
		}
	}
}