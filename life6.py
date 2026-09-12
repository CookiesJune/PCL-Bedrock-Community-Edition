import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
# Run() 在 1877 附近，读 1700-2350
seg = src[1650:2400]
print(seg.replace(chr(13),''))
