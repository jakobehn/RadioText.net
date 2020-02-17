using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Orneholm.RadioText.Core.Storage;
using Orneholm.RadioText.Web.Models;

namespace Orneholm.RadioText.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISummaryStorage _summaryStorage;
        private readonly ImmersiveReaderOptions _immersiveReaderOptions;

        public HomeController(ISummaryStorage summaryStorage, IOptions<ImmersiveReaderOptions> immersiveReaderOptions)
        {
            _summaryStorage = summaryStorage;
            _immersiveReaderOptions = immersiveReaderOptions.Value;
        }

        [Route("/")]
        public async Task<IActionResult> Index(string entityName = null, string entityType = null, string keyphrase = null, string query = null)
        {
            var episodes = await _summaryStorage.ListSummarizedEpisode(100);

            var filteredEpisodes = FilterEpisodes(entityName, entityType, keyphrase, episodes);
            var searchedEpisodes = SearchEpisodes(query, filteredEpisodes);
            var orderedEpisodes = OrderEpisodes(searchedEpisodes);

            return View(new HomeIndexViewModel
            {
                SearchQuery = query,
                Episodes = orderedEpisodes.ToList()
            });
        }

        [Route("/episode/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var episode = await _summaryStorage.GetSummarizedEpisode(id);

            return View(new HomeDetailsViewModel
            {
                Episode = episode
            });
        }

        private static IOrderedEnumerable<SrStoredSummarizedEpisode> OrderEpisodes(List<SrStoredSummarizedEpisode> filteredEpisodes)
        {
            return filteredEpisodes.OrderByDescending(x => x.PublishDateUtc);
        }

        private static List<SrStoredSummarizedEpisode> SearchEpisodes(string query, List<SrStoredSummarizedEpisode> episodes)
        {
            var filteredEpisodes = episodes;

            if (!string.IsNullOrWhiteSpace(query))
            {
                filteredEpisodes = filteredEpisodes
                    .Where(x => x.Transcription_Original.Text.Contains(query)
                        || x.Transcription_EN.Text.Contains(query)
                        || x.Transcription_SV.Text.Contains(query))
                    .ToList();
            }

            return filteredEpisodes;
        }

        private static List<SrStoredSummarizedEpisode> FilterEpisodes(string entityName, string entityType, string keyphrase, List<SrStoredSummarizedEpisode> episodes)
        {
            var filteredEpisodes = episodes;
            if (!string.IsNullOrWhiteSpace(entityName))
            {
                filteredEpisodes = filteredEpisodes
                    .Where(x => x.Transcription_Original.Entities.Any(y => y.Name == entityName && (string.IsNullOrWhiteSpace(entityType) || y.Type == entityType)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(keyphrase))
            {
                filteredEpisodes = filteredEpisodes
                    .Where(x => x.Transcription_Original.KeyPhrases.Contains(keyphrase))
                    .ToList();
            }

            return filteredEpisodes;
        }


        [HttpGet("/api/immersivereader/token")]
        public async Task<JsonResult> GetTokenAndSubdomain()
        {
            var tokenResult = await GetTokenAsync();

            return new JsonResult(new
            {
                token = tokenResult,
                subdomain = _immersiveReaderOptions.Subdomain
            });
        }

        private async Task<string> GetTokenAsync()
        {
            var authority = $"https://login.windows.net/{_immersiveReaderOptions.TenantId}";
            const string resource = "https://cognitiveservices.azure.com/";

            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(_immersiveReaderOptions.ClientId, _immersiveReaderOptions.ClientSecret);

            var authResult = await authContext.AcquireTokenAsync(resource, clientCredential);

            return authResult.AccessToken;
        }
    }
}
