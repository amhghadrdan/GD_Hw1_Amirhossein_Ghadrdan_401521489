using UnityEngine;
namespace DuneDash
{
    public sealed class FlyingObstacle : Interactable
    {
        public override float SpeedMultiplier => 1.1f;
        SpriteRenderer visual;
        Sprite[] frames;
        float age;
        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            frames = new[] { Resources.Load<Sprite>("Sprites/Bird0"), Resources.Load<Sprite>("Sprites/Bird1") };
        }
        protected override void Update()
        {
            base.Update();
            if (GameManager.Instance.IsRunning)
            {
                age += Time.deltaTime;
                visual.sprite = frames[(int)(age * 7) % 2];
            }
        }
        protected override bool OnInteract(DinoController player)
        {
            if (player.IsDashing) GameManager.Instance.Smash(transform.position);
            else player.TakeDamage();
            return true;
        }
    }
}
