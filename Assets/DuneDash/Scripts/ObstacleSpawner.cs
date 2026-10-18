using UnityEngine;
namespace DuneDash
{
    /// <summary>Time-based fair spacing. Pattern gaps scale with speed, never below a full jump.</summary>
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        public bool SpawningEnabled { get; set; } = true;
        GameObject cactus, tallCactus, bird, coin;
        float timer;
        int wave;
        public void Initialize()
        {
            cactus = Resources.Load<GameObject>("Prefabs/Cactus");
            tallCactus = Resources.Load<GameObject>("Prefabs/CactusTall");
            bird = Resources.Load<GameObject>("Prefabs/Bird");
            coin = Resources.Load<GameObject>("Prefabs/Coin");
        }
        public void Begin() { timer = 1.6f; wave = 0; }
        void Update()
        {
            var game = GameManager.Instance;
            if (!game.IsRunning || !SpawningEnabled) return;
            timer -= Time.deltaTime;
            if (timer > 0) return;
            SpawnWave();
            timer = Mathf.Max(1.2f, 2.15f - game.Distance/1000f) + Random.Range(0,.35f);
        }
        void SpawnWave()
        {
            wave++;
            // Teach the first ground jumps before introducing flying enemies.
            bool flying = wave > 3 && wave % 3 == 0;
            float x = 12.5f;
            if (flying)
            {
                float y = wave % 2 == 0 ? -1.85f : -.15f;
                Instantiate(bird, new Vector3(x,y,0), Quaternion.identity, transform);
                // Ground coins under high birds explicitly reward staying low.
                for (int i=0; i<3; i++) SpawnCoin(new Vector3(x-2.8f+i*.9f, y > -1 ? -2.25f : 1.2f,0));
            }
            else
            {
                var prefab = wave > 2 && Random.value > .55f ? tallCactus : cactus;
                Instantiate(prefab, new Vector3(x,DinoController.GroundY,0), Quaternion.identity, transform);
                // Coin arc at the apex of the safe jump trajectory.
                for (int i=0;i<5;i++)
                    SpawnCoin(new Vector3(x-2+i, -.45f + Mathf.Sin(i/4f*Mathf.PI)*.55f,0));
            }
        }
        public GameObject SpawnHazard(bool flying, Vector3 position)
        { return Instantiate(flying ? bird : cactus,position,Quaternion.identity,transform); }
        public GameObject SpawnCoin(Vector3 position)
        { return Instantiate(coin,position,Quaternion.identity,transform); }
        public void Clear()
        {
            foreach (Transform child in transform)
            { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
    }
}
