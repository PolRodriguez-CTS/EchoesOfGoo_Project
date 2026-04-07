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

    private int _bATKDmg = 20;

    [Header("Heavy ATK")]
    private int _hATKDmg = 40;
    [SerializeField] private float _timerDuration = 1f;
    private float _timer = 0;
    private bool isCharging = false;

    [Header("Timer")]
    private float attackTimer;
    private float attackCooldown = 0.3f;

    private float heavyAttackTimer;
    private float heavyAttackCooldown = 1.5f;

    [Header("Weapons")]
    [SerializeField] private GameObject Slime;
    [SerializeField] private GameObject Bate;
    [SerializeField] private GameObject Glove;
    [SerializeField] private GameObject Hammer;
    [SerializeField] private GameObject Anchor;

    [Header("ComboStep")]
    private int _comboCount = 0;
    private float _lastClickTime;
    private float _comboTimer; // El contador local
    [SerializeField] private float _comboResetTime = 1f;


    private WeaponReferences _weaponReferences;

    void Awake()
    {
        _basicATKAction = InputSystem.actions["Attack1"];
        _heavyATKAction = InputSystem.actions["Attack2"];

        _playerMovementScript = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
        _weaponReferences = GetComponentInChildren<WeaponReferences>();
    }

    void Start()
    {
        /*
        Hide(_weaponReferences.anchorParts);
        Hide(_weaponReferences.gloveParts);
        Hide(_weaponReferences.hammerParts);
        Hide(_weaponReferences.bateParts);
        */
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
            Debug.Log("Combo reseteado por inactividad");
        }
    }

    // 2. Manejo del Cooldown de Ataque
    attackTimer += Time.deltaTime;

    if(_basicATKAction.WasPressedThisFrame() && !isCharging && attackTimer >= attackCooldown)
    {
        ExecuteBasicAttack();
    }

        if(_heavyATKAction.IsPressed() && heavyAttackTimer >= heavyAttackCooldown)
        {
            //Show(_weaponReferences.hammerParts);
            Attack(_hATKDmg);
            animator.SetTrigger("ExecuteHeavy");
            heavyAttackTimer = 0;
            //Hide(_weaponReferences.hammerParts);
        }
    }

    void ExecuteBasicAttack()
    {
        // Bloqueo: Si el Animator está en transición, ignoramos el click para no repetir Atk1
        if (animator.IsInTransition(0)) return;

        // 1. Limpiamos triggers acumulados del spam
        animator.ResetTrigger("Attack");

        // 2. Aplicamos daño
        Attack(_bATKDmg);
        
        // 3. Seteamos el paso ANTES del trigger
        animator.SetInteger("ComboStep", _comboCount);
        animator.SetTrigger("Attack");

        // 4. Lógica de tiempos y contador
        _comboCount = (_comboCount + 1) % 2;

        _comboTimer = _comboResetTime; 
        attackTimer = 0; // El cooldown (0.1) ahora empezará DESDE aquí
    }

    private void Attack(int DmgDealed)
    {
        Collider[] enemies = Physics.OverlapSphere(_attackHitBox.position, _attackRadius);
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackHitBox.position, _attackRadius);
    }
}