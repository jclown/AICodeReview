using AICodeReview.ApiService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace AICodeReview.ApiService.Utils
{
    /// <summary>
    /// GitLab 工具类
    /// </summary>
    public class GitLabUtils
    {
        private readonly static string _projectId = ""; // 项目 ID
        private readonly static string _gitLabUrl = ""; // GitLab 服务器地址
        private readonly static string _privateToken = ""; // 私有令牌
        private readonly static string _branch = "";// 分支

        /// <summary>
        /// 获取最近的提交历史
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<List<Commits>> GetHistory(int pageIndex,int pageSize=50)
        {
            List<Commits> list = new List<Commits>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Private-Token", _privateToken);
                // 获取最近的提交历史
                string requestUrl = $"{_gitLabUrl}/{_projectId}/repository/commits?ref_name={_branch}&page={pageIndex}&per_page={pageSize}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    await BuildCommitsList(list, response);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            return list;
        }
        /// <summary>
        /// 构建 Commits 列表
        /// </summary>
        /// <param name="list"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task BuildCommitsList(List<Commits> list, HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            JArray jsonArray = JArray.Parse(responseBody);
            // 遍历JArray中的每个元素
            foreach (JObject obj in jsonArray.Cast<JObject>())
            {
                list.Add(new Commits
                {
                    Id = (string?)obj["id"],
                    AuthorName = (string?)obj["author_name"],
                    Message = (string?)obj["message"],
                    CommitName = (string?)obj["committer_name"],
                    CommitDate = (string?)obj["committed_date"],
                    CommitEmail = (string?)obj["committer_email"],

                });
            }
        }


        /// <summary>
        /// 获取指定文件路径的提交记录
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<List<Commits>> GetHistoryByFilePath(string filePath)
        {
            List<Commits> list = new List<Commits>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _privateToken);
                string url = $"{_gitLabUrl}/{_projectId}/repository/commits?ref_name={_branch}&path={Uri.EscapeDataString(filePath)}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    await BuildCommitsList(list, response);
                    return list;
                }
                else
                {
                    Console.WriteLine($"请求失败，状态码: {response.StatusCode}");
                }
            }
            return list;
        }
        /// <summary>
        /// 获取指定提交id的文件路径的内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="commitId"></param>
        /// <returns></returns>
        public static async Task<string> GetFileContent(string filePath, string commitId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _privateToken);
                string url = $"{_gitLabUrl}/{_projectId}/repository/files/{Uri.EscapeDataString(filePath)}?ref={commitId}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string fileContent = await response.Content.ReadAsStringAsync();
                    string content = JsonConvert.DeserializeObject<dynamic>(fileContent)?.content;
                    if (content != null)
                    {
                        var by = Convert.FromBase64String(content);
                        return Encoding.UTF8.GetString(by); 
                    }
                }
                else
                {
                    Console.WriteLine($"请求失败，状态码: {response.StatusCode}");
                }
            }
            return "";
        }
        /// <summary>
        /// 获取指定提交id的提交详情
        /// </summary>
        /// <param name="commitId"></param>
        /// <returns></returns>
        public static async Task<List<CommitDetails>> GetCommitDetail(string commitId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Private-Token", _privateToken);
                // 获取特定提交的内容明细
                string requestUrl = $"{_gitLabUrl}/{_projectId}/repository/commits/{commitId}/diff?unidiff=true";
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CommitDetails>>(responseBody);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            return null;
        }

    }
}
