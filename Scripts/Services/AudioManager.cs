using Godot;
using System;

public partial class AudioManager : Node
{
    // static giúp gọi hàm từ bất kỳ script nào khác
    public static AudioManager Instance { get; private set; }

    // Gọi hàm khởi tạo khi node được thêm vào cảnh
    public AudioStreamPlayer _musicPlayer; //dùng cho nhạc nền
    public AudioStreamPlayer _sfxPlayer;   //dùng cho hiệu ứng âm thanh
    private ConfigFile config = new ConfigFile();
    private string configPath = "user://settings.cfg";

    public override void _Ready()
    {
        //kiểm tra instance đảm bảo chỉ có 1 cái 
        if(Instance == null)
        {
            Instance = this;
            //always đảm bảo âm thanh vẫn hoạt động kể cả khi dừng 
            ProcessMode = ProcessModeEnum.Always;
        }
        else
        {
            QueueFree(); // nếu cái thứ 2 xuất hiện thì xóa nó đi
            return;
        } 
            
        //Cài đặt Music Player (nhạc nền)
        _musicPlayer = new AudioStreamPlayer();
        _musicPlayer.Bus = "Music"; //gán bus Music
        AddChild(_musicPlayer);

        //Cài đặt SFX Player (hiệu ứng âm thanh)
        _sfxPlayer = new AudioStreamPlayer();
        _sfxPlayer.Bus = "SFX"; //gán bus SFX
        AddChild(_sfxPlayer);

        // Load volume settings
        LoadSettings();
    }

    //Hàm phát nhạc nền
    public void PlayMusic(AudioStream musicStream)
    {
        if(_musicPlayer.Stream == musicStream && _musicPlayer.Playing)
            return; //nếu nhạc đang phát thì không làm gì cả

        _musicPlayer.Stop(); //dừng nhạc hiện tại
        _musicPlayer.Stream = musicStream; //gán nhạc mới
        _musicPlayer.Play(); //phát nhạc mới
    }

    //Hàm phát hiệu ứng âm thanh
    public void PlaySFX(AudioStream sfxStream)
    {
        AudioStreamPlayer tempPlayer = new AudioStreamPlayer();
        AddChild(tempPlayer);

        tempPlayer.Stream = sfxStream;
        tempPlayer.Bus = "SFX";
        tempPlayer.Play();

        //khi phát xong thì giải phóng node
        tempPlayer.Finished += () => tempPlayer.QueueFree();
    }

    //Hàm chỉnh âm lượng (Dùng cho Slider trong cài đặt)
    public void SetVolume (string busName, float linearVal)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        float dbVal = Mathf.LinearToDb(linearVal);

        AudioServer.SetBusVolumeDb(busIndex, dbVal);
    }

    private void LoadSettings()
    {
        Error err = config.Load(configPath);
        if (err == Error.Ok)
        {
            float musicVolume = (float)config.GetValue("audio", "music_volume", 0.5);
            float sfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.5);
            SetVolume("Music", musicVolume);
            SetVolume("SFX", sfxVolume);
        }
        else
        {
            // Default volumes
            SetVolume("Music", 0.5f);
            SetVolume("SFX", 0.5f);
        }
    }
}
