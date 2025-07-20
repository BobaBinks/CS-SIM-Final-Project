using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PathMovement))]
public abstract class EnemyAI: CharacterBase
{
    public Animator animator;
    public StateMachine<EnemyAI> Sm { get; set; }

    [HideInInspector]
    public List<Transform> wayPoints;

    public PathMovement pathMover { get; protected set; }


    [SerializeField] float attackRange = 2f;
    [SerializeField] float chaseRange = 2f;
    public float AttackRange
    {
        get 
        {
            if(attackRange > 0)
            {
                return attackRange;
            }
            return 0;
        }
    }

    public float ChaseRange
    {
        get
        {
            if (chaseRange > 0)
            {
                return chaseRange;
            }
            return 0;
        }
    }

    public Player player { get; private set; }

    public virtual void Start()
    {
        pathMover = GetComponent<PathMovement>();
    }

    protected virtual void Update()
    {
        if (Sm != null)
            Sm.Update(Time.deltaTime);
    }

    public virtual void AddPatrolWaypoints(List<Transform> wayPoints)
    {
        this.wayPoints = wayPoints;
    }

    public virtual bool PlayerInAttackRange()
    {
        if (player == null)
            return false;

        float distanceSquared = (player.transform.position - transform.position).sqrMagnitude;
        return distanceSquared < AttackRange * AttackRange;
    }

    public virtual bool PlayerInChaseRange()
    {
        if (player == null)
            return false;

        float distanceSquared = (player.transform.position - transform.position).sqrMagnitude;
        return distanceSquared < ChaseRange * ChaseRange;
    }

    public virtual void Initialize()
    {
        HealthPoints = maxHealthPoints;

        if (GameManager.Instance.player)
        {
            player = GameManager.Instance.player;
        }

        Sm = new StateMachine<EnemyAI>(this);
        Sm.AddState(new IdleState("idle"));
        Sm.AddState(new DeathState("death"));
        Sm.SetInitialState("idle");
    }

    public virtual void Initialize(bool shouldPatrol, List<Transform> wayPoints)
    {
        HealthPoints = maxHealthPoints;

        if (GameManager.Instance.player)
        {
            player = GameManager.Instance.player;
        }

        Sm = new StateMachine<EnemyAI>(this);
        Sm.AddState(new IdleState("idle"));
        Sm.AddState(new PatrolState("patrol"));
        Sm.AddState(new ChaseState("chase"));
        Sm.AddState(new DeathState("death"));

        this.wayPoints = wayPoints;

        if (shouldPatrol)
            Sm.SetInitialState("patrol");
        else
            Sm.SetInitialState("idle");
    }

    private void OnDrawGizmos()
    {
        if(attackRange > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        if (chaseRange > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
    }
}
