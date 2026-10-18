using UnityEngine;

namespace DuneDash
{
    /// <summary>Shared moving/interacting entity contract for hazards and collectibles.</summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public abstract class Interactable : MonoBehaviour
    {
        protected bool consumed;
        public virtual float SpeedMultiplier => 1f;
        protected virtual void Update()
        {
            var game = GameManager.Instance;
            if (game == null || !game.IsRunning) return;
            float dashBoost = game.Player.IsDashing ? 1.8f : 1f;
            transform.position += Vector3.left * (game.Speed * SpeedMultiplier * dashBoost * Time.deltaTime);
            if (transform.position.x < -13f) Destroy(gameObject);
        }
        public void TryInteract(DinoController player)
        {
            if (consumed || !GameManager.Instance.IsRunning) return;
            if (OnInteract(player))
            {
                consumed = true;
                GetComponent<Collider2D>().enabled = false;
                Destroy(gameObject);
            }
        }
        protected abstract bool OnInteract(DinoController player);
    }
}
