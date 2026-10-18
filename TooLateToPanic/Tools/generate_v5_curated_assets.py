import bpy
import bmesh
import math
import os
import hashlib

OUT_DIR = r"c:\ToLateToPanic\Assets\Game\Resources\CuratedYarkin\Models"
os.makedirs(OUT_DIR, exist_ok=True)

# Exact 28 CuratedYarkin Material names and GUIDs
MATERIAL_GUIDS = {
    "M_Blue": "f2987bce6e7459e49bccefdca296b5e4",
    "M_Brass": "86dd633e6d4b3e6488c49a33afdd686c",
    "M_BrownWood": "eb3e44ea66a00344f8ff5e83db0510d4",
    "M_ChocBrown": "2d75d1c2f85db0c45bf559985d26ff19",
    "M_Copper": "cb8001ea17faca446add2a9eb6687a57",
    "M_DarkMetal": "f16e0db8441e70e4b88cef6552551c4f",
    "M_DarkRed": "efe29c98227fe504a8fd00f31c3704f9",
    "M_FabricBeige": "18ee7dfc9e4ba45419fd3e84d875df80",
    "M_FabricBlue": "b2e8b03e9c8531449b8d261e3261ee6f",
    "M_GlassCyan": "dc4cb32416eb1244d8c57888bc0b1c23",
    "M_GlowGreen": "32d81ef227e6165419bd9a50f28134c5",
    "M_GlowWarm": "2c6552f4876310942b85cdc2ea07b785",
    "M_Green": "4c2827999c32caa46b43ca5ca92c6e66",
    "M_NavyBlue": "a9b622801357d8b4eb71fbddf77c8477",
    "M_NeonGreen": "4352c8d93aaec5c498d6cf850cd9c5b9",
    "M_OffWhite": "fd88e5c1fbd078842804b1b90fdfb058",
    "M_OliveGreen": "c6ffe3915edff414ab76b24debc343a1",
    "M_Orange": "c29ec13213db5cb4b8f0edddcd13e9b0",
    "M_Red": "367c754a017448f45a386881b75ed074",
    "M_RubberBlack": "1163e5b3512610e45ba91d18d7982a91",
    "M_RugBurgundy": "58b495a286e1acb4cacf2628ea07ce88",
    "M_RugGold": "7ae1f522ff80daa45ade267a05b2aa68",
    "M_SafetyOrange": "db7fc5eaf83e4f34b834ea42356bc2f1",
    "M_ScreenBlack": "c2176d341fcf2274790794df777e91ad",
    "M_SilverMetal": "1fc0346927a177946bf038f4168dcc00",
    "M_WaterBlue": "1b2c0705025feac4ba29ca33202bc5ad",
    "M_White": "a668328563a2b89469a148568b9178be",
    "M_Yellow": "dcacf34771d381b47be0502644d216b8",
}

MATERIAL_COLORS = {
    "M_Blue": (0.12, 0.38, 0.85, 1.0),
    "M_Brass": (0.78, 0.62, 0.24, 1.0),
    "M_BrownWood": (0.42, 0.28, 0.16, 1.0),
    "M_ChocBrown": (0.24, 0.15, 0.09, 1.0),
    "M_Copper": (0.72, 0.40, 0.25, 1.0),
    "M_DarkMetal": (0.18, 0.20, 0.22, 1.0),
    "M_DarkRed": (0.48, 0.10, 0.10, 1.0),
    "M_FabricBeige": (0.78, 0.73, 0.63, 1.0),
    "M_FabricBlue": (0.25, 0.38, 0.54, 1.0),
    "M_GlassCyan": (0.55, 0.78, 0.86, 0.45),
    "M_GlowGreen": (0.20, 0.95, 0.45, 1.0),
    "M_GlowWarm": (1.00, 0.90, 0.68, 1.0),
    "M_Green": (0.18, 0.56, 0.28, 1.0),
    "M_NavyBlue": (0.10, 0.16, 0.28, 1.0),
    "M_NeonGreen": (0.35, 0.92, 0.25, 1.0),
    "M_OffWhite": (0.88, 0.87, 0.83, 1.0),
    "M_OliveGreen": (0.34, 0.40, 0.26, 1.0),
    "M_Orange": (0.88, 0.46, 0.14, 1.0),
    "M_Red": (0.82, 0.14, 0.12, 1.0),
    "M_RubberBlack": (0.09, 0.09, 0.10, 1.0),
    "M_RugBurgundy": (0.45, 0.14, 0.18, 1.0),
    "M_RugGold": (0.76, 0.60, 0.28, 1.0),
    "M_SafetyOrange": (0.94, 0.42, 0.10, 1.0),
    "M_ScreenBlack": (0.04, 0.05, 0.06, 1.0),
    "M_SilverMetal": (0.72, 0.75, 0.78, 1.0),
    "M_WaterBlue": (0.18, 0.52, 0.78, 1.0),
    "M_White": (0.94, 0.95, 0.96, 1.0),
    "M_Yellow": (0.95, 0.78, 0.14, 1.0),
}


def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0


def get_mat(name):
    if name in bpy.data.materials:
        return bpy.data.materials[name]
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        col = MATERIAL_COLORS.get(name, (0.8, 0.8, 0.8, 1.0))
        bsdf.inputs["Base Color"].default_value = col
        if "Metal" in name or name in ("M_Brass", "M_Copper"):
            bsdf.inputs["Metallic"].default_value = 0.8
            bsdf.inputs["Roughness"].default_value = 0.28
        elif "Rubber" in name:
            bsdf.inputs["Roughness"].default_value = 0.82
        elif "Glass" in name:
            bsdf.inputs["Roughness"].default_value = 0.1
    return mat


class MeshGroupBuilder:
    """
    Builds a single multi-material Blender Mesh Object with a specified pivot in Unity coordinates!
    Note on Coordinate Conversion:
    We author coordinates in Unity space (X=right, Y=up, Z=forward) and convert them into
    Blender space so that when exported with standard FBX settings (-Z forward, Y up, bake_space_transform=True),
    they land at the exact Unity coordinates!
    Wait: with axis_forward='-Z', axis_up='Y', bake_space_transform=True:
    Blender (x, y, z) -> FBX -> Unity:
    Let's make the mapping explicit:
    In Blender, if we export with axis_forward='Z', axis_up='Y', bake_space_transform=False,
    or better yet, we build directly in Blender's native coordinates (X=right, Y=-Z_unity, Z=Y_unity)
    where Unity (ux, uy, uz) <-> Blender (bx = ux, by = -uz, bz = uy)!
    Let's verify: in SM_Refrigerator.fbx:
    Refrigerator height=1.76 is along Blender Z (bz = uy).
    Refrigerator front (Unity -Z) is along Blender -Y (by = -0.317 for front door)!
    Wait: in SM_Refrigerator.fbx, CabinetDoor_Hinge was at Blender (0.355, -0.317, 0.55).
    So in SM_Refrigerator.fbx:
    Blender Z = Unity Y (up)!
    Blender Y = Unity Z (or -Z)!
    Blender X = Unity X!
    """
    def __init__(self, name, pivot_u=(0.0, 0.0, 0.0), parent=None):
        self.name = name
        self.pivot_u = pivot_u
        self.parent = parent
        self.parts = []  # list of (bm, mat_name)

    @staticmethod
    def u2b_pos(ux, uy, uz):
        # Unity (x, y, z) -> Blender (x, -z, y)
        return (ux, -uz, uy)

    @staticmethod
    def u2b_size(sx, sy, sz):
        return (sx, sz, sy)

    def add_box(self, center_u, size_u, mat_name, bevel=0.008, rot_u=(0.0, 0.0, 0.0)):
        bm = bmesh.new()
        bmesh.ops.create_cube(bm, size=1.0)
        sx, sy, sz = self.u2b_size(*size_u)
        bmesh.ops.scale(bm, vec=(sx, sy, sz), verts=bm.verts)
        if bevel > 0.0005:
            bmesh.ops.bevel(
                bm,
                geom=bm.edges[:],
                offset=min(bevel, min(sx, sy, sz) * 0.28),
                segments=2,
                profile=0.5,
                affect='EDGES'
            )
        if any(abs(a) > 1e-4 for a in rot_u):
            rx, ry, rz = rot_u
            # Unity rot (rx around X, ry around Y-up, rz around Z-forward)
            # In Blender (X=X, Y=-Z, Z=Y):
            eul = mathutils_euler(rx, -rz, ry)
            bmesh.ops.rotate(bm, cent=(0, 0, 0), matrix=eul.to_matrix(), verts=bm.verts)
        px, py, pz = self.u2b_pos(
            center_u[0] - self.pivot_u[0],
            center_u[1] - self.pivot_u[1],
            center_u[2] - self.pivot_u[2],
        )
        bmesh.ops.translate(bm, vec=(px, py, pz), verts=bm.verts)
        self.parts.append((bm, mat_name))

    def add_cylinder(self, center_u, radius, length, mat_name, axis='Y', segments=16, r_top=None):
        """
        axis in Unity space: 'Y' (vertical), 'X' (lateral), 'Z' (longitudinal)
        """
        bm = bmesh.new()
        r2 = radius if r_top is None else r_top
        bmesh.ops.create_cone(
            bm,
            cap_ends=True,
            cap_tris=False,
            segments=segments,
            radius1=radius,
            radius2=r2,
            depth=length
        )
        # In Blender, create_cone is aligned along Blender Z (which is Unity Y!).
        if axis == 'X':
            # Rotate Blender Z to Blender X (90 deg around Y)
            rot = mathutils_Matrix.Rotation(math.pi * 0.5, 3, 'Y')
            bmesh.ops.rotate(bm, cent=(0, 0, 0), matrix=rot, verts=bm.verts)
        elif axis == 'Z':
            # Rotate Blender Z to Blender -Y (90 deg around X)
            rot = mathutils_Matrix.Rotation(math.pi * 0.5, 3, 'X')
            bmesh.ops.rotate(bm, cent=(0, 0, 0), matrix=rot, verts=bm.verts)
        px, py, pz = self.u2b_pos(
            center_u[0] - self.pivot_u[0],
            center_u[1] - self.pivot_u[1],
            center_u[2] - self.pivot_u[2],
        )
        bmesh.ops.translate(bm, vec=(px, py, pz), verts=bm.verts)
        self.parts.append((bm, mat_name))

    def build(self):
        mesh = bpy.data.meshes.new(self.name + "_Mesh")
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.collection.objects.link(obj)

        if self.parent is not None:
            obj.parent = self.parent
            p_piv = getattr(self.parent, "_pivot_u", (0.0, 0.0, 0.0))
            rel_u = (
                self.pivot_u[0] - p_piv[0],
                self.pivot_u[1] - p_piv[1],
                self.pivot_u[2] - p_piv[2],
            )
            obj.location = self.u2b_pos(*rel_u)
        else:
            obj.location = self.u2b_pos(*self.pivot_u)

        obj["_pivot_u"] = self.pivot_u

        combined = bmesh.new()
        mat_Index_map = {}
        for bm, mat_name in self.parts:
            if mat_name not in mat_Index_map:
                mat = get_mat(mat_name)
                obj.data.materials.append(mat)
                mat_Index_map[mat_name] = len(obj.data.materials) - 1
            m_idx = mat_Index_map[mat_name]
            bmesh.ops.triangulate(bm, faces=bm.faces[:])
            for f in bm.faces:
                f.material_index = m_idx
            me_tmp = bpy.data.meshes.new("_tmp")
            bm.to_mesh(me_tmp)
            bm.free()
            combined.from_mesh(me_tmp)
            bpy.data.meshes.remove(me_tmp)

        # Smart UV projection / box mapping on combined bmesh
        uv_layer = combined.loops.layers.uv.verify()
        for f in combined.faces:
            n = f.normal
            ax, ay, az = abs(n.x), abs(n.y), abs(n.z)
            for loop in f.loops:
                co = loop.vert.co
                if az >= ax and az >= ay:
                    loop[uv_layer].uv = (co.x * 0.5 + 0.5, co.y * 0.5 + 0.5)
                elif ax >= ay:
                    loop[uv_layer].uv = (co.y * 0.5 + 0.5, co.z * 0.5 + 0.5)
                else:
                    loop[uv_layer].uv = (co.x * 0.5 + 0.5, co.z * 0.5 + 0.5)

        combined.to_mesh(mesh)
        combined.free()
        mesh.update()
        return obj


import mathutils
mathutils_Matrix = mathutils.Matrix


def mathutils_euler(rx_deg, ry_deg, rz_deg):
    return mathutils.Euler(
        (math.radians(rx_deg), math.radians(ry_deg), math.radians(rz_deg)),
        'XYZ'
    )


def add_wheel_assembly(parent_obj, name, pos_u, radius, width, hub_mat="M_SilverMetal"):
    wb = MeshGroupBuilder(name, pivot_u=pos_u, parent=parent_obj)
    # Outer rubber tire
    wb.add_cylinder(pos_u, radius, width, "M_RubberBlack", axis='X', segments=20)
    # Sidewall tread ring
    wb.add_cylinder(pos_u, radius * 0.82, width + 0.008, "M_DarkMetal", axis='X', segments=16)
    # Alloy rim / hub
    wb.add_cylinder(pos_u, radius * 0.56, width + 0.016, hub_mat, axis='X', segments=14)
    # Center lug cap
    wb.add_cylinder(pos_u, radius * 0.22, width + 0.026, "M_DarkMetal", axis='X', segments=10)
    # 5 lug bolts
    for i in range(5):
        ang = i * (2 * math.pi / 5.0)
        ly = pos_u[1] + math.cos(ang) * (radius * 0.36)
        lz = pos_u[2] + math.sin(ang) * (radius * 0.36)
        wb.add_cylinder((pos_u[0], ly, lz), 0.016, width + 0.028, hub_mat, axis='X', segments=6)
    return wb.build()


def write_fbx_and_meta(asset_name):
    fbx_path = os.path.join(OUT_DIR, f"{asset_name}.fbx")
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_NONE',
        bake_space_transform=False,
        axis_forward='-Y',
        axis_up='Z',
        use_mesh_modifiers=True,
        mesh_smooth_type='FACE',
        add_leaf_bones=False,
        path_mode='AUTO'
    )
    meta_path = fbx_path + ".meta"
    if not os.path.exists(meta_path):
        guid = hashlib.md5(f"CuratedYarkin_V5_{asset_name}".encode("utf-8")).hexdigest()
        ext_lines = []
        for mat_name, mat_guid in sorted(MATERIAL_GUIDS.items()):
            ext_lines.append(f"""  - first:
      type: UnityEngine:Material
      assembly: UnityEngine.CoreModule
      name: {mat_name}
    second: {{fileID: 2100000, guid: {mat_guid}, type: 2}}""")
        ext_block = "\n".join(ext_lines)
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
ModelImporter:
  serializedVersion: 21300
  internalIDToNameTable: []
  externalObjects:
{ext_block}
  materials:
    materialImportMode: 2
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    removeConstantScaleCurves: 0
    motionNodeName: 
    rigImportErrors: 
    rigImportWarnings: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 1
    importVisibility: 1
    importBlendShapes: 1
    importCameras: 1
    importLights: 1
    nodeNameCollisionStrategy: 1
    fileIdsGeneration: 2
    swapUVChannels: 0
    generateSecondaryUV: 0
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    bakeAxisConversion: 0
    preserveHierarchy: 1
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    optimizeBones: 1
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVMarginMethod: 1
    secondaryUVMinLightmapResolution: 40
    secondaryUVMinObjectScale: 1
    secondaryUVPackMargin: 4
    useFileScale: 1
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 1
    normalSmoothingSource: 0
  referencedClips: []
  importAnimation: 1
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legTwist: 0.5
    armStretch: 0.05
    legStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {{instanceID: 0}}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 0
  humanoidOversampling: 1
  avatarSetup: 0
  addHumanoidExtraRootOnlyWhenUsingAvatar: 1
  remapMaterialsIfMaterialImportModeIsNone: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(meta_content)
    print(f"Exported {fbx_path}")


# ==============================================================================
# 1. AMBULANCE (SM_Ambulance.fbx) - 5.55m L, 2.02m W, 2.55m H (+Z front)
# ==============================================================================
def build_ambulance():
    reset_scene()
    body = MeshGroupBuilder("Ambulance_Body", pivot_u=(0, 0, 0))
    # Undercarriage & bumpers
    body.add_box((0, 0.30, 0), (1.92, 0.24, 5.42), "M_DarkMetal", 0.02)
    body.add_box((0, 0.32, 2.72), (2.02, 0.26, 0.18), "M_DarkMetal", 0.03)
    body.add_box((0, 0.30, -2.72), (2.04, 0.24, 0.18), "M_DarkMetal", 0.03)
    body.add_box((0, 0.22, -2.81), (1.42, 0.08, 0.18), "M_SilverMetal", 0.015)  # Rear step
    # Rear Medical Box Module (hollow rear opening for doors & interior)
    body.add_box((0, 0.44, -0.58), (2.02, 0.08, 3.68), "M_OffWhite", 0.01)  # Floor
    body.add_box((0, 2.47, -0.58), (2.02, 0.10, 3.68), "M_White", 0.02)     # Roof
    body.add_box((0, 1.46, 1.22), (2.02, 2.04, 0.08), "M_White", 0.015)     # Bulkhead wall
    for s in (-1, 1):
        body.add_box((s * 0.97, 1.46, -0.58), (0.08, 2.04, 3.68), "M_White", 0.02)  # Side walls
        # Red emergency side stripe & high-vis accent
        body.add_box((s * 1.015, 1.08, -0.45), (0.018, 0.24, 3.55), "M_Red", 0.004)
        body.add_box((s * 1.016, 1.25, -0.45), (0.016, 0.06, 3.55), "M_SafetyOrange", 0.002)
        # Side tinted clinical window
        body.add_box((s * 1.015, 1.72, -0.15), (0.02, 0.52, 1.25), "M_GlassCyan", 0.005)
        body.add_box((s * 1.015, 1.72, -0.15), (0.024, 0.14, 1.25), "M_White", 0.003)  # Frosted band
        # Red Cross / Star of Life emblem on rear side
        body.add_box((s * 1.018, 1.75, -1.55), (0.02, 0.46, 0.14), "M_Red", 0.004)
        body.add_box((s * 1.018, 1.75, -1.55), (0.02, 0.14, 0.46), "M_Red", 0.004)
        # Upper corner marker lights
        body.add_box((s * 0.96, 2.38, -2.41), (0.14, 0.08, 0.04), "M_Red", 0.005)
        body.add_box((s * 1.015, 2.38, -2.10), (0.02, 0.08, 0.22), s < 0 and "M_Blue" or "M_Red", 0.005)
    # Cab & Hood (+Z)
    body.add_box((0, 0.96, 1.75), (1.92, 1.08, 1.15), "M_White", 0.03)
    body.add_box((0, 1.75, 1.65), (1.84, 0.72, 1.05), "M_White", 0.03)
    body.add_box((0, 2.24, 1.50), (1.88, 0.36, 0.85), "M_White", 0.03)  # Aerodynamic cab over-brow
    body.add_box((0, 0.82, 2.48), (1.86, 0.68, 0.56), "M_White", 0.03)  # Nose/Bonnet
    body.add_box((0, 1.72, 2.18), (1.68, 0.58, 0.06), "M_GlassCyan", 0.01, rot_u=(18, 0, 0))  # Windshield
    body.add_box((0, 0.72, 2.765), (1.18, 0.38, 0.04), "M_DarkMetal", 0.008)  # Grille
    for i in range(4):
        body.add_box((0, 0.58 + i * 0.085, 2.78), (1.10, 0.022, 0.02), "M_SilverMetal", 0.003)
    # Hood cross emblem
    body.add_box((0, 1.165, 2.45), (0.36, 0.015, 0.11), "M_Red", 0.002)
    body.add_box((0, 1.165, 2.45), (0.11, 0.015, 0.36), "M_Red", 0.002)
    # Interior Medical Equipment (Stretcher, Cabinets, Oxygen Tank)
    body.add_box((-0.68, 1.22, -0.55), (0.42, 1.38, 2.85), "M_OffWhite", 0.01)  # Left medical wall cabinet
    body.add_box((-0.46, 1.35, -0.55), (0.03, 0.55, 2.40), "M_GlassCyan", 0.004)
    body.add_cylinder((-0.65, 1.05, 0.75), 0.11, 0.72, "M_Green", axis='Y', segments=12)  # Oxygen tank
    body.add_box((0.18, 0.72, -0.65), (0.64, 0.08, 1.92), "M_SilverMetal", 0.01)  # Stretcher frame
    body.add_box((0.18, 0.82, -0.65), (0.58, 0.12, 1.84), "M_SafetyOrange", 0.025)  # Stretcher mattress
    for sx in (-0.08, 0.44):
        for sz in (-1.40, 0.10):
            body.add_box((sx, 0.56, sz), (0.04, 0.26, 0.04), "M_SilverMetal", 0.005)
    body.add_box((0.68, 0.72, -0.65), (0.40, 0.44, 2.40), "M_FabricBlue", 0.015)  # Squad bench seat
    root_obj = body.build()

    # Interior Ceiling Lamp Lens (for Ambulance InteriorLight)
    ilamp = MeshGroupBuilder("Interior_CeilingLamp_Lens", pivot_u=(0, 2.41, -0.60), parent=root_obj)
    ilamp.add_box((0, 2.41, -0.60), (0.36, 0.04, 1.65), "M_GlowWarm", 0.008)
    ilamp.build()

    # Roof Lightbar & Beacons
    lb = MeshGroupBuilder("Lightbar_Frame", pivot_u=(0, 2.56, 1.55), parent=root_obj)
    lb.add_box((0, 2.56, 1.55), (1.46, 0.07, 0.28), "M_SilverMetal", 0.01)
    lb.build()
    b_blue = MeshGroupBuilder("Beacon_Blue_Lens", pivot_u=(-0.44, 2.61, 1.55), parent=root_obj)
    b_blue.add_box((-0.44, 2.61, 1.55), (0.52, 0.10, 0.24), "M_Blue", 0.018)
    b_blue.build()
    b_red = MeshGroupBuilder("Beacon_Red_Lens", pivot_u=(0.44, 2.61, 1.55), parent=root_obj)
    b_red.add_box((0.44, 2.61, 1.55), (0.52, 0.10, 0.24), "M_Red", 0.018)
    b_red.build()

    # Headlights & Taillights
    for side, lbl in ((-1, "L"), (1, "R")):
        hl = MeshGroupBuilder(f"Headlight_{lbl}_Lens", pivot_u=(side * 0.72, 0.78, 2.77), parent=root_obj)
        hl.add_box((side * 0.72, 0.78, 2.77), (0.26, 0.16, 0.04), "M_GlowWarm", 0.008)
        hl.build()
        tl = MeshGroupBuilder(f"Taillight_{lbl}_Lens", pivot_u=(side * 0.90, 0.88, -2.42), parent=root_obj)
        tl.add_box((side * 0.90, 0.88, -2.42), (0.14, 0.34, 0.04), "M_Red", 0.008)
        tl.build()

    # Steering Wheel
    sw = MeshGroupBuilder("SteeringWheel_Pivot", pivot_u=(-0.45, 1.24, 1.92), parent=root_obj)
    sw.add_cylinder((-0.45, 1.24, 1.92), 0.18, 0.04, "M_RubberBlack", axis='Z', segments=16)
    sw.add_box((-0.45, 1.24, 1.92), (0.30, 0.04, 0.03), "M_DarkMetal", 0.005)
    sw.build()

    # Cab Front Doors
    for side, lbl in ((-1, "Door_FL_Hinge"), (1, "Door_FR_Hinge")):
        px = side * 0.96
        d = MeshGroupBuilder(lbl, pivot_u=(px, 1.15, 2.08), parent=root_obj)
        d.add_box((px, 1.02, 1.72), (0.05, 0.96, 0.74), "M_White", 0.012)
        d.add_box((px + side * 0.01, 1.08, 1.72), (0.04, 0.22, 0.72), "M_Red", 0.005)
        d.add_box((px, 1.68, 1.72), (0.03, 0.44, 0.64), "M_GlassCyan", 0.005)
        d.add_box((px + side * 0.09, 1.52, 2.00), (0.14, 0.20, 0.08), "M_DarkMetal", 0.01)  # Side mirror
        d.add_box((px + side * 0.035, 1.12, 1.48), (0.03, 0.04, 0.14), "M_SilverMetal", 0.005)  # Handle
        d.build()

    # Rear Ambulance Double Doors (at -Z = -2.42)
    for side, lbl in ((-1, "Door_RL_Hinge"), (1, "Door_RR_Hinge")):
        px = side * 0.88
        d = MeshGroupBuilder(lbl, pivot_u=(px, 1.38, -2.42), parent=root_obj)
        cx = side * 0.44
        d.add_box((cx, 1.38, -2.42), (0.86, 1.78, 0.06), "M_White", 0.014)
        d.add_box((cx, 1.08, -2.435), (0.84, 0.24, 0.04), "M_Red", 0.005)
        d.add_box((cx, 1.76, -2.42), (0.58, 0.56, 0.07), "M_GlassCyan", 0.006)
        d.add_box((side * 0.08, 1.24, -2.46), (0.04, 0.18, 0.05), "M_SilverMetal", 0.006)
        d.build()

    # 4 Wheels
    for side, sl in ((-1, "L"), (1, "R")):
        add_wheel_assembly(root_obj, f"Wheel_F{sl}_Pivot", (side * 0.95, 0.42, 1.75), 0.42, 0.26)
        add_wheel_assembly(root_obj, f"Wheel_R{sl}_Pivot", (side * 0.95, 0.42, -1.45), 0.42, 0.26)

    write_fbx_and_meta("SM_Ambulance")


# ==============================================================================
# 2. POLICE CAR (SM_PoliceCar.fbx) - 4.52m L, 1.86m W, 1.60m H (+Z front)
# ==============================================================================
def build_police_car():
    reset_scene()
    body = MeshGroupBuilder("PoliceCar_Body", pivot_u=(0, 0, 0))
    # Lower chassis & navy fenders
    body.add_box((0, 0.28, 0), (1.78, 0.20, 4.38), "M_DarkMetal", 0.02)
    body.add_box((0, 0.62, 0), (1.86, 0.54, 4.44), "M_NavyBlue", 0.035)
    # White center belt & roof cabin
    body.add_box((0, 0.66, 0.02), (1.87, 0.36, 2.25), "M_White", 0.02)
    body.add_box((0, 1.18, -0.10), (1.60, 0.56, 2.15), "M_White", 0.04)
    body.add_box((0, 1.44, -0.10), (1.52, 0.08, 1.95), "M_White", 0.02)
    # Windshield & rear glass
    body.add_box((0, 1.16, 0.98), (1.46, 0.48, 0.06), "M_GlassCyan", 0.01, rot_u=(28, 0, 0))
    body.add_box((0, 1.16, -1.18), (1.44, 0.46, 0.06), "M_GlassCyan", 0.01, rot_u=(-26, 0, 0))
    # Side stripes & brass shields
    for s in (-1, 1):
        body.add_box((s * 0.938, 0.68, 0), (0.018, 0.14, 3.85), "M_Blue", 0.004)
        body.add_box((s * 0.942, 0.72, 0.35), (0.02, 0.18, 0.18), "M_Brass", 0.005)
        body.add_box((s * 0.79, 1.16, -0.58), (0.04, 0.42, 0.72), "M_GlassCyan", 0.005)
    # Front Push Bullbar & Grille (+Z)
    body.add_box((0, 0.34, 2.24), (1.84, 0.24, 0.16), "M_DarkMetal", 0.025)
    body.add_box((0, 0.34, -2.24), (1.84, 0.24, 0.16), "M_DarkMetal", 0.025)
    for s in (-1, 1):
        body.add_box((s * 0.38, 0.56, 2.32), (0.07, 0.54, 0.10), "M_DarkMetal", 0.012)
    body.add_box((0, 0.62, 2.34), (0.86, 0.06, 0.08), "M_DarkMetal", 0.01)
    body.add_box((0, 0.44, 2.34), (0.86, 0.06, 0.08), "M_DarkMetal", 0.01)
    body.add_box((0, 0.58, 2.23), (0.96, 0.26, 0.04), "M_SilverMetal", 0.008)
    # Hood & trunk contours
    body.add_box((0, 0.88, 1.55), (1.68, 0.08, 1.25), "M_NavyBlue", 0.02)
    body.add_box((0, 0.90, -1.68), (1.66, 0.08, 0.95), "M_NavyBlue", 0.02)
    # Interior seats & partition
    body.add_box((0, 0.78, 0.15), (1.36, 0.54, 0.52), "M_DarkMetal", 0.02)
    body.add_box((0, 1.05, -0.22), (1.42, 0.68, 0.04), "M_SilverMetal", 0.008)
    root_obj = body.build()

    # Lightbar & Beacons
    lb = MeshGroupBuilder("Lightbar_Frame", pivot_u=(0, 1.50, -0.05), parent=root_obj)
    lb.add_box((0, 1.50, -0.05), (1.24, 0.06, 0.26), "M_SilverMetal", 0.008)
    lb.build()
    b_blue = MeshGroupBuilder("Beacon_Blue_Lens", pivot_u=(-0.36, 1.55, -0.05), parent=root_obj)
    b_blue.add_box((-0.36, 1.55, -0.05), (0.46, 0.09, 0.22), "M_Blue", 0.015)
    b_blue.build()
    b_red = MeshGroupBuilder("Beacon_Red_Lens", pivot_u=(0.36, 1.55, -0.05), parent=root_obj)
    b_red.add_box((0.36, 1.55, -0.05), (0.46, 0.09, 0.22), "M_Red", 0.015)
    b_red.build()

    # Headlights & Taillights
    for side, lbl in ((-1, "L"), (1, "R")):
        hl = MeshGroupBuilder(f"Headlight_{lbl}_Lens", pivot_u=(side * 0.66, 0.64, 2.22), parent=root_obj)
        hl.add_box((side * 0.66, 0.64, 2.22), (0.28, 0.14, 0.05), "M_GlowWarm", 0.008)
        hl.build()
        tl = MeshGroupBuilder(f"Taillight_{lbl}_Lens", pivot_u=(side * 0.66, 0.68, -2.22), parent=root_obj)
        tl.add_box((side * 0.66, 0.68, -2.22), (0.30, 0.15, 0.05), "M_Red", 0.008)
        tl.build()

    # Steering Wheel
    sw = MeshGroupBuilder("SteeringWheel_Pivot", pivot_u=(-0.38, 0.92, 0.56), parent=root_obj)
    sw.add_cylinder((-0.38, 0.92, 0.56), 0.16, 0.035, "M_RubberBlack", axis='Z', segments=16)
    sw.build()

    # Front Doors
    for side, lbl in ((-1, "Door_FL_Hinge"), (1, "Door_FR_Hinge")):
        px = side * 0.91
        d = MeshGroupBuilder(lbl, pivot_u=(px, 0.78, 0.68), parent=root_obj)
        d.add_box((px, 0.66, 0.24), (0.05, 0.54, 0.88), "M_White", 0.012)
        d.add_box((px + side * 0.01, 0.68, 0.24), (0.04, 0.14, 0.86), "M_Blue", 0.004)
        d.add_box((px, 1.14, 0.22), (0.03, 0.42, 0.76), "M_GlassCyan", 0.005)
        d.add_box((px + side * 0.08, 0.98, 0.58), (0.12, 0.10, 0.07), "M_DarkMetal", 0.008)
        d.add_box((px + side * 0.03, 0.78, -0.08), (0.03, 0.04, 0.14), "M_SilverMetal", 0.005)
        d.build()

    # 4 Wheels
    for side, sl in ((-1, "L"), (1, "R")):
        add_wheel_assembly(root_obj, f"Wheel_F{sl}_Pivot", (side * 0.86, 0.36, 1.38), 0.36, 0.24)
        add_wheel_assembly(root_obj, f"Wheel_R{sl}_Pivot", (side * 0.86, 0.36, -1.35), 0.36, 0.24)

    write_fbx_and_meta("SM_PoliceCar")


# ==============================================================================
# 3. FIRE ENGINE (SM_FireEngine.fbx) - 6.30m L, 2.16m W, 2.35m H (+Z front)
# ==============================================================================
def build_fire_engine():
    reset_scene()
    body = MeshGroupBuilder("FireEngine_Body", pivot_u=(0, 0, 0))
    # Heavy chassis & chrome bumper
    body.add_box((0, 0.34, 0), (2.04, 0.26, 6.16), "M_DarkMetal", 0.025)
    body.add_box((0, 0.36, 3.08), (2.16, 0.28, 0.22), "M_SilverMetal", 0.025)
    body.add_box((0, 0.34, -3.08), (2.16, 0.26, 0.20), "M_DarkMetal", 0.025)
    # Crew Cab (+Z)
    body.add_box((0, 1.32, 1.88), (2.12, 1.72, 2.12), "M_Red", 0.035)
    body.add_box((0, 2.22, 1.88), (2.06, 0.12, 2.04), "M_White", 0.02)
    body.add_box((0, 1.66, 2.95), (1.88, 0.64, 0.06), "M_GlassCyan", 0.01)  # Split windshield
    body.add_box((0, 1.66, 2.96), (0.05, 0.64, 0.07), "M_DarkMetal", 0.005)
    body.add_box((0, 0.88, 2.95), (1.36, 0.56, 0.05), "M_SilverMetal", 0.01)  # Front grille
    for i in range(5):
        body.add_box((0, 0.68 + i * 0.09, 2.97), (1.26, 0.025, 0.03), "M_DarkMetal", 0.003)
    # Rear Apparatus Body & 6 Roller Shutters
    body.add_box((0, 1.28, -1.12), (2.16, 1.66, 3.84), "M_Red", 0.03)
    body.add_box((0, 2.13, -1.12), (2.12, 0.06, 3.80), "M_SilverMetal", 0.01)  # Diamond-plate deck
    for s in (-1, 1):
        # Reflective yellow hazard stripe
        body.add_box((s * 1.085, 0.72, 0.0), (0.018, 0.14, 5.85), "M_Yellow", 0.004)
        # 3 Aluminum Roll-up Equipment Bay Shutters per side
        for bay in range(3):
            bz = 0.15 - bay * 1.22
            body.add_box((s * 1.084, 1.38, bz), (0.024, 1.02, 0.98), "M_SilverMetal", 0.008)
            for slat in range(7):
                body.add_box((s * 1.096, 0.94 + slat * 0.14, bz), (0.012, 0.02, 0.94), "M_DarkMetal", 0.002)
            body.add_box((s * 1.098, 0.89, bz), (0.02, 0.035, 0.42), "M_Brass", 0.004)
        # Suction hose tubes along upper side
        body.add_cylinder((s * 1.04, 2.02, -1.12), 0.075, 3.55, "M_DarkRed", axis='Z', segments=12)
    # Rear Pump Panel (-Z = -3.05) with Gauges & Brass Couplings
    body.add_box((0, 1.26, -3.05), (1.42, 1.18, 0.05), "M_DarkMetal", 0.01)
    for g in range(3):
        body.add_cylinder((-0.36 + g * 0.36, 1.56, -3.08), 0.09, 0.04, "M_White", axis='Z', segments=14)
        body.add_cylinder((-0.36 + g * 0.36, 1.56, -3.09), 0.03, 0.05, "M_Red", axis='Z', segments=10)
        body.add_cylinder((-0.36 + g * 0.36, 1.04, -3.10), 0.085, 0.12, "M_Brass", axis='Z', segments=12)
    root_obj = body.build()

    # Articulated 2-Stage Telescopic Turntable Ladder on Roof!
    # Stage 1: Base Turntable & Outer Ladder Rails
    lad_base = MeshGroupBuilder("Ladder_Base_Pivot", pivot_u=(0, 2.18, -2.25), parent=root_obj)
    lad_base.add_cylinder((0, 2.22, -2.25), 0.44, 0.12, "M_DarkMetal", axis='Y', segments=18)
    for s in (-1, 1):
        lad_base.add_box((s * 0.28, 2.34, -0.45), (0.06, 0.14, 3.85), "M_SilverMetal", 0.008)
    for r in range(11):
        lad_base.add_cylinder((0, 2.31, -2.20 + r * 0.35), 0.022, 0.54, "M_SilverMetal", axis='X', segments=10)
    lad_base_obj = lad_base.build()

    # Stage 2: Telescopic Inner Extension Ladder (Child of Ladder_Base_Pivot)
    lad_ext = MeshGroupBuilder("Ladder_Extend_Pivot", pivot_u=(0, 2.36, -0.45), parent=lad_base_obj)
    for s in (-1, 1):
        lad_ext.add_box((s * 0.21, 2.37, 0.15), (0.045, 0.10, 3.20), "M_SilverMetal", 0.006)
        lad_ext.add_box((s * 0.21, 2.37, 1.65), (0.055, 0.12, 0.28), "M_SafetyOrange", 0.008)
    for r in range(9):
        lad_ext.add_cylinder((0, 2.36, -1.25 + r * 0.35), 0.018, 0.40, "M_SilverMetal", axis='X', segments=10)
    lad_ext.build()

    # Roof Water Monitor Cannon
    mon = MeshGroupBuilder("RoofMonitor_Pivot", pivot_u=(0, 2.28, 1.05), parent=root_obj)
    mon.add_cylinder((0, 2.34, 1.05), 0.10, 0.18, "M_Brass", axis='Y', segments=12)
    mon.add_cylinder((0, 2.44, 1.24), 0.055, 0.42, "M_SilverMetal", axis='Z', segments=12)
    mon.add_cylinder((0, 2.44, 1.48), 0.075, 0.10, "M_Red", axis='Z', segments=12)
    mon.build()

    # Lightbar & Beacons
    lb = MeshGroupBuilder("Lightbar_Frame", pivot_u=(0, 2.31, 2.45), parent=root_obj)
    lb.add_box((0, 2.31, 2.45), (1.64, 0.07, 0.28), "M_SilverMetal", 0.01)
    lb.build()
    b_red_l = MeshGroupBuilder("Beacon_Red_L_Lens", pivot_u=(-0.52, 2.37, 2.45), parent=root_obj)
    b_red_l.add_box((-0.52, 2.37, 2.45), (0.54, 0.11, 0.24), "M_Red", 0.018)
    b_red_l.build()
    b_red_r = MeshGroupBuilder("Beacon_Red_R_Lens", pivot_u=(0.52, 2.37, 2.45), parent=root_obj)
    b_red_r.add_box((0.52, 2.37, 2.45), (0.54, 0.11, 0.24), "M_Red", 0.018)
    b_red_r.build()

    # Headlights & Taillights
    for side, lbl in ((-1, "L"), (1, "R")):
        hl = MeshGroupBuilder(f"Headlight_{lbl}_Lens", pivot_u=(side * 0.82, 0.78, 2.96), parent=root_obj)
        hl.add_box((side * 0.82, 0.78, 2.96), (0.28, 0.18, 0.05), "M_GlowWarm", 0.008)
        hl.build()
        tl = MeshGroupBuilder(f"Taillight_{lbl}_Lens", pivot_u=(side * 0.88, 0.72, -3.05), parent=root_obj)
        tl.add_box((side * 0.88, 0.72, -3.05), (0.22, 0.24, 0.05), "M_Red", 0.008)
        tl.build()

    # Steering Wheel
    sw = MeshGroupBuilder("SteeringWheel_Pivot", pivot_u=(-0.52, 1.36, 2.55), parent=root_obj)
    sw.add_cylinder((-0.52, 1.36, 2.55), 0.20, 0.04, "M_RubberBlack", axis='Z', segments=16)
    sw.build()

    # Front Cab Doors
    for side, lbl in ((-1, "Door_FL_Hinge"), (1, "Door_FR_Hinge")):
        px = side * 1.06
        d = MeshGroupBuilder(lbl, pivot_u=(px, 1.28, 2.75), parent=root_obj)
        d.add_box((px, 1.24, 2.32), (0.05, 1.22, 0.84), "M_Red", 0.012)
        d.add_box((px + side * 0.01, 0.72, 2.32), (0.04, 0.14, 0.82), "M_Yellow", 0.004)
        d.add_box((px, 1.66, 2.32), (0.03, 0.48, 0.72), "M_GlassCyan", 0.005)
        d.add_box((px + side * 0.09, 1.58, 2.68), (0.14, 0.24, 0.08), "M_SilverMetal", 0.01)
        d.build()

    # 4 Heavy Truck Wheels
    for side, sl in ((-1, "L"), (1, "R")):
        add_wheel_assembly(root_obj, f"Wheel_F{sl}_Pivot", (side * 1.02, 0.48, 1.95), 0.48, 0.30)
        add_wheel_assembly(root_obj, f"Wheel_R{sl}_Pivot", (side * 1.02, 0.48, -1.75), 0.48, 0.30)

    write_fbx_and_meta("SM_FireEngine")


# ==============================================================================
# 4. SEARCH & RESCUE TRUCK (SM_RescueTruck.fbx) - 5.50m L, 2.02m W, 2.20m H (+Z front)
# ==============================================================================
def build_rescue_truck():
    reset_scene()
    body = MeshGroupBuilder("RescueTruck_Body", pivot_u=(0, 0, 0))
    # High-clearance 4x4 chassis & rugged bumpers
    body.add_box((0, 0.36, 0), (1.92, 0.26, 5.36), "M_DarkMetal", 0.025)
    body.add_box((0, 0.40, 2.68), (2.02, 0.28, 0.24), "M_DarkMetal", 0.03)
    body.add_box((0, 0.38, -2.68), (2.02, 0.26, 0.22), "M_DarkMetal", 0.03)
    # Crew Cab & Hood (+Z)
    body.add_box((0, 1.04, 1.85), (1.94, 0.96, 1.45), "M_SafetyOrange", 0.03)
    body.add_box((0, 1.62, 1.15), (1.88, 0.88, 1.65), "M_SafetyOrange", 0.035)
    body.add_box((0, 1.64, 1.98), (1.72, 0.56, 0.06), "M_GlassCyan", 0.01, rot_u=(18, 0, 0))
    body.add_box((0, 0.84, 2.58), (1.34, 0.46, 0.05), "M_DarkMetal", 0.01)
    # Snorkel intake on right A-pillar
    body.add_cylinder((0.98, 1.48, 1.85), 0.045, 0.95, "M_RubberBlack", axis='Y', segments=10)
    # Rear Utility Box & Equipment Lockers
    body.add_box((0, 1.24, -1.05), (2.02, 1.48, 3.05), "M_SafetyOrange", 0.03)
    body.add_box((0, 2.02, -1.05), (1.94, 0.10, 2.95), "M_DarkMetal", 0.015)  # Roof rack frame
    for s in (-1, 1):
        # White high-vis stripe + DarkMetal fender flares
        body.add_box((s * 1.015, 1.08, -0.35), (0.018, 0.22, 4.45), "M_White", 0.004)
        for bay in range(2):
            bz = -0.35 - bay * 1.18
            body.add_box((s * 1.016, 1.36, bz), (0.025, 0.82, 0.94), "M_SilverMetal", 0.008)
            body.add_box((s * 1.028, 1.36, bz), (0.02, 0.14, 0.06), "M_DarkMetal", 0.004)
        # Roof expedition boxes & jerrycans
        body.add_box((s * 0.58, 2.14, -0.85), (0.52, 0.22, 1.65), "M_DarkMetal", 0.015)
    root_obj = body.build()

    # Articulated Front Bumper Winch Drum + Tow Hook
    winch = MeshGroupBuilder("Winch_Pivot", pivot_u=(0, 0.44, 2.82), parent=root_obj)
    winch.add_cylinder((0, 0.44, 2.82), 0.11, 0.46, "M_DarkMetal", axis='X', segments=14)
    winch.add_cylinder((0, 0.44, 2.82), 0.085, 0.32, "M_SilverMetal", axis='X', segments=14)
    winch.add_box((0, 0.38, 2.93), (0.08, 0.14, 0.08), "M_Brass", 0.01)  # Heavy rescue tow hook
    winch.build()

    # Articulated Rear Recovery Crane Boom & Floodlight Mast
    crane = MeshGroupBuilder("CraneBoom_Pivot", pivot_u=(0, 2.05, -2.15), parent=root_obj)
    crane.add_cylinder((0, 2.15, -2.15), 0.18, 0.22, "M_DarkMetal", axis='Y', segments=14)
    crane.add_box((0, 2.38, -1.55), (0.14, 0.14, 1.45), "M_Yellow", 0.01, rot_u=(-16, 0, 0))
    crane.add_cylinder((0, 2.52, -2.35), 0.04, 0.65, "M_SilverMetal", axis='Y', segments=10)  # Floodlight mast
    crane.add_box((0, 2.84, -2.35), (0.56, 0.14, 0.08), "M_DarkMetal", 0.01)
    crane.add_box((-0.16, 2.84, -2.39), (0.18, 0.10, 0.03), "M_GlowWarm", 0.005)
    crane.add_box((0.16, 2.84, -2.39), (0.18, 0.10, 0.03), "M_GlowWarm", 0.005)
    crane.build()

    # Lightbar & Beacons (Amber + Blue + Red)
    lb = MeshGroupBuilder("Lightbar_Frame", pivot_u=(0, 2.10, 1.35), parent=root_obj)
    lb.add_box((0, 2.10, 1.35), (1.44, 0.07, 0.28), "M_SilverMetal", 0.01)
    lb.build()
    b_amb = MeshGroupBuilder("Beacon_Amber_Lens", pivot_u=(-0.44, 2.16, 1.35), parent=root_obj)
    b_amb.add_box((-0.44, 2.16, 1.35), (0.48, 0.10, 0.24), "M_Yellow", 0.016)
    b_amb.build()
    b_blu = MeshGroupBuilder("Beacon_Blue_Lens", pivot_u=(0.44, 2.16, 1.35), parent=root_obj)
    b_blu.add_box((0.44, 2.16, 1.35), (0.48, 0.10, 0.24), "M_Blue", 0.016)
    b_blu.build()

    # Headlights & Taillights
    for side, lbl in ((-1, "L"), (1, "R")):
        hl = MeshGroupBuilder(f"Headlight_{lbl}_Lens", pivot_u=(side * 0.74, 0.86, 2.59), parent=root_obj)
        hl.add_box((side * 0.74, 0.86, 2.59), (0.26, 0.16, 0.05), "M_GlowWarm", 0.008)
        hl.build()
        tl = MeshGroupBuilder(f"Taillight_{lbl}_Lens", pivot_u=(side * 0.84, 0.76, -2.59), parent=root_obj)
        tl.add_box((side * 0.84, 0.76, -2.59), (0.20, 0.22, 0.05), "M_Red", 0.008)
        tl.build()

    # Steering Wheel
    sw = MeshGroupBuilder("SteeringWheel_Pivot", pivot_u=(-0.44, 1.26, 1.62), parent=root_obj)
    sw.add_cylinder((-0.44, 1.26, 1.62), 0.18, 0.04, "M_RubberBlack", axis='Z', segments=16)
    sw.build()

    # Front Cab Doors
    for side, lbl in ((-1, "Door_FL_Hinge"), (1, "Door_FR_Hinge")):
        px = side * 0.96
        d = MeshGroupBuilder(lbl, pivot_u=(px, 1.18, 1.72), parent=root_obj)
        d.add_box((px, 1.16, 1.28), (0.05, 1.04, 0.84), "M_SafetyOrange", 0.012)
        d.add_box((px + side * 0.01, 1.08, 1.28), (0.04, 0.22, 0.82), "M_White", 0.004)
        d.add_box((px, 1.62, 1.28), (0.03, 0.44, 0.72), "M_GlassCyan", 0.005)
        d.add_box((px + side * 0.09, 1.50, 1.62), (0.14, 0.22, 0.08), "M_DarkMetal", 0.01)
        d.build()

    # 4 Off-road Wheels
    for side, sl in ((-1, "L"), (1, "R")):
        add_wheel_assembly(root_obj, f"Wheel_F{sl}_Pivot", (side * 0.94, 0.45, 1.68), 0.45, 0.28)
        add_wheel_assembly(root_obj, f"Wheel_R{sl}_Pivot", (side * 0.94, 0.45, -1.52), 0.45, 0.28)

    write_fbx_and_meta("SM_RescueTruck")


# ==============================================================================
# 5. ZODIAC INFLATABLE BOAT (SM_ZodiacBoat.fbx) - 3.48m L, 1.97m W (+Z bow)
# ==============================================================================
def build_zodiac_boat():
    reset_scene()
    hull = MeshGroupBuilder("Zodiac_Hull", pivot_u=(0, 0, 0))
    # Slatted aluminum deck & rigid keel
    hull.add_box((0, 0.14, -0.08), (1.28, 0.10, 2.55), "M_DarkMetal", 0.02)
    for slat in range(7):
        hull.add_box((0, 0.195, -1.10 + slat * 0.34), (1.20, 0.016, 0.24), "M_SilverMetal", 0.004)
    # Transom board (-Z)
    hull.add_box((0, 0.34, -1.32), (1.26, 0.42, 0.10), "M_DarkMetal", 0.015)
    # Left & Right Inflatable Pontoon Tubes (M_SafetyOrange with M_RubberBlack rub-strakes)
    for s in (-1, 1):
        hull.add_cylinder((s * 0.73, 0.30, -0.10), 0.25, 2.45, "M_SafetyOrange", axis='Z', segments=18)
        # Rear pontoon cones
        hull.add_cylinder((s * 0.73, 0.30, -1.52), 0.25, 0.42, "M_RubberBlack", axis='Z', segments=16, r_top=0.10)
        # Bow angled tubes meeting at nose (+Z)
        hull.add_box((s * 0.42, 0.33, 1.28), (0.46, 0.44, 0.75), "M_SafetyOrange", 0.12, rot_u=(0, -s * 28, 0))
        # Black heavy rub-strake & white grab rope blocks
        hull.add_box((s * 0.96, 0.30, -0.10), (0.04, 0.09, 2.35), "M_RubberBlack", 0.01)
        hull.add_box((s * 0.73, 0.55, -0.10), (0.08, 0.03, 2.10), "M_White", 0.005)
    # Bow Nose Cap & Searchlight
    hull.add_box((0, 0.35, 1.56), (0.74, 0.44, 0.38), "M_SafetyOrange", 0.12)
    hull.add_box((0, 0.56, 1.62), (0.28, 0.12, 0.18), "M_DarkMetal", 0.015)
    # Two Bench Thwarts & Center Steering Console
    hull.add_box((0, 0.38, -0.62), (1.26, 0.06, 0.30), "M_BrownWood", 0.01)
    hull.add_box((0, 0.38, 0.35), (1.26, 0.06, 0.30), "M_BrownWood", 0.01)
    hull.add_box((0.22, 0.62, 0.02), (0.42, 0.52, 0.36), "M_DarkMetal", 0.02)
    hull.add_box((0.22, 0.94, 0.08), (0.38, 0.22, 0.04), "M_GlassCyan", 0.005, rot_u=(20, 0, 0))
    # Rescue Lifebuoy Ring on port side
    hull.add_cylinder((-0.78, 0.56, 0.45), 0.20, 0.07, "M_Red", axis='Y', segments=16)
    hull.add_cylinder((-0.78, 0.57, 0.45), 0.11, 0.08, "M_White", axis='Y', segments=16)
    root_obj = hull.build()

    # Bow Lamp Lens
    blamp = MeshGroupBuilder("BowLamp_Lens", pivot_u=(0, 0.56, 1.72), parent=root_obj)
    blamp.add_box((0, 0.56, 1.72), (0.20, 0.08, 0.03), "M_GlowWarm", 0.005)
    blamp.build()

    # Console Steering Wheel
    sw = MeshGroupBuilder("SteeringWheel_Pivot", pivot_u=(0.22, 0.78, -0.17), parent=root_obj)
    sw.add_cylinder((0.22, 0.78, -0.17), 0.13, 0.03, "M_RubberBlack", axis='Z', segments=14)
    sw.build()

    # Articulated Outboard Motor (Outboard_Pivot at stern -Z = -1.42)
    ob = MeshGroupBuilder("Outboard_Pivot", pivot_u=(0, 0.36, -1.42), parent=root_obj)
    ob.add_box((0, 0.52, -1.52), (0.30, 0.34, 0.38), "M_DarkMetal", 0.03)  # Motor cowling
    ob.add_box((0, 0.56, -1.52), (0.31, 0.07, 0.36), "M_SafetyOrange", 0.008)
    ob.add_box((0, 0.18, -1.50), (0.09, 0.46, 0.16), "M_DarkMetal", 0.015)  # Lower unit leg
    ob.add_box((-0.08, 0.48, -1.18), (0.04, 0.04, 0.42), "M_RubberBlack", 0.008)  # Tiller arm
    ob_obj = ob.build()

    # Spinning 3-Blade Propeller (Child of Outboard_Pivot at (0, -0.02, -1.62))
    prop = MeshGroupBuilder("Propeller_Pivot", pivot_u=(0, -0.02, -1.62), parent=ob_obj)
    prop.add_cylinder((0, -0.02, -1.62), 0.035, 0.09, "M_Brass", axis='Z', segments=12)
    for b in range(3):
        ang = b * 120.0
        rad = math.radians(ang)
        bx = math.cos(rad) * 0.075
        by = -0.02 + math.sin(rad) * 0.075
        prop.add_box((bx, by, -1.63), (0.12, 0.035, 0.014), "M_Brass", 0.003, rot_u=(0, 0, ang))
    prop.build()

    write_fbx_and_meta("SM_ZodiacBoat")


# ==============================================================================
# 6. MICROWAVE (SM_Microwave.fbx) - 0.54m W, 0.32m H, 0.40m D (-Z front, Y=0 bottom)
# ==============================================================================
def build_microwave():
    reset_scene()
    body = MeshGroupBuilder("Microwave_Body", pivot_u=(0, 0, 0))
    w, h, d = 0.54, 0.32, 0.40
    # Rubber feet
    for sx in (-0.21, 0.21):
        for sz in (-0.15, 0.15):
            body.add_cylinder((sx, 0.01, sz), 0.022, 0.02, "M_RubberBlack", axis='Y', segments=10)
    # Outer Stainless/Enamel Shell (hollow cavity on left, control column on right)
    body.add_box((0, 0.035, 0), (w, 0.03, d), "M_SilverMetal", 0.006)          # Bottom base
    body.add_box((0, h - 0.015, 0), (w, 0.03, d), "M_SilverMetal", 0.008)      # Top panel
    body.add_box((0, h * 0.5, d * 0.5 - 0.015), (w, h - 0.04, 0.03), "M_DarkMetal", 0.006)  # Rear panel
    body.add_box((-w * 0.5 + 0.018, h * 0.5, 0), (0.036, h - 0.04, d), "M_SilverMetal", 0.006)  # Left wall
    # Right Control Column (width = 0.13m)
    body.add_box((w * 0.5 - 0.065, h * 0.5, 0), (0.13, h - 0.04, d), "M_SilverMetal", 0.008)
    body.add_box((w * 0.5 - 0.065, h * 0.5, -d * 0.5 - 0.004), (0.11, h - 0.06, 0.012), "M_ScreenBlack", 0.004)
    # Rotary dial & buttons on right control column
    body.add_cylinder((w * 0.5 - 0.065, 0.13, -d * 0.5 - 0.016), 0.026, 0.022, "M_SilverMetal", axis='Z', segments=16)
    for btn in range(4):
        body.add_box((w * 0.5 - 0.09 + (btn % 2) * 0.05, 0.065 + (btn // 2) * 0.028, -d * 0.5 - 0.012), (0.038, 0.016, 0.008), "M_OffWhite", 0.002)
    # Interior Cavity Walls (OffWhite reflective enamel)
    body.add_box((-0.055, 0.052, 0.01), (0.36, 0.01, 0.34), "M_OffWhite", 0.002)
    body.add_box((-0.055, h * 0.5, 0.17), (0.36, h - 0.06, 0.01), "M_OffWhite", 0.002)
    root_obj = body.build()

    # Digital Green LED Display (wired to MainsPowerV5)
    disp = MeshGroupBuilder("DigitalDisplay_Emitter", pivot_u=(w * 0.5 - 0.065, 0.24, -d * 0.5 - 0.011), parent=root_obj)
    disp.add_box((w * 0.5 - 0.065, 0.24, -d * 0.5 - 0.011), (0.082, 0.038, 0.006), "M_GlowGreen", 0.002)
    disp.build()

    # Interior Cavity Lamp Emitter (wired to MainsPowerV5 + Door state)
    clamp = MeshGroupBuilder("CavityLamp_Emitter", pivot_u=(0.11, 0.27, 0.0), parent=root_obj)
    clamp.add_box((0.11, 0.27, 0.0), (0.016, 0.04, 0.12), "M_GlowWarm", 0.003)
    clamp.build()

    # Rotating Glass Turntable Plate
    tt = MeshGroupBuilder("Turntable_Pivot", pivot_u=(-0.055, 0.065, 0.0), parent=root_obj)
    tt.add_cylinder((-0.055, 0.062, 0.0), 0.10, 0.008, "M_DarkMetal", axis='Y', segments=16)
    tt.add_cylinder((-0.055, 0.070, 0.0), 0.145, 0.010, "M_GlassCyan", axis='Y', segments=20)
    tt.build()

    # Hinged Microwave Door Leaf (named DoorLeaf so BindHinge / PanicV5FurnitureRig works!)
    door = MeshGroupBuilder("DoorLeaf", pivot_u=(-0.252, 0.16, -0.202), parent=root_obj)
    # Door frame around glass window (extends from x=-0.252 to x=+0.135)
    cx = -0.058
    door.add_box((cx, 0.275, -0.208), (0.388, 0.045, 0.024), "M_ScreenBlack", 0.005)
    door.add_box((cx, 0.045, -0.208), (0.388, 0.045, 0.024), "M_ScreenBlack", 0.005)
    door.add_box((-0.228, 0.16, -0.208), (0.048, 0.19, 0.024), "M_ScreenBlack", 0.005)
    door.add_box((0.112, 0.16, -0.208), (0.048, 0.19, 0.024), "M_ScreenBlack", 0.005)
    # Tinted viewing window
    door.add_box((cx, 0.16, -0.208), (0.295, 0.185, 0.010), "M_GlassCyan", 0.002)
    # Pull handle on right side of door
    door.add_box((0.105, 0.16, -0.232), (0.018, 0.19, 0.024), "M_SilverMetal", 0.005)
    door.build()

    write_fbx_and_meta("SM_Microwave")


# ==============================================================================
# 7. SHELVES: SM_ShelfLarge (1.84x2.12x0.50) & SM_ShelfSmall (1.04x1.12x0.42)
# ==============================================================================
def build_shelf_large():
    reset_scene()
    w, h, d = 1.84, 2.12, 0.50
    levels = [0.34, 0.86, 1.38, 1.90]
    body = MeshGroupBuilder("ShelfLarge_Body", pivot_u=(0, 0, 0))
    # 4 corner steel uprights + footpads + top caps
    for sx in (-1, 1):
        for sz in (-1, 1):
            px = sx * (w * 0.5 - 0.032)
            pz = sz * (d * 0.5 - 0.032)
            body.add_box((px, h * 0.5, pz), (0.052, h, 0.052), "M_DarkMetal", 0.006)
            body.add_box((px, 0.015, pz), (0.076, 0.03, 0.076), "M_SilverMetal", 0.004)
            body.add_box((px, h - 0.012, pz), (0.064, 0.024, 0.064), "M_SilverMetal", 0.004)
    # Rear cross-brace rods + partial back rail
    body.add_box((0, h * 0.52, d * 0.5 - 0.018), (0.026, h * 0.92, 0.016), "M_SilverMetal", 0.003, rot_u=(0, 0, 38))
    body.add_box((0, h * 0.52, d * 0.5 - 0.018), (0.026, h * 0.92, 0.016), "M_SilverMetal", 0.003, rot_u=(0, 0, -38))
    # 4 heavy wooden shelf decks with steel lip & side brackets
    for y in levels:
        body.add_box((0, y, 0), (w - 0.05, 0.036, d - 0.04), "M_BrownWood", 0.006)
        body.add_box((0, y - 0.022, -d * 0.5 + 0.026), (w - 0.04, 0.024, 0.022), "M_DarkMetal", 0.004)
        body.add_box((0, y + 0.042, d * 0.5 - 0.024), (w - 0.06, 0.055, 0.018), "M_ChocBrown", 0.004)
        for sx in (-1, 1):
            body.add_box((sx * (w * 0.5 - 0.032), y + 0.055, 0), (0.024, 0.028, d - 0.06), "M_DarkMetal", 0.004)
            for sz in (-1, 1):
                body.add_box((sx * (w * 0.5 - 0.06), y - 0.035, sz * (d * 0.5 - 0.04)), (0.06, 0.06, 0.018), "M_SilverMetal", 0.003)
    body.build()
    write_fbx_and_meta("SM_ShelfLarge")


def build_shelf_small():
    reset_scene()
    w, h, d = 1.04, 1.12, 0.42
    levels = [0.30, 0.80]
    body = MeshGroupBuilder("ShelfSmall_Body", pivot_u=(0, 0, 0))
    for sx in (-1, 1):
        for sz in (-1, 1):
            px = sx * (w * 0.5 - 0.028)
            pz = sz * (d * 0.5 - 0.028)
            body.add_box((px, h * 0.5, pz), (0.045, h, 0.045), "M_DarkMetal", 0.005)
            body.add_box((px, 0.014, pz), (0.068, 0.028, 0.068), "M_SilverMetal", 0.004)
    # Top cap rail
    for sx in (-1, 1):
        body.add_box((sx * (w * 0.5 - 0.028), h - 0.025, 0), (0.035, 0.035, d - 0.04), "M_DarkMetal", 0.004)
    body.add_box((0, h - 0.025, d * 0.5 - 0.024), (w - 0.04, 0.035, 0.032), "M_DarkMetal", 0.004)
    for y in levels:
        body.add_box((0, y, 0), (w - 0.04, 0.032, d - 0.03), "M_BrownWood", 0.005)
        body.add_box((0, y - 0.018, -d * 0.5 + 0.022), (w - 0.03, 0.022, 0.02), "M_SilverMetal", 0.003)
        for i in range(4):
            lx = -w * 0.36 + i * (w * 0.24)
            body.add_box((lx, y + 0.018, 0), (0.012, 0.004, d - 0.06), "M_ChocBrown", 0.001)
    body.build()
    write_fbx_and_meta("SM_ShelfSmall")


# ==============================================================================
# 8. CABINETS (3 Distinct Designs):
#    - SM_CabinetPantry (1.14 x 1.96 x 0.55)
#    - SM_CabinetWardrobe (1.62 x 2.20 x 0.64)
#    - SM_CabinetUtility (1.12 x 1.08 x 0.52)
# ==============================================================================
def build_cabinet_pantry():
    reset_scene()
    w, h, d = 1.14, 1.96, 0.55
    t = 0.042
    body = MeshGroupBuilder("CabinetPantry_Body", pivot_u=(0, 0, 0))
    # Plinth base & Crown molding
    body.add_box((0, 0.045, 0.02), (w - 0.06, 0.09, d - 0.06), "M_ChocBrown", 0.008)
    body.add_box((0, 0.09 + t * 0.5, 0), (w, t, d), "M_BrownWood", 0.008)
    body.add_box((0, h - t * 0.5, -0.01), (w + 0.04, t + 0.01, d + 0.03), "M_ChocBrown", 0.01)
    body.add_box((0, h * 0.52, d * 0.5 - 0.018), (w, h - 0.09, 0.032), "M_BrownWood", 0.006)
    for sx in (-1, 1):
        body.add_box((sx * (w - t) * 0.5, h * 0.52, 0), (t, h - 0.09, d), "M_BrownWood", 0.008)
    # 3 Interior Pantry Shelves with front spice retainer bar
    for y in (0.38, 0.88, 1.38):
        body.add_box((0, y, 0.02), (w - t * 2 - 0.01, 0.028, d - 0.07), "M_OffWhite", 0.004)
        body.add_cylinder((0, y + 0.038, -d * 0.5 + 0.075), 0.008, w - t * 2 - 0.04, "M_Brass", axis='X', segments=10)
    root_obj = body.build()

    # Interior Top Warm Light Strip
    ilamp = MeshGroupBuilder("Cabinet_InteriorLamp_Lens", pivot_u=(0, h - 0.055, -0.08), parent=root_obj)
    ilamp.add_box((0, h - 0.055, -0.08), (w * 0.55, 0.016, 0.04), "M_GlowWarm", 0.003)
    ilamp.build()

    # 2 Louvered Pantry Doors (Door_L_Hinge at x=-0.535, Door_R_Hinge at x=+0.535)
    leaf_w = (w - 0.07) * 0.5
    leaf_h = h - 0.145
    for side, lbl in ((-1, "Door_L_Hinge"), (1, "Door_R_Hinge")):
        px = side * (w * 0.5 - 0.035)
        pz = -d * 0.5 - 0.008
        door = MeshGroupBuilder(lbl, pivot_u=(px, 0.095, pz), parent=root_obj)
        cx = px - side * (leaf_w * 0.5)
        cy = 0.095 + leaf_h * 0.5
        door.add_box((cx, cy, pz), (leaf_w, leaf_h, 0.028), "M_BrownWood", 0.007)
        door.add_box((cx, cy, pz - 0.012), (leaf_w - 0.07, leaf_h - 0.12, 0.012), "M_ChocBrown", 0.004)
        # Louvered ventilation slats
        for sl in range(8):
            door.add_box((cx, cy + 0.12 + sl * 0.075, pz - 0.020), (leaf_w - 0.13, 0.018, 0.010), "M_BrownWood", 0.002)
        # Brass vertical handle near center seam
        hx = px - side * (leaf_w - 0.048)
        door.add_box((hx, cy, pz - 0.034), (0.020, 0.18, 0.028), "M_Brass", 0.004)
        door.build()

    write_fbx_and_meta("SM_CabinetPantry")


def build_cabinet_wardrobe():
    reset_scene()
    w, h, d = 1.62, 2.20, 0.64
    t = 0.044
    body = MeshGroupBuilder("CabinetWardrobe_Body", pivot_u=(0, 0, 0))
    # Sculpted base feet & Cornice top
    for sx in (-1, 1):
        for sz in (-1, 1):
            body.add_box((sx * (w * 0.5 - 0.07), 0.045, sz * (d * 0.5 - 0.07)), (0.09, 0.09, 0.09), "M_ChocBrown", 0.01)
    body.add_box((0, 0.09 + t * 0.5, 0), (w, t, d), "M_OffWhite", 0.008)
    body.add_box((0, h - t * 0.5, -0.012), (w + 0.05, t + 0.014, d + 0.035), "M_BrownWood", 0.012)
    body.add_box((0, h * 0.52, d * 0.5 - 0.018), (w, h - 0.09, 0.034), "M_OffWhite", 0.006)
    for sx in (-1, 1):
        body.add_box((sx * (w - t) * 0.5, h * 0.52, 0), (t, h - 0.09, d), "M_OffWhite", 0.008)
        body.add_box((sx * (w * 0.5 + 0.004), h * 0.52, 0), (0.012, h - 0.24, d - 0.12), "M_BrownWood", 0.004)
    # 3 Levels (0.34, 1.04, 1.72) + Chrome Clothes Hanging Rod with Hangers
    for y in (0.34, 1.04, 1.72):
        body.add_box((0, y, 0.02), (w - t * 2 - 0.01, 0.03, d - 0.08), "M_BrownWood", 0.005)
    body.add_cylinder((0, 1.96, 0.0), 0.014, w - t * 2 - 0.02, "M_SilverMetal", axis='X', segments=12)
    for hg in (-0.35, 0.0, 0.35):
        body.add_box((hg, 1.88, 0.0), (0.014, 0.10, 0.32), "M_SilverMetal", 0.003)
    root_obj = body.build()

    # Interior Top Warm Light Strip
    ilamp = MeshGroupBuilder("Cabinet_InteriorLamp_Lens", pivot_u=(0, h - 0.06, -0.10), parent=root_obj)
    ilamp.add_box((0, h - 0.06, -0.10), (w * 0.58, 0.016, 0.045), "M_GlowWarm", 0.003)
    ilamp.build()

    # 2 Paneled Armoire Doors (Door_L_Hinge at x=-0.765, Door_R_Hinge at x=+0.765)
    leaf_w = (w - 0.07) * 0.5
    leaf_h = h - 0.145
    for side, lbl in ((-1, "Door_L_Hinge"), (1, "Door_R_Hinge")):
        px = side * (w * 0.5 - 0.035)
        pz = -d * 0.5 - 0.008
        door = MeshGroupBuilder(lbl, pivot_u=(px, 0.095, pz), parent=root_obj)
        cx = px - side * (leaf_w * 0.5)
        cy = 0.095 + leaf_h * 0.5
        door.add_box((cx, cy, pz), (leaf_w, leaf_h, 0.030), "M_OffWhite", 0.008)
        # Upper & lower raised wood trim panels
        door.add_box((cx, cy + 0.42, pz - 0.015), (leaf_w - 0.11, leaf_h * 0.44, 0.014), "M_BrownWood", 0.005)
        door.add_box((cx, cy - 0.44, pz - 0.015), (leaf_w - 0.11, leaf_h * 0.36, 0.014), "M_BrownWood", 0.005)
        hx = px - side * (leaf_w - 0.055)
        door.add_box((hx, cy, pz - 0.036), (0.022, 0.24, 0.030), "M_SilverMetal", 0.005)
        door.build()

    write_fbx_and_meta("SM_CabinetWardrobe")


def build_cabinet_utility():
    reset_scene()
    w, h, d = 1.12, 1.08, 0.52
    t = 0.040
    body = MeshGroupBuilder("CabinetUtility_Body", pivot_u=(0, 0, 0))
    # Industrial steel feet & DarkMetal frame + Butcher-block worktop
    for sx in (-1, 1):
        for sz in (-1, 1):
            body.add_box((sx * (w * 0.5 - 0.055), 0.04, sz * (d * 0.5 - 0.055)), (0.075, 0.08, 0.075), "M_DarkMetal", 0.006)
    body.add_box((0, 0.08 + t * 0.5, 0), (w, t, d), "M_DarkMetal", 0.006)
    body.add_box((0, h - 0.024, 0), (w + 0.03, 0.048, d + 0.03), "M_BrownWood", 0.008)  # Worktop
    body.add_box((0, h - 0.024, -d * 0.5 - 0.016), (w + 0.03, 0.048, 0.012), "M_SilverMetal", 0.003)  # Steel edge
    body.add_box((0, h * 0.50, d * 0.5 - 0.018), (w, h - 0.11, 0.032), "M_DarkMetal", 0.005)
    for sx in (-1, 1):
        body.add_box((sx * (w - t) * 0.5, h * 0.50, 0), (t, h - 0.11, d), "M_NavyBlue", 0.006)
    # Center vertical partition between left locker & right drawers (at x = +0.04)
    body.add_box((0.04, h * 0.50, 0), (0.03, h - 0.12, d - 0.04), "M_DarkMetal", 0.004)
    # 2 Interior Shelf Levels (0.31, 0.73)
    for y in (0.31, 0.73):
        body.add_box((-0.24, y, 0.02), (0.52, 0.024, d - 0.07), "M_SilverMetal", 0.004)
    # 3 Right-side Tool Drawers with orange accent & label plates
    for dr in range(3):
        dy = 0.24 + dr * 0.28
        body.add_box((0.30, dy, -d * 0.5 - 0.008), (0.46, 0.24, 0.026), "M_SafetyOrange", 0.006)
        body.add_box((0.30, dy + 0.04, -d * 0.5 - 0.024), (0.12, 0.04, 0.008), "M_OffWhite", 0.002)
        body.add_box((0.30, dy - 0.03, -d * 0.5 - 0.028), (0.18, 0.022, 0.022), "M_SilverMetal", 0.004)
    root_obj = body.build()

    # Interior Top Warm Light Strip
    ilamp = MeshGroupBuilder("Cabinet_InteriorLamp_Lens", pivot_u=(-0.24, h - 0.065, -0.08), parent=root_obj)
    ilamp.add_box((-0.24, h - 0.065, -0.08), (0.38, 0.015, 0.04), "M_GlowWarm", 0.003)
    ilamp.build()

    # Left Hinged Utility Locker Door (Door_L_Hinge at x=-0.52, opens +97 deg)
    px = -w * 0.5 + 0.04
    pz = -d * 0.5 - 0.008
    leaf_w = 0.54
    leaf_h = h - 0.14
    door = MeshGroupBuilder("Door_L_Hinge", pivot_u=(px, 0.085, pz), parent=root_obj)
    cx = px + leaf_w * 0.5
    cy = 0.085 + leaf_h * 0.5
    door.add_box((cx, cy, pz), (leaf_w, leaf_h, 0.028), "M_NavyBlue", 0.006)
    door.add_box((cx, cy, pz - 0.012), (leaf_w - 0.08, leaf_h - 0.12, 0.012), "M_DarkMetal", 0.004)
    for v in range(5):
        door.add_box((cx, cy + 0.14 + v * 0.045, pz - 0.020), (leaf_w - 0.16, 0.014, 0.008), "M_SilverMetal", 0.002)
    door.add_box((px + leaf_w - 0.05, cy, pz - 0.032), (0.020, 0.16, 0.026), "M_SilverMetal", 0.004)
    door.build()

    write_fbx_and_meta("SM_CabinetUtility")


# ==============================================================================
# 9. RESPONDER KITS:
#    - SM_ResponderKit_Police.fbx (Polisman)
#    - SM_ResponderKit_Firefighter.fbx (İtfaiyeci)
#    - SM_ResponderKit_Paramedic.fbx (Medic adam)
#    - SM_ResponderKit_Rescue.fbx (Arama kurtarma)
# ==============================================================================
def build_responder_kits():
    # 9A. Police Kit
    reset_scene()
    cap = MeshGroupBuilder("PolicePeakedCap", pivot_u=(0, 0, 0))
    cap.add_cylinder((0, 0.05, 0), 0.115, 0.07, "M_NavyBlue", axis='Y', segments=16)
    cap.add_cylinder((0, 0.095, -0.01), 0.132, 0.04, "M_NavyBlue", axis='Y', segments=16)
    cap.add_cylinder((0, 0.032, 0), 0.117, 0.022, "M_Blue", axis='Y', segments=16)
    cap.add_box((0, 0.022, 0.105), (0.17, 0.012, 0.095), "M_RubberBlack", 0.004, rot_u=(14, 0, 0))
    cap.add_box((0, 0.068, 0.118), (0.036, 0.044, 0.012), "M_Brass", 0.003)
    cap.build()

    vest = MeshGroupBuilder("PoliceTacticalVest", pivot_u=(0, 0, 0))
    vest.add_box((0, 0.02, 0), (0.33, 0.34, 0.23), "M_NavyBlue", 0.018)
    vest.add_box((0, 0.05, 0.118), (0.28, 0.08, 0.016), "M_Blue", 0.004)
    vest.add_box((0, 0.05, -0.118), (0.28, 0.09, 0.016), "M_Blue", 0.004)
    vest.add_box((-0.08, 0.11, 0.122), (0.040, 0.048, 0.012), "M_Brass", 0.003)  # Star badge
    vest.add_box((0.08, 0.09, 0.122), (0.036, 0.052, 0.018), "M_RubberBlack", 0.003)  # Bodycam
    vest.add_box((0.08, 0.10, 0.132), (0.012, 0.012, 0.006), "M_GlowGreen", 0.001)
    vest.build()

    belt = MeshGroupBuilder("PoliceDutyBelt", pivot_u=(0, 0, 0))
    belt.add_box((0, 0, 0), (0.33, 0.06, 0.24), "M_RubberBlack", 0.008)
    belt.add_box((0, 0, 0.124), (0.055, 0.05, 0.016), "M_Brass", 0.003)
    belt.add_box((0.17, -0.03, 0.02), (0.045, 0.12, 0.08), "M_RubberBlack", 0.006)  # Holster
    belt.add_box((-0.16, 0.03, 0.04), (0.048, 0.10, 0.045), "M_RubberBlack", 0.005)  # WalkieTalkie
    belt.add_cylinder((-0.16, 0.12, 0.04), 0.006, 0.10, "M_DarkMetal", axis='Y', segments=8)
    belt.build()
    write_fbx_and_meta("SM_ResponderKit_Police")

    # 9B. Firefighter Kit
    reset_scene()
    fhelm = MeshGroupBuilder("FirefighterHelmet", pivot_u=(0, 0, 0))
    fhelm.add_cylinder((0, 0.07, 0), 0.128, 0.11, "M_Red", axis='Y', segments=18, r_top=0.095)
    fhelm.add_box((0, 0.02, -0.015), (0.27, 0.018, 0.32), "M_Red", 0.006)  # Wide brim
    fhelm.add_box((0, 0.13, 0), (0.038, 0.045, 0.22), "M_Red", 0.006)       # Comb crest
    fhelm.add_box((0, 0.085, 0.125), (0.065, 0.075, 0.016), "M_Brass", 0.004)  # Front shield
    fhelm.add_box((0, -0.02, 0.128), (0.20, 0.085, 0.012), "M_GlassCyan", 0.003)  # Visor
    fhelm.add_box((0, -0.03, -0.115), (0.22, 0.09, 0.024), "M_RubberBlack", 0.005)  # Neck shroud
    fhelm.add_box((0, 0.105, 0.140), (0.056, 0.036, 0.032), "M_DarkMetal", 0.004)  # Headlamp housing
    fhelm.add_box((0, 0.105, 0.158), (0.042, 0.024, 0.008), "M_GlowWarm", 0.002)   # Headlamp lens
    fhelm.build()

    fcoat = MeshGroupBuilder("FireTurnoutAndSCBA", pivot_u=(0, 0, 0))
    fcoat.add_box((0, 0.02, 0), (0.35, 0.38, 0.25), "M_DarkRed", 0.02)
    fcoat.add_box((0, -0.08, 0), (0.36, 0.055, 0.26), "M_Yellow", 0.005)
    fcoat.add_box((0, 0.08, 0), (0.36, 0.045, 0.26), "M_SilverMetal", 0.005)
    # SCBA Backplate & Yellow Compressed Air Cylinder
    fcoat.add_box((0, 0.03, -0.145), (0.22, 0.32, 0.035), "M_DarkMetal", 0.006)
    fcoat.add_cylinder((0, 0.05, -0.215), 0.072, 0.36, "M_Yellow", axis='Y', segments=16)
    fcoat.add_cylinder((0, 0.24, -0.215), 0.052, 0.05, "M_DarkMetal", axis='Y', segments=12)
    fcoat.add_cylinder((0, -0.14, -0.215), 0.032, 0.05, "M_Brass", axis='Y', segments=10)
    fcoat.build()

    fbelt = MeshGroupBuilder("FireBunkerBelt", pivot_u=(0, 0, 0))
    fbelt.add_box((0, 0, 0), (0.35, 0.06, 0.25), "M_RubberBlack", 0.008)
    fbelt.add_box((0.18, -0.04, 0), (0.03, 0.22, 0.04), "M_BrownWood", 0.004)  # Fire axe handle
    fbelt.add_box((0.18, 0.06, 0.03), (0.025, 0.06, 0.11), "M_Red", 0.004)     # Fire axe blade
    fbelt.build()
    write_fbx_and_meta("SM_ResponderKit_Firefighter")

    # 9C. Paramedic (Medic adam) Kit
    reset_scene()
    mcap = MeshGroupBuilder("ParamedicCap", pivot_u=(0, 0, 0))
    mcap.add_cylinder((0, 0.055, 0), 0.114, 0.08, "M_White", axis='Y', segments=16)
    mcap.add_box((0, 0.024, 0.105), (0.16, 0.012, 0.085), "M_Blue", 0.004, rot_u=(10, 0, 0))
    mcap.add_box((0, 0.065, 0.116), (0.042, 0.014, 0.008), "M_Red", 0.002)
    mcap.add_box((0, 0.065, 0.116), (0.014, 0.042, 0.008), "M_Red", 0.002)
    mcap.build()

    mvest = MeshGroupBuilder("ParamedicVest", pivot_u=(0, 0, 0))
    mvest.add_box((0, 0.02, 0), (0.33, 0.35, 0.23), "M_White", 0.018)
    mvest.add_box((0, -0.06, 0), (0.34, 0.05, 0.24), "M_SafetyOrange", 0.005)
    mvest.add_box((0, 0.08, 0), (0.34, 0.04, 0.24), "M_Blue", 0.005)
    for sz in (-0.122, 0.122):
        mvest.add_box((0, 0.04, sz), (0.11, 0.034, 0.010), "M_Red", 0.002)
        mvest.add_box((0, 0.04, sz), (0.034, 0.11, 0.010), "M_Red", 0.002)
    # Stethoscope collar
    mvest.add_cylinder((0, 0.19, 0.02), 0.095, 0.022, "M_RubberBlack", axis='Y', segments=14)
    mvest.add_cylinder((0.06, 0.11, 0.122), 0.022, 0.012, "M_SilverMetal", axis='Z', segments=12)
    mvest.build()

    mbag = MeshGroupBuilder("ParamedicFirstAidBag", pivot_u=(0, 0, 0))
    mbag.add_box((0, 0, 0), (0.24, 0.17, 0.14), "M_Red", 0.015)
    mbag.add_box((0, 0.095, 0), (0.10, 0.025, 0.03), "M_RubberBlack", 0.004)
    for sz in (-0.072, 0.072):
        mbag.add_box((0, 0, sz), (0.08, 0.024, 0.008), "M_White", 0.002)
        mbag.add_box((0, 0, sz), (0.024, 0.08, 0.008), "M_White", 0.002)
    mbag.build()
    write_fbx_and_meta("SM_ResponderKit_Paramedic")

    # 9D. Search & Rescue Kit
    reset_scene()
    rhelm = MeshGroupBuilder("RescueHelmet", pivot_u=(0, 0, 0))
    rhelm.add_cylinder((0, 0.065, 0), 0.124, 0.10, "M_SafetyOrange", axis='Y', segments=16, r_top=0.095)
    rhelm.add_box((0, 0.045, 0.122), (0.17, 0.042, 0.022), "M_GlassCyan", 0.005)
    rhelm.add_box((0, 0.098, 0.128), (0.058, 0.036, 0.030), "M_DarkMetal", 0.004)
    rhelm.add_box((0, 0.098, 0.146), (0.044, 0.024, 0.008), "M_GlowWarm", 0.002)
    rhelm.build()

    rpack = MeshGroupBuilder("RescueHarnessAndPack", pivot_u=(0, 0, 0))
    rpack.add_box((0, 0.02, 0), (0.34, 0.35, 0.24), "M_SafetyOrange", 0.018)
    for sx in (-0.08, 0.08):
        rpack.add_box((sx, 0.02, 0), (0.042, 0.36, 0.25), "M_RubberBlack", 0.004)
    rpack.add_box((0, 0.04, 0.128), (0.045, 0.055, 0.016), "M_SilverMetal", 0.004)
    rpack.add_box((0, 0.03, -0.175), (0.24, 0.30, 0.11), "M_SafetyOrange", 0.018)
    rpack.add_cylinder((0, 0.20, -0.175), 0.045, 0.24, "M_Yellow", axis='X', segments=12)
    rpack.build()
    write_fbx_and_meta("SM_ResponderKit_Rescue")


if __name__ == "__main__":
    build_ambulance()
    build_police_car()
    build_fire_engine()
    build_rescue_truck()
    build_zodiac_boat()
    build_microwave()
    build_shelf_large()
    build_shelf_small()
    build_cabinet_pantry()
    build_cabinet_wardrobe()
    build_cabinet_utility()
    build_responder_kits()
    print("ALL 15 V5 CURATED FBX MODELS GENERATED SUCCESSFULLY.")
