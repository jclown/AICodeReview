namespace AICodeReview.ApiService.Model
{
    public class AIAnswerDetail
    {
        public string Id { get; set; }
        /// <summary>
        /// AI的回答
        /// </summary>
        public string AIAnswer { get; set; }
      
        /// <summary>
        /// AI的回答类型 0=过程 1=汇总结果
        /// </summary>
        public int AIAnswerType { get;  set; }
        /// <summary>
        /// 提交记录id
        /// </summary>
        public string CommitId { get;  set; }
    }
}
