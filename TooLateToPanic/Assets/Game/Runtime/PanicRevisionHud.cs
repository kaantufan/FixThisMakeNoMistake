using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        int _dragSlot = -1, _bagSelection = -1;
        bool _dragFromBag;
        string _dragId;
        int _dragCount;
        public int TitleChoice { get { return _menuChoice; } }
        public int PauseChoice { get { return _pauseChoice; } }
        public bool InventoryDragging { get { return _dragSlot >= 0; } }
        public bool NotebookModalBlocked { get { return FreeplayOnboardingInputBlockedV414 || _paused || DeveloperMenuOpen || DeveloperInputBlockedThisFrame || _helpVisible || SimulationSelectorOpen; } }

        public void OpenPhysicalBag()
        {
            if (!World || World.Model == null || World.Model.IsPaused || World.WorkActive || World.IsMoving) return;
            if (World.Model.Phase != PanicPhase.Preparing && World.Model.Phase != PanicPhase.Tutorial) return;
            CancelInventoryDrag();
            if (!World.InventoryOpen) World.ToggleInventory();
        }
        static bool IsNavigationKey(KeyCode key)
        {
            return key == KeyCode.W || key == KeyCode.A || key == KeyCode.S || key == KeyCode.D ||
                key == KeyCode.UpArrow || key == KeyCode.LeftArrow || key == KeyCode.DownArrow || key == KeyCode.RightArrow;
        }
        /// <summary>Matches the actual title layout: two full rows, a two-column row, then Exit.</summary>
        public static int NavigateTitleChoice(int choice, KeyCode key)
        {
            choice = Mathf.Clamp(choice,0,4);
            bool left = key == KeyCode.A || key == KeyCode.LeftArrow;
            bool right = key == KeyCode.D || key == KeyCode.RightArrow;
            if (left) return choice == 3 ? 2 : choice;
            if (right) return choice == 2 ? 3 : choice;
            bool up = key == KeyCode.W || key == KeyCode.UpArrow;
            bool down = key == KeyCode.S || key == KeyCode.DownArrow;
            if (up) return choice == 0 ? 4 : choice == 4 ? 2 : choice >= 2 ? 1 : 0;
            if (down) return choice == 4 ? 0 : choice >= 2 ? 4 : choice + 1;
            return choice;
        }
        public static int NavigatePauseChoice(int choice, KeyCode key)
        {
            if (key == KeyCode.W || key == KeyCode.A || key == KeyCode.UpArrow || key == KeyCode.LeftArrow) return (choice+5)%6;
            if (key == KeyCode.S || key == KeyCode.D || key == KeyCode.DownArrow || key == KeyCode.RightArrow) return (choice+1)%6;
            return Mathf.Clamp(choice,0,5);
        }
        void HandlePauseNavigation()
        {
            if (DeveloperInputBlockedThisFrame) return;
            Event e=Event.current;
            if (e.type != EventType.KeyDown) return;
            if (IsNavigationKey(e.keyCode))
            { _pauseChoice=NavigatePauseChoice(_pauseChoice,e.keyCode);e.Use(); }
            else if (e.keyCode==KeyCode.Return || e.keyCode==KeyCode.KeypadEnter)
            {
                e.Use();World.PlayUiSound();
                switch(_pauseChoice)
                {
                    case 0: SetPaused(false);break;
                    case 1: ToggleDeveloperMenu();break;
                    case 2: RestartRun();break;
                    case 3: ReturnToMenu();break;
                    case 4: PanicBackgroundMusic.Enabled=!PanicBackgroundMusic.Enabled;break;
                    case 5: World.SetReducedFpsMotionV54(!World.ReducedFpsMotionV54);break;
                }
            }
        }
        public Rect InventorySlotBounds(bool bag,int index)
        {
            if (index<0||index>=(bag?8:12)) return new Rect();
            return bag ? new Rect(824+(index%4)*168,286+(index/4)*113,156,101) :
                new Rect(126+(index%4)*156,286+(index/4)*113,144,101);
        }
        void HandleInventorySlot(Rect cell,bool bag,int index)
        {
            Event e=Event.current;
            if (World.DiscreteInputFrame==Time.frameCount || World.CancelFrame==Time.frameCount || World.Model.IsPaused) return;
            if (!cell.Contains(PanicInteractionAudit.GuiPointer(e.mousePosition))) return;
            var slots=bag?World.Expedition.Bag:World.Expedition.Inventory;
            if (e.type==EventType.MouseDown && e.button==0)
            {
                if (bag) _bagSelection=index;else { World.InventorySelect(index);_bagSelection=-1; }
                if (HasStock(slots[index]))
                { _dragFromBag=bag;_dragSlot=index;_dragId=slots[index].Id;_dragCount=slots[index].Count; }
                else CancelInventoryDrag();
                e.Use();
            }
            else if(e.type==EventType.MouseUp && e.button==0 && _dragSlot>=0)
            {
                var source=_dragFromBag?World.Expedition.Bag:World.Expedition.Inventory;
                // An asynchronous key action must not drag a different stack from the same slot.
                if(source[_dragSlot].Id==_dragId && source[_dragSlot].Count==_dragCount)
                    World.Expedition.TransferSlots(_dragFromBag,_dragSlot,bag,index);
                CancelInventoryDrag();e.Use();
            }
        }
        void DrawInventoryDrag()
        {
            if (_dragSlot<0) return;
            Event e=Event.current;
            if(e.type==EventType.MouseUp && e.button==0){CancelInventoryDrag();e.Use();return;}
            if(e.type==EventType.KeyDown && e.keyCode==KeyCode.Escape){CancelInventoryDrag();e.Use();return;}
            Vector2 pointer=PanicInteractionAudit.GuiPointer(e.mousePosition);
            Rect ghost=new Rect(pointer.x+16,pointer.y+14,128,88);
            Frame(ghost,Night,Brass,1f);DrawSlot(ghost,new SupplySlot(_dragId,_dragCount),true);
        }
        void CancelInventoryDrag(){_dragSlot=-1;_dragId=null;_dragCount=0;}

        void DrawMaterialPool()
        {
            if (World.Expedition==null || World.BagCameraOwnsView) return;
            Rect panel=new Rect(460,126,624,38);
            Frame(panel,Night,Line,1f);Region(panel);
            // The primary three counters stay on the HUD; all seven are visible in the inventory.
            string[] ids={"screws","bracket","plank"};
            for(int i=0;i<ids.Length;i++)
            {
                float x=474+i*202;
                DrawIcon(new Rect(x,131,28,28),ids[i]);
                Label(new Rect(x+34,131,162,28),SlotName(new SupplySlot(ids[i],1))+" "+World.Expedition.Count(ids[i]),_tiny);
            }
        }
        void DrawInventoryMaterialPool()
        {
            if (World.BagFinalStageActive) return;
            Rect box=new Rect(114,704,650,46);Frame(box,Night,Line,1f);Mark(box);
            string[] labels=new string[PanicExpedition.MaterialIds.Length];
            for(int i=0;i<labels.Length;i++)
            { string id=PanicExpedition.MaterialIds[i];labels[i]=SlotName(new SupplySlot(id,1))+": "+World.Expedition.Count(id); }
            FittedLabel(new Rect(122,710,634,32),string.Join(" · ",labels),_tiny,Brass,10);
        }
    }
}
