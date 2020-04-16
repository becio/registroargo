using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace School.Core.Models
{
    public class DownloadableFile
    {
        const string generatorUrl = "/argoweb/famiglia/objgeneratorservlet";

        readonly string _code;
        readonly HttpClient _httpClient;

        public DownloadableFile(HttpClient httpClient, string code, string name)
        {
            _httpClient = httpClient;
            _code = code;
            Name = name;
        }

        public string Name { get; }

        public async Task<DownloadResponse> DownloadAsync()
        {
            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", $"[evt=panel-messaggiBacheca:pannello|event|custom|riga|{_code}|operazione|download]"),
            });
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded; charset=UTF-8");
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.portaleargo.it/argoweb/famiglia/index.jsf");
            var ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();
            // check
            if (html.IndexOf("errore+di+sistema") > 0)
                throw new ArgumentException("Errore durante il download");

            ret = await _httpClient.PostAsync(generatorUrl, new StringContent(""));
            ret.EnsureSuccessStatusCode();

            return new DownloadResponse
            {
                ResponseStream = await ret.Content.ReadAsStreamAsync(),
                ContentType = ret.Content.Headers.ContentType,
                ContentLength = ret.Content.Headers.ContentLength
            };
        }
    }
}
