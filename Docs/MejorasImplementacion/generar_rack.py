"""Vista frontal esquemática del rack integrado. No asigna posiciones U no verificadas."""
from generar_figuras import Figure, BLUE, TEAL, GRAY, INK

f=Figure('F-F-rack-actualizado',1600,1900,'Rack de comunicaciones · Módulo 2',
         'Vista frontal esquemática del montaje previsto al completar la instalación de SW1.')

# Centros documentados en el marco del modelo. Las U son referencias nominales.
items=[
 ('PDU horizontal · PDU-A',1.86107,1,'power','Alimentación del switch principal'),
 ('Almacenamiento de red · NAS',1.75076,2,'nas','Equipo de reconocimiento'),
 ('Panel de distribución de fibra',1.34697,1,'fiber','Terminación y organización de fibra'),
 ('Organizador horizontal superior',1.28897,1,'organizer','Organización del cableado'),
 ('Patch panel UTP · PP-A',1.10205,1,'panel','Puertos /01, /02 y /03 del plan de conexión'),
 ('Switch secundario · SW2',1.00182,1,'switch','Equipo fijo · enlace SW2/Gi04 ↔ SW1/Gi04'),
 ('Router',0.88407,1,'router','Equipo de reconocimiento'),
 ('Organizador horizontal inferior',0.81897,1,'organizer','Elemento provisional de organización'),
 ('Unidad de ventilación',0.69784,2,'fan','Infraestructura de ventilación'),
 ('Switch principal · SW1',0.60896,1,'switch-main','Posición final de instalación del equipo manipulable'),
 ('Servidor',0.40614,2,'server','Equipo de reconocimiento'),
 ('Sistema de alimentación ininterrumpida · UPS',0.17606,2,'ups','Respaldo eléctrico · zona inferior'),
]
left,right=330,800
f.box(left-36,145,right-left+72,1480,fill='#fafbfd',stroke=INK)
f.box(left-20,160,15,1450,fill='#e0e6ec',stroke=GRAY)
f.box(right+5,160,15,1450,fill='#e0e6ec',stroke=GRAY)
for y in range(175,1600,22):
    f.circle(left-12,y,3,fill='white',stroke=GRAY)
    f.circle(right+12,y,3,fill='white',stroke=GRAY)

# PDU posterior representada a un lado para que pueda identificarse.
f.box(220,370,46,780,fill='#e5e9ee',stroke=GRAY)
for y in range(400,1130,60):f.box(230,y,26,35,fill='#435364',stroke=GRAY)
f.text(40,520,'PDU vertical',23,bold=True)
f.text(40,555,'0U · lateral posterior',19,color=GRAY)
f.text(40,585,'Representada fuera',18,color=GRAY)
f.text(40,612,'del frente para',18,color=GRAY)
f.text(40,639,'facilitar su lectura.',18,color=GRAY)
f.line([(95,680),(185,680),(220,720)],color=GRAY,width=2)

for label,height,u,kind,desc in items:
    y=170+(1.92-height)*760
    h=u*0.04445*760
    special=kind=='switch-main'
    edge=TEAL if special else GRAY
    face='#e1f1ed' if special else '#e5e9ee'
    f.box(left,y-h/2,right-left,h,fill=face,stroke=edge)
    for x in (left+10,right-10):f.circle(x,y,3,stroke=edge)
    if kind in ('panel','fiber','switch','switch-main'):
        n=16 if kind=='panel' else 12
        for i in range(n):
            x=left+36+i*24
            f.box(x,y-5,15,10,fill='#374b5c',stroke=edge)
        f.circle(right-40,y,4,fill=TEAL,stroke=TEAL)
    elif kind=='organizer':
        for i in range(12):
            x=left+38+i*32
            f.line([(x,y-10),(x,y+10)],color=GRAY,width=3)
        f.line([(left+35,y),(right-35,y)],color=GRAY,width=4)
    elif kind=='fan':
        for x in (left+115,left+235,left+355):
            f.circle(x,y,22,fill='#f5f7fa',stroke=GRAY)
            f.circle(x,y,7,fill=GRAY,stroke=GRAY)
            for dx,dy in [(17,0),(0,17),(-17,0),(0,-17)]:
                f.line([(x,y),(x+dx,y+dy)],color=GRAY,width=3)
    elif kind in ('server','nas'):
        for i in range(7):
            f.box(left+38+i*43,y-20,32,40,fill='#697888',stroke=GRAY)
        f.circle(right-45,y,7,fill=TEAL,stroke=TEAL)
    elif kind=='ups':
        f.box(left+40,y-19,100,38,fill='#5d6e7e',stroke=GRAY)
        for i in range(14):
            x=left+190+i*15
            f.line([(x,y-18),(x,y+18)],color=GRAY,width=2)
    elif kind=='power':
        for i in range(8):
            f.box(left+45+i*46,y-8,23,16,fill='#4e6072',stroke=GRAY)
    elif kind=='router':
        for i in range(6):f.box(left+40+i*30,y-5,19,10,fill='#4e6072',stroke=GRAY)
        for i in range(3):f.circle(right-100+i*18,y,3,fill=TEAL,stroke=TEAL)
    label_y=y-10 if kind=='fiber' else y+10 if label=='Organizador horizontal superior' else y
    f.line([(right+22,y),(845,y),(865,label_y)],color=TEAL if special else '#9aa8b6',width=2)
    f.text(885,label_y-22,f'{label}  ·  {u}U',22,bold=special,color=TEAL if special else INK)
    f.text(885,label_y+8,desc,17,color=GRAY)

f.text(565,430,'Separaciones entre equipos',18,color=GRAY,center=True)
f.text(565,460,'según la distribución del modelo',18,color=GRAY,center=True)
f.box(45,1675,1510,90,
      'Verde: SW1 en su posición final de montaje. El equipo inicia fuera del rack.\nLas U corresponden a las fichas del proyecto; no se asignan números de posición U.',size=21)
f.text(45,1790,'Fuente: montaje integrado de Modulo2 y fichas de componentes. Vista sin cableado para facilitar la identificación.',18,color=GRAY)
f.save()
print('Rack actualizado generado en PNG y SVG.')
