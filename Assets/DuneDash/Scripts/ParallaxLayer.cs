using UnityEngine;
namespace DuneDash
{
    public sealed class ParallaxLayer : MonoBehaviour
    {
        public float factor = .2f;
        public float width = 24;
        void Update()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            float visibleWidth=Camera.main.orthographicSize*2*Camera.main.aspect;
            int needed=Mathf.Max(3,Mathf.CeilToInt(visibleWidth/width)+2);
            // Keep an extra tile on each side, including after a window resize.
            while(transform.childCount<needed)
            {
                float rightmost=float.NegativeInfinity;
                foreach(Transform child in transform) rightmost=Mathf.Max(rightmost,child.position.x);
                var tile=Instantiate(transform.GetChild(0).gameObject,transform);
                var position=tile.transform.position;position.x=rightmost+width;tile.transform.position=position;
            }
            float speed = game.IsRunning ? game.Speed * (game.Player.IsDashing ? 1.8f : 1f) : game.State == GameState.MainMenu ? .9f : 0;
            foreach (Transform tile in transform)
            {
                tile.position += Vector3.left * (speed * factor * Time.deltaTime);
                if (tile.position.x < -visibleWidth*.5f-width*.5f) tile.position += Vector3.right * width*transform.childCount;
            }
        }
    }
}
