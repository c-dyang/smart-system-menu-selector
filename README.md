# SmartSystemMenu Selector

SmartSystemMenu 的 fork，新增**显示器选择器**功能：

- **热键 `Win+Shift+N`**：在鼠标位置弹出显示器选择器（各显示器色块+编号）
- **点击目标屏**：把当前活动窗口移动到该显示器
- **保持窗口状态**：最大化窗口移动后仍最大化，还原窗口保持还原

## 构建

```bash
dotnet build -c Release
# 产物: bin/Release/net8.0-windows/SmartSystemMenu.exe
```

## 热键

| 功能 | 热键 |
|------|------|
| 显示器选择器（点击移动） | Win+Shift+N |
| 移动到下一显示器 | 默认 Ctrl+Alt+Right（可改） |
| 移动到上一显示器 | 默认 Ctrl+Alt+Left（可改） |

配置在 `SmartSystemMenu.xml`（mover/select 节点）。

> 上游: https://github.com/AlexanderPro/SmartSystemMenu (MIT)
