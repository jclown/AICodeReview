using Newtonsoft.Json;
namespace AICodeReview.ApiService.Model;
/// <summary>
/// 提交详情
/// </summary>
public class CommitDetails
{
    /// <summary>
    /// 差异信息
    /// </summary>
    [JsonProperty("diff")]
    public string Diff { get; set; }

    /// <summary>
    /// 新文件路径
    /// </summary>
    [JsonProperty("new_path")]
    public string NewPath { get; set; }

    /// <summary>
    /// 旧文件路径
    /// </summary>
    [JsonProperty("old_path")]
    public string OldPath { get; set; }

    /// <summary>
    /// A模式
    /// </summary>
    [JsonProperty("a_mode")]
    public string AMode { get; set; }

    /// <summary>
    /// B模式
    /// </summary>
    [JsonProperty("b_mode")]
    public string BMode { get; set; }

    /// <summary>
    /// 是否为新文件
    /// </summary>
    [JsonProperty("new_file")]
    public bool NewFile { get; set; }

    /// <summary>
    /// 是否为重命名文件
    /// </summary>
    [JsonProperty("renamed_file")]
    public bool RenamedFile { get; set; }

    /// <summary>
    /// 是否为删除文件
    /// </summary>
    [JsonProperty("deleted_file")]
    public bool DeletedFile { get; set; }
}