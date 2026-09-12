src = open(r'PCL.Core/Utils/OS/EnvironmentInterop.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('SecretDictionary')
print(src[max(0,i-200):i+800])
