using Godot;
using YojigenShift.Po5.Scripts.Managers;

namespace YojigenShift.Po5.Scripts.UI;

public partial class SettingsController : Control
{
	[Export] public HSlider BGMSlider { get; set; }
	[Export] public HSlider SFXSlider { get; set; }
	[Export] public Button CloseButton { get; set; }
	[Export] public Button QuitButton { get; set; } // "Return Main Menu" Button

	[Export] public bool IsInGame { get; set; } = false;

	public override void _Ready()
	{
		if (BGMSlider != null)
		{
			BGMSlider.Value = AudioManager.Instance.BGMVolume;
			BGMSlider.ValueChanged += OnBGMChanged;
		}

		if (SFXSlider != null)
		{
			SFXSlider.Value = AudioManager.Instance.SFXVolume;
			SFXSlider.ValueChanged += OnSFXChanged;
		}

		if (CloseButton != null) CloseButton.Pressed += ClosePanel;

		if (QuitButton != null)
		{
			QuitButton.Visible = IsInGame;
			QuitButton.Pressed += OnQuitPressed;
		}

		Visible = false;
	}

	public void Open()
	{
		Visible = true;

		if (IsInGame)
		{
			GetTree().Paused = true;
		}
	}

	private void ClosePanel()
	{
		Visible = false;

		if (IsInGame)
		{
			GetTree().Paused = false;
		}
	}

	private void OnBGMChanged(double value)
	{
		AudioManager.Instance.SetBGMVolume((float)value);
	}

	private void OnSFXChanged(double value)
	{
		AudioManager.Instance.SetSFXVolume((float)value);
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;

		if (SceneManager.Instance != null)
			SceneManager.Instance.ChangeScene("res://scenes/UI/MainMenu.tscn");
		else
			GetTree().ChangeSceneToFile("res://scenes/UI/MainMenu.tscn");
	}
}
