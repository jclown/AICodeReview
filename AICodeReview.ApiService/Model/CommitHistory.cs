namespace AICodeReview.ApiService.Model
{
    public class CommitHistory
    {
        /// <summary>
        /// 提交记录id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 当前提交内容  
        /// </summary>
        public string CurrentContent { get; set; }
        /// <summary>
        /// 上次提交内容
        /// </summary>
        public string PreviousContent { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get;  set; }
        /// <summary>
        /// 提交记录id
        /// </summary>
        public string CommitId { get;  set; }
    }
}
