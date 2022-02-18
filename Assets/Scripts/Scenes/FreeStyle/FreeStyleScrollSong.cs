using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FreeStyleScrollSong : ScrollBase, IKeyBind
{
    public SongInfomation prefab;

    public int maxShowCount = 7;
    public int extraCount   = 8;
    private int median;

    public delegate void DelSelectSong( Song _song );
    public event DelSelectSong OnSelectSong;

    private Song curSong;
    private float playback;
    private float soundLength;
    private uint previewTime;
    private readonly uint waitPreviewTime = 500;

    private Scene scene;
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private LinkedListNode<SongInfomation> curNode, prevNode, nextNode;
    private CustomVerticalLayoutGroup group;

    private RectTransform rt;
    private float curPos;
    private float size;

    private void Awake()
    {
        IsLoop = true;
        rt = transform as RectTransform;
        curPos = rt.anchoredPosition.y;

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        KeyBind();

        median = Mathf.FloorToInt( ( maxShowCount + extraCount ) / 2f );

        // 객체 할당
        Length    = NowPlaying.Inst.Songs.Count;
        int count = NowPlaying.Inst.CurrentSongIndex - median < 0 ?
                    NowPlaying.Inst.CurrentSongIndex - median + Length :
                    NowPlaying.Inst.CurrentSongIndex - median;

        for ( int i = 0; i < maxShowCount + extraCount; i++ )
        {
            if ( count > Length - 1 ) count = 0;
            var song = Instantiate( prefab, transform );
            song.Initialize();
            song.SetInfo( NowPlaying.Inst.GetSongIndexAt( count++ ) );

            songs.AddLast( song );
        }
        Select( NowPlaying.Inst.CurrentSongIndex );

        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();

        // 중앙 위치에 있는 객체
        curNode = songs.First;
        for ( int i = 0; i < median; i++ )
        {
            curNode = curNode.Next;
        }
        size = curNode.Value.rt.sizeDelta.y + group.spacing;

        // Active 조절기준이 될 객체
        prevNode = songs.First;
        nextNode = songs.Last;
        int extraHalf = Mathf.FloorToInt( extraCount / 2f );
        for ( int i = 0; i < extraHalf; i++ )
        {
            prevNode.Value.gameObject.SetActive( false );
            nextNode.Value.gameObject.SetActive( false );
            prevNode = prevNode.Next;
            nextNode = nextNode.Previous;
        }

        curNode.Value.ActiveOutline( true );
    }

    private void Start()
    {
        UpdateSong();
    }

    private void Update()
    {
        playback += Time.deltaTime * 1000f;

        if ( soundLength + waitPreviewTime < playback &&
             !SoundManager.Inst.IsPlaying( ChannelType.BGM ) )
        {
            SoundManager.Inst.Play();
            SoundManager.Inst.Position = GetPreviewTime();
            playback = previewTime;
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();

        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        // Song 정보 수정
        int infoIndex = CurrentIndex - median < 0 ?
                        CurrentIndex - median + Length:
                        CurrentIndex - median;
        last.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );
        
        // 활성화
        nextNode.Value.gameObject.SetActive( false );
        nextNode = nextNode.Previous;

        prevNode = prevNode.Previous;
        prevNode.Value.gameObject.SetActive( true );

        // 노드 이동
        songs.RemoveLast();
        songs.AddFirst( last );
       
        // 아웃라인 
        curNode.Value.ActiveOutline( false );
        curNode = curNode.Previous;
        curNode.Value.ActiveOutline( true );

        // 위치 갱신
        curPos -= size;
        rt.DOAnchorPosY( curPos, .25f );

        UpdateSong();
    }

    public override void NextMove()
    {
        base.NextMove();

        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );
        
        // Song 정보 수정
        int infoIndex = CurrentIndex + median >= Length ?
                        CurrentIndex + median - Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );

        // 활성화
        prevNode.Value.gameObject.SetActive( false );
        prevNode = prevNode.Next;

        nextNode = nextNode.Next;
        nextNode.Value.gameObject.SetActive( true );

        // 노드 이동
        songs.RemoveFirst();
        songs.AddLast( first );

        // 아웃라인 
        curNode.Value.ActiveOutline( false );
        curNode = curNode.Next;
        curNode.Value.ActiveOutline( true );

        // 위치 갱신
        curPos += size;
        rt.DOAnchorPosY( curPos, .25f );

        UpdateSong();
    }

    private void UpdateSong()
    {
        Debug.Log( $"{NowPlaying.Inst.Songs.Count} {CurrentIndex}" );
        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;
        curSong = NowPlaying.Inst.CurrentSong;
        soundLength = curSong.totalTime;

        OnSelectSong( curSong );

        SoundManager.Inst.LoadBgm( curSong.audioPath, false, true, false );
        SoundManager.Inst.Play();

        previewTime = GetPreviewTime();
        SoundManager.Inst.Position = previewTime;
        playback = previewTime;
    }

    private uint GetPreviewTime()
    {
        int time = curSong.previewTime;
        if ( time <= 0 ) return ( uint )( soundLength * 0.3141592f );
        else             return ( uint )curSong.previewTime;
    }

    public void KeyBind()
    {
        scene.Bind( SceneAction.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
        scene.Bind( SceneAction.Main, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SceneType.Game ) );

        scene.Bind( SceneAction.Main, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.UpArrow, () => PrevMove() );

        scene.Bind( SceneAction.Main, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
    }
}
