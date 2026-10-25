using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DuneDash
{
    /// <summary>Builds the scene from authored Resources/prefabs so a fresh clone is immediately playable.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
            Physics2D.gravity = new Vector2(0,-9.81f);
            var cameraObject = new GameObject("Game Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 6;
            camera.backgroundColor = Hex("DCE9EC"); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(0,0,-10);
            camera.allowHDR = false; camera.allowMSAA = false;
            var game = gameObject.AddComponent<GameManager>();
            gameObject.AddComponent<AudioDirector>();
            gameObject.AddComponent<VisualEffects>();
            BuildEnvironment();
            var player = Instantiate(Resources.Load<GameObject>("Prefabs/Dinosaur"),new Vector3(DinoController.RunX,DinoController.GroundY,0),Quaternion.identity).GetComponent<DinoController>();
            player.name = "Dinosaur";
            var shadow = Sprite("Player Shadow", "Shadow", new Vector3(DinoController.RunX,-2.965f,0),-1);
            shadow.gameObject.AddComponent<PlayerShadow>();
            var spawner = new GameObject("Interactables").AddComponent<ObstacleSpawner>(); spawner.Initialize();
            var canvasObject = new GameObject("Game Canvas", typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
            var events = new GameObject("Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var ui = canvasObject.AddComponent<GameUI>(); ui.Initialize(game);
            game.Initialize(player,ui,spawner);
            gameObject.AddComponent<PlayerInputRouter>();
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-smokeTest") >= 0)
                gameObject.AddComponent<RuntimeSmokeTests>();
        }
        void BuildEnvironment()
        {
            Sprite("Ringed coastal moon","Sun",new Vector3(5.6f,2.4f,0),-90).transform.localScale = Vector3.one*1.3f;
            Layer("Clouds","Cloud",new Vector3(0,3,0),.065f,-85,18);
            Layer("Far dunes","FarDunes",new Vector3(0,-3,0),.12f,-80,24);
            Layer("Middle dunes","MidDunes",new Vector3(0,-3.3f,0),.28f,-70,24);
            Layer("Near dunes","NearDunes",new Vector3(0,-4,0),.48f,-60,24);
            var sand = Sprite("Sand fill","Pixel",new Vector3(0,-6,0),-12);
            sand.color = Hex("87A5B0"); sand.transform.localScale = new Vector3(120,24,1);
            Layer("Ground","Ground",new Vector3(0,DinoController.GroundY,0),1,-5,8);
            var ground = new GameObject("Ground collider",typeof(BoxCollider2D));
            ground.transform.position = new Vector3(0,DinoController.GroundY-.25f,0);
            ground.GetComponent<BoxCollider2D>().size = new Vector2(100,.5f);
            // Small rock silhouettes make the foreground parallax depth easy to see.
            var plants = new GameObject("Distant crystals");
            for(int i=0;i<3;i++)
            {
                var p = Sprite("Rock silhouette","CactusTall",new Vector3(-8+i*8,-3.05f,0),-40);
                p.color = Hex("7895A2"); p.transform.localScale = Vector3.one*.38f; p.transform.SetParent(plants.transform);
            }
            plants.AddComponent<ParallaxLayer>().factor = .7f;
        }
        static void Layer(string name,string art,Vector3 position,float factor,int sorting,float width)
        {
            var root = new GameObject(name);
            var layer = root.AddComponent<ParallaxLayer>(); layer.factor=factor; layer.width=width;
            for(int i=-1;i<=1;i++)
            {
                var tile = Sprite(name+" tile",art,position+Vector3.right*width*i,sorting);
                tile.transform.SetParent(root.transform);
            }
        }
        public static SpriteRenderer Sprite(string name,string art,Vector3 position,int sorting)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.sprite=Resources.Load<Sprite>("Sprites/"+art);sr.sortingOrder=sorting;sr.transform.position=position; return sr;
        }
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c); return c; }
    }
}
