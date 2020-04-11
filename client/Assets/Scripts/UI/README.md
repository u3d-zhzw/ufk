# uguiext
包装unity ugui解决常见的细节问题


Sprite
----
1. 运行时加载贴图出现的“闪烁”。
如下图加载物品图标，由于是异步加载图标，物品图标会“闪一下图2白块“
解决：
1. 配置默认图标
2. 当spriteName = nil时，移除网络顶点

![](https://raw.githubusercontent.com/lizijie/MyImageHosting/master/uguiext/sprite_icon_1.png)
![](https://raw.githubusercontent.com/lizijie/MyImageHosting/master/uguiext/sprite_icon_2.png)

2. 扫描非tile类型警告。日常构建流程扫描工程界面prefab，检查这类型警告
![](https://raw.githubusercontent.com/lizijie/MyImageHosting/master/uguiext/sprite_tile_issue.png)

其它
https://lizijie.github.io/2019/05/23/ugui%E9%9D%9E%E5%B8%B8%E4%B8%8D%E5%8F%8B%E5%A5%BD.html

Button
1. 增加buttonId，以允许全局监听按钮事件。比强制流程、数据埋点。
