using LibVLCSharp.Shared;

namespace musical_journey.Services.Interfaces;

public interface IAudioService
{
    MediaPlayer MediaPlayer { get; }
    void Play(string path);
    void Stop();
}