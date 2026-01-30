using System;
using LibVLCSharp.Shared;

namespace musical_journey.Services.Interfaces;

public class AudioService : IAudioService, IDisposable
{
    private readonly LibVLC _libVlc;
    public MediaPlayer MediaPlayer { get; }

    public AudioService()
    {
        _libVlc = new LibVLC();
        MediaPlayer = new MediaPlayer(_libVlc);
    }

    public void Play(string path)
    {
        using var media = new Media(_libVlc, path, FromType.FromPath);
        MediaPlayer.Play(media);
    }

    public void Stop() => MediaPlayer.Stop();

    public void Dispose()
    {
        MediaPlayer.Dispose();
        _libVlc.Dispose();
    }
}
