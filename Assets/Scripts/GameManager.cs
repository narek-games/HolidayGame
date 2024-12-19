using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using unityroom.Api;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    // Canvasの表示を管理する配列
    public GameObject[] canvas;

    //制限時間のカウントダウン
    public float limitTime = 90.0f;

    //時間を表示するText型の変数
    public TextMeshProUGUI timeText;

    // スタート前のカウントダウン
    public float countDown = 2.0f;

    // スタート前のカウントダウンを表示するText型の変数
    public TextMeshProUGUI countDownText;

    // スコアを表示するText型の変数
    public TextMeshProUGUI scoreText;

    // 結果スコアを表示するText型の変数
    public TextMeshProUGUI resultScoreText;

    // メッセージを表示するText型の変数
    public TextMeshProUGUI resultMessage;

    // スコアの変数
    public int score = 0;

    // 表示するholidayマークの配列
    public GameObject[] holidayMark;

    // 最初に生成されるholidayの数(固定や範囲指定をするときは生成数を表す変数を用意する)
    //public int startHolidayCount = 4;

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

    // ゲームスタートのフラグ
    public bool gameStartFlag = false;

    // 1週間そろった時のSEの変数
    public AudioClip holidaySE;

    // 正しく入力できた時のSEの変数
    public AudioClip correctSE;

    // ゲームが終わった時のSEの変数
    public AudioClip resultSE;

    // SEを流すときに使う
    AudioSource audioSource;

    void Start()
    {
        // タイトル画面だけを表示
        canvas[0].SetActive(true);
        canvas[1].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);

        // SEの初期化
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && gameStartFlag == false){

            // カウントダウンを表示/タイトルとゲーム画面とリザルトを非表示
            canvas[0].SetActive(false);
            canvas[1].SetActive(false);
            canvas[2].SetActive(false);
            canvas[3].SetActive(true);

            // カウントダウン初期化
            countDown = 2.0f;

            // マーク初期化
            for (int i = 0; i < holidayMark.Length; i++)
            {
                holidayMark[i].SetActive(false);
            }

            // スコア初期化
            score = 0;

            // 制限時間の初期化
            limitTime = 90.0f;

            // holidayマークを生成する処理
            GenerateHoliday();
            // アルファベットを表示
            GenerateAlphabet();

            // ゲームスタートフラグをtrueに
            gameStartFlag = true;

        }

        if(gameStartFlag == true)
        {
            // カウントダウンを表示
            CountDown();

            //時間を表示する
            countDownText.text = (countDown + 1.0f).ToString("f0");

        }

        if(countDown <= 0)
        {
            // 制限時間を表示
            LimitTime();

            // スコアテキスト更新
            scoreText.text = score.ToString() + "週間";

            //時間を表示する
            timeText.text = limitTime.ToString("f1") + "秒";
        }

        // スペースキー(タイトルに戻る)入力時
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // タイトル画面だけを表示
            canvas[0].SetActive(true);
            canvas[1].SetActive(false);
            canvas[2].SetActive(false);
            canvas[3].SetActive(false);

            // ゲームスタートフラグをfalseに
            gameStartFlag = false;
        }

        // 各対応キー入力時
        if (Input.anyKeyDown)
        {
            string keyStr = Input.inputString;
            char keyChar = keyStr.ToCharArray()[0];

            for (int i = 0; i < weekAlphabet.Length; i++) 
            {
                if(weekAlphabet[i] == keyChar && week[i] == 0)
                {
                    week[i] = 1;
                    holidayMark[i].SetActive(true);
                    holidayCount++;
                    // SEを鳴らす
                    audioSource.PlayOneShot(correctSE);

                    break;
                }
            }

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
    }

    // カウントダウンの関数
    void CountDown()
    {

        // 時間をカウントダウンする
        countDown -= Time.deltaTime;

        // 時間切れになったら
        if(countDown <= 0)
        {
            // ゲームを表示/タイトルとカウントダウンとリザルトを非表示
            canvas[0].SetActive(false);
            canvas[1].SetActive(true);
            canvas[2].SetActive(false);
            canvas[3].SetActive(false);

        }
    }

    // 制限時間の関数
    void LimitTime()
    {
        //時間をカウントダウンする
        limitTime -= Time.deltaTime;

        // 時間切れになったら
        if (limitTime <= 0)
        {
            // リザルト画面だけを表示
            canvas[0].SetActive(false);
            canvas[1].SetActive(false);
            canvas[2].SetActive(true);
            canvas[3].SetActive(false);

            // SEを鳴らす
            audioSource.PlayOneShot(resultSE);

            // 結果スコアを表示
            resultScoreText.text = score.ToString() + "週間";

            // ボードNo1にスコアを送信する。
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

            // ゲームスタートフラグをfalseに
            gameStartFlag = false;
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

        // holidayマークをつけるループ
        for (int i = 0; i < week.Length; i++)
        {
            if (!(holidayCount == 6))
            {
                week[i] = Random.Range(0, 2);
                if (week[i] == 1)
                {
                    holidayMark[i].SetActive(true);
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
