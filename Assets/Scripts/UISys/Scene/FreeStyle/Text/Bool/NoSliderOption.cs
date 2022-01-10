using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoSliderOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = GameSetting.CurrentGameMod.HasFlag( GameMod.NoSlider ) ? 1 : 0;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )BooleanOption.Count; i++ )
        {
            texts.Add( ( ( BooleanOption )i ).ToString() );
        }
    }
    public override void Process()
    {
        if ( curIndex == 0 ) GameSetting.CurrentGameMod &= ~GameMod.NoSlider;
        else                 GameSetting.CurrentGameMod |=  GameMod.NoSlider;
        Debug.Log( GameSetting.CurrentGameMod.HasFlag( GameMod.NoSlider ) );
    }
}
