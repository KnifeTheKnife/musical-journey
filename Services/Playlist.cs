using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace musical_journey.Services.Interfaces;

public class Playlist : IPlaylist
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public ObservableCollection<Song> Songs { get; }

    public Playlist(string name)
    {
        Name = name;
        CreatedDate = DateTime.Now;
        ModifiedDate = DateTime.Now;
        Songs = new ObservableCollection<Song>();
    }

 
    public void AddSong(Song song)
    {
        if (!Songs.Any(s => s.path == song.path))
        {
            Songs.Add(song);
            ModifiedDate = DateTime.Now;
        }
    }


    public bool RemoveSong(string songPath)
    {
        var song = Songs.FirstOrDefault(s => s.path == songPath);
        if (song.path != null)
        {
            Songs.Remove(song);
            ModifiedDate = DateTime.Now;
            return true;
        }
        return false;
    }


    public bool RemoveSongAt(int index)
    {
        if (index >= 0 && index < Songs.Count)
        {
            Songs.RemoveAt(index);
            ModifiedDate = DateTime.Now;
            return true;
        }
        return false;
    }


    public int GetSongCount() => Songs.Count;


    public void Clear()
    {
        Songs.Clear();
        ModifiedDate = DateTime.Now;
    }
}
