//難易度選択画面の管理

using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    //難易度を設定して確認画面へ移動
    void SelectDifficulty(Difficulty difficulty)
    {
        //すでに画面遷移中の場合
        if(isTransitioning) return;

        //難易度が正しいか確認
        if(!System.Enum.IsDefined(typeof(Difficulty), difficulty))
        {
            Debug.LogError("無効な難易度が指定されました。");
            return;
        }

        GameSettings.SelectedDifficulty = difficulty;

        //画面遷移を実行
        isTransitioning = true;
        ButtonManager.LoadConfirmScene();
    }

    //画面遷移中かどうか
    private bool isTransitioning = false;

    //かんたん
    public void SelectEasy()
    {
        SelectDifficulty(Difficulty.Easy);
    }

    //ふつう
    public void SelectNormal()
    {
        SelectDifficulty(Difficulty.Normal);
    }

    //むずかしい
    public void SelectHard()
    {
        SelectDifficulty(Difficulty.Hard);
    }
    
}