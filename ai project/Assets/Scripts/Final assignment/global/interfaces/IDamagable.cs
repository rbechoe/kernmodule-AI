using UnityEngine;

public interface IDamagable
{
    void TakeDamage(int amount, DamageType damageType, GameObject attacker);
}

public enum DamageType
{
    RawDamage,
    Daze,
}
