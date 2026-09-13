from pathlib import Path
import re, json, uuid
root = Path('D:/TTRedesVR/NetworkSimulatorVR')
template = (root/'Assets/Recursos/ToolWheel/Models/TesterTextures/cabletester.png.meta').read_text()
for asset in json.loads((root/'tmp/cable-cards/assets.json').read_text()):
    t=template
    for key,value in {'guid':asset['guid'],'spriteMode':1,'textureType':8,'nPOTScale':0,'alphaIsTransparency':1,'spriteMeshType':1,'wrapU':1,'wrapV':1,'spriteGenerateFallbackPhysicsShape':0}.items():
        t = re.sub(r'^(\s*)'+key+r':[^\n]*',lambda m:m[1]+key+': '+str(value),t,flags=re.M)
    t=re.sub(r'(spriteID:) [^\n]*',lambda m:m[1]+' '+uuid.uuid4().hex,t)
    (root/f"Assets/Art/CableCards/{asset['name']}.png.meta").write_text(t)
for folder in ['Assets/Art','Assets/Art/CableCards']:
    p=root/(folder+'.meta')
    if not p.exists(): p.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
print('Four single-sprite imports configured.')
