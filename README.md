# DockingManager 设计文档

## 接口和类

### ILayoutGroup
- 定义布局组的基本结构。

### LayoutDocument
- 代表一个文档。

### LayoutDocumentGroup
- 代表一组 `LayoutDocument`。

### LayoutAnchor
- 代表一个可锚定的元素。

### LayoutAnchorGroup
- 代表一组 `LayoutAnchor`。

### LayoutSide
- 代表区域的一侧。

### LayoutPanel
- 代表中心区域的面板，可以包含文档或可锚定的元素。
- `LayoutPanel.Children` 是一个 `ILayoutGroup` 数组，使用 `DockPanel` 布局。