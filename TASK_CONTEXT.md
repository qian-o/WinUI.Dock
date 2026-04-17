# WinUI.Dock 改进任务

## 总体计划
- **Phase 1**：LayoutPanel 布局机制优化 ✅ 已完成
- **Phase 2**：统一拖拽系统重写（合并任务 1+2+3）⬜ 待开始

## Phase 1 已完成：LayoutPanel 布局优化

### 改动文件
1. **src/WinUI.Dock/Abstracts/DockModule.cs**
   - 新增 `DockSize` DependencyProperty（double, 默认 NaN）
   - `CopyDimensions` / `ClearDimensions` 同步 DockSize
2. **src/WinUI.Dock/LayoutPanel.xaml.cs**
   - 新增 `pixelHintsConverted` 字段 + `SizeChanged` 事件处理
   - 新增 `TryConvertPixelHints()`：首次布局后将像素提示转换为等价星号比例
   - 重写 `UpdateLayoutStructure()`：优先用 DockSize（星号），回退 Width/Height（像素）；始终注册 PropertyChangedCallback 双向同步
   - `CalculateHeight`/`CalculateWidth` 改为固定 25% 预览
   - 修复 GridSplitter 遍历删除 bug（`.OfType<GridSplitter>().ToArray()`）
3. **src/WinUI.Dock/DockManager.xaml.cs**
   - `Dock()` 方法：边缘停靠 1:3 比例（新面板 DockSize=1, 现有面板 DockSize=3）
4. **src/WinUI.Dock/DocumentGroup.xaml.cs**
   - `Dock()` 方法：同向拆分对半分割 DockSize
5. **src/WinUI.Dock/Helpers/LayoutHelpers.cs**
   - 序列化/反序列化增加 DockSize 字段，向后兼容旧 JSON

### 编译状态
- 两个 TFM 编译通过，0 警告 0 错误
- 分支：`feature/layout-and-drag-improvements`

## Phase 2 待实现：统一拖拽系统重写

### 合并的子任务
1. **Tab 页签拖拽排序** — 同 DocumentGroup 内拖拽调整 Tab 顺序
2. **平台 API 鼠标捕获** — 替代框架 DragDrop API，用 PointerPressed + 系统 API 轮询鼠标
3. **半透明浮动预览** — 拖拽时显示半透明窗口跟随鼠标

### 涉及文件（预估）
- DockTabItem.xaml.cs — 移除 CanDrag/DragStarting/DropCompleted，改用 PointerPressed 驱动
- DragDropHelpers.cs — 可能重构或替换
- DockTargetButton.xaml.cs — 改用命中测试而非 DragEnter/Drop
- FloatingWindow.xaml.cs — 拖拽预览模式支持
- DockManager.xaml.cs / DocumentGroup.xaml.cs — 拖拽目标显示逻辑
- PointerHelpers.cs — 已有跨平台鼠标位置获取，可直接复用

### 设计思路
- 新 DragService：PointerPressed 记录起始点 → DispatcherQueueTimer 16ms 轮询鼠标位置
- 5px 阈值判断：
  - 同 DocumentGroup Tab 区域内 → Tab 重排（Children 索引交换）
  - 超出 Tab 区域 → Detach + 创建半透明预览 FloatingWindow（opacity 0.6, always-on-top）
- 命中测试 DockTargetRegistry 判断停靠目标
- ESC 取消并恢复原位
