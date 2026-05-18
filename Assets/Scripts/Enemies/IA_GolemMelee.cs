using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class IA_GolemMelee : MonoBehaviour, IAtacante, IKnockbackeable
{
    private NavMeshAgent _agent;
    private Transform player;
    private Animator animator;
    private Vector3 originPoint;
    public enum State { Wandering, Chase, Attack, Stunned }
    public State currentState;

    [Header("Movement")]
    public float wanderingRadius = 7f;
    public float maxDistanceDelta = 25f; // Distancia máxima desde el origen
    public float chaseRange = 15f;
    public float attackRange = 3f; 
    public float rotationSpeed = 15f; // Giro rápido y fluido
    public float walkSpeed = 4f;
    public float runSpeed = 8f;

    [Header("Detection & FOV")]
    public float eyeHeight = 1.6f;
    public float viewAngle = 100f;
    public float forwardOffset = 0.6f; // Para saltarse su propia colisión

    [Header("Wandering Settings")]
    public float waitTime = 2f;
    private float waitTimer;
    private bool isWaiting;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    private float attackTimer;
    public float stoppingDistanceBuffer = 0.5f;
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private float attackHitboxRange;
    [SerializeField] private float attackDamage;

    [Header("Combo Settings")]
    public int comboStep = 0;
    public float comboResetTime = 3.5f;
    private float lastAttackTime;

    private Rigidbody _rigidBody;
    public bool IsStunned => currentState == State.Stunned;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _collisionForceThreshold = 4f;
    [SerializeField] private int _explosionDamage = 50;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _maxAirTime = 2.0f;

    [Header("Levitation Audio")]
    [SerializeField] private AudioSource levitationAudioSource; // El AudioSource que tendrá el loop
    [SerializeField] private float maxLevitationVolume = 0.5f;   // Volumen máximo al correr
    [SerializeField] private float volumeLerpSpeed = 5f;        // Suavidad del cambio de volumen

    [Header("VFX stun")]
    [SerializeField] private GameObject _VFXPrefab;
    private GameObject _currentStunVFX;
    [SerializeField] private Transform _vfxSpawnPoint;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        originPoint = transform.position;

        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.isKinematic = true;
    }

    void Start()
    {
        // Configuración para evitar conflictos de rotación y permitir frenado en seco
        _agent.updateRotation = false; 
        _agent.acceleration = 100f; 

        currentState = State.Wandering;
        PickRandomPoint();

        if (levitationAudioSource != null)
        {
            levitationAudioSource.loop = true;
            levitationAudioSource.volume = 0;
            levitationAudioSource.Play();
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distToOrigin = Vector3.Distance(transform.position, originPoint);
        
        if (distToOrigin > maxDistanceDelta && currentState != State.Wandering)
            {
                ReturnToOrigin();
            }

        switch (currentState)
        {
            case State.Wandering:
            UpdateWanderingState(distanceToPlayer);
            break;

            case State.Chase:
            UpdateChaseState(distanceToPlayer);
            break;

            case State.Attack:
            UpdateAttackState(distanceToPlayer);
            break;

            case State.Stunned:
            HandleStun();
            break;
        }

        // Aplicamos la rotación manual para que sea instantánea al cambiar de dirección
        ApplyManualRotation();
        
        // Actualizamos las animaciones
        UpdateAnimator();

        UpdateLevitationSound();
    }

    private void UpdateWanderingState(float distanceToPlayer)
    {
        if (_agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.speed = walkSpeed;
            _agent.stoppingDistance = 0.2f; // Permitir que llegue al punto exacto

            // Detectar jugador
            if (CanSeePlayer(distanceToPlayer))
            {
                isWaiting = false;
                currentState = State.Chase;
                return;
            }

            // Lógica de patrulla con espera
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    waitTimer = 0f;
                }

                waitTimer += Time.deltaTime;

                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    PickRandomPoint();
                }
            }
        }
    }

    private void UpdateChaseState(float distanceToPlayer)
    {
        if (!_agent.enabled || !_agent.isOnNavMesh) return;

        _agent.isStopped = false;
        _agent.speed = runSpeed;
        _agent.stoppingDistance = attackRange - 0.5f; 
        SafeSetDestination(player.position);

        // Atacar de inmediato si entra en rango
        if (distanceToPlayer <= attackRange)
        {
            StartAttackSequence();
        }

        // Perder al jugador
        if (!CanSeePlayer(distanceToPlayer) && distanceToPlayer > attackRange + 2f)
        {
            isWaiting = false;
            ReturnToOrigin();
        }
    }

    private void UpdateAttackState(float distanceToPlayer)
    {
        if (_agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
        }

        // Clavar los pies mientras esté en rango de ataque
        if (distanceToPlayer <= attackRange)
            _agent.isStopped = true;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            Attack();
            attackTimer = 0f;
        }

        // Si el jugador se aleja lo suficiente, volver a perseguir
        if (distanceToPlayer > attackRange + stoppingDistanceBuffer)
        {
            _agent.isStopped = false;
            currentState = State.Chase;
        }
    }

    private void ApplyManualRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        if (currentState == State.Attack)
        {
            targetDirection = (player.position - transform.position).normalized;
        }
        else if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            targetDirection = _agent.velocity.normalized;
        }

        if (targetDirection != Vector3.zero)
        {
            targetDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void StartAttackSequence()
    {
        // Esta función fuerza el primer golpe sin esperar al timer
        animator.SetInteger("ComboIndex", comboStep);
        animator.SetTrigger("Attack"); 
        
        attackTimer = 0f; 
        currentState = State.Attack;
    }

    void PickRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderingRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderingRadius, NavMesh.AllAreas))
        {
            SafeSetDestination(hit.position);
        }
    }

    bool CanSeePlayer(float dist)
    {
        if (dist > chaseRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        // USAMOS EL ÁNGULO
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < viewAngle / 2f)
        {
            // AJUSTE CRÍTICO: El targetPos debe ser el centro del Player, no los pies.
            // Si player.position es la base (pies), el rayo al suelo suele fallar.
            Vector3 startPos = transform.position + (Vector3.up * eyeHeight) + (transform.forward * forwardOffset);
            
            // Apuntamos a 1 metro sobre la base del player (el pecho/cabeza)
            Vector3 targetPos = player.position + Vector3.up * 1.2f; 
            Vector3 direction = (targetPos - startPos).normalized;

            RaycastHit hit;

            // Lanzamos el rayo
            if (Physics.Raycast(startPos, direction, out hit, chaseRange))
            {
                // Debug para ver el rayo en tiempo real
                Debug.DrawLine(startPos, hit.point, Color.red);

                if (hit.collider.CompareTag("Player")) 
                {
                    return true; 
                }
                else 
                {
                    // Si choca con otra cosa, imprime qué es para saber qué lo bloquea
                    // Debug.Log("Bloqueado por: " + hit.collider.name);
                }
            }
        }
            return false;
    }

    void Attack()
    {
        if (animator == null) return;

        // Reset del combo si pasó mucho tiempo
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        // Disparar animación
        animator.ResetTrigger("Attack");
        animator.SetInteger("ComboIndex", comboStep);
        animator.SetTrigger("Attack");

        lastAttackTime = Time.time;
        
        // Siguiente paso del combo
        comboStep = (comboStep + 1) % 2;
    }

    void ReturnToOrigin()
    {
        currentState = State.Wandering; SafeSetDestination(originPoint);
    }

    private void SafeSetDestination(Vector3 target)
    {
        // Solo si el agente está encendido, activo en la jerarquía y tocando el NavMesh
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.SetDestination(target);
        }
    }

    public void PlayerDamage()
    {
        Collider[] reachedObjects = Physics.OverlapSphere(attackHitbox.position, attackHitboxRange);
        foreach(Collider col in reachedObjects)
        {
            if(col.CompareTag("Player"))
            {
                // 1. Daño (Tu lógica actual)
                PlayerHealth _playerHealthScript = col.gameObject.GetComponent<PlayerHealth>();
                if(_playerHealthScript != null)
                {
                    _playerHealthScript.Damaged(attackDamage);
                }

                // 2. Knockback (NUEVO)
                PlayerController player = col.GetComponent<PlayerController>();
                if(player != null)
                {
                    // Calculamos dirección desde el enemigo al jugador
                    Vector3 knockDir = (col.transform.position - transform.position);
                    knockDir.y = 0; // Limpiamos Y para normalizar
                    knockDir = knockDir.normalized;

                    // Definimos la fuerza: 
                    // Un poco de XZ para empujar y un poco de Y para "levantar" ligeramente
                    float horizontalForce = 15f;
                    float verticalLift = 2f; 
                    
                    Vector3 finalForce = (knockDir * horizontalForce) + (Vector3.up * verticalLift);

                    SoundManager.PlaySound(SoundType.GolemHit, 0.5f);
                    player.ApplyKnockback(finalForce);
                }
            }
        }
    }

    float stunTimer;
    void HandleStun()
    {
        stunTimer -= Time.deltaTime;
        //si llega a cero se reactiva el agent y cambia de estado
        if(stunTimer <= 0)
        {
            _agent.enabled = true;
            currentState = State.Chase;
        }
    }

    private IEnumerator AutoExplodeSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Si llegados a este punto el enemigo sigue volando (Stunned), explota
        if (currentState == State.Stunned)
        {
            Explode();
        }
    }

    public void GetKnockedBack(Vector3 force, float duration)
    {
        StopAllCoroutines(); // Detiene recuperaciones y auto-explosiones previas

        // LIMPIEZA BRUTA: Busca si quedó algún clon suelto por la escena con el mismo nombre y lo borra
        if (_currentStunVFX != null) Destroy(_currentStunVFX);
    
        // Esto buscará en toda la escena cualquier objeto que se llame como tu prefab clonado y lo fulminará
        GameObject ghostVFX = GameObject.Find(_VFXPrefab.name + "(Clone)");
        if (ghostVFX != null) Destroy(ghostVFX);

        SoundManager.PlaySound(SoundType.GolemStun, 0.3f);
        currentState = State.Stunned;
        stunTimer = duration;
        _agent.enabled = false;


        if (_VFXPrefab != null && _currentStunVFX == null)
        {
            // Calculamos una posición un poco por encima del Golem (ej. 2 metros arriba)
            Vector3 spawnPosition = transform.position + Vector3.up * 2f;

            // INSTANTIATE CON PADRE: Prefab, Posición, Rotación y transform (el padre)
            _currentStunVFX = Instantiate(_VFXPrefab, spawnPosition, Quaternion.identity, transform);
        }

        Instantiate(_VFXPrefab, _vfxSpawnPoint.position, _vfxSpawnPoint.rotation);

        if(force.magnitude > 0.1f)
        {
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;
            _rigidBody.AddForce(force, ForceMode.Impulse);
            
            animator.SetBool("isStunned", true);    
            
            // --- INICIAMOS AMBOS CAMINOS ---
            StartCoroutine(RecoverySequence(duration)); // Intento de recuperarse
            StartCoroutine(AutoExplodeSequence(_maxAirTime)); // Mecha de explosión por tiempo
        }
        else
        {
            // Stun estático (sin explosión por tiempo)
            _rigidBody.isKinematic = true;
            _rigidBody.linearVelocity = Vector3.zero;
            animator.SetBool("isStunned", true);
            StartCoroutine(StaticStunSequence(duration));
        }
    }

    private IEnumerator StaticStunSequence(float duration)
    {
        yield return new WaitForSeconds(duration);

        if(currentState == State.Stunned && _rigidBody.isKinematic)
        {
            currentState = State.Chase;
            _agent.enabled = true;
        }

        if (_currentStunVFX != null)
        {
            // Si tu prefab tiene un ParticleSystem, puedes pararlo primero para que se desvanezca
            if (_currentStunVFX.TryGetComponent(out ParticleSystem ps))
            {
                ps.Stop();
                Destroy(_currentStunVFX, 1f); // Espera 1 segundo a que desaparezcan las últimas partículas
            }
            else
            {
                Destroy(_currentStunVFX); // Si no, se destruye de golpe
            }
            
            _currentStunVFX = null; // Vaciamos la referencia
        }
    }

    private IEnumerator RecoverySequence(float duration)
    {
        yield return new WaitForSeconds(duration);

        bool grounded = false;
        while (!grounded)
        {
            if (this == null) yield break;
            
            // Solo intentamos recuperarnos si estamos tocando el suelo DE VERDAD
            grounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 0.8f);
            
            yield return new WaitForSeconds(0.1f);
        }

        // SOLO después de estar en el suelo cambiamos el estado
        _rigidBody.isKinematic = true;
        _rigidBody.useGravity = false;
        animator.SetBool("isStunned", false);
        
        yield return new WaitForSeconds(1.0f);

        _agent.enabled = true;
        currentState = State.Chase; // <--- El estado cambia SOLO al final de todo

         if (_currentStunVFX != null)
        {
            // Si tu prefab tiene un ParticleSystem, puedes pararlo primero para que se desvanezca
            if (_currentStunVFX.TryGetComponent(out ParticleSystem ps))
            {
                ps.Stop();
                Destroy(_currentStunVFX, 1f); // Espera 1 segundo a que desaparezcan las últimas partículas
            }
            else
            {
                Destroy(_currentStunVFX); // Si no, se destruye de golpe
            }
            
            _currentStunVFX = null; // Vaciamos la referencia
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // Si el agente está apagado (volando), usamos la velocidad del Rigidbody
            float currentSpeed = (_agent.enabled) ? _agent.velocity.magnitude : _rigidBody.linearVelocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }
        
        if (animator == null) return;
        // El parámetro "Speed" debe mover el Blend Tree de Idle a Run
        animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

   private void OnCollisionEnter(Collision collision)
    {
        if (currentState == State.Stunned)
        {
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce > _collisionForceThreshold)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        // 1. Audio de explosión
        // SoundManager.Instance.PlaySound("ExplosionEnemy", transform.position); // <-- LINEA DE AUDIO

        // 2. Instanciar el efecto visual
        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }

        // 3. Daño de área
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider col in nearbyEnemies)
        {
            if (col.gameObject == this.gameObject) continue;

            if (col.TryGetComponent(out EnemyHealth health))
            {
                health.Damaged(_explosionDamage);
            }
        }

        //Sonido de explotar
        SoundManager.PlaySound(SoundType.GolemExplode);

        // 4. Destruir al enemigo
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderingRadius);

        //FOV
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * chaseRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * chaseRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackHitbox.position, attackHitboxRange);
    }

    private void UpdateLevitationSound()
    {
        if (levitationAudioSource == null) return;

        // Calculamos la velocidad actual (agente o rigidbody)
        float currentSpeed = (_agent.enabled) ? _agent.velocity.magnitude : _rigidBody.linearVelocity.magnitude;

        // Si está aturdido o casi quieto, el volumen objetivo es 0
        float targetVolume = (currentState == State.Stunned || currentSpeed < 0.1f) ? 0f : (currentSpeed / runSpeed) * maxLevitationVolume;

        // Suavizamos el cambio de volumen para que no se escuche un "clic" brusco
        levitationAudioSource.volume = Mathf.Lerp(levitationAudioSource.volume, targetVolume, Time.deltaTime * volumeLerpSpeed);

        // Opcional: Variar un poco el pitch según la velocidad para darle dinamismo
        levitationAudioSource.pitch = Mathf.Lerp(0.8f, 1.2f, currentSpeed / runSpeed);
    }
}