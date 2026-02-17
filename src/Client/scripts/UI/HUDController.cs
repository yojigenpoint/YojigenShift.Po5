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
	[ExportGroup("Core UI")]
	[Export] public Label ScoreLabel { get; set; }
	[Export] public Label HighScoreLabel { get; set; }
	[Export] public Button PauseButton { get; set; }

	[ExportGroup("Game Over UI")]
	[Export]public Control GameOverPanel { get; set; }
	[Export] public Label GameOverTitle { get; set; }
	[Export] public Label GameOverMessage { get; set; }
	[Export] public Button RestartButton { get; set; }

	[ExportGroup("Preview UI")]
	[Export] public Control NextBallContainer { get; set; }
	[Export] public TextureRect NextBallPreview { get; set; }
	[Export] public CheckButton PreviewToggle { get; set; }

	[ExportGroup("External Components")]
	[Export] public ElementAsset ElemTheme;
	[Export] public SettingsController SettingsPopup;
	[Export] public NarrativeAsset StoryData;

	public override void _Ready()
	{
		if (ScoreLabel == null || GameOverPanel == null || RestartButton == null || HighScoreLabel == null || SettingsPopup == null)
		{
			GD.PushError("[HUDController] UI elements missing!");
		}

		// Init state
		if (GameOverPanel != null) GameOverPanel.Visible = false;
		UpdateScoreUI(GameManager.Instance.CurrentScore);
		UpdateHighScoreUI(GameManager.Instance.HighScore);

		// Signals
		GameManager.Instance.ScoreChanged += UpdateScoreUI;
		GameManager.Instance.HighScoreChanged += UpdateHighScoreUI;
		GameManager.Instance.GameEnded += ShowGameOver;
		
		if (RestartButton != null) RestartButton.Pressed += OnRestartClicked;

		if (PauseButton != null && SettingsPopup != null)
			PauseButton.Pressed += () => SettingsPopup.Open();

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
		if (GameOverPanel == null) return;

		GameOverPanel.Visible = true;

		if (StoryData != null && GameOverMessage != null)
		{
			var comforts = StoryData.GameOverComforts;
			if (comforts != null && comforts.Length > 0)
			{
				int index = GD.RandRange(0, comforts.Length - 1);
				GameOverMessage.Text = comforts[index];
			}
			else
			{
				GameOverMessage.Text = "休息一下，养精蓄锐。";
			}
		}

		if (GameOverTitle != null)
		{
			bool isNewRecord = GameManager.Instance.CurrentScore >= GameManager.Instance.HighScore && GameManager.Instance.CurrentScore > 0;
			GameOverTitle.Text = isNewRecord ? "功德圆满！" : "胜败乃常事";
			GameOverTitle.Modulate = isNewRecord ? new Color(1, 0.8f, 0) : new Color(1, 1, 1); // Gold or White
		}
	}

	private void OnRestartClicked()
	{
		GameOverPanel.Visible = false;
		GameManager.Instance.ResetGame();
		GetTree().ReloadCurrentScene();
	}
}
