import jpype
import jpype.imports
from tkinter import filedialog

print("\033c\033[47;30m")
print("give me a .class to open?")
a=filedialog.askopenfile(title="give me the .video pack file ?",defaultextension="*.video")
a=a.name

if not jpype.isJVMStarted():
    jpype.startJVM(classpath=["."])
a=a.replace(".class","")
a=a.replace("\\","/")

aa=0
while(1):
    f=a.find("/")
    if f>-1:
       a=a[f+1:]
    else:
       break  
 
a=a.strip()
print("##################################################")
print(a)

Hello = jpype.JClass(a)

Hello.main([])

jpype.shutdownJVM()
