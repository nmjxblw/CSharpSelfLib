import os
import pygame
import sys

class Game():
    """
    游戏基类
    """
    def __init__(self, title:str, width:int, height:int):
        """
        初始化
        """
        self.title = title
        self.width = width
        self.height = height
        pygame.init()
        
        return self
    
    def toString(self)->str:
        return f"{sys.version}"
    
if __name__ == "main":
    game = Game()
    print(f"{game.toString()}")
    os.system('pause')