# WinUI.Dock 改进任务

## 总体计划
- **Phase 1**：LayoutPanel 布局机制优化 ✅ 已完成
- **Phase 2**：统一拖拽系统重写（合并任务 1+2+3）✅ 已完成

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

## Phase 2 已完成：统一拖拽系统重写

### 合并的子任务
1. **Tab 页签拖拽排序** ✅ — 同 DocumentGroup 内拖拽调整 Tab 顺序
2. **平台 API 鼠标捕获** ✅ — 替代框架 DragDrop API，用 PointerPressed + DispatcherQueueTimer 轮询鼠标
3. **半透明浮动预览** ✅ — 拖拽时显示半透明窗口跟随鼠标

### 改动文件
1. **src/WinUI.Dock/Helpers/PointerHelpers.cs**
   - 新增 `IsPointerButtonPressed()` — 跨平台鼠标按键状态检测（Windows/Linux/macOS）
   - 新增 `IsEscapePressed()` — ESC 键检测（Windows，其他平台暂不支持）
   - 新增 `GetWindowAtScreenPoint()` — 屏幕坐标下的窗口句柄获取（Windows）
   - 新增 `GetClientOrigin()` — 窗口客户区屏幕起点（Windows）
   - 新增 `GetActiveWindowHandle()` — 活动窗口句柄
   - 新增 `SetWindowTransparent()` — 设置窗口穿透点击（WS_EX_TRANSPARENT）
   - 新增 P/Invoke: `GetAsyncKeyState`, `WindowFromPoint`, `GetAncestor`, `ClientToScreen`, `GetActiveWindow`, `GetWindowLongPtrW`, `SetWindowLongPtrW`, `CGEventSourceButtonState`
2. **src/WinUI.Dock/Helpers/DragService.cs** — 新文件，统一拖拽编排器
   - `BeginTabDrag()` — DockTabItem 拖拽入口
   - `BeginSidePopupDrag()` — SidePopup 拖拽入口
   - `Cancel()` — ESC 取消恢复原位
   - 16ms DispatcherQueueTimer 轮询鼠标位置和按键状态
   - 5px 阈值判断：Y 位移 > 30px 进入浮动拖拽，否则 Tab 重排
   - Tab 重排：通过 `ObservableCollection.Move()` 交换索引
   - 浮动拖拽：创建半透明预览窗口（opacity 0.6，WS_EX_TRANSPARENT 穿透）
   - 命中测试：屏幕坐标 → 窗口本地坐标 → DockManager/DocumentGroup 区域检测
   - 支持跨窗口停靠（主窗口 + FloatingWindow）
3. **src/WinUI.Dock/Controls/DockTabItem.xaml / .cs**
   - 移除 `CanDrag`, `DragStarting`, `DropCompleted`
   - `OnPointerPressed` 改为调用 `DragService.BeginTabDrag()`
   - `ContentOptions_PointerPressed` 同样调用 DragService
4. **src/WinUI.Dock/Controls/DockTargetButton.xaml / .cs**
   - 移除 `AllowDrop`，设为 `IsHitTestVisible="False"`（纯视觉指示器）
   - 移除 `OnDragEnter`, `OnDragLeave`, `OnDragOver`, `OnDrop` 重写
5. **src/WinUI.Dock/DockManager.xaml / .cs**
   - 移除 `AllowDrop` Setter、`OnDragEnter`, `OnDragLeave` 重写
   - 新增 `ShowDockTargets()` 内部方法
6. **src/WinUI.Dock/DocumentGroup.xaml / .cs**
   - 移除 `AllowDrop` Setter、`OnDragEnter`, `OnDragLeave` 重写
   - 新增 `ShowDockTargets()`, `HideDockTargets()` 内部方法
7. **src/WinUI.Dock/Controls/FloatingWindow.xaml / .cs**
   - 移除 `AllowDrop`, `DragEnter`, `OnDragEnter` 事件
8. **src/WinUI.Dock/Controls/SidePopup.xaml / .cs**
   - 移除 `CanDrag`, `DragStarting`, `DropCompleted`
   - `Header_PointerPressed` 改为调用 `DragService.BeginSidePopupDrag()`
   - 新增 `DetachForDrag()` 内部方法

### 编译状态
- 三个项目编译通过（WinUI.Dock 双 TFM + Example.WinUI + Example.Uno），0 错误 0 警告
- 分支：`feature/layout-and-drag-improvements`
