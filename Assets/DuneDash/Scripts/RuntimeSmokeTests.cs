using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DuneDash
{
    /// <summary>Opt-in end-to-end checks. Run with -smokeTest. Uses isolated PlayerPrefs.</summary>
    public sealed class RuntimeSmokeTests : MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        string folder;
        Keyboard keyboard;
        GameManager game;
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,Application.isEditor ? "../Documentation" : "../../../Documentation"));
            if(!Directory.Exists(folder)) folder=Path.Combine(Application.persistentDataPath,"Verification");
            Directory.CreateDirectory(Path.Combine(folder,"Screenshots"));
            game=GameManager.Instance;
            keyboard=InputSystem.AddDevice<Keyboard>();
            yield return null;
            yield return new WaitForSecondsRealtime(.3f);
            Check("Main menu on launch",game.State==GameState.MainMenu);
            yield return Capture("01-main-menu");
            yield return Press(Key.Space);
            Check("Space starts run through New Input System",game.IsRunning && game.Player.Lives==3);
            game.Spawner.SpawningEnabled=false;
            float start=game.Distance;
            yield return new WaitForSeconds(.25f);
            Check("Distance and live score advance",game.Distance>start && game.Score>0);
            yield return Press(Key.Space);
            yield return new WaitForSeconds(.12f);
            Check("Space jumps",game.Player.transform.position.y>DinoController.GroundY+.3f);
            Check("Animator Jump",game.Player.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Jump"));
            float velocity=game.Player.GetComponent<Rigidbody2D>().velocity.y;
            Check("No double jump",!game.Player.Jump() && game.Player.GetComponent<Rigidbody2D>().velocity.y==velocity);
            yield return Capture("02-jump");
            yield return new WaitForSeconds(.85f);
            Check("Returns to ground",game.Player.IsGrounded);
            yield return Press(Key.LeftShift);
            Check("Shift activates dash",game.Player.IsDashing && game.Player.DashCharge<1);
            Check("Dash cannot spam",!game.Player.Dash());
            int beforeSmashScore=game.Score;
            game.Spawner.SpawnHazard(false,game.Player.transform.position+Vector3.right*.55f);
            yield return new WaitForSeconds(.05f);
            Check("Dash smashes obstacles without damage",game.Player.Lives==3 && game.Score>=beforeSmashScore+5);
            yield return Capture("03-dash");
            yield return new WaitForSeconds(.35f);
            int score=game.Score;
            game.Spawner.SpawnCoin(game.Player.transform.position+Vector3.up*.8f);
            yield return new WaitForSeconds(.08f);
            Check("Coin trigger awards 25",game.Coins==1 && game.Score>=score+25);
            yield return Press(Key.Escape);
            Check("Escape opens pause",game.State==GameState.Paused && Time.timeScale==0);
            float frozen=game.Distance;
            yield return new WaitForSecondsRealtime(.2f);
            Check("Pause freezes simulation",game.Distance==frozen);
            yield return Capture("04-pause");
            yield return Press(Key.Escape);
            Check("Escape resumes",game.IsRunning && Time.timeScale==1);
            game.Spawner.SpawnHazard(false,game.Player.transform.position);
            yield return new WaitForSeconds(.08f);
            Check("Ground hazard removes one life",game.Player.Lives==2);
            Check("Hit protection prevents repeat damage",!game.Player.TakeDamage() && game.Player.Lives==2);
            yield return Capture("05-hit");
            yield return new WaitForSeconds(1.45f);
            game.Spawner.SpawnHazard(true,game.Player.transform.position+Vector3.up*.8f);
            yield return new WaitForSeconds(.08f);
            Check("Flying hazard removes one life",game.Player.Lives==1);
            yield return new WaitForSeconds(1.45f);
            game.Spawner.SpawnHazard(false,game.Player.transform.position);
            yield return new WaitForSeconds(.1f);
            Check("Third hit ends run",game.Player.Lives==0 && game.State==GameState.Dying);
            yield return new WaitForSecondsRealtime(.9f);
            Check("Game Over screen",game.State==GameState.GameOver);
            Check("Animator Death",game.Player.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Death"));
            Check("Record persisted with PlayerPrefs",PlayerPrefs.GetInt(game.PersistenceKey,0)==game.HighScore && game.HighScore>0);
            yield return Capture("06-game-over");
            int record=game.HighScore;
            yield return Press(Key.R);
            game.Spawner.SpawningEnabled=false;
            Check("R restarts with three lives",game.IsRunning && game.Player.Lives==3 && game.Coins==0);
            Check("Record survives restart",game.HighScore==record);
            yield return Press(Key.Escape);
            game.ReturnToMenu();
            Check("Main menu resets pause time",game.State==GameState.MainMenu && Time.timeScale==1);
            game.StartRun();game.Spawner.SpawningEnabled=true;
            yield return new WaitForSeconds(.1f);
            // A staged gameplay screenshot includes both hazard classes and collectibles.
            game.Spawner.SpawningEnabled=false;
            game.Spawner.SpawnHazard(false,new Vector3(1,-3,0));
            game.Spawner.SpawnHazard(true,new Vector3(7,-.3f,0));
            for(int i=0;i<4;i++) game.Spawner.SpawnCoin(new Vector3(-1+i,-.1f+Mathf.Sin(i)*.3f,0));
            yield return Capture("07-gameplay");
            Check("Parallax ground covers the viewport continuously",GroundIsContinuous());
            var text=string.Join("\n",results)+"\n";
            File.WriteAllText(Path.Combine(folder,"test-results.txt"),text);
            Debug.Log(text);
            bool passed=!text.Contains("FAIL");
            game.SuppressRecordWrites=true;
            PlayerPrefs.DeleteKey(game.PersistenceKey);PlayerPrefs.Save();
            InputSystem.RemoveDevice(keyboard);
            Application.Quit(passed?0:1);
        }
        IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));
            yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            yield return null;yield return null;
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Screenshots",name+".png"));
            yield return null;
        }
        bool GroundIsContinuous()
        {
            var renderers=GameObject.Find("Ground").GetComponentsInChildren<SpriteRenderer>();
            Array.Sort(renderers,(a,b)=>a.bounds.min.x.CompareTo(b.bounds.min.x));
            var camera=Camera.main;
            float left=camera.transform.position.x-camera.orthographicSize*camera.aspect;
            float right=camera.transform.position.x+camera.orthographicSize*camera.aspect;
            float covered=left;
            foreach(var renderer in renderers)
            {
                var bounds=renderer.bounds;
                if(bounds.max.x<left) continue;
                if(bounds.min.x>covered+.02f) return false;
                covered=Mathf.Max(covered,bounds.max.x);
                if(covered>=right-.02f) return true;
            }
            return false;
        }
        void Check(string name,bool passed)
        {
            string result=(passed?"PASS: ":"FAIL: ")+name;
            results.Add(result);if(!passed) Debug.LogError(result);else Debug.Log(result);
        }
    }
}
