# 水动力模型平台
## 概述
水动力模型平台整合多种开源水动力模型，基于Unity URP，实现模型运行、结果可视化、结果导出等功能。
## 特性
* 使用json文件保存模型参数信息。
* 在"Model Selector"中，可以选择多种模型运行。
* 多种参数控制模型的结果输出、绘制步长、绘制总时间等。
* 三维可视化显示水动力模型结果。
* 计算结果文件导出。
## 功能简介
### Select Model
打开"Model Selector"界面，选择需要运行的水动力模型，设置Saving/Ending/Drawing Step共3个参数，点击运行。
* Saving Step：每SavingStep个时间步长，保存计算结果。
* Ending Step：当运行到EndingStep时，模型运行结束。
* Drawing Step：每DrawingStep个时间步长，切换绘制纹理，用于插值绘制结果。
### Export Files
选择输出文件的路径，输出所有保存的计算结果，包括每个格网点的位置、水深、速度。
### Close Model
结束运行该模型，释放所有资源。
## 案例
使用Assets/Resources中的HongKongTestArea.tif模型，使用VPM模型，使用默认参数，开始运行。
> 若使用自己的栅格，请确保栅格尺寸为**正方形**。
## TODO
* 用于自由控制相机的脚本。
* 更多水动力模型的加入。
* ......