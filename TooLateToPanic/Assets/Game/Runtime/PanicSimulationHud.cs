using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        bool simulationSelectOpen;
        int simulationChoice;
        static readonly DisasterType[] SimulationTypes = { DisasterType.Earthquake, DisasterType.Flood, DisasterType.Fire, DisasterType.Storm };
        public bool SimulationSelectorOpen { get { return simulationSelectOpen; } }

        public void OpenSimulationSelector() { simulationSelectOpen = true; simulationChoice = 0; }
        public void CloseSimulationSelector() { simulationSelectOpen = false; }

        void DrawSimulationSelector()
        {
            Fill(LogicalCanvasBounds,new Color(.04f,.05f,.06f,.62f));
            Rect panel = new Rect(230,95,1140,710); Frame(panel,Night,Line,1); Region(panel);
            Label(new Rect(272,130,1030,65),"AFETİ DENE",_title);
            Label(new Rect(276,208,1020,42),"Afeti seç. Evini hazırla, başlat ve sonucunu incele.",_body);
            string[] hints = { "Eşyaları sabitle. Sarsıntının etkisini gör.", "Drenaj ve suya karşı önlemleri dene.", "Yangın önlemlerini hazırla ve sonucu izle.", "Çatı ve kapıları güçlendir, gevşek eşyaları sabitle." };
            for(int i=0;i<4;i++)
            {
                int choice=i; float x=272+(i%2)*538, y=282+(i/2)*180;
                Frame(new Rect(x,y,510,158),Panel,Line,1);
                Button(new Rect(x+14,y+14,482,64),DisasterName(SimulationTypes[i]),simulationChoice==i,i==3?Brass:Sea,()=>StartSelectedSimulation(SimulationTypes[choice]));
                Label(new Rect(x+24,y+94,462,52),hints[i],_wrap);
            }
            Button(new Rect(272,685,290,64),"GERİ",false,Line,()=>simulationSelectOpen=false);
            Label(new Rect(590,697,730,50),"Çanta başlangıçta hazır · Sonlu malzeme · Skor dışı",_small);
            var e=Event.current;
            if(DeveloperInputBlockedThisFrame||e.type!=EventType.KeyDown)return;
            if(e.keyCode==KeyCode.Escape||e.keyCode==KeyCode.Backspace){simulationSelectOpen=false;e.Use();}
            else if(e.keyCode==KeyCode.LeftArrow||e.keyCode==KeyCode.UpArrow){simulationChoice=(simulationChoice+3)%4;e.Use();}
            else if(e.keyCode==KeyCode.RightArrow||e.keyCode==KeyCode.DownArrow){simulationChoice=(simulationChoice+1)%4;e.Use();}
            else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter){StartSelectedSimulation(SimulationTypes[simulationChoice]);e.Use();}
        }

        void StartSelectedSimulation(DisasterType type)
        {
            simulationSelectOpen=false; if(_paused)SetPaused(false);
            _helpVisible=false; _inspectionStep=0; World.StartDisasterSimulation(type);
        }

        void DrawSimulationActions()
        {
            if(!World.Model.IsSimulation || World.LootVisible || World.IsMoving)return;
            Rect panel=new Rect(1032,622,536,175); Frame(panel,Night,Line,1);Region(panel);
            bool complete=World.Model.SimulationComplete;
            Label(new Rect(1050,634,495,31),complete?"SONUCU İNCELE":"HAZIRLIĞINI DENE",_body);
            Label(new Rect(1050,675,495,35),complete?"Evde ve çevrede önlemlerin etkisini gör.":"Çanta hazır. Önlemini seç, afeti başlat.",_small);
            if(complete)
            {
                Button(new Rect(1050,724,240,53),"TEKRAR DENE",false,Brass,()=>StartSelectedSimulation(World.Model.Disaster));
                Button(new Rect(1302,724,248,53),"ANA MENÜ",false,Line,ReturnToMenu);
            }
            else Button(new Rect(1050,724,500,53),"AFETİ BAŞLAT",true,Brass,World.TriggerSimulation);
        }
    }
}
