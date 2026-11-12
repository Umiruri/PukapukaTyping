//ゲーム画面
//タイマー・問題表示・ゲーム進行

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    //Inspectorから設定
    public TMP_Text timerText;
    public TMP_Text questionText;
    public TMP_Text furiganaText;
    public TMP_Text romajiText;
    public TMP_Text inputText;
    public GameObject timeUpPanel;
    public GameObject pausePanel;

    public TextAsset questionCSV;
    public TextAsset inputRulesCSV;

    private float remainingTime;
    private bool isTimeUp = false;
    private bool isPaused = false;

    //画面遷移中かどうか
    private bool isTransitioning = false;

    private float elapsedPlayTime = 0f;

    //問題データ
    private List<QuestionData> questionList = new List<QuestionData>();

    private RomajiConverter romajiConverter;

    //現在の問題
    private string currentFurigana = "";

    //前回表示した問題の番号
    private int previousQuestionIndex = -1;

    //イベントを登録したキーボード
    private Keyboard subscribedKeyboard;

    //入力済みの文字
    private string inputBuffer = "";

    //正解になり得るローマ字の候補
    private List<string> possibleAnswers = new List<string>();

    //ミス回数
    private int mistakeCount = 0;

    //正解タイプ数
    private int correctKeyCount = 0;

    private class QuestionData
    {
        public string question;
        public string furigana;
    }

    void Start()
    {
        //必要な参照が設定されているか確認
        if(!CheckRequiredReferences())
        {
            enabled = false;
            return;
        }

        //ローマ字変換クラスを作成
        romajiConverter = new RomajiConverter();

        //入力ルールCSVを読み込む
        romajiConverter.LoadRules(inputRulesCSV);

        //問題CSVを読み込む
        LoadQuestions();

        //問題が1件も読み込めなかった場合
        if(questionList.Count == 0)
        {
            Debug.LogError("問題が読み込まれていないため、ゲームを開始できません");
            enabled = false;
            return;
        }

        //制限時間を取得
        remainingTime = GameSettings.TimeLimit;
        UpdateTimerText();

        //表示設定を反映
        romajiText.gameObject.SetActive(GameSettings.ShowRomaji);
        furiganaText.gameObject.SetActive(GameSettings.ShowFurigana);

        //最初の問題を表示
        ShowRandomQuestion();

        //ポーズ画面を最初は非表示にする
        pausePanel.SetActive(false);
    }

    //必要な参照が設定されているか確認
    bool CheckRequiredReferences()
    {
        bool isValid = true;

        if(timerText == null)
        {
            Debug.LogError("TimerTextが設定されていません");
            isValid = false;
        }

        if(questionText == null)
        {
            Debug.LogError("QuestionTextが設定されていません");
            isValid = false;
        }

        if(furiganaText == null)
        {
            Debug.LogError("FuriganaTextが設定されていません");
            isValid = false;
        }

        if(romajiText == null)
        {
            Debug.LogError("RomajiTextが設定されていません");
            isValid = false;
        }

        if(inputText == null)
        {
            Debug.LogError("InputTextが設定されていません");
            isValid = false;
        }

        if(timeUpPanel == null)
        {
            Debug.LogError("TimeUpPanelが設定されていません");
            isValid = false;
        }

        if(pausePanel == null)
        { 
            Debug.LogError("PausePanelが設定されていません");
            isValid = false;
        }

        if(questionCSV == null)
        {
            Debug.LogError("Question.csvが設定されていません");
            isValid = false;
        }

        if(inputRulesCSV == null)
        {
            Debug.LogError("InputRules.csvが設定されていません");
            isValid = false;
        }

        return isValid;
    }

    void OnEnable()
    {
        UpdateKeyboardSubscription();
    }

    //キーボードの登録状態を更新
    void UpdateKeyboardSubscription()
    {
        Keyboard currentKeyboard = Keyboard.current;

        //同じキーボードなら何もしない
        if(subscribedKeyboard == currentKeyboard) return;

        //以前のキーボードの登録を解除
        if(subscribedKeyboard != null) subscribedKeyboard.onTextInput -= OnTextInput;

        subscribedKeyboard = currentKeyboard;

        //新しいキーボードに登録
        if(subscribedKeyboard != null) subscribedKeyboard.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        if(subscribedKeyboard != null)
        {
            subscribedKeyboard.onTextInput -= OnTextInput;
            subscribedKeyboard = null;
        }
    }

    void Update()
    {
        UpdateKeyboardSubscription();

        //時間切れ後の処理
        if(isTimeUp)
        {
            if(Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                GoToResult();
            }

            return;
        }

        //ESCキーでポーズ・再開
        if(Keyboard.current != null &&
        Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if(isPaused) ResumeGame();
            else PauseGame();

            return;
        }

        //ポーズ中はここから先の処理を行わない
        if(isPaused) return;

        //今回のフレームで経過した時間
        float deltaTime = Time.deltaTime;

        //実際のプレイ時間を記録
        elapsedPlayTime += Mathf.Min(deltaTime, remainingTime);

        //残り時間を減らす
        remainingTime -= deltaTime;

        //時間切れの判定
        if(remainingTime <= 0)
        {
            remainingTime = 0;
            isTimeUp = true;

            timeUpPanel.SetActive(true);

            //結果データを保存
            SaveResult();
        }

        UpdateTimerText();
    }

    //キーボードから文字が入力されたとき
    void OnTextInput(char character)
    {
        //時間切れ・ポーズ中・画面遷移中は入力を受け付けない
        if(isTimeUp || isPaused || isTransitioning) return;

        //正解候補がない場合は入力を受け付けない
        if(possibleAnswers.Count == 0) return;

        //制御文字は入力として扱わない
        if(char.IsControl(character)) return;

        //キー入力音を再生（正解・ミス共通）
        if(AudioManager.Instance != null) AudioManager.Instance.PlayTypingSound();

        //小文字に変換
        string typedCharacter = character.ToString().ToLower();

        //正解候補を絞り込む
        List<string> newCandidates = new List<string>();

        foreach(string answer in possibleAnswers)
        {
            if(answer.StartsWith(inputBuffer + typedCharacter)) newCandidates.Add(answer);
        }

        //正解候補がなければミス
        if(newCandidates.Count == 0)
        {
            mistakeCount++;

            //ミス音を再生
            if(AudioManager.Instance != null) AudioManager.Instance.PlayMistakeSound();

            Debug.Log("ミス回数：" + mistakeCount);
            return;
        }

        //正しい入力
        inputBuffer += typedCharacter;
        possibleAnswers = newCandidates;

        //正解キー数を増やす
        correctKeyCount++;

        //入力表示を更新
        inputText.text = inputBuffer.ToUpper();

        //入力に合ったローマ字のお手本に更新
        romajiText.text = possibleAnswers[0].ToUpper();

        Debug.Log("正解タイプ数：" + correctKeyCount);
        Debug.Log("ミス回数：" + mistakeCount);

        //答えが完成したか確認
        foreach(string answer in possibleAnswers)
        {
            if(answer == inputBuffer)
            {
                Debug.Log("正解！");

                //正解音を再生
                if(AudioManager.Instance != null) AudioManager.Instance.PlayCorrectSound();

                ShowRandomQuestion();
                return;
            }
        }
    }

    //CSVから問題を読み込む
    void LoadQuestions()
    {
        if(questionCSV == null)
        {
            Debug.LogError("Question.csv が設定されていません");
            return;
        }

        string[] lines = questionCSV.text.Split('\n');

        for(int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(',');

            if(columns.Length < 6) continue;

            string question = columns[1].Trim();
            string furigana = columns[2].Trim();
            string difficulty = columns[4].Trim();
            string use = columns[5].Trim();

            //問題文・ふりがなが空欄なら読み込まない
            if(string.IsNullOrEmpty(question) ||
            string.IsNullOrEmpty(furigana))
            {
                Debug.LogWarning("問題文またはふりがなが空欄のため、読み込みをスキップします。");
                continue;
            }

            if (use != "はい") continue;

            if (difficulty != GetDifficultyName()) continue;

            QuestionData data = new QuestionData();

            data.question = question;
            data.furigana = furigana;

            questionList.Add(data);
        }

        Debug.Log("読み込んだ問題数：" + questionList.Count);
    }

    //難易度名を取得
    string GetDifficultyName()
    {
        switch(GameSettings.SelectedDifficulty)
        {
            case Difficulty.Easy:
                return "かんたん";

            case Difficulty.Normal:
                return "ふつう";

            case Difficulty.Hard:
                return "むずかしい";

            default: //誤操作防止
                return "かんたん";
        }
    }

    //ランダムに問題を表示
    void ShowRandomQuestion()
    {
        //問題が残っていない場合
        if(questionList.Count == 0)
        {
            Debug.LogError("表示できる問題がありません");

            questionText.text = "利用できる問題がありません";
            furiganaText.text = "";
            romajiText.text = "";
            inputText.text = "";

            possibleAnswers.Clear();
            return;
        }

        //変換できる問題が見つかるまで繰り返す
        while(questionList.Count > 0)
        {
            //表示する問題の番号を決める
            int randomIndex = Random.Range(0, questionList.Count);

            //問題が複数ある場合、前回と同じ問題を避ける
            if(questionList.Count > 1)
            {
                while(randomIndex == previousQuestionIndex) randomIndex = Random.Range(0, questionList.Count);
            }

            QuestionData selectedQuestion = questionList[randomIndex];

            currentFurigana = selectedQuestion.furigana;

            //入力状態と正解候補をリセット
            inputBuffer = "";
            possibleAnswers.Clear();

            //ローマ字候補を作る
            BuildAnswers(currentFurigana, 0, "");

            //有効な候補が作れなかった場合
            if(possibleAnswers.Count == 0)
            {
                Debug.LogWarning(
                    "ローマ字に変換できないため、問題をスキップします：" +
                    selectedQuestion.question
                    );

                //問題リストから除外
                questionList.RemoveAt(randomIndex);

                //前回の問題番号を調整
                if (previousQuestionIndex == randomIndex) previousQuestionIndex = -1;
                else if (previousQuestionIndex > randomIndex) previousQuestionIndex--;

                continue;
            }

            //有効な問題を表示
            previousQuestionIndex = randomIndex;

            questionText.text = selectedQuestion.question;
            furiganaText.text = selectedQuestion.furigana;

            //表示用ローマ字
            romajiText.text =
                romajiConverter.ConvertToRomaji(currentFurigana);

            //入力表示をリセット
            inputText.text = "";

            Debug.Log("今回の問題：" + selectedQuestion.question);

            return;
        }

        //すべての問題が変換できなかった場合
        Debug.LogError("変換できる問題がありません。");

        questionText.text = "利用できる問題がありません";
        furiganaText.text = "";
        romajiText.text = "";
        inputText.text = "";

        possibleAnswers.Clear();
        currentFurigana = "";
    }

    //すべてのローマ字候補を作る
    void BuildAnswers(string hiragana, int position, string currentAnswer)
    {
        //最後まで変換できたら候補に追加
        if(position >= hiragana.Length)
        {
            if(!possibleAnswers.Contains(currentAnswer)) possibleAnswers.Add(currentAnswer);

            return;
        }

        //現在位置のローマ字候補を取得
        int consumedLength;

        string[] candidates = romajiConverter.GetCandidates(
            hiragana,
            position,
            out consumedLength
        );

        if(candidates.Length == 0 || consumedLength == 0)
        {
            Debug.LogWarning("変換できないかな：" + hiragana[position]);
            return;
        }

        //それぞれの候補を組み合わせる
        foreach(string candidate in candidates)
        {
            BuildAnswers(
                hiragana,
                position + consumedLength,
                currentAnswer + candidate
            );
        }
    }

    //タイマー表示
    void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(remainingTime);

        timerText.text = seconds + "秒";
    }

    void SaveResult()
    {
        //正解タイプ数
        ResultData.CorrectKeyCount = correctKeyCount;

        //ミスタイプ数
        ResultData.MistakeCount = mistakeCount;

        //プレイ時間
        ResultData.PlayTime = elapsedPlayTime;
    }

    //ゲームを一時停止
    public void PauseGame()
    {
        //すでにポーズ中、または時間切れ・画面遷移中なら何もしない
        if(isPaused || isTimeUp || isTransitioning) return;

        isPaused = true;
        pausePanel.SetActive(true);
    }

    //ゲームを再開
    public void ResumeGame()
    {
        //ポーズ中でなければ何もしない
        if(!isPaused || isTransitioning) return;

        isPaused = false;
        pausePanel.SetActive(false);
    }

    //リザルト画面へ移動
    public void GoToResult()
    {
        //すでに画面遷移を開始していたら何もしない
        if(isTransitioning) return;

        isTransitioning = true;

        SaveResult();
        SceneManager.LoadScene(GameSettings.ResultScene);
    }

}