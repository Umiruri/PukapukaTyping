//結果画面の表示

using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public TMP_Text correctKeyText;
    public TMP_Text mistakeText;
    public TMP_Text totalInputText;
    public TMP_Text averageSpeedText;

    void Start()
    {
        //TextMeshProの参照確認
        if(!CheckReferences()) return;

        //結果データの取得
        int correctCount = Mathf.Max(0, ResultData.CorrectKeyCount);
        int mistakeCount = Mathf.Max(0, ResultData.MistakeCount);
        float averageSpeed = Mathf.Max(0f, ResultData.AverageSpeed);

        //正解タイプ数
        correctKeyText.text =
            "正解タイプ数：" + correctCount;

        //ミスタイプ数
        mistakeText.text =
            "ミスタイプ数：" + mistakeCount;

        //合計入力数
        int totalInputCount = correctCount + mistakeCount;

        totalInputText.text =
            "合計入力数：" + totalInputCount;

        //平均入力速度
        averageSpeedText.text =
            "平均入力速度：" + averageSpeed.ToString("F2") + " 回/秒";
    }

    //TextMeshProの参照確認
    bool CheckReferences()
    {
        bool isValid = true;

        if(correctKeyText == null)
        {
            Debug.LogError("Correct Key Textが設定されていません");
            isValid = false;
        }

        if(mistakeText == null)
        {
            Debug.LogError("Mistake Textが設定されていません");
            isValid = false;
        }

        if(totalInputText == null)
        {
            Debug.LogError("Total Input Textが設定されていません");
            isValid = false;
        }

        if(averageSpeedText == null)
        {
            Debug.LogError("Average Speed Textが設定されていません");
            isValid = false;
        }

        return isValid;
    }
}