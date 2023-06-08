using UnityEngine;

public class NoteRenderer : MonoBehaviour, IObjectPool<NoteRenderer>
{
    public ObjectPool<NoteRenderer> pool { get; set; }

    private Note note;
    public SpriteRenderer head, body, tail;
    public int SpawnIndex { get; private set; }
    public float HeadPos => transform.position.y;
    public float TailPos => transform.position.y + BodyLength;
    public double Time => note.time;
    public double CalcTime => note.calcTime;
    public double SliderTime => note.sliderTime;
    public double CalcSliderTime => note.calcSliderTime;
    public bool IsSlider => note.isSlider;
    public bool ShouldResizeSlider { get; set; }
    public float BodyLength { get; private set; }
    public KeySound Sound => note.keySound;

    private float column;
    private static readonly Color NoteFailColor = new Color( .5f, .5f, .5f, 1f );
    private double newTime;

    private void Awake()
    {
        head.transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        body.transform.localPosition = new Vector2( 0, GameSetting.NoteHeight * .5f );
        tail.transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
    }

    public void SetInfo( int _lane, in Note _note, int _spawnIndex )
    {
        SpawnIndex = _spawnIndex;
        ShouldResizeSlider = false;
        note      = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        body.enabled = tail.enabled = IsSlider;
        head.color   = body.color = tail.color = Color.white;
        ResizeSlider( false );
        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( newTime - NowPlaying.ScaledPlayback ) * GameSetting.Weight );
    }

    public void SetSliderFail()
    {
        ShouldResizeSlider = false;
        head.color = body.color = tail.color = NoteFailColor;
    }

    public void Despawn()
    {
        ShouldResizeSlider = false;
        pool.Despawn( this );
    }

    public void StartResizeSlider()
    {
        if ( !IsSlider ) return;

        ShouldResizeSlider = true;
        ResizeSlider( true );
    }

    private void ResizeSlider( bool _isSnap )
    {
        if ( !IsSlider ) return;

        if ( _isSnap )
             newTime = NowPlaying.ScaledPlayback;

        BodyLength = ( float )( ( CalcSliderTime - newTime ) * GameSetting.Weight );

        float length         =  Global.Math.Clamp( BodyLength - GameSetting.NoteHeight,  0f, float.MaxValue );
        body.transform.localScale    = new Vector2( GameSetting.NoteWidth, length );
        tail.transform.localPosition = new Vector2( 0f, length );
    }

    private void LateUpdate()
    {
        // 롱노트 판정선에 붙기
        if ( IsSlider && ShouldResizeSlider && Time < NowPlaying.Playback )
             ResizeSlider( true );

        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( newTime - NowPlaying.ScaledPlayback ) * GameSetting.Weight );
    }
}
