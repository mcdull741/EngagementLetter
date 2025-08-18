# 模板条件设置重构说明

## 概述

本次重构将模板条件设置功能从Create.cshtml和Edit.cshtml中抽离出来，作为一个独立的公共模块，通过iframe方式嵌入，确保Create和Edit页面的条件设置功能完全一致。

## 文件结构

### 核心文件
- `_TemplateConditions.cshtml` - 模板条件设置的核心功能模块
- `_ConditionsFrame.cshtml` - iframe承载页面
- `Create.cshtml` - 创建模板页面（已重构）
- `Edit.cshtml` - 编辑模板页面（新增）

### 功能特点
1. **一致性保证** - Create和Edit页面使用完全相同的条件设置逻辑
2. **独立维护** - 条件设置逻辑集中在一个文件中维护
3. **数据同步** - 通过iframe消息机制实现父页面与子页面的数据同步
4. **用户体验** - 自动高度调整，确保良好的用户体验

## 技术实现

### iframe通信机制
```javascript
// 父页面发送消息
frame.contentWindow.postMessage({
    type: 'setQuestionnaireId',
    questionnaireId: selectedQuestionnaireId,
    conditions: conditionsData
}, '*');

// 子页面接收消息
window.addEventListener('message', function(event) {
    if (event.data.type === 'setQuestionnaireId') {
        // 处理数据初始化
    }
});

// 子页面发送数据变更通知
window.parent.postMessage({
    type: 'conditionsData',
    conditions: getAllConditions()
}, '*');
```

### 数据流
1. **初始化** - 父页面加载时，通过iframe消息传递初始条件和问卷ID
2. **用户交互** - 用户在iframe中操作条件设置
3. **数据同步** - 每次操作后，子页面通过postMessage通知父页面数据变更
4. **表单提交** - 父页面收集iframe中的最终条件数据进行提交

### 多选问题修复
- 修复了复选框类型问题expectedAnswer只能保存一个选项的问题
- 使用JSON数组格式存储多个选中值
- 确保表单提交时正确收集所有选中选项

## 使用说明

### 在Create页面中使用
```html
<iframe id="conditionsFrame" 
        src="/Templates/_ConditionsFrame" 
        width="100%" 
        height="400" 
        frameborder="0"
        style="min-height: 400px;"></iframe>
<input type="hidden" id="conditionsData" name="ConditionsData" />
```

### 在Edit页面中使用
```html
<iframe id="conditionsFrame" 
        src="/Templates/_ConditionsFrame" 
        width="100%" 
        height="400" 
        frameborder="0"
        style="min-height: 400px;"></iframe>
<input type="hidden" id="conditionsData" name="ConditionsData" />
<input type="hidden" id="originalConditions" value='@Html.Raw(Json.Serialize(Model.Conditions))' />
```

## API依赖

需要以下API端点：
- `GET /api/Questions/GetByQuestionnaire/{questionnaireId}` - 获取问卷下的所有问题
- `GET /api/Questions/GetById/{id}` - 获取单个问题的详细信息

## 浏览器兼容性

- 支持所有现代浏览器（IE11+）
- 使用postMessage API进行跨域通信
- 自动高度调整确保良好的移动端体验