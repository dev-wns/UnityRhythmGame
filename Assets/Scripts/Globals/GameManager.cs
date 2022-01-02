using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonUnity<GameManager>
{
    public List<Song> Songs = new List<Song>();
    public Song CurrentSound { get; private set; }
    public int CurrentSoundIndex { get; private set; }
    public float MedianBpm { get; private set; }

    private void Awake()
    {
        using ( FileConverter converter = new FileConverter() )
        {
            converter.ReLoad();
        }

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParseArray( ref Songs );
        }

        if ( Songs.Count > 0 ) { SelectSong( 0 ); }
        Debug.Log( "Parse Success " );
    }

    private void Update()
    {
        //SoundManager.Inst.Update();
    }

    private void OnApplicationQuit()
    {
        //SoundManager.Inst.Release();
    }

    public void SelectSong( int _index )
    {
        if ( _index < 0 || _index > Songs.Count - 1 )
        {
            Debug.Log( $"Sound Select Out Of Range. Index : {_index}" );
            return;
        }

        CurrentSoundIndex = _index;
        CurrentSound      = Songs[_index];
        MedianBpm         = Songs[_index].medianBpm;
    }
}
