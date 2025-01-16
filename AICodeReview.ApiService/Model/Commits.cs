namespace AICodeReview.ApiService.Model
{
    /// <summary>
    /// 提交记录
    /// </summary>
    public class Commits
    {
        /// <summary>
        /// 提交记录Id
        /// </summary>
        public string? Id { get;  set; }
        /// <summary>
        /// 提交记录信息
        /// </summary>
        public string? Message { get;  set; }
        /// <summary>
        /// 提交人
        /// </summary>
        public string? AuthorName { get;  set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public string? CommitDate { get;  set; }
        /// <summary>
        /// 提交人邮箱
        /// </summary>
        public string? CommitEmail { get;  set; }
        /// <summary>
        /// 提交人姓名
        /// </summary>
        public string? CommitName { get;  set; }
        /// <summary>
        ///  状态
        /// </summary>
        public string? Status { get;  set; }
    }
}
