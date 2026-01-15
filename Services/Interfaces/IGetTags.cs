using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace musical_journey.Services.Interfaces;

public interface IGetTags
{
    /// <summary>
    /// Fetches tags from music file.
    /// </summary>
    /// <param name="Path"></param>
    /// <returns>Song struct.</returns>
    Song GetTagsByPath(string Path);

}