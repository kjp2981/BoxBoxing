using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using FD.Dev;

public class PlayerManagement : MonoBehaviour
{

    [SerializeField] public PlayerState playerState;
    [SerializeField] private PlayerInput input;
    [SerializeField] private UnityEvent dieEvent;
    [SerializeField] private ShieldUp up;
    [SerializeField] private bool isGod;

    public Vector2 size;
    public Vector2 pos;
    public LayerMask mask;
    public bool godMode;
    private CinemachineBasicMultiChannelPerlin channelPerlin;
    private int HitCount = 4;

    private bool isDead = false;
    private bool isLow = false;
    private bool attackAble;
    private int EnemyHp = 3;
    [HideInInspector] public bool isFuckingDie;

    private void Awake()
    {
        
        channelPerlin = FindObjectOfType<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if(playerState == null)
        {

            Debug.Log($"{name} PlayerState is null!");

        }
        if(input == null)
        {

            Debug.LogError($"{name} PlayerInput is null!");

        }

    }

    public void Die()
    {

        isDead = true;
        dieEvent?.Invoke();
        playerState.SetState(Define.PlayerStates.Die);

    }

    public void Attack()
    {
        if (isDead == true) return;

        FAED.InvokeDelay(() =>
        {

            Debug.Log("��!���");

            Collider2D col = Physics2D.OverlapBox(transform.position + (Vector3)pos, size, 0, mask);

            if (isGod && col != null)
            {

                PlayerManagement manager = col.GetComponent<PlayerManagement>();
                manager.Hit();

            }

            if (col != null && col.gameObject.name != "Enemy4")
            {

                PlayerManagement mamange = col.GetComponent<PlayerManagement>();
                mamange.Hit();

            }
            else if(col != null && col.gameObject.name == "Enemy4")
            {

                if(col.GetComponent<PlayerManagement>().playerState.currentState != Define.PlayerStates.Guard && col.GetComponent<PlayerManagement>().playerState.currentState != Define.PlayerStates.Sit) EnemyHp--;

                if(EnemyHp <= 0)
                {

                    PlayerManagement mamange = col.GetComponent<PlayerManagement>();
                    mamange.Hit();

                }

                StartCoroutine(CameraShakeCo());

            }

        }, 0.2f);



    }

    public void SetGuard()
    {

        if (isFuckingDie) 
        {

            playerState.SetIdle();
            return;
        
        }

        up.Shield(HitCount, isLow);

    }

    public void DeGuard(bool isBreak = false)
    {

        if(isBreak == true)
        {

            isLow = true;

        }
        if (isLow)
        {


            HitCount = 2;

        }

        up.DeShield(isBreak);
    }

    public void Hit()
    {

        Debug.Log(playerState.currentState);

        if(playerState.currentState != Define.PlayerStates.Guard && !godMode)
        {

            Die();
            
        }
        else if(godMode)
        {

            if (attackAble) return;

            attackAble = true;
                FAED.Pop("MissFX", transform.position, Quaternion.identity);

            FAED.InvokeDelay(() =>
            {

                attackAble = false;

            }, 0.1f);

        }
        else
        {

            if(isFuckingDie) return;

            SoundManager.instance.SFXPlay(Random.Range(1, 6));

            HitCount--;
            if(HitCount > 0)
            {

                FAED.Pop("GuardFX", transform.position, Quaternion.identity);

                up.Shield(HitCount, isLow);

                if(isLow == true && HitCount == 0)
                {

                    isFuckingDie = true;

                }

            }
            else
            {

                if (isLow)
                {

                    isFuckingDie = true;

                }

                DeGuard(true);
            }
            StartCoroutine(CameraShakeCo());

        }
    }

    IEnumerator CameraShakeCo()
    {

        channelPerlin.m_AmplitudeGain = 4f;
        channelPerlin.m_FrequencyGain = 4f;
        yield return new WaitForSecondsRealtime(0.1f);
        channelPerlin.m_AmplitudeGain = 0f;
        channelPerlin.m_FrequencyGain = 0f;

    }

    private void OnDisable()
    {
        isDead = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + (Vector3)pos, size);
        Gizmos.color = Color.white;
    }
#endif
}
