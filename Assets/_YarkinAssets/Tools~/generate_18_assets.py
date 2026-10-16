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

def finalize_and_export(asset_name, parts, toybox_scale=1.0):
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = f"SM_{asset_name}"

    if toybox_scale != 1.0:
        obj.scale = (toybox_scale, toybox_scale, toybox_scale)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

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

# 18. AirConditioner (Klima - 1.00x wedge profile so isometric camera sees vents & display)
def build_air_conditioner():
    clear_scene()
    parts = [
        add_box("Chassis", (0.86, 0.24, 0.28), (0, 0, 0.14), "White", bevel=0.042, segs=5, subsurf=1),
        add_box("SlopedFront", (0.83, 0.045, 0.18), (0, -0.105, 0.165), "OffWhite", rot=(math.radians(-12), 0, 0), bevel=0.018, segs=5, subsurf=1),
        add_box("ChromeStrip", (0.82, 0.016, 0.014), (0, -0.122, 0.102), "SilverMetal", bevel=0.004),
        add_box("VentFlap1", (0.78, 0.038, 0.016), (0, -0.110, 0.044), "DarkMetal", rot=(math.radians(28), 0, 0), bevel=0.004),
        add_box("VentFlap2", (0.78, 0.032, 0.014), (0, -0.102, 0.024), "DarkMetal", rot=(math.radians(28), 0, 0), bevel=0.004),
        add_box("TempLED", (0.095, 0.016, 0.038), (0.26, -0.120, 0.175), "GlowGreen", rot=(math.radians(-12), 0, 0), bevel=0.004),
    ]
    finalize_and_export("AirConditioner", parts, toybox_scale=1.00)

# ==============================================================================
# PART B: 14 NEW IMO DISASTER EXPANSION ASSETS (Infrastructure, Fixtures, Kits)
# ==============================================================================

# 19. GasValve (Doğalgaz Ana Kesme Vanası — Stops post-quake Fire Wave)
def build_gas_valve():
    clear_scene()
    parts = [
        add_box("WallPlate", (0.24, 0.035, 0.42), (0, 0.06, 0.21), "DarkMetal", bevel=0.008),
        add_cyl("PipeLower", 0.032, 0.20, (0, 0, 0.10), "Yellow", 32, bevel=0.004),
        add_cyl("PipeUpper", 0.032, 0.20, (0, 0, 0.32), "Yellow", 32, bevel=0.004),
        add_sphere("ValveValveBody", 0.055, (0, 0, 0.21), "Brass", 32, 20),
        add_cyl("FlangeTop", 0.052, 0.022, (0, 0, 0.265), "Brass", 28, bevel=0.004),
        add_cyl("FlangeBot", 0.052, 0.022, (0, 0, 0.155), "Brass", 28, bevel=0.004),
        add_cyl("ValveStem", 0.022, 0.045, (0, -0.055, 0.21), "Brass", 24, rot=(math.radians(90), 0, 0), bevel=0.003),
        add_box("LeverHandle", (0.038, 0.022, 0.19), (0, -0.082, 0.27), "Red", bevel=0.006),
        add_cyl("GasGauge", 0.038, 0.024, (0.068, -0.01, 0.31), "White", 28, rot=(math.radians(90), 0, 0), bevel=0.004),
    ]
    finalize_and_export("GasValve", parts, toybox_scale=1.25)

# 20. WaterValve (Su Sayacı ve Ana Kesme Vanası — Stops Flood Wave)
def build_water_valve():
    clear_scene()
    parts = [
        add_box("MountBoard", (0.38, 0.035, 0.30), (0, 0.06, 0.15), "DarkMetal", bevel=0.008),
        add_cyl("PipeHoriz", 0.030, 0.36, (0, 0, 0.15), "SilverMetal", 32, rot=(0, math.radians(90), 0), bevel=0.004),
        add_cyl("MeterBody", 0.058, 0.085, (-0.06, 0, 0.165), "Brass", 32, bevel=0.006),
        add_cyl("MeterGlass", 0.048, 0.015, (-0.06, 0, 0.210), "WaterBlue", 32, bevel=0.003),
        add_cyl("ValveStem", 0.020, 0.065, (0.075, 0, 0.195), "Brass", 24, bevel=0.003),
        add_torus("WheelRim", 0.062, 0.012, (0.075, 0, 0.230), "Blue", 32, 14),
        add_box("WheelSpoke1", (0.12, 0.014, 0.012), (0.075, 0, 0.230), "Blue", bevel=0.003),
        add_box("WheelSpoke2", (0.014, 0.12, 0.012), (0.075, 0, 0.230), "Blue", bevel=0.003),
    ]
    finalize_and_export("WaterValve", parts, toybox_scale=1.25)

# 21. BreakerBox (Elektrik Ana Sigorta Panosu — Stops Electrical Fire/Shock)
def build_breaker_box():
    clear_scene()
    parts = [
        add_box("Cabinet", (0.36, 0.11, 0.48), (0, 0, 0.24), "DarkMetal", bevel=0.014, segs=5),
        add_box("FrontFrame", (0.33, 0.022, 0.44), (0, -0.052, 0.24), "SilverMetal", bevel=0.008),
        add_box("HazardSign", (0.12, 0.012, 0.11), (0, -0.062, 0.37), "Yellow", bevel=0.004),
        add_box("MainSwitchSlot", (0.24, 0.018, 0.14), (0, -0.058, 0.21), "RubberBlack", bevel=0.005),
        add_box("MainBreakerToggle", (0.065, 0.038, 0.065), (-0.05, -0.072, 0.21), "Red", bevel=0.006),
        add_box("SubBreaker1", (0.032, 0.030, 0.050), (0.02, -0.068, 0.21), "Blue", bevel=0.004),
        add_box("SubBreaker2", (0.032, 0.030, 0.050), (0.065, -0.068, 0.21), "Blue", bevel=0.004),
        add_sphere("StatusLED", 0.014, (0.11, -0.065, 0.37), "GlowGreen", 20, 14),
    ]
    finalize_and_export("BreakerBox", parts, toybox_scale=1.15)

# 22. Bookcase (Büyük Ahşap Kitaplık — Securable Heavy Fixture)
def build_bookcase():
    clear_scene()
    parts = [
        add_box("BackPanel", (0.88, 0.03, 1.78), (0, 0.15, 0.89), "BrownWood", bevel=0.008),
        add_box("SideWallL", (0.045, 0.34, 1.80), (-0.42, 0, 0.90), "BrownWood", bevel=0.010),
        add_box("SideWallR", (0.045, 0.34, 1.80), (0.42, 0, 0.90), "BrownWood", bevel=0.010),
        add_box("Shelf1", (0.84, 0.33, 0.038), (0, 0, 0.04), "BrownWood", bevel=0.008),
        add_box("Shelf2", (0.84, 0.33, 0.038), (0, 0, 0.48), "BrownWood", bevel=0.008),
        add_box("Shelf3", (0.84, 0.33, 0.038), (0, 0, 0.92), "BrownWood", bevel=0.008),
        add_box("Shelf4", (0.84, 0.33, 0.038), (0, 0, 1.36), "BrownWood", bevel=0.008),
        add_box("TopCrown", (0.90, 0.36, 0.045), (0, 0, 1.78), "BrownWood", bevel=0.012),
        # Colorful books on shelves
        add_box("BookCluster1", (0.28, 0.22, 0.30), (-0.22, -0.02, 1.10), "Red", bevel=0.010),
        add_box("BookCluster2", (0.22, 0.22, 0.28), (0.15, -0.02, 1.09), "Blue", bevel=0.010),
        add_box("BookCluster3", (0.32, 0.22, 0.31), (0.12, -0.02, 0.66), "Yellow", bevel=0.010),
        add_box("BookCluster4", (0.24, 0.22, 0.26), (-0.18, -0.02, 1.52), "Green", bevel=0.010),
    ]
    finalize_and_export("Bookcase", parts, toybox_scale=1.00)

# 23. Refrigerator (Mutfak Buzdolabı — Securable Heavy Fixture)
def build_refrigerator():
    clear_scene()
    parts = [
        add_box("MainCabinet", (0.76, 0.70, 1.76), (0, 0, 0.88), "White", bevel=0.035, segs=5, subsurf=1),
        add_box("LowerDoor", (0.74, 0.055, 1.06), (0, -0.35, 0.55), "OffWhite", bevel=0.022, segs=5),
        add_box("UpperFreezerDoor", (0.74, 0.055, 0.58), (0, -0.35, 1.44), "OffWhite", bevel=0.022, segs=5),
        add_box("GapTrim", (0.75, 0.065, 0.025), (0, -0.35, 1.11), "DarkMetal", bevel=0.005),
        add_cyl("HandleLower", 0.016, 0.42, (-0.30, -0.40, 0.78), "SilverMetal", 24, bevel=0.004),
        add_cyl("HandleUpper", 0.016, 0.28, (-0.30, -0.40, 1.42), "SilverMetal", 24, bevel=0.004),
        add_box("DigitalPanel", (0.14, 0.015, 0.08), (0, -0.38, 1.52), "GlowGreen", bevel=0.004),
    ]
    finalize_and_export("Refrigerator", parts, toybox_scale=1.00)

# 24. WaterHeater (Banyo Termosifonu / Şofben — High-Risk Securable Fixture)
def build_water_heater():
    clear_scene()
    parts = [
        add_cyl("TankCylinder", 0.23, 0.74, (0, 0, 0.44), "White", 40, bevel=0.025),
        add_sphere("TopDome", 0.23, (0, 0, 0.81), "White", 40, 20, (1, 1, 0.45)),
        add_sphere("BotDome", 0.23, (0, 0, 0.07), "White", 40, 20, (1, 1, 0.45)),
        add_box("ThermostatDialBox", (0.16, 0.04, 0.12), (0, -0.22, 0.26), "DarkMetal", bevel=0.008),
        add_cyl("TempKnob", 0.032, 0.025, (-0.03, -0.24, 0.26), "SafetyOrange", 24, rot=(math.radians(90), 0, 0), bevel=0.004),
        add_sphere("HeatLED", 0.014, (0.04, -0.24, 0.26), "Red", 18, 12),
        add_cyl("PipeHot", 0.018, 0.09, (-0.07, 0, 0.03), "Red", 20, bevel=0.003),
        add_cyl("PipeCold", 0.018, 0.09, (0.07, 0, 0.03), "Blue", 20, bevel=0.003),
    ]
    finalize_and_export("WaterHeater", parts, toybox_scale=1.05)

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

# 27. DocumentPouch (Su Geçirmez Evrak Dosyası — Tapu/DASK/Kimlik - 1.35x)
def build_document_pouch():
    clear_scene()
    parts = [
        add_box("WaterproofSleeve", (0.32, 0.24, 0.028), (0, 0, 0.014), "WaterBlue", bevel=0.009, segs=5, subsurf=1),
        add_box("ZipperHeader", (0.33, 0.032, 0.034), (0, 0.11, 0.017), "SafetyOrange", bevel=0.006),
        add_box("DocumentPaper", (0.26, 0.18, 0.030), (-0.01, -0.01, 0.015), "White", bevel=0.004),
        add_box("OfficialSealBadge", (0.065, 0.045, 0.033), (0.07, -0.03, 0.016), "Red", bevel=0.004),
        add_box("IDCard", (0.085, 0.055, 0.032), (-0.06, -0.04, 0.016), "Yellow", bevel=0.004),
    ]
    finalize_and_export("DocumentPouch", parts, toybox_scale=1.35)

# 28. WorkGloves (Ağır Hizmet İş Eldiveni - 1.45x)
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

# 29. MultiTool (Çok Amaçlı Pense / Çakı - 1.65x)
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

# 30. DustMask (FFP3 Valfli Toz Maskesi - 1.55x)
def build_dust_mask():
    clear_scene()
    parts = [
        add_sphere("MaskCup", 0.082, (0, 0, 0.042), "White", 36, 20, scale=(1.10, 1.25, 0.65)),
        add_box("NoseClip", (0.058, 0.018, 0.016), (0, -0.062, 0.088), "SilverMetal", bevel=0.004),
        add_cyl("ExhaleValve", 0.024, 0.018, (0.026, -0.015, 0.085), "SafetyOrange", 28, rot=(math.radians(-15), math.radians(18), 0), bevel=0.004),
        add_torus("ElasticStrap", 0.095, 0.005, (0, 0.02, 0.022), "Yellow", 36, 12, scale=(1.05, 1.20, 0.45)),
    ]
    finalize_and_export("DustMask", parts, toybox_scale=1.55)

# 31. ShatteredGlass (Kırık Cam Parçaları — Floor Hazard)
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

# 32. RubblePile (Dökülmüş Sıva ve Tuğla Moloz Yığını — Corridor Obstacle)
def build_rubble_pile():
    clear_scene()
    parts = [
        add_sphere("DustMound", 0.34, (0, 0, 0.06), "OffWhite", 32, 16, scale=(1.25, 1.05, 0.32)),
        add_box("Brick1", (0.20, 0.10, 0.075), (-0.14, -0.08, 0.09), "Red", rot=(math.radians(18), math.radians(-12), math.radians(25)), bevel=0.008),
        add_box("Brick2", (0.20, 0.10, 0.075), (0.15, 0.06, 0.10), "DarkRed", rot=(math.radians(-15), math.radians(22), math.radians(-35)), bevel=0.008),
        add_box("ConcreteChunk1", (0.24, 0.18, 0.11), (0.02, -0.04, 0.13), "DarkMetal", rot=(math.radians(12), math.radians(15), math.radians(10)), bevel=0.016, subsurf=1),
        add_box("ConcreteChunk2", (0.18, 0.15, 0.09), (-0.08, 0.12, 0.09), "OffWhite", rot=(math.radians(-20), math.radians(-8), math.radians(50)), bevel=0.014, subsurf=1),
    ]
    finalize_and_export("RubblePile", parts, toybox_scale=1.10)

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
    # Part B: 14 New IMO Disaster Expansion Assets
    build_gas_valve()
    build_water_valve()
    build_breaker_box()
    build_bookcase()
    build_refrigerator()
    build_water_heater()
    build_sturdy_table()
    build_emergency_whistle()
    build_document_pouch()
    build_work_gloves()
    build_multi_tool()
    build_dust_mask()
    build_shattered_glass()
    build_rubble_pile()
    print("=== ALL 32 ISOMETRIC 3D DISASTER ASSETS & SWATCH ATLAS GENERATED SUCCESSFULLY ===")
