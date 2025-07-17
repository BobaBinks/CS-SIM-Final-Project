using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteFlipper),typeof(Rigidbody2D))]
public abstract class EnemyAI: MonoBehaviour
{
    public Animator animator;
    public StateMachine<EnemyAI> Sm { get; set; }

    [HideInInspector]
    public List<Transform> wayPoints;

    public SpriteFlipper spriteFlipper { get; protected set; }
    public Rigidbody2D rigidBody { get; protected set; }

    #region Enemy Stats
    [SerializeField]
    private float maxHealthPoints = 100f; // default value for safety
    [SerializeField]
    private float moveSpeed = 3.5f;

    public List<Vector3> _pathDebugList;

    public float MaxHealthPoints 
    {
        get { return maxHealthPoints; }
    }
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public float HealthPoints { get; protected set; }
    #endregion

    public virtual void Start()
    {
        spriteFlipper = GetComponent<SpriteFlipper>();
        rigidBody = GetComponent<Rigidbody2D>();
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

    public virtual void Initialize()
    {
        HealthPoints = maxHealthPoints;
        
        Sm = new StateMachine<EnemyAI>(this);
        Sm.AddState(new IdleState("idle"));
        Sm.AddState(new DeathState("death"));
        Sm.SetInitialState("idle");
    }

    public virtual void Initialize(bool shouldPatrol, List<Transform> wayPoints)
    {
        HealthPoints = maxHealthPoints;
        Sm = new StateMachine<EnemyAI>(this);
        Sm.AddState(new IdleState("idle"));
        Sm.AddState(new PatrolState("patrol"));
        Sm.AddState(new DeathState("death"));

        this.wayPoints = wayPoints;

        if (shouldPatrol)
            Sm.SetInitialState("patrol");
        else
            Sm.SetInitialState("idle");
    }

    private void OnDrawGizmos()
    {
        if (_pathDebugList == null || _pathDebugList.Count == 0)
            return;

        for(int i = 0; i < _pathDebugList.Count; ++i)
        {
            if (i == _pathDebugList.Count - 1)
                break;

            Gizmos.DrawLine(_pathDebugList[i], _pathDebugList[i + 1]);
        }
    }
}
