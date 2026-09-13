from pathlib import Path
import re, json

root = Path('D:/TTRedesVR/NetworkSimulatorVR')
data = [
 ('Cat5e (Categoría 5e)', 'Evolucionó para corregir interferencias en la red, siendo el estándar más común para redes domésticas y de oficina que requieren velocidad Gigabit.', ['Hasta 1 Gbps','100 MHz','100 metros','Sin blindaje (UTP)','RJ45'], (0.2,0.72,0.38), 'Cat5e'),
 ('Cat6 (Categoría 6)', 'Ofrece un rendimiento superior al Cat5e con mejor protección contra interferencias y diafonía, soportando velocidades de 10 Gbps en distancias cortas.', ['Hasta 10 Gbps','250 MHz','55 metros (a 10 Gbps) / 100 metros (a 1 Gbps)','Sin blindaje (UTP)','RJ45'], (0.1,0.4,0.95), 'Cat6'),
 ('Cat6A (Categoría 6A o "Aumentada")', 'Diseñada para garantizar 10 Gbps en toda la distancia estándar de 100 metros, cumple con los estrictos estándares TIA y ofrece mayor margen de futuro.', ['Hasta 10 Gbps','500 MHz','100 metros','Con blindaje de lámina (SFTP)','RJ45'], (0.08,0.82,0.96), 'Cat6A'),
 ('Cat7 (Categoría 7)', 'Posee un ancho de banda y una protección contra interferencias extremadamente altos, aunque no es un estándar oficial de la industria (TIA) y tiene compatibilidad limitada.', ['Hasta 10 Gbps','600 MHz','100 metros','Blindaje individual por par y general (SFTP)','GG45 o TERA (no RJ45)'], (0.55,0.32,0.9), 'Cat7')
]
labels = ['Velocidad','Ancho de banda','Distancia máxima','Estructura','Conector']
descriptions = [intro+'\n\n'+'\n'.join('• <b>'+label+':</b> '+value for label,value in zip(labels,values)) for _,intro,values,_,_ in data]
guids = ['5da56c9853e34a7096a5a67482489a0'+str(i) for i in range(4)]
q = lambda s: json.dumps(s, ensure_ascii=True)
scene = root/'Assets/Scenes/Modulo1.unity'
s = scene.read_text(encoding='utf-8-sig')
blocks = re.split(r'(?=^--- !u!)',s,flags=re.M)
def edit(fid, changes):
    for i,b in enumerate(blocks):
        if re.match(r'--- !u!\d+ &'+str(fid)+r'\n',b):
            for key,value in changes.items():
                b,n = re.subn(r'^  '+re.escape(key)+r':[^\n]*',lambda _: '  '+key+': '+str(value), b,flags=re.M)
                assert n == 1,(fid,key,n)
            blocks[i] = b
            return
    raise ValueError(fid)
def rect(fid,x,y,w,h):
    edit(fid, {'m_AnchoredPosition':f'{{x: {x}, y: {y}}}', 'm_SizeDelta':f'{{x: {w}, y: {h}}}'})
rect(10597226,-300,0,280,410)
rect(955340934,0,-30,260,260)
rect(1149308640,0,145,260,105)
rect(191839393,150,0,540,410)
rect(1787204621,-365,-245,105,105)
rect(1193018704,365,-245,105,105)
edit(191839395,{'m_fontSize':23,'m_fontSizeBase':23,'m_HorizontalAlignment':1,'m_VerticalAlignment':256,'m_RaycastTarget':0,'m_text':q(descriptions[0])})
edit(1149308642,{'m_fontSize':28,'m_fontSizeBase':28,'m_enableAutoSizing':1,'m_fontSizeMin':22,'m_fontSizeMax':28,'m_RaycastTarget':0,'m_text':q('<b>'+data[0][0]+'</b>')})
edit(955340936,{'m_Sprite':f'{{fileID: 21300000, guid: {guids[0]}, type: 3}}','m_Color':'{r: 1, g: 1, b: 1, a: 1}','m_PreserveAspect':1,'m_RaycastTarget':0})
for i,b in enumerate(blocks):
    if re.search(r'^  m_Name: Wire_\d+',b,re.M):
        blocks[i] = b.replace('  m_IsActive: 1','  m_IsActive: 0')
    if b.startswith('--- !u!114 &754472232\n'):
        opts = '  options:\n'
        for j,(name,_,_,color,_) in enumerate(data):
            opts += f'  - displayName: {q(name)}\n    description: {q(descriptions[j])}\n    accentColor: {{r: {color[0]}, g: {color[1]}, b: {color[2]}, a: 1}}\n    image: {{fileID: 21300000, guid: {guids[j]}, type: 3}}\n'
        blocks[i] = b[:b.index('  options:\n')] + opts
scene.write_text(''.join(blocks),encoding='utf-8')

# Keep the editor rebuild command consistent with the saved scene.
p = root/'Assets/Editor/Module01CableSelectorSetup.cs'
t = p.read_text(encoding='utf-8-sig')
t = t.replace('new Vector2(0, 160), new Vector2(650, 86)', 'new Vector2(150, 0), new Vector2(540, 410)')
t = t.replace('new Vector2(0, -20), new Vector2(510, 270)', 'new Vector2(-300, 0), new Vector2(280, 410)')
t = t.replace('new Vector2(0, 35), new Vector2(310, 76)', 'new Vector2(0, -30), new Vector2(260, 260)')
t = t.replace('        AddCableLines(preview.rectTransform);', '        preview.preserveAspect = true;\n        preview.raycastTarget = false;\n        description.alignment = TextAlignmentOptions.TopLeft;\n        description.richText = true;\n        description.raycastTarget = false;')
t = t.replace('new Vector2(0, -82), new Vector2(430, 62), 38', 'new Vector2(0, 145), new Vector2(260, 105), 28')
t = t.replace('        Button previous =', '        name.enableAutoSizing = true;\n        name.fontSizeMin = 22;\n        name.fontSizeMax = 28;\n        name.raycastTarget = false;\n\n        Button previous =')
t = t.replace('new Vector2(-365, -15)', 'new Vector2(-365, -245)').replace('new Vector2(365, -15)', 'new Vector2(365, -245)')
options = []
for i,(name,_,_,color,asset) in enumerate(data):
    options.append('                Option('+q(name)+', '+q(descriptions[i])+', new Color('+', '.join(str(c)+'f' for c in color)+'), '+q(asset)+')')
t = re.sub(r'                Option\("Cat 5e".*?\n            \}\);', lambda _: ',\n'.join(options)+'\n            });',t,flags=re.S)
t = t.replace('string description, Color color) =>', 'string description, Color color, string imageName) =>')
t = t.replace('description = description, accentColor = color };', 'description = description, accentColor = color,\n            image = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/CableCards/{imageName}.png") };')
p.write_text(t,encoding='utf-8')
(root/'tmp/cable-cards/assets.json').write_text(json.dumps([{'name':row[4], 'guid':guids[i]} for i,row in enumerate(data)]))
print('Updated four cable options, card layout, and editor builder.')
