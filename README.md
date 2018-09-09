# 基于TexturePacker的一键打图集工具

打图集是一件频繁且费时的工作，开发工具进行打图集是十分必要的。
使用TexturePacker打图集，会输出两个文件
-   PNG合图
-   txt 图集信息文件

而到Unity使用UIAtlas图集则需要一个Material材质文件和一个Prefab预制体文件，这两个文件也由工具一键生成

## 工具使用介绍
Atlas.ini图集配置文件

该配置以CSV格式进行配置，共有5列：
图片文件夹相对路径、PNG合图输出路径、txt图集信息文件输出路径、UIAtlasPrefab输出路径、Material材质输出路径，后面四个输出路径都是基于Unity工程的Assets目录。

配置好该文件后，Unity菜单栏 Tools->一键打图集工具 打开工具

工具界面分为三个部分：搜索框、图集勾选框、菜单栏
支持一键全选、取消勾选功能。
勾选好图集，一键打图集按钮即可打图集。

![Alt text](https://github.com/JingFengJi/CreateAtlas/blob/master/Photo/TexturePacker.png)

## 命令行打图集
TexturePacker工具支持使用命令行打图集。

```ruby
$TexturePacker_PATH $imgDir --max-size 4096 --allow-free-size --format unity --size-constraints POT --shape-padding 2 --border-padding 2 --common-divisor-x 1 --common-divisor-y 1 --disable-rotation --algorithm MaxRects --opt RGBA8888 --scale 1 --scale-mode Smooth --smart-update --sheet $ASSETS_PATH$atlasPng --data $ASSETS_PATH$atlasData
```

## 一键批量设置图片资源信息
PNG合图文件输出到Unity后，通过使用Unity提供的TextureImporter类和TextureImporterPlatformSettings类进行图片设置

## 生成材质和Prefab文件并自动关联引用
材质使用的Shader是"Unlit/Transparent Colored"，mainTexture则为命令行输出的PNG合图

创建Prefab，并Add UIAtlas Component

给UIAtlas导入texture和txt图集信息文件
```ruby
NGUIEditorTools.ImportTexture(uiAtlas.texture, false, false, !uiAtlas.premultipliedAlpha);
NGUIJson.LoadSpriteData(uiAtlas, atlasTextAsset);
```

## UIAtlas信息拷贝
创建图集后，有时需要拷贝另一个图集的border和padding信息（例如：Event换肤）

菜单栏Tools->CopyAtlasData

![Alt text](https://github.com/JingFengJi/CreateAtlas/blob/master/Photo/CopyAtlasData.png)

## 清理Editor进度条
为避免Atlas.ini配置错误，导致打图集中断，而EditorUtility.DisplayProgressBar进度条无法清除，提供Editor进度条清理功能，菜单栏Tools->清理进度条