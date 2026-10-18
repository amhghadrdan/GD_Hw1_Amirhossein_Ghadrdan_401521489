using System.Collections.Generic;
using UnityEngine;
namespace DuneDash
{
    public sealed class VisualEffects : MonoBehaviour
    {
        public static VisualEffects Instance { get; private set; }
        sealed class Particle
        {
            public SpriteRenderer sprite; public Vector3 velocity; public float life, duration; public bool gravity; public Color color;
        }
        readonly List<Particle> particles = new List<Particle>();
        Camera gameCamera;
        float shakeTime, shakeStrength;
        Sprite pixel, spark;
        void Awake()
        {
            Instance = this; gameCamera = Camera.main;
            pixel = Resources.Load<Sprite>("Sprites/Pixel"); spark = Resources.Load<Sprite>("Sprites/Spark");
        }
        void Add(Vector3 position, Sprite art, Vector3 scale, Color color, Vector3 velocity, float life, bool gravity)
        {
            if (particles.Count > 140) return;
            var obj = new GameObject("Particle"); obj.transform.SetParent(transform);
            obj.transform.position = position; obj.transform.localScale = scale;
            var sr = obj.AddComponent<SpriteRenderer>(); sr.sprite = art; sr.sortingOrder = 25; sr.color = color;
            particles.Add(new Particle {sprite=sr,velocity=velocity,life=life,duration=life,gravity=gravity,color=color});
        }
        public void Burst(Vector3 position, Color color, int count)
        {
            for (int i=0;i<count;i++)
                Add(position,pixel,Vector3.one*Random.Range(.22f,.5f),color,new Vector3(Random.Range(-3f,3f),Random.Range(1f,5f),0),Random.Range(.25f,.6f),true);
            Add(position,spark,Vector3.one*.55f,new Color(1,.94f,.73f),Vector3.up,.28f,false);
        }
        public void Dust(Vector3 position)
        { Add(position,pixel,new Vector3(.4f,.25f,1),new Color(.73f,.48f,.32f,.5f),new Vector3(-2,.5f,0),.28f,false); }
        public void Ghost(SpriteRenderer source)
        { Add(source.transform.position,source.sprite,source.transform.lossyScale,new Color(1,.64f,.34f,.38f),Vector3.left*2.5f,.22f,false); }
        public void Shake(float strength, float time)
        { shakeStrength = strength; shakeTime = time; }
        void LateUpdate()
        {
            float dt = Time.deltaTime;
            for (int i=particles.Count-1;i>=0;i--)
            {
                var p = particles[i]; p.life -= dt;
                if (p.life <= 0 || p.sprite == null)
                { if (p.sprite != null) Destroy(p.sprite.gameObject); particles.RemoveAt(i); continue; }
                if (p.gravity) p.velocity += Vector3.down*12*dt;
                p.sprite.transform.position += p.velocity*dt;
                p.sprite.color = new Color(p.color.r,p.color.g,p.color.b,p.color.a*p.life/p.duration);
            }
            if (gameCamera == null) return;
            shakeTime = Mathf.Max(0,shakeTime-dt);
            gameCamera.transform.position = new Vector3(shakeTime > 0 ? Random.Range(-1f,1f)*shakeStrength : 0,
                shakeTime > 0 ? Random.Range(-1f,1f)*shakeStrength : 0,-10);
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
