using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TooLateToPanic
{
    /// <summary>
    /// Yarkin's original jam meshes, adapted as presentation only. No imported item scripts,
    /// automatic pipeline builder, colliders, scores, hazard evaluator or runtime physics.
    /// Current model, interaction and work rules remain the sole gameplay authority.
    /// </summary>
    public static class PanicCuratedAssetArt
    {
        const string Root = "CuratedYarkin/";
        static readonly Dictionary<string, GameObject> Models = new Dictionary<string, GameObject>();
        static readonly Dictionary<string, Texture2D> Icons = new Dictionary<string, Texture2D>();
        static readonly HashSet<string> Missing = new HashSet<string>();
        static readonly Dictionary<string, Mesh[]> DetailMeshes = new Dictionary<string, Mesh[]>();
        static Material[] detailMaterials;

        public static bool TryCreatePickup(string supplyId, out GameObject visual)
        {
            visual = CreatePickupVariant(supplyId, 0);
            return visual != null;
        }

        /// <summary>Variants preserve the same supply identity and quantity. Callers choose a stable index.</summary>
        public static GameObject CreatePickupVariant(string supplyId, int variant)
        {
            string id = (supplyId ?? "").ToLowerInvariant();
            string asset; float size;
            switch (id)
            {
                case "water": asset = "WaterBottle"; size = .33f; break;
                case "food":
                    int foodVariant = ((variant % 3) + 3) % 3;
                    asset = foodVariant == 1 ? "Apple" : foodVariant == 2 ? "SnackBar" : "CannedFood";
                    size = foodVariant == 1 ? .18f : foodVariant == 2 ? .22f : .23f; break;
                case "firstaid": asset = "FirstAidKit"; size = .35f; break;
                case "flashlight": asset = "Flashlight"; size = .29f; break;
                case "battery": asset = "SmallBattery"; size = .17f; break;
                case "radio": asset = "EmergencyRadio"; size = .32f; break;
                case "blanket": asset = "Blanket"; size = .38f; break;
                case "extinguisher": asset = "FireExtinguisher"; size = .46f; break;
                // Existing authored bandage, antiseptic, screws, brackets, timber and garden tools
                // retain their precise identity; unrelated Yarkin props are not substituted for them.
                default: return null;
            }
            var result = CreateDisplay(asset, size);
            if (result) result.name = "Pickup_" + id;
            return result;
        }

        /// <summary>Creates a visual with uniform scale and bottom-centre pivot. Longest dimension is in metres.</summary>
        public static GameObject CreateDisplay(string assetName, float longestDimension)
        {
            if (string.IsNullOrEmpty(assetName) || float.IsNaN(longestDimension) ||
                float.IsInfinity(longestDimension) || longestDimension <= 0) return null;
            if (!Models.TryGetValue(assetName, out var prefab))
            {
                prefab = Resources.Load<GameObject>(Root + "Models/SM_" + assetName);
                Models[assetName] = prefab;
            }
            if (!prefab)
            {
                if (Missing.Add(assetName)) Debug.LogWarning("CURATED_ART_MISSING " + assetName + "; existing visual remains available");
                return null;
            }
            var root = new GameObject("CuratedDisplay_" + assetName);
            var model = UnityEngine.Object.Instantiate(prefab, root.transform, false);
            model.name = "Visual_" + assetName;
            // Upstream FBX detail faces +Z (its icon rig also views from +Z).
            // Home's furniture/pickup convention is -Z. Rotate the visual, then fit its bounds.
            model.transform.localRotation = Quaternion.Euler(0, 180, 0) * model.transform.localRotation;
            // FBX importers disable generated colliders and animation. Keep a defensive guard
            // if a future export adds either, before it can affect a live frame.
            foreach (var c in model.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
                Remove(c);
            }
            foreach (var a in model.GetComponentsInChildren<Animator>(true)) { a.enabled = false; Remove(a); }
            foreach (var a in model.GetComponentsInChildren<Animation>(true)) { a.enabled = false; Remove(a); }
            foreach (var r in model.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            if (!TryGetLocalBounds(root.transform, out var bounds)) { Remove(root); return null; }
            float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longest <= .00001f) { Remove(root); return null; }
            model.transform.localScale *= Mathf.Clamp(longestDimension, .025f, 4f) / longest;
            TryGetLocalBounds(root.transform, out bounds);
            model.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            // Source-specific small details in the already fitted, -Z-front coordinate system.
            // Imported meshes, swatch UVs, materials and their uniform fit are never modified.
            if (TryGetLocalBounds(root.transform, out bounds)) AttachRefinements(root, assetName, bounds);
            return root;
        }

        enum DetailSurface { Ink, Paper, Metal }

        static void AttachRefinements(GameObject root, string assetName, Bounds originalBounds)
        {
            if (!DetailMeshes.TryGetValue(assetName, out var meshes))
            {
                meshes = BuildRefinements(assetName); DetailMeshes[assetName] = meshes;
            }
            if (meshes == null) return;
            float scale = Mathf.Max(originalBounds.size.x, Mathf.Max(originalBounds.size.y, originalBounds.size.z));
            // A future upstream bounds change should keep its original visual, not receive oversized trim.
            foreach (var mesh in meshes)
            {
                if (!mesh) continue;
                var b = mesh.bounds;
                Vector3 min = b.min * scale, max = b.max * scale;
                const float margin = .00002f;
                if (min.x < originalBounds.min.x - margin || min.y < originalBounds.min.y - margin || min.z < originalBounds.min.z - margin ||
                    max.x > originalBounds.max.x + margin || max.y > originalBounds.max.y + margin || max.z > originalBounds.max.z + margin)
                {
                    if (Missing.Add("detail-bounds:" + assetName)) Debug.LogWarning("CURATED_DETAIL_BOUNDS " + assetName + "; original visual retained");
                    return;
                }
            }
            if (detailMaterials == null)
            {
                var shader = Shader.Find("Standard");
                if (!shader) return;
                Color[] colors = { new Color(.075f,.105f,.13f), new Color(.88f,.85f,.74f), new Color(.50f,.58f,.59f) };
                detailMaterials = new Material[3];
                for (int i = 0; i < 3; i++)
                {
                    var mat = new Material(shader) { name = "CuratedR2_" + (DetailSurface)i, color = colors[i] };
                    mat.SetFloat("_Metallic", i == 2 ? .75f : 0f);
                    mat.SetFloat("_Glossiness", i == 2 ? .36f : .16f);
                    detailMaterials[i] = mat;
                }
            }
            for (int i = 0; i < meshes.Length; i++)
            {
                if (!meshes[i]) continue;
                var child = new GameObject("CuratedDetail_" + assetName + "_" + (DetailSurface)i);
                child.transform.SetParent(root.transform, false); child.transform.localScale = Vector3.one * scale;
                child.AddComponent<MeshFilter>().sharedMesh = meshes[i];
                var renderer = child.AddComponent<MeshRenderer>(); renderer.sharedMaterial = detailMaterials[i];
                renderer.shadowCastingMode = i == (int)DetailSurface.Metal ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }

        // Measurements are normalized from the retained Yarkin FBX geometry, after 180-degree yaw
        // and bottom-centre fitting. No runtime Mesh.vertices read, importer change or collider needed.
        static Mesh[] BuildRefinements(string assetName)
        {
            var b = new DetailBuilder();
            switch (assetName)
            {
                case "WaterBottle":
                    b.CurveGlyph(0,0,.163f,.353f,new[]{P(0,.037f),P(.025f,-.005f),P(.020f,-.028f),P(0,-.034f),P(-.020f,-.028f),P(-.025f,-.005f)},DetailSurface.Ink);
                    for (int i = 0; i < 7; i++)
                    {
                        float a = (-72 + i * 24) * Mathf.Deg2Rad;
                        b.Box(new Vector3(Mathf.Sin(a)*.0875f,.944f,-Mathf.Cos(a)*.0875f),new Vector3(.009f,.070f,.004f),DetailSurface.Ink,Quaternion.Euler(0,-a*Mathf.Rad2Deg,0));
                    }
                    break;
                case "CannedFood":
                    Wheat(b,0,0,.386f,.68f,.9f);
                    break;
                case "SnackBar":
                    for (int s = -1; s <= 1; s += 2) for (int i = 0; i < 3; i++)
                        b.Box(new Vector3(s*(.438f+i*.020f),.140f,0),new Vector3(.005f,.003f,.29f),DetailSurface.Ink,Quaternion.identity);
                    // A large grain mark on the wrap, rather than illegible miniature brand copy.
                    Wheat(b,.05f,0,.196f,.12f,.55f);
                    break;
                case "FirstAidKit":
                    b.Box(new Vector3(0,.207f,-.283f),new Vector3(.87f,.009f,.004f),DetailSurface.Ink,Quaternion.identity);
                    for (int s = -1; s <= 1; s += 2) for (int r = 0; r < 2; r++)
                        b.Box(new Vector3(s*.409f,.064f+r*.286f,-.274f),new Vector3(.060f,.046f,.008f),DetailSurface.Metal,Quaternion.identity);
                    break;
                case "Flashlight":
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i*Mathf.PI*.25f;
                        b.Box(new Vector3(Mathf.Sin(a)*.103f,.19f+Mathf.Cos(a)*.103f,.373f),new Vector3(.018f,.008f,.12f),DetailSurface.Ink,Quaternion.Euler(0,0,-a*Mathf.Rad2Deg));
                    }
                    break;
                case "SmallBattery":
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float x = s*.192f;
                        b.CurveGlyph(x,0,.167f,.31f,new[]{P(-.039f,-.009f),P(.039f,-.009f),P(.039f,.009f),P(-.039f,.009f)},DetailSurface.Paper);
                        b.CurveGlyph(x,0,.1675f,.31f,new[]{P(-.009f,-.039f),P(.009f,-.039f),P(.009f,.039f),P(-.009f,.039f)},DetailSurface.Paper);
                    }
                    break;
                case "EmergencyRadio":
                    for (int i = 0; i < 6; i++)
                    {
                        float y=.112f+i*.043f, dy=y-.225f;
                        float w=2*Mathf.Sqrt(Mathf.Max(.001f,.142f*.142f-dy*dy));
                        b.Box(new Vector3(.244f,y,-.154f),new Vector3(w,.010f,.003f),DetailSurface.Ink,Quaternion.identity);
                    }
                    for (int i = 0; i < 5; i++) b.Box(new Vector3(-.169f+i*.042f,.300f,-.151f),new Vector3(.007f,i%2==0?.034f:.023f,.003f),DetailSurface.Ink,Quaternion.identity);
                    b.Box(new Vector3(-.087f,.330f,-.153f),new Vector3(.008f,.061f,.003f),DetailSurface.Metal,Quaternion.identity);
                    break;
                case "Blanket":
                    // Short hems lie on measured cloth surfaces and stop before the existing straps.
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.TopHem(s*.41f,-.30f,.3149f,-.23f,.3156f);
                        b.TopHem(s*.41f,-.07f,.3160f,.035f,.3161f);
                        b.TopHem(s*.41f,.20f,.3158f,.30f,.3271f);
                    }
                    break;
                case "FireExtinguisher":
                    b.CurveGlyph(-.07f,-.023f,.151f,.397f,new[]{P(0,.105f),P(.049f,.025f),P(.047f,-.055f),P(0,-.092f),P(-.047f,-.055f),P(-.035f,.025f)},DetailSurface.Ink);
                    b.CurveGlyph(-.07f,-.023f,.152f,.36f,new[]{P(0,.047f),P(.020f,-.008f),P(0,-.030f),P(-.020f,-.008f)},DetailSurface.Paper);
                    break;
                // Apple's existing leaf, stem and organic silhouette already carry its identity.
                // Do not bolt extra ornaments onto it or invent a new food/utility identity.
                default: return null;
            }
            return b.Finish(assetName);
        }

        static Vector2 P(float x,float y) { return new Vector2(x,y); }
        static void Wheat(DetailBuilder b,float x,float z,float radius,float y,float size)
        {
            b.CurveGlyph(x,z,radius,y,new[]{P(-.005f*size,-.085f*size),P(.005f*size,-.085f*size),P(.005f*size,.095f*size),P(-.005f*size,.095f*size)},DetailSurface.Paper);
            for (int s = -1; s <= 1; s += 2) for (int i = 0; i < 2; i++)
            {
                float cy=(-.025f+i*.063f)*size;
                b.CurveGlyph(x,z,radius+.0006f,y,new[]{P(s*.008f*size,cy),P(s*.047f*size,cy+.013f*size),P(s*.044f*size,cy+.043f*size),P(s*.008f*size,cy+.024f*size)},DetailSurface.Paper);
            }
        }

        sealed class DetailBuilder
        {
            readonly List<Vector3>[] vertices={new List<Vector3>(),new List<Vector3>(),new List<Vector3>()};
            readonly List<int>[] triangles={new List<int>(),new List<int>(),new List<int>()};
            void Face(DetailSurface surface,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            {
                int m=(int)surface,n=vertices[m].Count;vertices[m].AddRange(new[]{a,b,c,d});
                triangles[m].AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            public void Box(Vector3 p,Vector3 size,DetailSurface surface,Quaternion rotation)
            {
                Vector3 h=size*.5f;
                Vector3[] v=new Vector3[8];for(int i=0;i<8;i++)v[i]=p+rotation*new Vector3((i&1)==0?-h.x:h.x,(i&2)==0?-h.y:h.y,(i&4)==0?-h.z:h.z);
                Face(surface,v[0],v[2],v[3],v[1]);Face(surface,v[4],v[5],v[7],v[6]);
                Face(surface,v[0],v[4],v[6],v[2]);Face(surface,v[1],v[3],v[7],v[5]);
                Face(surface,v[0],v[1],v[5],v[4]);Face(surface,v[2],v[6],v[7],v[3]);
            }
            public void CurveGlyph(float x,float z,float radius,float y,Vector2[] outline,DetailSurface surface)
            {
                int m=(int)surface;float area=0;
                for(int i=0;i<outline.Length;i++)
                {
                    Vector2 p=outline[i],next=outline[(i+1)%outline.Length];area+=p.x*next.y-next.x*p.y;
                }
                // A single flat quad cuts through a curved label. Subdivide in the print plane
                // before wrapping, so every small triangle remains on the visible cylinder skin.
                for(int i=1;i<outline.Length-1;i++)
                {
                    Vector2 a=outline[0],b=outline[i],c=outline[i+1];
                    float span=Mathf.Max(Mathf.Abs(a.x-b.x),Mathf.Max(Mathf.Abs(a.x-c.x),Mathf.Abs(b.x-c.x)));
                    int steps=Mathf.Max(1,Mathf.CeilToInt(span/.018f));
                    Action<Vector2,Vector2,Vector2> emit=(p,q,r)=>
                    {
                        int n=vertices[m].Count;
                        vertices[m].Add(Wrap(x,z,radius,y,p));vertices[m].Add(Wrap(x,z,radius,y,q));vertices[m].Add(Wrap(x,z,radius,y,r));
                        triangles[m].Add(n);triangles[m].Add(n+(area>0?2:1));triangles[m].Add(n+(area>0?1:2));
                    };
                    for(int u=0;u<steps;u++)for(int v=0;v<steps-u;v++)
                    {
                        Vector2 p=Blend(a,b,c,(float)u/steps,(float)v/steps),q=Blend(a,b,c,(float)(u+1)/steps,(float)v/steps),r=Blend(a,b,c,(float)u/steps,(float)(v+1)/steps);
                        emit(p,q,r);
                        if(u+v<steps-1)emit(q,Blend(a,b,c,(float)(u+1)/steps,(float)(v+1)/steps),r);
                    }
                }
            }
            static Vector2 Blend(Vector2 a,Vector2 b,Vector2 c,float u,float v)
            {return new Vector2(a.x+(b.x-a.x)*u+(c.x-a.x)*v,a.y+(b.y-a.y)*u+(c.y-a.y)*v);}
            static Vector3 Wrap(float x,float z,float radius,float y,Vector2 p)
            {float a=p.x/radius;return new Vector3(x+Mathf.Sin(a)*radius,y+p.y,z-Mathf.Cos(a)*radius);}
            public void TopHem(float x,float z0,float y0,float z1,float y1)
            {Face(DetailSurface.Paper,new Vector3(x-.004f,y0,z0),new Vector3(x-.004f,y1,z1),new Vector3(x+.004f,y1,z1),new Vector3(x+.004f,y0,z0));}
            public Mesh[] Finish(string assetName)
            {
                var result=new Mesh[3];for(int i=0;i<3;i++)
                {
                    if(vertices[i].Count==0)continue;
                    var mesh=new Mesh{name="CuratedR2_"+assetName+"_"+(DetailSurface)i};
                    // Unity's FBX import reflects source X before the retained 180-degree yaw.
                    // Match that handedness only in our authored trim, with outward winding intact.
                    for(int j=0;j<vertices[i].Count;j++)
                    {Vector3 p=vertices[i][j];p.x=-p.x;vertices[i][j]=p;}
                    for(int j=0;j<triangles[i].Count;j+=3)
                    {int index=triangles[i][j+1];triangles[i][j+1]=triangles[i][j+2];triangles[i][j+2]=index;}
                    mesh.SetVertices(vertices[i]);mesh.SetTriangles(triangles[i],0);
                    var uv=new Vector2[vertices[i].Count];for(int j=0;j<uv.Length;j++)uv[j]=new Vector2(vertices[i][j].x+vertices[i][j].z,vertices[i][j].y);
                    mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();result[i]=mesh;
                }
                return result;
            }
        }

        /// <summary>Imported renders are optional; original raster icons remain the fallback.</summary>
        public static bool TryGetIcon(string supplyId, out Texture2D icon)
        {
            string asset;
            switch ((supplyId ?? "").ToLowerInvariant())
            {
                case "water": asset = "WaterBottle"; break;
                case "food": asset = "CannedFood"; break;
                case "firstaid": asset = "FirstAidKit"; break;
                case "flashlight": asset = "Flashlight"; break;
                case "battery": asset = "SmallBattery"; break;
                case "radio": asset = "EmergencyRadio"; break;
                case "blanket": asset = "Blanket"; break;
                case "extinguisher": asset = "FireExtinguisher"; break;
                default: icon = null; return false;
            }
            if (!Icons.TryGetValue(asset, out icon))
            {
                icon = Resources.Load<Texture2D>(Root + "Icons/T_Icon_" + asset);
                Icons[asset] = icon;
            }
            return icon != null;
        }

        public static bool TryGetLocalBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds(); bool any = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                Bounds b = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    corner = root.InverseTransformPoint(filter.transform.TransformPoint(corner));
                    if (!any) { bounds = new Bounds(corner, Vector3.zero); any = true; }
                    else bounds.Encapsulate(corner);
                }
            }
            return any;
        }

        static void Remove(UnityEngine.Object obj)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
