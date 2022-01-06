using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VerticalScrollSound : HideScroll
{
    public OptionBase songPrefab; // sound infomation prefab
    private RectTransform rt;
    
    public int spacing = 0;
    private float curPos, moveOffset;

    protected override void Awake()
    {
        base.Awake();

        curIndex = GameManager.Inst.CurrentSoundIndex;
        moveOffset = ( songPrefab.transform as RectTransform ).rect.height + spacing;

        // 시작인덱스 위치로 이동
        curPos = ( curIndex ) * moveOffset;
        rt.localPosition = new Vector2( rt.localPosition.x, curPos );
    }

    protected override void CreateContents() 
    {
        rt = GetComponent<RectTransform>();

        // Create Scroll Contents
        contents.Capacity =  GameManager.Inst.Songs.Count;
        for ( int i = 0; i < GameManager.Inst.Songs.Count; i++ )
        {
            // scrollview song contents
            var obj = Instantiate( songPrefab, rt );

            // 사운드 이름 설정
            Song data = GameManager.Inst.Songs[i];
            System.Text.StringBuilder artist = new System.Text.StringBuilder();
            artist.Capacity = data.artist.Length + 8 + data.creator.Length;
            artist.Append( data.artist ).Append( " // " ).Append( data.creator );

            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            info[0].text = data.title;
            info[1].text = data.version;
            //info[2].text = artist.ToString();

            // 객체 위치 설정
            RectTransform dataTransform = obj.transform as RectTransform;
            float height = dataTransform.sizeDelta.y;
            float startPos = -rt.anchoredPosition.y - ( height * .5f );
            float offset = ( height + spacing ) * i;
            dataTransform.anchoredPosition = new Vector2( 0, startPos - offset );

            contents.Add( obj );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        curPos -= moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        GameManager.Inst.SelectSong( curIndex );
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        curPos += moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        GameManager.Inst.SelectSong( curIndex );
    }
}
