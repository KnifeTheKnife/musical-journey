using System;
using musical_journey.Services.Interfaces;
namespace musical_journey.Services;

public class GetTag : IGetTags
{
    public Song GetTagsByPath(string Path)
    {
        try
        {
            var file = TagLib.File.Create(Path);
            string title = file.Tag.Title ?? "";
            string artist = file.Tag.FirstPerformer ?? "";
            string album = file.Tag.Album ?? "";
            string trackNo = file.Tag.Track.ToString() ?? "";
            string genre = file.Tag.FirstGenre ?? "";
            string date = file.Tag.Year.ToString() ?? "";
            string discNo = file.Tag.Disc.ToString() ?? "";

            return new Song(title, artist, album, trackNo, genre, date, discNo, Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading tags from {Path}: {ex.Message}");
            return new Song("", "", "", "", "", "", "", Path);
        }
    }
}