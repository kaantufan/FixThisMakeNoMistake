using UnityEngine;
namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        // Root may replace this with the critical worker's actual visible bounds.
        public Rect InteractionAvoidRectV513 { get; set; } = new Rect(24,116,430,220);
        public Rect InteractionPromptBoundsV513 { get; private set; }
        public string InteractionPromptTextV513 { get; private set; } = string.Empty;
        public int InteractionPromptRepaintsV513 { get; private set; }
        public bool WaveAnnouncementActiveV513 { get { return announcementAgeV513 < 2f; } }
        public Rect WaveAnnouncementBoundsV513 { get; private set; }
        public string WaveAnnouncementTextV513 { get; private set; } = string.Empty;
        public int WaveAnnouncementCountV513 { get; private set; }
        int observedWaveV513 = -1, announcedWaveV513 = -1, announcementMaskV513;
        PanicModel announcementModelV513;
        float announcementAgeV513 = 2f;
        string announcementTitleV513;
        bool PresentationModalV513 { get { return !World || World.Model == null || _paused || World.Model.IsPaused ||
            _developerMenuOpen || DeveloperInputBlockedThisFrame || KeyboardHelpBlockingV56 || WaveLedgerInputBlockedV56 ||
            FreeplayOnboardingInputBlockedV414 || _helpVisible || SimulationSelectorOpen || World.NotebookBlocksInput ||
            World.InventoryOpen || World.WorkActive || World.IsMoving || World.CinematicActive || World.LossAnnounced ||
            World.InputLocked || World.BagCameraTransitionActive; } }
        float promptOpacityV513;
        string promptIdentityV513 = string.Empty;
        GUIStyle promptShapeV513, promptKeyV513, promptVerbV513;
        public void TickPresentationV513()
        {
            TickInteractionBubbleV513();
            if (!World || World.Model == null) return;
            var m = World.Model;
            if (announcementModelV513 != m || m.Phase == PanicPhase.Title || m.Wave < observedWaveV513)
            {
                announcementModelV513 = m; observedWaveV513 = m.Wave; announcedWaveV513 = -1; announcementAgeV513 = 2f;
                WaveAnnouncementTextV513 = string.Empty;
            }
            // Phase observation is after FinishInspection actually chooses the next hazards.
            if (m.Phase == PanicPhase.Preparing && m.Wave > observedWaveV513)
            {
                observedWaveV513 = m.Wave;
                if (m.Wave > 1 && announcedWaveV513 != m.Wave && !m.IsSimulation && !m.IsTutorial && !m.IsJuryDemo)
                {
                    announcedWaveV513 = m.Wave; announcementAgeV513 = 0f;
                    announcementMaskV513 = m.ActiveDisasterMaskV54;
                    announcementTitleV513 = "DALGA " + m.Wave + " · YAKLAŞAN AFET";
                    WaveAnnouncementTextV513 = m.ActiveDisasterLabelV54;
                    WaveAnnouncementCountV513++;
                }
            }
            if (m.Phase != PanicPhase.Preparing && m.Phase != PanicPhase.Inspection) announcementAgeV513 = 2f;
            if (!PresentationModalV513 && WaveAnnouncementActiveV513)
                announcementAgeV513 = Mathf.Min(2f, announcementAgeV513 + Time.unscaledDeltaTime);
        }
        public void DrawPresentationV513()
        {
            InteractionPromptTextV513 = string.Empty; InteractionPromptBoundsV513 = new Rect();
            if (PresentationModalV513) return;
            if (WaveAnnouncementActiveV513)
            {
                float t = Mathf.Clamp01((announcementAgeV513 - 1.2f) / .8f);
                t = t*t*(3f-2f*t);
                Rect a = new Rect(440,315,720,180), b = WaveCardBounds;
                Rect r = new Rect(Mathf.Lerp(a.x,b.x,t),Mathf.Lerp(a.y,b.y,t),Mathf.Lerp(a.width,b.width,t),Mathf.Lerp(a.height,b.height,t));
                WaveAnnouncementBoundsV513 = r;
                DrawWavePanelV513(r,t);
                return;
            }
            if (World.Model.Phase == PanicPhase.Preparing && announcedWaveV513 == World.Model.Wave && !string.IsNullOrEmpty(WaveAnnouncementTextV513))
                DrawWavePanelV513(WaveCardBounds,1f);
            string key, verb; Vector3 p;
            if (!World.TryInteractionPromptV513(out key,out verb,out p)) return;
            Vector3 screen = World.View.WorldToScreenPoint(p); if(screen.z<=0) return;
            Vector2 logical=(new Vector2(screen.x,Screen.height-screen.y)-_offset)/_scale;
            if (promptOpacityV513 <= 0f) return;
            if(promptKeyV513==null)
            {
                promptKeyV513=new GUIStyle(_heading) { alignment=TextAnchor.MiddleCenter, fontSize=19, padding=new RectOffset() };
                promptVerbV513=new GUIStyle(_body) { alignment=TextAnchor.MiddleLeft, fontSize=18, padding=new RectOffset() };
            }
            // A 48px object hint, not another HUD card. Measure with the actual text style.
            float widthPrompt=Mathf.Clamp(promptVerbV513.CalcSize(new GUIContent(verb)).x+64,120,250);
            Rect desired=new Rect(Mathf.Clamp(logical.x-widthPrompt*.5f,16,CanvasWidth-widthPrompt-16),Mathf.Clamp(logical.y-8,150,690),widthPrompt,48);
            Rect selected=SelectedTargetHudVisibleV56?SelectedTargetHudBoundsV56:new Rect();
            bool found=false; Rect candidate=desired;
            // Remain local to the projected object; hide when surrounding HUD leaves no space.
            for(int row=0;row<5&&!found;row++) for(int col=0;col<3&&!found;col++)
            {
                candidate=desired;
                candidate.y=Mathf.Clamp(desired.y+(row%2==0?1:-1)*((row+1)/2)*56,150,690);
                if(col==1)candidate.x=Mathf.Clamp(desired.x-96,16,CanvasWidth-widthPrompt-16);
                else if(col==2)candidate.x=Mathf.Clamp(desired.x+96,16,CanvasWidth-widthPrompt-16);
                if(candidate.Overlaps(InteractionAvoidRectV513)||candidate.Overlaps(selected)||!WorldMarkerClear(candidate)||!WorldMarkerAvoidsDialogueV59(candidate))continue;
                found=true;
            }
            if(!found)return;
            float eased=promptOpacityV513*promptOpacityV513*(3f-2f*promptOpacityV513);
            float pop=Mathf.Lerp(.94f,1f,eased);
            candidate=new Rect(candidate.center.x-candidate.width*pop*.5f,candidate.center.y-candidate.height*pop*.5f+3f*(1f-eased),candidate.width*pop,candidate.height*pop);
            Color oldColor=GUI.color;
            GUI.color=new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a*eased);
            BubbleFaceV513(new Rect(candidate.x,candidate.y+3,candidate.width,candidate.height),new Color(0,0,0,.22f));
            BubbleFaceV513(candidate,new Color(.63f,.56f,.40f,.65f));
            BubbleFaceV513(new Rect(candidate.x+1,candidate.y+1,candidate.width-2,candidate.height-2),new Color(.065f,.075f,.085f,.95f));
            Rect cap=new Rect(candidate.x+7,candidate.y+7,34*pop,34*pop);
            BubbleFaceV513(cap,new Color(.63f,.58f,.46f,.85f));
            BubbleFaceV513(new Rect(cap.x+1,cap.y+1,cap.width-2,cap.height-2),new Color(.15f,.16f,.17f,1f));
            FittedLabel(cap,key,promptKeyV513,new Color(.96f,.91f,.80f),key.Length>1?12:19);
            FittedLabel(new Rect(candidate.x+51,candidate.y+6,candidate.width-61,candidate.height-12),verb,promptVerbV513,MainText,14);
            GUI.color=oldColor;
            InteractionPromptTextV513=key+" · "+verb; InteractionPromptBoundsV513=candidate;
            if(Event.current.type==EventType.Repaint)InteractionPromptRepaintsV513++;
        }

        void TickInteractionBubbleV513()
        {
            if(PresentationModalV513 || WaveAnnouncementActiveV513)
            { promptOpacityV513=0; promptIdentityV513=string.Empty; return; }
            string key,verb; Vector3 position;
            bool eligible=World.TryInteractionPromptV513(out key,out verb,out position);
            string identity=eligible?key+" · "+verb:string.Empty;
            if(eligible && identity!=promptIdentityV513) promptOpacityV513=0;
            promptIdentityV513=identity;
            promptOpacityV513=Mathf.MoveTowards(promptOpacityV513,eligible?1f:0f,Time.unscaledDeltaTime/(eligible?.14f:.10f));
        }
        void BubbleFaceV513(Rect rect,Color color)
        {
            if(promptShapeV513==null)
            {
                const int size=32;
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
                texture.name="Interaction bubble rounded face V513";
                texture.hideFlags=HideFlags.HideAndDontSave;
                texture.wrapMode=TextureWrapMode.Clamp;
                texture.filterMode=FilterMode.Bilinear;
                var pixels=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float dx=Mathf.Max(Mathf.Abs(x-15.5f)-5.5f,0f);
                    float dy=Mathf.Max(Mathf.Abs(y-15.5f)-5.5f,0f);
                    pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(10.5f-Mathf.Sqrt(dx*dx+dy*dy)));
                }
                texture.SetPixels(pixels); texture.Apply(false,true);
                promptShapeV513=new GUIStyle { border=new RectOffset(12,12,12,12) };
                promptShapeV513.normal.background=texture;
            }
            Color previous=GUI.color;
            GUI.color=new Color(previous.r*color.r,previous.g*color.g,previous.b*color.b,previous.a*color.a);
            GUI.Box(rect,GUIContent.none,promptShapeV513);
            GUI.color=previous;
        }

        void DrawWavePanelV513(Rect r,float t)
        {
            WaveAnnouncementBoundsV513 = r;
                Frame(r,Night,Brass,2f);
                float icon = Mathf.Lerp(45,24,t), x = r.x+18;
                int count=0; for(int i=0;i<4;i++)if((announcementMaskV513&(1<<i))!=0)count++;
                for(int i=0;i<4;i++) if((announcementMaskV513&(1<<i))!=0)
                { DrawDisasterSymbol(new Rect(x,r.y+r.height*.5f-icon*.5f,icon,icon),(DisasterType)i,Brass); x+=icon+8; }
                float textX=x+8, width=r.xMax-textX-12;
                FittedLabel(new Rect(textX,r.y+10,width,Mathf.Lerp(35,20,t)),announcementTitleV513,_caps,Brass,12);
                FittedLabel(new Rect(textX,r.y+Mathf.Lerp(60,32,t),width,Mathf.Lerp(70,32,t)),WaveAnnouncementTextV513,_heading,MainText,15);
        }
    }
}


