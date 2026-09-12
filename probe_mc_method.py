src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 方法定义
import re
for m in re.finditer(r'(?:public|private|internal).*CompProjectsGet', src):
    print(f'at {m.start()}: {src[m.start():m.start()+200]}')
# 找方法内的结果处理（CurseForge data 解析）
i = src.find('CompProjectsGet', 74102)
if i > 0:
    # 找方法结束（下一个 public/private 方法）
    j = src.find('\n        public ', i+100)
    if j < 0: j = src.find('\n        private ', i+100)
    print(f'Method ends approx at {j}')
    # 在方法范围内找 bedrockCategoryFilter 使用
    k = src.find('bedrockCategoryFilter', i)
    while k > 0 and k < j:
        print(f'bedrockCategoryFilter used at {k}: {src[max(0,k-50):k+150]}')
        k = src.find('bedrockCategoryFilter', k+1)
