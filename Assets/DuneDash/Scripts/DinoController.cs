using UnityEngine;

namespace DuneDash
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class DinoController : MonoBehaviour
    {
        public const float GroundY = -3f;
        public const float RunX = -6.8f;
        public int Lives { get; private set; } = 3;
        public bool IsDashing => dashRemaining > 0;
        public bool IsInvulnerable => IsDashing || invulnerability > 0;
        public bool IsGrounded => body.position.y <= GroundY + .065f && body.velocity.y <= .1f;
        public float DashCharge => 1f - Mathf.Clamp01(cooldown / DashCooldown);
        public const float DashCooldown = 2.2f;
        Rigidbody2D body;
        SpriteRenderer visual;
        Animator animator;
        float dashRemaining, cooldown, invulnerability, trailTimer, stepTimer;
        Vector2 menuPosition = new Vector2(4.7f, GroundY);
        static readonly int Grounded = Animator.StringToHash("Grounded");
        static readonly int Dead = Animator.StringToHash("Dead");
        static readonly int Hit = Animator.StringToHash("Hit");

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }
        public void ResetForRun()
        {
            Lives = 3; dashRemaining = cooldown = invulnerability = 0;
            transform.localScale = Vector3.one;
            transform.position = new Vector3(RunX, GroundY, 0);
            body.position = transform.position; body.velocity = Vector2.zero;
            body.gravityScale = 3.4f; body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            visual.color = Color.white;
            animator.speed = 1; animator.Rebind(); animator.Update(0); animator.SetBool(Dead, false); animator.SetBool(Grounded, true);
        }
        public void ShowOnMenu()
        {
            SetSimulation(false);
            transform.position = menuPosition; body.position = menuPosition;
            transform.localScale = Vector3.one * 2.15f;
            visual.color = Color.white;
            dashRemaining = cooldown = invulnerability = 0;
            animator.speed = 1; animator.Rebind(); animator.Update(0); animator.SetBool(Dead, false); animator.SetBool(Grounded, true);
        }
        public void SetSimulation(bool enabled)
        {
            body.simulated = enabled;
        }
        public bool Jump()
        {
            if (!GameManager.Instance.IsRunning || !IsGrounded) return false;
            body.velocity = new Vector2(0, 13.7f);
            animator.SetBool(Grounded, false);
            AudioDirector.Instance.Play("Jump");
            VisualEffects.Instance.Burst(transform.position + Vector3.up*.1f, new Color(.9f,.71f,.48f), 7);
            return true;
        }
        public bool Dash()
        {
            if (!GameManager.Instance.IsRunning || cooldown > 0) return false;
            dashRemaining = .32f; cooldown = DashCooldown;
            AudioDirector.Instance.Play("Dash");
            VisualEffects.Instance.Shake(.08f, .15f);
            return true;
        }
        void Update()
        {
            var game = GameManager.Instance;
            if (game == null || !game.IsRunning) return;
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
            dashRemaining = Mathf.Max(0, dashRemaining - Time.deltaTime);
            invulnerability = Mathf.Max(0, invulnerability - Time.deltaTime);
            animator.SetBool(Grounded, IsGrounded);
            animator.speed = IsDashing ? 1.8f : game.Speed / 7f;
            visual.color = invulnerability > 0 ? new Color(1,.45f,.35f, Mathf.Sin(invulnerability*38) > 0 ? .35f : 1f)
                : IsDashing ? new Color(1, .86f, .57f) : Color.white;
            trailTimer -= Time.deltaTime; stepTimer -= Time.deltaTime;
            if (IsDashing && trailTimer <= 0)
            {
                trailTimer = .045f;
                VisualEffects.Instance.Ghost(visual);
            }
            if (IsGrounded && stepTimer <= 0)
            {
                stepTimer = .14f;
                VisualEffects.Instance.Dust(transform.position + new Vector3(-.4f,.08f,0));
            }
        }
        void FixedUpdate()
        {
            if (!GameManager.Instance.IsRunning) return;
            // Protect against tiny penetrations at high speed and cap terminal fall velocity.
            if (body.position.y < GroundY) body.position = new Vector2(RunX, GroundY);
            if (body.velocity.y < -19f) body.velocity = new Vector2(0,-19f);
        }
        public bool TakeDamage()
        {
            if (!GameManager.Instance.IsRunning || IsInvulnerable) return false;
            Lives = Mathf.Max(0, Lives - 1);
            invulnerability = 1.35f;
            animator.SetTrigger(Hit);
            VisualEffects.Instance.Shake(.2f,.22f);
            VisualEffects.Instance.Burst(transform.position+Vector3.up, new Color(1,.47f,.34f), 14);
            AudioDirector.Instance.Play("Hit");
            if (Lives == 0)
            {
                animator.SetBool(Dead, true); animator.speed = 1;
                visual.color = Color.white;
                GameManager.Instance.EndRun();
            }
            return true;
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            var item = other.GetComponent<Interactable>();
            if (item != null) item.TryInteract(this);
        }
    }
}
