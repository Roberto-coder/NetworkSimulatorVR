"""Figuras de documentación; SVG editables y PNG. Requiere Pillow.
Ejecutar desde cualquier directorio. No modifica escenas ni el documento fuente.
"""
from pathlib import Path
from xml.sax.saxutils import escape
from collections import deque
import csv
import json
import math
import re
from PIL import Image, ImageDraw, ImageFont

BASE = Path(__file__).resolve().parent
ROOT = BASE.parent.parent
OUT = BASE / 'figuras'
OUT.mkdir(exist_ok=True)
INK = '#172d44'
BLUE = '#23658c'
TEAL = '#127a72'
RED = '#b13d32'
GRAY = '#58687a'
LIGHT = '#eef5fa'
FONTS = Path('C:/Windows/Fonts')

class Figure:
    def __init__(self, name, w, h, title, sub):
        self.name, self.w, self.h = name, w, h
        self.im = Image.new('RGB', (w*2, h*2), 'white')
        self.d = ImageDraw.Draw(self.im)
        self.svg = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">', '<rect width="100%" height="100%" fill="white"/>']
        self.text(40, 35, title, 32, bold=True)
        self.text(40, 85, sub, 20, color=GRAY)

    def text(self, x, y, text, size=21, color=INK, bold=False, center=False):
        font = ImageFont.truetype(str(FONTS / ('arialbd.ttf' if bold else 'arial.ttf')), size*2)
        for n, s in enumerate(text.split('\n')):
            yy = y+n*(size+8)
            xx = x-self.d.textlength(s, font=font)/4 if center else x
            self.d.text((round(xx*2), yy*2), s, font=font, fill=color)
            anchor = 'middle' if center else 'start'
            self.svg.append(f'<text x="{x}" y="{yy+size}" text-anchor="{anchor}" font-family="Arial, sans-serif" font-size="{size}" font-weight="{700 if bold else 400}" fill="{color}">{escape(s)}</text>')

    def box(self,x,y,w,h,label='',fill=LIGHT,stroke=BLUE,size=21):
        self.d.rounded_rectangle((x*2,y*2,(x+w)*2,(y+h)*2),radius=14,fill=fill,outline=stroke,width=4)
        self.svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="7" fill="{fill}" stroke="{stroke}" stroke-width="2"/>')
        if label:
            count=len(label.split('\n'))
            self.text(x+w/2,y+(h-count*(size+8))/2,label,size,center=True)

    def line(self, pts, color=BLUE, width=3, dash=False, arrow=False):
        if not dash:
            self.d.line([(round(x*2),round(y*2)) for x,y in pts], fill=color,width=width*2)
        else:
            for a,b in zip(pts,pts[1:]):
                length=math.dist(a,b)
                for t in range(0,int(length),18):
                    p=(a[0]+(b[0]-a[0])*t/length,a[1]+(b[1]-a[1])*t/length)
                    u=min(t+10,length)
                    q=(a[0]+(b[0]-a[0])*u/length,a[1]+(b[1]-a[1])*u/length)
                    self.d.line([(p[0]*2,p[1]*2),(q[0]*2,q[1]*2)],fill=color,width=width*2)
        attr=' stroke-dasharray="10 8"' if dash else ''
        self.svg.append(f'<polyline points="{" ".join(f"{x},{y}" for x,y in pts)}" fill="none" stroke="{color}" stroke-width="{width}"{attr}/>')
        if arrow:
            a,b=pts[-2:]; ang=math.atan2(b[1]-a[1],b[0]-a[0]); r=12
            p=[b,(b[0]-r*math.cos(ang-.45),b[1]-r*math.sin(ang-.45)),(b[0]-r*math.cos(ang+.45),b[1]-r*math.sin(ang+.45))]
            self.d.polygon([(x*2,y*2) for x,y in p], fill=color)
            self.svg.append(f'<polygon points="{" ".join(f"{x},{y}" for x,y in p)}" fill="{color}"/>')

    def circle(self,x,y,r=18,fill='white',stroke=BLUE):
        self.d.ellipse(((x-r)*2,(y-r)*2,(x+r)*2,(y+r)*2),fill=fill,outline=stroke,width=5)
        self.svg.append(f'<circle cx="{x}" cy="{y}" r="{r}" fill="{fill}" stroke="{stroke}" stroke-width="2.5"/>')

    def save(self):
        self.line([(40,self.h-70),(self.w-40,self.h-70)],color='#c6d1dc',width=1)
        self.text(40,self.h-48,'Elaboración propia · Esquema basado en el proyecto; no es una captura de Unity.',17,color=GRAY)
        self.im.save(OUT/(self.name+'.png'),dpi=(240,240))
        (OUT/(self.name+'.svg')).write_text('\n'.join(self.svg+['</svg>']),encoding='utf-8')

def physics():
    f=Figure('F-A-puntos-y-joints',1500,830,'F-A · Estructura física y visual del cable','El Point es un objeto físico; el SpringJoint es un componente que enlaza dos Rigidbody.')
    coords=[(180,260),(470,380),(780,420),(1080,320)]
    for (x,y),(xx,yy) in zip(coords,coords[1:]):
        f.line([(x,y),(xx,yy)],color='#a4d4ca',width=22)
        vx,vy=xx-x,yy-y; length=math.hypot(vx,vy); nx,ny=-vy/length,vx/length
        zig=[]
        for i in range(19):
            t=i/18; amp=0 if i in (0,18) else (10 if i%2 else -10)
            zig.append((x+vx*t+nx*amp,y+vy*t+ny*amp))
        f.line(zig,color=BLUE,width=3)
    for i,(x,y) in enumerate(coords):
        f.circle(x,y,19,fill='white')
        f.text(x,y+35,['Extremo A','Point 0','Point 1','Extremo B'][i],23,bold=True,center=True)
        f.text(x,y+69,'Rigidbody',19,center=True)
    f.box(1200,245,235,130,'Puerto\nRigidbody fijo',size=23)
    f.line([(1100,320),(1200,310)],color=RED,width=4)
    f.text(1135,200,'FixedJoint',20,color=RED,center=True)
    f.line([(1135,229),(1150,310)],color=RED,width=2,arrow=True)
    f.text(435,170,'SpringJoint',24,bold=True,color=BLUE)
    f.text(435,205,'connectedBody → cuerpo vecino',20)
    f.line([(575,237),(600,397)],width=2,arrow=True)
    f.text(880,540,'Tramo visual (connector)',23,bold=True,color=TEAL)
    f.text(880,575,'Posición media, orientación y escala',20)
    f.line([(1070,530),(950,371)],color=TEAL,width=2,arrow=True)
    f.box(50,650,660,70,'Anclajes locales = (0,0,0) · distancia base = space',size=21)
    f.box(745,650,700,70,'Base: spring = 200 · damper = 0.2 (pueden ajustarse)',size=21)
    f.save()

def states():
    f=Figure('F-B-estados-modulo1',1500,960,'F-B · Fabricación del cable en el módulo 1','La misma máquina se aplica de forma independiente a Left y Right.')
    xs=[45,340,635,930,1225]
    labels=['Whole\nEntero','Peeled\nPelado','Rj45Disordered\nHilos sin ordenar','Rj45Ordered\nOrden correcto','Rj45Crimped\nPonchado']
    for x,label in zip(xs,labels): f.box(x,220,225,100,label,size=22)
    f.circle(55,165,9,fill=INK,stroke=INK)
    f.line([(55,175),(100,220)],arrow=True)
    for i,label in enumerate(['Pelar','Tomar y soltar','Validar T568B','Crimpar']):
        f.line([(xs[i]+225,270),(xs[i+1],270)],arrow=True)
        f.text(xs[i]+255,170,label,18,center=True)
    f.line([(747,320),(747,390),(820,390),(820,320)],color=RED,arrow=True)
    f.text(795,415,'Orden incorrecto: conservar estado y corregir',21,color=RED,center=True)
    f.box(45,485,1405,82,'Guardas: objetivo activo + extremo correcto + estado actual + condición de la acción.',size=22)
    f.text(50,605,'Validación del cable completo (flujo del módulo)',25,bold=True)
    f.box(50,665,420,100,'Left y Right en Rj45Crimped\nCanTest = true',size=22)
    f.box(550,665,380,100,'Ambos extremos\nconectados al tester',size=22)
    f.box(1010,665,440,100,'Ejecutar prueba autorizada\nCableEvents.CableValidated',fill='#e9f5f1',stroke=TEAL,size=22)
    f.line([(470,715),(550,715)],arrow=True)
    f.line([(930,715),(1010,715)],arrow=True)
    f.text(50,810,'La prueba no agrega otro valor a CableState; confirma condiciones y completa el objetivo.',21,color=GRAY)
    f.save()

def topology():
    f=Figure('F-C-topologia-modulo3',1600,1110,'F-C · Red de oficina: topología y fallas iniciales','Segmento office · LAN 192.168.10.0/24 · Sin gateway en el modelo de diagnóstico.')
    f.box(45,145,240,90,'Laptop/eth0\n192.168.10.100/24',size=21)
    f.line([(165,235),(165,290)],width=4)
    f.text(300,245,'LaptopLink',18)
    f.box(45,290,240,560,'SW-01\nPuertos modelados:\nP01, P02, P03, P08\n\nConmutación interna\nentre habilitados\n\nmgmt:\n192.168.10.2/24',size=21)
    f.text(55,300,'P08',19,color=TEAL)
    f.text(455,145,'Patch panel PP-01',23,bold=True)
    f.text(885,145,'Rosetas',23,bold=True)
    f.text(1335,145,'Equipos',23,bold=True)
    for i,y in enumerate([365,575,785],1):
        f.text(295,y-75,f'P0{i}',21,bold=True,color=RED if i==2 else INK)
        f.box(415,y-42,135,84,f'front0{i}',size=20)
        f.box(640,y-42,135,84,f'back0{i}',size=20)
        f.box(885,y-42,135,84,f'R0{i}\nback',size=20)
        f.box(1095,y-42,135,84,f'R0{i}\nfront',size=20)
        ip='11' if i==1 else '12'
        f.box(1340,y-50,215,100,f'PC-0{i}/eth0\n192.168.10.{ip}/24',fill='#fff0ec' if i>1 else LIGHT,size=20)
        f.line([(285,y),(415,y)],color=RED if i==2 else BLUE,width=4)
        f.text(350,y-35,f'Rack-0{i}',17,center=True)
        f.line([(550,y),(640,y)],color=TEAL,dash=True)
        f.line([(775,y),(885,y)],width=4)
        f.text(830,y-76,f'Horizontal\n-0{i}',17,center=True)
        f.line([(1020,y),(1095,y)],color=TEAL,dash=True)
        f.line([(1230,y),(1340,y)],color=RED if i==1 else BLUE,width=4)
        f.text(1285,y-35,f'Patch-0{i}',17,center=True)
        if i==1:
            f.text(1285,y+64,'Averiado',20,color=RED,center=True)
            f.line([(1270,y-12),(1290,y+12)],color=RED,width=4)
            f.line([(1270,y+12),(1290,y-12)],color=RED,width=4)
        if i==2: f.text(350,y+64,'P02 deshabilitado',19,color=RED,center=True)
        if i>1: f.text(1440,y+64,'IP duplicada',20,color=RED,center=True)
    f.box(45,900,650,75,'Replacement-01: íntegro, desconectado al inicio.',fill='#e9f5f1',stroke=TEAL,size=21)
    f.line([(760,914),(820,914)],width=4);f.text(840,899,'Cable',20)
    f.line([(1020,914),(1080,914)],color=TEAL,dash=True); f.text(1100,899,'Continuidad pasiva',20)
    f.text(760,958,'Rojo + texto: condición de falla inicial.',20,color=RED)
    f.text(45,1000,'El switch se resume como bloque: el algoritmo utiliza sus puertos como vértices.',21,color=GRAY)
    f.save()

def read_definition():
    # Subconjunto YAML serializado por Unity: listas de objetos con campos escalares.
    src=(ROOT/'Assets/GameData/Module03/Topologies/OfficeInitialState.asset').read_text(encoding='utf-8')
    groups={}; current=None; item=None
    for line in src.splitlines():
        m=re.fullmatch(r'    (devices|ports|cables|passiveConnections):',line)
        if m:
            current=m[1]; groups[current]=[];item=None;continue
        if re.match(r'    [A-Za-z]',line): current=None
        if current is None: continue
        m=re.match(r'    - (\w+):\s*(.*)',line)
        if m:
            item={};groups[current].append(item)
        else: m=re.match(r'      (\w+):\s*(.*)',line)
        if m and item is not None: item[m[1]]=m[2].strip().strip("'")
    return groups

def traverse(net, repaired=False):
    ports={p['id']:dict(p) for p in net['ports']}
    if repaired: ports['SW-01/P02']['enabled']='1'
    graph={p:[] for p in ports}
    def link(a,b):
        if not a or not b:return
        if ports[a]['enabled']!='1' or ports[b]['enabled']!='1' or ports[a]['segmentId']!=ports[b]['segmentId']:return
        graph[a].append(b);graph[b].append(a)
    for c in net['cables']:
        if c['id']=='Patch-01' and repaired: link('R01/front','PC-01/eth0')
        elif c['intact']=='1':link(c['portA'],c['portB'])
    for p in net['passiveConnections']:link(p['portA'],p['portB'])
    sw=sorted(p for p in ports if ports[p]['deviceId']=='SW-01')
    for i,a in enumerate(sw):
        for b in sw[i+1:]:link(a,b)
    queue=deque(['Laptop/eth0']);depth={'Laptop/eth0':0};parents={};order=[]
    while queue:
        a=queue.popleft();order.append(a)
        for b in sorted(graph[a]):
            if b in depth:continue
            depth[b]=depth[a]+1;parents[b]=a;queue.append(b)
    return depth,parents,order

def bfs(net):
    f=Figure('F-D-bfs-modulo3',1550,1100,'F-D · BFS desde Laptop/eth0','Caso reparado: Replacement-01 conectado, P02 habilitado y PC-03 con 192.168.10.13/24.')
    depth,parents,order=traverse(net,True)
    for level in range(8):
        y=145+level*79
        f.box(45,y,140,57,f'Nivel {level}',size=20)
        ids=[p for p in order if depth[p]==level]
        for i,p in enumerate(ids):
            f.box(225+i*320,y,298,57,p,size=19,fill='#e9f5f1' if level==7 else LIGHT,stroke=TEAL if level==7 else BLUE)
        if level<7:f.line([(110,y+57),(110,y+79)],arrow=True,width=2)
    f.text(45,805,'Parents permite reconstruir el camino hacia PC-01',24,bold=True)
    f.text(45,850,'Laptop/eth0 → SW-01/P08 → SW-01/P01 → PP-01/front01 → PP-01/back01',22)
    f.text(45,890,'→ R01/back → R01/front → PC-01/eth0   (último cable: Replacement-01)',22)
    f.text(45,945,'La cola sigue explorando: se necesitan todos los alcanzables para detectar IP duplicadas.',21,color=GRAY)
    f.text(45,985,'Los niveles cuentan aristas del modelo; no representan TTL ni saltos de router.',21,color=GRAY)
    f.save()
    initial,_,_=traverse(net)
    assert len(net['devices'])==9 and len(net['ports'])==21 and len(net['cables'])==11
    assert len(net['passiveConnections'])==6
    assert len(depth)==21 and depth['PC-01/eth0']==7
    assert 'PC-01/eth0' not in initial and 'PC-02/eth0' not in initial and 'PC-03/eth0' in initial
    report={'initial_reachable_count':len(initial),'repaired_reachable_count':len(depth),'repaired_bfs_levels':{str(n):[p for p in order if depth[p]==n] for n in range(8)}}
    (BASE/'verificacion-topologia.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')

def connections():
    f=Figure('F-E-conexiones-modulo2',1500,1030,'F-E · Detección y validación de conexiones','Compatibilidad local del conector y cumplimiento global del plan son condiciones distintas.')
    f.box(50,155,650,115,'Puerto con Module02NetworkSocket (XRI)\nHover / selección y pose de la punta',size=23)
    f.box(800,155,650,115,'Puerto sin XRSocketInteractor\nTrigger + CableEndSocketDetector al soltar',size=23)
    f.line([(375,270),(375,315),(750,315),(750,350)],arrow=True)
    f.line([(1125,270),(1125,315),(750,315)],arrow=False)
    f.box(270,350,960,105,'Compatibilidad local\nTipo de cable/puerto · ocupación · Male/Female · color si se exige · etapa',size=22)
    f.line([(750,455),(750,500)],arrow=True)
    f.box(270,500,960,100,'Unión física: Connector.Connect\nAlinear connectionPoint · FixedJoint · estabilizar Rigidbody',size=23)
    f.line([(750,600),(750,645)],arrow=True)
    f.box(270,645,960,100,'Registro lógico: PatchCableLink → Module02CablingState\nComparar puertos y tipos contra Module02ConnectionPlan',size=23)
    f.line([(750,745),(750,790)],arrow=True)
    f.box(270,790,960,105,'Cinco enlaces correctos → bloquear conexiones y cuerpos\nDespués: diez etiquetas → consola → encendido → configuración',fill='#e9f5f1',stroke=TEAL,size=23)
    f.text(50,925,'Un trigger detecta un candidato; las condiciones anteriores determinan si la operación es válida.',21,color=GRAY)
    f.save()

def inventories(net):
    def write(name,fields,rows):
        with (BASE/name).open('w',newline='',encoding='utf-8-sig') as out:
            writer=csv.DictWriter(out,fieldnames=fields);writer.writeheader();writer.writerows(rows)
    write('inventario-puertos-modulo3.csv',list(net['ports'][0]),net['ports'])
    write('inventario-enlaces-modulo3.csv',list(net['cables'][0]),net['cables'])
    kinds={'0':'Computadora','1':'Switch','2':'Elemento pasivo','3':'Laptop'}
    rows=[]
    for d in net['devices']:
        ports=[p for p in net['ports'] if p['deviceId']==d['id']]
        rows.append({'id':d['id'],'tipo':kinds[d['kind']],'prefabKey':d['prefabKey'],'puertos':'; '.join(p['id'] for p in ports),'IPv4_inicial':'; '.join(p['ipv4']+'/24' for p in ports if p['ipv4']),'observacion':'P02 deshabilitado' if d['id']=='SW-01' else 'IP duplicada' if d['id'] in ('PC-02','PC-03') else ''})
    write('inventario-dispositivos-modulo3.csv',list(rows[0]),rows)
    write('continuidades-pasivas-modulo3.csv',['portA','portB'],net['passiveConnections'])

if __name__=='__main__':
    net=read_definition()
    physics();states();topology();bfs(net);connections();inventories(net)
    print('Generadas 5 figuras SVG + 5 PNG, 4 CSV y verificación de topología.')
