using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    public VerticalScrollSound scrollSound;
    public GameObject optionCanvas;

    public static ObjectPool<FadeBackground> bgPool;
    public FadeBackground bgPrefab, curBackground;
    private Sprite background;

    private bool IsBGLoadDone = false;

    public delegate void DelSelectSound( Song _song );
    public event DelSelectSound OnSelectSound;

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );
        //StartCoroutine( FadeBackground() );
        ChangeAction( SceneAction.FreeStyle );
    }

    public void Start()
    {
        ChangePreview();
    }

    private void ChangePreview()
    {
        if ( scrollSound.IsDuplicate ) return;
        
        Song curSong = GameManager.Inst.CurrentSound;
        StartCoroutine( FadeBackground() );

        Globals.Timer.Start();
        {
            SoundManager.Inst.LoadBgm( curSong.audioPath, SOUND_LOAD_TYPE.STREAM );
            SoundManager.Inst.PlayBgm();
        }
        OnSelectSound( curSong );

        Debug.Log( $"Sound Load {Globals.Timer.End()} ms" );

        // 중간부터 재생
        int time = curSong.previewTime;
        if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
        else             SoundManager.Inst.SetPosition( ( uint )time );
    }

    protected IEnumerator LoadBackground( string _path )
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path );
        yield return www.SendWebRequest();

        Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;
        background = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GlobalSetting.PPU, 0, SpriteMeshType.FullRect );

        // 원시 버젼 메모리 재할당이 큼
        //Texture2D tex = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        //byte[] binaryData = File.ReadAllBytes( _path );

        //while ( !tex.LoadImage( binaryData ) ) yield return null;
        //background = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GlobalSetting.PPU, 0, SpriteMeshType.FullRect );

        IsBGLoadDone = true;
    }

    public IEnumerator FadeBackground()
    {
        Globals.Timer.Start();
        {
            StartCoroutine( LoadBackground( GameManager.Inst.CurrentSound.imagePath ) );
            yield return new WaitUntil( () => IsBGLoadDone );
            if ( curBackground != null )
                 curBackground.Despawn();

            curBackground = bgPool.Spawn();
            curBackground.image.sprite = background;

            IsBGLoadDone = false;
        }
        Debug.Log( $"BackgroundLoad {Globals.Timer.elapsedMilliSeconds} ms" );
    }
    #endregion



    public override void KeyBind()
    {
        Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => scrollSound.PrevMove() );
        Bind( SceneAction.FreeStyle, KeyCode.UpArrow, () => ChangePreview() );
              
        Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => scrollSound.NextMove() );
        Bind( SceneAction.FreeStyle, KeyCode.DownArrow, () => ChangePreview() );
              
        Bind( SceneAction.FreeStyle, KeyCode.Return, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.GAME ) );
              
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( true ) );
        Bind( SceneAction.FreeStyle, KeyCode.Space, () => ChangeAction( SceneAction.FreeStyleOption ) );
              
        Bind( SceneAction.FreeStyle, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.LOBBY ) );
    }
}