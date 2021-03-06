﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DataAugmentation.Xem.Model;

namespace NzbDrone.Core.DataAugmentation.Xem
{
    public interface IXemProxy
    {
        List<int> GetXemSeriesIds();
        List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id);
        List<SceneMapping> GetSceneTvdbNames();
    }

    public class XemProxy : IXemProxy
    {
        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;

        private const string XEM_BASE_URL = "http://thexem.de/map/";

        private static readonly string[] IgnoredErrors = { "no single connection", "no show with the tvdb_id" };
        private HttpRequestBuilder _xemRequestBuilder;


        public XemProxy(Logger logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            _xemRequestBuilder = new HttpRequestBuilder(XEM_BASE_URL)
            {
                PostProcess = r => r.UriBuilder.SetQueryParam("origin", "tvdb")
            };
        }


        public List<int> GetXemSeriesIds()
        {
            _logger.Debug("Fetching Series IDs from");

            var request = _xemRequestBuilder.Build("/havemap");
            var response = _httpClient.Get<XemResult<List<int>>>(request).Resource;
            CheckForFailureResult(response);

            return response.Data.ToList();
        }

        public List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id)
        {
            _logger.Debug("Fetching Mappings for: {0}", id);


            var request = _xemRequestBuilder.Build("/all");
            request.UriBuilder.SetQueryParam("id", id);

            var response = _httpClient.Get<XemResult<List<XemSceneTvdbMapping>>>(request).Resource;

            return response.Data.Where(c => c.Scene != null).ToList();
        }

        public List<SceneMapping> GetSceneTvdbNames()
        {
            _logger.Debug("Fetching alternate names");

            var request = _xemRequestBuilder.Build("/allNames");
            request.UriBuilder.SetQueryParam("seasonNumbers", true);

            var response = _httpClient.Get<XemResult<Dictionary<Int32, List<JObject>>>>(request).Resource;

            var result = new List<SceneMapping>();

            foreach (var series in response.Data)
            {
                foreach (var name in series.Value)
                {
                    foreach (var n in name)
                    {
                        int seasonNumber;
                        if (!Int32.TryParse(n.Value.ToString(), out seasonNumber))
                        {
                            continue;
                        }

                        //hack to deal with Fate/Zero 
                        if (series.Key == 79151 && seasonNumber > 1)
                        {
                            continue;
                        }

                        result.Add(new SceneMapping
                                   {
                                       Title = n.Key,
                                       SearchTerm = n.Key,
                                       SeasonNumber = seasonNumber,
                                       TvdbId = series.Key
                                   });
                    }
                }
            }

            return result;
        }

        private static void CheckForFailureResult<T>(XemResult<T> response)
        {
            if (response.Result.Equals("failure", StringComparison.InvariantCultureIgnoreCase) &&
                !IgnoredErrors.Any(knowError => response.Message.Contains(knowError)))
            {
                throw new Exception("Error response received from Xem: " + response.Message);
            }
        }
    }
}
