using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class BaseWeapon : MonoBehaviour
{

    #region Stat
    [SerializeField] protected int Level = 0;
    #endregion

    #region Stat Curves
    [Header("Stat Curves")]
    [SerializeField] protected AnimationCurve damageCurve;
    #endregion

    public virtual float GetDamage()
    {
        if (damageCurve == null || damageCurve.Evaluate(Level) < 0)
            return 0;

        return damageCurve.Evaluate(Level);
    }

    public void SetLevel(int level)
    {
        if (level < 0)
            return;

        this.Level = level;
    }

    public int GetLevel()
    {
        return Level;
    }

    public abstract void Fire();
}
