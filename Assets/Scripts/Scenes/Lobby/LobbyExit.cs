using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : SceneOptionBase
{
    public override void KeyBind()
    {
        currentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => PrevMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow,  () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => NextMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Lobby ) );
        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }

    public void Cancel()
    {
        currentScene.ChangeAction( SceneAction.Lobby );
        gameObject.SetActive( false );
    }

    public void Exit()
    {
        Application.Quit();
    }
}
