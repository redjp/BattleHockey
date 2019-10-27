using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private float maxSpeed;
    [SerializeField]
    private Vector2 maxSpeedAtXToY;
    [SerializeField]
    private float collisionForce;
    [SerializeField]
    private GameObject particle;
    [SerializeField]
    private float particleSpeed;
    [HideInInspector]
    public bool enableParticle;

    private float mag;
    private Vector3 collisionVelocity;
    private PlayerController playerController;
    private EnemyController enemyController;

    [Header("Attack")]
    public int attackSlowDamage;
    public int attackFastDamage;

    //サウンド関連
    private AudioManager audioManager;
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip se_ShootSlow;
    [SerializeField]
    private AudioClip se_ShootFast;
    [SerializeField]
    private AudioClip se_HitSlow;
    [SerializeField]
    private AudioClip se_HitFast;
    [SerializeField]
    private AudioClip se_WallSlow;
    [SerializeField]
    private AudioClip se_WallFast;
    [SerializeField]
    private AudioClip se_OutOfField;

    private bool isOutOfField;
    private GameObject parentObject;

    void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        maxSpeed = maxSpeedAtXToY.x;
    }

    void FixedUpdate()
    {
        mag = rb.velocity.magnitude;
        if (mag > maxSpeed)
            rb.velocity *= maxSpeed / mag;

        if (!enableParticle && mag >= particleSpeed)
        {
            particle.SetActive(true);
            enableParticle = !enableParticle;
        }
        else if (enableParticle && mag < particleSpeed)
        {
            particle.SetActive(false);
            enableParticle = !enableParticle;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                playerController = collision.gameObject.GetComponent<PlayerController>();
                collisionVelocity = playerController.currentVelocity * collisionForce;

                //ヒット音（プレイヤー）
                if (collisionVelocity.magnitude >= 3.0f)
                {
                    PlayAudioClip(se_ShootFast);
                    maxSpeed = maxSpeedAtXToY.y;
                }
                else
                {
                    PlayAudioClip(se_ShootSlow);
                    maxSpeed = maxSpeedAtXToY.x;
                }
                break;
            case "Enemy":
                //敵がダメージを受けたときの関数呼び出し
                enemyController = collision.gameObject.GetComponent<EnemyController>();

                //ヒット音（敵）
                if (enableParticle)
                {
                    enemyController.DamageEnemy(attackFastDamage, Random.Range(2, 5));
                }
                else
                {
                    enemyController.DamageEnemy(attackSlowDamage, 1);
                }
                break;
            case "Ball":
                //ヒット音（壁）
                if (enableParticle)
                {
                    PlayAudioClip(se_HitFast);
                }
                else
                {
                    PlayAudioClip(se_HitSlow);
                }
                break;
            case "Sidebar":
                //ヒット音（壁）
                if (enableParticle)
                {
                    PlayAudioClip(se_WallFast);
                }
                else
                {
                    PlayAudioClip(se_WallSlow);
                }
                break;
        }
    }

    IEnumerator OnTriggerEnter(Collider collider)
    {
        switch (collider.gameObject.tag)
        {
            case "OutOfField":
                //ダメージを受ける
                GameManager.playerLife--;

                audioSource.PlayOneShot(se_OutOfField);
                yield return new WaitForSeconds(1.0f);

                //新しいボールを出す
                parentObject.GetComponent<EnemyController>().newBallFlag = true;
                this.gameObject.SetActive(false);
                break;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            rb.velocity = rb.velocity * 0.75f + collisionVelocity;
        }
    }

    public void SetParent(GameObject gameObject)
    {
        parentObject = gameObject;
    }

    void PlayAudioClip(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, audioManager.seVolume);
        }
    }
}
