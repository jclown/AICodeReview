namespace AICodeReview.ApiService.Vo
{
    /// <summary>
    /// 详情Vo
    /// </summary>
    public class CodeReviewDetailVo
    {
        /// <summary>
        /// AI的回答
        /// </summary>
        public string AIAnswer { get; set; }
        /// <summary>
        /// 差异内容
        /// </summary>
        public List<DiffContentVo> DiffContents { get; set; }
    }
}
