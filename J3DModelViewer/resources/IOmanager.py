# Model Viewer
import bpy
import sys
import os.path
path=' '.join(sys.argv[4:])  # path to the bmd file

print("PATH: " + path)

bpy.data.objects.remove(bpy.data.objects['Cube'])
bpy.data.meshes.remove(bpy.data.meshes['Cube'])

bpy.data.objects.remove(bpy.data.objects['Light'])
bpy.data.lights.remove(bpy.data.lights['Light'])

bpy.data.objects.remove(bpy.data.objects['Camera'])
bpy.data.cameras.remove(bpy.data.cameras['Camera'])

try:
    bpy.ops.blemd.importer(filepath=path)
except AttributeError:  # module not loaded: do it manually
    import blemd
    temp = blemd.BModel.BModel()
    # current_dir = OSPath.abspath(OSPath.split(__file__)[0])  # automatically find where we are
    temp.SetBmdViewExePath(os.path.split(blemd.__file__)[0]+os.path.sep)  # add backslash for good measure
    temp.Import(path,
        boneThickness=5,
        frc_cr_bn=False,
        sv_anim='SEPARATE',
        tx_pck='DO',
        ic_sc=True,
        imtype='TGA'
    )
    # actual model importing
    
    for ob in bpy.data.objects:
        if "armature" in ob.name:
            try:
                #ob.rotation_euler[0] = math.radians(90)
                print ("removed material from " + ob.name)
                ob.scale = (0.01, 0.01, 0.01)
            except:
                print (ob.name + " does not have materials.")
                        
    for a in bpy.context.screen.areas:
        if a.type == 'VIEW_3D':
             for s in a.spaces:
                if s.type == 'VIEW_3D':
                    s.clip_end = 999999999
                            
    for window in bpy.context.window_manager.windows:
        for area in window.screen.areas: # iterate through areas in current screen
            if area.type == 'VIEW_3D':
                for space in area.spaces: # iterate through spaces in current VIEW_3D area
                    if space.type == 'VIEW_3D': # check if space is a 3D view
                        space.shading.type = 'MATERIAL'


# this line (below) is the export command. feel free to change it to whatever you want
bpy.ops.export_scene.fbx(filepath=path[:-3]+'fbx', axis_forward='Y', axis_up='Z', path_mode='COPY', embed_textures=True, apply_scale_options='FBX_SCALE_UNITS',apply_unit_scale=True)

bpy.ops.wm.quit_blender()  # quit blender the clean way