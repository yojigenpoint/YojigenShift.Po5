using Godot;

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
	[Signal]
	public delegate void GameEndedEventHandler();

	public int CurrentScore { get; private set; } = 0;
	public bool IsGameOver { get; private set; } = false;

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
		}
	}

	/// <summary>
	/// Adds points to the score and emits a signal to update UI.
	/// </summary>
	public void AddScore(int amount)
	{
		if (IsGameOver) return;
		CurrentScore += amount;
		EmitSignal(SignalName.ScoreChanged, CurrentScore);
	}

	public void TriggerGameOver()
	{
		if (IsGameOver) return;
		IsGameOver = true;
		EmitSignal(SignalName.GameEnded);
	}

	public void ResetGame()
	{
		IsGameOver = false;
		CurrentScore = 0;
		EmitSignal(SignalName.ScoreChanged, CurrentScore);
	}
}
