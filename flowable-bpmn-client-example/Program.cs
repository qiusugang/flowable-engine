using System;
using System.Threading.Tasks;
using FlowableBpmnClient;

namespace FlowableBpmnClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Flowable REST API 配置
            // 默认 Flowable REST 运行在 8080 端口
            const string baseUrl = "http://localhost:8080/flowable-rest/service";
            const string username = "rest-admin";
            const string password = "test";

            // 示例流程定义 ID（需要替换为实际的流程定义 ID）
            string processDefinitionId = "Process_1";

            using var client = new FlowableBpmnClient(baseUrl, username, password);

            try
            {
                // ===========================================
                // 示例 1：获取原始 BPMN XML 内容
                // ===========================================
                Console.WriteLine("=== 获取原始 BPMN XML ===");
                string bpmnXml = await client.GetBpmnXmlAsync(processDefinitionId);
                Console.WriteLine($"BPMN XML 长度：{bpmnXml.Length} 字符");
                Console.WriteLine("BPMN XML 内容（前 200 字符）:");
                Console.WriteLine(bpmnXml.Substring(0, Math.Min(200, bpmnXml.Length)));
                Console.WriteLine();

                // ===========================================
                // 示例 2：保存 BPMN XML 到文件
                // ===========================================
                string filePath = "process.bpmn20.xml";
                await client.SaveBpmnXmlToFileAsync(processDefinitionId, filePath);
                Console.WriteLine($"=== BPMN XML 已保存到 {filePath} ===");
                Console.WriteLine();

                // ===========================================
                // 示例 3：获取 BPMN Model JSON（包含流程结构）
                // ===========================================
                Console.WriteLine("=== 获取 BPMN Model JSON ===");
                string bpmnModelJson = await client.GetBpmnModelJsonAsync(processDefinitionId);
                Console.WriteLine($"BPMN Model JSON 长度：{bpmnModelJson.Length} 字符");
                Console.WriteLine("BPMN Model JSON（前 300 字符）:");
                Console.WriteLine(bpmnModelJson.Substring(0, Math.Min(300, bpmnModelJson.Length)));
                Console.WriteLine();

                // ===========================================
                // 示例 4：获取流程定义基本信息
                // ===========================================
                Console.WriteLine("=== 获取流程定义信息 ===");
                string processInfo = await client.GetProcessDefinitionAsync(processDefinitionId);
                Console.WriteLine("流程定义信息:");
                Console.WriteLine(processInfo);
            }
            catch (FlowableNotFoundException ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
                Console.WriteLine("请检查流程定义 ID 是否正确，或流程是否已部署。");
            }
            catch (FlowableConnectionException ex)
            {
                Console.WriteLine($"连接错误：{ex.Message}");
                Console.WriteLine("请检查 Flowable REST 服务是否正在运行。");
            }
            catch (FlowableApiException ex)
            {
                Console.WriteLine($"API 错误：{ex.Message}");
                Console.WriteLine($"HTTP 状态码：{ex.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未知错误：{ex.Message}");
                Console.WriteLine($"详细信息：{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 批量获取多个流程定义的 BPMN XML
        /// </summary>
        static async Task BatchGetBpmnXmlAsync()
        {
            const string baseUrl = "http://localhost:8080/flowable-rest/service";
            const string username = "rest-admin";
            const string password = "test";

            using var client = new FlowableBpmnClient(baseUrl, username, password);

            string[] processDefinitionIds = { "Process_1", "Process_2", "Process_3" };

            foreach (var processId in processDefinitionIds)
            {
                try
                {
                    string xml = await client.GetBpmnXmlAsync(processId);
                    string filePath = $"./bpmn/{processId}.bpmn20.xml";
                    
                    // 确保目录存在
                    System.IO.Directory.CreateDirectory("./bpmn");
                    
                    await System.IO.File.WriteAllTextAsync(filePath, xml, System.Text.Encoding.UTF8);
                    Console.WriteLine($"已保存：{filePath}");
                }
                catch (FlowableNotFoundException)
                {
                    Console.WriteLine($"未找到流程：{processId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理流程 {processId} 时出错：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 解析并显示 BPMN XML 中的基本信息
        /// </summary>
        static void ParseBpmnXmlInfo(string bpmnXml)
        {
            // 简单的 XML 解析示例（生产环境建议使用 XDocument 或专门的 BPMN 库）
            
            // 提取流程 ID
            var processIdStart = bpmnXml.IndexOf("id=\"");
            if (processIdStart > 0)
            {
                processIdStart += 4;
                var processIdEnd = bpmnXml.IndexOf("\"", processIdStart);
                if (processIdEnd > processIdStart)
                {
                    var processId = bpmnXml.Substring(processIdStart, processIdEnd - processIdStart);
                    Console.WriteLine($"流程 ID: {processId}");
                }
            }

            // 提取流程名称
            var nameStart = bpmnXml.IndexOf("name=\"");
            if (nameStart > 0)
            {
                nameStart += 6;
                var nameEnd = bpmnXml.IndexOf("\"", nameStart);
                if (nameEnd > nameStart)
                {
                    var name = bpmnXml.Substring(nameStart, nameEnd - nameStart);
                    Console.WriteLine($"流程名称：{name}");
                }
            }

            // 统计任务数量（简单示例）
            int userTaskCount = bpmnXml.Split("<bpmn2:userTask").Length - 1;
            int serviceTaskCount = bpmnXml.Split("<bpmn2:serviceTask").Length - 1;
            Console.WriteLine($"用户任务数：{userTaskCount}");
            Console.WriteLine($"服务任务数：{serviceTaskCount}");
        }
    }
}
