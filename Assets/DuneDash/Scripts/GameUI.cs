using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DuneDash
{
    /// <summary>Canvas-based HUD plus independent main, pause, help and game-over menus.</summary>
    public sealed class GameUI : MonoBehaviour
    {
        static readonly Color Ink = GameBootstrap.Hex("23464B"), Cream = GameBootstrap.Hex("FFF1D5"), Coral = GameBootstrap.Hex("DF785B"), Muted = GameBootstrap.Hex("778A7E");
        GameManager game;
        Font body, display;
        GameObject main, hud, pause, over, help;
        Text score, best, menuBest, healthLabel, dashLabel, finalScore, finalBest, finalCoins, muteLabel;
        Image[] hearts = new Image[3]; Image dashFill;
        Button startButton, resumeButton, retryButton, helpBack;
        public bool IsHelpOpen => help != null && help.activeSelf;
        readonly List<Toast> toasts = new List<Toast>();
        sealed class Toast { public RectTransform rect; public Text text; public float time; }
        public void Initialize(GameManager manager)
        {
            game = manager;
            body = Resources.Load<Font>("Fonts/Body"); display = Resources.Load<Font>("Fonts/Display");
            BuildMain(); BuildHud(); BuildPause(); BuildOver(); BuildHelp();
            var sound = Button(transform,"Sound",new Vector2(-138,-26),new Vector2(100,32),"SOUND ON",()=>AudioDirector.Instance.ToggleMute(),false,new Vector2(1,1));
            sound.GetComponent<Image>().color = new Color(1,.95f,.83f,.8f);
            muteLabel = sound.GetComponentInChildren<Text>(); muteLabel.fontSize=12;
        }
        void BuildMain()
        {
            main = Group("Main Menu");
            Label(main.transform,"Edition",new Vector2(80,-48),new Vector2(500,30),"DUNE DASH   /   AN ENDLESS DESERT RUN",15,Ink,true);
            Label(main.transform,"Eyebrow",new Vector2(83,-157),new Vector2(450,25),"SMALL DINO. BIG ADVENTURE.",15,Ink,true);
            var title=Label(main.transform,"Title",new Vector2(74,-197),new Vector2(600,205),"DUNE\nDASH.",91,Ink,true);title.lineSpacing=.82f;
            Label(main.transform,"Description",new Vector2(83,-410),new Vector2(520,60),"One little dinosaur. An endless horizon.\nJump, dash, and make every life count.",21,Ink);
            startButton=Button(main.transform,"Start Run",new Vector2(83,-496),new Vector2(215,58),"START RUN    >",()=>game.StartRun());
            Button(main.transform,"How To Play",new Vector2(312,-496),new Vector2(160,58),"HOW TO PLAY",ShowHelp,false);
            Button(main.transform,"Quit",new Vector2(486,-496),new Vector2(90,58),"QUIT",()=>game.Quit(),false);
            Label(main.transform,"Controls",new Vector2(84,-594),new Vector2(600,28),"SPACE  jump       SHIFT  dash       ESC  pause",16,Ink,true);
            Label(main.transform,"Credit",new Vector2(84,30),new Vector2(700,22),"01 / THE LONG WAY HOME                                 ORIGINAL PIXEL ART + SOUND",12,Ink,false,new Vector2(0,0));
            menuBest=Label(main.transform,"Best record",new Vector2(-278,-80),new Vector2(240,45),"PERSONAL BEST  00000",15,Ink,true,new Vector2(1,1));
            Label(main.transform,"Landscape caption",new Vector2(-350,-573),new Vector2(340,25),"THE DESERT IS CALLING.",15,Ink,true,new Vector2(1,1));
        }
        void BuildHud()
        {
            hud=Group("HUD");
            var top=Box(hud.transform,"Header",new Vector2(0,0),new Vector2(0,108),new Color(1,.95f,.84f,.92f),new Vector2(0,1));
            top.anchorMin=new Vector2(0,1);top.anchorMax=Vector2.one;top.offsetMin=new Vector2(0,-108);top.offsetMax=Vector2.zero;
            Label(hud.transform,"Brand",new Vector2(38,-22),new Vector2(240,28),"DUNE / DASH",21,Ink,true);
            for(int i=0;i<3;i++) hearts[i]=Box(hud.transform,"Life "+(i+1),new Vector2(40+i*51,-65),new Vector2(42,13),Coral).GetComponent<Image>();
            healthLabel=Label(hud.transform,"Health",new Vector2(203,-61),new Vector2(95,25),"3 / 3",14,Ink,true);
            Label(hud.transform,"Score label",new Vector2(-525,-23),new Vector2(130,20),"CURRENT SCORE",12,Muted,true,new Vector2(1,1));
            score=Label(hud.transform,"Current Score",new Vector2(-528,-45),new Vector2(170,46),"00000",34,Ink,true,new Vector2(1,1));
            Label(hud.transform,"Best label",new Vector2(-300,-23),new Vector2(130,20),"PERSONAL BEST",12,Muted,true,new Vector2(1,1));
            best=Label(hud.transform,"High Score",new Vector2(-303,-45),new Vector2(170,46),"00000",34,Ink,true,new Vector2(1,1));
            Button(hud.transform,"Pause",new Vector2(-128,-65),new Vector2(90,27),"II  PAUSE",()=>game.TogglePause(),false,new Vector2(1,1));
            Label(hud.transform,"Control hints",new Vector2(40,48),new Vector2(660,23),"SPACE  jump       SHIFT  dash       ESC  pause",15,Ink,true,new Vector2(0,0));
            dashLabel=Label(hud.transform,"Dash status",new Vector2(-240,67),new Vector2(205,23),"DASH READY",13,Ink,true,new Vector2(1,0));
            Box(hud.transform,"Dash track",new Vector2(-240,40),new Vector2(205,9),new Color(.14f,.27f,.29f,.2f),new Vector2(1,0));
            dashFill=Box(hud.transform,"Dash charge",new Vector2(-240,40),new Vector2(205,9),Ink,new Vector2(1,0)).GetComponent<Image>();
        }
        void BuildPause()
        {
            pause=Overlay("Pause Menu");
            var panel=Card(pause.transform,520,460);
            CenterLabel(panel,"Pause eyebrow",new Vector2(0,174),new Vector2(440,26),"TAKE A BREATHER",14,Coral,true);
            CenterLabel(panel,"Pause title",new Vector2(0,112),new Vector2(440,64),"PAUSED",49,Cream,true);
            CenterLabel(panel,"Pause description",new Vector2(0,48),new Vector2(440,60),"The horizon can wait.\nYour run is right where you left it.",19,Cream);
            resumeButton=CenterButton(panel,"Resume",new Vector2(0,-34),new Vector2(356,53),"CONTINUE RUN",()=>game.Resume());
            CenterButton(panel,"Restart",new Vector2(0,-100),new Vector2(356,46),"RESTART",()=>game.StartRun(),false);
            CenterButton(panel,"Main Menu",new Vector2(0,-157),new Vector2(356,46),"MAIN MENU",()=>game.ReturnToMenu(),false);
        }
        void BuildOver()
        {
            over=Overlay("Game Over Menu");
            var panel=Card(over.transform,550,490);
            CenterLabel(panel,"Over eyebrow",new Vector2(0,194),new Vector2(490,24),"EVERY RUN IS A NEW STORY",14,Coral,true);
            CenterLabel(panel,"Over title",new Vector2(0,136),new Vector2(490,64),"RUN COMPLETE",43,Cream,true);
            finalScore=CenterLabel(panel,"Final score",new Vector2(0,57),new Vector2(490,75),"00000",57,Cream,true);
            finalCoins=CenterLabel(panel,"Coins collected",new Vector2(0,-2),new Vector2(490,27),"0 COINS COLLECTED",15,Cream);
            finalBest=CenterLabel(panel,"Final best",new Vector2(0,-39),new Vector2(490,25),"PERSONAL BEST  00000",15,Coral,true);
            retryButton=CenterButton(panel,"Play Again",new Vector2(0,-104),new Vector2(372,54),"RUN AGAIN    /    R",()=>game.StartRun());
            CenterButton(panel,"Back to menu",new Vector2(0,-171),new Vector2(372,46),"MAIN MENU",()=>game.ReturnToMenu(),false);
        }
        void BuildHelp()
        {
            help=Overlay("How To Play"); var panel=Card(help.transform,660,550);
            CenterLabel(panel,"Help title",new Vector2(0,213),new Vector2(590,55),"A FEW DESERT RULES",34,Cream,true);
            CenterLabel(panel,"Jump hint",new Vector2(0,137),new Vector2(560,55),"SPACE  /  GAMEPAD A\nJump over cacti. You can jump again after landing.",18,Cream);
            CenterLabel(panel,"Dash hint",new Vector2(0,50),new Vector2(560,70),"SHIFT  /  GAMEPAD X\nBurst forward and smash obstacles for +5.\nYour dash recharges in 2.2 seconds.",18,Cream);
            CenterLabel(panel,"Life hint",new Vector2(0,-43),new Vector2(560,65),"THREE LIVES. ONE LONG RUN.\nAvoid low birds or dash through them.\nCollect coins for +25. Your best score is saved.",18,Cream);
            CenterLabel(panel,"Pause hint",new Vector2(0,-131),new Vector2(560,45),"ESC or P pauses. R restarts after a run or while paused.\nUse the sound button to mute music and effects.",16,Cream);
            helpBack=CenterButton(panel,"Got It",new Vector2(0,-204),new Vector2(350,48),"LET'S GO",CloseHelp);
            help.SetActive(false);
        }
        void ShowHelp() { help.SetActive(true); main.GetComponent<CanvasGroup>().interactable=false; Select(helpBack); }
        public void CloseHelp() {help.SetActive(false);main.GetComponent<CanvasGroup>().interactable=true;Select(startButton);}
        public void ShowState(GameState state)
        {
            main.SetActive(state==GameState.MainMenu);
            hud.SetActive(state!=GameState.MainMenu);
            pause.SetActive(state==GameState.Paused);
            over.SetActive(state==GameState.GameOver);
            help.SetActive(false);
            main.GetComponent<CanvasGroup>().interactable=true;
            hud.GetComponent<CanvasGroup>().interactable=state==GameState.Running;
            if(state==GameState.MainMenu) Select(startButton);
            if(state==GameState.Paused) Select(resumeButton);
            if(state==GameState.GameOver) Select(retryButton);
            if(state==GameState.Running || state==GameState.Dying) EventSystem.current.SetSelectedGameObject(null);
            Refresh();
        }
        public void Refresh()
        {
            if(game.Player==null) return;
            score.text=game.Score.ToString("D5"); best.text=game.HighScore.ToString("D5");
            menuBest.text="PERSONAL BEST  "+game.HighScore.ToString("D5");
            healthLabel.text=game.Player.Lives+" / 3";
            for(int i=0;i<3;i++) hearts[i].color=i<game.Player.Lives ? Coral : new Color(.14f,.27f,.29f,.14f);
            float charge=game.Player.DashCharge;
            dashFill.rectTransform.sizeDelta=new Vector2(205*charge,9);
            dashFill.color=charge>=1 ? Ink : Coral;
            dashLabel.text=game.Player.IsDashing ? "DASHING!" : charge>=1 ? "SHIFT  /  DASH READY" : "DASH RECHARGING";
            finalScore.text=game.Score.ToString("D5");
            finalBest.text="PERSONAL BEST  "+game.HighScore.ToString("D5");
            finalCoins.text=game.Coins+" COINS COLLECTED   /   "+Mathf.FloorToInt(game.Distance)+" METERS";
            if(muteLabel!=null) muteLabel.text=AudioDirector.Instance.Muted ? "SOUND OFF" : "SOUND ON";
        }
        void Update()
        {
            for(int i=toasts.Count-1;i>=0;i--)
            {
                var t=toasts[i]; t.time-=Time.deltaTime;
                if(t.time<=0) {Destroy(t.rect.gameObject);toasts.RemoveAt(i);continue;}
                t.rect.anchoredPosition+=Vector2.up*Time.deltaTime*38;
                var c=t.text.color;c.a=Mathf.Min(1,t.time*2);t.text.color=c;
            }
        }
        public void ShowToast(string value,Vector3 world)
        {
            Vector2 screen=Camera.main.WorldToScreenPoint(world+Vector3.up*.7f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,screen,null,out var local);
            var text=CenterLabel(transform,"Score popup",local,new Vector2(170,35),value,20,Ink,true);
            toasts.Add(new Toast {rect=text.rectTransform,text=text,time=.85f});
        }
        GameObject Group(string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasGroup));go.transform.SetParent(transform,false);
            var r=(RectTransform)go.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.sizeDelta=Vector2.zero;return go;
        }
        GameObject Overlay(string name)
        {
            var go=Group(name);var bg=go.AddComponent<Image>();bg.color=new Color(.1f,.2f,.22f,.53f);return go;
        }
        Transform Card(Transform parent,float width,float height)
        {
            var r=Box(parent,"Card",Vector2.zero,new Vector2(width,height),Ink,new Vector2(.5f,.5f));r.pivot=new Vector2(.5f,.5f);return r;
        }
        RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size,Vector2 anchor)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;
            r.anchorMin=r.anchorMax=anchor;r.pivot= new Vector2(0,1);r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        RectTransform Box(Transform parent,string name,Vector2 position,Vector2 size,Color color,Vector2? anchor=null)
        {var r=Rect(parent,name,position,size,anchor??new Vector2(0,1));r.gameObject.AddComponent<Image>().color=color;return r;}
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,string text,int fontSize,Color color,bool bold=false,Vector2? anchor=null)
        {
            var r=Rect(parent,name,position,size,anchor??new Vector2(0,1));var label=r.gameObject.AddComponent<Text>();
            label.font=bold?display:body;label.text=text;label.fontSize=fontSize;label.color=color;label.raycastTarget=false;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Overflow;return label;
        }
        Text CenterLabel(Transform parent,string name,Vector2 position,Vector2 size,string text,int fontSize,Color color,bool bold=false)
        {
            var t=Label(parent,name,position,size,text,fontSize,color,bold,new Vector2(.5f,.5f));
            t.rectTransform.pivot=new Vector2(.5f,.5f);t.alignment=TextAnchor.MiddleCenter;return t;
        }
        Button Button(Transform parent,string name,Vector2 position,Vector2 size,string text,UnityAction action,bool filled=true,Vector2? anchor=null)
        {
            var r=Box(parent,name,position,size,filled?Coral:Cream,anchor);
            var button=r.gameObject.AddComponent<Button>();var colors=button.colors;
            colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.88f,.69f);colors.pressedColor=new Color(.83f,.74f,.62f);colors.selectedColor=colors.highlightedColor;button.colors=colors;
            button.onClick.AddListener(()=>{AudioDirector.Instance.Play("Click");action();});
            var label=CenterLabel(r,"Label",Vector2.zero,size,text,16,Ink,true);label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);
            return button;
        }
        Button CenterButton(Transform parent,string name,Vector2 position,Vector2 size,string text,UnityAction action,bool filled=true)
        {var b=Button(parent,name,position,size,text,action,filled,new Vector2(.5f,.5f));((RectTransform)b.transform).pivot=new Vector2(.5f,.5f);return b;}
        static void Select(Button button) { if(EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(button.gameObject); }
    }
}
