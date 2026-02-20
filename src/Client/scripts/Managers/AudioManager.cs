using Godot;
using System.Collections.Generic;

namespace YojigenShift.Po5.Scripts.Managers;

/// <summary>
/// Manages BGM and SFX playback. 
/// Uses a pool of players to handle multiple overlapping sounds (e.g., chain reactions).
/// </summary>
public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	public bool IsBGMEnabled { get; private set; } = true;
	public bool IsSFXEnabled { get; private set; } = true;

	private AudioStreamPlayer _bgmPlayer;
	private List<AudioStreamPlayer> _sfxPool = new List<AudioStreamPlayer>();
	private int _poolSize = 12; // Allow 12 concurrent sounds
	private int _poolIndex = 0;

	// Cache loaded sounds so we don't load from disk every time
	private Dictionary<string, AudioStream> _soundLibrary = new Dictionary<string, AudioStream>();

	private readonly Dictionary<string, string> _sfxPaths = new()
	{
		{ "click", "res://assets/audio/sfx/ui_click.ogg" },
		{ "popup", "res://assets/audio/sfx/ui_popup.ogg" },
		{ "merge", "res://assets/audio/sfx/merge.ogg" },
		{ "destroy", "res://assets/audio/sfx/destroy.ogg" },
		{ "spawn", "res://assets/audio/sfx/spawn.ogg" },
		{ "warning", "res://assets/audio/sfx/warning.wav" },
		{ "game_over", "res://assets/audio/sfx/game_over.wav" }
	};

	public override void _Ready()
	{
		if (Instance == null) Instance = this;
		else { QueueFree(); return; }

		InitializeBGM();
		InitializeSFXPool();
		LoadAllSounds();
	}

	private void InitializeBGM()
	{
		_bgmPlayer = new AudioStreamPlayer();
		_bgmPlayer.Name = "BGMPlayer";
		_bgmPlayer.Bus = "Master";
		AddChild(_bgmPlayer);
	}

	private void InitializeSFXPool()
	{
		for (int i = 0; i < _poolSize; i++)
		{
			var player = new AudioStreamPlayer();
			player.Name = $"SFXPlayer_{i}";
			player.Bus = "Master";
			AddChild(player);
			_sfxPool.Add(player);
		}
	}

	public void SetBGMEnabled(bool enabled)
	{
		IsBGMEnabled = enabled;
		_bgmPlayer.VolumeDb = enabled ? 0f : -80f;
	}

	public void SetSFXEnabled(bool enabled)
	{
		IsSFXEnabled = enabled;

		foreach (var player in _sfxPool)
		{
			player.VolumeDb = enabled ? 0f : -80f;
		}
	}

	/// <summary>
	/// Preloads a sound resource.
	/// </summary>
	public void LoadSound(string key, string path)
	{
		if (ResourceLoader.Exists(path))
		{
			var stream = GD.Load<AudioStream>(path);
			_soundLibrary[key] = stream;
		}
		else
		{
			GD.PrintErr($"[AudioManager] Sound file not found: {path}");
		}
	}

	/// <summary>
	/// Plays a sound effect by key.
	/// </summary>
	public void PlaySFX(string key, float pitchRange = 0.1f)
	{
		if (!IsSFXEnabled || !_soundLibrary.ContainsKey(key)) return;

		// Get next available player in pool
		var player = _sfxPool[_poolIndex];
		_poolIndex = (_poolIndex + 1) % _poolSize;

		player.Stream = _soundLibrary[key];
		player.VolumeDb = 0f;

		// Randomize pitch slightly to make it sound less repetitive
		if (pitchRange > 0)
		{
			player.PitchScale = 1.0f + (float)GD.RandRange(-pitchRange, pitchRange);
		}
		else
		{
			player.PitchScale = 1.0f;
		}

		player.Play();
	}

	public void PlayBGM(string path, float crossfadeTime = 1.0f)
	{
		if (!ResourceLoader.Exists(path))
		{
			GD.PrintErr($"[AudioManager] Cannot find the BGM: {path}");
			return;
		}

		var newStream = GD.Load<AudioStream>(path);

		if (_bgmPlayer.Stream == newStream && _bgmPlayer.Playing) return;

		_bgmPlayer.Stream = newStream;
		_bgmPlayer.Play();

		_bgmPlayer.VolumeDb = -80f;
		float targetDb = IsBGMEnabled ? 0f : -80f;
		
		var tween = CreateTween();
		tween.TweenProperty(_bgmPlayer, "volume_db", targetDb, crossfadeTime);
	}

	public void StopBGM(float fadeOutTime = 1.0f)
	{
		if (!_bgmPlayer.Playing) return;

		var tween = CreateTween();
		tween.TweenProperty(_bgmPlayer, "volume_db", -80f, fadeOutTime);
		tween.TweenCallback(Callable.From(_bgmPlayer.Stop));
	}

	private void LoadAllSounds()
	{
		foreach (var kvp in _sfxPaths)
		{
			if (ResourceLoader.Exists(kvp.Value))
				_soundLibrary[kvp.Key] = GD.Load<AudioStream>(kvp.Value);
			else
				GD.PrintErr($"[AudioManager] Cannot find the SFX: {kvp.Value} (Please check the path and extension)");
		}
	}
}
