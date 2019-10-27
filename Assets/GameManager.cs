using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static Vector3 fieldScale = new Vector3(1.0f, 1.0f, 1.0f);
    public static Vector3 bottomScale = new Vector3(0.5f, 0.5f, 2.0f);
    [Header("Field Option")]
    [Range(1.0f, 2.5f)]
    [SerializeField]
    private float targetFieldScaleZ;
    [Range(0.5f, 2.0f)]
    [SerializeField]
    private float targetBottomScaleZ;
    [SerializeField]
    private float animSpeedMax;
    private Vector3 vec;

    public static int enemyCount = 0;
    public static int playerLife = 0;

    private int difficulty = 0;
    [SerializeField]
    private GameObject enemyPrefab;
    private Vector3[,] enemyPosArray = {
        {new Vector3(-2.0f, -0.8f, 0.0f), Vector3.zero, Vector3.zero},
        {new Vector3(-1.8f, -1.0f, -0.9f), new Vector3(-1.8f, -0.8f, 0.9f), Vector3.zero},
        {new Vector3(-0.6f, -0.8f, -2.8f), new Vector3(-0.6f, -0.9f, 2.8f), new Vector3(-2.5f, -1.2f, 0.0f)}
    };
    private GameObject[] enemyObjects;

    [SerializeField]
    private GameObject titleCanvas;
    [SerializeField]
    private GameObject replayCanvas;
    [SerializeField]
    private GameObject clearCanvas;
    [SerializeField]
    private GameObject gameCanvas;
    [SerializeField]
    private TextMeshProUGUI lifeText;
    private bool isInGame = false;

    //サウンド関連
    private AudioManager audioManager;
    [Header("audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip se_Start;
    [SerializeField]
    private AudioClip se_Waah;
    [SerializeField]
    private AudioClip se_Finish;

    // Use this for initialization
    void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        enemyObjects = new GameObject[enemyPosArray.GetLength(1)];
        for (int i = 0; i < enemyPosArray.GetLength(1); i++)
        {
            enemyObjects[i] = Instantiate(enemyPrefab, Vector3.down * 2, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isInGame)
        {
            if (playerLife <= 0)
            {
                //負け処理
                for (int i = 0; i < enemyPosArray.GetLength(1); i++)
                {
                    enemyObjects[i].GetComponent<EnemyController>().DePop();
                }
                PlayAudioClip(se_Finish);

                gameCanvas.SetActive(false);
                replayCanvas.SetActive(true);
                targetBottomScaleZ = 2.0f;
                isInGame = false;
            }
            else if (enemyCount <= 0)
            {
                difficulty++;
                switch (difficulty)
                {
                    case 1:
                        targetFieldScaleZ = 1.0f;
                        targetBottomScaleZ = 1.4f;
                        PopNewEnemies(0);
                        break;
                    case 2:
                        targetFieldScaleZ = 1.5f;
                        targetBottomScaleZ = 1.3f;
                        PopNewEnemies(1);
                        break;
                    case 3:
                        targetFieldScaleZ = 2.0f;
                        targetBottomScaleZ = 1.2f;
                        PopNewEnemies(2);
                        break;
                    case 4:
                        //クリア処理
                        PlayAudioClip(se_Waah);
                        PlayAudioClip(se_Finish);

                        gameCanvas.SetActive(false);
                        clearCanvas.SetActive(true);
                        replayCanvas.SetActive(true);
                        targetBottomScaleZ = 2.0f;
                        isInGame = false;
                        break;
                }
            }

            lifeText.text = playerLife.ToString();
        }
    }

    void PopNewEnemies(int i)
    {
        for (int j = 0; j < enemyPosArray.GetLength(1); j++)
        {
            if (enemyPosArray[i, j] != Vector3.zero)
            {
                enemyObjects[j].GetComponent<EnemyController>().RePop(enemyPosArray[i, j]);
                enemyCount++;
            }
            else
            {
                enemyObjects[j].transform.position = Vector3.down * 2;
            }
        }
    }

    void FixedUpdate()
    {
        vec = fieldScale;
        vec.z += Mathf.Clamp(targetFieldScaleZ - vec.z, -animSpeedMax, animSpeedMax);
        fieldScale = vec;

        vec = bottomScale;
        vec.z += Mathf.Clamp(targetBottomScaleZ - vec.z, -animSpeedMax, animSpeedMax);
        bottomScale = vec;
    }

    public void OnStartButton()
    {
        titleCanvas.SetActive(false);
        InitGame();
    }

    public void OnReplayButton()
    {
        clearCanvas.SetActive(false);
        replayCanvas.SetActive(false);
        InitGame();
    }

    //ゲームの初期化
    void InitGame()
    {
        playerLife = 5;
        difficulty = 0;
        enemyCount = 0;

        PlayAudioClip(se_Start);
        gameCanvas.SetActive(true);
        isInGame = true;
    }

    void PlayAudioClip(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, audioManager.seVolume);
        }
    }
}
