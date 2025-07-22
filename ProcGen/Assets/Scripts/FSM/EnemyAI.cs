using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(PathMovement))]
public abstract class EnemyAI: CharacterBase, IDamagable
{
    public Animator animator;
    public StateMachine<EnemyAI> Sm { get; set; }

    [HideInInspector]
    public List<Transform> wayPoints;

    #region UI
    [Header("UI")]
    [SerializeField] protected Image healthBar;

    #endregion

    public PathMovement pathMover { get; protected set; }


    #region Additional Stats
    [Header("Additional Stats")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float chaseRange = 2f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackDamage = 1f;
    public float AttackTimer { get; set; }

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

    public float AttackSpeed
    {
        get
        {
            if (attackSpeed > 0)
            {
                return attackSpeed;
            }
            return 0;
        }
    }

    public float AttackCooldown
    {
        get
        {
            if (attackCooldown > 0)
            {
                return attackCooldown;
            }
            return 0;
        }
    }

    public float AttackDamage
    {
        get
        {
            if (attackDamage > 0)
            {
                return attackDamage;
            }
            return 0;
        }
    }
    #endregion

    public Player player { get; private set; }

    public virtual void Start()
    {
        pathMover = GetComponent<PathMovement>();
        if (healthBar)
            healthBar.fillAmount = HealthPoints / MaxHealthPoints;
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

    public virtual void DealDamage()
    {
        if (player != null)
            player.TakeDamage(AttackDamage);
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
        Sm.AddState(new AttackState("attack"));
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
