namespace AICodeReview.Web.Model
{
    public class CodeReviewDetail
    {
        public string Id { get; set; }
        /// <summary>
        /// AI的回答
        /// </summary>
        public string AIAnswer { get; set; }
        /// <summary>
        /// 差异内容
        /// </summary>
        public List<DiffContent> DiffContents { get; set; }
    }
}
