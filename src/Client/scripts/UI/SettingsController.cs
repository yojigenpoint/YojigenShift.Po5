using Godot;
using YojigenShift.Po5.Scripts.Managers;
using YojigenShift.YiFramework.Core;

namespace YojigenShift.Po5.Scripts.UI;

public partial class SettingsController : Control
{
	[Export] public CheckButton BGMToggle { get; set; }
	[Export] public CheckButton SFXToggle { get; set; }
	[Export] public CheckButton PreviewToggle { get; set; }
	[Export] public OptionButton LanguageSelect { get; set; }
	[Export] public Button CloseButton { get; set; }
	[Export] public Button QuitButton { get; set; } // "Return Main Menu" Button

	[Export] public bool IsInGame { get; set; } = false;

	[Signal] public delegate void PreviewToggledEventHandler(bool isOn);

	public override void _Ready()
	{
		if (BGMToggle != null)
		{
			BGMToggle.ButtonPressed = AudioManager.Instance.IsBGMEnabled;
			BGMToggle.Toggled += (isOn) => AudioManager.Instance.SetBGMEnabled(isOn);
		}

		if (SFXToggle != null)
		{
			SFXToggle.ButtonPressed = AudioManager.Instance.IsSFXEnabled;
			SFXToggle.Toggled += (isOn) => AudioManager.Instance.SetSFXEnabled(isOn);
		}

		if (PreviewToggle != null)
		{
			PreviewToggle.ButtonPressed = true;
			PreviewToggle.Toggled += OnPreviewToggled;
		}

		if (LanguageSelect != null)
		{
			string currentLocale = TranslationServer.GetLocale();
			LanguageSelect.Selected = currentLocale.StartsWith("zh") ? 0 : 1;
			LanguageSelect.ItemSelected += OnLanguageSelected;
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
		AudioManager.Instance.PlaySFX("popup");
		Visible = true;

		if (IsInGame)
		{
			GetTree().Paused = true;
		}
	}

	private void ClosePanel()
	{
		AudioManager.Instance.PlaySFX("click");
		Visible = false;

		if (IsInGame)
		{
			GetTree().Paused = false;
		}
	}

	private void OnQuitPressed()
	{
		AudioManager.Instance.PlaySFX("click");
		GetTree().Paused = false;

		if (SceneManager.Instance != null)
			SceneManager.Instance.ChangeScene("res://scenes/UI/MainMenu.tscn");
		else
			GetTree().ChangeSceneToFile("res://scenes/UI/MainMenu.tscn");
	}
	private void OnPreviewToggled(bool isOn)
	{
		EmitSignal(SignalName.PreviewToggled, isOn);
	}

	private void OnLanguageSelected(long index)
	{
		string locale = (index == 0) ? "zh_CN" : "en_US";
		TranslationServer.SetLocale(locale);
		YiLocalization.CurrentLanguage = locale;
	}
}
