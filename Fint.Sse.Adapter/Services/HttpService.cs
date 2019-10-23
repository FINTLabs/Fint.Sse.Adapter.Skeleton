﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fint.Sse.Adapter.Services
{
    public class HttpService : IHttpService
    {
        private readonly ILogger<HttpService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public HttpService(ILogger<HttpService> logger, HttpClient httpClient, ITokenService tokenService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _tokenService = tokenService;

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
        }

        public async void Post(string endpoint, Event<object> serverSideEvent)
        {
            var accessToken = "";

            if (_tokenService.OAuthEnabled)
            {
                var task = Task.Run(async () => await _tokenService.GetAccessTokenAsync());

                accessToken = task.Result;
            }


            if (_tokenService.OAuthEnabled)
            {
                _httpClient.SetBearerToken(accessToken);
            }

            var json = JsonConvert.SerializeObject(serverSideEvent);
            var content = new StringContent(json);
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");

            content.Headers.Add(FintHeaders.ORG_ID_HEADER, serverSideEvent.OrgId);
            content.Headers.ContentType = contentType;

            try
            {
                _logger.LogTrace("JSON event: {json}", json);
                var response = await _httpClient.PostAsync(endpoint, content);
                _logger.LogDebug("Provider {endpoint}: {reponse}",
                    endpoint, response.StatusCode);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Could not POST {event} to {endpoint}. Error: {error}",
                    serverSideEvent, endpoint, e.Message);
            }
        }
    }
}