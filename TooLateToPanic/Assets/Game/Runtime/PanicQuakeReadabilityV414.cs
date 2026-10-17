using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeWorld
    {
        PanicQuakeReadabilityV414 quakeReadabilityV414;
        public PanicQuakeReadabilityV414 QuakeReadabilityV414 { get { return quakeReadabilityV414; } }
        internal float QuakeReadabilityAgeV414 { get { return residenceCinematicAge; } }
        internal bool QuakeReadabilityWindowV414
        {
            get { return isActiveAndEnabled && Model != null && !JuryActive && CinematicActive &&
                CinematicKind == "Disaster" && Model.HasActiveDisasterV54(DisasterType.Earthquake) &&
                residenceCinematicAge >= 4.7f && residenceCinematicAge < 8f; }
        }

        void ResetQuakeReadabilityV414(string reason)
        {
            if (quakeReadabilityV414) quakeReadabilityV414.Restore(reason);
        }

        bool TryQuakeReadabilityShotV414(out Vector3 camera, out Vector3 target, out float fov)
        {
            camera = target = Vector3.zero; fov = 52f;
            if (!QuakeReadabilityWindowV414) return false;
            if (!quakeReadabilityV414)
            {
                quakeReadabilityV414 = gameObject.AddComponent<PanicQuakeReadabilityV414>();
                quakeReadabilityV414.Owner = this;
            }
            if (!quakeReadabilityV414.Active && !Model.IsPaused && !NotebookBlocksInput && !InputLocked)
            {
                // The earlier two-object cosmetic cue yields once; its original child poses are restored.
                // No character pose/Animator/controller is involved in this handover.
                RestoreHeroReactions();
                var candidates = new List<PanicQuakeReadabilityV414.Subject>();
                foreach (var item in Model.Items)
                {
                    if (!item.Installed || !item.Movable || item.Kind != ItemKind.Furniture ||
                        item.Position.y < -.1f || item.Position.y > .3f || item.Position.x < -7.7f ||
                        item.Position.x > -1.5f || item.Position.z < -5.6f || item.Position.z > 5.5f) continue;
                    if (items.TryGetValue(item.Id, out var visual) && visual)
                        candidates.Add(new PanicQuakeReadabilityV414.Subject(item, visual));
                }
                // Explicit residence art only: no scene clone, neighbours, actor or stair physics.
                var shell = new List<PanicQuakeReadabilityV414.HouseRoot>();
                foreach (var floor in floorVisuals)
                    if (floor && floor.transform.position.y >= -.1f)
                        shell.Add(new PanicQuakeReadabilityV414.HouseRoot(floor, false));
                if (roof) shell.Add(new PanicQuakeReadabilityV414.HouseRoot(roof, true));
                if (residenceGarage) shell.Add(new PanicQuakeReadabilityV414.HouseRoot(residenceGarage, false));
                if (residenceBalcony) shell.Add(new PanicQuakeReadabilityV414.HouseRoot(residenceBalcony, false));
                // Exterior door/window glass, frames and applied wall measures follow the same visual pivot.
                foreach (var item in Model.Items)
                    if (item.Installed && item.Position.y >= -.1f && item.Position.y < 3.4f &&
                        (item.Kind == ItemKind.Door || item.Kind == ItemKind.Window || item.Kind == ItemKind.Wall) &&
                        (Mathf.Abs(item.Position.x) > 7.6f || Mathf.Abs(item.Position.z) > 5.6f) &&
                        items.TryGetValue(item.Id, out var part) && part)
                        shell.Add(new PanicQuakeReadabilityV414.HouseRoot(part, false));
                if (residenceTargets.TryGetValue("@garage", out var garageDoor) && garageDoor)
                    shell.Add(new PanicQuakeReadabilityV414.HouseRoot(garageDoor, false));
                quakeReadabilityV414.Begin(candidates, shell);
            }
            // The user asked for the exterior house itself to move. All of the final3.3s stays outside.
            camera = new Vector3(19f, 12.2f, -24f);
            target = new Vector3(1.5f, 2.25f, .25f);
            fov = 50f;
            if (Model.ActiveDisasterCountV54 > 1)
            {
                CinematicTitle = Model.ActiveDisasterLabelV54 + ": ev sarsılıyor";
                if (Model.HasActiveDisasterV54(DisasterType.Storm))
                {
                    // Preserve the same existing cinematic interval and motion, with room
                    // for the west-side storm column and the shaking home in one frame.
                    camera = new Vector3(-22f, 21f, -29f);
                    target = new Vector3(-6.5f, 5.1f, 4.5f); fov = 56f;
                }
                else if (Model.HasActiveDisasterV54(DisasterType.Flood))
                {
                    camera = new Vector3(-26f, 21f, -24f);
                    target = new Vector3(-5f, 2.25f, 0f); fov = 56f;
                }
            }
            else CinematicTitle = "Deprem: ev sarsılıyor";
            float t = residenceCinematicAge - 4.7f;
            float envelope = Mathf.SmoothStep(0, 1, t / .18f) *
                (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(7.7f, 8f, residenceCinematicAge)));
            camera += new Vector3(Mathf.Sin(t * 32) * .006f, Mathf.Sin(t * 43) * .004f, 0) * envelope;
            return true;
        }
    }

    /// <summary>
    /// Bounded cinematic display copies. Original transforms, colliders, materials, Model and clocks never move.
    /// Only the owner's already-running cinematic age drives motion. No autonomous time or random stream.
    /// </summary>
    [DefaultExecutionOrder(700)]
    public sealed class PanicQuakeReadabilityV414 : MonoBehaviour
    {
        public readonly struct Subject
        {
            public readonly PanicItem Item;
            public readonly GameObject Visual;
            public Subject(PanicItem item, GameObject visual) { Item = item; Visual = visual; }
        }
        public readonly struct HouseRoot
        {
            public readonly GameObject Visual;
            public readonly bool Roof;
            public HouseRoot(GameObject visual, bool roof) { Visual = visual; Roof = roof; }
        }
        sealed class Copy
        {
            public string Id;
            public Transform Pivot;
            public Vector3 Origin;
            public bool Secured;
            public float Height, Phase;
        }
        struct Hidden { public Renderer Renderer; public bool WasForcedOff, House, Roof; }
        public PanicHomeWorld Owner;
        public bool Active { get; private set; }
        public int SubjectCount { get { return copies.Count; } }
        public int SecuredSubjects { get; private set; }
        public int LooseSubjects { get; private set; }
        public int SourceRendererCount { get; private set; }
        public int ActiveHiddenRendererCount { get { return hidden.Count; } }
        public int SampledFrames { get; private set; }
        public int RestoreCount { get; private set; }
        public int ProxyRendererCount { get; private set; }
        public GameObject DisplayRoot { get { return display; } }
        public int ActiveProxyObjectCount { get { return Active && display ? display.GetComponentsInChildren<Transform>(true).Length : 0; } }
        public int ActiveProxyColliderCount { get { return display ? display.GetComponentsInChildren<Collider>(true).Length : 0; } }
        public int ActiveOwnedAssetCount { get { return owned.Count; } }
        public int LastRestoredRendererCount { get; private set; }
        public int RestoreMismatchCount { get; private set; }
        public Transform HousePivot { get { return housePivot; } }
        public int HouseRootCount { get; private set; }
        public int ExternalHouseProxyCount { get; private set; }
        public int RoofProxyCount { get; private set; }
        public int ActiveExternalHouseProxyCount { get { return Active && housePivot ? ExternalHouseProxyCount : 0; } }
        public int LastRestoredHouseRendererCount { get; private set; }
        public int LastRestoredRoofRendererCount { get; private set; }
        public Vector3 CurrentHouseOffset { get; private set; }
        public float CurrentHouseTilt { get; private set; }
        public float MaximumHouseOffset { get; private set; }
        public float MaximumHouseTilt { get; private set; }
        public float CurrentPresentationScaleV52 { get; private set; } = 1f;
        public Bounds HouseRoofBounds { get; private set; }
        public Renderer[] OriginalHouseRenderers { get { return hidden.FindAll(entry => entry.House).ConvertAll(entry => entry.Renderer).ToArray(); } }
        public Renderer[] OriginalRoofRenderers { get { return hidden.FindAll(entry => entry.Roof).ConvertAll(entry => entry.Renderer).ToArray(); } }
        public float SampledAge { get; private set; }
        public float MaximumLooseOffset { get; private set; }
        public float MaximumSecuredOffset { get; private set; }
        public float MaximumLooseTilt { get; private set; }
        public float MaximumSecuredTilt { get; private set; }
        public float CurrentPulse { get; private set; }
        public string Shot { get; private set; } = "idle";
        public string LastResetReason { get; private set; } = "none";
        public string[] VisibleSubjectIds { get { return copies.ConvertAll(c => c.Id).ToArray(); } }
        public string[] SecuredSubjectIds { get { return copies.FindAll(c => c.Secured).ConvertAll(c => c.Id).ToArray(); } }
        public string[] LooseSubjectIds { get { return copies.FindAll(c => !c.Secured).ConvertAll(c => c.Id).ToArray(); } }
        public Renderer[] OriginalSourceRenderers { get { return hidden.ConvertAll(entry => entry.Renderer).ToArray(); } }
        public Vector3 RoomFocus { get; private set; }

        readonly List<Copy> copies = new List<Copy>(8);
        readonly List<Hidden> hidden = new List<Hidden>(80);
        readonly List<Object> owned = new List<Object>();
        readonly HashSet<Renderer> claimed = new HashSet<Renderer>();
        GameObject display;
        Transform housePivot;
        Mesh dustMesh, chipsMesh;
        Vector3[] dustVertices, chipVertices;
        Color[] dustColors;
        const int DustCount = 42, ChipCount = 32;
        static readonly Vector3[] Corners = { new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),
            new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1) };
        static readonly int[] CubeFaces = { 0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2 };

        public void Begin(List<Subject> candidates, List<HouseRoot> shell = null)
        {
            if (Active) return;
            display = new GameObject("Temporary readable earthquake V414");
            display.transform.SetParent(transform, true);
            display.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            display.transform.localScale = Vector3.one;
            SampledFrames = SourceRendererCount = ProxyRendererCount = SecuredSubjects = LooseSubjects = 0;
            HouseRootCount = ExternalHouseProxyCount = RoofProxyCount = 0;
            MaximumHouseOffset = MaximumHouseTilt = CurrentHouseTilt = 0;
            CurrentPresentationScaleV52 = Owner && Owner.Model != null ? PanicDisasterArt.ClampPresentationScaleV52(Owner.Model.DisasterPresentationScaleV52) : 1f;
            CurrentHouseOffset = Vector3.zero;
            claimed.Clear();
            HouseRoofBounds = new Bounds(new Vector3(0,7.45f,0),new Vector3(17.15f,2.2f,13.15f));
            MaximumLooseOffset = MaximumSecuredOffset = MaximumLooseTilt = MaximumSecuredTilt = 0;
            RoomFocus = new Vector3(-5.7f, 0, 2.7f);
            // Prefer the actual tall bookcase, wherever it was left within this room.
            foreach (var subject in candidates)
                if (subject.Item.Id == "bookshelf") { RoomFocus = subject.Item.Position + new Vector3(0,0,-.9f); break; }
            RoomFocus = new Vector3(Mathf.Clamp(RoomFocus.x, -6.6f, -3.0f), 0, Mathf.Clamp(RoomFocus.z, -2.8f, 3.8f));
            foreach (var subject in candidates)
            {
                if (copies.Count >= 8) break;
                var pivot = new GameObject("Quake display copy " + subject.Item.Id).transform;
                pivot.SetParent(display.transform, true); pivot.position = subject.Visual.transform.position;
                int countBefore = hidden.Count;
                foreach (var source in subject.Visual.GetComponentsInChildren<MeshRenderer>(true))
                {
                    CopyRenderer(source,pivot,false,false);
                }
                if (hidden.Count == countBefore) { Destroy(pivot.gameObject); continue; }
                copies.Add(new Copy { Id = subject.Item.Id, Pivot = pivot, Origin = pivot.position,
                    Secured = subject.Item.Anchored, Height = subject.Item.Size.y, Phase = copies.Count * .71f });
                if (subject.Item.Anchored) SecuredSubjects++; else LooseSubjects++;
            }
            housePivot=new GameObject("Synchronized exterior house display pivot").transform;
            housePivot.SetParent(display.transform,true);housePivot.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            bool roofBoundsCaptured=false;
            if(shell!=null)foreach(var root in shell)
            {
                if(!root.Visual)continue;int previous=ExternalHouseProxyCount;
                foreach(var source in root.Visual.GetComponentsInChildren<MeshRenderer>(true))
                {
                    bool roofPart=root.Roof;
                    for(var parent=source.transform;!roofPart&&parent&&parent!=root.Visual.transform;parent=parent.parent)
                        roofPart=parent.name=="Roof";
                    if(!CopyRenderer(source,housePivot,true,roofPart))continue;
                    if(root.Roof)
                    {
                        if(!roofBoundsCaptured){HouseRoofBounds=source.bounds;roofBoundsCaptured=true;}
                        else {var bounds=HouseRoofBounds;bounds.Encapsulate(source.bounds);HouseRoofBounds=bounds;}
                    }
                }
                if(ExternalHouseProxyCount>previous)HouseRootCount++;
            }
            CreateDust(); CreateChips();
            Active = true;
        }

        bool CopyRenderer(MeshRenderer source,Transform pivot,bool house,bool roofPart)
        {
            if(!source||!source.enabled||!source.gameObject.activeInHierarchy||source.forceRenderingOff||
                source.GetComponent<PanicPermanentlyHiddenV414>()||claimed.Contains(source))return false;
            var filter=source.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)return false;
            var part=new GameObject((house?"House display mesh ":"Display mesh ")+source.name);part.layer=2;
            part.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            part.transform.localScale=source.transform.lossyScale;part.transform.SetParent(pivot,true);
            part.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
            var renderer=part.AddComponent<MeshRenderer>();renderer.sharedMaterials=source.sharedMaterials;
            renderer.shadowCastingMode=source.shadowCastingMode;renderer.receiveShadows=source.receiveShadows;
            var block=new MaterialPropertyBlock();source.GetPropertyBlock(block);renderer.SetPropertyBlock(block);
            hidden.Add(new Hidden{Renderer=source,WasForcedOff=source.forceRenderingOff,House=house,Roof=roofPart});
            claimed.Add(source);source.forceRenderingOff=true;SourceRendererCount++;ProxyRendererCount++;
            if(house)ExternalHouseProxyCount++;if(roofPart)RoofProxyCount++;
            return true;
        }

        void LateUpdate()
        {
            if (!Owner || !Owner.QuakeReadabilityWindowV414) { Restore("window ended or owner disabled"); return; }
            if (Owner.Model.IsPaused || Owner.NotebookBlocksInput || Owner.InputLocked)
            { Restore("paused or presentation suspended"); return; }
            if (!Active || !display) return;
            float age = Owner.QuakeReadabilityAgeV414, t = age - 4.7f;
            SampledAge = age; SampledFrames++; Shot = "roofed exterior";
            CurrentPresentationScaleV52 = PanicDisasterArt.ClampPresentationScaleV52(Owner.Model.DisasterPresentationScaleV52);
            float envelope = Mathf.SmoothStep(0,1,t/.16f) * (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(7.72f,8,age)));
            CurrentPulse = envelope * (.65f + .35f * Mathf.Pow(Mathf.Sin(t * 9), 2));
            if(housePivot)
            {
                // One ground-level world pivot for ALL shell pieces: no roof/wall/garage seams.
                // Cosmetic proxy only; V5.2 bounds remain below2.3deg and26cm translation.
                float wave=Mathf.Sin(t*18)*.72f+Mathf.Sin(t*31)*.28f,cross=Mathf.Sin(t*21+.7f);
                CurrentHouseOffset=new Vector3(wave*.14f,Mathf.Sin(t*26)*.012f,cross*.065f)*envelope*CurrentPresentationScaleV52;
                housePivot.position=CurrentHouseOffset;
                housePivot.rotation=Quaternion.Euler(cross*.4f*envelope*CurrentPresentationScaleV52,0,wave*1.25f*envelope*CurrentPresentationScaleV52);
                CurrentHouseTilt=Quaternion.Angle(Quaternion.identity,housePivot.rotation);
                MaximumHouseOffset=Mathf.Max(MaximumHouseOffset,CurrentHouseOffset.magnitude);
                MaximumHouseTilt=Mathf.Max(MaximumHouseTilt,CurrentHouseTilt);
            }
            foreach (var copy in copies)
            {
                if (!copy.Pivot) continue;
                float wave = Mathf.Sin(t * 17 + copy.Phase) * .7f + Mathf.Sin(t * 29 + copy.Phase) * .3f;
                float height = Mathf.Clamp01(copy.Height / 2.1f);
                float angle = (copy.Secured ? wave * .65f : wave * (7.5f + height * 3.5f) +
                    Mathf.SmoothStep(0,1,t/.7f) * (3 + height * 3)) * envelope * CurrentPresentationScaleV52;
                float shift = copy.Secured ? wave * .006f : wave * .12f + Mathf.SmoothStep(0,1,t/.9f) * .12f;
                Vector3 offset = new Vector3(shift, 0, wave * (copy.Secured ? .003f : .055f)) * envelope * CurrentPresentationScaleV52;
                copy.Pivot.position = copy.Origin + offset;
                copy.Pivot.rotation = Quaternion.Euler(angle, 0, wave * (copy.Secured ? .25f : 2.2f) * envelope * CurrentPresentationScaleV52);
                if (copy.Secured) { MaximumSecuredOffset = Mathf.Max(MaximumSecuredOffset, offset.magnitude); MaximumSecuredTilt = Mathf.Max(MaximumSecuredTilt, Mathf.Abs(angle)); }
                else { MaximumLooseOffset = Mathf.Max(MaximumLooseOffset, offset.magnitude); MaximumLooseTilt = Mathf.Max(MaximumLooseTilt, Mathf.Abs(angle)); }
            }
            SampleDust(t, envelope); SampleChips(t, envelope);
        }

        Mesh MakeMesh(string name, Material material, Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { name = name }; mesh.MarkDynamic(); mesh.vertices = vertices; mesh.triangles = triangles;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(40,15,32)); owned.Add(mesh);
            var go = new GameObject(name); go.layer = 2; go.transform.SetParent(display.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return mesh;
        }

        void CreateDust()
        {
            const int side = 48; var pixels = new Color[side * side];
            for (int y=0;y<side;y++) for (int x=0;x<side;x++)
            {
                float u=(x+.5f)/side*2-1,v=(y+.5f)/side*2-1;
                float a=Mathf.Pow(Mathf.Clamp01(1-Mathf.Sqrt(u*u+v*v)),1.6f);
                pixels[y*side+x]=new Color(1,1,1,a);
            }
            var texture = new Texture2D(side,side,TextureFormat.RGBA32,false) { name="Quake soft plaster puff",wrapMode=TextureWrapMode.Clamp };
            texture.SetPixels(pixels); texture.Apply(false,true); owned.Add(texture);
            var shader=Shader.Find("Panic/SoftWeather"); if(!shader)shader=Shader.Find("Sprites/Default");
            var material=new Material(shader) { name="Quake readable local dust",mainTexture=texture };
            material.SetColor("_Color",Color.white); owned.Add(material);
            dustVertices=new Vector3[DustCount*4];dustColors=new Color[DustCount*4];
            var triangles=new int[DustCount*6];var uv=new Vector2[DustCount*4];
            for(int i=0;i<DustCount;i++)
            {
                int v=i*4,n=i*6;triangles[n]=v;triangles[n+1]=v+1;triangles[n+2]=v+2;
                triangles[n+3]=v;triangles[n+4]=v+2;triangles[n+5]=v+3;
                uv[v]=Vector2.zero;uv[v+1]=Vector2.up;uv[v+2]=Vector2.one;uv[v+3]=Vector2.right;
            }
            dustMesh=MakeMesh("Foundation and moving roof-edge quake dust",material,dustVertices,triangles);dustMesh.uv=uv;
        }

        void SampleDust(float t,float envelope)
        {
            if(!Owner.View)return;
            Vector3 right=Owner.View.transform.right,up=Owner.View.transform.up;
            for(int i=0;i<DustCount;i++)
            {
                bool roofDust=i>=24;float h=Hash(i*11+2);
                float cycle=Mathf.Repeat(t*.7f+h,.95f)/.95f;
                Vector3 origin;
                if(roofDust)
                {
                    origin=RoofEdge(i)+Vector3.up*.06f;
                    if(housePivot)origin=housePivot.TransformPoint(origin);
                }
                else
                {
                    int edge=i%4;float along=Hash(i*3+17);
                    origin=edge<2?new Vector3(edge==0?-8.25f:8.25f,.06f,Mathf.Lerp(-5.7f,5.7f,along)):
                        new Vector3(Mathf.Lerp(-7.7f,7.7f,along),.06f,edge==2?-6.3f:6.3f);
                }
                Vector3 p=origin+new Vector3(Mathf.Sin(t*3+i)*.13f,cycle*(roofDust?.65f:1.75f),Mathf.Cos(t*2+i)*.1f);
                float size=((roofDust?.35f:.8f)+cycle*(roofDust?.60f:1.1f))*(1f+.25f*(CurrentPresentationScaleV52-1f));
                float alpha=Mathf.Sin(cycle*Mathf.PI)*(roofDust?.38f:.43f)*envelope*CurrentPresentationScaleV52;
                Vector3 r=right*size,u=up*size*(roofDust?.55f:.7f);int v=i*4;
                dustVertices[v]=p-r-u;dustVertices[v+1]=p-r+u;dustVertices[v+2]=p+r+u;dustVertices[v+3]=p+r-u;
                Color color=new Color(.69f,.64f,.53f,alpha);
                dustColors[v]=dustColors[v+1]=dustColors[v+2]=dustColors[v+3]=color;
            }
            dustMesh.vertices=dustVertices;dustMesh.colors=dustColors;
        }

        void CreateChips()
        {
            var material=new Material(Shader.Find("Standard")) { name="Small falling plaster chips",color=new Color(.77f,.71f,.58f) };
            material.SetFloat("_Glossiness",.04f); owned.Add(material);
            chipVertices=new Vector3[ChipCount*8];var triangles=new int[ChipCount*CubeFaces.Length];
            for(int i=0;i<ChipCount;i++)for(int k=0;k<CubeFaces.Length;k++)triangles[i*CubeFaces.Length+k]=i*8+CubeFaces[k];
            chipsMesh=MakeMesh("Small exterior roof-edge plaster fragments without collision",material,chipVertices,triangles);
        }

        void SampleChips(float t,float envelope)
        {
            for(int i=0;i<ChipCount;i++)
            {
                float h=Hash(i*7+3),delay=(i<16?0:.95f)+h*.65f,fall=Mathf.Max(0,t-delay);
                Vector3 p=RoofEdge(i+70);
                if(housePivot)p=housePivot.TransformPoint(p);
                Vector3 outward=new Vector3(p.x,0,p.z).normalized;
                p+=outward*(.12f+Mathf.Min(fall,1.6f)*.25f);
                p.y=Mathf.Max(.035f,p.y-4.4f*fall*fall);
                var rotation=Quaternion.Euler(i*29+fall*90,i*47+fall*70,i*11);
                float size=(.025f+h*.032f)*envelope*(t>=delay?1:0);
                for(int c=0;c<8;c++)chipVertices[i*8+c]=p+rotation*Vector3.Scale(Corners[c],new Vector3(size,size*.35f,size*.7f));
            }
            chipsMesh.vertices=chipVertices;chipsMesh.RecalculateNormals();
        }

        Vector3 RoofEdge(int i)
        {
            Bounds bounds=HouseRoofBounds;int edge=i%4;float along=Hash(i*3+17);
            float y=Mathf.Clamp(bounds.min.y+.10f,6.25f,6.6f);
            return edge<2?new Vector3(edge==0?bounds.min.x:bounds.max.x,y,Mathf.Lerp(bounds.min.z,bounds.max.z,along)):
                new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,along),y,edge==2?bounds.min.z:bounds.max.z);
        }

        public void Restore(string reason)
        {
            if(!Active&&!display&&hidden.Count==0)return;
            LastRestoredRendererCount=RestoreMismatchCount=0;
            LastRestoredHouseRendererCount=LastRestoredRoofRendererCount=0;
            foreach(var entry in hidden)if(entry.Renderer)
            {
                entry.Renderer.forceRenderingOff=entry.WasForcedOff;LastRestoredRendererCount++;
                if(entry.House)LastRestoredHouseRendererCount++;if(entry.Roof)LastRestoredRoofRendererCount++;
                if(entry.Renderer.forceRenderingOff!=entry.WasForcedOff)RestoreMismatchCount++;
            }
            hidden.Clear();copies.Clear();claimed.Clear();housePivot=null;
            if(display){display.SetActive(false);Destroy(display);}display=null;
            foreach(var asset in owned)if(asset)Destroy(asset);owned.Clear();
            Active=false;Shot="idle";CurrentPulse=0;CurrentHouseOffset=Vector3.zero;CurrentHouseTilt=0;LastResetReason=reason;RestoreCount++;
        }
        static float Hash(int x){return Mathf.Repeat(Mathf.Sin(x*127.17f+31.73f)*43758.5453f,1);}
        void OnDisable(){Restore("driver disabled");}
        void OnDestroy(){Restore("driver destroyed");}
    }
}
