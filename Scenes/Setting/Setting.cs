using Godot;
using System;

public partial class Setting : Node
{
    // Sliders để điều chỉnh âm lượng
    [Export] private HSlider masterSlider; //điều chỉnh âm lượng chung
    [Export] private HSlider musicSlider; //điều chỉnh âm lượng nhạc nền
    [Export] private HSlider sfxSlider; //điều chỉnh âm lượng hiệu ứng âm thanh


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Lấy giá trị âm lượng hiện tại để hiển thị
        UpdateSliderValue("Chung", masterSlider);
        UpdateSliderValue("Music", musicSlider);
        UpdateSliderValue("SFX", sfxSlider);

        //giá trị thay đổi âm lượng
        masterSlider.ValueChanged += (value) => OnVolumeChanged("Master", (float)value);
        musicSlider.ValueChanged += (value) => OnVolumeChanged("Music", (float)value);
        sfxSlider.ValueChanged += (value) => OnVolumeChanged("SFX", (float)value);
    }

    private void OnVolumeChanged(string busName, float value)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        float dbValue = Mathf.LinearToDb(value);

        AudioServer.SetBusVolumeDb(busIndex, dbValue);

        // Nếu kéo về 0 thì tắt tiếng hoàn toàn (Mute)
        AudioServer.SetBusMute(busIndex, value <= 0.001f);
    }

    // Cập nhật giá trị slider dựa trên âm lượng hiện tại của bus
    private void UpdateSliderValue(string busName, HSlider slider)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        float dbValue = AudioServer.GetBusVolumeDb(busIndex);
        slider.Value = Mathf.DbToLinear(dbValue);
    }
}
