using Godot;
using System;

public partial class SettingScenes : Node2D
{
	[Export] HSlider MusicSlider;
	[Export] HSlider SFXSlider;
	[Export] Button ExitButton;
	
	private ConfigFile config = new ConfigFile();
	private string configPath = "user://settings.cfg";
	
	public override void _Ready()
	{
		// Load volume hiện tại
		LoadSettings();
		
		// Set volume ban đầu
		AudioManager.Instance.SetVolume("Music", (float)MusicSlider.Value);
		AudioManager.Instance.SetVolume("SFX", (float)SFXSlider.Value);
		
		// Connect signals
		MusicSlider.ValueChanged += OnMusicVolumeChanged;
		SFXSlider.ValueChanged += OnSFXVolumeChanged;
		ExitButton.Pressed += OnExitPressed;
	}

	private void OnMusicVolumeChanged(double value)
	{
		AudioManager.Instance.SetVolume("Music", (float)value);
		SaveSettings();
	}

	private void OnSFXVolumeChanged(double value)
	{
		AudioManager.Instance.SetVolume("SFX", (float)value);
		SaveSettings();
	}
	
	private void OnExitPressed()
	{
		SaveSettings();
		QueueFree();
	}
	
	private void LoadSettings()
	{
		Error err = config.Load(configPath);
		if (err == Error.Ok)
		{
			MusicSlider.Value = (double)config.GetValue("audio", "music_volume", 0.5);
			SFXSlider.Value = (double)config.GetValue("audio", "sfx_volume", 0.5);
		}
		else
		{
			// Default values
			MusicSlider.Value = 0.5;
			SFXSlider.Value = 0.5;
		}
	}
	
	private void SaveSettings()
	{
		config.SetValue("audio", "music_volume", MusicSlider.Value);
		config.SetValue("audio", "sfx_volume", SFXSlider.Value);
		config.Save(configPath);
	}
}
