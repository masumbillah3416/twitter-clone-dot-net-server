using Core.Dtos;

namespace Core.Interfaces
{
    public interface ITimeLineService
    {
        Task<PaginatedTweetResponseDto> GetUserTimeLine(string userId,int size, int page);
        Task<PaginatedTweetResponseDto> GetNewsFeed( int size, int page);
    }
}