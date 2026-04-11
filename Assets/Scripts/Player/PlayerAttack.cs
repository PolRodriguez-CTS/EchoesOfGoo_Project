using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private InputAction _basicATKAction;
    private InputAction _heavyATKAction;

    private PlayerController _playerMovementScript;

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

    [Header("ComboStep")]
    private int _comboCount = 0;
    private float _comboTimer;
    [SerializeField] private float _comboResetTime = 1f;

    private WeaponReferences _weaponReferences;

    void Awake()
    {
        _basicATKAction = InputSystem.actions["Attack1"];
        _heavyATKAction = InputSystem.actions["Attack2"];

        _playerMovementScript = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
        _weaponReferences = GetComponentInChildren<WeaponReferences>();

        OnlyShowSlime();
    }

    void Update()
    {
        //Ataque fuerte
        heavyAttackTimer += Time.deltaTime;

        // 1. Manejo del Reset del Combo
    if (_comboCount > 0)
    {
        _comboTimer -= Time.deltaTime;
        if (_comboTimer <= 0)
        {
            _comboCount = 0;
            animator.SetInteger("ComboStep", 0);
            //Debug.Log("Combo reseteado por inactividad");
        }
    }

    // 2. Manejo del Cooldown de Ataque
    attackTimer += Time.deltaTime;

    if(_basicATKAction.WasPressedThisFrame() && attackTimer >= attackCooldown)
    {
        ExecuteBasicAttack();
    }

        if(_heavyATKAction.IsPressed() && heavyAttackTimer >= heavyAttackCooldown)
        {
            WeaponHeavyAttack();
            Attack(_hATKDmg, _heavyAttackHitBox, _heavyAttackRadius);
            animator.SetTrigger("ExecuteHeavy");
            SoundManager.PlaySound(SoundType.Heavy1, 1);
            heavyAttackTimer = 0;
            StartCoroutine(ReturnFromAttack());
        }
    }

    void ExecuteBasicAttack()
    {
        // Bloqueo: Si el Animator está en transición, ignoramos el click para no repetir Atk1
        if (animator.IsInTransition(0)) return;

        WeaponBaseAttack();

        // 1. Limpiamos triggers acumulados del spam
        animator.ResetTrigger("Attack");
        SoundManager.PlaySound(SoundType.Attack1, 1);

        // 2. Aplicamos daño
        Attack(_bATKDmg, _attackHitBox, _attackRadius);
        
        // 3. Seteamos el paso ANTES del trigger
        animator.SetInteger("ComboStep", _comboCount);
        animator.SetTrigger("Attack");

        // 4. Lógica de tiempos y contador
        _comboCount = (_comboCount + 1) % 2;

        _comboTimer = _comboResetTime; 
        attackTimer = 0; // El cooldown (0.1) ahora empezará DESDE aquí

        StartCoroutine(ReturnFromAttack());
    }

    private void Attack(int DmgDealed, Transform hitBox, float radius)
    {
        Collider[] enemies = Physics.OverlapSphere(hitBox.position, radius);
        foreach(var item in enemies)
        {
            if(item.gameObject.layer == 6)
            {
                EnemyHealth _enemyHealthScript = item.gameObject.GetComponent<EnemyHealth>();
                if(_enemyHealthScript != null)
                {
                    _enemyHealthScript.Damaged(DmgDealed);
                }
            }
            
            if(item.TryGetComponent(out IKnockbackeable knockbackeable))
            {
                Vector3 direction = (item.transform.position - transform.position);
                direction.y = 0;
                direction = direction.normalized;

                float force = 30f;
                float duration = 0.2f;

                knockbackeable.GetKnockedBack(direction * force, duration);
            }

            if(item.TryGetComponent(out IRageable rageable))
            {
                IALoglin loglinScript = item.gameObject.GetComponent<IALoglin>();
                loglinScript.Raged();
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
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackHitBox.position, _attackRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_heavyAttackHitBox.position, _heavyAttackRadius);
    }
}