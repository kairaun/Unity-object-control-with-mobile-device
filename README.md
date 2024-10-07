# Unity-object-control-with-mobile-device

實現在Runtime導入外部圖形格式(.obj、.glb、.gltf)，並透過UDP連線方式，在同一區網下使數據能夠連接，並從手機端(Client)控制Server端所讀取的物件行為。

Server：透過UI讀取物件，UI會新增出記錄該讀取物件信息的Button，該Button可透過右鍵刪除(同時刪除該物件)；所有讀取進場景的物件皆會被記錄以便下次開啟時還原。

Client：所有透過UI讀取的物件數據會被傳送到Dropdown中可選擇切換指定物件，並透過UI去控制物件的位移旋轉縮放等。

# Unity version

```
Unity2021.3.5f1
```

# Result

ServerToClient (ServerToClient Folder)

![image](https://github.com/kairaun/Unity-object-control-with-mobile-device/blob/main/pic/Server.jpg)  

ClientToServer (ClientToServer.unitypackage)

![image](https://github.com/kairaun/Unity-object-control-with-mobile-device/blob/main/pic/Client.jpg)  
