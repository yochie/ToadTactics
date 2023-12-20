using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SongListData")]
public class SongListSO : ScriptableObject
{
    [SerializeField]
    private List<AudioClip> gameplaySongList;

    [SerializeField]
    private List<AudioClip> menuSongList;

    private const string resourcePath = "SongListData";
    private static SongListSO singleton = null;
    public static SongListSO Singleton
    {
        get
        {
            if (SongListSO.singleton == null)
                SongListSO.singleton = Resources.Load<SongListSO>(resourcePath);
            return SongListSO.singleton;
        }
    }

    public List<AudioClip> GetRandomGameplaySongQueue()
    {
        return this.GenerateRandomQueue(this.gameplaySongList);
    }

    public List<AudioClip> GetRandomMenuSongQueue()
    {
        return this.GenerateRandomQueue(this.menuSongList);
    }

    public List<AudioClip> GenerateRandomQueue(List<AudioClip> list)
    {
        List<AudioClip> songQueue = list.GetRange(0, list.Count);
        songQueue.Shuffle<AudioClip>();
        return songQueue;
    }

}
