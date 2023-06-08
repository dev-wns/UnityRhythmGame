using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType { None, Default, Slider }


public class InputSystem : MonoBehaviour
{
    #region Variables
    #region Objects
    private Lane lane;
    private InGame scene;
    private Judgement judge;
    #endregion

    #region Note
    private ObjectPool<NoteRenderer> notePool;
    public NoteRenderer note1 /* Lane 0,2,3,5 */, note2 /* Lane 1,4 */, noteMedian;
    private List<Note> noteDatas = new List<Note>();
    private int noteSpawnIndex;
    private double endNoteTime;
    #endregion

    private Queue<NoteRenderer> notes            = new Queue<NoteRenderer>();
    private Queue<NoteRenderer> sliderMissQueue  = new Queue<NoteRenderer>();
    private Queue<NoteRenderer> sliderEarlyQueue = new Queue<NoteRenderer>();

    private Note curData;
    private NoteRenderer curNote;

    public event Action<NoteType, InputType>  OnHitNote;
    public event Action<InputType> OnInputEvent;

    private KeyCode key;
    private KeySound curSound;
    private bool isAuto, isPress;

    #region Time
    private double inputStartTime;
    private double inputHoldTime;
    #endregion

    #region Auto
    private double target;
    #endregion
    #endregion

    #region Unity Event Function

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += StartProcess;
        scene.OnGameOver  += GameOver;
        scene.OnReLoad    += ReLoad;
        scene.OnPause     += Pause;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();

        lane  = GetComponent<Lane>(); 
        lane.OnLaneInitialize += Initialize;

        isAuto = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );
        NowPlaying.Inst.OnSpawnObjects += SpawnNotes;
    }

    private void Start()
    {
        target = UnityEngine.Random.Range( -( float )Judgement.NoteJudgeData.bad, ( float )Judgement.NoteJudgeData.bad );
    }

    private void LateUpdate()
    {
        if ( scene.IsGameInputLock ) 
             return;

        // Note Select
        if ( curNote == null && notes.Count > 0 )
        {
            curNote  = notes.Dequeue();
            curSound = curNote.Sound;
        }

        // Judgement
        if ( isAuto ) AutoCheckNote();
        else
        {
            CheckNote();

            // Input Effect
            if ( Input.GetKeyDown( key ) )
            {
                OnInputEvent?.Invoke( InputType.Down );
                SoundManager.Inst.Play( curSound );
            }
            else if ( Input.GetKeyUp( key ) )
            {
                OnInputEvent?.Invoke( InputType.Up );
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        NowPlaying.Inst.OnSpawnObjects -= SpawnNotes;
    }
    #endregion

    #region Event
    public void Initialize( int _key )
    {
        key = KeySetting.Inst.Keys[( GameKeyCount )NowPlaying.KeyCount][_key];

        NoteRenderer note = note1;
        if ( NowPlaying.KeyCount == 4 )      note = _key == 1 || _key == 2 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 6 ) note = _key == 1 || _key == 4 ? note2 : note1;
        else if ( NowPlaying.KeyCount == 7 ) note = _key == 1 || _key == 5 ? note2 : _key == 3 ? noteMedian : note1;
        notePool ??= new ObjectPool<NoteRenderer>( note, 10 );

        if ( noteDatas.Count > 0 )
        {
            curData = noteDatas[noteSpawnIndex];
            curSound = noteDatas[noteSpawnIndex].keySound;
        }
    }

    private void StartProcess()
    {
        StartCoroutine( SliderMissCheck() );
        StartCoroutine( SliderEarlyCheck() );
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        while ( sliderMissQueue.Count > 0 )
        {
            var slider = sliderMissQueue.Dequeue();
            slider.Despawn();
        }

        while ( sliderEarlyQueue.Count > 0 )
        {
            var slider = sliderEarlyQueue.Dequeue();
            slider.Despawn();
        }

        curNote?.Despawn();
        while ( notes.Count > 0 )
        {
            var note = notes.Dequeue();
            note.Despawn();
        }

        NowPlaying.Inst.ResetData();
        noteSpawnIndex = 0;
        curNote        = null;
        isPress        = false;
        curSound       = new KeySound();
    }

    public void AddNote( in Note _note )
    {
        endNoteTime = endNoteTime < _note.time ? _note.time : endNoteTime;
        noteDatas.Add( _note );
    }

    private void GameOver()
    {
        OnInputEvent?.Invoke( InputType.Up );
    }

    /// <summary>
    /// process the slider when pausing, it will be judged immediately.
    /// </summary>
    private void Pause( bool _isPause )
    {
        OnInputEvent?.Invoke( InputType.Up );

        if ( !_isPause || curNote == null || !curNote.IsSlider ) 
             return;

        if ( isAuto )
        {
            judge.ResultUpdate( HitResult.Perfect, NoteType.Slider );
            SelectNextNote();
        }
        else
        {
            curNote.SetSliderFail();
            judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
            sliderMissQueue.Enqueue( curNote );
            SelectNextNote( false );
        }
    }
    #endregion

    #region Note Process
    private void SpawnNotes( double _playback, double _scaledPlayback )
    {
        while ( noteSpawnIndex < noteDatas.Count && curData.calcTime <= _scaledPlayback + GameSetting.PreLoadTime )
        {
            NoteRenderer note = notePool.Spawn();
            note.SetInfo( lane.Key, in curData, noteSpawnIndex );
            notes.Enqueue( note );

            if ( ++noteSpawnIndex < noteDatas.Count )
                 curData = noteDatas[noteSpawnIndex];
        }
    }

    private IEnumerator SliderEarlyCheck()
    {
        var WaitEnqueue  = new WaitUntil( () => sliderEarlyQueue.Count > 0 );
        while ( true )
        {
            yield return WaitEnqueue;

            var slider = sliderEarlyQueue.Peek();
            if ( slider.SliderTime < NowPlaying.Playback )
            {
                slider.Despawn();
                sliderEarlyQueue.Dequeue();
            }
        }
    }

    private IEnumerator SliderMissCheck()
    {
        var WaitEnqueue  = new WaitUntil( () => sliderMissQueue.Count > 0 );
        while ( true )
        {
            yield return WaitEnqueue;
            
            var slider = sliderMissQueue.Peek();
            if ( slider.TailPos < -640f )
            {
                slider.Despawn();
                sliderMissQueue.Dequeue();
            }
        }
    }

    private void SelectNextNote( bool _isDespawn = true )
    {
        if ( curNote.SpawnIndex == ( noteDatas.Count - 1 ) )
             scene.HitLastNote( lane.Key );

        if ( _isDespawn )
        {
            curNote.gameObject.SetActive( false );
            curNote.Despawn();
        } 
            
        curNote = null;
        isPress = false;
    }

    private void AutoCheckNote()
    {
        if ( curNote == null ) return;

        double startDiff = curNote.Time       - NowPlaying.Playback;
        double endDiff   = curNote.SliderTime - NowPlaying.Playback;

        if ( !curNote.IsSlider )
        {
            if ( startDiff <= ( GameSetting.IsAutoRandom ? target : 0d ) )
            {
                target = UnityEngine.Random.Range( -( float )Judgement.NoteJudgeData.bad, ( float )Judgement.NoteJudgeData.bad );

                OnInputEvent?.Invoke( InputType.Down );
                OnInputEvent?.Invoke( InputType.Up );

                OnHitNote?.Invoke( NoteType.Default, InputType.Down );
                judge.ResultUpdate( GameSetting.IsAutoRandom ? target : 0d, NoteType.Default );
                SoundManager.Inst.Play( curSound );
                SelectNextNote();
            }
        }
        else
        {
            if ( !isPress )
            {
                if ( startDiff < 0d )
                {
                    OnInputEvent?.Invoke( InputType.Down );

                    isPress = true;
                    curNote.StartResizeSlider();
                    OnHitNote?.Invoke( NoteType.Slider, InputType.Down );
                    SoundManager.Inst.Play( curSound );
                    judge.ResultUpdate( 0d, NoteType.Default );

                    inputStartTime = curNote.Time;
                }
            }
            else
            {
                if ( endDiff < 0d )
                {
                    OnInputEvent?.Invoke( InputType.Up );

                    OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                    judge.ResultUpdate( 0d, NoteType.Slider );
                    SelectNextNote();
                }

                inputHoldTime = NowPlaying.Playback - inputStartTime;
                if ( inputHoldTime > .1f )
                {
                    judge.ResultUpdate( HitResult.None, NoteType.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1f );
                }
            }
        }
    }

    private void CheckNote()
    {
        if ( curNote == null ) return;

        double startDiff = curNote.Time - NowPlaying.Playback;
        if ( !curNote.IsSlider )
        {
            if ( Input.GetKeyDown( key ) && judge.CanBeHit( startDiff, NoteType.Default ) )
            {
                OnHitNote?.Invoke( NoteType.Default, InputType.Down );
                judge.ResultUpdate( startDiff, NoteType.Default );
                SelectNextNote();
                return;
            }

            if ( judge.IsMiss( startDiff, NoteType.Default ) )
            {
                judge.ResultUpdate( HitResult.Miss, NoteType.Default );
                SelectNextNote();
            }
        }
        else
        {
            if ( !isPress )
            {
                if ( Input.GetKeyDown( key ) && judge.CanBeHit( startDiff, NoteType.Default ) )
                {
                    isPress = true;
                    curNote.StartResizeSlider();

                    OnHitNote?.Invoke( NoteType.Slider, InputType.Down );
                    judge.ResultUpdate( startDiff, NoteType.Default );

                    inputStartTime = curNote.Time;
                    return;
                }

                if ( judge.IsMiss( startDiff, NoteType.Default ) )
                {
                    curNote.SetSliderFail();
                    judge.ResultUpdate( HitResult.Miss, NoteType.Slider, 2 );
                    sliderMissQueue.Enqueue( curNote );
                    SelectNextNote( false );
                }
            }
            else
            {
                double endDiff = curNote.SliderTime - NowPlaying.Playback;
                if ( endDiff < 0d )
                {
                    judge.ResultUpdate( endDiff, NoteType.Slider );
                    OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                    SelectNextNote();
                    return;
                }

                inputHoldTime = NowPlaying.Playback - inputStartTime;
                if ( inputHoldTime > .1d )
                {
                    judge.ResultUpdate( HitResult.None, NoteType.None );
                    inputStartTime = NowPlaying.Playback - ( inputHoldTime - .1d );
                }
                
                if ( Input.GetKeyUp( key ) )
                {
                    if ( judge.CanBeHit( endDiff, NoteType.Slider ) )
                    {
                        OnHitNote?.Invoke( NoteType.Slider, InputType.Up );
                        judge.ResultUpdate( endDiff, NoteType.Slider );
                        sliderEarlyQueue.Enqueue( curNote );
                        SelectNextNote( false );
                    }
                    else
                    {
                        curNote.SetSliderFail();
                        judge.ResultUpdate( HitResult.Miss, NoteType.Slider );
                        sliderMissQueue.Enqueue( curNote );
                        SelectNextNote( false );
                    }
                }
            }
        }
    }
    #endregion
}
