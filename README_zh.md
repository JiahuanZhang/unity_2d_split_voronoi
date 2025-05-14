
![image](https://github.com/user-attachments/assets/2007fb25-0635-4589-a53f-6126cf10b2fe)

# 简介
在unity中使用Voronoi算法，生成2D网格，实现2D破碎效果。

支持功能：1.预处理生成网格数据；2.实时生成网格；

参考了[这篇csdn文章](https://blog.csdn.net/w1594731007/article/details/89705489 )的算法实现,我添加了构造晶胞，并生成网格，生成UV数据的功能。

# 如何使用

## 范例1：运行 CreateMesh 场景
提供功能：
1. 预处理生成网格数据；
2. 自定义修改生成的网格数据；

### 生成网格
* 修改 CreateMesh 场景下的 VoronoiCreator 节点中的参数，运行场景后点击`Save Mesh` 生成网格。
* 在此期间可以删除/合并网格，选中任意一个或多个网格，进行1)删除操作：在Inspector界面中选择`Delete Mesh`. 2)合并操作：选中多个网格，然后在Inspector界面中选择`Combine Meshes`.
  
重要参数解释：
* **Is Alpha Test Enable**: 是否开启Alpha测试，如果开启，则会丢弃贴图中透明区域的网格。
* **Uv**: uv 范围，默认(1.0,1.0),生成网格的uv范围为(0,0)到(1.0,1.0)。
* **Seed**: 随机种子，默认为0时会随机生成一个新种子，也可以手动输入一个种子。
* **Mesh Size**: 生成的网格尺寸；
* **Point Count X/Y**: X/Y方向上voronoi特征点数，数值越大，生成的网格越多；
* **Max Offset X/Y**: X/Y方向上voronoi特征点最大偏移量，数值越大，生成的网格越不规则；


## 范例2：运行 MeshModifer 场景
提供功能：
修改范例1中生成的网格，包括：
1. 网格模式：合并，删除网格；
2. 顶点模式：调整网格的顶点位置；

### 1. 网格模式
* 运行场景VoronoiModifier
* 在Game界面中选择切换为：Modify Mesh
* 在Scene界面中选中需要编辑的网格，进行1)删除操作：在Inspector界面中选择`Delete Mesh`. 2)合并操作：选中多个网格，然后在Inspector界面中选择`Combine Meshes`.

### 2. 顶点模式
* 运行场景VoronoiModifier
* 在Game界面中选择切换为：Modify Vertex
* 在Scene界面中选中需要编辑的顶点(蓝色小球)，移动位置来编辑顶点。

### 3. 保存
* 在Game界面中点击`Save` 保存网格数据。















