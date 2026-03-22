using Unity.VisualScripting;
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

    private int _bATKDmg = 1;

    [Header("Heavy ATK")]
    [SerializeField] private int _hATKDmg = 2;
    [SerializeField] private float _timerDuration = 1f;
    private float _timer = 0;
    private bool isCharging = false;
    
    /*
    [Header("Shoot")]
    [SerializeField] private Transform _shootSpawn;
    [SerializeField] private GameObject _bulletPrefab;
    private float _shotForce = 15f;
    private float upwardForce = 10f;
    */

    void Awake()
    {
        _basicATKAction = InputSystem.actions["Attack1"];
        _heavyATKAction = InputSystem.actions["Attack2"];

        _playerMovementScript = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if(_basicATKAction.WasPressedThisFrame() && !isCharging)
        {
            Attack(_bATKDmg);
            animator.SetTrigger("Attack");
            Debug.Log("Ataque normal");
        }

        if(_heavyATKAction.IsPressed())
        {
            isCharging = true;
            _timer += Time.deltaTime;

            if(_timer >= _timerDuration)
            {
                Debug.Log("Ataque cargado");
            }
        }

        if(_heavyATKAction.WasReleasedThisFrame())
        {
            if(_timer >= _timerDuration)
            {
                //animator.SetTrigger("A")
                Debug.Log("Ejecutar ataque cargadoo");
                Attack(_hATKDmg);
            }

            if(_timer < _timerDuration)
            {
                Debug.Log("Cancelar animación");
                _timer = 0;
            }

            _timer = 0;
            isCharging = false;
        }

        /*
        if(_basicATKAction.WasPressedThisFrame() && _playerMovementScript.isToggled)
        {
            Debug.Log("Disparo");
            ShootSlime2();
        }
        */
    }

    private void Attack(int DmgDealed)
    {
        Collider[] enemies = Physics.OverlapSphere(_attackHitBox.position, _attackRadius);
        foreach(var item in enemies)
        {
            if(item.gameObject.layer == 6)
            {
                //EnemyHealth enemyHealth = item.gameObject.GetComponent<EnemyHealth>();
                //EnemyHitFlash enemy = item.gameObject.GetComponent<EnemyHitFlash>();
                /*if(enemyHealth != null)
                {
                    enemyHealth.TakeDamage(DmgDealed);
                    enemy.GetComponent<EnemyHitFlash>().TakeDamage();
                }*/
            }

            //Para los muros
            if(item.gameObject.tag == "Breakable" && DmgDealed == _hATKDmg)
            {
                //Break _break = item.gameObject.GetComponent<Break>();
                //_break.BreakTheThing();
            }
        }
    }

    /*
    private void ShootSlime2()
    {
    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    RaycastHit hit;

    Vector3 targetPoint;

    if (Physics.Raycast(ray, out hit))
        targetPoint = hit.point;
    else
        targetPoint = ray.GetPoint(100);

    Vector3 direction = (targetPoint - _shootSpawn.position).normalized;

    GameObject currentBullet = Instantiate(
        _bulletPrefab,
        _shootSpawn.position,
        Quaternion.identity
    );

    Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
    rb.linearVelocity = Vector3.zero;

    // Fuerza combinada (horizontal + vertical)
    Vector3 force = direction * _shotForce + Vector3.up * upwardForce;

    rb.AddForce(force, ForceMode.Impulse);
    }
    */

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackHitBox.position, _attackRadius);
    }
}