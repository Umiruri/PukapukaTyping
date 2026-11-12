//確認画面

using UnityEngine;
using UnityEngine.InputSystem;

public class ConfirmManager : MonoBehaviour
{
    //ゲーム開始処理を実行したか
    private bool isStarting = false;

    void Update()
    {
        //すでにゲーム開始処理を実行している場合
        if(isStarting) return;

        //キーボードが使用できない場合
        if(Keyboard.current == null) return;

        if(Keyboard.current.spaceKey.wasPressedThisFrame ||
           Keyboard.current.enterKey.wasPressedThisFrame ||
           Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            isStarting = true;
            ButtonManager.LoadGameScene();
        }
    }
}