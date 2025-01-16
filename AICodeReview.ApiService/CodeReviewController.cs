using AICodeReview.ApiService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using AICodeReview.ApiService.Utils;
using MongoDB.Bson;
using AICodeReview.ApiService.Vo;
using System.IO;
namespace AICodeReview.ApiService
{
    /// <summary>
    /// 代码审核
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CodeReviewController : ControllerBase
    {
        private readonly IMongoClient _mongoClient;
        private readonly IHttpClientFactory _httpClientFactory;
        public CodeReviewController(IMongoClient mongoClient, IHttpClientFactory httpClientFactory)
        {
            _mongoClient = mongoClient;
            _httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// 提交记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        [Description("提交记录")]
        public async Task<List<Commits>> Get(int pageIndex)
        {
            var list = await GitLabUtils.GetHistory(pageIndex);
            var ids = list.Select(x => x.Id);
            if (ids.Any())
            {
                var oldList = (await _mongoClient.GetDatabase("aicodereview")
                    .GetCollection<AIAnswerDetail>("AIAnswerDetail")
                    .FindAsync(x => ids.Contains(x.CommitId))).ToList();
                foreach (var item in list)
                {
                    item.Status = oldList?.FirstOrDefault(x => x.CommitId == item.Id) != null ? "已审核" : "未审核";
                }
            }
            return list;
        }
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("detail")]
        public async Task<CodeReviewDetailVo> GetByIdAsync(string id)
        {
            CodeReviewDetailVo detail = new CodeReviewDetailVo();
            var result = await _mongoClient.GetDatabase("aicodereview")
                                     .GetCollection<AIAnswerDetail>("AIAnswerDetail")
                                     .FindAsync(x => x.CommitId == id && x.AIAnswerType==1);
            var history = await _mongoClient.GetDatabase("aicodereview")
                                     .GetCollection<CommitHistory>("CommitHistory")
                                     .FindAsync(x => x.CommitId == id);
            detail.AIAnswer = (await result.FirstOrDefaultAsync())?.AIAnswer;
            detail.DiffContents = new List<DiffContentVo>();
            var sideBySideDiffBuilder = new SideBySideDiffBuilder();
            foreach (var item in history.ToList())
            {
                var sideBySideDiffModel = sideBySideDiffBuilder.BuildDiffModel(item.PreviousContent, item.CurrentContent,true);
                detail.DiffContents.Add(new DiffContentVo
                {
                    FileName = item.FileName,
                    OldContent = RenderDiffs(sideBySideDiffModel.OldText.Lines
                                            .Where(x => x.Type != ChangeType.Unchanged && !string.IsNullOrWhiteSpace(x.Text))
                                            .OrderBy(x => x.Position).ToList()),
                    NewContent = RenderDiffs(sideBySideDiffModel.NewText.Lines
                                            .Where(x => x.Type != ChangeType.Unchanged && !string.IsNullOrWhiteSpace(x.Text))
                                            .OrderBy(x => x.Position).ToList())
                });
            }
            return detail;
        }
        /// <summary>
        /// 渲染差异
        /// </summary>
        /// <param name="file"></param>
        /// <param name="old"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static string RenderDiffs(List<DiffPiece> diffPiece)
        {
            var sb = new StringBuilder();
            // Convert to HTML
            sb.Append("<div class='file'>");
            foreach (var line in diffPiece)
            {
                switch (line.Type)
                {
                    case ChangeType.Modified:
                        sb.Append($"<upd>{line.Text}</upd>");
                        break;
                    case ChangeType.Inserted:
                        sb.Append($"<ins>++ {line.Text}</ins>");
                        break;
                    case ChangeType.Deleted:
                        sb.Append($"<del>{line.Text}</del>");
                        break;
                }
                sb.Append("<br />");
            }
            sb.Append("</div>");
            return sb.ToString();
        }
        /// <summary>
        /// webhook
        /// </summary>
        [HttpPost]
        [Route("hook")]
        public async Task Hook()
        {
            var gitList= await GitLabUtils.GetHistory(1,1);
            var detail = await GitLabUtils.GetCommitDetail(gitList[0].Id);
            if (detail != null)
            {
                List<CommitHistory> histories = new List<CommitHistory>();
                foreach (var item in detail)
                {
                    var history = (await GitLabUtils.GetHistoryByFilePath(item.NewPath))
                               .OrderByDescending(x => Convert.ToDateTime(x.CommitDate))
                               .ToList();
                    var currentContent = await GitLabUtils.GetFileContent(item.NewPath, gitList[0].Id);
                    var previousContent = "";
                    //得到上一次提交记录
                    var index = history.FindIndex(x => x.Id == gitList[0].Id);
                    if (index + 1 < history.Count)
                    {
                        var previousCommit = history[index + 1];
                        previousContent = await GitLabUtils.GetFileContent(item.NewPath, previousCommit.Id);// 获取上一次提交记录的文件内容
                    }
                    histories.Add(new CommitHistory
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        CurrentContent = currentContent,
                        PreviousContent = previousContent,
                        FileName= item.NewPath,
                        CommitId= gitList[0].Id
                    });
                }
                if (histories.Count != 0)
                {
                    await _mongoClient.GetDatabase("aicodereview")
                                   .GetCollection<CommitHistory>("CommitHistory")
                                   .DeleteManyAsync(x => x.CommitId == gitList[0].Id);
                    await _mongoClient.GetDatabase("aicodereview")
                                   .GetCollection<CommitHistory>("CommitHistory")
                                   .InsertManyAsync(histories);
                }
            }

            await CallCozeApi(gitList[0].Id, JsonConvert.SerializeObject(detail));
            Console.WriteLine("hook");
        }
        /// <summary>
        /// 调用coze
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commitContent"></param>
        /// <returns></returns>
        private async Task<string> CallCozeApi(string id, string commitContent)
        {
            // 你的 API 密钥
            string apiKey = "";
            // API 端点
            string apiUrl = "https://api.coze.cn/v3/chat";
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            // 请求体，根据 API 文档进行修改
            request.Content = new StringContent(
                 JsonConvert.SerializeObject(new
                 {
                     stream = true,
                     additional_messages = new[]
                     {
                        new
                        {
                            content_type = "text",
                            content = commitContent,
                            role = "user",
                            type = "question"
                        }
                     },
                     bot_id = "",
                     user_id = "",
                     auto_save_history = false
                 }),
                 Encoding.UTF8, "application/json"
             );
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            // 添加 API 密钥到请求头
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            using HttpClient client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder lineBuilder = new StringBuilder();
                string line;
                string msgEvent = "";
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    lineBuilder.AppendLine(line);
                    if (line.StartsWith("event:"))
                    {
                        msgEvent = line;
                    }
                    else if (line.StartsWith("data:") && msgEvent == "event:conversation.message.completed")
                    {
                        try
                        {
                            string json = line.Substring(5);
                            var jsonLine = JObject.Parse(json);
                            // 处理 JSON 数据，根据 API 文档提取所需信息
                            if (jsonLine.TryGetValue("type", out JToken typeToken)
                                && typeToken != null
                                && typeToken.ToString() == "answer"
                                && jsonLine.TryGetValue("content", out JToken contentToken)
                                && contentToken != null
                                && contentToken.Type == JTokenType.String)
                            {
                                stringBuilder.AppendLine(contentToken.ToString());
                            }
                        }
                        catch (JsonReaderException ex)
                        {
                            Console.WriteLine($"Failed to parse JSON line: {ex.Message}");
                        }
                    }
                }
                if (stringBuilder.Length > 0)
                {
                    var records = new List<AIAnswerDetail>()
                    {
                        new()
                        {
                             Id = Guid.NewGuid().ToString("N"),
                             AIAnswer = stringBuilder.ToString(),
                             AIAnswerType = 1,
                             CommitId=id
                         },
                         new()
                         {
                             Id = Guid.NewGuid().ToString("N"),
                             AIAnswer = lineBuilder.ToString(),
                             AIAnswerType = 0,
                             CommitId=id
                         }
                    };
                    await _mongoClient.GetDatabase("aicodereview")
                            .GetCollection<AIAnswerDetail>("AIAnswerDetail")
                            .DeleteManyAsync(x => x.CommitId == id);
                    await _mongoClient.GetDatabase("aicodereview")
                         .GetCollection<AIAnswerDetail>("AIAnswerDetail")
                         .InsertManyAsync(records);
                    return stringBuilder.ToString();
                }
            }
            else
            {
                Console.WriteLine($"请求失败: {response.StatusCode}");
            }
            return "";
        }
    }
}
