using School.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace School.Core.Models
{
    public class LoginModel
    {
        const string baseAddress = "https://www.portaleargo.it";
        const string entryPointUrl = "/argoweb/famiglia/index1.jsp?cod_utente=";
        internal const string homeUrl = "/argoweb/famiglia/index.jsf";
        const string loginUrl = "/argoweb/famiglia/common/login_form2.jsp";
        const string postLoginUrl = "/argoweb/famiglia/common/j_security_check";

        public string SchoolCode { get; set; }

        public string UserCode { get; set; }

        public string Password { get; set; }

        public async Task<HomePageModel> LoginAsync()
        {
            var httpClient = new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri(baseAddress) };

            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var ret = await httpClient.GetAsync(entryPointUrl + SchoolCode);
            if (ret.StatusCode != HttpStatusCode.Redirect)
                return null;
            
            ret = await httpClient.GetAsync(homeUrl);
            ret.EnsureSuccessStatusCode();

            ret = await httpClient.GetAsync(loginUrl);
            ret.EnsureSuccessStatusCode();

            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("utente", UserCode),
                new KeyValuePair<string, string>("j_password", Password),
                new KeyValuePair<string, string>("j_username", $"{UserCode}#{SchoolCode}"),
                new KeyValuePair<string, string>("submit", "Entra")
            });

            ret = await httpClient.PostAsync(postLoginUrl, content);
            if (ret.StatusCode != HttpStatusCode.Found)
                return null;

            ret = await httpClient.GetAsync(homeUrl);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();

            return new HomePageModel(httpClient, html);
        }

    }
}
