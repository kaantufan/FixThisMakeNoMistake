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
                CinematicKind == "Disaster" && Model.Disaster == DisasterType.Earthquake &&
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
                quakeReadabilityV414.Begin(candidates);
            }
            if (residenceCinematicAge < 6.65f && quakeReadabilityV414.SubjectCount > 0)
            {
                // A real camera inside the west living room, below its intact ceiling.
                // No cutaway/roof or facade state is changed for this shot.
                Vector3 focus = quakeReadabilityV414.RoomFocus;
                camera = new Vector3(Mathf.Clamp(focus.x + 4.0f, -6.4f, -1.7f), 1.72f,
                    Mathf.Clamp(focus.z - 3.25f, -5.2f, 4.7f));
                target = focus + Vector3.up * 1.05f;
                fov = 58f;
                CinematicTitle = "Deprem: eşyalar sarsılıyor";
            }
            else
            {
                // Stable, fully roofed home and foundation dust. Keep the house readable.
                camera = new Vector3(19f, 12.2f, -24f);
                target = new Vector3(1.5f, 2.25f, .25f);
                fov = 50f;
                CinematicTitle = "Deprem sürüyor";
            }
            float t = residenceCinematicAge - 4.7f;
            float envelope = Mathf.SmoothStep(0, 1, t / .18f) *
                (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(7.7f, 8f, residenceCinematicAge)));
            camera += new Vector3(Mathf.Sin(t * 32) * .022f, Mathf.Sin(t * 43) * .012f, 0) * envelope;
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
        sealed class Copy
        {
            public string Id;
            public Transform Pivot;
            public Vector3 Origin;
            public bool Secured;
            public float Height, Phase;
        }
        struct Hidden { public Renderer Renderer; public bool WasForcedOff; }
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
        GameObject display;
        Mesh dustMesh, chipsMesh;
        Vector3[] dustVertices, chipVertices;
        Color[] dustColors;
        const int DustCount = 42, ChipCount = 32;
        static readonly Vector3[] Corners = { new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),
            new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1) };
        static readonly int[] CubeFaces = { 0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,0,1,5,0,5,4,3,7,6,3,6,2 };

        public void Begin(List<Subject> candidates)
        {
            if (Active) return;
            display = new GameObject("Temporary readable earthquake V414");
            display.transform.SetParent(transform, true);
            display.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            display.transform.localScale = Vector3.one;
            SampledFrames = SourceRendererCount = ProxyRendererCount = SecuredSubjects = LooseSubjects = 0;
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
                    if (!source.enabled || !source.gameObject.activeInHierarchy || source.forceRenderingOff ||
                        source.GetComponent<PanicPermanentlyHiddenV414>()) continue;
                    var filter = source.GetComponent<MeshFilter>();
                    if (!filter || !filter.sharedMesh) continue;
                    var part = new GameObject("Display mesh " + source.name); part.layer = 2;
                    part.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                    part.transform.localScale = source.transform.lossyScale;
                    part.transform.SetParent(pivot, true);
                    part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                    var renderer = part.AddComponent<MeshRenderer>(); renderer.sharedMaterials = source.sharedMaterials;
                    renderer.shadowCastingMode = source.shadowCastingMode; renderer.receiveShadows = source.receiveShadows;
                    var block = new MaterialPropertyBlock(); source.GetPropertyBlock(block); renderer.SetPropertyBlock(block);
                    hidden.Add(new Hidden { Renderer = source, WasForcedOff = source.forceRenderingOff });
                    source.forceRenderingOff = true; SourceRendererCount++; ProxyRendererCount++;
                }
                if (hidden.Count == countBefore) { Destroy(pivot.gameObject); continue; }
                copies.Add(new Copy { Id = subject.Item.Id, Pivot = pivot, Origin = pivot.position,
                    Secured = subject.Item.Anchored, Height = subject.Item.Size.y, Phase = copies.Count * .71f });
                if (subject.Item.Anchored) SecuredSubjects++; else LooseSubjects++;
            }
            CreateDust(); CreateChips();
            var light = new GameObject("Local cinematic room fill").AddComponent<Light>();
            light.transform.SetParent(display.transform, false); light.transform.position = RoomFocus + new Vector3(1.5f, 2.6f, -.8f);
            light.type = LightType.Point; light.range = 7; light.intensity = .85f; light.color = new Color(1,.87f,.70f);
            Active = true;
        }

        void LateUpdate()
        {
            if (!Owner || !Owner.QuakeReadabilityWindowV414) { Restore("window ended or owner disabled"); return; }
            if (Owner.Model.IsPaused || Owner.NotebookBlocksInput || Owner.InputLocked)
            { Restore("paused or presentation suspended"); return; }
            if (!Active || !display) return;
            float age = Owner.QuakeReadabilityAgeV414, t = age - 4.7f;
            SampledAge = age; SampledFrames++; Shot = age < 6.65f && SubjectCount > 0 ? "room" : "roofed exterior";
            float envelope = Mathf.SmoothStep(0,1,t/.16f) * (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(7.72f,8,age)));
            CurrentPulse = envelope * (.65f + .35f * Mathf.Pow(Mathf.Sin(t * 9), 2));
            foreach (var copy in copies)
            {
                if (!copy.Pivot) continue;
                float wave = Mathf.Sin(t * 17 + copy.Phase) * .7f + Mathf.Sin(t * 29 + copy.Phase) * .3f;
                float height = Mathf.Clamp01(copy.Height / 2.1f);
                float angle = (copy.Secured ? wave * .65f : wave * (7.5f + height * 3.5f) +
                    Mathf.SmoothStep(0,1,t/.7f) * (3 + height * 3)) * envelope;
                float shift = copy.Secured ? wave * .006f : wave * .12f + Mathf.SmoothStep(0,1,t/.9f) * .12f;
                Vector3 offset = new Vector3(shift, 0, wave * (copy.Secured ? .003f : .055f)) * envelope;
                copy.Pivot.position = copy.Origin + offset;
                copy.Pivot.rotation = Quaternion.Euler(angle, 0, wave * (copy.Secured ? .25f : 2.2f) * envelope);
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
            dustMesh=MakeMesh("Local and foundation quake dust",material,dustVertices,triangles);dustMesh.uv=uv;
        }

        void SampleDust(float t,float envelope)
        {
            if(!Owner.View)return;
            Vector3 right=Owner.View.transform.right,up=Owner.View.transform.up;
            for(int i=0;i<DustCount;i++)
            {
                bool outdoor=i>=18;float h=Hash(i*11+2),k=Hash(i*7+9);
                float cycle=Mathf.Repeat(t*.7f+h,.95f)/.95f;
                Vector3 origin;
                if(!outdoor)origin=RoomFocus+new Vector3((h-.5f)*3.5f,.1f,(k-.5f)*2.3f);
                else
                {
                    int edge=(i-18)%4;float along=Hash(i*3+17);
                    origin=edge<2?new Vector3(edge==0?-8.25f:8.25f,.06f,Mathf.Lerp(-5.7f,5.7f,along)):
                        new Vector3(Mathf.Lerp(-7.7f,7.7f,along),.06f,edge==2?-6.3f:6.3f);
                }
                Vector3 p=origin+new Vector3(Mathf.Sin(t*3+i)*.13f,cycle*(outdoor?1.75f:1.05f),Mathf.Cos(t*2+i)*.1f);
                float size=(outdoor?.8f:.20f)+cycle*(outdoor?1.1f:.40f);
                float alpha=Mathf.Sin(cycle*Mathf.PI)*(outdoor?.43f:.42f)*envelope;
                Vector3 r=right*size,u=up*size*(outdoor?.7f:1.2f);int v=i*4;
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
            chipsMesh=MakeMesh("Ceiling plaster fragments without collision",material,chipVertices,triangles);
        }

        void SampleChips(float t,float envelope)
        {
            for(int i=0;i<ChipCount;i++)
            {
                float h=Hash(i*7+3),k=Hash(i*13+5),fall=Mathf.Max(0,t-h*.75f);
                float y=Mathf.Max(.035f,2.94f-3.4f*fall*fall);
                Vector3 p=RoomFocus+new Vector3((h-.5f)*3.1f+Mathf.Min(fall,1.2f)*(k-.5f)*.3f,y,(k-.5f)*2.4f);
                var rotation=Quaternion.Euler(i*29+fall*90,i*47+fall*70,i*11);
                float size=(.017f+h*.025f)*envelope*(t>=h*.75f?1:0);
                for(int c=0;c<8;c++)chipVertices[i*8+c]=p+rotation*Vector3.Scale(Corners[c],new Vector3(size,size*.35f,size*.7f));
            }
            chipsMesh.vertices=chipVertices;chipsMesh.RecalculateNormals();
        }

        public void Restore(string reason)
        {
            if(!Active&&!display&&hidden.Count==0)return;
            LastRestoredRendererCount=RestoreMismatchCount=0;
            foreach(var entry in hidden)if(entry.Renderer)
            {
                entry.Renderer.forceRenderingOff=entry.WasForcedOff;LastRestoredRendererCount++;
                if(entry.Renderer.forceRenderingOff!=entry.WasForcedOff)RestoreMismatchCount++;
            }
            hidden.Clear();copies.Clear();
            if(display){display.SetActive(false);Destroy(display);}display=null;
            foreach(var asset in owned)if(asset)Destroy(asset);owned.Clear();
            Active=false;Shot="idle";CurrentPulse=0;LastResetReason=reason;RestoreCount++;
        }
        static float Hash(int x){return Mathf.Repeat(Mathf.Sin(x*127.17f+31.73f)*43758.5453f,1);}
        void OnDisable(){Restore("driver disabled");}
        void OnDestroy(){Restore("driver destroyed");}
    }
}
