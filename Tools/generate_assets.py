#!/usr/bin/env python3
"""Original, deterministic pixel art and synthesized sounds. No external art dependencies."""
from pathlib import Path
from PIL import Image, ImageDraw
import math, random, wave, struct, shutil, hashlib
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT/'Assets/DuneDash/Resources'
random.seed(1977)
ink='#23464b'; coral='#e47858'; dark='#ad4d43'; light='#ffbe83'; sand='#edbd82'
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
        d.line((46,38,51,42),fill=ink,width=2); d.line((51,38,46,42),fill=ink,width=2)
        rect((22,54,28,59),ink); rect((34,54,40,59),ink)
    else:
        bob=1 if state=='run' and frame%2 else 0
        # T-rex silhouette, tail, head and jaw, hand-authored at native pixel size.
        points=[(5,31+bob),(11,36+bob),(18,38+bob),(18,30+bob),(25,30+bob),(25,23+bob),(31,23+bob),(31,8+bob),(35,4+bob),(56,4+bob),(60,8+bob),(60,25+bob),(45,25+bob),(45,30+bob),(37,30+bob),(37,41+bob),(30,49+bob),(16,49+bob),(11,44+bob),(5,42+bob)]
        d.polygon(points,fill=ink)
        d.polygon([(8,35+bob),(18,41+bob),(21,34+bob),(28,34+bob),(28,26+bob),(34,26+bob),(34,9+bob),(38,7+bob),(54,7+bob),(57,10+bob),(57,21+bob),(43,21+bob),(43,27+bob),(34,27+bob),(34,40+bob),(28,46+bob),(17,46+bob),(13,41+bob),(8,40+bob)],fill=coral if state!='hit' else '#f7dba7')
        rect((35,11+bob,38,21+bob),light)
        rect((50,10+bob,53,14+bob),ink);rect((50,10+bob,51,11+bob),'#ffffff')
        rect((47,20+bob,59,22+bob),ink)
        rect((34,32+bob,43,35+bob),ink);rect((40,34+bob,43,38+bob),ink)
        rect((22,36+bob,26,45+bob),light)
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
for name,tall in [('Cactus',False),('CactusTall',True)]:
    im=Image.new('RGBA',(48,64));d=ImageDraw.Draw(im)
    d.rectangle((19,8 if tall else 20,30,62),fill=ink)
    d.rectangle((21,10 if tall else 22,27,61),fill='#44867a')
    d.rectangle((6,28,12,49),fill=ink);d.rectangle((6,45,22,51),fill=ink)
    d.rectangle((31,32,42,39),fill=ink);d.rectangle((36,17,42,39),fill=ink)
    d.rectangle((8,30,10,47),fill='#74ab85');d.rectangle((21,13 if tall else 24,23,55),fill='#8cb98d')
    d.rectangle((38,20,40,36),fill='#74ab85'); d.rectangle((16,60,34,63),fill=ink)
    save(im,name)
for i in range(2):
    im=Image.new('RGBA',(64,40)); d=ImageDraw.Draw(im)
    d.polygon([(4,20),(19,14),(31,16),(43,12),(53,16),(62,21),(48,24),(41,29),(26,28),(17,24)],fill=ink)
    wing=[(23,20),(17,2),(31,9),(39,23)] if i==0 else [(23,19),(21,36),(36,28),(40,21)]
    d.polygon(wing,fill=ink); d.polygon([(x+2,y) for x,y in wing[:-1]],fill='#628a8c')
    d.rectangle((47,16,49,18),fill='#f8dd9c'); d.polygon([(54,20),(63,21),(54,23)],fill=coral)
    save(im,'Bird'+str(i),32,(.5,.5))
im=Image.new('RGBA',(24,28));d=ImageDraw.Draw(im)
d.polygon([(7,1),(17,1),(22,7),(22,20),(17,26),(7,26),(1,20),(1,7)],fill='#9d653c')
d.polygon([(8,3),(16,3),(20,8),(20,19),(16,24),(8,24),(4,19),(4,8)],fill='#f0bb58')
d.line([(8,6),(7,10),(7,18)],fill='#fff0b1',width=2);d.rectangle((11,8,14,19),fill='#bf833e')
save(im,'Coin',32,(.5,.5))
# Wide seamless parallax layers, with transparent sky.
for name,color,amp,base,periods in [('FarDunes','#e5bc91',42,65,2),('MidDunes','#dca67b',55,88,3),('NearDunes','#c98b67',30,104,4)]:
    im=Image.new('RGBA',(768,192));d=ImageDraw.Draw(im)
    points=[(x,int(base+amp*math.sin(x/768*math.tau*periods)+amp*.25*math.sin(x/768*math.tau*(periods+1)))) for x in range(769)]
    d.polygon(points+[(768,192),(0,192)],fill=color)
    save(im,name,32)
im=Image.new('RGBA',(128,128));d=ImageDraw.Draw(im);d.ellipse((2,2,126,126),fill='#f4ba75');d.ellipse((12,12,116,116),fill='#f8c885');save(im,'Sun',32,(.5,.5))
im=Image.new('RGBA',(256,64));d=ImageDraw.Draw(im);d.rectangle((0,0,255,63),fill='#e9bc86');d.rectangle((0,0,255,3),fill=ink);d.rectangle((0,4,255,8),fill='#b48060')
for _ in range(65):
 x=random.randrange(256);y=random.randrange(14,64);d.rectangle((x,y,x+random.randrange(2,7),y+1),fill=random.choice(['#d5a16e','#f7d5a0','#c69366']))
save(im,'Ground',32,(.5,1))
im=Image.new('RGBA',(128,64));d=ImageDraw.Draw(im)
for box in [(2,30,58,49),(25,12,78,48),(59,23,115,49)]: d.ellipse(box,fill='#faecd0')
save(im,'Cloud',32,(.5,.5))
im=Image.new('RGBA',(64,32));d=ImageDraw.Draw(im)
d.ellipse((1,6,62,29),fill=(35,70,75,35));save(im,'Shadow',32,(.5,.5))
im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im)
d.polygon([(16,0),(20,12),(31,16),(20,20),(16,31),(12,20),(0,16),(12,12)],fill='#fff0bd');save(im,'Spark',32,(.5,.5))
im=Image.new('RGBA',(8,8),'white');save(im,'Pixel',32,(.5,.5))
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
