using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hands : Enemy
{
    public float triggerRange = 3f;
    public float attackRadius = 1f;
    public float turnSpeed = 4f;

    RaycastHit hit;
    public GameObject Hand;
    public bool ignoreHit = false;
    int layer_mask;
    int walls;
    Vector3 targetDir;
    bool playerGrabbed = false;
    public Transform grabPos;
    public Animator anim;
    string[] s;
    // Start is called before the first frame update
    void Start()
    {
        layer_mask = LayerMask.GetMask("Player");
        walls = LayerMask.GetMask("Default");
        targetDir = Vector3.zero;
        hit = new RaycastHit();
        s = new string[] { "hand_raise", "hand_idle", "hand_grab", "hand_grabdown", "hand_fall" };
    }

    void Update()
    {
        if (TimelineManager.cutscenePlaying)
            return;
        if (!playerGrabbed)
        {
            CheckState();
            if (state == EnemyState.ATTACK)
            {
                Attack();
            }
            else
            {
                Rotate();
            }
        }
        else if(!PlayerManager.ptr.isDead)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(s[3]) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                PlayerManager.ptr.isDead = true;
            }
            PlayerManager.ptr.transform.position = grabPos.position;
        }
    }

    public void PlayerEscaped()
    {
        playerGrabbed = false;
        anim.SetInteger("state", 1);
    }

    void Rotate(float turnMult = 1f)
    {
        targetDir = (PlayerManager.ptr.transform.position - transform.position).normalized;
        targetDir.y = 0;
        Hand.transform.rotation = Quaternion.Lerp(Hand.transform.rotation, Quaternion.LookRotation(targetDir, Vector3.up), Time.deltaTime * turnSpeed * turnMult);
    }

    void CheckState()
    {
        if (state == EnemyState.PATROL && Vector3.Distance(PlayerManager.ptr.transform.position, transform.position) <= triggerRange)
        {
            state = EnemyState.TRIGGER;
            anim.SetInteger("state", 1);
        }
        else if(state == EnemyState.TRIGGER)
        {
            float distance = Vector3.Distance(PlayerManager.ptr.transform.position, transform.position);
            if (distance > triggerRange)
            {
                state = EnemyState.PATROL;
                anim.SetInteger("state", 4);
            }
            else if (distance <= attackRange && !Physics.Linecast(transform.position + Vector3.up, PlayerManager.ptr.transform.position, walls))
            {
                state = EnemyState.ATTACK;
                attackState = AttackState.START;
            }
        }
        else if (state == EnemyState.ATTACK && attackState != AttackState.ATTACK)
        {
            float distance = Vector3.Distance(PlayerManager.ptr.transform.position, transform.position);
            if (distance > triggerRange)
            {
                state = EnemyState.PATROL;
                attackState = AttackState.START;
                anim.SetInteger("state", 4);
            }
            else if(distance > attackRange)
            {
                state = EnemyState.TRIGGER;
                anim.SetInteger("state", 1);
            }
        }
    }

    void Attack()
    {
        switch (attackState)
        {
            case AttackState.START:
                Rotate();
                if (anim.GetCurrentAnimatorStateInfo(0).IsName(s[1]) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
                {
                    attackState = AttackState.ATTACK;
                    anim.SetInteger("state", 2);
                }
                break;
            case AttackState.ATTACK:
                if (!ignoreHit)
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName(s[2]) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.05f && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.375f && Physics.CheckSphere(grabPos.position, attackRadius, layer_mask, QueryTriggerInteraction.Ignore))
                    {
                        PlayerManager.ptr.SetDesc("Oh no it's got me!!!");
                        PlayerManager.ptr.SetHand(this);
                        playerGrabbed = true;
                        attackState = AttackState.COOLDOWN;
                        anim.SetInteger("state", 3);
                        return;
                    }
                }

                if (anim.GetCurrentAnimatorStateInfo(0).IsName(s[2]) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    attackState = AttackState.COOLDOWN;
                    anim.SetInteger("state", 1);
                }
                break;
            case AttackState.COOLDOWN:
                Rotate();
                if (anim.GetCurrentAnimatorStateInfo(0).IsName(s[1]) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.25)
                {
                    attackState = AttackState.ATTACK;
                    anim.SetInteger("state", 2);
                }
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackState == AttackState.ATTACK)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPos.position, attackRadius);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
