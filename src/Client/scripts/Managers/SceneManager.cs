using Godot;

namespace YojigenShift.Po5.Scripts.Managers;

/// <summary>
/// Handles scene transitions with a fade-in/fade-out effect.
/// Must be configured as an Autoload in Project Settings.
/// </summary>
public partial class SceneManager : Node
{
	public static SceneManager Instance { get; private set; }

	private CanvasLayer _transitionLayer;
	private ColorRect _fadeRect;

	public override void _Ready()
	{
		if (Instance == null) Instance = this;
		else { QueueFree(); return; }

		// Setup the transition overlay layer programmatically
		_transitionLayer = new CanvasLayer();
		_transitionLayer.Layer = 100; // Ensure it's on top of everything (HUD is usually 1-10)
		AddChild(_transitionLayer);

		_fadeRect = new ColorRect();
		_fadeRect.Color = new Color(0, 0, 0, 0); // Start transparent
		_fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore; // Let clicks pass through when transparent
		_transitionLayer.AddChild(_fadeRect);
	}

	/// <summary>
	/// Changes the scene with a fade transition.
	/// </summary>
	/// <param name="scenePath">The resource path to the new scene (e.g., "res://scenes/MainMenu.tscn")</param>
	public async void ChangeScene(string scenePath)
	{
		// 1. Block input (optional, prevents accidental clicks during fade)
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Stop;

		// 2. Fade Out (Screen becomes Black)
		Tween tween = CreateTween();
		tween.TweenProperty(_fadeRect, "color:a", 1.0f, 0.5f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

		await ToSignal(tween, "finished");

		// 3. Change actual scene
		Error err = GetTree().ChangeSceneToFile(scenePath);
		if (err != Error.Ok)
		{
			GD.PushError($"[SceneManager] Failed to load scene: {scenePath}");
			// If failed, fade back in so game isn't stuck on black screen
		}

		// 4. Fade In (Screen reveals new scene)
		tween = CreateTween();
		tween.TweenProperty(_fadeRect, "color:a", 0.0f, 0.5f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);

		await ToSignal(tween, "finished");

		// 5. Unblock input
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
	}
}
