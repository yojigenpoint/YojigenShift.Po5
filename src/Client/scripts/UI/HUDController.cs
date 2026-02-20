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
	[Export] public Label GameOverScore { get; set; }
	[Export] public Label GameOverMessage { get; set; }
	[Export] public Button RestartButton { get; set; }
	[Export] public Button MainMenuButton { get; set; }

	[ExportGroup("Preview UI")]
	[Export] public Control NextBallContainer { get; set; }
	[Export] public TextureRect NextBallPreview { get; set; }

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

		if (MainMenuButton != null) MainMenuButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/UI/MainMenu.tscn");

		if (PauseButton != null && SettingsPopup != null)
			PauseButton.Pressed += () => SettingsPopup.Open();

		if (SettingsPopup != null)
		{
			SettingsPopup.PreviewToggled += SetPreviewVisible;

			if (SettingsPopup.PreviewToggle != null)
				SetPreviewVisible(SettingsPopup.PreviewToggle.ButtonPressed);
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationTranslationChanged)
		{
			UpdateScoreUI(GameManager.Instance.CurrentScore);
			UpdateHighScoreUI(GameManager.Instance.HighScore);

			if (GameOverPanel != null && GameOverPanel.Visible)
				UpdateGameOverUI();
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

		if (SettingsPopup != null)
		{
			SettingsPopup.PreviewToggled -= SetPreviewVisible;
		}
	}

	public void OnNextBallChanged(int nextTypeInt)
	{
		if (ElemTheme == null) return;

		var type = (YiBridge.GameElement)nextTypeInt;

		NextBallPreview.Texture = ElemTheme.GetIcon(type);
		NextBallPreview.Modulate = ElemTheme.GetColor(type);
	}

	private void SetPreviewVisible(bool isVisible)
	{
		if (NextBallContainer != null)
			NextBallContainer.Visible = isVisible;
	}

	private void UpdateScoreUI(int newScore)
	{
		// Format: "Score: 1234"
		// For later localization, we will use a key like "UI_SCORE"
		ScoreLabel.Text = Helpers.GetLocalizedFormat(Tr("TXT_SCORE"), newScore);
	}

	private void UpdateHighScoreUI(int newHighScore)
	{
		HighScoreLabel.Text = Helpers.GetLocalizedFormat(Tr("TXT_HIGH_SCORE"), newHighScore);
	}

	private void ShowGameOver()
	{
		if (GameOverPanel == null) return;

		AudioManager.Instance.StopBGM(2.0f);
		AudioManager.Instance.PlaySFX("game_over");

		GameOverPanel.Visible = true;

		var paper = GameOverPanel.GetNodeOrNull<Control>("PaperBg");

		GameOverPanel.Modulate = new Color(1, 1, 1, 0); // Start transparent
		var tween = CreateTween();
		tween.TweenProperty(GameOverPanel, "modulate:a", 1.0f, 0.3f);

		if (paper != null)
		{
			paper.PivotOffset = paper.Size / 2;
			paper.Scale = new Vector2(0.5f, 0.5f);

			tween.Parallel().TweenProperty(paper, "scale", Vector2.One, 0.5f)
				 .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		}

		UpdateGameOverUI();
	}

	private void UpdateGameOverUI()
	{
		if (StoryData != null && GameOverMessage != null)
		{
			var comforts = StoryData.GameOverComforts;
			if (comforts != null && comforts.Length > 0)
			{
				int index = GD.RandRange(0, comforts.Length - 1);
				GameOverMessage.Text = Tr(comforts[index]);
			}
			else
			{
				GameOverMessage.Text = "休息一下，养精蓄锐。";
			}
		}

		if (GameOverScore != null)
			GameOverScore.Text = Helpers.GetLocalizedFormat(Tr("TXT_SCORE"), GameManager.Instance.CurrentScore);

		if (GameOverTitle != null)
		{
			bool isNewRecord = GameManager.Instance.CurrentScore >= GameManager.Instance.HighScore && GameManager.Instance.CurrentScore > 0;
			GameOverTitle.Text = isNewRecord ? Tr("GAME_OVER_WIN") : Tr("GAME_OVER_LOSE");
			GameOverTitle.Modulate = isNewRecord ? new Color(1, 0.8f, 0) : new Color(1, 1, 1); // Gold or White
		}

	}

	private void OnRestartClicked()
	{
		AudioManager.Instance.PlaySFX("click");
		GameOverPanel.Visible = false;
		GameManager.Instance.ResetGame();
		GetTree().ReloadCurrentScene();
	}
}
