using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.UI;

/// <summary>
/// Controls the Heads-Up Display (Score, Level Info, etc.)
/// </summary>
public partial class HUDController : Control
{
	[Export]
	public Label ScoreLabel { get; set; }
	[Export]
	public Control GameOverPanel { get; set; }
	[Export]
	public Button RestartButton { get; set; }

	public override void _Ready()
	{
		if (ScoreLabel == null)
		{
			GD.PushError("[HUDController] ScoreLabel is not assigned!");
			return;
		}

		// Init state
		GameOverPanel.Visible = false;
		UpdateScoreUI(GameManager.Instance.CurrentScore);

		// Signals
		GameManager.Instance.ScoreChanged += UpdateScoreUI;
		GameManager.Instance.GameEnded += ShowGameOver;
		RestartButton.Pressed += OnRestartClicked;
	}

	public override void _ExitTree()
	{
		// Always unsubscribe from static events/signals to prevent memory leaks
		if (GameManager.Instance != null)
		{
			GameManager.Instance.ScoreChanged -= UpdateScoreUI;
			GameManager.Instance.GameEnded -= ShowGameOver;
		}
	}

	private void UpdateScoreUI(int newScore)
	{
		// Format: "Score: 1234"
		// For later localization, we will use a key like "UI_SCORE"
		ScoreLabel.Text = $"功德: {newScore}";
	}

	private void ShowGameOver()
	{
		GameOverPanel.Visible = true;
		// Optionally, we could also play a sound or animation here.
	}

	private void OnRestartClicked()
	{
		GameOverPanel.Visible = false;
		GameManager.Instance.ResetGame();
		GetTree().ReloadCurrentScene();
	}
}
