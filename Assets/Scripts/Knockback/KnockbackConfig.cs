using UnityEngine;

[CreateAssetMenu(fileName = "New Knockback Config", menuName = "Scriptable Objects/KnockbackConfig")]
public class KnockbackConfig : ScriptableObject
{
    public float KnockbackStrength = 1000;
    public ParticleSystem.MinMaxCurve DistanceFalloff;

    public Vector3 GetKnockbackStrength(Vector3 direction, float distance)
    {
        return KnockbackStrength * DistanceFalloff.Evaluate(distance) * direction;
    }
}
