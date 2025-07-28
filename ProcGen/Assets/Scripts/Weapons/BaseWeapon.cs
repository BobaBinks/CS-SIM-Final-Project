using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class BaseWeapon : MonoBehaviour
{

    #region Stat
    protected int Level = 0;
    #endregion

    #region Stat Curves
    [Header("Stat Curves")]
    [SerializeField] protected AnimationCurve damageCurve;
    #endregion

    public abstract void Fire();
}
