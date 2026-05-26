using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FlowableBpmnClient
{
    /// <summary>
    /// Flowable BPMN REST API 客户端
    /// </summary>
    public class FlowableBpmnClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// 初始化 Flowable BPMN 客户端
        /// </summary>
        /// <param name="baseUrl">Flowable REST API 基础 URL，例如：http://localhost:8080/flowable-rest/service</param>
        /// <param name="username">Basic Auth 用户名</param>
        /// <param name="password">Basic Auth 密码</param>
        public FlowableBpmnClient(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            
            // 设置 Basic Authentication
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            
            // 设置 Accept 头
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }

        /// <summary>
        /// 通过 processDefinitionId 获取原始 BPMN XML 文件
        /// </summary>
        /// <param name="processDefinitionId">流程定义 ID</param>
        /// <returns>BPMN XML 内容字符串</returns>
        public async Task<string> GetBpmnXmlAsync(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ArgumentException("流程定义 ID 不能为空", nameof(processDefinitionId));
            }

            var endpoint = $"/repository/process-definitions/{processDefinitionId}/resourcedata";
            
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var xmlBytes = await response.Content.ReadAsByteArrayAsync();
                    return Encoding.UTF8.GetString(xmlBytes);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FlowableNotFoundException($"未找到流程定义：{processDefinitionId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new FlowableApiException(
                        $"获取 BPMN XML 失败：{response.StatusCode} - {errorContent}",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new FlowableConnectionException($"连接 Flowable 服务失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 通过 processDefinitionId 获取 BPMN Model JSON（包含流程结构信息）
        /// </summary>
        /// <param name="processDefinitionId">流程定义 ID</param>
        /// <returns>BPMN Model JSON 字符串</returns>
        public async Task<string> GetBpmnModelJsonAsync(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ArgumentException("流程定义 ID 不能为空", nameof(processDefinitionId));
            }

            var endpoint = $"/repository/process-definitions/{processDefinitionId}/model";
            
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FlowableNotFoundException($"未找到流程定义：{processDefinitionId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new FlowableApiException(
                        $"获取 BPMN Model 失败：{response.StatusCode} - {errorContent}",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new FlowableConnectionException($"连接 Flowable 服务失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取流程定义信息
        /// </summary>
        /// <param name="processDefinitionId">流程定义 ID</param>
        /// <returns>流程定义 JSON 信息</returns>
        public async Task<string> GetProcessDefinitionAsync(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ArgumentException("流程定义 ID 不能为空", nameof(processDefinitionId));
            }

            var endpoint = $"/repository/process-definitions/{processDefinitionId}";
            
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FlowableNotFoundException($"未找到流程定义：{processDefinitionId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new FlowableApiException(
                        $"获取流程定义失败：{response.StatusCode} - {errorContent}",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new FlowableConnectionException($"连接 Flowable 服务失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存 BPMN XML 到文件
        /// </summary>
        /// <param name="processDefinitionId">流程定义 ID</param>
        /// <param name="filePath">保存的文件路径</param>
        public async Task SaveBpmnXmlToFileAsync(string processDefinitionId, string filePath)
        {
            var xmlContent = await GetBpmnXmlAsync(processDefinitionId);
            await System.IO.File.WriteAllTextAsync(filePath, xmlContent, Encoding.UTF8);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Flowable API 异常 - 未找到资源
    /// </summary>
    public class FlowableNotFoundException : Exception
    {
        public FlowableNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Flowable API 异常 - 连接错误
    /// </summary>
    public class FlowableConnectionException : Exception
    {
        public FlowableConnectionException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    /// <summary>
    /// Flowable API 异常 - 通用 API 错误
    /// </summary>
    public class FlowableApiException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }

        public FlowableApiException(string message, System.Net.HttpStatusCode statusCode) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
