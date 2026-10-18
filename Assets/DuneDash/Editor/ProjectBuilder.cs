using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuneDash.Editor
{
    /// <summary>Creates real Animator, AnimationClip and Prefab assets on first import.</summary>
    [InitializeOnLoad]
    public static class ProjectBuilder
    {
        const string Root="Assets/DuneDash/";
        const string Prefabs=Root+"Resources/Prefabs/";
        const string ControllerPath=Root+"Resources/Dinosaur.controller";
        public const string ScenePath=Root+"Scenes/Desert.unity";
        static ProjectBuilder() { EditorApplication.delayCall+=OnFirstImport; }
        static void OnFirstImport()
        {
            if(EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!File.Exists(Prefabs+"Dinosaur.prefab") || !File.Exists(ControllerPath) || !File.Exists(ScenePath))
            {
                Generate();
                if(!Application.isBatchMode) EditorSceneManager.OpenScene(ScenePath);
            }
        }
        [MenuItem("Dune Dash/Generate or repair project assets")]
        public static void Generate()
        {
            Directory.CreateDirectory(Prefabs); Directory.CreateDirectory(Root+"Animations"); Directory.CreateDirectory(Root+"Scenes");
            AssetDatabase.Refresh();
            foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"Resources/Sprites"}))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                var artName=Path.GetFileNameWithoutExtension(importer.assetPath);
                Vector2 pivot=(artName.StartsWith("Dino") || artName.StartsWith("Cactus") || artName.EndsWith("Dunes")) ? new Vector2(.5f,0) : new Vector2(.5f,.5f);
                if(artName=="Ground") pivot=new Vector2(.5f,1);
                var textureSettings=new TextureImporterSettings();importer.ReadTextureSettings(textureSettings);
                if(importer.textureType!=TextureImporterType.Sprite || importer.filterMode!=FilterMode.Point || textureSettings.spriteAlignment!=(int)SpriteAlignment.Custom || textureSettings.spritePivot!=pivot)
                {
                    importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                    importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.spritePixelsPerUnit=32;
                    importer.ReadTextureSettings(textureSettings);textureSettings.spriteAlignment=(int)SpriteAlignment.Custom;textureSettings.spritePivot=pivot;importer.SetTextureSettings(textureSettings);
                    importer.SaveAndReimport();
                }
            }
            CreateAnimator();CreatePrefabs();
            if(!File.Exists(ScenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Dune Dash - Game Bootstrap",typeof(GameBootstrap));
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.companyName="HM1";PlayerSettings.productName="Dune Dash";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var activeInput=settings.FindProperty("activeInputHandler");if(activeInput!=null) activeInput.intValue=1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("DUNE DASH: project assets ready. Open Desert.unity and press Play.");
        }
        static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Resources/Sprites/"+name+".png");
        static AnimationClip Clip(string name,string[] sprites,float step,bool loop)
        {
            var path=Root+"Animations/"+name+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}
            clip.frameRate=12;
            var binding=new EditorCurveBinding {path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"};
            var frames=new ObjectReferenceKeyframe[sprites.Length+1];
            for(int i=0;i<sprites.Length;i++) frames[i]=new ObjectReferenceKeyframe {time=i*step,value=Art(sprites[i])};
            frames[sprites.Length]=new ObjectReferenceKeyframe {time=sprites.Length*step,value=Art(sprites[sprites.Length-1])};
            AnimationUtility.SetObjectReferenceCurve(clip,binding,frames);
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);
            EditorUtility.SetDirty(clip);return clip;
        }
        static void CreateAnimator()
        {
            if(File.Exists(ControllerPath)) AssetDatabase.DeleteAsset(ControllerPath);
            var controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter(new AnimatorControllerParameter {name="Grounded",type=AnimatorControllerParameterType.Bool,defaultBool=true});
            controller.AddParameter("Dead",AnimatorControllerParameterType.Bool);controller.AddParameter("Hit",AnimatorControllerParameterType.Trigger);
            var machine=controller.layers[0].stateMachine;
            var run=machine.AddState("Run",new Vector3(240,40));run.motion=Clip("Run",new[]{"DinoRun0","DinoRun1","DinoRun2","DinoRun3"},.09f,true);
            var jump=machine.AddState("Jump",new Vector3(490,40));jump.motion=Clip("Jump",new[]{"DinoJump"},.15f,true);
            var hit=machine.AddState("Hit",new Vector3(240,180));hit.motion=Clip("Hit",new[]{"DinoHit","DinoRun0","DinoHit"},.085f,false);
            var death=machine.AddState("Death",new Vector3(490,180));death.motion=Clip("Death",new[]{"DinoHit","DinoDeath","DinoDeath"},.13f,false);
            machine.defaultState=run;
            var toDeath=machine.AddAnyStateTransition(death);Configure(toDeath);toDeath.AddCondition(AnimatorConditionMode.If,0,"Dead");
            var toHit=machine.AddAnyStateTransition(hit);Configure(toHit);toHit.AddCondition(AnimatorConditionMode.If,0,"Hit");toHit.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
            var takeoff=run.AddTransition(jump);Configure(takeoff);takeoff.AddCondition(AnimatorConditionMode.IfNot,0,"Grounded");
            var landing=jump.AddTransition(run);Configure(landing);landing.AddCondition(AnimatorConditionMode.If,0,"Grounded");
            var recoverGround=hit.AddTransition(run);Configure(recoverGround);recoverGround.hasExitTime=true;recoverGround.exitTime=1;recoverGround.AddCondition(AnimatorConditionMode.If,0,"Grounded");recoverGround.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
            var recoverAir=hit.AddTransition(jump);Configure(recoverAir);recoverAir.hasExitTime=true;recoverAir.exitTime=1;recoverAir.AddCondition(AnimatorConditionMode.IfNot,0,"Grounded");recoverAir.AddCondition(AnimatorConditionMode.IfNot,0,"Dead");
            EditorUtility.SetDirty(controller);
        }
        static void Configure(AnimatorStateTransition t) {t.hasExitTime=false;t.duration=0;t.canTransitionToSelf=false;}
        static void CreatePrefabs()
        {
            var dino=new GameObject("Dinosaur");
            var rb=dino.AddComponent<Rigidbody2D>();rb.gravityScale=3.4f;rb.freezeRotation=true;
            rb.constraints=RigidbodyConstraints2D.FreezeRotation|RigidbodyConstraints2D.FreezePositionX;
            rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
            var box=dino.AddComponent<BoxCollider2D>();box.size=new Vector2(.95f,1.66f);box.offset=new Vector2(.05f,.83f);
            var material=new PhysicsMaterial2D("Runner frictionless"){friction=0,bounciness=0};
            const string materialPath=Root+"Resources/Runner.physicsMaterial2D";
            var stored=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
            if(stored==null){AssetDatabase.CreateAsset(material,materialPath);stored=material;}else UnityEngine.Object.DestroyImmediate(material);
            box.sharedMaterial=stored;
            var visual=new GameObject("Visual");visual.transform.SetParent(dino.transform,false);
            var sr=visual.AddComponent<SpriteRenderer>();sr.sprite=Art("DinoRun0");sr.sortingOrder=10;
            var animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            dino.AddComponent<DinoController>();Save(dino,"Dinosaur");
            Hazard<GroundObstacle>("Cactus","Cactus",new Vector2(.72f,1.28f),new Vector2(0,.68f));
            Hazard<GroundObstacle>("CactusTall","CactusTall",new Vector2(.72f,1.72f),new Vector2(0,.9f));
            Hazard<FlyingObstacle>("Bird","Bird0",new Vector2(1.36f,.63f),Vector2.zero);
            Hazard<Coin>("Coin","Coin",new Vector2(.62f,.75f),Vector2.zero);
        }
        static void Hazard<T>(string name,string art,Vector2 size,Vector2 offset) where T:Interactable
        {
            var go=new GameObject(name);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=Art(art);sr.sortingOrder=9;
            var box=go.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=size;box.offset=offset;
            go.AddComponent<T>();Save(go,name);
        }
        static void Save(GameObject go,string name)
        {PrefabUtility.SaveAsPrefabAsset(go,Prefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(go);}
        [MenuItem("Dune Dash/Build Linux game")]
        public static void BuildLinux() {Build(BuildTarget.StandaloneLinux64,"Builds/Linux/DuneDash.x86_64");}
        [MenuItem("Dune Dash/Build Windows game")]
        public static void BuildWindows() {Build(BuildTarget.StandaloneWindows64,"Builds/Windows/DuneDash.exe");}
        public static void BuildFromCommandLine()
        {
            var args=Environment.GetCommandLineArgs();
            bool windows=Array.IndexOf(args,"-windows")>=0;
            Build(windows?BuildTarget.StandaloneWindows64:BuildTarget.StandaloneLinux64,windows?"Builds/Windows/DuneDash.exe":"Builds/Linux/DuneDash.x86_64");
        }
        static void Build(BuildTarget target,string output)
        {
            Generate();Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName=output,target=target,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build failed: "+report.summary.result);
            Debug.Log("DUNE DASH BUILD SUCCESS: "+output+" ("+report.summary.totalSize+" bytes)");
        }
    }
}
