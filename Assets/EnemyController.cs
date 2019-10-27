using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private GameObject model;
    [SerializeField]
    private GameObject particle;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private CapsuleCollider enemyCollider;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private int maxHP;
    private int currentHP;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float walkSpeed;

    [Header("Ball")]
    [SerializeField]
    private GameObject ballPrefab;
    private GameObject ballObject;
    [SerializeField]
    private float ballHeight;

    [Header("Init")]
    [SerializeField]
    private float animSpeedMax;
    [SerializeField]
    private float initThrowSpeed;

    //サウンド関連
    private AudioManager audioManager;
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip se_DamageSlow;
    [SerializeField]
    private AudioClip se_DamageFast;
    [SerializeField]
    private AudioClip se_Reflect;
    [SerializeField]
    private AudioClip se_Throw;
    [SerializeField]
    private AudioClip se_Death;

    //状態
    [HideInInspector]
    public bool newBallFlag;
    private float stayTimer;
    private bool isInit;
    private bool isThrowing;

    private Vector3 targetPos;

    void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();

        ballObject = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        ballObject.GetComponent<BallController>().SetParent(this.gameObject);
        ballObject.SetActive(false);

        currentHP = 0;
        stayTimer = 0.0f;
        newBallFlag = false;
        isThrowing = false;
    }

    public void RePop(Vector3 pos)
    {
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        model.transform.localRotation = Quaternion.identity;
        currentHP = maxHP;

        stayTimer = 0.0f;
        newBallFlag = false;
        isThrowing = false;
        animator.SetBool("Moving", false);

        model.SetActive(true);
        particle.SetActive(true);
        StartCoroutine(InitCroutine());
    }

    public void DePop()
    {
        ballObject.SetActive(false);
    }

    //登場処理
    IEnumerator InitCroutine()
    {
        if (isInit) { yield break; }
        isInit = true;

        enemyCollider.enabled = false;
        var initAnimation = StartCoroutine(InitAnimation());
        yield return initAnimation;
        enemyCollider.enabled = true;

        //ボールが場になければボールを投げる
        Coroutine throwNewBall;
        if (!ballObject.activeSelf)
        {
            newBallFlag = false;
            throwNewBall = StartCoroutine(ThrowNewBall(initThrowSpeed));
            yield return throwNewBall;
        }

        targetPos = transform.position;

        isInit = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInit && !isThrowing && currentHP > 0)
        {
            stayTimer -= Time.deltaTime;

            if (newBallFlag)
            {
                newBallFlag = false;
                StartCoroutine(ThrowNewBall());
            }
            else if (stayTimer <= 0.0f)
            {
                //ランダム移動
                RandomMove();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            SetNextTargetPos();
        }
    }

    //登場シーン
    IEnumerator InitAnimation()
    {
        Vector3 vec;
        //ゆっくりと0.5fの高さに移動
        while (true)
        {
            if (transform.position.y == 0.5f) break;

            vec = transform.position;
            vec.y += Mathf.Clamp(0.5f - vec.y, -animSpeedMax, animSpeedMax);
            transform.position = vec;

            yield return null;
        }
    }

    //ランダム移動
    void RandomMove()
    {
        if (Vector3.Distance(targetPos, transform.position) > moveSpeed)
        {
            var vec = (targetPos - transform.position).normalized;
            rb.MovePosition(transform.position + vec * moveSpeed);
            model.transform.rotation = Quaternion.LookRotation(vec);

            animator.SetFloat("Velocity X", vec.x * walkSpeed);
            animator.SetFloat("Velocity Z", vec.z * walkSpeed);
            animator.SetBool("Moving", true);
        }
        else
        {
            //次の目的地を設定
            SetNextTargetPos();
            stayTimer = 0.1f;
            animator.SetBool("Moving", false);
        }
    }

    void SetNextTargetPos()
    {
        var pos = transform.position;
        pos.x = Random.Range(-0.2f, -3.2f);
        pos.z = Random.Range(-1.5f * GameManager.fieldScale.z, 1.5f * GameManager.fieldScale.z);
        targetPos = pos;
    }

    //ダメージを受けた時の処理（BallControllerから呼び出し）
    public void DamageEnemy(int damage, int hitMotion)
    {
        if (isThrowing)
        {
            //反射音
            PlayAudioClip(se_Reflect);
        }
        else
        {
            //硬直時間
            stayTimer = 0.4f;

            currentHP -= damage;
            //音を鳴らす
            if (hitMotion == 1)
                PlayAudioClip(se_DamageSlow);
            else
                PlayAudioClip(se_DamageFast);

            //死亡判定
            if (currentHP <= 0)
            {
                StartCoroutine(DeadEnemy());
            }
            else
            {
                switch (hitMotion)
                {
                    case 1:
                        animator.SetBool("GetHit1Trigger", true);
                        break;
                    case 2:
                        animator.SetBool("GetHit2Trigger", true);
                        break;
                    case 3:
                        animator.SetBool("GetHit3Trigger", true);
                        break;
                    case 4:
                        animator.SetBool("GetHit4Trigger", true);
                        break;
                    case 5:
                        animator.SetBool("GetHit5Trigger", true);
                        break;
                }
            }
        }
    }

    //死んだ時の処理
    IEnumerator DeadEnemy()
    {
        enemyCollider.enabled = false;
        particle.SetActive(false);
        animator.SetBool("Death1Trigger", true);
        yield return new WaitForSeconds(0.7f);
        PlayAudioClip(se_Death);
        yield return new WaitForSeconds(0.4f);
        model.SetActive(false);
        GameManager.enemyCount--;
    }

    public IEnumerator ThrowNewBall(float throwSpeed = 0.0f)
    {
        if (isThrowing) { yield break; }
        isThrowing = true;

        ballObject.SetActive(false);

        var pos = transform.position;
        pos.y = ballHeight;
        ballObject.transform.position = pos;

        var collider = ballObject.GetComponent<CapsuleCollider>();
        collider.enabled = false;

        var rb = ballObject.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        var originalRotation = model.transform.localRotation;
        //ランダムに向きと速度を変える
        model.transform.rotation = Quaternion.Euler(0.0f, Random.Range(45.0f, 135.0f), 0.0f);
        if (throwSpeed <= 0.0f)
            throwSpeed = Random.Range(3.0f, 10.0f);

        if (Random.value < 0.50f)
            animator.SetBool("Attack3Trigger", true);
        else
            animator.SetBool("Attack6Trigger", true);

        yield return new WaitForSeconds(0.1f);

        ballObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        rb.velocity = model.transform.forward * throwSpeed;
        PlayAudioClip(se_Throw);
        yield return new WaitForSeconds(1 / throwSpeed);

        collider.enabled = true;
        yield return new WaitForSeconds(0.4f);

        model.transform.localRotation = originalRotation;

        isThrowing = false;
    }

    void PlayAudioClip(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, audioManager.seVolume);
        }
    }
}
