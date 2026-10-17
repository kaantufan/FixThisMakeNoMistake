import bpy
import bmesh
import math
import os
from mathutils import Vector, Euler

OUT_MESH_DIR = r"c:\GameJam\Assets\_YarkinAssets\Art\Meshes"
OUT_BLEND_DIR = r"c:\GameJam\Assets\_YarkinAssets\Art\Source_Blend~"
OUT_TEX_DIR = r"c:\GameJam\Assets\_YarkinAssets\Art\Textures\Packed"

os.makedirs(OUT_MESH_DIR, exist_ok=True)
os.makedirs(OUT_BLEND_DIR, exist_ok=True)
os.makedirs(OUT_TEX_DIR, exist_ok=True)

# Redesigned Semantic 4-Tier Disaster Color Palette (sRGB RGBA, Metallic, Roughness)
PALETTE = {
    # Tier 1: Emergency & Warning
    "SafetyOrange":   ((1.00, 0.34, 0.13, 1.0), 0.05, 0.45),
    "Red":            ((0.85, 0.16, 0.16, 1.0), 0.08, 0.40),
    "DarkRed":        ((0.52, 0.08, 0.08, 1.0), 0.05, 0.55),
    "Yellow":         ((1.00, 0.72, 0.02, 1.0), 0.10, 0.40),
    "Orange":         ((0.96, 0.46, 0.05, 1.0), 0.08, 0.42),

    # Tier 2: Vital / Hydration / Medical / Nutrition
    "WaterBlue":      ((0.02, 0.70, 0.86, 1.0), 0.00, 0.18),
    "Blue":           ((0.10, 0.44, 0.82, 1.0), 0.08, 0.35),
    "NavyBlue":       ((0.14, 0.24, 0.40, 1.0), 0.05, 0.65),
    "Green":          ((0.16, 0.72, 0.48, 1.0), 0.05, 0.40),
    "OliveGreen":     ((0.28, 0.38, 0.22, 1.0), 0.00, 0.72),
    "NeonGreen":      ((0.25, 0.92, 0.20, 1.0), 0.15, 0.28),

    # Tier 3: Hardware, Metals & Polymers (No pitch black!)
    "RubberBlack":    ((0.17, 0.19, 0.23, 1.0), 0.15, 0.62),  # Tactical Slate
    "DarkMetal":      ((0.26, 0.29, 0.34, 1.0), 0.78, 0.35),
    "SilverMetal":    ((0.80, 0.84, 0.87, 1.0), 0.88, 0.24),
    "Brass":          ((0.88, 0.65, 0.24, 1.0), 0.82, 0.28),
    "Copper":         ((0.82, 0.45, 0.22, 1.0), 0.78, 0.30),
    "White":          ((0.96, 0.97, 0.98, 1.0), 0.05, 0.38),
    "OffWhite":       ((0.88, 0.86, 0.81, 1.0), 0.00, 0.60),
    "ScreenBlack":    ((0.08, 0.10, 0.14, 1.0), 0.25, 0.15),

    # Tier 4: Domestic & Organic
    "BrownWood":      ((0.42, 0.26, 0.15, 1.0), 0.00, 0.58),
    "ChocBrown":      ((0.30, 0.17, 0.09, 1.0), 0.05, 0.48),
    "FabricBeige":    ((0.78, 0.71, 0.58, 1.0), 0.00, 0.80),
    "FabricBlue":     ((0.24, 0.40, 0.60, 1.0), 0.00, 0.78),
    "RugBurgundy":    ((0.55, 0.14, 0.20, 1.0), 0.00, 0.82),
    "RugGold":        ((0.86, 0.68, 0.28, 1.0), 0.00, 0.75),

    # Emissive / Highlights
    "GlowWarm":       ((1.00, 0.90, 0.55, 1.0), 0.00, 0.10),
    "GlowGreen":      ((0.05, 0.95, 0.48, 1.0), 0.00, 0.12),
    "GlassCyan":      ((0.55, 0.88, 0.96, 1.0), 0.10, 0.10),
}

PALETTE_KEYS = list(PALETTE.keys())

def generate_color_swatch_png():
    """Generates a 256x256 8x4 grid color swatch texture for single-draw-call atlas mapping."""
    width, height = 256, 256
    cols, rows = 8, 4
    cell_w, cell_h = width // cols, height // rows
    img = bpy.data.images.new("T_DisasterColorSwatch_256", width=width, height=height, alpha=True)
    pixels = [1.0] * (width * height * 4)

    for idx, key in enumerate(PALETTE_KEYS):
        rgba, _, _ = PALETTE[key]
        c = idx % cols
        r = idx // cols
        for y in range(r * cell_h, (r + 1) * cell_h):
            for x in range(c * cell_w, (c + 1) * cell_w):
                p_idx = (y * width + x) * 4
                pixels[p_idx + 0] = rgba[0]
                pixels[p_idx + 1] = rgba[1]
                pixels[p_idx + 2] = rgba[2]
                pixels[p_idx + 3] = 1.0

    img.pixels = pixels
    swatch_path = os.path.join(OUT_TEX_DIR, "T_DisasterColorSwatch_256.png")
    img.filepath_raw = swatch_path
    img.file_format = 'PNG'
    img.save()
    print(f"[OK] Saved Color Swatch Atlas: {swatch_path}")

def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0

def get_mat(key):
    if key not in PALETTE:
        key = "White"
    mat_name = f"M_{key}"
    if mat_name in bpy.data.materials:
        return bpy.data.materials[mat_name]
    mat = bpy.data.materials.new(name=mat_name)
    mat.use_nodes = True
    rgba, met, rough = PALETTE[key]
    mat.diffuse_color = rgba
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Metallic"].default_value = met
        bsdf.inputs["Roughness"].default_value = rough
    return mat

def assign_mat_and_uv(obj, key):
    if key not in PALETTE:
        key = "White"
    mat = get_mat(key)
    obj.data.materials.clear()
    obj.data.materials.append(mat)

    # Also project UVs into the corresponding cell of the 8x4 swatch grid
    idx = PALETTE_KEYS.index(key)
    cols, rows = 8, 4
    u_center = ((idx % cols) + 0.5) / cols
    v_center = ((idx // rows if False else (idx // cols)) + 0.5) / rows
    if not obj.data.uv_layers:
        obj.data.uv_layers.new(name="UVMap")
    uv_layer = obj.data.uv_layers.active.data
    for poly in obj.data.polygons:
        for loop_idx in poly.loop_indices:
            uv_layer[loop_idx].uv = (u_center, v_center)

def apply_bevel(obj, width, segments=4):
    if width <= 0.0:
        return
    bpy.context.view_layer.objects.active = obj
    mod = obj.modifiers.new(name="Bevel", type='BEVEL')
    mod.width = width
    mod.segments = segments
    mod.limit_method = 'ANGLE'
    mod.angle_limit = math.radians(30.0)
    bpy.ops.object.modifier_apply(modifier=mod.name)

def add_box(name, size, loc, mat_key, rot=(0,0,0), bevel=0.0, segs=4, subsurf=0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    auto_bevel = bevel if bevel > 0.0 else min(size) * 0.15
    apply_bevel(obj, auto_bevel, segments=segs)
    if subsurf > 0:
        mod = obj.modifiers.new(name="Subsurf", type='SUBSURF')
        mod.levels = subsurf
        mod.render_levels = subsurf
        bpy.ops.object.modifier_apply(modifier=mod.name)
    assign_mat_and_uv(obj, mat_key)
    return obj

def add_cyl(name, r, depth, loc, mat_key, verts=36, rot=(0,0,0), bevel=0.0, segs=3):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    auto_bevel = bevel if bevel > 0.0 else min(r, depth) * 0.14
    apply_bevel(obj, auto_bevel, segments=segs)
    assign_mat_and_uv(obj, mat_key)
    return obj

def add_cone(name, r1, r2, depth, loc, mat_key, verts=36, rot=(0,0,0), bevel=0.0):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if bevel > 0.0:
        apply_bevel(obj, bevel, segments=3)
    assign_mat_and_uv(obj, mat_key)
    return obj

def add_sphere(name, r, loc, mat_key, segs=36, rings=24, scale=(1,1,1), rot=(0,0,0)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segs, ring_count=rings, radius=r, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    assign_mat_and_uv(obj, mat_key)
    return obj

def add_torus(name, major_r, minor_r, loc, mat_key, major_seg=36, minor_seg=16, rot=(0,0,0), scale=(1,1,1)):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_r,
        minor_radius=minor_r,
        major_segments=major_seg,
        minor_segments=minor_seg,
        location=loc,
        rotation=rot
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    assign_mat_and_uv(obj, mat_key)
    return obj

def add_hollow_frustum(name, r_in_bot, r_out_bot, r_in_top, r_out_top, depth, loc, mat_key, segs=48, bevel=0.006, bevel_segs=3):
    """Creates a manifold quad-mesh hollow cylindrical tube or conical frustum centered at loc."""
    mesh = bpy.data.meshes.new(name + "_Mesh")
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.location = loc

    bm = bmesh.new()
    half_d = depth * 0.5
    in_bot, out_bot, out_top, in_top = [], [], [], []
    for i in range(segs):
        ang = 2.0 * math.pi * i / segs
        ca, sa = math.cos(ang), math.sin(ang)
        in_bot.append(bm.verts.new((r_in_bot * ca, r_in_bot * sa, -half_d)))
        out_bot.append(bm.verts.new((r_out_bot * ca, r_out_bot * sa, -half_d)))
        out_top.append(bm.verts.new((r_out_top * ca, r_out_top * sa, half_d)))
        in_top.append(bm.verts.new((r_in_top * ca, r_in_top * sa, half_d)))

    for i in range(segs):
        j = (i + 1) % segs
        bm.faces.new((in_bot[i], in_top[i], in_top[j], in_bot[j]))     # Inner wall (points inward)
        bm.faces.new((out_bot[i], out_bot[j], out_top[j], out_top[i])) # Outer wall (points outward)
        bm.faces.new((in_top[i], out_top[i], out_top[j], in_top[j]))   # Top annular face (+Z)
        bm.faces.new((out_bot[i], in_bot[i], in_bot[j], out_bot[j]))   # Bottom annular face (-Z)

    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()

    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    if bevel > 0.0:
        apply_bevel(obj, bevel, segments=bevel_segs)
    assign_mat_and_uv(obj, mat_key)
    return obj

def finalize_and_export(asset_name, parts, toybox_scale=1.0):
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = f"SM_{asset_name}"

    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if toybox_scale != 1.0:
        for v in obj.data.vertices:
            v.co *= toybox_scale
        obj.data.update()

    for poly in obj.data.polygons:
        poly.use_smooth = True

    verts = [v.co for v in obj.data.vertices]
    min_x = min(v.x for v in verts)
    max_x = max(v.x for v in verts)
    min_y = min(v.y for v in verts)
    max_y = max(v.y for v in verts)
    min_z = min(v.z for v in verts)

    cx = (min_x + max_x) * 0.5
    cy = (min_y + max_y) * 0.5
    offset = Vector((-cx, -cy, -min_z))
    for v in obj.data.vertices:
        v.co += offset
    obj.data.update()

    blend_path = os.path.join(OUT_BLEND_DIR, f"SM_{asset_name}.blend")
    fbx_path = os.path.join(OUT_MESH_DIR, f"SM_{asset_name}.fbx")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_NONE',
        axis_forward='-Z',
        axis_up='Y',
        use_mesh_modifiers=True,
        mesh_smooth_type='EDGE',
        bake_space_transform=True,
        object_types={'MESH'}
    )
    print(f"[OK] Exported SM_{asset_name}.fbx (scale={toybox_scale}x, {len(obj.data.polygons)} polys)")

def finalize_and_export_multipart(asset_name, body_name, body_parts, moving_groups, toybox_scale=1.0, keep_exact_origin=False):
    """
    Exports a multi-part FBX hierarchy where the static body (`body_name`) has its origin at the
    bottom-center of the combined assembly (or exact (0,0,0) if keep_exact_origin=True), and each
    moving sub-mesh (`child_name`) has its local origin at its exact mechanical axle/hinge pivot.
    """
    bpy.ops.object.select_all(action='DESELECT')
    for p in body_parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = body_parts[0]
    if len(body_parts) > 1:
        bpy.ops.object.join()
    body_obj = bpy.context.active_object
    body_obj.name = body_name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if toybox_scale != 1.0:
        for v in body_obj.data.vertices:
            v.co *= toybox_scale
        body_obj.data.update()
    for poly in body_obj.data.polygons:
        poly.use_smooth = True

    joined_children = []
    for child_name, child_parts, pivot_xyz in moving_groups:
        bpy.ops.object.select_all(action='DESELECT')
        for p in child_parts:
            p.select_set(True)
        bpy.context.view_layer.objects.active = child_parts[0]
        if len(child_parts) > 1:
            bpy.ops.object.join()
        c_obj = bpy.context.active_object
        c_obj.name = child_name
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        if toybox_scale != 1.0:
            for v in c_obj.data.vertices:
                v.co *= toybox_scale
            c_obj.data.update()
        for poly in c_obj.data.polygons:
            poly.use_smooth = True
        joined_children.append((c_obj, Vector(pivot_xyz) * toybox_scale))

    all_objs = [body_obj] + [c[0] for c in joined_children]
    if keep_exact_origin:
        offset = Vector((0.0, 0.0, 0.0))
    else:
        all_verts = [v.co for o in all_objs for v in o.data.vertices]
        min_x = min(v.x for v in all_verts)
        max_x = max(v.x for v in all_verts)
        min_y = min(v.y for v in all_verts)
        max_y = max(v.y for v in all_verts)
        min_z = min(v.z for v in all_verts)

        cx = (min_x + max_x) * 0.5
        cy = (min_y + max_y) * 0.5
        offset = Vector((-cx, -cy, -min_z))

    for v in body_obj.data.vertices:
        v.co += offset
    body_obj.data.update()
    body_obj.location = (0.0, 0.0, 0.0)

    total_polys = len(body_obj.data.polygons)
    for c_obj, pivot_scaled in joined_children:
        for v in c_obj.data.vertices:
            v.co -= pivot_scaled
        c_obj.data.update()
        c_obj.location = pivot_scaled + offset
        total_polys += len(c_obj.data.polygons)

    bpy.ops.object.select_all(action='DESELECT')
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = body_obj

    blend_path = os.path.join(OUT_BLEND_DIR, f"SM_{asset_name}.blend")
    fbx_path = os.path.join(OUT_MESH_DIR, f"SM_{asset_name}.fbx")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_NONE',
        axis_forward='-Z',
        axis_up='Y',
        use_mesh_modifiers=True,
        mesh_smooth_type='EDGE',
        bake_space_transform=True,
        object_types={'MESH'}
    )
    print(f"[OK] Exported Multi-Part SM_{asset_name}.fbx ({len(all_objs)} parts, scale={toybox_scale}x, {total_polys} polys)")

# ==============================================================================
# PART A: ORIGINAL 18 ASSETS (Upgraded Isometric Silhouette + Toy-Box Scale)
# ==============================================================================

# 1. FireExtinguisher (Yangın Tüpü - 1.20x, flared hose & large gauge)
def build_fire_extinguisher():
    clear_scene()
    parts = [
        add_cyl("TankBody", 0.078, 0.34, (0, 0, 0.19), "Red", 40, bevel=0.010),
        add_sphere("DomeTop", 0.078, (0, 0, 0.36), "Red", 40, 20, (1, 1, 0.85)),
        add_cyl("RubberSkirt", 0.082, 0.050, (0, 0, 0.025), "RubberBlack", 40, bevel=0.008),
        add_cyl("InstructionBand", 0.0795, 0.16, (0, 0, 0.21), "White", 40, bevel=0.002),
        add_cyl("StripeTop", 0.080, 0.022, (0, 0, 0.28), "Yellow", 40, bevel=0.001),
        add_torus("BeltUpper", 0.080, 0.006, (0, 0, 0.31), "RubberBlack", 40, 14),
        add_torus("BeltLower", 0.080, 0.006, (0, 0, 0.10), "RubberBlack", 40, 14),
        add_cyl("ValveNeck", 0.032, 0.028, (0, 0, 0.425), "Brass", 28, bevel=0.004),
        add_cyl("ValveBody", 0.026, 0.060, (0, 0, 0.460), "Brass", 28, bevel=0.005),
        add_box("HandleLower", (0.026, 0.125, 0.015), (0, 0.046, 0.472), "DarkMetal", rot=(math.radians(-6), 0, 0), bevel=0.005),
        add_box("HandleLever", (0.028, 0.130, 0.016), (0, 0.048, 0.504), "Red", rot=(math.radians(18), 0, 0), bevel=0.005),
        add_cyl("GaugeHousing", 0.022, 0.020, (0.032, 0, 0.456), "Brass", 24, rot=(0, math.radians(90), 0), bevel=0.003),
        add_cyl("GaugeDial", 0.018, 0.005, (0.042, 0, 0.456), "GlowGreen", 24, rot=(0, math.radians(90), 0), bevel=0.001),
        add_torus("PullPinRing", 0.018, 0.0045, (-0.026, -0.016, 0.475), "Yellow", 28, 12, rot=(math.radians(90), 0, 0)),
        # Wide flared hose loop for instant isometric silhouette recognition
        add_torus("HoseFlareArch", 0.058, 0.013, (-0.065, 0, 0.415), "RubberBlack", 32, 14, rot=(math.radians(90), 0, 0)),
        add_cyl("HosePipe", 0.013, 0.23, (-0.118, 0, 0.295), "RubberBlack", 24, rot=(0, math.radians(-5), 0), bevel=0.003),
        add_cone("DischargeHorn", 0.036, 0.015, 0.13, (-0.125, 0, 0.125), "RubberBlack", 32, bevel=0.004),
    ]
    finalize_and_export("FireExtinguisher", parts, toybox_scale=1.20)

# 2. Blanket (Battaniye - 1.15x, soft draped layers + orange thermal stripes)
def build_blanket():
    clear_scene()
    parts = [
        add_box("Fold1", (0.44, 0.32, 0.042), (0, 0, 0.021), "FabricBlue", bevel=0.018, segs=5, subsurf=1),
        add_box("Fold2", (0.43, 0.31, 0.040), (0.005, 0.004, 0.056), "OffWhite", bevel=0.018, segs=5, subsurf=1),
        add_box("Fold3", (0.42, 0.30, 0.042), (-0.004, -0.003, 0.092), "FabricBlue", bevel=0.018, segs=5, subsurf=1),
        add_cyl("RolledEdge", 0.058, 0.425, (0, 0.125, 0.060), "FabricBlue", 32, rot=(0, math.radians(90), 0), bevel=0.015),
        add_box("EmergencyStripe1", (0.425, 0.050, 0.044), (0, -0.068, 0.093), "SafetyOrange", bevel=0.014, segs=4, subsurf=1),
        add_box("EmergencyStripe2", (0.425, 0.050, 0.044), (0, 0.048, 0.093), "Yellow", bevel=0.014, segs=4, subsurf=1),
        add_torus("StrapL", 0.072, 0.012, (-0.125, 0, 0.058), "BrownWood", 32, 14, rot=(0, math.radians(90), 0), scale=(1.0, 2.5, 1.0)),
        add_torus("StrapR", 0.072, 0.012, (0.125, 0, 0.058), "BrownWood", 32, 14, rot=(0, math.radians(90), 0), scale=(1.0, 2.5, 1.0)),
        add_box("BuckleL", (0.032, 0.020, 0.026), (-0.125, -0.172, 0.058), "Brass", bevel=0.005),
        add_box("BuckleR", (0.032, 0.020, 0.026), (0.125, -0.172, 0.058), "Brass", bevel=0.005),
        add_box("Handle", (0.26, 0.026, 0.016), (0, -0.178, 0.058), "BrownWood", bevel=0.006, subsurf=1),
    ]
    finalize_and_export("Blanket", parts, toybox_scale=1.15)

# 3. SnackBar (Enerji Barı - 1.75x puffy foil wrapper)
def build_snackbar():
    clear_scene()
    parts = [
        add_box("PouchMain", (0.12, 0.056, 0.032), (0, 0, 0.016), "Orange", bevel=0.012, segs=5, subsurf=1),
        add_box("EnergyBand", (0.052, 0.059, 0.034), (-0.010, 0, 0.016), "Yellow", bevel=0.010, segs=4, subsurf=1),
        add_box("ChocChunk", (0.036, 0.042, 0.035), (0.030, 0, 0.016), "ChocBrown", bevel=0.008, segs=4),
        add_box("LabelStripe", (0.038, 0.018, 0.036), (-0.010, 0, 0.016), "White", bevel=0.004),
        add_box("FoilCrimpL", (0.020, 0.062, 0.006), (-0.066, 0, 0.016), "SilverMetal", bevel=0.002),
        add_box("FoilCrimpR", (0.020, 0.062, 0.006), (0.066, 0, 0.016), "SilverMetal", bevel=0.002),
    ]
    finalize_and_export("SnackBar", parts, toybox_scale=1.75)

# 4. Apple (Elma - 1.60x organic 5-lobed shape + chunky leaf)
def build_apple():
    clear_scene()
    bpy.ops.mesh.primitive_uv_sphere_add(segments=40, ring_count=28, radius=0.048, location=(0, 0, 0.046))
    body = bpy.context.active_object
    body.name = "AppleBody"
    for v in body.data.vertices:
        z_norm = v.co.z / 0.048
        r_xy = math.hypot(v.co.x, v.co.y)
        angle = math.atan2(v.co.y, v.co.x)
        lobe = 1.0 + 0.035 * math.cos(5.0 * angle)
        if z_norm > 0.52 and r_xy < 0.026:
            v.co.z -= (0.026 - r_xy) * 0.60
        if z_norm < -0.52 and r_xy < 0.024:
            v.co.z += (0.024 - r_xy) * 0.50
        if z_norm > 0:
            s = (1.0 + 0.10 * math.sin(z_norm * math.pi * 0.5)) * lobe
        else:
            s = (1.0 - 0.14 * abs(z_norm)) * lobe
        v.co.x *= s
        v.co.y *= s
    body.data.update()
    assign_mat_and_uv(body, "Red")

    stem = add_cone("Stem", 0.004, 0.0065, 0.028, (0.002, 0, 0.088), "BrownWood", 18, rot=(math.radians(16), math.radians(10), 0), bevel=0.0015)
    leaf = add_sphere("Leaf", 0.022, (0.020, -0.004, 0.092), "Green", 24, 16, scale=(1.35, 0.65, 0.24), rot=(math.radians(15), math.radians(-24), math.radians(20)))
    finalize_and_export("Apple", [body, stem, leaf], toybox_scale=1.60)

# 5. CannedFood (Konserve - 1.38x raised pull-tab & top emblem)
def build_canned_food():
    clear_scene()
    parts = [
        add_cyl("CanWall", 0.046, 0.110, (0, 0, 0.056), "SilverMetal", 40, bevel=0.004),
        add_torus("BeadRib1", 0.0465, 0.0022, (0, 0, 0.094), "SilverMetal", 40, 12),
        add_torus("BeadRib2", 0.0465, 0.0022, (0, 0, 0.018), "SilverMetal", 40, 12),
        add_cyl("LabelWrap", 0.0475, 0.070, (0, 0, 0.056), "Orange", 40, bevel=0.002),
        add_cyl("LabelStripe", 0.0480, 0.024, (0, 0, 0.056), "Yellow", 40, bevel=0.001),
        add_torus("RimTop", 0.0468, 0.0042, (0, 0, 0.111), "SilverMetal", 40, 14),
        add_torus("RimBottom", 0.0468, 0.0042, (0, 0, 0.004), "SilverMetal", 40, 14),
        add_cyl("TopLid", 0.042, 0.004, (0, 0, 0.109), "DarkMetal", 36, bevel=0.001),
        add_cyl("TopFoodIcon", 0.018, 0.003, (-0.010, 0, 0.111), "Orange", 24, bevel=0.001),
        add_torus("PullRing", 0.014, 0.0032, (0.012, 0, 0.116), "SilverMetal", 28, 12, rot=(0, math.radians(-18), 0)),
    ]
    finalize_and_export("CannedFood", parts, toybox_scale=1.38)

# 6. WaterBottle (Su Şişesi - 1.42x high-contrast cyan & navy cap)
def build_water_bottle():
    clear_scene()
    parts = [
        add_cyl("BaseSection", 0.040, 0.055, (0, 0, 0.0275), "WaterBlue", 36, bevel=0.009),
        add_torus("WaistRing1", 0.0385, 0.0035, (0, 0, 0.024), "Blue", 36, 12),
        add_torus("WaistRing2", 0.0385, 0.0035, (0, 0, 0.044), "Blue", 36, 12),
        add_cyl("MidBody", 0.038, 0.070, (0, 0, 0.088), "WaterBlue", 36, bevel=0.005),
        add_cyl("LabelBlue", 0.0398, 0.054, (0, 0, 0.088), "Blue", 36, bevel=0.002),
        add_cyl("LabelWhiteWave", 0.0404, 0.020, (0, 0, 0.088), "White", 36, bevel=0.001),
        add_cyl("UpperBody", 0.040, 0.045, (0, 0, 0.140), "WaterBlue", 36, bevel=0.008),
        add_sphere("Shoulder", 0.040, (0, 0, 0.162), "WaterBlue", 36, 20, (1.0, 1.0, 1.15)),
        add_cyl("Neck", 0.016, 0.032, (0, 0, 0.210), "WaterBlue", 32, bevel=0.002),
        add_torus("SafetySeal", 0.0185, 0.0032, (0, 0, 0.218), "NavyBlue", 32, 12),
        add_cyl("BottleCap", 0.019, 0.026, (0, 0, 0.236), "NavyBlue", 32, bevel=0.004),
    ]
    finalize_and_export("WaterBottle", parts, toybox_scale=1.42)

# 7. EnergyDrink (Enerji İçeceği - 1.48x neon green + lightning badge)
def build_energy_drink():
    clear_scene()
    parts = [
        add_cyl("CanCylinder", 0.033, 0.145, (0, 0, 0.076), "RubberBlack", 40, bevel=0.005),
        add_cyl("NeonCrown", 0.0338, 0.032, (0, 0, 0.120), "NeonGreen", 40, bevel=0.002),
        add_cyl("NeonBaseBand", 0.0338, 0.020, (0, 0, 0.032), "NeonGreen", 40, bevel=0.002),
        add_box("LightningBolt", (0.022, 0.008, 0.068), (0, -0.032, 0.082), "Yellow", rot=(0, math.radians(18), 0), bevel=0.0025),
        add_cone("TopTaper", 0.033, 0.028, 0.015, (0, 0, 0.155), "SilverMetal", 40, bevel=0.002),
        add_torus("ChimeRim", 0.028, 0.003, (0, 0, 0.163), "SilverMetal", 40, 14),
        add_torus("GreenTab", 0.009, 0.0024, (0.005, 0, 0.166), "NeonGreen", 24, 10),
    ]
    finalize_and_export("EnergyDrink", parts, toybox_scale=1.48)

# 8. PowerBattery (Taşınabilir Güç İstasyonu - 1.22x rugged handle & LED bars)
def build_power_battery():
    clear_scene()
    parts = [
        add_box("StationBody", (0.23, 0.15, 0.14), (0, 0, 0.070), "RubberBlack", bevel=0.016, segs=5),
        add_box("OrangeArmorTop", (0.238, 0.156, 0.036), (0, 0, 0.146), "SafetyOrange", bevel=0.010, segs=5),
        add_box("FrontFaceplate", (0.185, 0.014, 0.092), (0, -0.073, 0.072), "DarkMetal", bevel=0.005),
        add_box("LCDScreen", (0.082, 0.016, 0.044), (-0.038, -0.075, 0.082), "GlowGreen", bevel=0.004),
        add_box("Socket220V", (0.042, 0.016, 0.042), (0.052, -0.075, 0.080), "Yellow", bevel=0.004),
        add_cyl("TermPos", 0.020, 0.024, (-0.072, 0, 0.172), "Red", 28, bevel=0.004),
        add_cyl("TermNeg", 0.020, 0.024, (0.072, 0, 0.172), "Blue", 28, bevel=0.004),
        add_cyl("PillarL", 0.012, 0.045, (-0.072, 0.038, 0.180), "DarkMetal", 20, bevel=0.003),
        add_cyl("PillarR", 0.012, 0.045, (0.072, 0.038, 0.180), "DarkMetal", 20, bevel=0.003),
        add_cyl("RuggedHandle", 0.015, 0.165, (0, 0.038, 0.202), "Yellow", 28, rot=(0, math.radians(90), 0), bevel=0.005),
    ]
    finalize_and_export("PowerBattery", parts, toybox_scale=1.22)

# 9. SmallBattery (İkili Kalem Pil Paketi / Twin AA Pack - 1.90x so it never disappears on floor!)
def build_small_battery():
    clear_scene()
    parts = [
        # Backing blister card base so it reads clearly from top-down isometric view
        add_box("PackTray", (0.088, 0.048, 0.014), (0, 0, 0.007), "Yellow", bevel=0.005, segs=4),
        # Twin chunky cells side-by-side
        add_cyl("Cell1Lower", 0.018, 0.064, (-0.021, 0, 0.042), "RubberBlack", 36, bevel=0.003),
        add_cyl("Cell1Copper", 0.0184, 0.030, (-0.021, 0, 0.086), "Copper", 36, bevel=0.003),
        add_cyl("Cell1Term", 0.0085, 0.010, (-0.021, 0, 0.104), "Brass", 28, bevel=0.0025),
        add_cyl("Cell2Lower", 0.018, 0.064, (0.021, 0, 0.042), "RubberBlack", 36, bevel=0.003),
        add_cyl("Cell2Copper", 0.0184, 0.030, (0.021, 0, 0.086), "Copper", 36, bevel=0.003),
        add_cyl("Cell2Term", 0.0085, 0.010, (0.021, 0, 0.104), "Brass", 28, bevel=0.0025),
        add_box("HolderBand", (0.086, 0.044, 0.022), (0, 0, 0.055), "SafetyOrange", bevel=0.004),
    ]
    finalize_and_export("SmallBattery", parts, toybox_scale=1.80)

# 10. WalkieTalkie (El Telsizi - 1.50x thick tactical antenna & oversized dials)
def build_walkie_talkie():
    clear_scene()
    parts = [
        add_box("Body", (0.074, 0.048, 0.150), (0, 0, 0.075), "RubberBlack", bevel=0.014, segs=5),
        add_box("YellowBumper", (0.079, 0.040, 0.096), (0, 0, 0.058), "Yellow", bevel=0.012, segs=5),
        add_box("ScreenBezel", (0.056, 0.010, 0.044), (0, -0.022, 0.110), "DarkMetal", bevel=0.004),
        add_box("LCDDisplay", (0.048, 0.010, 0.035), (0, -0.024, 0.110), "GlowGreen", bevel=0.003),
        add_box("SpeakerGrill", (0.054, 0.010, 0.048), (0, -0.023, 0.048), "DarkMetal", bevel=0.004),
        add_cone("AntennaBoot", 0.015, 0.011, 0.030, (-0.018, 0, 0.162), "RubberBlack", 28, bevel=0.003),
        add_cyl("ThickAntenna", 0.0105, 0.095, (-0.018, 0, 0.215), "RubberBlack", 28, bevel=0.004),
        add_sphere("AntennaTip", 0.012, (-0.018, 0, 0.264), "SafetyOrange", 24, 16),
        add_cyl("TuneKnob", 0.013, 0.024, (0.018, 0, 0.158), "SafetyOrange", 28, bevel=0.004),
        add_box("PTTButton", (0.014, 0.024, 0.042), (-0.039, 0, 0.098), "SafetyOrange", bevel=0.005),
    ]
    finalize_and_export("WalkieTalkie", parts, toybox_scale=1.50)

# 11. Television (Televizyon - 1.00x thicker bezel, emergency broadcast screen)
def build_television():
    clear_scene()
    parts = [
        add_box("StandBase", (0.46, 0.26, 0.028), (0, 0, 0.014), "DarkMetal", bevel=0.010, segs=5),
        add_cyl("StandNeck", 0.038, 0.10, (0, 0.02, 0.068), "DarkMetal", 28, bevel=0.005),
        add_box("FrameBezel", (0.96, 0.068, 0.55), (0, 0, 0.370), "RubberBlack", bevel=0.020, segs=5),
        add_box("RearElectronics", (0.64, 0.055, 0.34), (0, 0.042, 0.330), "DarkMetal", bevel=0.015, segs=4),
        add_box("Screen", (0.89, 0.014, 0.48), (0, -0.030, 0.370), "ScreenBlack", bevel=0.006),
        add_box("AlertBanner", (0.76, 0.016, 0.095), (0, -0.032, 0.395), "Red", bevel=0.005),
        add_box("WarningTicker", (0.60, 0.016, 0.038), (0, -0.032, 0.300), "Yellow", bevel=0.004),
        add_sphere("PowerLED", 0.009, (0.42, -0.034, 0.112), "GlowWarm", 18, 12),
    ]
    finalize_and_export("Television", parts, toybox_scale=1.00)

# 12. Rug (Halı - 1.00x with organic lifted corner & fringe)
def build_rug():
    clear_scene()
    parts = [
        add_box("FringeBase", (1.88, 1.18, 0.012), (0, 0, 0.006), "OffWhite", bevel=0.005, segs=4),
        add_box("BurgundyBody", (1.76, 1.18, 0.018), (0, 0, 0.009), "RugBurgundy", bevel=0.008, segs=5),
        add_box("GoldFrame", (1.52, 0.96, 0.020), (0, 0, 0.010), "RugGold", bevel=0.007, segs=4),
        add_box("NavyCenter", (1.34, 0.78, 0.022), (0, 0, 0.011), "NavyBlue", bevel=0.007, segs=4),
        add_cyl("CenterMedallion", 0.26, 0.024, (0, 0, 0.012), "RugGold", 36, bevel=0.006),
        add_cyl("CenterCore", 0.13, 0.026, (0, 0, 0.013), "RugBurgundy", 24, bevel=0.005),
    ]
    finalize_and_export("Rug", parts, toybox_scale=1.00)

# 13. FoldedClothes (Katlanmış Kıyafet - 1.20x offset soft layers)
def build_folded_clothes():
    clear_scene()
    parts = [
        add_box("DenimBottom", (0.38, 0.30, 0.052), (0, 0, 0.026), "NavyBlue", bevel=0.022, segs=5, subsurf=1),
        add_box("SweaterMid", (0.36, 0.28, 0.054), (0.006, -0.005, 0.072), "Red", bevel=0.022, segs=5, subsurf=1, rot=(0, 0, math.radians(4))),
        add_box("JacketTop", (0.34, 0.26, 0.050), (-0.005, 0.006, 0.118), "Yellow", bevel=0.020, segs=5, subsurf=1, rot=(0, 0, math.radians(-3))),
        add_torus("Collar", 0.055, 0.015, (0, -0.048, 0.144), "White", 28, 14, scale=(1.25, 0.9, 0.65)),
    ]
    finalize_and_export("FoldedClothes", parts, toybox_scale=1.20)

# 14. EmergencyBackpack (Afet Çantası - 1.25x unobstructed top + high-vis strips)
def build_emergency_backpack():
    clear_scene()
    parts = [
        add_box("MainPack", (0.35, 0.20, 0.44), (0, 0.02, 0.22), "SafetyOrange", bevel=0.052, segs=5, subsurf=1),
        add_box("ReinforcedBase", (0.358, 0.208, 0.10), (0, 0.02, 0.050), "RubberBlack", bevel=0.026, segs=5, subsurf=1),
        add_box("FrontPocket", (0.27, 0.085, 0.23), (0, -0.095, 0.160), "Red", bevel=0.032, segs=5, subsurf=1),
        add_box("TopLidDome", (0.33, 0.19, 0.075), (0, 0.02, 0.425), "Red", bevel=0.030, segs=5, subsurf=1),
        add_box("HiVisStripTop", (0.27, 0.020, 0.036), (0, -0.082, 0.345), "Yellow", bevel=0.005),
        add_box("HiVisStripLow", (0.24, 0.020, 0.034), (0, -0.132, 0.205), "White", bevel=0.005),
        add_box("CrossH", (0.065, 0.020, 0.022), (0, -0.134, 0.135), "White", bevel=0.004),
        add_box("CrossV", (0.022, 0.020, 0.065), (0, -0.134, 0.135), "White", bevel=0.004),
        add_cyl("BottlePouchL", 0.050, 0.18, (-0.182, 0.02, 0.120), "DarkMetal", 28, bevel=0.012),
        add_cyl("BottlePouchR", 0.050, 0.18, (0.182, 0.02, 0.120), "DarkMetal", 28, bevel=0.012),
        # Bedroll tucked at back-top so front/top silhouette stays super clear
        add_cyl("Bedroll", 0.055, 0.36, (0, 0.085, 0.455), "OliveGreen", 36, rot=(0, math.radians(90), 0), bevel=0.014),
        add_torus("TopGrabLoop", 0.048, 0.010, (0, -0.02, 0.465), "RubberBlack", 28, 12, rot=(math.radians(90), 0, 0)),
    ]
    finalize_and_export("EmergencyBackpack", parts, toybox_scale=1.25)

# 15. FirstAidKit (İlk Yardım Kiti - 1.30x bold red case + white disc + cross)
def build_first_aid_kit():
    clear_scene()
    parts = [
        add_box("LowerCase", (0.29, 0.21, 0.060), (0, 0, 0.030), "Red", bevel=0.020, segs=5, subsurf=1),
        add_box("CenterSeal", (0.296, 0.216, 0.016), (0, 0, 0.060), "White", bevel=0.006, segs=4),
        add_box("UpperCase", (0.29, 0.21, 0.058), (0, 0, 0.090), "Red", bevel=0.020, segs=5, subsurf=1),
        add_cyl("WhiteBadgeDisc", 0.072, 0.010, (0, 0, 0.118), "White", 36, bevel=0.004),
        add_box("RedCrossH", (0.092, 0.030, 0.012), (0, 0, 0.121), "Red", bevel=0.004),
        add_box("RedCrossV", (0.030, 0.092, 0.012), (0, 0, 0.121), "Red", bevel=0.004),
        add_torus("Handle", 0.054, 0.012, (0, -0.112, 0.060), "RubberBlack", 28, 14, scale=(1.25, 0.70, 0.85)),
        add_box("ClampL", (0.036, 0.020, 0.032), (-0.086, -0.106, 0.060), "SilverMetal", bevel=0.005),
        add_box("ClampR", (0.036, 0.020, 0.032), (0.086, -0.106, 0.060), "SilverMetal", bevel=0.005),
    ]
    finalize_and_export("FirstAidKit", parts, toybox_scale=1.30)

# 16. Flashlight (El Feneri - 1.50x wide trumpet bezel + glowing lens)
def build_flashlight():
    clear_scene()
    parts = [
        add_cyl("Tube", 0.025, 0.155, (0, 0.025, 0.042), "RubberBlack", 36, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_cyl("TailCap", 0.028, 0.028, (0, 0.100, 0.042), "SafetyOrange", 36, rot=(math.radians(90), 0, 0), bevel=0.006),
        add_cyl("YellowGrip", 0.027, 0.076, (0, 0.030, 0.042), "Yellow", 36, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_cone("TrumpetHead", 0.044, 0.025, 0.065, (0, -0.072, 0.042), "DarkMetal", 36, rot=(math.radians(-90), 0, 0), bevel=0.004),
        add_cyl("BezelRing", 0.046, 0.022, (0, -0.108, 0.042), "SafetyOrange", 36, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_sphere("LensGlow", 0.038, (0, -0.115, 0.042), "GlowWarm", 32, 18, (1.0, 0.35, 1.0)),
        add_box("Button", (0.020, 0.032, 0.014), (0, -0.015, 0.068), "Red", bevel=0.005),
    ]
    finalize_and_export("Flashlight", parts, toybox_scale=1.50)

# 17. EmergencyRadio (Acil Durum Radyosu - 1.35x deployed dynamo crank & solar panel)
def build_emergency_radio():
    clear_scene()
    parts = [
        add_box("RadioBody", (0.25, 0.102, 0.150), (0, 0, 0.075), "Red", bevel=0.020, segs=5, subsurf=1),
        add_box("BumperL", (0.032, 0.112, 0.156), (-0.118, 0, 0.078), "RubberBlack", bevel=0.014, segs=5, subsurf=1),
        add_box("BumperR", (0.032, 0.112, 0.156), (0.118, 0, 0.078), "RubberBlack", bevel=0.014, segs=5, subsurf=1),
        add_cyl("SpeakerDrum", 0.055, 0.016, (-0.052, -0.048, 0.075), "DarkMetal", 36, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_torus("SpeakerChrome", 0.053, 0.0045, (-0.052, -0.054, 0.075), "SilverMetal", 36, 14, rot=(math.radians(90), 0, 0)),
        add_box("DialWindow", (0.090, 0.014, 0.040), (0.052, -0.048, 0.106), "GlowWarm", bevel=0.004),
        add_cyl("TuneDial", 0.022, 0.024, (0.032, -0.055, 0.048), "Yellow", 32, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_cyl("VolDial", 0.017, 0.020, (0.080, -0.055, 0.048), "SafetyOrange", 32, rot=(math.radians(90), 0, 0), bevel=0.004),
        add_box("SolarPanel", (0.13, 0.062, 0.010), (0, 0, 0.152), "Blue", bevel=0.004),
        add_torus("CarryArch", 0.076, 0.013, (0, 0, 0.152), "RubberBlack", 32, 14, rot=(math.radians(90), 0, 0), scale=(1.0, 0.45, 1.0)),
        # Hand crank deployed out on right side for unmistakable silhouette
        add_box("CrankArm", (0.055, 0.016, 0.016), (0.152, 0, 0.078), "Yellow", rot=(0, math.radians(-25), 0), bevel=0.004),
        add_cyl("CrankKnob", 0.012, 0.032, (0.180, -0.018, 0.090), "RubberBlack", 20, rot=(math.radians(90), 0, 0), bevel=0.003),
        add_cyl("Antenna", 0.0065, 0.21, (-0.088, 0.022, 0.225), "SilverMetal", 24, rot=(0, math.radians(20), 0), bevel=0.002),
        add_sphere("AntennaTip", 0.010, (-0.052, 0.022, 0.322), "SafetyOrange", 20, 14),
    ]
    finalize_and_export("EmergencyRadio", parts, toybox_scale=1.35)

# 18. AirConditioner (Klima - 1.00x wedge profile + motorized louver flap & LED)
def build_air_conditioner():
    clear_scene()
    body_parts = [
        add_box("Chassis", (0.86, 0.24, 0.28), (0, 0, 0.14), "White", bevel=0.042, segs=5, subsurf=1),
        add_box("SlopedFront", (0.83, 0.045, 0.18), (0, -0.105, 0.165), "OffWhite", rot=(math.radians(-12), 0, 0), bevel=0.018, segs=5, subsurf=1),
        add_box("ChromeStrip", (0.82, 0.016, 0.014), (0, -0.122, 0.102), "SilverMetal", bevel=0.004),
        add_box("VentSlotRecess", (0.79, 0.036, 0.048), (0, -0.098, 0.042), "RubberBlack", bevel=0.004),
        add_box("TempLEDBezel", (0.108, 0.014, 0.048), (0.26, -0.116, 0.175), "ScreenBlack", rot=(math.radians(-12), 0, 0), bevel=0.004),
    ]
    # Motorized horizontal louver deflector flap pivoted along horizontal X axle at (0.0, -0.114, 0.042)
    flap_parts = [
        add_cyl("FlapAxlePinL", 0.009, 0.030, (-0.38, -0.114, 0.042), "SilverMetal", 16, rot=(0, math.radians(90), 0), bevel=0.002),
        add_cyl("FlapAxlePinR", 0.009, 0.030, (0.38, -0.114, 0.042), "SilverMetal", 16, rot=(0, math.radians(90), 0), bevel=0.002),
        add_box("VentFlapBlade", (0.78, 0.048, 0.014), (0, -0.132, 0.034), "SafetyOrange", rot=(math.radians(24), 0, 0), bevel=0.004),
        add_box("VentFlapTrim", (0.78, 0.018, 0.012), (0, -0.148, 0.028), "SilverMetal", rot=(math.radians(24), 0, 0), bevel=0.003),
    ]
    led_parts = [
        add_box("TempLED", (0.094, 0.016, 0.036), (0.26, -0.122, 0.175), "GlowGreen", rot=(math.radians(-12), 0, 0), bevel=0.003),
    ]
    finalize_and_export_multipart(
        "AirConditioner",
        "AirConditionerBody",
        body_parts,
        [
            ("VentFlap_Pivot", flap_parts, (0.0, -0.114, 0.042)),
            ("StatusLED_Pivot", led_parts, (0.26, -0.122, 0.175)),
        ],
        toybox_scale=1.00
    )

# ==============================================================================
# PART B: 14 NEW IMO DISASTER EXPANSION ASSETS (Infrastructure, Fixtures, Kits)
# ==============================================================================

# 19. GasValve (Doğalgaz Ana Kesme Vanası — Coaxial Quarter-Turn Ball Valve + Manometer Needle)
def build_gas_valve():
    clear_scene()
    body_parts = [
        add_box("WallPlate", (0.26, 0.035, 0.42), (0, 0.06, 0.21), "DarkMetal", bevel=0.008),
        add_cyl("PipeLower", 0.032, 0.20, (0, 0, 0.10), "Yellow", 32, bevel=0.004),
        add_cyl("PipeUpper", 0.032, 0.20, (0, 0, 0.32), "Yellow", 32, bevel=0.004),
        add_box("PipeClampLow", (0.088, 0.046, 0.024), (0, 0.028, 0.065), "DarkMetal", bevel=0.004),
        add_box("PipeClampHigh", (0.088, 0.046, 0.024), (0, 0.028, 0.365), "DarkMetal", bevel=0.004),
        add_sphere("ValveValveBody", 0.056, (0, 0, 0.21), "Brass", 32, 20),
        add_cyl("FlangeTop", 0.052, 0.022, (0, 0, 0.265), "Brass", 28, bevel=0.004),
        add_cyl("FlangeBot", 0.052, 0.022, (0, 0, 0.155), "Brass", 28, bevel=0.004),
        add_cyl("ValveStemBoss", 0.024, 0.042, (0, -0.050, 0.21), "Brass", 24, rot=(math.radians(90), 0, 0), bevel=0.003),
        # Mechanical 90-degree stop lugs on brass body
        add_box("StopLugOpen", (0.014, 0.018, 0.014), (-0.022, -0.064, 0.232), "Brass", bevel=0.002),
        add_box("StopLugClosed", (0.014, 0.018, 0.014), (0.026, -0.064, 0.188), "Brass", bevel=0.002),
        # Manometer mounted on upper-left (-X_B) so clockwise +90 deg lever sweep to +X_B is 100% clear!
        add_cyl("GaugeTapPipe", 0.011, 0.072, (-0.040, 0, 0.315), "Brass", 20, rot=(0, math.radians(90), 0), bevel=0.002),
        add_cyl("GasGaugeCase", 0.040, 0.024, (-0.072, -0.010, 0.315), "SilverMetal", 28, rot=(math.radians(90), 0, 0), bevel=0.004),
        add_cyl("GasGaugeDial", 0.034, 0.006, (-0.072, -0.022, 0.315), "White", 28, rot=(math.radians(90), 0, 0), bevel=0.001),
        add_box("GaugeGreenZone", (0.015, 0.004, 0.009), (-0.084, -0.024, 0.328), "Green", rot=(0, math.radians(32), 0), bevel=0.001),
    ]
    # Coaxial Quarter-Turn Lever Handle pivoted strictly at ValveStem axis (0.0, -0.076, 0.210)
    handle_parts = [
        add_cyl("StemShaft", 0.016, 0.028, (0, -0.068, 0.210), "Brass", 24, rot=(math.radians(90), 0, 0), bevel=0.002),
        add_cyl("LeverHubCollar", 0.026, 0.020, (0, -0.076, 0.210), "Red", 24, rot=(math.radians(90), 0, 0), bevel=0.004),
        add_box("LeverArm", (0.034, 0.020, 0.172), (0, -0.076, 0.286), "Red", bevel=0.006),
        add_box("LeverVinylGrip", (0.038, 0.024, 0.105), (0, -0.076, 0.322), "SafetyOrange", bevel=0.006),
        add_cyl("StemLockNut", 0.013, 0.010, (0, -0.088, 0.210), "Brass", 6, rot=(math.radians(90), 0, 0), bevel=0.002),
    ]
    # Manometer Pressure Needle pivoted strictly at dial center (-0.072, -0.026, 0.315)
    needle_parts = [
        add_cyl("NeedleCap", 0.0065, 0.006, (-0.072, -0.026, 0.315), "RubberBlack", 16, rot=(math.radians(90), 0, 0), bevel=0.001),
        add_box("NeedlePointer", (0.0055, 0.004, 0.024), (-0.072, -0.026, 0.325), "Red", bevel=0.001),
    ]
    finalize_and_export_multipart(
        "GasValve",
        "GasValveBody",
        body_parts,
        [
            ("ValveHandle_Pivot", handle_parts, (0.0, -0.076, 0.210)),
            ("GaugeNeedle_Pivot", needle_parts, (-0.072, -0.026, 0.315)),
        ],
        toybox_scale=1.25
    )

# 20. WaterValve (Su Sayacı, Yana Bakan Rising-Stem Ana Kesme Vanası & Tahliye Musluğu)
def build_water_valve():
    clear_scene()
    body_parts = [
        add_box("MountBoard", (0.44, 0.035, 0.30), (0, 0.06, 0.155), "DarkMetal", bevel=0.008),
        add_cyl("PipeHoriz", 0.028, 0.42, (0, 0, 0.155), "SilverMetal", 32, rot=(0, math.radians(90), 0), bevel=0.004),
        add_box("PipeBracketL", (0.024, 0.048, 0.076), (-0.185, 0.028, 0.155), "DarkMetal", bevel=0.004),
        add_box("PipeBracketR", (0.024, 0.048, 0.076), (0.185, 0.028, 0.155), "DarkMetal", bevel=0.004),
        # Left (-0.118): Sideways/Outward-Facing Water Meter (Yana bakan su sayacı)
        add_cyl("MeterBody", 0.050, 0.074, (-0.118, -0.010, 0.155), "Brass", 32, rot=(math.radians(90), 0, 0), bevel=0.006),
        add_cyl("MeterDialFace", 0.042, 0.006, (-0.118, -0.048, 0.155), "White", 32, rot=(math.radians(90), 0, 0), bevel=0.001),
        add_torus("MeterGlassRim", 0.043, 0.006, (-0.118, -0.051, 0.155), "WaterBlue", 32, 14, rot=(math.radians(90), 0, 0)),
        # Center (+0.020): Main Globe Valve Body & Sideways/Outward-Facing Hex Bonnet (Yana bakan vana gövdesi ve boynu)
        add_sphere("GlobeValveChamber", 0.048, (0.020, 0, 0.155), "Brass", 28, 18),
        add_cyl("ValveBonnetHex", 0.025, 0.052, (0.020, -0.042, 0.155), "Brass", 6, rot=(math.radians(90), 0, 0), bevel=0.003),
        # Right (+0.145): Sideways-Facing Brass Drain Faucet / Tap (Tahliye Musluğu)
        add_cyl("FaucetTeeBody", 0.033, 0.048, (0.145, 0, 0.155), "Brass", 24, rot=(0, math.radians(90), 0), bevel=0.003),
        add_cyl("FaucetBonnet", 0.016, 0.038, (0.145, -0.028, 0.155), "Brass", 20, rot=(math.radians(90), 0, 0), bevel=0.002),
        add_cyl("FaucetSpoutNozzle", 0.013, 0.044, (0.145, -0.024, 0.122), "Brass", 20, bevel=0.002),
    ]
    # 1. Sideways-Facing Rising-Stem Handwheel (Yana bakan ana su vanası volanı) pivoted coaxially at (0.020, -0.102, 0.155)
    wheel_parts = [
        add_cyl("RisingSpindle", 0.013, 0.064, (0.020, -0.075, 0.155), "SilverMetal", 24, rot=(math.radians(90), 0, 0), bevel=0.002),
        add_torus("WheelRim", 0.060, 0.013, (0.020, -0.102, 0.155), "Blue", 32, 14, rot=(math.radians(90), 0, 0)),
        add_box("WheelSpoke1", (0.114, 0.012, 0.015), (0.020, -0.102, 0.155), "Blue", bevel=0.003),
        add_box("WheelSpoke2", (0.015, 0.012, 0.114), (0.020, -0.102, 0.155), "Blue", bevel=0.003),
        add_cyl("WheelHubNut", 0.019, 0.016, (0.020, -0.109, 0.155), "Brass", 6, rot=(math.radians(90), 0, 0), bevel=0.003),
        add_cyl("WheelIndexKnob", 0.011, 0.024, (0.020 + 0.054, -0.116, 0.155), "Yellow", 18, rot=(math.radians(90), 0, 0), bevel=0.003),
    ]
    # 2. Sideways-Facing Drain Faucet Butterfly Tap Handle (Musluk Kelebeği) pivoted coaxially at (0.145, -0.056, 0.155)
    tap_parts = [
        add_cyl("TapSpindle", 0.0085, 0.026, (0.145, -0.046, 0.155), "SilverMetal", 16, rot=(math.radians(90), 0, 0), bevel=0.002),
        add_box("TapButterflyBar", (0.016, 0.014, 0.064), (0.145, -0.058, 0.155), "WaterBlue", bevel=0.004),
        add_cyl("TapIndexCap", 0.011, 0.010, (0.145, -0.065, 0.155), "Red", 16, rot=(math.radians(90), 0, 0), bevel=0.002),
    ]
    # 3. Sideways-Facing Water Meter Impeller Star (Su Sayacı Çarkı) pivoted coaxially at (-0.118, -0.052, 0.155)
    turbine_parts = [
        add_box("TurbineBlade1", (0.054, 0.006, 0.010), (-0.118, -0.052, 0.155), "Red", bevel=0.002),
        add_box("TurbineBlade2", (0.010, 0.006, 0.054), (-0.118, -0.052, 0.155), "Red", bevel=0.002),
        add_cyl("TurbineCenterPin", 0.009, 0.008, (-0.118, -0.054, 0.155), "Brass", 16, rot=(math.radians(90), 0, 0), bevel=0.002),
    ]
    # 4. Faucet Spout Water Droplet (Musluk Su Damlası) at nozzle tip (0.145, -0.024, 0.088)
    drop_parts = [
        add_sphere("WaterDroplet", 0.012, (0.145, -0.024, 0.086), "WaterBlue", 18, 14, scale=(0.85, 0.85, 1.35)),
    ]
    finalize_and_export_multipart(
        "WaterValve",
        "WaterValveBody",
        body_parts,
        [
            ("ValveHandle_Pivot", wheel_parts, (0.020, -0.102, 0.155)),
            ("FaucetTap_Pivot", tap_parts, (0.145, -0.056, 0.155)),
            ("MeterTurbine_Pivot", turbine_parts, (-0.118, -0.052, 0.155)),
            ("WaterDrop_Pivot", drop_parts, (0.145, -0.024, 0.088)),
        ],
        toybox_scale=1.25
    )

# 21. BreakerBox (Elektrik Ana Sigorta Panosu — Tek Büyük Ana Şalter Kollu & Yüksek Görünürlüklü)
def build_breaker_box():
    clear_scene()
    body_parts = [
        add_box("Cabinet", (0.36, 0.11, 0.48), (0, 0, 0.24), "DarkMetal", bevel=0.014, segs=5),
        add_box("FrontFrame", (0.33, 0.022, 0.44), (0, -0.052, 0.24), "SilverMetal", bevel=0.008),
        # Top-Left Hazard Warning Badge
        add_box("HazardSign", (0.11, 0.012, 0.095), (-0.055, -0.062, 0.375), "Yellow", bevel=0.004),
        add_box("LightningIcon", (0.022, 0.014, 0.060), (-0.055, -0.065, 0.375), "RubberBlack", rot=(0, math.radians(18), 0), bevel=0.002),
        # Top-Right Oversized Power Status Indicator Dome (AÇIK=Parlak Yeşil, KAPALI=Koyu Kırmızı)
        add_cyl("StatusLEDBezel", 0.032, 0.016, (0.085, -0.058, 0.375), "DarkMetal", 24, rot=(math.radians(90), 0, 0), bevel=0.003),
        add_sphere("StatusLEDOffCore", 0.022, (0.085, -0.062, 0.375), "DarkRed", 20, 14),
        # Center Oversized Single Main Breaker Bay with High-Contrast ON (Top Green) / OFF (Bottom Red) Zones
        add_box("MainSwitchSlot", (0.24, 0.020, 0.23), (0, -0.056, 0.185), "RubberBlack", bevel=0.006),
        add_box("MCBHousingMain", (0.18, 0.022, 0.20), (0, -0.060, 0.185), "OffWhite", bevel=0.005),
        add_box("TopOnZoneBar", (0.15, 0.016, 0.034), (0, -0.066, 0.262), "Green", bevel=0.003),
        add_box("BotOffZoneBar", (0.15, 0.016, 0.034), (0, -0.066, 0.108), "DarkRed", bevel=0.003),
    ]
    # 1. Single Giant Industrial Main Breaker Switch Lever (Tek Büyük Ana Şalter Kolu)
    # Pivoted on horizontal X-axis axle at (0.0, -0.072, 0.185) — swings from -48° (HIGH UP = ON) to +48° (LOW DOWN = OFF)
    main_toggle_parts = [
        add_cyl("MainToggleDrum", 0.032, 0.132, (0.0, -0.072, 0.185), "Red", 28, rot=(0, math.radians(90), 0), bevel=0.005),
        add_box("MainToggleArmL", (0.026, 0.076, 0.024), (-0.048, -0.106, 0.185), "Red", bevel=0.004),
        add_box("MainToggleArmR", (0.026, 0.076, 0.024), (0.048, -0.106, 0.185), "Red", bevel=0.004),
        add_box("MainToggleCenterWeb", (0.082, 0.056, 0.018), (0.0, -0.098, 0.185), "Red", bevel=0.004),
        add_box("MainToggleCrossbar", (0.160, 0.034, 0.038), (0.0, -0.142, 0.185), "SafetyOrange", bevel=0.006),
        add_box("MainToggleGripStripe", (0.130, 0.036, 0.014), (0.0, -0.144, 0.185), "Yellow", bevel=0.003),
    ]
    # 2. Oversized Green Live Power Indicator Dome (Completely covers DarkRed core when ON)
    led_parts = [
        add_sphere("StatusLED", 0.026, (0.085, -0.066, 0.375), "GlowGreen", 24, 16),
    ]
    finalize_and_export_multipart(
        "BreakerBox",
        "BreakerBoxBody",
        body_parts,
        [
            ("MainBreaker_Pivot", main_toggle_parts, (0.0, -0.072, 0.185)),
            ("StatusLED_Pivot", led_parts, (0.085, -0.066, 0.375)),
        ],
        toybox_scale=1.15
    )

# 22. Bookcase (Büyük Ahşap Kitaplık — Dikey 2 Bölmeli, İçi Boş)
def build_bookcase():
    clear_scene()
    parts = [
        add_box("BackPanel", (0.96, 0.028, 1.80), (0, 0.16, 0.90), "BrownWood", bevel=0.006),
        add_box("LeftOuterWall", (0.042, 0.35, 1.80), (-0.46, 0, 0.90), "BrownWood", bevel=0.010),
        add_box("RightOuterWall", (0.042, 0.35, 1.80), (0.46, 0, 0.90), "BrownWood", bevel=0.010),
        # Central vertical divider creating 2 vertical compartments (Dikey 2 bölmeli)
        add_box("CenterVerticalDivider", (0.038, 0.34, 1.72), (0.0, 0.005, 0.90), "BrownWood", bevel=0.008),
        # Horizontal shelves spanning across
        add_box("BottomPlinth", (0.92, 0.35, 0.055), (0, 0, 0.0275), "BrownWood", bevel=0.010),
        add_box("Shelf1", (0.90, 0.34, 0.036), (0, 0.005, 0.46), "BrownWood", bevel=0.008),
        add_box("Shelf2", (0.90, 0.34, 0.036), (0, 0.005, 0.90), "BrownWood", bevel=0.008),
        add_box("Shelf3", (0.90, 0.34, 0.036), (0, 0.005, 1.34), "BrownWood", bevel=0.008),
        add_box("TopCrown", (0.98, 0.37, 0.048), (0, 0, 1.78), "BrownWood", bevel=0.012),
    ]
    finalize_and_export("Bookcase", parts, toybox_scale=1.00)

# 33. FireCabinet (Yangın Tüpü Dolabı — Açılabilir Kapaklı & Duvara Sabitlenebilir)
def build_fire_cabinet():
    clear_scene()
    # 1. Cabinet Body (hollow 0.44m x 0.32m x 0.82m box so FireExtinguisher fits comfortably inside!)
    body_parts = [
        add_box("BackWall", (0.44, 0.028, 0.82), (0, 0.146, 0.41), "Red", bevel=0.008),
        add_box("BottomWall", (0.44, 0.32, 0.032), (0, 0, 0.016), "Red", bevel=0.008),
        add_box("TopWall", (0.44, 0.32, 0.032), (0, 0, 0.804), "Red", bevel=0.008),
        add_box("LeftWall", (0.030, 0.32, 0.76), (-0.205, 0, 0.41), "Red", bevel=0.008),
        add_box("RightWall", (0.030, 0.32, 0.76), (0.205, 0, 0.41), "Red", bevel=0.008),
        add_box("InnerShelfCradle", (0.34, 0.24, 0.016), (0, 0.01, 0.036), "DarkMetal", bevel=0.004),
        add_box("BackMountClamp", (0.16, 0.024, 0.065), (0, 0.122, 0.44), "DarkMetal", bevel=0.004),
    ]
    bpy.ops.object.select_all(action='DESELECT')
    for p in body_parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = body_parts[0]
    bpy.ops.object.join()
    cabinet_body = bpy.context.active_object
    cabinet_body.name = "CabinetBody"
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for poly in cabinet_body.data.polygons:
        poly.use_smooth = True

    # 2. Hinged Door (Separate mesh with local origin at Left Front Hinge (-0.205, -0.162, 0.41))
    hinge_pos = Vector((-0.205, -0.162, 0.41))
    door_parts = [
        # Top, Bottom, Left, Right frame bars of the door so the center window is genuinely open/transparent into the cabinet!
        add_box("DoorFrameLeft", (0.050, 0.024, 0.76), (-0.180, -0.162, 0.41), "Red", bevel=0.006),
        add_box("DoorFrameRight", (0.050, 0.024, 0.76), (0.180, -0.162, 0.41), "Red", bevel=0.006),
        add_box("DoorFrameTop", (0.32, 0.024, 0.060), (0, -0.162, 0.760), "Red", bevel=0.006),
        add_box("DoorFrameBot", (0.32, 0.024, 0.060), (0, -0.162, 0.060), "Red", bevel=0.006),
        # Glass trim bezel strips
        add_box("GlassTrimLeft", (0.014, 0.028, 0.64), (-0.150, -0.163, 0.41), "White", bevel=0.003),
        add_box("GlassTrimRight", (0.014, 0.028, 0.64), (0.150, -0.163, 0.41), "White", bevel=0.003),
        add_box("GlassTrimTop", (0.314, 0.028, 0.014), (0, -0.163, 0.725), "White", bevel=0.003),
        add_box("GlassTrimBot", (0.314, 0.028, 0.014), (0, -0.163, 0.095), "White", bevel=0.003),
        # Door Handle & Latch on right edge
        add_box("LatchBase", (0.028, 0.024, 0.095), (0.182, -0.176, 0.41), "SilverMetal", bevel=0.004),
        add_box("PullHandle", (0.018, 0.036, 0.120), (0.182, -0.194, 0.41), "Yellow", bevel=0.005),
        # Hinge barrels on left edge
        add_cyl("HingeTop", 0.012, 0.060, (-0.205, -0.162, 0.64), "SilverMetal", 20, bevel=0.002),
        add_cyl("HingeBot", 0.012, 0.060, (-0.205, -0.162, 0.18), "SilverMetal", 20, bevel=0.002),
    ]
    bpy.ops.object.select_all(action='DESELECT')
    for p in door_parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = door_parts[0]
    bpy.ops.object.join()
    door_obj = bpy.context.active_object
    door_obj.name = "CabinetDoor_Hinge"
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    # Shift door vertices so its local origin is exactly at hinge_pos
    for v in door_obj.data.vertices:
        v.co -= hinge_pos
    door_obj.data.update()
    door_obj.location = hinge_pos
    for poly in door_obj.data.polygons:
        poly.use_smooth = True

    # Export multi-part hierarchy (CabinetBody + CabinetDoor_Hinge)
    bpy.ops.object.select_all(action='SELECT')
    blend_path = os.path.join(OUT_BLEND_DIR, "SM_FireCabinet.blend")
    fbx_path = os.path.join(OUT_MESH_DIR, "SM_FireCabinet.fbx")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_NONE',
        axis_forward='-Z',
        axis_up='Y',
        use_mesh_modifiers=True,
        mesh_smooth_type='EDGE',
        bake_space_transform=True,
        object_types={'MESH'}
    )
    print(f"[OK] Exported SM_FireCabinet.fbx (with separate hinged CabinetDoor_Hinge)")

# 23. Refrigerator (Mutfak Buzdolabı — Hinged Upper Freezer & Lower Fridge Doors)
def build_refrigerator():
    clear_scene()
    body_parts = [
        add_box("MainCabinetBack", (0.76, 0.56, 1.76), (0, 0.07, 0.88), "White", bevel=0.032, segs=5, subsurf=1),
        add_box("LeftSideWall", (0.048, 0.68, 1.76), (-0.356, 0.01, 0.88), "White", bevel=0.012),
        add_box("RightSideWall", (0.048, 0.68, 1.76), (0.356, 0.01, 0.88), "White", bevel=0.012),
        add_box("TopRoofWall", (0.76, 0.68, 0.050), (0, 0.01, 1.735), "White", bevel=0.012),
        add_box("BottomFloorWall", (0.76, 0.68, 0.055), (0, 0.01, 0.0275), "White", bevel=0.012),
        add_box("CenterDividerMullion", (0.75, 0.68, 0.045), (0, 0.005, 1.11), "DarkMetal", bevel=0.006),
        add_box("InteriorShelf1", (0.66, 0.52, 0.020), (0, 0.02, 0.42), "GlassCyan", bevel=0.004),
        add_box("InteriorShelf2", (0.66, 0.52, 0.020), (0, 0.02, 0.78), "GlassCyan", bevel=0.004),
        add_box("FreezerShelf", (0.66, 0.52, 0.020), (0, 0.02, 1.38), "GlassCyan", bevel=0.004),
    ]
    # Hinges on Right edge (x_B = +0.355, y_B = -0.35), Handles on Left edge (x_B = -0.30)
    lower_door_parts = [
        add_box("LowerDoor", (0.74, 0.055, 1.04), (0, -0.35, 0.55), "OffWhite", bevel=0.020, segs=5),
        add_cyl("HandleLower", 0.016, 0.42, (-0.30, -0.40, 0.78), "SilverMetal", 24, bevel=0.004),
        add_cyl("HingePinLow", 0.014, 0.050, (0.355, -0.35, 0.12), "SilverMetal", 18, bevel=0.002),
    ]
    upper_door_parts = [
        add_box("UpperFreezerDoor", (0.74, 0.055, 0.58), (0, -0.35, 1.44), "OffWhite", bevel=0.020, segs=5),
        add_cyl("HandleUpper", 0.016, 0.28, (-0.30, -0.40, 1.42), "SilverMetal", 24, bevel=0.004),
        add_box("DigitalPanel", (0.14, 0.015, 0.08), (0, -0.38, 1.52), "GlowGreen", bevel=0.004),
        add_cyl("HingePinHigh", 0.014, 0.050, (0.355, -0.35, 1.70), "SilverMetal", 18, bevel=0.002),
    ]
    finalize_and_export_multipart(
        "Refrigerator",
        "RefrigeratorBody",
        body_parts,
        [
            ("CabinetDoor_Hinge", lower_door_parts, (0.355, -0.35, 0.55)),
            ("UpperDoor_Hinge", upper_door_parts, (0.355, -0.35, 1.44)),
        ],
        toybox_scale=1.00
    )

# 24. WaterHeater (Banyo Termosifonu / Şofben — Rotary Thermostat Knob & Cold-Water Shutoff Tap)
def build_water_heater():
    clear_scene()
    body_parts = [
        add_cyl("TankCylinder", 0.23, 0.70, (0, 0, 0.48), "White", 40, bevel=0.025),
        add_sphere("TopDome", 0.23, (0, 0, 0.83), "White", 40, 20, (1, 1, 0.42)),
        add_sphere("BotDome", 0.23, (0, 0, 0.13), "White", 40, 20, (1, 1, 0.35)),
        add_box("ThermostatDialBox", (0.16, 0.04, 0.12), (0, -0.22, 0.28), "DarkMetal", bevel=0.008),
        add_sphere("HeatLEDOffBase", 0.012, (0.04, -0.236, 0.28), "DarkRed", 16, 12),
        # Hot (left, Red) and Cold (right, Blue) bottom plumbing pipes positioned at front-bottom so tap is clearly visible
        add_cyl("PipeHot", 0.020, 0.13, (-0.075, -0.145, 0.065), "Red", 22, bevel=0.003),
        add_cyl("PipeCold", 0.020, 0.13, (0.075, -0.145, 0.065), "Blue", 22, bevel=0.003),
        add_sphere("ColdValveBody", 0.028, (0.075, -0.145, 0.055), "Brass", 22, 16),
        add_cyl("ColdValveStem", 0.013, 0.036, (0.075, -0.172, 0.055), "Brass", 18, rot=(math.radians(90), 0, 0), bevel=0.002),
    ]
    # Cold-water inlet quarter-turn butterfly shutoff valve on PipeCold (0.075, -0.192, 0.055)
    inlet_tap_parts = [
        add_cyl("InletTapHub", 0.016, 0.016, (0.075, -0.192, 0.055), "Brass", 18, rot=(math.radians(90), 0, 0), bevel=0.002),
        add_box("InletTapHandle", (0.022, 0.016, 0.078), (0.075, -0.196, 0.076), "Red", bevel=0.004),
    ]
    # Front rotary thermostat dial knob at (-0.03, -0.242, 0.28)
    knob_parts = [
        add_cyl("TempKnob", 0.034, 0.025, (-0.03, -0.242, 0.28), "SafetyOrange", 24, rot=(math.radians(90), 0, 0), bevel=0.004),
        add_box("TempKnobPointer", (0.010, 0.014, 0.030), (-0.03, -0.255, 0.292), "White", bevel=0.002),
    ]
    led_parts = [
        add_sphere("HeatLED", 0.016, (0.04, -0.242, 0.28), "GlowGreen", 18, 12),
    ]
    finalize_and_export_multipart(
        "WaterHeater",
        "WaterHeaterBody",
        body_parts,
        [
            ("ValveHandle_Pivot", inlet_tap_parts, (0.075, -0.192, 0.055)),
            ("ThermostatKnob_Pivot", knob_parts, (-0.03, -0.242, 0.28)),
            ("StatusLED_Pivot", led_parts, (0.04, -0.242, 0.28)),
        ],
        toybox_scale=1.05
    )


# 25. SturdyTable (Sağlam Ahşap/Çelik Masa — Safe Zone / Triangle of Life Prop)
def build_sturdy_table():
    clear_scene()
    parts = [
        add_box("TableTop", (1.40, 0.86, 0.065), (0, 0, 0.74), "BrownWood", bevel=0.016, segs=5),
        add_box("SteelFrameApron", (1.30, 0.76, 0.055), (0, 0, 0.69), "DarkMetal", bevel=0.008),
        add_box("LegFL", (0.075, 0.075, 0.72), (-0.61, -0.35, 0.36), "BrownWood", bevel=0.012),
        add_box("LegFR", (0.075, 0.075, 0.72), (0.61, -0.35, 0.36), "BrownWood", bevel=0.012),
        add_box("LegBL", (0.075, 0.075, 0.72), (-0.61, 0.35, 0.36), "BrownWood", bevel=0.012),
        add_box("LegBR", (0.075, 0.075, 0.72), (0.61, 0.35, 0.36), "BrownWood", bevel=0.012),
    ]
    finalize_and_export("SturdyTable", parts, toybox_scale=1.00)

# 26. EmergencyWhistle (Deprem Düdüğü - 2.00x Toy-Box Scale)
def build_emergency_whistle():
    clear_scene()
    parts = [
        add_cyl("SoundChamber", 0.032, 0.036, (0, 0.01, 0.032), "SafetyOrange", 36, rot=(0, math.radians(90), 0), bevel=0.006),
        add_box("Mouthpiece", (0.030, 0.052, 0.022), (0, -0.028, 0.036), "SafetyOrange", rot=(math.radians(10), 0, 0), bevel=0.005),
        add_box("SoundSlot", (0.024, 0.016, 0.010), (0, -0.006, 0.052), "RubberBlack", bevel=0.002),
        add_torus("LanyardRing", 0.016, 0.004, (0, 0.046, 0.032), "SilverMetal", 28, 12),
        add_torus("NeckCord", 0.048, 0.0055, (0, 0.090, 0.012), "Red", 36, 12, scale=(0.85, 1.35, 0.5)),
    ]
    finalize_and_export("EmergencyWhistle", parts, toybox_scale=1.85)

# 27. WorkGloves (Ağır Hizmet İş Eldiveni - 1.45x)
def build_work_gloves():
    clear_scene()
    parts = [
        add_box("PalmLeft", (0.11, 0.14, 0.036), (-0.045, 0, 0.018), "Yellow", rot=(0, 0, math.radians(10)), bevel=0.012, segs=5, subsurf=1),
        add_box("CuffLeft", (0.115, 0.075, 0.040), (-0.035, 0.095, 0.020), "SafetyOrange", rot=(0, 0, math.radians(10)), bevel=0.010, segs=4, subsurf=1),
        add_box("ThumbLeft", (0.042, 0.075, 0.032), (-0.10, -0.01, 0.018), "Yellow", rot=(0, 0, math.radians(38)), bevel=0.010, subsurf=1),
        add_box("PalmRight", (0.11, 0.14, 0.036), (0.045, -0.01, 0.038), "Yellow", rot=(0, 0, math.radians(-12)), bevel=0.012, segs=5, subsurf=1),
        add_box("CuffRight", (0.115, 0.075, 0.040), (0.035, 0.085, 0.040), "SafetyOrange", rot=(0, 0, math.radians(-12)), bevel=0.010, segs=4, subsurf=1),
    ]
    finalize_and_export("WorkGloves", parts, toybox_scale=1.45)

# 28. MultiTool (Çok Amaçlı Pense / Çakı - 1.65x)
def build_multi_tool():
    clear_scene()
    parts = [
        add_box("HandleLeft", (0.036, 0.14, 0.024), (-0.032, 0.02, 0.012), "Red", rot=(0, 0, math.radians(16)), bevel=0.006, segs=4),
        add_box("HandleRight", (0.036, 0.14, 0.024), (0.032, 0.02, 0.012), "Red", rot=(0, 0, math.radians(-16)), bevel=0.006, segs=4),
        add_box("GripInlayL", (0.020, 0.09, 0.027), (-0.032, 0.02, 0.012), "RubberBlack", rot=(0, 0, math.radians(16)), bevel=0.003),
        add_box("GripInlayR", (0.020, 0.09, 0.027), (0.032, 0.02, 0.012), "RubberBlack", rot=(0, 0, math.radians(-16)), bevel=0.003),
        add_cyl("PivotHinge", 0.024, 0.028, (0, -0.055, 0.014), "SilverMetal", 28, bevel=0.004),
        add_cone("PlierJaws", 0.028, 0.008, 0.075, (0, -0.10, 0.014), "SilverMetal", 24, rot=(math.radians(90), 0, 0), bevel=0.003),
    ]
    finalize_and_export("MultiTool", parts, toybox_scale=1.65)

# 29. ShatteredGlass (Kırık Cam Parçaları — Floor Hazard)
def build_shattered_glass():
    clear_scene()
    parts = [
        add_cone("Shard1", 0.16, 0.01, 0.016, (-0.12, 0.05, 0.008), "GlassCyan", 5, rot=(0, 0, math.radians(15)), bevel=0.002),
        add_cone("Shard2", 0.14, 0.01, 0.015, (0.14, -0.06, 0.008), "WaterBlue", 4, rot=(0, 0, math.radians(52)), bevel=0.002),
        add_cone("Shard3", 0.11, 0.01, 0.014, (0.02, 0.14, 0.007), "GlassCyan", 6, rot=(0, 0, math.radians(-28)), bevel=0.002),
        add_cone("Shard4", 0.13, 0.01, 0.015, (-0.06, -0.13, 0.008), "White", 4, rot=(0, 0, math.radians(80)), bevel=0.002),
        add_cone("Shard5", 0.09, 0.01, 0.018, (0.01, -0.01, 0.009), "GlassCyan", 5, rot=(0, 0, math.radians(33)), bevel=0.002),
    ]
    finalize_and_export("ShatteredGlass", parts, toybox_scale=1.15)

# 30. RubblePile (Parçacıklı Yıkıntı, Kırık Tuğla, Beton Plaka, Ahşap Kiriş ve İnşaat Demiri Moloz Yığını — Corridor Obstacle)
def build_rubble_pile():
    clear_scene()
    parts = [
        # 1. Multi-Faceted Crushed Rubble & Mortar Base Bed (Gives solid silhouette & grounding)
        add_cone("RubbleBedCore", 0.34, 0.22, 0.065, (0.0, 0.0, 0.032), "RubberBlack", 7, rot=(0, 0, math.radians(15)), bevel=0.006),
        add_cone("RubbleBedMoundL", 0.26, 0.15, 0.082, (-0.09, -0.03, 0.041), "ChocBrown", 6, rot=(math.radians(5), math.radians(-6), math.radians(42)), bevel=0.005),
        add_cone("RubbleBedMoundR", 0.25, 0.14, 0.078, (0.10, 0.04, 0.039), "DarkMetal", 6, rot=(math.radians(-4), math.radians(7), math.radians(-28)), bevel=0.005),
        add_cone("MortarDustSubMound", 0.24, 0.13, 0.092, (0.01, 0.02, 0.046), "BrownWood", 7, rot=(math.radians(6), math.radians(4), math.radians(75)), bevel=0.005),

        # 2. Fractured Reinforced Concrete Wall Slabs & Mortar Blocks (Kırık Betonarme Bloklar)
        add_box("ConcreteSlabMain", (0.27, 0.19, 0.056), (-0.04, 0.03, 0.102), "DarkMetal", rot=(math.radians(26), math.radians(-18), math.radians(32)), bevel=0.004, segs=2),
        add_box("PlasterSlabLayer", (0.22, 0.15, 0.026), (-0.03, 0.02, 0.122), "RubberBlack", rot=(math.radians(26), math.radians(-18), math.radians(32)), bevel=0.003, segs=2),
        add_box("ConcreteChunkUpper", (0.21, 0.15, 0.062), (0.09, -0.04, 0.128), "DarkMetal", rot=(math.radians(-28), math.radians(-22), math.radians(-40)), bevel=0.004, segs=2),
        add_box("ConcreteSlabLeft", (0.18, 0.14, 0.050), (-0.18, -0.05, 0.072), "DarkMetal", rot=(math.radians(18), math.radians(28), math.radians(15)), bevel=0.004, segs=2),
        add_box("ConcreteSlabRight", (0.19, 0.13, 0.048), (0.18, 0.08, 0.070), "RubberBlack", rot=(math.radians(-16), math.radians(-24), math.radians(55)), bevel=0.004, segs=2),
        add_box("ConcreteBlockFront", (0.15, 0.12, 0.054), (0.02, -0.14, 0.068), "DarkMetal", rot=(math.radians(32), math.radians(12), math.radians(-18)), bevel=0.004, segs=2),
        add_cone("AngularConcretePeak", 0.11, 0.035, 0.095, (0.02, -0.01, 0.175), "DarkMetal", 5, rot=(math.radians(14), math.radians(-19), math.radians(45)), bevel=0.003),

        # 3. Splintered Structural Timber Beams (Kırık Çatı/Tavan Ahşap Kiriş Parçaları)
        add_box("BrokenTimberBeam1", (0.32, 0.068, 0.058), (0.04, 0.11, 0.092), "BrownWood", rot=(math.radians(-18), math.radians(24), math.radians(-22)), bevel=0.004, segs=2),
        add_box("BrokenTimberBeam2", (0.22, 0.056, 0.048), (-0.12, -0.02, 0.115), "ChocBrown", rot=(math.radians(22), math.radians(15), math.radians(48)), bevel=0.004, segs=2),

        # 4. Perforated Red & Terracotta Construction Bricks & Broken Half-Bricks (Delikli İnşaat Tuğlaları)
        add_box("HollowBrick1", (0.18, 0.092, 0.066), (-0.11, -0.11, 0.086), "Red", rot=(math.radians(22), math.radians(-14), math.radians(28)), bevel=0.004, segs=2),
        add_box("HollowBrick1_Holes", (0.186, 0.062, 0.022), (-0.11, -0.11, 0.086), "RubberBlack", rot=(math.radians(22), math.radians(-14), math.radians(28)), bevel=0.002),
        add_box("HollowBrick2", (0.18, 0.092, 0.066), (0.14, -0.09, 0.088), "DarkRed", rot=(math.radians(-19), math.radians(24), math.radians(-38)), bevel=0.004, segs=2),
        add_box("HollowBrick2_Holes", (0.186, 0.062, 0.022), (0.14, -0.09, 0.088), "RubberBlack", rot=(math.radians(-19), math.radians(24), math.radians(-38)), bevel=0.002),
        add_box("HollowBrick3_Top", (0.17, 0.088, 0.062), (-0.04, 0.05, 0.168), "Red", rot=(math.radians(-30), math.radians(16), math.radians(64)), bevel=0.004, segs=2),
        add_box("HollowBrick3_Holes", (0.176, 0.058, 0.020), (-0.04, 0.05, 0.168), "RubberBlack", rot=(math.radians(-30), math.radians(16), math.radians(64)), bevel=0.002),
        add_box("HalfBrick4", (0.11, 0.088, 0.060), (0.08, 0.16, 0.072), "Red", rot=(math.radians(15), math.radians(28), math.radians(110)), bevel=0.004, segs=2),
        add_box("HalfBrick5", (0.12, 0.086, 0.058), (-0.16, 0.12, 0.074), "DarkRed", rot=(math.radians(-24), math.radians(-15), math.radians(-22)), bevel=0.004, segs=2),
        add_box("HalfBrick6", (0.10, 0.082, 0.055), (0.23, -0.02, 0.054), "Red", rot=(math.radians(12), math.radians(-30), math.radians(18)), bevel=0.004, segs=2),
        add_box("BrickShard7", (0.088, 0.070, 0.048), (-0.24, 0.02, 0.044), "DarkRed", rot=(math.radians(-10), math.radians(20), math.radians(48)), bevel=0.003, segs=2),
        add_box("BrickShard8", (0.092, 0.065, 0.045), (-0.01, -0.20, 0.045), "Red", rot=(math.radians(25), math.radians(8), math.radians(-15)), bevel=0.003, segs=2),

        # 5. Protruding Bent Steel Rebar Rods (Beton Bloklardan Çıkan Eğrilmiş İnşaat Demirleri / Filizler)
        add_cyl("RebarRod1", 0.0080, 0.28, (-0.08, 0.02, 0.17), "SilverMetal", 12, rot=(math.radians(48), math.radians(-32), math.radians(15)), bevel=0.001),
        add_cyl("RebarRod2", 0.0080, 0.25, (0.11, -0.03, 0.16), "Copper", 12, rot=(math.radians(-42), math.radians(38), math.radians(-25)), bevel=0.001),
        add_cyl("RebarRod3", 0.0075, 0.22, (0.02, 0.10, 0.15), "SilverMetal", 12, rot=(math.radians(-55), math.radians(-18), math.radians(40)), bevel=0.001),
        add_cyl("RebarHook4", 0.0075, 0.12, (-0.15, -0.04, 0.22), "SilverMetal", 12, rot=(math.radians(72), math.radians(25), 0), bevel=0.001),

        # 6. Dense Particulate Gravel, Crushed Concrete & Brick Chips on Pile & Perimeter (20 Parçacıklı Moloz Taşı)
        add_cone("ParticulateChunk1", 0.062, 0.022, 0.048, (-0.09, -0.02, 0.145), "BrownWood", 5, rot=(math.radians(18), 0, math.radians(25)), bevel=0.002),
        add_cone("ParticulateChunk2", 0.058, 0.020, 0.045, (0.06, 0.05, 0.152), "DarkMetal", 6, rot=(math.radians(-15), math.radians(12), math.radians(50)), bevel=0.002),
        add_cone("ParticulateChunk3", 0.054, 0.018, 0.042, (0.01, -0.09, 0.135), "Red", 4, rot=(math.radians(22), math.radians(-10), math.radians(15)), bevel=0.002),
        add_cone("ParticulateChunk4", 0.050, 0.018, 0.038, (-0.14, 0.04, 0.118), "ChocBrown", 5, rot=(0, math.radians(20), math.radians(-35)), bevel=0.002),
        add_cone("ParticulateChunk5", 0.052, 0.018, 0.040, (0.15, 0.01, 0.115), "DarkRed", 5, rot=(math.radians(-12), 0, math.radians(65)), bevel=0.002),
        add_cone("ParticulateChunk6", 0.048, 0.016, 0.036, (-0.05, 0.14, 0.110), "DarkMetal", 6, rot=(math.radians(10), math.radians(-15), math.radians(40)), bevel=0.002),
        add_cone("DebrisStone1", 0.054, 0.020, 0.040, (-0.28, -0.12, 0.020), "DarkMetal", 5, rot=(0, 0, math.radians(15)), bevel=0.002),
        add_cone("DebrisStone2", 0.046, 0.016, 0.034, (-0.22, -0.21, 0.017), "Red", 4, rot=(0, 0, math.radians(55)), bevel=0.002),
        add_cone("DebrisStone3", 0.058, 0.022, 0.042, (0.27, -0.14, 0.021), "ChocBrown", 6, rot=(0, 0, math.radians(-30)), bevel=0.002),
        add_cone("DebrisStone4", 0.044, 0.015, 0.032, (0.30, 0.04, 0.016), "DarkMetal", 5, rot=(0, 0, math.radians(42)), bevel=0.002),
        add_cone("DebrisStone5", 0.050, 0.018, 0.036, (0.22, 0.21, 0.018), "DarkRed", 4, rot=(0, 0, math.radians(78)), bevel=0.002),
        add_cone("DebrisStone6", 0.055, 0.020, 0.038, (-0.12, 0.25, 0.019), "BrownWood", 5, rot=(0, 0, math.radians(-60)), bevel=0.002),
        add_cone("DebrisStone7", 0.042, 0.014, 0.030, (-0.29, 0.14, 0.015), "DarkMetal", 4, rot=(0, 0, math.radians(25)), bevel=0.002),
        add_cone("DebrisStone8", 0.045, 0.016, 0.032, (0.13, -0.25, 0.016), "Red", 5, rot=(0, 0, math.radians(-45)), bevel=0.002),
        add_cone("DebrisStone9", 0.038, 0.014, 0.028, (-0.09, -0.27, 0.014), "DarkMetal", 5, rot=(0, 0, math.radians(12)), bevel=0.002),
        add_cone("DebrisStone10", 0.040, 0.015, 0.028, (0.05, 0.27, 0.014), "Red", 4, rot=(0, 0, math.radians(33)), bevel=0.002),
        add_cone("DebrisStone11", 0.034, 0.012, 0.024, (-0.33, -0.02, 0.012), "DarkRed", 5, rot=(0, 0, math.radians(19)), bevel=0.002),
        add_cone("DebrisStone12", 0.036, 0.013, 0.025, (0.33, -0.05, 0.013), "Red", 4, rot=(0, 0, math.radians(-22)), bevel=0.002),
        add_cone("DebrisStone13", 0.035, 0.012, 0.024, (-0.19, -0.26, 0.012), "DarkMetal", 5, rot=(0, 0, math.radians(64)), bevel=0.002),
        add_cone("DebrisStone14", 0.034, 0.012, 0.024, (0.19, -0.22, 0.012), "ChocBrown", 4, rot=(0, 0, math.radians(-14)), bevel=0.002),
    ]
    finalize_and_export("RubblePile", parts, toybox_scale=1.12)

# 32. BunkerHatch (Zemin Sığınak Kapağı / Kapısı — Yuvarlak, Kalın Menteşeli, Ağır Zırhlı & Derin Karanlık Kuyulu)
def build_bunker_hatch():
    clear_scene()
    body_parts = [
        # 1. Heavy Reinforced Concrete & Steel Ground Foundation Apron (Bottom strictly at z = 0.000m)
        add_hollow_frustum("FoundationBase", 0.575, 0.96, 0.575, 0.89, 0.120, (0, 0, 0.060), "DarkMetal", segs=16, bevel=0.012),
        add_hollow_frustum("SlopedBlastCurb", 0.575, 0.88, 0.575, 0.74, 0.230, (0, 0, 0.235), "RubberBlack", segs=36, bevel=0.010),
        add_hollow_frustum("SteelCoamingCollar", 0.575, 0.755, 0.575, 0.735, 0.070, (0, 0, 0.375), "SilverMetal", segs=48, bevel=0.008),
        add_torus("CollarGasketSeat", 0.612, 0.022, (0, 0, 0.408), "RubberBlack", 48, 16),

        # 2. Front Approach Anti-Slip Tread Steps (y_B < 0 -> +Z Front in Unity where Character stands in Cutscene)
        add_box("FrontStepLower", (0.76, 0.24, 0.110), (0, -0.94, 0.055), "DarkMetal", bevel=0.008),
        add_box("FrontStepLowerNosing", (0.74, 0.036, 0.028), (0, -1.045, 0.100), "SafetyOrange", bevel=0.004),
        add_box("FrontStepUpper", (0.72, 0.22, 0.220), (0, -0.78, 0.110), "DarkMetal", bevel=0.008),
        add_box("FrontStepUpperNosing", (0.70, 0.036, 0.028), (0, -0.875, 0.210), "Yellow", bevel=0.004),
        add_box("FrontStepCheekL", (0.042, 0.38, 0.240), (-0.37, -0.85, 0.120), "SilverMetal", rot=(math.radians(18), 0, 0), bevel=0.005),
        add_box("FrontStepCheekR", (0.042, 0.38, 0.240), (0.37, -0.85, 0.120), "SilverMetal", rot=(math.radians(18), 0, 0), bevel=0.005),
        add_box("FrontCurbWarningPlate", (0.42, 0.024, 0.075), (0, -0.79, 0.285), "Yellow", rot=(math.radians(24), 0, 0), bevel=0.004),
        add_box("FrontCurbWarningStripe", (0.36, 0.026, 0.028), (0, -0.795, 0.285), "SafetyOrange", rot=(math.radians(24), 0, 0), bevel=0.003),

        # 3. 4 Inner Collar Latch Strike Pockets (+X, -X, +Y, -Y)
        add_box("CollarLatchStrikeE", (0.045, 0.11, 0.048), (0.585, 0, 0.365), "Brass", bevel=0.004),
        add_box("CollarLatchStrikeW", (0.045, 0.11, 0.048), (-0.585, 0, 0.365), "Brass", bevel=0.004),
        add_box("CollarLatchStrikeN", (0.11, 0.045, 0.048), (0, 0.585, 0.365), "Brass", bevel=0.004),
        add_box("CollarLatchStrikeS", (0.11, 0.045, 0.048), (0, -0.585, 0.365), "Brass", bevel=0.004),

        # 4. Upper Well Shaft Liner & Top Ladder Rungs (z = 0.008m .. 0.36m, visible right inside open mouth)
        add_hollow_frustum("UpperShaftWall", 0.568, 0.600, 0.568, 0.600, 0.352, (0, 0, 0.184), "ScreenBlack", segs=48, bevel=0.002),
        add_torus("UpperShaftRib1", 0.562, 0.018, (0, 0, 0.295), "DarkMetal", 48, 14),
        add_torus("UpperShaftRib2", 0.562, 0.018, (0, 0, 0.150), "RubberBlack", 48, 14),
        # Upper Steel Descent Ladder on Back-Inner Wall (y_B = +0.47m, facing camera when hatch is open!)
        add_cyl("UpperLadderRailL", 0.016, 0.350, (-0.20, 0.485, 0.185), "Yellow", 20, bevel=0.002),
        add_cyl("UpperLadderRailR", 0.016, 0.350, (0.20, 0.485, 0.185), "Yellow", 20, bevel=0.002),
        add_box("UpperLadderMountL", (0.032, 0.075, 0.032), (-0.20, 0.525, 0.310), "SilverMetal", bevel=0.003),
        add_box("UpperLadderMountR", (0.032, 0.075, 0.032), (0.20, 0.525, 0.310), "SilverMetal", bevel=0.003),
        add_cyl("UpperLadderRung1", 0.015, 0.420, (0, 0.485, 0.310), "SafetyOrange", 24, rot=(0, math.radians(90), 0), bevel=0.003),
        add_cyl("UpperLadderRung2", 0.015, 0.420, (0, 0.485, 0.130), "Yellow", 24, rot=(0, math.radians(90), 0), bevel=0.003),
        # Interior Bunker Utility Conduit Pipe along right shaft wall
        add_cyl("UpperConduitPipe", 0.020, 0.350, (0.46, 0.24, 0.185), "Copper", 20, bevel=0.002),
        add_box("UpperConduitClamp", (0.055, 0.045, 0.028), (0.48, 0.25, 0.260), "Brass", bevel=0.003),

        # 5. Massive Industrial Rear Hinge Stanchion Assembly (y_B = +0.72m, z_B = 0.425m)
        add_box("HingePedestalBase", (0.90, 0.28, 0.280), (0, 0.74, 0.140), "DarkMetal", bevel=0.012),
        add_box("HingeButtressL", (0.085, 0.24, 0.260), (-0.38, 0.82, 0.140), "DarkMetal", rot=(math.radians(-22), 0, 0), bevel=0.008),
        add_box("HingeButtressR", (0.085, 0.24, 0.260), (0.38, 0.82, 0.140), "DarkMetal", rot=(math.radians(-22), 0, 0), bevel=0.008),
        # 3 Massive Cast-Steel Hinge Stanchion Towers (Left x=-0.33, Center x=0.0, Right x=+0.33)
        add_box("HingeStanchionL", (0.115, 0.22, 0.240), (-0.33, 0.72, 0.340), "DarkMetal", bevel=0.010),
        add_box("HingeStanchionC", (0.125, 0.22, 0.240), (0.00, 0.72, 0.340), "DarkMetal", bevel=0.010),
        add_box("HingeStanchionR", (0.115, 0.22, 0.240), (0.33, 0.72, 0.340), "DarkMetal", bevel=0.010),
        add_cyl("StanchionBarrelL", 0.070, 0.115, (-0.33, 0.72, 0.425), "DarkMetal", 32, rot=(0, math.radians(90), 0), bevel=0.006),
        add_cyl("StanchionBarrelC", 0.070, 0.125, (0.00, 0.72, 0.425), "DarkMetal", 32, rot=(0, math.radians(90), 0), bevel=0.006),
        add_cyl("StanchionBarrelR", 0.070, 0.115, (0.33, 0.72, 0.425), "DarkMetal", 32, rot=(0, math.radians(90), 0), bevel=0.006),
        # Continuous 9.0 cm Thick Solid Hardened Steel Hinge Axle Pin & Oversized M48 Brass Lock Nuts
        add_cyl("MassiveHingeAxlePin", 0.046, 0.96, (0, 0.72, 0.425), "SilverMetal", 32, rot=(0, math.radians(90), 0), bevel=0.005),
        add_cyl("HingeAxleHexNutL", 0.064, 0.042, (-0.49, 0.72, 0.425), "Brass", 6, rot=(0, math.radians(90), 0), bevel=0.004),
        add_cyl("HingeAxleHexNutR", 0.064, 0.042, (0.49, 0.72, 0.425), "Brass", 6, rot=(0, math.radians(90), 0), bevel=0.004),
        add_sphere("HingeGreaseCapL", 0.036, (-0.52, 0.72, 0.425), "SafetyOrange", 20, 14),
        add_sphere("HingeGreaseCapR", 0.036, (0.52, 0.72, 0.425), "SafetyOrange", 20, 14),
        # Coaxial Heavy Helical Torsion Springs around Hinge Shaft
        add_torus("TorsionSpringL1", 0.058, 0.012, (-0.41, 0.72, 0.425), "Copper", 28, 12, rot=(0, math.radians(90), 0)),
        add_torus("TorsionSpringL2", 0.058, 0.012, (-0.435, 0.72, 0.425), "Copper", 28, 12, rot=(0, math.radians(90), 0)),
        add_torus("TorsionSpringR1", 0.058, 0.012, (0.41, 0.72, 0.425), "Copper", 28, 12, rot=(0, math.radians(90), 0)),
        add_torus("TorsionSpringR2", 0.058, 0.012, (0.435, 0.72, 0.425), "Copper", 28, 12, rot=(0, math.radians(90), 0)),
        # Side Outrigger Brackets & Lower Hydraulic Damper Barrels (x = ±0.56m)
        add_box("DamperBaseBracketL", (0.10, 0.16, 0.12), (-0.56, 0.56, 0.18), "DarkMetal", bevel=0.006),
        add_box("DamperBaseBracketR", (0.10, 0.16, 0.12), (0.56, 0.56, 0.18), "DarkMetal", bevel=0.006),
        add_cyl("DamperLowerCylinderL", 0.034, 0.24, (-0.56, 0.53, 0.29), "SafetyOrange", 24, rot=(math.radians(-18), 0, 0), bevel=0.004),
        add_cyl("DamperLowerCylinderR", 0.034, 0.24, (0.56, 0.53, 0.29), "SafetyOrange", 24, rot=(math.radians(-18), 0, 0), bevel=0.004),
    ]

    # Perimeter Ground Anchor Lugs & Collar Flange Bolts
    for i in range(8):
        ang = math.radians(i * 45.0 + 22.5)
        ca, sa = math.cos(ang), math.sin(ang)
        if abs(sa) < 0.92:  # Skip direct front step & rear hinge center
            body_parts.append(add_box(f"GroundAnchorLug_{i}", (0.11, 0.14, 0.036), (ca * 0.92, sa * 0.92, 0.018), "SilverMetal", rot=(0, 0, ang), bevel=0.004))
            body_parts.append(add_cyl(f"GroundAnchorBolt_{i}", 0.022, 0.032, (ca * 0.93, sa * 0.93, 0.042), "Yellow", 6, bevel=0.003))

    for i in range(16):
        ang = math.radians(i * 22.5)
        ca, sa = math.cos(ang), math.sin(ang)
        if sa < 0.88:
            body_parts.append(add_cyl(f"CollarBolt_{i}", 0.015, 0.018, (ca * 0.695, sa * 0.695, 0.414), "Brass", 6, bevel=0.002))

    # ==========================================================================
    # MOVING PART 1: HatchLid_Hinge (Massive Round Armored Lid, Pivot at Hinge Axle (0.0, 0.72, 0.425))
    # ==========================================================================
    lid_parts = [
        # 1. Top Armored Circular Blast Plate & Machined Outer Steel Rim (Total Lid Thickness ~16 cm!)
        add_cyl("LidMainArmorSlab", 0.665, 0.060, (0, 0, 0.440), "OliveGreen", 48, bevel=0.016),
        add_hollow_frustum("LidOuterSteelArmorRim", 0.575, 0.705, 0.595, 0.685, 0.066, (0, 0, 0.440), "DarkMetal", segs=48, bevel=0.010),
        add_cyl("LidCenterArmorBoss", 0.245, 0.036, (0, 0, 0.478), "DarkMetal", 36, bevel=0.008),
        add_torus("LidCenterBossRing", 0.242, 0.012, (0, 0, 0.492), "SilverMetal", 36, 12),

        # 2. Structural Radial & Cross Stiffening Ribs on Top of Lid
        add_box("LidCrossRibX", (1.18, 0.075, 0.028), (0, 0, 0.474), "DarkMetal", bevel=0.005),
        add_box("LidFrontRibY", (0.085, 0.56, 0.028), (0, -0.32, 0.474), "DarkMetal", bevel=0.005),
        add_box("LidDiagRib1", (1.12, 0.056, 0.024), (0, 0, 0.472), "DarkMetal", rot=(0, 0, math.radians(45)), bevel=0.004),
        add_box("LidDiagRib2", (1.12, 0.056, 0.024), (0, 0, 0.472), "DarkMetal", rot=(0, 0, math.radians(-45)), bevel=0.004),

        # 3. High-Visibility Hazard Badges & Chunky Front Lift Handle (y_B = -0.56m)
        add_box("LidHazardPlateL", (0.22, 0.095, 0.018), (-0.28, -0.34, 0.472), "Yellow", rot=(0, 0, math.radians(-22)), bevel=0.004),
        add_box("LidHazardPlateR", (0.22, 0.095, 0.018), (0.28, -0.34, 0.472), "SafetyOrange", rot=(0, 0, math.radians(22)), bevel=0.004),
        add_box("FrontHandlePostL", (0.042, 0.052, 0.055), (-0.13, -0.555, 0.485), "SilverMetal", bevel=0.005),
        add_box("FrontHandlePostR", (0.042, 0.052, 0.055), (0.13, -0.555, 0.485), "SilverMetal", bevel=0.005),
        add_cyl("FrontGrabHandleBar", 0.020, 0.30, (0, -0.555, 0.508), "SafetyOrange", 24, rot=(0, math.radians(90), 0), bevel=0.004),
        add_cyl("FrontGrabHandleGrip", 0.023, 0.16, (0, -0.555, 0.508), "Yellow", 24, rot=(0, math.radians(90), 0), bevel=0.004),

        # 4. Massive Twin Forged-Steel Hinge Yoke Arms & 14.8cm Coaxial Barrels (Kalın Menteşe Kolları & Kovanları)
        add_cyl("MovingHingeBarrelL", 0.074, 0.195, (-0.165, 0.72, 0.425), "SilverMetal", 36, rot=(0, math.radians(90), 0), bevel=0.008),
        add_cyl("MovingHingeBarrelR", 0.074, 0.195, (0.165, 0.72, 0.425), "SilverMetal", 36, rot=(0, math.radians(90), 0), bevel=0.008),
        add_torus("HingeBearingRingL1", 0.075, 0.009, (-0.245, 0.72, 0.425), "Brass", 32, 12, rot=(0, math.radians(90), 0)),
        add_torus("HingeBearingRingL2", 0.075, 0.009, (-0.085, 0.72, 0.425), "Brass", 32, 12, rot=(0, math.radians(90), 0)),
        add_torus("HingeBearingRingR1", 0.075, 0.009, (0.085, 0.72, 0.425), "Brass", 32, 12, rot=(0, math.radians(90), 0)),
        add_torus("HingeBearingRingR2", 0.075, 0.009, (0.245, 0.72, 0.425), "Brass", 32, 12, rot=(0, math.radians(90), 0)),
        # Thick Forged Hinge Boom Straps spanning across the lid
        add_box("ForgedHingeBoomL", (0.115, 1.06, 0.062), (-0.165, 0.22, 0.478), "DarkMetal", bevel=0.010),
        add_box("ForgedHingeBoomR", (0.115, 1.06, 0.062), (0.165, 0.22, 0.478), "DarkMetal", bevel=0.010),
        add_box("HingeBoomAccentL", (0.072, 0.88, 0.018), (-0.165, 0.20, 0.508), "SafetyOrange", bevel=0.004),
        add_box("HingeBoomAccentR", (0.072, 0.88, 0.018), (0.165, 0.20, 0.508), "SafetyOrange", bevel=0.004),
        # Massive Under-Hinge Gusset Necks (Visible when hatch swings open to -108 deg!)
        add_box("UnderHingeNeckL", (0.115, 0.26, 0.082), (-0.165, 0.60, 0.405), "SilverMetal", bevel=0.008),
        add_box("UnderHingeNeckR", (0.115, 0.26, 0.082), (0.165, 0.60, 0.405), "SilverMetal", bevel=0.008),
        add_box("HingeCrossBridge", (0.48, 0.16, 0.080), (0, 0.76, 0.435), "DarkMetal", bevel=0.010),
        add_cyl("RearStopBumperL", 0.036, 0.045, (-0.165, 0.835, 0.435), "RubberBlack", 20, rot=(math.radians(90), 0, 0), bevel=0.005),
        add_cyl("RearStopBumperR", 0.036, 0.045, (0.165, 0.835, 0.435), "RubberBlack", 20, rot=(math.radians(90), 0, 0), bevel=0.005),

        # 5. Deep Stepped Underside Vault Plug, EPDM Gasket & 4-Way Locking Dogs (Visible when Open!)
        add_cyl("UndersideVaultPlug", 0.552, 0.056, (0, 0, 0.382), "DarkMetal", 48, bevel=0.010),
        add_hollow_frustum("UndersideCofferRing", 0.19, 0.485, 0.19, 0.485, 0.022, (0, 0, 0.352), "RubberBlack", segs=36, bevel=0.004),
        add_torus("UndersideRubberGasket", 0.572, 0.026, (0, 0, 0.395), "RubberBlack", 48, 16),
        add_cyl("UndersideGearBox", 0.140, 0.044, (0, 0, 0.346), "Brass", 32, bevel=0.006),
        add_cyl("UndersideGearCap", 0.082, 0.052, (0, 0, 0.342), "SilverMetal", 24, bevel=0.005),
        # 4 Heavy Radial Steel Locking Dog Bars (+X, -X, +Y, -Y)
        add_box("UndersideLockBarEW", (1.15, 0.060, 0.034), (0, 0, 0.344), "SilverMetal", bevel=0.005),
        add_box("UndersideLockBarNS", (0.060, 1.15, 0.034), (0, 0, 0.344), "SilverMetal", bevel=0.005),
        add_box("LockDogWedgeE", (0.095, 0.072, 0.042), (0.555, 0, 0.344), "Brass", bevel=0.005),
        add_box("LockDogWedgeW", (0.095, 0.072, 0.042), (-0.555, 0, 0.344), "Brass", bevel=0.005),
        add_box("LockDogWedgeN", (0.072, 0.095, 0.042), (0, 0.555, 0.344), "Brass", bevel=0.005),
        add_box("LockDogWedgeS", (0.072, 0.095, 0.042), (0, -0.555, 0.344), "Brass", bevel=0.005),
        add_box("LockBarGuideE", (0.048, 0.092, 0.044), (0.36, 0, 0.346), "SafetyOrange", bevel=0.004),
        add_box("LockBarGuideW", (0.048, 0.092, 0.044), (-0.36, 0, 0.346), "SafetyOrange", bevel=0.004),
        add_box("LockBarGuideN", (0.092, 0.048, 0.044), (0, 0.36, 0.346), "SafetyOrange", bevel=0.004),
        add_box("LockBarGuideS", (0.092, 0.048, 0.044), (0, -0.36, 0.346), "SafetyOrange", bevel=0.004),
        # Interior Emergency Pull Handle on Underside
        add_cyl("InteriorPullHandle", 0.018, 0.28, (0, -0.26, 0.320), "Yellow", 20, rot=(0, math.radians(90), 0), bevel=0.003),
    ]

    for i in range(16):
        ang = math.radians(i * 22.5 + 11.25)
        ca, sa = math.cos(ang), math.sin(ang)
        lid_parts.append(add_cyl(f"LidArmorBolt_{i}", 0.018, 0.022, (ca * 0.635, sa * 0.635, 0.474), "SilverMetal", 6, bevel=0.003))

    for y_b in (-0.22, 0.02, 0.26, 0.48):
        lid_parts.append(add_cyl(f"BoomBoltL_{y_b:.2f}", 0.018, 0.022, (-0.165, y_b, 0.518), "Brass", 6, bevel=0.003))
        lid_parts.append(add_cyl(f"BoomBoltR_{y_b:.2f}", 0.018, 0.022, (0.165, y_b, 0.518), "Brass", 6, bevel=0.003))

    # ==========================================================================
    # MOVING PART 2: LockWheel_Pivot (Coaxial 6-Spoked Central Locking Handwheel, Pivot at (0.0, 0.0, 0.535))
    # ==========================================================================
    wheel_parts = [
        add_cyl("WheelSpindleShaft", 0.040, 0.095, (0, 0, 0.520), "SilverMetal", 28, bevel=0.004),
        add_cyl("WheelSpindleCollar", 0.065, 0.028, (0, 0, 0.502), "Brass", 6, bevel=0.004),
        add_torus("WheelOuterRim", 0.245, 0.028, (0, 0, 0.552), "SafetyOrange", 40, 16),
        add_box("WheelSpokeBar1", (0.47, 0.032, 0.026), (0, 0, 0.552), "Yellow", bevel=0.005),
        add_box("WheelSpokeBar2", (0.47, 0.032, 0.026), (0, 0, 0.552), "Yellow", rot=(0, 0, math.radians(60)), bevel=0.005),
        add_box("WheelSpokeBar3", (0.47, 0.032, 0.026), (0, 0, 0.552), "Yellow", rot=(0, 0, math.radians(120)), bevel=0.005),
        add_cyl("WheelCenterHub", 0.072, 0.042, (0, 0, 0.556), "Brass", 28, bevel=0.006),
        add_cyl("WheelCenterLockNut", 0.042, 0.024, (0, 0, 0.582), "SilverMetal", 6, bevel=0.003),
        add_cyl("WheelCrankSpinner", 0.024, 0.062, (0.235, 0, 0.592), "Yellow", 20, bevel=0.005),
    ]

    # ==========================================================================
    # MOVING PART 3: HydraulicPiston_Pivot (Twin Side Assist Pistons, Pivot at (0.0, 0.44, 0.455))
    # ==========================================================================
    piston_parts = [
        add_box("PistonTopClevisL", (0.056, 0.095, 0.052), (-0.56, 0.44, 0.455), "DarkMetal", bevel=0.005),
        add_box("PistonTopClevisR", (0.056, 0.095, 0.052), (0.56, 0.44, 0.455), "DarkMetal", bevel=0.005),
        add_cyl("PistonChromeRodL", 0.020, 0.22, (-0.56, 0.48, 0.375), "SilverMetal", 20, rot=(math.radians(-18), 0, 0), bevel=0.002),
        add_cyl("PistonChromeRodR", 0.020, 0.22, (0.56, 0.48, 0.375), "SilverMetal", 20, rot=(math.radians(-18), 0, 0), bevel=0.002),
    ]

    # ==========================================================================
    # PART 4: WellPortal_Disc (Floor-Cutout Depth/Stencil Portal Mask at z = 0.008m)
    # ==========================================================================
    portal_parts = [
        add_cyl("WellPortalMaskDisc", 0.574, 0.004, (0, 0, 0.008), "ScreenBlack", 48, bevel=0.0),
    ]

    # ==========================================================================
    # PART 5: WellShaft_Deep (2.1m Deep Underground Cylindrical Well Shaft & Descending Steel Ladder)
    # ==========================================================================
    deep_shaft_parts = [
        add_hollow_frustum("DeepShaftWall", 0.568, 0.618, 0.568, 0.618, 2.108, (0, 0, -1.046), "ScreenBlack", segs=48, bevel=0.002),
        add_cyl("DeepShaftAbyssFloor", 0.615, 0.040, (0, 0, -2.100), "ScreenBlack", 48, bevel=0.002),
        add_cyl("DeepLadderRailL", 0.016, 2.02, (-0.20, 0.485, -1.00), "DarkMetal", 20, bevel=0.002),
        add_cyl("DeepLadderRailR", 0.016, 2.02, (0.20, 0.485, -1.00), "DarkMetal", 20, bevel=0.002),
        add_cyl("DeepConduitPipe", 0.020, 2.02, (0.46, 0.24, -1.00), "Copper", 20, bevel=0.002),
    ]
    for idx, z_rib in enumerate((-0.16, -0.46, -0.76, -1.06, -1.36, -1.66)):
        mat_r = "DarkMetal" if idx < 2 else ("RubberBlack" if idx < 4 else "ScreenBlack")
        deep_shaft_parts.append(add_torus(f"DeepShaftRib_{idx}", 0.562, 0.018, (0, 0, z_rib), mat_r, 48, 12))

    for idx, z_rung in enumerate((-0.05, -0.25, -0.45, -0.65, -0.85, -1.05, -1.25)):
        mat_rung = "SafetyOrange" if idx == 0 else ("Yellow" if idx == 1 else ("SilverMetal" if idx < 4 else "DarkMetal"))
        deep_shaft_parts.append(add_cyl(f"DeepLadderRung_{idx}", 0.015, 0.420, (0, 0.485, z_rung), mat_rung, 24, rot=(0, math.radians(90), 0), bevel=0.002))

    finalize_and_export_multipart(
        "BunkerHatch",
        "BunkerBaseBody",
        body_parts,
        [
            ("HatchLid_Hinge", lid_parts, (0.0, 0.72, 0.425)),
            ("LockWheel_Pivot", wheel_parts, (0.0, 0.0, 0.535)),
            ("HydraulicPiston_Pivot", piston_parts, (0.0, 0.44, 0.455)),
            ("WellPortal_Disc", portal_parts, (0.0, 0.0, 0.008)),
            ("WellShaft_Deep", deep_shaft_parts, (0.0, 0.0, 0.008)),
        ],
        toybox_scale=1.00,
        keep_exact_origin=True
    )

if __name__ == "__main__":
    generate_color_swatch_png()
    # Part A: Original 18 Upgraded Assets
    build_fire_extinguisher()
    build_blanket()
    build_snackbar()
    build_apple()
    build_canned_food()
    build_water_bottle()
    build_energy_drink()
    build_power_battery()
    build_small_battery()
    build_walkie_talkie()
    build_television()
    build_rug()
    build_folded_clothes()
    build_emergency_backpack()
    build_first_aid_kit()
    build_flashlight()
    build_emergency_radio()
    build_air_conditioner()
    # Part B: 14 IMO Disaster Expansion Assets (including Heavy Round Hinged BunkerHatch)
    build_gas_valve()
    build_water_valve()
    build_breaker_box()
    build_bookcase()
    build_refrigerator()
    build_water_heater()
    build_sturdy_table()
    build_emergency_whistle()
    build_work_gloves()
    build_multi_tool()
    build_shattered_glass()
    build_rubble_pile()
    build_fire_cabinet()
    build_bunker_hatch()
    print("=== ALL 32 ISOMETRIC 3D DISASTER ASSETS & SWATCH ATLAS GENERATED SUCCESSFULLY ===")
