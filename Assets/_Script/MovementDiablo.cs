using System.Collections;
using UnityEngine;

public class MovementDiablo : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 8f;
    public float jumpPower = 12f;
    public float doubleJumpPower = 10f;
    public float safeDistance = 5;

    [Header("Slide Settings")]
    public float slideSpeedMultiplier = 2.0f;
    public float slideDuration = 0.8f;

    [Header("Obstacle Detection")]
    public Transform footCheckPoint;
    public Transform headCheckPoint;
    public float obstacleCheckDistance = 1.0f;
    public LayerMask obstacleLayer;

    [Header("Collision Checks")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask waterLayer;

    [Header("Components")]
    [SerializeField] private Animator _animator;
    public Transform MC;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;

    // State Variable
    private bool isRunningAway = false;
    private bool isGround;
    private bool canDoubleJump;
    private bool isSliding;
    private bool isWater;

    // Timer agar tidak spam lompat (jeda antara lompat 1 dan 2)
    private float jumpBufferTime = 0.3f;
    private float jumpTimerCounter;

    // Timer buffer animasi (agar animasi lompat mulus tidak flicker)
    // private float jumpBufferTime = 0.2f;
    // private float jumpBufferCounter;

    public Vector2 slideSize = new Vector2(11.31f, 4.37f);
    public Vector2 slideOffset = new Vector2(0f, -1.2f);
    private Vector2 standSize;
    private Vector2 standOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();

        standSize = coll.size;
        standOffset = coll.offset;
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Timer cooldown lompat dikurangi terus
        if (jumpTimerCounter > 0) jumpTimerCounter -= Time.deltaTime;

        // if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        if (MC == null) return;

        // 1. Cek Tanah (Supaya gak bisa lompat pas terbang)
        // isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        CheckSurroundings();

        // 2. Cek Jarak (Logic asli kamu)
        CheckDistance();

        // 3. Update Animasi Lompat
        UpdateAnimation();
        // if (!isSliding)
        // {
        //     if (isGround)
        //     {
        //         canDoubleJump = true;
        //         _animator.SetBool("isJumping", false);
        //     }
        //     else
        //     {
        //         _animator.SetBool("isWalking", false);
        //         _animator.SetBool("isJumping", true);

        //     }
        // }
    }

    void FixedUpdate()
    {
        if (MC == null) return;

        if (isRunningAway)
        {
            RunAway();
        }
        else
        {
            // rb.linearVelocity = Vector2.zero;
            // _animator.SetBool("isWalking", false);
            StopRunning();
        }
    }

    void CheckDistance()
    {
        if (MC == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, MC.position);
        
        if (distanceToPlayer < safeDistance)
        {
            isRunningAway = true;
        }
        else
        {
            isRunningAway = false;
        }
    }

    void RunAway()
    {
        float currentSpeed = moveSpeed;

        // 1. Tentukan arah lari (hanya sumbu X)
        // Jika player di Kanan DABLO, maka (DABLO - MC) hasilnya Minus (Lari ke Kiri)
        float directionX = transform.position.x - MC.position.x;

        // kita ambil arahnya saja (1 atau -1)
        float directionSign = Mathf.Sign(directionX);

        if (isSliding)
        {
            // rb.linearVelocity = new Vector2(directionSign * moveSpeed * slideSpeedMultiplier, rb.linearVelocity.y);
            // return;
            currentSpeed *= slideSpeedMultiplier;
        }

        // 2. Gerakan musuh (Pertahankan Y velocity biar gravitasi tetap jalan)
        rb.linearVelocity = new Vector2(directionSign * currentSpeed, rb.linearVelocity.y);

        // 4. Animasi & Flip
        // if (!isSliding && isGround) _animator.SetBool("isWalking", true);
        Flip(directionSign);

        // --- Otak Utama AI (Sensor Logic) ---
        bool footHit = CheckWall(footCheckPoint, directionSign);
        bool headHit = CheckWall(headCheckPoint, directionSign);

        // Kondisi 1: Obstacle tinggi (kaki kena, kepala kena) --> double jump
        if (footHit && headHit)
        {
            // canDoubleJump = true;
            TryJump();
        }
        // Kondisi 2: Celah Sempit (Kaki aman, Kepala kena) --> Slide
        else if (!footHit && headHit)
        {
            StartCoroutine(SlideProcess());
        }
        else if (footHit && !headHit)
        {
            // canDoubleJump = false;
            TryJump();
        }

        // 3. LOGIKA BARU: Minta laporan dari CheckObstacle
        // "Apakah ada tembok di depan (sesuai arah lari)?"
        // if (CheckObstacle(directionSign))
        // {
        //     // Jump();
        //     TryJump();
        // }

        // Vector2 direction = (transform.position - MC.position).normalized;
        // rb.linearVelocity = direction * moveSpeed;
    }

    // bool CheckObstacle(float directionSign)
    // {
    //     // Kalau sedang terbang, anggap tidak ada obstacle (biar gak lompat di udara)
    //     // if (!isGround) return false;
    //     if (footCheckPoint == null) return false;

    //     // Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + 0.5f);
    //     Vector2 rayDirection = Vector2.right * directionSign;

    //     RaycastHit2D hit = Physics2D.Raycast(footCheckPoint.position, rayDirection, obstacleCheckDistance, obstacleLayer);

    //     // Kembalikan TRUE jika kena tembok, FALSE jika kosong
    //     return hit.collider != null;
    // }

    bool CheckWall(Transform sensorPoint, float directionSign)
    {
        if (sensorPoint == null) return false;
        Vector2 origin = sensorPoint.position;
        Vector2 dir = Vector2.right * directionSign;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, obstacleCheckDistance, obstacleLayer);
        return hit.collider != null;
    }

    IEnumerator SlideProcess()
    {
        isSliding = true;

        //Ubah bentuk collider jadi gepeng
        coll.size = slideSize;
        coll.offset = slideOffset;
        coll.direction = CapsuleDirection2D.Horizontal;

        // _animator.SetBool("isWalking", false);
        _animator.SetBool("isSliding", true);

        yield return new WaitForSeconds(slideDuration);

        // Kembali Normal
        coll.size = standSize;
        coll.offset = standOffset;
        coll.direction = CapsuleDirection2D.Vertical;
        _animator.SetBool("isSliding", false);
        isSliding = false;
    }

    void TryJump()
    {
        // Cek kondisi dulu (biar gak double jump instan dalam 1 frame)
        if (jumpTimerCounter > 0) return;
        
        // kondisi 1: Lompat Pertama (dari tanah)
        if (isGround)
        {
            Jump(jumpPower);
            canDoubleJump = true;
            // jumpTimerCounter = jumpBufferTime;
        }
        else if (canDoubleJump)
        {
            Jump(doubleJumpPower);
            canDoubleJump = false;

            // Reset animasi biar keliatan nendang lagi (opsional)
            // _animator.Play("dablo jump", -1, 0f); 
        }
    }

    void Jump(float power)
    {
        // Pastikan tidak sedang lompat (safety extra)
        // if (!isGround) return;
        jumpTimerCounter = jumpBufferTime;

        isGround = false;
        isWater = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * power, ForceMode2D.Impulse);

        _animator.SetBool("isJumping", true);
    }

    void UpdateAnimation()
    {
        bool isWalking = isRunningAway && (isGround || isWater);
        _animator.SetBool("isWalking", isWalking);

        if ((isGround || isSliding) && jumpTimerCounter <= 0)
        {
            _animator.SetBool("isJumping", false);
        }
        // else
        // {
        //     _animator.SetBool("isJumping", true);
        // }

        // _animator.SetBool("isWalking", isWalking);
    }

    void CheckSurroundings()
    {
        if (jumpTimerCounter > 0)
        {
            isGround = false;
            return;
        }

        isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGround)
        {
            isWater = false;
        }
        else
        {
            isWater = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, waterLayer);
        }
    }

    void StopRunning()
    {
        // Hentikan X saja, Y biarkan (biar tetap jatuh kalau di udara)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        _animator.SetBool("isWalking", false);
    }

    void Flip(float velocityX)
    {
        // Jika bergerak ke kanan (velocity > 0), hadap kanan
        // Jika bergerak ke kanan (velocity < 0), hadap kiri

        Vector3 localScale = transform.localScale;

        if (velocityX > 0)
        {
            localScale.x = Mathf.Abs(localScale.x);
        }
        else if (velocityX < 0)
        {
            localScale.x = -Mathf.Abs(localScale.x);
        }

        transform.localScale = localScale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        float directionSign = Mathf.Sign(transform.localScale.x);
        Vector3 vecDir = Vector3.right * directionSign * obstacleCheckDistance;

        // Visualisasi sensor kaki (merah)
        if (footCheckPoint != null)
        {
            Gizmos.color = Color.red;
            // Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 0.5f);
            Gizmos.DrawLine(footCheckPoint.position, footCheckPoint.position + vecDir);
        }

        // Visualisasi sensor kepala (orange)
        if (headCheckPoint != null)
        {
            Gizmos.color = new Color(1, 0.5f, 0); // Orange
            Gizmos.DrawLine(headCheckPoint.position, headCheckPoint.position + vecDir);
        }
    }
}
