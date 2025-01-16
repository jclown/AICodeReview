using AICodeReview.Web.Model;

namespace AICodeReview.Web;

public class ServiceApiClient(HttpClient httpClient)
{
    public async Task<List<CodeReviewList>> GetCodeReviewListAsync(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        List<CodeReviewList> list = [];

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<CodeReviewList>("/api/CodeReview/list?pageIndex=" + pageIndex, cancellationToken))
        {
            if (forecast is not null)
            {
                list.Add(forecast);
            }
        }
        return list;
    }
    public async Task<CodeReviewDetail> GetCodeReviewDetailAsync(string id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CodeReviewDetail>("/api/CodeReview/detail?id=" + id, cancellationToken);
    }



}
