using UnityEngine;

public interface IKnockbackeable
{
    void GetKnockedBack (Vector3 force, float duration);
    bool IsStunned { get; }
}
