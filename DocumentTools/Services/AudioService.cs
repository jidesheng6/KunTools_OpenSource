using NAudio.CoreAudioApi;
using System;

namespace DocumentTools.Services
{
    /// <summary>
    /// 音频控制服务：封装系统音频设备的控制操作
    /// </summary>
    public class AudioService : IAudioService
    {
        public void SetSystemAudioVolume(bool isSetMute, float audioVolumeValue)
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                using (var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                {
                    defaultDevice.AudioEndpointVolume.Mute = isSetMute;
                    defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = audioVolumeValue;
                }
            }
        }

        public (bool AudioMute, float AudioPer) GetSystemDefaultAudioStatus()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                using (var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                {
                    return (defaultDevice.AudioEndpointVolume.Mute, defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar);
                }
            }
        }
    }

    public interface IAudioService
    {
        void SetSystemAudioVolume(bool isSetMute, float audioVolumeValue);
        (bool AudioMute, float AudioPer) GetSystemDefaultAudioStatus();
    }
}
