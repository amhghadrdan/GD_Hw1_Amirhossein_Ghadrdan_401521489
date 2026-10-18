using UnityEngine;
namespace DuneDash
{
    public sealed class Coin : Interactable
    {
        float age;
        protected override void Update()
        {
            base.Update();
            if (!GameManager.Instance.IsRunning) return;
            age += Time.deltaTime;
            transform.localScale = new Vector3(.65f + .35f * Mathf.Abs(Mathf.Cos(age*4f)), 1, 1);
        }
        protected override bool OnInteract(DinoController player)
        {
            GameManager.Instance.AddCoin(transform.position);
            return true;
        }
    }
}
