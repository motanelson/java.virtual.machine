import tkinter as tk
import os

global rets 
rets=""


class myapp:
    def __init__(self,root:tk.Tk,title:str,backgrounds:str,foregrounds:str,types:str):
        self.root=root
        self.root.files=None
        self.root.title=title
        self.root.configure(background=backgrounds)
        self.lists=tk.Listbox(background=backgrounds,foreground=foregrounds)
        self.lists.pack(padx=10,pady=10)
        self.oks=tk.Button(background=backgrounds,foreground=foregrounds,text="ok",command=self.selected)
        self.oks.pack(padx=10,pady=10)
        a=os.listdir(".")
        for b in a:
            if b.find(types)>-1:
                self.lists.insert(tk.END,b)
    def selected(self):
        global rets
        try:
            self.files=self.lists.selection_get()
            rets=self.files
            self.root.quit()
        except:
            print("error")

def starts(title:str="open file",backgrounds:str="black",foregrounds:str="white",types:str=".txt"):
    root=tk.Tk()
    apps=myapp(root,title=title,backgrounds=backgrounds,foregrounds=foregrounds,types=types)
    root.mainloop()
    return rets