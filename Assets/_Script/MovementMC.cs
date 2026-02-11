using System.Collections;
using UnityEngine;

public class MovementMC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpPower = 12f;
    public float doubleJumpPower = 8f;
    public float waterSpeedMultiplier = 0.6f;

    [Header("Slide Settings")]
    public float slideSpeedMultiplier = 2.0f;
    public float slideDuration = 0.8f;
    public float slideCooldown = 1.0f;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Collision Checks")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask waterLayer;

    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer sr;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;

    // State Variable
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGround;
    private bool isWater;

    // State Khusus
    private bool isSliding;
    private bool canSlide = true;
    private bool canDoubleJump;

    // Timer Lompat (Anti Spam)
    private float jumpBufferTime = 0.2f;
    private float jumpTimerCounter;

    // Collider Size
    public Vector2 slideSize = new Vector2(11.31f, 4.37f);
    public Vector2 slideOffset = new Vector2(0f, -1.2f);
    private Vector2 standSize;
    private Vector2 standOffset;

    // private float playerHalfHeight;
    // private bool canDoubleJump;
    // public Vector2 boxSize;
    // public float castDistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();

        // Simpan ukuran asli saat berdiri
        standSize = coll.size;
        standOffset = coll.offset;
    }

    void Start()
    {
        // isGround = true;
        // isWater = false;
        // playerHalfHeight = sr.bounds.extents.y;
    }

    void Update()
    {
        // moveSpeed += acceleration * Time.deltaTime;
        // transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
        if (jumpTimerCounter > 0)
        {
            jumpTimerCounter -= Time.deltaTime;
        }

        // 1. Check Surroundings
        CheckSurroundings();

        // 2. Input Movement
        if (!isSliding)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        // if (horizontalInput != 0)
        // {
        //     _animator.SetBool("isWalking", true);
        // }
        // else
        // {
        //     _animator.SetBool("isWalking", false);
        // }

        // 3. Input Jump
        if (Input.GetKeyDown(KeyCode.Space) && !isSliding)
        {
            // Kondisi 1: Lompat dari Tanah/Air
            if (isGround || isWater)
            {
                Jump(jumpPower);
                canDoubleJump = true;
            }
            // Kondisi 2: Double Jump di Udara
            else if (canDoubleJump)
            {
                Jump(doubleJumpPower);
                canDoubleJump = false;

                // Opsional: Reset animasi biar kelihatan lompat lagi
                // _animator.Play("mc jump", -1, 0f);
            }
            
        }

        // 3. Input Slide
        if (Input.GetKeyDown(slideKey) && isGround && !isSliding && canSlide)
        {
            // StartSlide();
            StartCoroutine(SlideProcess());
            // _animator.SetBool("isSliding", true);
        }
        // else
        // {
            // coll.size = standSize;
            // coll.offset = standOffset;
            // _animator.SetBool("isSliding", false);
            // float oriSpeed = moveSpeed;
            // StopSlide();
        // }

        // 4. Animation Setup
        UpdateAnimation();

        // 5. Flip Character
        if (!isSliding) Flip();

        // CheckIsWater();
        // CheckIsGround();
        // Debug.DrawRay(transform.position, Vector2.down * (playerHalfHeight + 0.1f), Color.red);

    }

    void FixedUpdate()
    {
        float currentSpeed = moveSpeed;
        float inputDirection = horizontalInput;

        if (isSliding)
        {
            // Paksa gerak ke arah hadap (Facing Right = 1, Left = -1)
            // Jadi walau tombol dilepas, dia tetap meluncur
            inputDirection = isFacingRight ? 1f : -1f;
            currentSpeed *= slideSpeedMultiplier;
        }
        else if (isWater)
        {
            currentSpeed *= waterSpeedMultiplier;
        }

        // Gerakkan karakter
        // NOTE: Jika error di Unity versi lama, ganti 'linearVelocity' jadi 'velocity'
        rb.linearVelocity = new Vector2(inputDirection * currentSpeed, rb.linearVelocity.y);
    }

    IEnumerator SlideProcess()
    {
        // A. Mulai slide
        isSliding = true;
        canSlide = false;

        // Ubah bentuk collider jadi gepeng
        coll.size = slideSize;
        coll.offset = slideOffset;
        coll.direction = CapsuleDirection2D.Horizontal;

        // Mainkan animasi
        _animator.SetBool("isSliding", true);

        // B. Tunggu selama durasi (misal 0.8 detik)
        yield return new WaitForSeconds(slideDuration);

        // C. Selesai Slide (Kembali Berdiri)
        coll.size = standSize;
        coll.offset = standOffset;
        coll.direction = CapsuleDirection2D.Vertical;
        _animator.SetBool("isSliding", false);
        isSliding = false;

        // D. Cooldown (Opsional: Tunggu sebentar sebelum bisa slide lagi)
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }

    void Jump(float power)
    {
            // rb.linearVelocityY = jumpPower;
            jumpTimerCounter = jumpBufferTime;

            isGround = false;
            isWater = false;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * power, ForceMode2D.Impulse);

            _animator.SetBool("isJumping", true);
    }

    void UpdateAnimation()
    {
        // Set animasi jalan hanya jika ada input dan di tanah
        bool isWalking = horizontalInput != 0 && (isGround || isWater);
        _animator.SetBool("isWalking", isWalking);

        if ((isGround || isSliding) && jumpTimerCounter <= 0)
        {
            _animator.SetBool("isJumping", false);
        }
    }


    void CheckSurroundings()
    {
        // Gunakan OverlapCircle untuk keduanya agar konsisten (dari kaki)

        // Kalau timer lompat masih jalan, paksa isGround jadi false (pura-pura melayang)
        if (jumpTimerCounter > 0)
        {
            isGround = false;
            // isWater = false;
            return;
        }

        // 1. Cek Tanah
        isGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 2. Cek Air
        // Prioritaskan tanah, Jika napak tanah, anggap tidak di air (meski collider air nempel)
        if (isGround)
        {
            isWater = false;
            canDoubleJump = true;
        }
        else
        {
            isWater = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, waterLayer);
            if (isWater) canDoubleJump = true;
        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localeScale = transform.localScale;
            localeScale.x *= -1f;
            transform.localScale = localeScale;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
