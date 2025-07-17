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


}
