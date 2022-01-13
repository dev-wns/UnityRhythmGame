using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BooleanOption { Off, On, Count }

public enum Alignment { Left, Center, Right, Count, }

public enum GameRandom
{
    None,
    Mirror,
    Random,
    Half_Random,
    Max_Random,
    Count,
}

public enum GameFader
{
    None,
    Fade_In,
    Fade_Out,
    Count,
}

[Flags]
public enum GameMode
{
    None     = 0,
    AutoPlay = 1 << 0,
    NoFail   = 1 << 1,
    NoSlider = 1 << 2,

    All      = int.MaxValue,
}

[Flags]
public enum GameVisualFlag
{
    None        = 0,
    BGAPlay     = 1 << 0,
    TouchEffect = 1 << 1,
    LineEffect  = 1 << 2,
    ShowMeasure = 1 << 3,
    ShowJudge   = 1 << 4,

    All         = int.MaxValue,
}
public enum GameKeyAction : int
{
    _0, _1, _2, _3, _4, _5, Count // InGame Input Keys
};

public static class Globals
{
    public static Timer Timer = new Timer();
}

public class GameSetting : SingletonUnity<GameSetting>
{
    // Mode
    public static GameVisualFlag CurrentVisualFlag    = GameVisualFlag.All;
    public static GameMode       CurrentGameMode      = GameMode.None;
    public static GameRandom     CurrentRandom        = GameRandom.None;
    public static GameFader      CurrentFader         = GameFader.None;
    public static Alignment      CurrentGearAlignment = Alignment.Center;

    // PPU
    public static int PPU { get; private set; } = 100; // pixel per unit

    // Speed
    private static int OriginScrollSpeed = 25;
    public static float ScrollSpeed
    {
        
        get { return OriginScrollSpeed * .0015f; }
        set
        {
            var speed = OriginScrollSpeed + Mathf.FloorToInt( value );
            if ( speed <= 1 )
            {
                Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
                return;
            }

            OriginScrollSpeed = speed;
            Debug.Log( $"ScrollSpeed : {OriginScrollSpeed}" );
        }
    }
    public static float Weight { get { return ( 60f / NowPlaying.Inst.CurrentSong.medianBpm ) * ScrollSpeed; } }
    public static float PreLoadTime { get { return ( 1250f / Weight ); } }

    // Sound
    public static float SoundPitch = 1f;

    // Opacity
    public static float BGAOpacity = 0f;
    public static float PanelOpacity = 0f;

    // IO
    public static readonly string SoundDirectoryPath = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
    public static readonly string DefaultImagePath   = System.IO.Path.Combine( Application.dataPath, "Textures", "Default", "DefaultImage.jpg" );
    public static readonly string FailedPath         = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );

    // Measure
    public static float MeasureHeight { get; private set; } = 3f;

    // Jugdement
    public static float JudgePos    { get; set; }         = -470f;
    public static float JudgeHeight { get; private set; } = 100f; // scaleY

    // note
    public static float NoteWidth { get; private set; }  = 80f;
    public static float NoteHeight { get; private set; } = 30f;
    public static float NoteBlank { get; private set; }  = 2f;
    public static float NoteStartPos { get { return -( ( NoteWidth * 5f ) + ( NoteBlank * 7f ) ) * .5f; } }

    // Gear
    public static float GearStartPos { get { return ( -( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ) * .5f ); } }
    public static float GearWidth { get { return ( ( NoteWidth * 6f ) + ( NoteBlank * 7f ) ); } }

    public Dictionary<GameKeyAction, KeyCode> Keys = new Dictionary<GameKeyAction, KeyCode>();
    private KeyCode[] defaultKeys = new KeyCode[]
    {
        KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.K, KeyCode.L, KeyCode.Semicolon,
        KeyCode.Alpha1, KeyCode.Alpha2,
        KeyCode.Escape
    };

    private void Awake()
    {
        for ( int i = 0; i < defaultKeys.Length; i++ )
        {
            Keys.Add( ( GameKeyAction )i, defaultKeys[i] );
        }
    }
}
