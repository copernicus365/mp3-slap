import subprocess

arr = [
    (0, "NIV-Suchet-01-Genesis.mp3", "C:/Dropbox/Vids/mp3-split/mp3-split/test1/NIV-Suchet-01-Genesis.mp3", "C:/Dropbox/Vids/mp3-split/mp3-split/test1/logs5/log#niv-suchet-01-genesis.mp3#silencedetect.log", "print('hi!')")
]

for i in arr:
  idx = i[0]
  fname = i[1]
  fileP = i[2]
  logP = i[3]
  script = i[4]

  print(f"--- i: {idx} run silence detect on: '{fname}' ---")

  print(f" -log: '{logP}")

  
  subprocess.call(script, shell=True)
