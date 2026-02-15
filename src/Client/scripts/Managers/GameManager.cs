using Godot;
using Godot.Collections;
using System.Linq;

namespace YojigenShift.Po5.Scripts.Managers;

/// <summary>
/// Handles global game state, score tracking, and session management.
/// Must be configured as an Autoload in Project Settings.
/// </summary>
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Signal]
	public delegate void ScoreChangedEventHandler(int newScore);
	[Signal] public delegate void HighScoreChangedEventHandler(int newHighScore);
	[Signal]
	public delegate void GameEndedEventHandler();

	public int CurrentScore { get; private set; } = 0;
	public int HighScore { get; private set; } = 0;
	public bool IsGameOver { get; private set; } = false;

	private const string SavePath = "user://po5_save.dat";

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}

		LoadGame();
	}

	/// <summary>
	/// Adds points to the score and emits a signal to update UI.
	/// </summary>
	public void AddScore(int amount)
	{
		if (IsGameOver) return;
		CurrentScore += amount;
		EmitSignal(SignalName.ScoreChanged, CurrentScore);

		if (CurrentScore > HighScore)
		{
			HighScore = CurrentScore;
			EmitSignal(SignalName.HighScoreChanged, HighScore);
		}
	}

	public void TriggerGameOver()
	{
		if (IsGameOver) return;
		IsGameOver = true;

		if (CurrentScore > HighScore)
		{
			SaveGame();
		}

		EmitSignal(SignalName.GameEnded);
	}

	public void ResetGame()
	{
		IsGameOver = false;
		CurrentScore = 0;
		EmitSignal(SignalName.ScoreChanged, CurrentScore);
	}

	private void SaveGame()
	{
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.Print($"Failed to save game: {FileAccess.GetOpenError()}");
			return;
		}

		var data = new Dictionary<string, Variant>
		{
			{"high_score", HighScore }
		};

		file.StoreString(Json.Stringify(data));
	}

	private void LoadGame()
	{
		if (!FileAccess.FileExists(SavePath)) return;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		string content = file.GetAsText();

		var json = new Json();
		if (json.Parse(content) == Error.Ok)
		{
			var data = json.Data.AsGodotDictionary();
			if (data.ContainsKey("high_score"))
			{
				HighScore = (int)data["high_score"];
				CallDeferred(nameof(EmitHighScoreSignal));
			}
		}
	}

	private void EmitHighScoreSignal()
	{
		EmitSignal(SignalName.HighScoreChanged, HighScore);
	}
}
