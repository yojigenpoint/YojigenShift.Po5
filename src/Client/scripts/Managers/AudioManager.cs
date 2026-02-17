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

	public float BGMVolume { get; private set; } = 1.0f;
	public float SFXVolume { get; private set; } = 1.0f;

	private AudioStreamPlayer _bgmPlayer;
	private List<AudioStreamPlayer> _sfxPool = new List<AudioStreamPlayer>();
	private int _poolSize = 12; // Allow 12 concurrent sounds
	private int _poolIndex = 0;

	// Cache loaded sounds so we don't load from disk every time
	private Dictionary<string, AudioStream> _soundLibrary = new Dictionary<string, AudioStream>();

	public override void _Ready()
	{
		if (Instance == null) Instance = this;
		else { QueueFree(); return; }

		InitializeBGM();
		InitializeSFXPool();

		// TODO: Load your actual sound files here
		// LoadSound("spawn", "res://assets/audio/spawn.wav");
		// LoadSound("merge", "res://assets/audio/merge.wav");
		// LoadSound("gameover", "res://assets/audio/gameover.wav");
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

	public void SetBGMVolume(float value)
	{
		BGMVolume = Mathf.Clamp(value, 0f, 1f);
		float db = (value <= 0.001f) ? -80f : Mathf.LinearToDb(value);
		_bgmPlayer.VolumeDb = db;
	}

	public void SetSFXVolume(float value)
	{
		SFXVolume = Mathf.Clamp(value, 0f, 1f);
		float db = (value <= 0.001f) ? -80f : Mathf.LinearToDb(value);

		foreach (var player in _sfxPool)
		{
			player.VolumeDb = db;
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
		if (!_soundLibrary.ContainsKey(key)) return;

		// Get next available player in pool
		var player = _sfxPool[_poolIndex];
		_poolIndex = (_poolIndex + 1) % _poolSize;

		player.Stream = _soundLibrary[key];

		float db = (SFXVolume <= 0.001f) ? -80f : Mathf.LinearToDb(SFXVolume);
		player.VolumeDb = db;

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

	public void PlayBGM(string path)
	{
		if (ResourceLoader.Exists(path))
		{
			_bgmPlayer.Stream = GD.Load<AudioStream>(path);
			_bgmPlayer.Play();
		}
	}

	public void StopBGM()
	{
		_bgmPlayer.Stop();
	}
}
