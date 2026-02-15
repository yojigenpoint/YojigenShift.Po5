using Godot;
using YojigenShift.Po5.Scripts.Bridges;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.Po5.Scripts.Resources;

namespace YojigenShift.Po5.Scripts.UI;

/// <summary>
/// Controls the Heads-Up Display (Score, Level Info, etc.)
/// </summary>
public partial class HUDController : Control
{
	[Export]
	public Label ScoreLabel { get; set; }
	[Export]
	public Label HighScoreLabel { get; set; }
	[Export]
	public Control GameOverPanel { get; set; }
	[Export]
	public Button RestartButton { get; set; }

	[Export] public Control NextBallContainer { get; set; }
	[Export] public TextureRect NextBallPreview { get; set; }
	[Export] public CheckButton PreviewToggle { get; set; }

	[Export] public ElementAsset ElemTheme { get; set; }

	public override void _Ready()
	{
		if (ScoreLabel == null || GameOverPanel == null || RestartButton == null || HighScoreLabel == null)
		{
			GD.PushError("[HUDController] UI elements missing!");
		}

		// Init state
		GameOverPanel.Visible = false;
		UpdateScoreUI(GameManager.Instance.CurrentScore);
		UpdateHighScoreUI(GameManager.Instance.HighScore);

		// Signals
		GameManager.Instance.ScoreChanged += UpdateScoreUI;
		GameManager.Instance.HighScoreChanged += UpdateHighScoreUI;
		GameManager.Instance.GameEnded += ShowGameOver;
		RestartButton.Pressed += OnRestartClicked;

		if (PreviewToggle != null)
		{
			PreviewToggle.Toggled += OnPreviewToggled;
			PreviewToggle.ButtonPressed = true;
			NextBallContainer.Visible = true;
		}
	}

	public override void _ExitTree()
	{
		// Always unsubscribe from static events/signals to prevent memory leaks
		if (GameManager.Instance != null)
		{
			GameManager.Instance.ScoreChanged -= UpdateScoreUI;
			GameManager.Instance.HighScoreChanged -= UpdateHighScoreUI;
			GameManager.Instance.GameEnded -= ShowGameOver;
		}
	}

	public void OnNextBallChanged(int nextTypeInt)
	{
		if (ElemTheme == null) return;

		var type = (YiBridge.GameElement)nextTypeInt;

		NextBallPreview.Texture = ElemTheme.GetIcon(type);
		NextBallPreview.Modulate = ElemTheme.GetColor(type);
	}

	private void OnPreviewToggled(bool active)
	{
		NextBallContainer.Visible = active;
	}

	private void UpdateScoreUI(int newScore)
	{
		// Format: "Score: 1234"
		// For later localization, we will use a key like "UI_SCORE"
		ScoreLabel.Text = $"功德: {newScore}";
	}

	private void UpdateHighScoreUI(int newHighScore)
	{
		HighScoreLabel.Text = $"最高: {newHighScore}";
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
