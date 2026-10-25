#!/usr/bin/env python3
"""Original, deterministic pixel art and synthesized sounds. No external art dependencies."""
from pathlib import Path
from PIL import Image, ImageDraw
import math, random, wave, struct, shutil, hashlib, argparse
parser = argparse.ArgumentParser()
parser.add_argument('--art-only', action='store_true', help='Regenerate pixel art without changing sound or fonts.')
args = parser.parse_args()
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT/'Assets/DuneDash/Resources'
random.seed(1027)
# Salt Coast: ocean slate, sea-glass explorer, amber cloth and pale salt.
ink='#1c3049'; coral='#63c4a2'; dark='#36877d'; light='#d7f1cd'; sand='#87a5b0'
def save(im,name,ppu=32,pivot=(.5,0)):
    p=OUT/'Sprites'/f'{name}.png'; im.save(p)
    guid=hashlib.md5(('dunedash/'+name).encode()).hexdigest()
    p.with_suffix('.png.meta').write_text(f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  mipmaps:
    enableMipMap: 0
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  spriteMode: 1
  spritePixelsToUnits: {ppu}
  alignment: 9
  spritePivot: {{x: {pivot[0]}, y: {pivot[1]}}}
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
  platformSettings: []
  assetBundleName:
  assetBundleVariant:
''')

def dino(name,frame=0,state='run'):
    im=Image.new('RGBA',(64,64)); d=ImageDraw.Draw(im)
    def rect(box,col): d.rectangle(box,fill=col)
    if state=='death':
        # The death sprite is drawn on its side, with a closed eye.
        d.polygon([(8,51),(8,43),(16,43),(16,37),(39,37),(39,32),(56,32),(56,49),(44,49),(44,57),(17,57),(17,51)],fill=ink)
        rect((17,40,40,53),coral);rect((42,35,53,46),coral)
        rect((37,40,41,50),'#e4b970');rect((22,41,30,47),dark)
        d.line((46,38,51,42),fill=ink,width=2); d.line((51,38,46,42),fill=ink,width=2)
        rect((22,54,28,59),ink); rect((34,54,40,59),ink)
    else:
        bob=1 if state=='run' and frame%2 else 0
        # T-rex silhouette, tail, head and jaw, hand-authored at native pixel size.
        points=[(5,31+bob),(11,36+bob),(18,38+bob),(18,30+bob),(25,30+bob),(25,23+bob),(31,23+bob),(29,12+bob),(34,6+bob),(50,6+bob),(57,12+bob),(60,16+bob),(60,25+bob),(45,25+bob),(45,30+bob),(37,30+bob),(37,41+bob),(30,49+bob),(16,49+bob),(11,44+bob),(5,42+bob)]
        d.polygon(points,fill=ink)
        d.polygon([(8,35+bob),(18,41+bob),(21,34+bob),(28,34+bob),(28,26+bob),(34,26+bob),(33,14+bob),(36,9+bob),(49,9+bob),(54,14+bob),(57,18+bob),(57,21+bob),(43,21+bob),(43,27+bob),(34,27+bob),(34,40+bob),(28,46+bob),(17,46+bob),(13,41+bob),(8,40+bob)],fill=coral if state!='hit' else '#f7dba7')
        rect((35,11+bob,38,21+bob),light)
        rect((49,12+bob,52,16+bob),ink);rect((49,12+bob,50,13+bob),'#ffffff')
        rect((47,20+bob,59,22+bob),ink)
        rect((34,32+bob,43,35+bob),ink);rect((40,34+bob,43,38+bob),ink)
        rect((22,36+bob,26,45+bob),light)
        # Explorer details are drawn in every pose, including the airborne frame.
        for x,y in [(32,6),(38,3),(44,4)]:
            d.polygon([(x,y+5+bob),(x+2,y+bob),(x+5,y+5+bob)],fill=dark)
        rect((29,24+bob,42,28+bob),'#e4b970')
        d.polygon([(30,25+bob),(24,24+bob),(12,20+bob+frame%2),(15,27+bob),(29,29+bob)],fill='#c58450')
        rect((17,29+bob,25,39+bob),ink);rect((19,31+bob,23,37+bob),'#a2cab1')
        rect((20,33+bob,22,34+bob),'#e4b970')
        # Two genuinely different running leg poses.
        if state=='jump':
            rect((17,47,22,54),ink);rect((17,52,27,55),ink);rect((29,45,34,51),ink);rect((33,48,40,51),ink)
        elif frame%2==0:
            rect((17,46+bob,22,59),ink);rect((18,48+bob,20,56),coral);rect((17,58,28,61),ink)
            rect((29,45+bob,34,53),ink);rect((33,51,40,54),ink)
        else:
            rect((28,45+bob,34,59),ink);rect((29,48+bob,31,56),coral);rect((29,58,40,61),ink)
            rect((16,45+bob,21,52),ink);rect((11,50,20,54),ink)
    save(im,name)
for i in range(4): dino('DinoRun'+str(i),i)
dino('DinoJump',state='jump');dino('DinoHit',state='hit');dino('DinoDeath',state='death')
# Crystalline basalt obstacles retain the gameplay names and collider dimensions.
for name,tall in [('Cactus',False),('CactusTall',True)]:
    im=Image.new('RGBA',(48,64));d=ImageDraw.Draw(im);peak=5 if tall else 19
    d.polygon([(5,63),(9,43),(16,36),(18,peak+8),(27,peak),(34,30),(40,39),(44,63)],fill=ink)
    d.polygon([(10,60),(13,45),(20,39),(21,peak+10),(26,peak+5),(30,32),(36,43),(40,60)],fill='#4f7289')
    d.polygon([(21,peak+10),(26,peak+5),(26,53),(18,58)],fill='#8eb7be')
    d.line([(27,peak+12),(25,34),(31,42),(26,58)],fill='#e4b970',width=2)
    d.polygon([(6,62),(12,52),(18,63)],fill='#c7dfd7');d.polygon([(33,62),(39,49),(44,63)],fill='#9bc4c5')
    save(im,name)
# Two poses of an airborne salt-coast ray, with a contrasting amber tail.
for i in range(2):
    im=Image.new('RGBA',(64,40));d=ImageDraw.Draw(im)
    d.polygon([(2,24),(19,19),(31,13),(46,14),(59,22),(46,29),(29,27),(18,24)],fill=ink)
    wing=[(20,20),(13,2),(30,9),(40,22)] if i==0 else [(20,20),(14,37),(31,30),(40,22)]
    d.polygon(wing,fill=ink)
    d.polygon([(23,20),(18,7 if i==0 else 32),(31,13 if i==0 else 27),(36,22)],fill='#63c4a2')
    d.polygon([(31,17),(44,17),(53,22),(44,25),(31,24)],fill='#4f7289')
    d.line([(4,24),(17,24),(23,27)],fill='#e4b970',width=2);d.rectangle((47,19,49,20),fill='#fff0cf')
    save(im,'Bird'+str(i),32,(.5,.5))
im=Image.new('RGBA',(24,28));d=ImageDraw.Draw(im)
d.polygon([(12,0),(22,8),(20,20),(12,27),(3,20),(1,8)],fill=ink)
d.polygon([(12,3),(19,9),(17,19),(12,23),(6,18),(4,9)],fill='#e4b970')
d.polygon([(12,3),(12,23),(6,18),(4,9)],fill='#fff0cf');d.polygon([(12,10),(17,12),(14,19),(12,23)],fill='#b9784e')
save(im,'Coin',32,(.5,.5))
# Seamless flat-topped salt cliffs replace the rounded desert dunes.
for name,color,amp,base,periods in [('FarDunes','#aac3cc',42,65,2),('MidDunes','#779caa',55,88,3),('NearDunes','#4f7289',30,104,4)]:
    im=Image.new('RGBA',(768,192));d=ImageDraw.Draw(im)
    profile=[(0,.5),(.12,.5),(.27,-.6),(.35,-.6),(.58,.25),(.68,.25),(.85,.5),(1,.5)]
    points=[(int((cycle+u)*768/periods),int(base+amp*v)) for cycle in range(periods) for u,v in profile]
    d.polygon(points+[(768,192),(0,192)],fill=color)
    strata={'FarDunes':'#bfd3d7','MidDunes':'#91b2bc','NearDunes':'#63899c'}[name]
    for x in range(0,768,96):d.line((x,158,x+60,158),fill=strata,width=2)
    save(im,name,32)
im=Image.new('RGBA',(128,128));d=ImageDraw.Draw(im)
d.ellipse((17,10,111,104),fill='#f2d6a4');d.ellipse((39,26,58,45),fill='#ddbc88');d.ellipse((70,52,98,80),fill='#e4c18a');d.ellipse((37,66,48,77),fill='#fff0cf')
d.arc((2,38,126,106),0,180,fill='#c29a67',width=4);d.arc((4,34,124,101),180,360,fill='#fff0cf',width=3)
save(im,'Sun',32,(.5,.5))
im=Image.new('RGBA',(256,64));d=ImageDraw.Draw(im)
d.rectangle((0,0,255,63),fill=sand);d.rectangle((0,0,255,3),fill=ink);d.rectangle((0,4,255,8),fill='#c4d9d6')
for _ in range(65):
    x=random.randrange(256);y=random.randrange(14,64)
    d.line((x,y,x+random.randrange(2,7),y-2),fill=random.choice(['#6b8f9f','#d4e4df','#a9c5cd']),width=1)
save(im,'Ground',32,(.5,1))
im=Image.new('RGBA',(128,64));d=ImageDraw.Draw(im)
d.polygon([(3,39),(16,31),(30,31),(39,19),(66,19),(77,29),(107,29),(121,39),(113,47),(18,47)],fill='#f4faf6')
d.line((15,53,89,53),fill='#eaf3ef',width=2)
save(im,'Cloud',32,(.5,.5))
im=Image.new('RGBA',(64,32));d=ImageDraw.Draw(im)
d.ellipse((1,6,62,29),fill=(28,48,73,40));save(im,'Shadow',32,(.5,.5))
im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im)
d.polygon([(16,0),(20,12),(31,16),(20,20),(16,31),(12,20),(0,16),(12,12)],fill='#fff0bd');save(im,'Spark',32,(.5,.5))
im=Image.new('RGBA',(8,8),'white');save(im,'Pixel',32,(.5,.5))
if args.art_only:
 print('Generated Salt Coast sprites; sound and fonts were preserved.')
 raise SystemExit(0)
# UI typefaces, distributed under the SIL Open Font License.
for src,name in [('/usr/share/fonts/truetype/lato/Lato-Regular.ttf','Body.ttf'),('/usr/share/fonts/truetype/lato/Lato-Heavy.ttf','Display.ttf')]:
 shutil.copy(src,OUT/'Fonts'/name)
license_path=Path('/usr/share/doc/fonts-lato/copyright')
if license_path.exists(): shutil.copy(license_path,ROOT/'Documentation/FONT-LICENSE.txt')

def tone(name,duration,base,kind):
 rate=22050;s=[]
 for j in range(int(duration*rate)):
  t=j/rate;v=t/duration;en=(1-v)**2*min(1,t*100)
  freq=base*(1+v*1.4 if kind=='up' else 1-v*.65)
  val=math.sin(math.tau*freq*t)*.55+math.sin(math.tau*freq*2*t)*.12
  if kind=='noise':val=val*.35+random.uniform(-1,1)*.5
  s.append(int(max(-1,min(1,val*en*.4))*32767))
 with wave.open(str(OUT/'Audio'/f'{name}.wav'),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(struct.pack('<'+'h'*len(s),*s))
for a in [('Jump',.22,390,'up'),('Dash',.30,220,'noise'),('Hit',.22,160,'noise'),('Coin',.24,880,'up'),('Death',.65,230,'down'),('Click',.08,560,'up')]:tone(*a)
# Quiet pentatonic ambient loop. Exact note boundaries ensure a seamless loop.
rate=22050; duration=16; samples=[]; notes=[196,246.94,293.66,369.99,440,369.99,293.66,246.94]
for j in range(rate*duration):
 t=j/rate; beat=int(t/.5);loc=t%.5;f=notes[(beat*3//2)%len(notes)]
 env=math.sin(math.pi*loc/.5)**2
 val=(math.sin(math.tau*f*t)+.25*math.sin(math.tau*f*2*t))*env*.07
 pad=math.sin(math.tau*98*t)*.025*math.sin(math.pi*t/duration)**2
 samples.append(int((val+pad)*32767))
with wave.open(str(OUT/'Audio/Ambience.wav'),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(struct.pack('<'+'h'*len(samples),*samples))
print('Generated original sprites, sound effects and music.')
