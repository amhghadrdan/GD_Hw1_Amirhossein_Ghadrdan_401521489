namespace DuneDash
{
    public sealed class GroundObstacle : Interactable
    {
        protected override bool OnInteract(DinoController player)
        {
            if (player.IsDashing) GameManager.Instance.Smash(transform.position + UnityEngine.Vector3.up*.7f);
            else player.TakeDamage();
            return true;
        }
    }
}
