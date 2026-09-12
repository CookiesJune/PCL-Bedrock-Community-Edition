import subprocess
r = subprocess.run(['grep', '-rn', 'public.*DlModRequest', r'PCL2-CE-official/Plain Craft Launcher 2/Modules/'], capture_output=True, text=True)
print(r.stdout)
