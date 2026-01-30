using LibVLCSharp.Shared;

namespace musical_journey.Services;

public interface IAudioService
{
    MediaPlayer MediaPlayer { get; }
    void Play(string path);
    void Stop();
}