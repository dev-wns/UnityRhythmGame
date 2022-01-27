using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSystem : MonoBehaviour
{
    public InGame CurrentScene { get; private set; }
    private Lane lane;

    private ObjectPool<NoteRenderer> nPool;
    public NoteRenderer nPrefab;

    private List<Note> notes = new List<Note>();
    private Note curNote;
    private int curIndex;
    private double loadTime;

    private void Awake()
    {
        lane = GetComponent<Lane>();
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        CurrentScene.OnGameStart += () => StartCoroutine( Process() );
        CurrentScene.OnScrollChanged += ScrollUpdate;

        nPool = new ObjectPool<NoteRenderer>( nPrefab, 10 );

        ScrollUpdate();
    }

    private void OnDestroy() => CurrentScene.OnScrollChanged -= ScrollUpdate;

    public void ScrollUpdate() => loadTime = GameSetting.PreLoadTime;

    public void AddNote( Note _note ) => notes.Add( _note );

    public void Despawn( NoteRenderer _note ) => nPool.Despawn( _note );

    private IEnumerator Process()
    {
        if( notes.Count > 0 )
            curNote = notes[curIndex];

        WaitUntil waitNextNote = new WaitUntil( () => curNote.calcTime <= NowPlaying.PlaybackChanged + loadTime );
        while ( curIndex < notes.Count )
        {
            yield return waitNextNote;
         
            NoteRenderer note = nPool.Spawn();
            note.SetInfo( lane.Key, this, in curNote );

            lane.InputSys.Enqueue( note );

            if ( ++curIndex < notes.Count )
                 curNote = notes[curIndex];
        }
    } 
}
