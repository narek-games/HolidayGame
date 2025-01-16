using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using unityroom.Api;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Canvasの表示を管理する配列(0->タイトル、1->ゲーム、2->リザルト、3->カウントダウン)
    public GameObject[] canvas;

    // タイトルの"Holiday"の表示を管理する配列
    public GameObject[] titleHoliday;

    // タイトルの"Holiday"が入力済みかを文字ごとに判断するためのフラグ配列
    public int[] titleHolidayFlag = new int[] { 0, 0, 0, 0, 0, 0, 0 };

    // 隠し難易度に遷移するためのカウント(7になったら遷移)
    public int titleHolidayCount = 0;

    // 制限時間のカウント
    //カウントダウン
    public float countdown = 90.0f;

    //時間を表示するText型の変数
    public TextMeshProUGUI timeText;

    // ゲームスタート前のカウントダウン
    public float startCountdown = 3.0f;

    // ↑を表示するText型の変数
    public TextMeshProUGUI scText;

    // スコアを表示するText型の変数
    public TextMeshProUGUI scoreText;

    // 現在の難易度を表示するText型の変数
    public TextMeshProUGUI levelText;

    // 結果スコアを表示するText型の変数
    public TextMeshProUGUI resultScoreText;

    // メッセージを表示するText型の変数
    public TextMeshProUGUI resultMessage;

    // スコアの変数
    public int score = 0;

    // 表示するholidayマークの配列
    public GameObject[] holidayMark;

    // 1週間の配列を生成(holiday判定　0:weekday　1:holiday)
    public int[] week = new int[] { 0, 0, 0, 0, 0, 0, 0 };

    // 現在のholidayの数
    public int holidayCount = 0;

    // アルファベット配列
    private char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    // 1週間のアルファベットを代入する配列
    public char[] weekAlphabet = new char[7];

    // アルファベット表示用の配列
    public TMP_Text[] alphabetText;

    // 隠し難易度との表示差別化のための配列
    public GameObject[] AlphabetSetActive;

    // ゲーム状態のフラグ
    public int gameStateFlag = 0;

    // 難易度を判別する関数(0->簡単、1->1普通、2->難しい、3->隠し難易度)
    public int level = 0;

    // 1週間そろった時のSEの変数
    public AudioClip holidaySE;

    // 正しく入力できた時のSEの変数
    public AudioClip correctSE;

    // ゲームが終わった時のSEの変数
    public AudioClip resultSE;

    // SEを流すときに使う
    AudioSource audioSource;

    // eventSystem型の変数を宣言(アルファベットボタン用)
    [SerializeField] private EventSystem eventSystem;

    // GameObject型の変数を宣言(ボタンオブジェクトを入れる箱)
    private GameObject button_ob;

    // GameObject型の変数を宣言(テキストオブジェクトを入れる箱)
    private GameObject NameText_ob;

    // Text型の変数を宣言(テキストコンポーネントを入れる箱)
    private TMP_Text name_text;

    // keyCharを大域変数で宣言(関数化の都合)
    public char keyChar = ' ';

    void Start()
    {
        // タイトル画面だけを表示
        canvas[0].SetActive(true);
        canvas[1].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);

        // 隠しコマンド表示の"Holiday"を隠す
        for(int i = 0; i < titleHoliday.Length; i++)
        {
            titleHoliday[i].SetActive(false);
        }

        // SEの初期化
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(titleHolidayCount == 7)
        {
            level = 3;
            GameStart();
        }

        if (gameStateFlag == 3)
        {
            // カウントダウン
            StartCountdown();
        }

        if (gameStateFlag == 1)
        {
            // 制限時間を表示
            LimitTime();

            // スコアテキスト更新
            scoreText.text = score.ToString() + "週間";

            // 現在の難易度を表示
            if (level == 0)
            {
                levelText.text = "Easy";
            }
            else if (level == 1)
            {
                levelText.text = "Normal";
            }
            else if (level == 2)
            {
                levelText.text = "Difficult";
            }

            //時間を表示する
            timeText.text = countdown.ToString("f1") + "秒";
        }

        // スペースキー(リトライ)入力時
        if (Input.GetKeyDown(KeyCode.Space) && gameStateFlag == 2)
        {
            GameStart();
        }

        // タブキー(タイトルに戻る)入力時
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            BackTitle();
        }

        // 各対応キー入力時
        if (Input.anyKeyDown && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            string keyStr = Input.inputString;
            keyChar = keyStr.ToCharArray()[0];

            if(level < 3 && gameStateFlag == 1)
            {
                checkInputAlphabet();
            }

            if(gameStateFlag == 0)
            {
                checkHiddenHoliday();
            }
        }
    }

    // 関数-------------------------------------------------------------------------------------

    // ゲームスタート(カウントダウン遷移)の関数
    public void GameStart()
    {
        // カウントダウンのみ表示
        canvas[0].SetActive(false);
        canvas[1].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(true);

        // マーク初期化
        for (int i = 0; i < holidayMark.Length; i++)
        {
            holidayMark[i].SetActive(false);
        }

        // スコア初期化
        score = 0;

        // 制限時間の初期化
        countdown = 90.0f;

        // カウントダウンの初期化
        startCountdown = 3.0f;

        // カウントダウンの開始
        StartCountdown();

        if(level < 3)    // 隠し難易度以外なら
        {
            // アルファベットを表示/曲名を非表示
            AlphabetSetActive[0].SetActive(true);
            AlphabetSetActive[1].SetActive(false);
            // holidayマークを生成する処理
            GenerateHoliday();
            // アルファベットを表示
            GenerateAlphabet();
        }
        else if(level == 3)   // 隠し難易度なら
        {
            AlphabetSetActive[0].SetActive(false);
            AlphabetSetActive[1].SetActive(true);
        }

        // ゲーム状態のフラグを3(カウントダウン)に
        gameStateFlag = 3;
    }

    // 難易度を「簡単」に切り替える関数
    public void SwitchEasy()
    {
        level = 0;
    }

    // 難易度を「普通」に切り替える関数
    public void SwitchNormal()
    {
        level = 1;
    }

    // 難易度を「難しい」に切り替える関数
    public void SwitchDifficult()
    {
        level = 2;
    }

    // アルファベットボタンを押したときの関数
    public void OnAlphabet(Button sender)
    {
        if(gameStateFlag == 1)
        {
            // ボタンの子のテキストオブジェクトを名前指定で取得
            NameText_ob = sender.transform.Find("Text (TMP)").gameObject;

            // テキストオブジェクトのテキストを取得
            name_text = NameText_ob.GetComponent<TMP_Text>();

            string keyStr = name_text.text.ToString();
            keyChar = keyStr.ToCharArray()[0];

            if(level < 3)
            {
                checkInputAlphabet();
            }
        }

        if(gameStateFlag == 0)
        {
            // ボタンの子のテキストオブジェクトを名前指定で取得
            NameText_ob = sender.transform.Find("Text (TMP)").gameObject;

            // テキストオブジェクトのテキストを取得
            name_text = NameText_ob.GetComponent<TMP_Text>();

            string keyStr = name_text.text.ToString();
            char keyChar = keyStr.ToCharArray()[0];

            checkHiddenHoliday();
        }
    }

    // 隠しコマンド入力判定処理
    public void checkHiddenHoliday()
    {
        if (keyChar == 'h' && titleHolidayFlag[0] == 0)
        {
            titleHoliday[0].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'o' && titleHolidayFlag[1] == 0)
        {
            titleHoliday[1].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'l' && titleHolidayFlag[2] == 0)
        {
            titleHoliday[2].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'i' && titleHolidayFlag[3] == 0)
        {
            titleHoliday[3].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'd' && titleHolidayFlag[4] == 0)
        {
            titleHoliday[4].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'a' && titleHolidayFlag[5] == 0)
        {
            titleHoliday[5].SetActive(true);
            titleHolidayCount++;
        }
        else if (keyChar == 'y' && titleHolidayFlag[6] == 0)
        {
            titleHoliday[6].SetActive(true);
            titleHolidayCount++;
        }
    }

    // 入力したアルファベットの正誤判定処理
    public void checkInputAlphabet()
    {
        for (int i = 0; i < weekAlphabet.Length; i++)
        {
            if (weekAlphabet[i] == keyChar && week[i] == 0)
            {
                week[i] = 1;
                holidayMark[i].SetActive(true);
                holidayCount++;
                // SEを鳴らす
                audioSource.PlayOneShot(correctSE);

                break;
            }
        }

        NextWeek();
    }

    // 一週間そろった時の処理
    public void NextWeek()
    {
        if (holidayCount == 7)
        {
            // holidayマークを生成する処理
            GenerateHoliday();
            // アルファベットを表示
            GenerateAlphabet();
            // SEを鳴らす
            audioSource.PlayOneShot(holidaySE);
            // スコアを加算
            score++;
        }
    }

    // タイトル画面に遷移する関数
    public void BackTitle()
    {
        // タイトル画面だけを表示
        canvas[0].SetActive(true);
        canvas[1].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);

        // ゲーム状態フラグを0に
        gameStateFlag = 0;
    }

    // ゲームスタート前のカウントダウンの関数
    void StartCountdown()
    {
        // 隠しコマンド表示の"Holiday"を隠す
        for (int i = 0; i < titleHoliday.Length; i++)
        {
            titleHoliday[i].SetActive(false);
        }

        // titleHolidayFlagの初期化
        for (int i = 0; i < titleHolidayFlag.Length; i++)
        {
            titleHolidayFlag[i] = 0;
        }

        // titleHolidayCountの初期化
        titleHolidayCount = 0;

        //カウントダウンする
        startCountdown -= Time.deltaTime;

        float displaySC = Mathf.Ceil(startCountdown);
        //時間を表示する
        scText.text = displaySC.ToString("f0");

        // カウントが0になったら
        if (startCountdown <= 0)
        {
            // ゲームを表示/カウントダウンを非表示
            canvas[1].SetActive(true);
            canvas[3].SetActive(false);

            // ゲーム状態フラグを1(ゲーム)に
            gameStateFlag = 1;
        }
    }

    // 制限時間の関数
    void LimitTime()
    {
        //時間をカウントダウンする
        countdown -= Time.deltaTime;

        // 時間切れになったら
        if (countdown <= 0)
        {
            // リザルト画面だけを表示
            canvas[1].SetActive(false);
            canvas[2].SetActive(true);

            // SEを鳴らす
            audioSource.PlayOneShot(resultSE);

            // 結果スコアを表示
            resultScoreText.text = score.ToString() + "週間";

            // ボードNo1にスコア123.45fを送信する。
            UnityroomApiClient.Instance.SendScore(1, score, ScoreboardWriteMode.HighScoreDesc);

            // メッセージ判定
            if (score < 10)
            {
                resultMessage.text = "1限目に遅刻だわ";
            }
            else if(score < 20)
            {
                resultMessage.text = "いつもより背伸びをしたけどこんな日も悪くないかも";
            }
            else if(score < 30)
            {
                resultMessage.text = "たまにはちょっと冒険してみない?私たちと";
            }
            else if(score < 40)
            {
                resultMessage.text = "この眼はいつも君を追いかけてる";
            }
            else if(score < 52)
            {
                resultMessage.text = "キラキラ輝いてるそれは未来";
            }
            else if(score < 70)
            {
                resultMessage.text = "365日全部毎日がHoliday";
            }
            else if(score < 90)
            {
                resultMessage.text = "みんなのことを花咲かせちゃいます！";
            }
            else
            {
                resultMessage.text = "あたし、センパイと残陽したんだっ！";
            }

            // ゲームスタートフラグを2(リザルト)に
            gameStateFlag = 2;
        }
    }

    // ランダムにHolidayをつける
    void GenerateHoliday()
    {
        // 現在のholidayの数の初期化
        holidayCount = 0;

        // マークの初期化
        for (int i = 0; i < holidayMark.Length; i++)
        {
            holidayMark[i].SetActive(false);
        }

        // 配列の初期化(すべて0に)
        for(int i = 0; i < week.Length; i++)
        {
            week[i] = 0;
        }

        // holidayの初期数を入れる変数の初期化
        int　startHoliday = 0;

        // holidayの初期数を決定
        if (level == 0)
        {
            startHoliday = Random.Range(4, 7);
        }
        else if (level == 1)
        {
            startHoliday = Random.Range(2, 6);
        }
        else if(level == 2)
        {
            startHoliday = Random.Range(0, 3);
        }


        // holidayマークをつけるループ
        for (int i = 0; i < week.Length; i++)
        {
            if (startHoliday > 0)
            {
                if(i < 7 - startHoliday)
                {
                    week[i] = Random.Range(0, 2);
                    if (week[i] == 1)
                    {
                        holidayMark[i].SetActive(true);
                        startHoliday--;
                        holidayCount++;
                    }
                }
                else if(i == 7 - startHoliday)
                {
                    week[i] = 1;
                    holidayMark[i].SetActive(true);
                    startHoliday--;
                    holidayCount++;
                }
                
            }       
            
        }
    }

    // ランダムにアルファベットを生成
    void GenerateAlphabet()
    {
        // マークの初期化
        for (int i = 0; i < weekAlphabet.Length; i++)
        {
            weekAlphabet[i] = '0';
        }

        // アルファベットを重複なしで格納して表示
        for (int i = 0; i < weekAlphabet.Length; i++)
        {
            weekAlphabet[i] = alphabet[Random.Range(0, 26)];

            for(int j = 0; j < i; j++)
            {
                if (weekAlphabet[i] == weekAlphabet[j])
                {
                    i--;
                }
            }

            alphabetText[i].text = weekAlphabet[i].ToString();
        }
    }
}
