import bpy
from mathutils import *
import math
import random
import msgpackrpc
import sys


class RenderServer(object):
    def renderImage(self, path, luku):
        random.seed()
        rotations = [ (0,math.pi*1.5,0),(math.pi*1,0,0),(math.pi*0.5,0,0),(math.pi*1.5,0,0),(0,0,0),(0,math.pi*0.5,0) ]
        noppa = bpy.data.objects["Noppa"]
        noppa.rotation_mode = 'XYZ'
        noppa.rotation_euler = ( rotations[luku] )
        noppa.rotation_euler[2] =random.random()*2*math.pi 
        noppa.location = (random.random()-0.5,random.random()*2-1,0.50)
        bpy.data.scenes['Scene'].render.filepath = path
        bpy.data.scenes['Scene'].render.image_settings.file_format = 'JPEG'
        bpy.ops.render.render( write_still=True ) 

    def exit(self):
        sys.exit()

server = msgpackrpc.Server(RenderServer())
server.listen(msgpackrpc.Address("localhost", 18800))
server.start()
