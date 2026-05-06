using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private InputAction _basicATKAction;
    private InputAction _heavyATKAction;

    [Header("Hitbox")]
    [SerializeField] private Transform _attackHitBox;
    [SerializeField] private float _attackRadius;

    [SerializeField] private Transform _heavyAttackHitBox;
    [SerializeField] private float _heavyAttackRadius;

    private int _bATKDmg = 1;

    [Header("Heavy ATK")]
    private int _hATKDmg = 2;

    [Header("Timer")]
    private float attackTimer;
    private float attackCooldown = 0.3f;
    private float heavyAttackTimer;
    private float heavyAttackCooldown = 1.5f;
    private WeaponReferences _weaponReferences;

    void Awake()
    {
        _basicATKAction = InputSystem.actions["Attack1"];
        _heavyATKAction = InputSystem.actions["Attack2"];

        animator = GetComponentInChildren<Animator>();
        _weaponReferences = GetComponentInChildren<WeaponReferences>();

        OnlyShowSlime();
    }

    void Update()
    {
        if(!LocalLevelLock.CanAttack) return;

        //Ataque fuerte
        heavyAttackTimer += Time.deltaTime;

        // 2. Manejo del Cooldown de Ataque
        attackTimer += Time.deltaTime;

        if(_basicATKAction.WasPressedThisFrame() && attackTimer >= attackCooldown)
        {
            ExecuteBasicAttack();
        }

        if(_heavyATKAction.IsPressed() && heavyAttackTimer >= heavyAttackCooldown)
        {
            ExecuteHeavyAttack();
            //WeaponHeavyAttack();
            //Attack(_hATKDmg, _heavyAttackHitBox, _heavyAttackRadius);
            //animator.SetTrigger("ExecuteHeavy");
            //SoundManager.PlaySound(SoundType.Heavy1, 1);
            //heavyAttackTimer = 0;
            //StartCoroutine(ReturnFromAttack());
        }
    }

    void ExecuteBasicAttack()
    {
        if (animator.IsInTransition(0)) return;
        WeaponBaseAttack();

        attackTimer = 0;
        //Attack(0, _attackHitBox, _attackRadius);
        Knockout(_attackHitBox, _attackRadius);
    
        animator.SetTrigger("Attack");
    }

    void ExecuteHeavyAttack()
    {
        WeaponHeavyAttack();
        Stun(_hATKDmg, _heavyAttackHitBox, _heavyAttackRadius);

        animator.SetTrigger("ExecuteHeavy");
        SoundManager.PlaySound(SoundType.Heavy1, 1);

        heavyAttackTimer = 0;

        StartCoroutine(ReturnFromAttack());
    }

    private void Stun(int DmgDealed, Transform hitBox, float radius)
    {
        Collider[] enemies = Physics.OverlapSphere(hitBox.position, radius);
        foreach(var item in enemies)
        {
            if(item.gameObject.layer == 6)
            {
                EnemyHealth _enemyHealthScript = item.gameObject.GetComponent<EnemyHealth>();
                if(_enemyHealthScript != null)
                {
                    //_enemyHealthScript.Damaged(DmgDealed);
                }
            }
            
            if(item.TryGetComponent(out IKnockbackeable knockbackeable))
            {
                Vector3 direction = (item.transform.position - transform.position).normalized;

                //float force = 0f;
                float duration = 4.5f;

                knockbackeable.GetKnockedBack(Vector3.zero, duration);
            }

            /*
            if(item.TryGetComponent(out IRageable rageable))
            {
                IALoglin loglinScript = item.gameObject.GetComponent<IALoglin>();
                loglinScript.Raged();
            }
            */

            if(item.gameObject.CompareTag("Chains"))
            {
                Eslabon _chainScript = item.gameObject.GetComponent<Eslabon>();
                _chainScript.RecibirGolpe();
            }
        }
    }

    private void Knockout(Transform hitBox, float radius)
    {
        Collider[] enemies = Physics.OverlapSphere(hitBox.position, radius);
        foreach(var item in enemies)
        {
            if(item.TryGetComponent(out IKnockbackeable knockbackeable))
            {
                Vector3 direction = (item.transform.position - transform.position).normalized;
                direction.y = 0;

                float forceMultiplier =  1f;
                float duration = 1.2f;

                if (knockbackeable.IsStunned)
                {
                    forceMultiplier = 10f; // Fuerza de "remate" mucho mayor
                    duration = 4.5f;       // Más tiempo volando
                }
                else
                {
                    forceMultiplier = 1.2f; // Impulso normal/pequeño
                    duration = 0.5f;
                }
                float baseForce = 300;
                Vector3 forceVector = (direction + Vector3.up * 0.3f) * (baseForce * forceMultiplier);
                //Vector3 verticalForce = Vector3.up * 0.2f;

                //direction = (direction + verticalForce).normalized;

                knockbackeable.GetKnockedBack(/*direction * baseForce*/forceVector, duration);
            }

            if(item.gameObject.CompareTag("Chains"))
            {
                Eslabon _chainScript = item.gameObject.GetComponent<Eslabon>();
                _chainScript.RecibirGolpe();
            }
        }
    }

    void Show(GameObject[] _weapon)
    {
        foreach (var part in _weapon)
        {
            part.SetActive(true);
        }
    }

    void Hide(GameObject[] _weapon)
    {
        foreach (var part in _weapon)
        {
            part.SetActive(false);
        }
    }

    private IEnumerator ReturnFromAttack()
    {
        yield return new WaitForSeconds(2.5f);
        OnlyShowSlime();
    }

    void HideAll()
    {
        foreach (var part in _weaponReferences.slimeParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.anchorParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.gloveParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.hammerParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.bateParts)
        {
            part.SetActive(false);
        }
    }

    void OnlyShowSlime()
    {
        foreach (var part in _weaponReferences.slimeParts)
        {
            part.SetActive(true);
        }
        foreach (var part in _weaponReferences.anchorParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.gloveParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.hammerParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.bateParts)
        {
            part.SetActive(false);
        }
    }

    void WeaponHeavyAttack()
    {
        foreach (var part in _weaponReferences.slimeParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.anchorParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.gloveParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.hammerParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.bateParts)
        {
            part.SetActive(false);
        }

        int randomWeapon = Random.Range(0, 2);
        if(randomWeapon == 0)
        {
            foreach (var part in _weaponReferences.anchorParts)
            {
                part.SetActive(true);
            }
        }
        else
        {
            foreach (var part in _weaponReferences.hammerParts)
            {
                part.SetActive(true);
            }
        }
    }

    void WeaponBaseAttack()
    {
        foreach (var part in _weaponReferences.slimeParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.anchorParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.gloveParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.hammerParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.bateParts)
        {
            part.SetActive(false);
        }
        foreach (var part in _weaponReferences.bateParts)
        {
            part.SetActive(true);
        }

        /*
        int randomWeapon = Random.Range(0, 2);
        if(randomWeapon == 0)
        {
            foreach (var part in _weaponReferences.gloveParts)
            {
                part.SetActive(true);
            }
        }
        else
        {
            foreach (var part in _weaponReferences.bateParts)
            {
                part.SetActive(true);
            }
        }
        */
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackHitBox.position, _attackRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_heavyAttackHitBox.position, _heavyAttackRadius);
    }
}