import bpy
import bmesh
import math
import os
from mathutils import Vector

OUT_MESH_DIR = r"c:\GameJam\Assets\_YarkinAssets\Art\Meshes"
OUT_BLEND_DIR = r"c:\GameJam\Assets\_YarkinAssets\Art\Source_Blend~"

os.makedirs(OUT_MESH_DIR, exist_ok=True)
os.makedirs(OUT_BLEND_DIR, exist_ok=True)

# Shared stylized color palette (Linear RGBA + metallic + roughness)
PALETTE = {
    "Red":         ((0.82, 0.12, 0.10, 1.0), 0.1, 0.35),
    "DarkRed":     ((0.45, 0.06, 0.05, 1.0), 0.0, 0.55),
    "White":       ((0.92, 0.92, 0.90, 1.0), 0.05, 0.40),
    "OffWhite":    ((0.84, 0.82, 0.77, 1.0), 0.0, 0.65),
    "DarkMetal":   ((0.16, 0.17, 0.19, 1.0), 0.75, 0.35),
    "SilverMetal": ((0.72, 0.75, 0.78, 1.0), 0.85, 0.25),
    "Brass":       ((0.85, 0.64, 0.18, 1.0), 0.80, 0.30),
    "Copper":      ((0.78, 0.42, 0.20, 1.0), 0.75, 0.30),
    "Yellow":      ((0.95, 0.76, 0.12, 1.0), 0.1, 0.40),
    "Orange":      ((0.92, 0.42, 0.10, 1.0), 0.1, 0.45),
    "Blue":        ((0.12, 0.45, 0.85, 1.0), 0.1, 0.35),
    "WaterBlue":   ((0.28, 0.68, 0.95, 1.0), 0.0, 0.15),
    "NavyBlue":    ((0.10, 0.18, 0.36, 1.0), 0.0, 0.75),
    "Green":       ((0.18, 0.62, 0.24, 1.0), 0.05, 0.45),
    "OliveGreen":  ((0.25, 0.35, 0.20, 1.0), 0.0, 0.75),
    "NeonGreen":   ((0.35, 0.90, 0.15, 1.0), 0.2, 0.30),
    "BrownWood":   ((0.38, 0.22, 0.11, 1.0), 0.0, 0.65),
    "ChocBrown":   ((0.26, 0.14, 0.07, 1.0), 0.05, 0.50),
    "FabricBeige": ((0.76, 0.68, 0.55, 1.0), 0.0, 0.85),
    "FabricBlue":  ((0.20, 0.36, 0.58, 1.0), 0.0, 0.85),
    "RubberBlack": ((0.08, 0.08, 0.09, 1.0), 0.0, 0.75),
    "ScreenBlack": ((0.03, 0.04, 0.06, 1.0), 0.2, 0.15),
    "GlowWarm":    ((1.00, 0.92, 0.65, 1.0), 0.0, 0.10),
    "GlowGreen":   ((0.20, 0.95, 0.35, 1.0), 0.0, 0.15),
    "RugBurgundy": ((0.52, 0.12, 0.16, 1.0), 0.0, 0.88),
    "RugGold":     ((0.82, 0.66, 0.28, 1.0), 0.0, 0.82),
}

def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0

def get_mat(key):
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

def assign_mat(obj, key):
    mat = get_mat(key)
    obj.data.materials.clear()
    obj.data.materials.append(mat)

def add_box(name, size, loc, mat_key, rot=(0,0,0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if bevel > 0.0:
        mod = obj.modifiers.new(name="Bevel", type='BEVEL')
        mod.width = bevel
        mod.segments = 2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    assign_mat(obj, mat_key)
    return obj

def add_cyl(name, r, depth, loc, mat_key, verts=16, rot=(0,0,0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    assign_mat(obj, mat_key)
    return obj

def add_cone(name, r1, r2, depth, loc, mat_key, verts=16, rot=(0,0,0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    assign_mat(obj, mat_key)
    return obj

def add_sphere(name, r, loc, mat_key, segs=16, rings=12, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segs, ring_count=rings, radius=r, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    assign_mat(obj, mat_key)
    return obj

def finalize_and_export(asset_name, parts):
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = f"SM_{asset_name}"

    # Apply all transforms
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # Shift vertices so bottom-center is at (0,0,0)
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

    # Smart UV project
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(66.0), island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')

    # Save .blend and export .fbx
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
        mesh_smooth_type='FACE',
        bake_space_transform=True,
        object_types={'MESH'}
    )
    print(f"[OK] Exported SM_{asset_name}.fbx ({len(obj.data.polygons)} polys)")

# ---------------------------------------------------------
# 1. FireExtinguisher (Yangın Tüpü) ~0.16 x 0.16 x 0.52m
# ---------------------------------------------------------
def build_fire_extinguisher():
    clear_scene()
    parts = [
        add_cyl("Body", 0.075, 0.36, (0, 0, 0.19), "Red", 20),
        add_sphere("DomeTop", 0.075, (0, 0, 0.37), "Red", 20, 10, (1, 1, 0.7)),
        add_cyl("BaseRing", 0.077, 0.03, (0, 0, 0.015), "RubberBlack", 20),
        add_Box := add_box("Label", (0.09, 0.005, 0.16), (0, -0.075, 0.20), "White"),
        add_cyl("Neck", 0.024, 0.05, (0, 0, 0.43), "Brass", 14),
        add_box("HandleFixed", (0.02, 0.09, 0.012), (0, 0.03, 0.46), "DarkMetal"),
        add_box("HandleLever", (0.02, 0.095, 0.012), (0, 0.032, 0.485), "Red", rot=(math.radians(15), 0, 0)),
        add_cyl("Gauge", 0.014, 0.015, (0.026, 0, 0.44), "Green", 12, rot=(0, math.radians(90), 0)),
        add_cyl("PinRing", 0.012, 0.005, (-0.025, 0, 0.45), "Yellow", 12, rot=(math.radians(90), 0, 0)),
        add_cyl("HoseUpper", 0.011, 0.24, (-0.08, 0, 0.31), "RubberBlack", 12, rot=(0, math.radians(12), 0)),
        add_cone("NozzleHorn", 0.026, 0.012, 0.11, (-0.095, 0, 0.15), "RubberBlack", 14),
        add_box("BeltClip", (0.158, 0.158, 0.018), (0, 0, 0.24), "RubberBlack"),
    ]
    finalize_and_export("FireExtinguisher", parts)

# ---------------------------------------------------------
# 2. Blanket (Battaniye - Katlanmış + Rulo) ~0.46 x 0.34 x 0.14m
# ---------------------------------------------------------
def build_blanket():
    clear_scene()
    parts = [
        add_box("FoldBottom", (0.44, 0.32, 0.042), (0, 0, 0.021), "FabricBlue", bevel=0.012),
        add_box("FoldMid", (0.42, 0.30, 0.040), (0, 0.005, 0.062), "OffWhite", bevel=0.012),
        add_box("FoldTop", (0.40, 0.28, 0.042), (0, 0, 0.103), "FabricBlue", bevel=0.012),
        add_box("StripeA", (0.405, 0.05, 0.045), (0, -0.06, 0.103), "Orange", bevel=0.005),
        add_box("StripeB", (0.405, 0.05, 0.045), (0, 0.06, 0.103), "Orange", bevel=0.005),
        add_box("Strap1", (0.03, 0.33, 0.13), (-0.12, 0, 0.065), "BrownWood", bevel=0.003),
        add_box("Strap2", (0.03, 0.33, 0.13), (0.12, 0, 0.065), "BrownWood", bevel=0.003),
    ]
    finalize_and_export("Blanket", parts)

# ---------------------------------------------------------
# 3. SnackBar (Enerji / Protein Barı) ~0.14 x 0.05 x 0.025m
# ---------------------------------------------------------
def build_snackbar():
    clear_scene()
    parts = [
        add_box("WrapperMain", (0.11, 0.046, 0.022), (0, 0, 0.011), "Orange", bevel=0.004),
        add_box("WrapperBand", (0.04, 0.048, 0.024), (-0.01, 0, 0.011), "Yellow", bevel=0.002),
        add_box("ChocLabel", (0.03, 0.036, 0.025), (0.025, 0, 0.011), "ChocBrown"),
        add_box("CrimpedLeft", (0.018, 0.050, 0.005), (-0.062, 0, 0.011), "SilverMetal"),
        add_box("CrimpedRight", (0.018, 0.050, 0.005), (0.062, 0, 0.011), "SilverMetal"),
    ]
    finalize_and_export("SnackBar", parts)

# ---------------------------------------------------------
# 4. Apple (Elma) ~0.09 x 0.09 x 0.10m
# ---------------------------------------------------------
def build_apple():
    clear_scene()
    parts = [
        add_sphere("AppleBody", 0.044, (0, 0, 0.042), "Red", 18, 12, (1.0, 1.0, 0.92)),
        add_cyl("Stem", 0.0035, 0.022, (0, 0, 0.088), "BrownWood", 8, rot=(math.radians(10), 0, 0)),
        add_box("Leaf", (0.024, 0.012, 0.003), (0.012, 0, 0.092), "Green", rot=(0, math.radians(-20), math.radians(15)), bevel=0.001),
    ]
    finalize_and_export("Apple", parts)

# ---------------------------------------------------------
# 5. CannedFood (Konserve Kutusu) ~0.10 x 0.10 x 0.12m
# ---------------------------------------------------------
def build_canned_food():
    clear_scene()
    parts = [
        add_cyl("CanBody", 0.046, 0.11, (0, 0, 0.055), "SilverMetal", 20),
        add_cyl("LabelWrap", 0.047, 0.075, (0, 0, 0.055), "Orange", 20),
        add_cyl("LabelStripe", 0.0475, 0.022, (0, 0, 0.055), "White", 20),
        add_cyl("RimBottom", 0.048, 0.006, (0, 0, 0.003), "SilverMetal", 20),
        add_cyl("RimTop", 0.048, 0.006, (0, 0, 0.107), "SilverMetal", 20),
        add_box("PullTab", (0.022, 0.014, 0.003), (0.01, 0, 0.111), "SilverMetal"),
    ]
    finalize_and_export("CannedFood", parts)

# ---------------------------------------------------------
# 6. WaterBottle (Su Şişesi) ~0.08 x 0.08 x 0.25m
# ---------------------------------------------------------
def build_water_bottle():
    clear_scene()
    parts = [
        add_cyl("LowerBody", 0.038, 0.15, (0, 0, 0.075), "WaterBlue", 18),
        add_cyl("Label", 0.039, 0.055, (0, 0, 0.085), "Blue", 18),
        add_cyl("LabelWhite", 0.0395, 0.016, (0, 0, 0.085), "White", 18),
        add_cone("Shoulder", 0.038, 0.015, 0.055, (0, 0, 0.1775), "WaterBlue", 18),
        add_cyl("Neck", 0.014, 0.025, (0, 0, 0.215), "WaterBlue", 16),
        add_cyl("Cap", 0.016, 0.020, (0, 0, 0.235), "Blue", 16),
    ]
    finalize_and_export("WaterBottle", parts)

# ---------------------------------------------------------
# 7. EnergyDrink (Enerji İçeceği Kutusu) ~0.066 x 0.066 x 0.165m
# ---------------------------------------------------------
def build_energy_drink():
    clear_scene()
    parts = [
        add_cyl("CanCore", 0.032, 0.145, (0, 0, 0.075), "RubberBlack", 20),
        add_cyl("NeonBandTop", 0.0325, 0.028, (0, 0, 0.115), "NeonGreen", 20),
        add_cyl("NeonBandMid", 0.0325, 0.018, (0, 0, 0.065), "NeonGreen", 20),
        add_cone("TopTaper", 0.032, 0.027, 0.012, (0, 0, 0.153), "SilverMetal", 20),
        add_cyl("BottomRim", 0.029, 0.008, (0, 0, 0.004), "SilverMetal", 20),
        add_box("Tab", (0.018, 0.011, 0.002), (0, 0, 0.160), "NeonGreen"),
    ]
    finalize_and_export("EnergyDrink", parts)

# ---------------------------------------------------------
# 8. PowerBattery (Büyük Batarya / Akü / Powerbank) ~0.22 x 0.14 x 0.17m
# ---------------------------------------------------------
def build_power_battery():
    clear_scene()
    parts = [
        add_box("CaseMain", (0.22, 0.14, 0.13), (0, 0, 0.065), "RubberBlack", bevel=0.006),
        add_box("LidTop", (0.224, 0.144, 0.025), (0, 0, 0.14), "Red", bevel=0.004),
        add_box("FrontLabel", (0.16, 0.006, 0.075), (0, -0.071, 0.065), "Yellow"),
        add_cyl("TermPos", 0.014, 0.024, (-0.065, 0, 0.162), "Red", 14),
        add_cyl("TermNeg", 0.014, 0.024, (0.065, 0, 0.162), "Blue", 14),
        add_box("HandleBar", (0.14, 0.022, 0.012), (0, 0, 0.175), "DarkMetal"),
    ]
    finalize_and_export("PowerBattery", parts)

# ---------------------------------------------------------
# 9. SmallBattery (Küçük Kalem Pil / AA Pack) ~0.036 x 0.036 x 0.095m
# ---------------------------------------------------------
def build_small_battery():
    clear_scene()
    parts = [
        add_cyl("CellBody", 0.018, 0.062, (0, 0, 0.031), "RubberBlack", 18),
        add_cyl("CopperTop", 0.0182, 0.026, (0, 0, 0.075), "Copper", 18),
        add_cyl("SilverRing", 0.0183, 0.005, (0, 0, 0.060), "SilverMetal", 18),
        add_cyl("PosCap", 0.007, 0.008, (0, 0, 0.091), "SilverMetal", 14),
    ]
    finalize_and_export("SmallBattery", parts)

# ---------------------------------------------------------
# 10. WalkieTalkie (El Telsizi) ~0.07 x 0.045 x 0.24m
# ---------------------------------------------------------
def build_walkie_talkie():
    clear_scene()
    parts = [
        add_box("Body", (0.068, 0.042, 0.145), (0, 0, 0.0725), "RubberBlack", bevel=0.005),
        add_box("GripYellow", (0.072, 0.036, 0.085), (0, 0, 0.055), "Yellow", bevel=0.003),
        add_box("Screen", (0.046, 0.006, 0.032), (0, -0.020, 0.105), "GlowGreen"),
        add_box("SpeakerGrill", (0.048, 0.006, 0.042), (0, -0.020, 0.048), "DarkMetal"),
        add_cyl("Antenna", 0.006, 0.095, (-0.018, 0, 0.19), "RubberBlack", 12),
        add_cyl("ChannelKnob", 0.009, 0.018, (0.016, 0, 0.152), "Orange", 12),
        add_box("PTTButton", (0.008, 0.018, 0.032), (-0.036, 0, 0.095), "Orange"),
    ]
    finalize_and_export("WalkieTalkie", parts)

# ---------------------------------------------------------
# 11. Television (Televizyon) ~0.96 x 0.22 x 0.62m
# ---------------------------------------------------------
def build_television():
    clear_scene()
    parts = [
        add_box("BasePlate", (0.42, 0.22, 0.022), (0, 0, 0.011), "DarkMetal", bevel=0.004),
        add_box("StandNeck", (0.10, 0.06, 0.09), (0, 0.02, 0.065), "DarkMetal"),
        add_box("PanelFrame", (0.96, 0.055, 0.54), (0, 0, 0.36), "RubberBlack", bevel=0.008),
        add_box("ScreenGlass", (0.90, 0.01, 0.48), (0, -0.025, 0.36), "ScreenBlack"),
        add_box("AlertBanner", (0.76, 0.012, 0.08), (0, -0.027, 0.36), "Red"),
        add_box("PowerLED", (0.016, 0.012, 0.008), (0.42, -0.028, 0.10), "Red"),
    ]
    finalize_and_export("Television", parts)

# ---------------------------------------------------------
# 12. Rug (Halı) ~1.80 x 1.20 x 0.016m
# ---------------------------------------------------------
def build_rug():
    clear_scene()
    parts = [
        add_box("RugFringe", (1.86, 1.16, 0.008), (0, 0, 0.004), "OffWhite"),
        add_box("RugOuter", (1.76, 1.18, 0.012), (0, 0, 0.006), "RugBurgundy", bevel=0.003),
        add_box("RugGoldBorder", (1.52, 0.96, 0.014), (0, 0, 0.007), "RugGold"),
        add_box("RugInner", (1.36, 0.80, 0.015), (0, 0, 0.0075), "NavyBlue"),
        add_box("RugMedallion", (0.55, 0.38, 0.016), (0, 0, 0.008), "RugGold", rot=(0, 0, math.radians(0))),
    ]
    finalize_and_export("Rug", parts)

# ---------------------------------------------------------
# 13. FoldedClothes (Katlanmış Kıyafet Yığını) ~0.38 x 0.30 x 0.15m
# ---------------------------------------------------------
def build_folded_clothes():
    clear_scene()
    parts = [
        add_box("JeansBottom", (0.36, 0.28, 0.045), (0, 0, 0.0225), "NavyBlue", bevel=0.012),
        add_box("SweaterMid", (0.34, 0.26, 0.048), (0.005, -0.005, 0.068), "Red", bevel=0.012),
        add_box("ShirtTop", (0.32, 0.24, 0.042), (-0.004, 0.004, 0.112), "Yellow", bevel=0.010),
        add_box("CollarFold", (0.12, 0.10, 0.016), (0, -0.05, 0.138), "White", bevel=0.004),
    ]
    finalize_and_export("FoldedClothes", parts)

# ---------------------------------------------------------
# 14. EmergencyBackpack (Afet / Acil Durum Çantası) ~0.36 x 0.24 x 0.48m
# ---------------------------------------------------------
def build_emergency_backpack():
    clear_scene()
    parts = [
        add_box("MainPack", (0.34, 0.18, 0.44), (0, 0.02, 0.22), "Orange", bevel=0.025),
        add_box("BottomReinforce", (0.345, 0.185, 0.09), (0, 0.02, 0.045), "RubberBlack", bevel=0.012),
        add_box("FrontPocket", (0.26, 0.06, 0.22), (0, -0.09, 0.15), "Red", bevel=0.015),
        add_box("ReflectiveStrip1", (0.24, 0.01, 0.028), (0, -0.12, 0.20), "Yellow"),
        add_box("ReflectiveStrip2", (0.28, 0.01, 0.028), (0, -0.072, 0.34), "White"),
        add_cyl("SideBottlePocketL", 0.042, 0.16, (-0.18, 0.02, 0.11), "DarkMetal", 12),
        add_cyl("SideBottlePocketR", 0.042, 0.16, (0.18, 0.02, 0.11), "DarkMetal", 12),
        add_cyl("SleepingMatRoll", 0.055, 0.36, (0, 0.02, 0.475), "OliveGreen", 16, rot=(0, math.radians(90), 0)),
        add_box("ShoulderStrapL", (0.055, 0.03, 0.34), (-0.09, 0.12, 0.22), "RubberBlack", bevel=0.005),
        add_box("ShoulderStrapR", (0.055, 0.03, 0.34), (0.09, 0.12, 0.22), "RubberBlack", bevel=0.005),
    ]
    finalize_and_export("EmergencyBackpack", parts)

# ---------------------------------------------------------
# 15. FirstAidKit (İlk Yardım Çantası) ~0.30 x 0.22 x 0.12m
# ---------------------------------------------------------
def build_first_aid_kit():
    clear_scene()
    parts = [
        add_box("CaseBottom", (0.28, 0.20, 0.055), (0, 0, 0.0275), "White", bevel=0.010),
        add_box("CaseRim", (0.284, 0.204, 0.012), (0, 0, 0.055), "Red"),
        add_box("CaseTop", (0.28, 0.20, 0.050), (0, 0, 0.082), "White", bevel=0.010),
        add_box("CrossHoriz", (0.095, 0.030, 0.008), (0, 0, 0.109), "Red"),
        add_box("CrossVert", (0.030, 0.095, 0.008), (0, 0, 0.109), "Red"),
        add_box("Handle", (0.11, 0.025, 0.020), (0, -0.11, 0.055), "Red", bevel=0.004),
        add_box("LatchL", (0.028, 0.012, 0.026), (-0.08, -0.102, 0.055), "DarkMetal"),
        add_box("LatchR", (0.028, 0.012, 0.026), (0.08, -0.102, 0.055), "DarkMetal"),
    ]
    finalize_and_export("FirstAidKit", parts)

# ---------------------------------------------------------
# 16. Flashlight (El Feneri) ~0.07 x 0.22 x 0.07m
# ---------------------------------------------------------
def build_flashlight():
    clear_scene()
    parts = [
        add_cyl("GripTube", 0.022, 0.15, (0, 0.02, 0.035), "RubberBlack", 18, rot=(math.radians(90), 0, 0)),
        add_cyl("GripBand", 0.023, 0.07, (0, 0.03, 0.035), "Yellow", 18, rot=(math.radians(90), 0, 0)),
        add_cone("HeadCone", 0.035, 0.022, 0.055, (0, -0.075, 0.035), "DarkMetal", 20, rot=(math.radians(-90), 0, 0)),
        add_cyl("BezelRim", 0.036, 0.015, (0, -0.105, 0.035), "Yellow", 20, rot=(math.radians(90), 0, 0)),
        add_cyl("LensGlow", 0.031, 0.006, (0, -0.111, 0.035), "GlowWarm", 20, rot=(math.radians(90), 0, 0)),
        add_box("PowerButton", (0.014, 0.022, 0.008), (0, -0.01, 0.058), "Red"),
    ]
    finalize_and_export("Flashlight", parts)

# ---------------------------------------------------------
# 17. EmergencyRadio (Acil Durum Radyosu) ~0.26 x 0.11 x 0.18m
# ---------------------------------------------------------
def build_emergency_radio():
    clear_scene()
    parts = [
        add_box("RadioBody", (0.25, 0.095, 0.145), (0, 0, 0.0725), "Red", bevel=0.008),
        add_box("SideBumperL", (0.022, 0.102, 0.150), (-0.12, 0, 0.075), "RubberBlack", bevel=0.004),
        add_box("SideBumperR", (0.022, 0.102, 0.150), (0.12, 0, 0.075), "RubberBlack", bevel=0.004),
        add_cyl("SpeakerGrill", 0.048, 0.012, (-0.05, -0.046, 0.072), "DarkMetal", 20, rot=(math.radians(90), 0, 0)),
        add_box("DialWindow", (0.085, 0.010, 0.036), (0.052, -0.046, 0.102), "GlowWarm"),
        add_cyl("TuningKnob", 0.018, 0.018, (0.035, -0.052, 0.048), "Yellow", 16, rot=(math.radians(90), 0, 0)),
        add_cyl("VolumeKnob", 0.014, 0.016, (0.080, -0.052, 0.048), "DarkMetal", 16, rot=(math.radians(90), 0, 0)),
        add_box("TopHandle", (0.14, 0.024, 0.016), (0, 0, 0.162), "RubberBlack"),
        add_cyl("WhipAntenna", 0.004, 0.20, (-0.09, 0.02, 0.22), "SilverMetal", 10, rot=(0, math.radians(22), 0)),
    ]
    finalize_and_export("EmergencyRadio", parts)

# ---------------------------------------------------------
# 18. AirConditioner (Duvar Tipi Split Klima) ~0.88 x 0.24 x 0.30m
# ---------------------------------------------------------
def build_air_conditioner():
    clear_scene()
    parts = [
        add_box("MainUnit", (0.86, 0.22, 0.28), (0, 0, 0.14), "White", bevel=0.016),
        add_box("FrontPanel", (0.83, 0.02, 0.17), (0, -0.105, 0.165), "OffWhite", bevel=0.006),
        add_box("AirVentFlap", (0.80, 0.03, 0.035), (0, -0.102, 0.035), "DarkMetal", rot=(math.radians(20), 0, 0)),
        add_box("TempDisplay", (0.075, 0.008, 0.028), (0.28, -0.114, 0.155), "GlowGreen"),
        add_box("BrandBadge", (0.06, 0.006, 0.016), (0, -0.114, 0.155), "SilverMetal"),
    ]
    finalize_and_export("AirConditioner", parts)

if __name__ == "__main__":
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
    print("=== ALL 18 ASSETS GENERATED SUCCESSFULLY ===")
