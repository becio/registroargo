using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace School.Core.Models
{
    public class LoginPageModel
    {
        public const string BaseAddress = "https://www.portaleargo.it";

        const string entryPointUrl = "/argoweb/famiglia/index1.jsp?cod_utente=";
        internal const string homeUrl = "/argoweb/famiglia/index.jsf";
        const string loginUrl = "/argoweb/famiglia/common/login_form2.jsp";
        const string postLoginUrl = "/argoweb/famiglia/common/j_security_check";

        LoginOptions _loginOptions;
        HttpClient _httpClient;

        public LoginPageModel(LoginOptions loginOptions) : this(CreateHttpClient(), loginOptions)
        {

        }

        public LoginPageModel(HttpClient httpClient, LoginOptions loginOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _loginOptions = loginOptions ?? throw new ArgumentNullException(nameof(loginOptions));
        }

        public static HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler()) { BaseAddress = new Uri(BaseAddress) };
            var headers = client.DefaultRequestHeaders;
            headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return client;
        }

        public async Task<HomePageModel> LoginAsync()
        {
            var ret = await _httpClient.GetAsync(entryPointUrl + _loginOptions.SchoolCode);
            if (ret.StatusCode != HttpStatusCode.Redirect)
                return null;
            
            ret = await _httpClient.GetAsync(homeUrl);
            ret.EnsureSuccessStatusCode();

            ret = await _httpClient.GetAsync(loginUrl);
            ret.EnsureSuccessStatusCode();

            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("utente", _loginOptions.Username),
                new KeyValuePair<string, string>("j_password", _loginOptions.Password),
                new KeyValuePair<string, string>("j_username", $"{_loginOptions.Username}#{_loginOptions.SchoolCode}"),
                new KeyValuePair<string, string>("submit", "Entra")
            });

            ret = await _httpClient.PostAsync(postLoginUrl, content);
            if (ret.StatusCode != HttpStatusCode.Found)
                return null;

            ret = await _httpClient.GetAsync(homeUrl);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();

            return new HomePageModel(_httpClient, html);
        }

    }
}
