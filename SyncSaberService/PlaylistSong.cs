using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SyncSaberService
{
    public class PlaylistSong
    {
        [JsonProperty("hash")]
        public string hash { get; set; }
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("songName")]
        public string songName { get; set; }
        [JsonIgnore]
        private Data.SongInfo _songInfo;

        public PlaylistSong(string _songName, string _songIndex, string _hash)
        {
            songName = _songName;
            key = _songIndex;
            hash = _hash;
        }

        public Data.SongInfo ToSongInfo()
        {
            if (!string.IsNullOrEmpty(hash))
                _songInfo = Web.BeatSaverReader.GetSongFromHash(hash);
            else if (!string.IsNullOrEmpty(key))
                _songInfo = Web.BeatSaverReader.GetSongFromKey(key);
            else
                _songInfo = null;
            return _songInfo;
        }
    }
}
