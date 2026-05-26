# Flowable BPMN XML 获取示例（C#）

本项目演示如何使用 C# 通过 REST API 从 Flowable 平台获取流程定义的 BPMN XML 数据。

## 项目结构

```
flowable-bpmn-client-example/
├── FlowableBpmnClient.cs          # Flowable 客户端封装类
├── Program.cs                     # 使用示例
├── FlowableBpmnClientExample.csproj  # 项目文件
└── README.md                      # 说明文档
```

## 核心 API 端点

### 1. 获取原始 BPMN XML
```
GET /repository/process-definitions/{processDefinitionId}/resourcedata
```

**返回**: BPMN 2.0 XML 文件的原始内容（Content-Type: application/xml）

### 2. 获取 BPMN Model JSON
```
GET /repository/process-definitions/{processDefinitionId}/model
```

**返回**: BPMN 模型的对象结构（JSON 格式）

### 3. 获取流程定义信息
```
GET /repository/process-definitions/{processDefinitionId}
```

**返回**: 流程定义的基本信息（JSON 格式）

## 使用方法

### 步骤 1：配置 Flowable REST API

确保 Flowable REST 服务正在运行，默认配置：
- **URL**: `http://localhost:8080/flowable-rest/service`
- **用户名**: `rest-admin`
- **密码**: `test`

> 注意：默认凭据仅用于开发环境，生产环境请修改密码。

### 步骤 2：编译并运行

```bash
cd flowable-bpmn-client-example

# 还原 NuGet 包
dotnet restore

# 编译
dotnet build

# 运行
dotnet run
```

### 步骤 3：修改流程定义 ID

打开 `Program.cs`，将示例中的流程定义 ID 修改为你要获取的实际流程 ID：

```csharp
string processDefinitionId = "Process_1";  // 修改为实际的流程 ID
```

## 代码说明

### FlowableBpmnClient 类

封装了 Flowable REST API 的主要方法：

```csharp
// 创建客户端
using var client = new FlowableBpmnClient(baseUrl, username, password);

// 获取 BPMN XML
string xml = await client.GetBpmnXmlAsync(processDefinitionId);

// 保存到文件
await client.SaveBpmnXmlToFileAsync(processDefinitionId, "process.bpmn20.xml");

// 获取 BPMN Model JSON
string json = await client.GetBpmnModelJsonAsync(processDefinitionId);

// 获取流程定义信息
string info = await client.GetProcessDefinitionAsync(processDefinitionId);
```

### 错误处理

客户端封装了三种异常类型：

1. **FlowableNotFoundException**: 流程定义不存在
2. **FlowableConnectionException**: 网络连接错误
3. **FlowableApiException**: API 调用错误（包含 HTTP 状态码）

## 获取流程定义 ID

可以通过以下方式获取已部署的流程定义 ID：

### REST API
```bash
curl -u rest-admin:test \
  http://localhost:8080/flowable-rest/service/repository/process-definitions
```

### C# 代码
```csharp
// 使用 HttpClient 直接调用
var response = await httpClient.GetAsync("/repository/process-definitions");
var processDefinitions = await response.Content.ReadAsStringAsync();
Console.WriteLine(processDefinitions);
```

## 示例输出

```text
=== 获取原始 BPMN XML ===
BPMN XML 长度：15234 字符
BPMN XML 内容（前 200 字符）:
<?xml version="1.0" encoding="UTF-8"?>
<definitions xmlns="http://www.omg.org/spec/BPMN/20100524/MODEL" id="Process_1"...

=== BPMN XML 已保存到 process.bpmn20.xml ===

=== 获取 BPMN Model JSON ===
BPMN Model JSON 长度：8765 字符
BPMN Model JSON（前 300 字符）:
{"processes":[{"id":"Process_1","name":"请假流程","flowElements":[...
```

## 生产环境最佳实践

1. **使用 HTTPS**: 生产环境务必使用 HTTPS 协议
2. **Token 认证**: 考虑使用 OAuth2 或 JWT 替代 Basic Auth
3. **连接池**: 长时间运行的应用应该复用 HttpClient 实例
4. **超时设置**: 配置合理的请求超时时间
5. **重试策略**: 实现指数退避重试机制
6. **日志记录**: 记录 API 调用日志用于审计
7. **密码管理**: 使用配置中心或密钥管理服务存储凭据

## 依赖项

- .NET 8.0+
- System.Net.Http 4.3.4+

## 参考资料

- [Flowable 官方文档](https://www.flowable.org/docs/)
- [Flowable REST API 文档](https://www.flowable.org/open-source/docs)
- [BPMN 2.0 规范](https://www.omg.org/spec/BPMN/2.0.2/)

## 许可证

Apache 2.0
