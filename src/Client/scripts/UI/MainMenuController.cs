using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.UI;

public partial class MainMenuController : Control
{
	[ExportGroup("UI References")]
	[Export] public Label HighScoreLabel { get; set; }
	[Export] public BaseButton StartButton { get; set; }  
	[Export] public BaseButton LibraryButton { get; set; }
	[Export] public BaseButton SettingsButton { get; set; }
	[Export] public Control VersionLabel { get; set; }
	[Export] public SettingsController SettingsPopup { get; set; }

	[ExportGroup("Scene Config")]
	[Export(PropertyHint.File, "*.tscn")]
	public string GameScenePath { get; set; } = "res://scenes/Stages/GameStage.tscn";
	[Export(PropertyHint.File, "*.tscn")]
	public string LibraryScenePath { get; set; } = "res://scenes/UI/Library.tscn";

	private TextureRect _gameTitle;

	public override void _Ready()
	{
		// 1. Initialization
		UpdateHighScore();
		_gameTitle = GetNode<TextureRect>("TitleLogo");

		// 2. Button Events
		if (StartButton != null) StartButton.Pressed += OnStartPressed;
		if (LibraryButton != null) LibraryButton.Pressed += OnLibraryPressed;
		if (SettingsButton != null) SettingsButton.Pressed += OnSettingsPressed;

		// 3. Fade in
		Modulate = new Color(1, 1, 1, 0);
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1.0f, 0.5f).SetEase(Tween.EaseType.Out);

		if (_gameTitle != null)
		{
			var tweenTitle = CreateTween();

			tweenTitle.TweenProperty(_gameTitle, "scale", new Vector2(1.05f, 1.05f), 2.0f)
				 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			tweenTitle.TweenProperty(_gameTitle, "scale", Vector2.One, 2.0f)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		}
	}

	private void UpdateHighScore()
	{
		if (HighScoreLabel != null && GameManager.Instance != null)
		{
			HighScoreLabel.Text = $"最高功德: {GameManager.Instance.HighScore}";
		}
	}

	private void OnStartPressed()
	{
		AudioManager.Instance.PlaySFX("ui_click");

		GD.Print("[MainMenu] Start Game Pressed");

		if (SceneManager.Instance != null)
		{
			SceneManager.Instance.ChangeScene(GameScenePath);
		}
		else
		{
			GetTree().ChangeSceneToFile(GameScenePath);
		}
	}

	private void OnLibraryPressed()
	{
		SceneManager.Instance.ChangeScene(LibraryScenePath);
	}

	private void OnSettingsPressed()
	{
		SettingsPopup.Open();
	}
}
