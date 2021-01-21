using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float attackRange = 2f;
    public enum EnemyState { PATROL, TRIGGER, ATTACK }
    public enum AttackState { START, ATTACK, COOLDOWN }
    protected EnemyState state;
    protected AttackState attackState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
