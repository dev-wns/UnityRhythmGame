using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoteRenderer : MonoBehaviour
{
    private NoteSystem system;
    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;
    public double Time { get; private set; }
    public double CalcTime { get; private set; }
    public double SliderTime { get; private set; }
    public double CalcSliderTime { get; private set; }
    public bool IsSlider { get; private set; }
    public bool isHolding { get; set; }

    private float column;
    private double newTime;

    private static readonly Color MiddleColor   = new Color( 0.2078432f, 0.7843138f, 1f, 1f );
    private static readonly Color BodyColor     = new Color( .4f, .4f, .4f, 1f );
    private static readonly Color BodyFailColor = new Color( .15f, .15f, .15f, 1f );

    private void Awake()
    {
        headTf = head.transform;
        bodyTf = body.transform;
        tailTf = tail.transform;
        tail.enabled = false;
    }

    public void SetInfo( int _lane, NoteSystem _system, in Note _data )
    {
        system    = _system;
        newTime   = _data.calcTime;
        isHolding = false;

        Time           = _data.time;
        CalcTime       = _data.calcTime;
        SliderTime     = _data.sliderTime;
        CalcSliderTime = _data.calcSliderTime;
        IsSlider       = _data.isSlider;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );

        if ( IsSlider )
        {
            //system.CurrentScene.OnScrollChanged += ScaleUpdate;
            head.enabled = true;
            body.enabled = true;
            //tail.enabled = true;
        }
        else
        {
            head.enabled = true;
            body.enabled = false;
            //tail.enabled = false;
        }

        ScaleUpdate();
        if ( _lane == 1 || _lane == 4 ) head.color = tail.color = MiddleColor;
        else                            head.color = tail.color = Color.white;

        body.color = BodyColor;
    }

    public void SetBodyFail() => body.color = BodyFailColor;

    private void ScaleUpdate()
    {
        headTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        if ( IsSlider )
        {
            double sliderLengthAbs = Globals.Abs( ( CalcSliderTime - CalcTime ) * GameSetting.Weight );
            bodyTf.localScale = new Vector2( GameSetting.NoteWidth * .8f, ( float )sliderLengthAbs );
            //tail.transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        }
    }

    public void Despawn()
    {
        //if ( IsSlider )
        //    system.CurrentScene.OnScrollChanged -= ScaleUpdate;

        this.gameObject.SetActive( false );
        //head.enabled = body.enabled = false;
        system.Despawn( this );
    }

    private void LateUpdate()
    {
        double weight = GameSetting.Weight;
        if ( isHolding )
        {
            if ( head.transform.position.y <= GameSetting.JudgePos )
                 newTime = NowPlaying.PlaybackChanged;

            Vector2 startPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( newTime  - NowPlaying.PlaybackChanged ) * weight ) ) );
            headTf.position = bodyTf.position = startPos;
            tailTf.position = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( CalcSliderTime - NowPlaying.PlaybackChanged ) * weight ) ) );

            double bodyDiff = tailTf.position.y - headTf.position.y;
            double bodyScale = bodyDiff <= 0d ? 0d : bodyDiff;
            bodyTf.localScale = new Vector2( GameSetting.NoteWidth * .8f, ( float )bodyScale );
        }
        else
        {
            Vector2 startPos = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( newTime  - NowPlaying.PlaybackChanged ) * weight ) ) );
            headTf.position  = startPos;

            if ( IsSlider )
            {
                bodyTf.position = startPos;
                tailTf.position = new Vector2( column, GameSetting.JudgePos + ( float )( ( ( CalcSliderTime - NowPlaying.PlaybackChanged ) * weight ) ) );
            }
        }
    }
}
