import jpype
import jpype.imports

print("\033c\033[47;30m")
print("give me a .class to open?")
a = "Hello.class"
a=input().strip()
if not jpype.isJVMStarted():
    jpype.startJVM(classpath=["."])
a=a.replace(".class","")
Hello = jpype.JClass(a)

Hello.main([])

jpype.shutdownJVM()#a=input().strip()
