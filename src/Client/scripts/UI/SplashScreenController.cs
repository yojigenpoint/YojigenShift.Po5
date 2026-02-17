using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.UI;

public partial class SplashScreenController : Control
{
	[Export] public TextureRect LogoStudio { get; set; }
	[Export] public TextureRect LogoGame { get; set; }
	[Export] public Control WarningContainer { get; set; }
	[Export] public TextureRect LoadingSpinner { get; set; }

	[Export(PropertyHint.File, "*.tscn")]
	public string NextScenePath { get; set; } = "res://scenes/UI/MainMenu.tscn";

	public override void _Ready()
	{
		// Initial State: Everything transparent
		if (LogoStudio != null) LogoStudio.Modulate = new Color(1, 1, 1, 0);
		if (LogoGame != null) LogoGame.Modulate = new Color(1, 1, 1, 0);
		if (WarningContainer != null) WarningContainer.Modulate = new Color(1, 1, 1, 0);
		if (LoadingSpinner != null) LoadingSpinner.Modulate = new Color(1, 1, 1, 0);

		// Start the animation sequence
		StartSplashSequence();
	}

	public override void _Process(double delta)
	{
		// Rotate the loading spinner
		if (LoadingSpinner != null && LoadingSpinner.Visible)
		{
			LoadingSpinner.RotationDegrees += (float)(delta * 180.0); // 180 degrees per second
		}
	}

	private async void StartSplashSequence()
	{
		// 1. Fade In Studio Logo
		if (LogoStudio != null)
		{
			var t1 = CreateTween();
			t1.TweenProperty(LogoStudio, "modulate:a", 1.0f, 1.0f);
			await ToSignal(t1, "finished");

			// Hold for 1 second
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

			// Fade Out Studio Logo
			var t2 = CreateTween();
			t2.TweenProperty(LogoStudio, "modulate:a", 0.0f, 0.5f);
			await ToSignal(t2, "finished");
		}

		// Fade In Game Logo
		if (LogoGame != null)
		{
			var t3 = CreateTween();
			t3.TweenProperty(LogoGame, "modulate:a", 1.0f, 1.0f);
			await ToSignal(t3, "finished");

			// Hold for 1 second
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

			// Fade Out Game Logo
			var t4 = CreateTween();
			t4.TweenProperty(LogoGame, "modulate:a", 0.0f, 0.5f);
			await ToSignal(t4, "finished");
		}

		// 2. Fade In Warning + Loading (Simulate loading assets)
		if (WarningContainer != null && LoadingSpinner != null)
		{
			var t5 = CreateTween();
			t5.SetParallel(true);
			t5.TweenProperty(WarningContainer, "modulate:a", 1.0f, 0.8f);
			t5.TweenProperty(LoadingSpinner, "modulate:a", 1.0f, 0.8f);
			await ToSignal(t5, "finished");

			// Hold for reading (2 seconds)
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		}

		// 3. Transition to Main Menu
		if (SceneManager.Instance != null)
		{
			SceneManager.Instance.ChangeScene(NextScenePath);
		}
		else
		{
			// Fallback if SceneManager isn't autoloaded
			GetTree().ChangeSceneToFile(NextScenePath);
		}
	}
}
