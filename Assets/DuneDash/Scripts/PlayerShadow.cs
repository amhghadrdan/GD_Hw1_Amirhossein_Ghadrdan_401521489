using UnityEngine;
namespace DuneDash
{
    public sealed class PlayerShadow : MonoBehaviour
    {
        void LateUpdate()
        {
            var player = GameManager.Instance.Player;
            if (player == null) return;
            transform.position = new Vector3(player.transform.position.x,DinoController.GroundY+.035f,0);
            float size = Mathf.Clamp(1-(player.transform.position.y-DinoController.GroundY)*.15f,.35f,1);
            float menu = GameManager.Instance.State == GameState.MainMenu ? 2.15f : 1;
            transform.localScale = new Vector3(size*menu,size*menu,1);
        }
    }
}
