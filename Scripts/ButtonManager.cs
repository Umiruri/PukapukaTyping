//共通の画面遷移・ボタン操作

using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    //ひとつ前に開いていたシーン
    public static string PreviousScene = GameSettings.TitleScene;

    //画面遷移中かどうか
    private static bool isTransitioning = false;

    //シーン読み込み完了時に呼ばれる
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isTransitioning = false;
    }

    //シーンを安全に読み込む共通処理
    static bool LoadSceneSafely(string sceneName)
    {
        //すでに画面遷移中の場合
        if(isTransitioning) return false;

        //シーン名が空の場合
        if(string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("読み込むシーン名が設定されていません");
            return false;
        }

        //シーンがBuild Profilesに登録されていない場合
        if(!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("読み込めないシーンです: " + sceneName);
            return false;
        }

        //現在と同じシーンの場合
        if(SceneManager.GetActiveScene().name == sceneName) return false;

        isTransitioning = true;

        //シーン読み込み完了時のイベントを登録
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(sceneName);

        return true;
    }

    //難易度選択画面へ移動
    public void OpenDifficulty()
    {
        LoadSceneSafely(GameSettings.DifficultyScene);
    }

    //確認画面へ移動
    public static void LoadConfirmScene()
    {
        LoadSceneSafely(GameSettings.ConfirmScene);
    }

    //ゲーム開始
    public void StartGame()
    {
        LoadGameScene();
    }

    //ゲーム画面へ移動する共通処理
    public static void LoadGameScene()
    {
        LoadSceneSafely(GameSettings.GameScene);
    }

    //遊び方画面を開く
    public void OpenHowToPlay()
    {
        PreviousScene = SceneManager.GetActiveScene().name;
        LoadSceneSafely(GameSettings.HowToPlayScene);
    }

    //設定画面を開く
    public void OpenSettings()
    {
        PreviousScene = SceneManager.GetActiveScene().name;
        LoadSceneSafely(GameSettings.SettingsScene);
    }

    //タイトル画面へ戻る
    public void BackToTitle()
    {
        LoadSceneSafely(GameSettings.TitleScene);
    }

    //前のシーンへ戻る
    public void ReturnToPreviousScene()
    {
        string sceneName = PreviousScene;

        //戻り先が無効な場合はタイトル画面へ戻す
        if(string.IsNullOrWhiteSpace(sceneName) ||
           !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning("戻り先のシーンが無効なため、タイトル画面へ戻ります");
            sceneName = GameSettings.TitleScene;
        }

        LoadSceneSafely(sceneName);
    }

    //ゲームを終了
    public void ExitGame()
    {
        Application.Quit();
    }
    
}