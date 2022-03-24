using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using MusicStreaming.Handlers;

namespace MusicStreaming.Services
{
    public class Youtube
    {
        public async Task <List<string>> FindByType(string x)
        {

            string type = "";
            switch (x.ToLower().Trim())
            {
                case "rock":
                    type = "/m/06by7";
                    break;
                case "pop":
                    type = "/m/064t9";
                    break;
                case "hiphop":
                    type = "/m/0glt670";
                    break;
                default:
                    //return;
                    break;
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = GlobalData.Config.API,
                ApplicationName = this.GetType().ToString()
            });
            if (type == "") { return null; }
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.MaxResults = 15;
            searchListRequest.TopicId = type;
            searchListRequest.Type = "music";
            searchListRequest.RegionCode = "RU";
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.SearchSortUnspecified;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            foreach (var result in searchListResponse.Items) {
                videos.Add($"{result.Id.VideoId}");
            }
            return videos;
        }
    }
}
