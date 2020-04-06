using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace School.Core.Models
{
    public class BulletinBoardModel
    {
        static readonly string[] Months = new[] { "Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dec" };

        readonly HttpClient _httpClient;

        internal BulletinBoardModel(HttpClient httpClient, string html)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<BulletinBoardRow>> School()
        {
            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", "[evt=sheet-bacheca:tree:scuola|event|select][att=sheet-bacheca:tree:scuola|selected|true]")
            });

            var ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tableNode = doc.DocumentNode.Descendants("table").First();
            var trs = tableNode.Element("tbody").ChildNodes.Elements("tr");
            var list = new List<BulletinBoardRow>();
            var rowIndex = 1;
            foreach(var tr in trs)
            {
                var row = ParseRow(tr, rowIndex++);
                list.Add(row);
            }
            return list.AsReadOnly();
        }

        BulletinBoardRow ParseRow(HtmlNode tr, int rowIndex)
        {
            var td = tr.SelectSingleNode("td[1]/table/tbody/tr[1]/td");
            var value = td.InnerText.Clean();
            var parts = value.Split('/');
            var month = parts[0];
            var yr = int.Parse(parts[1]) + 2000;

            td = tr.SelectSingleNode("td[1]/table/tbody/tr[2]/td");
            var day = int.Parse(td.InnerText.Clean());
            var midx = Array.IndexOf(Months, month)+1;

            var row = new BulletinBoardRow
            {
                Date = new DateTime(yr, midx, day)
            };

            var trs = tr.SelectNodes("td[2]/table/tr");
            var files = new List<DownloadableFile>();
            var subRowIndex = 1;
            foreach (var itr in trs)
            {
                var firstTd = itr.SelectSingleNode("td[1]");
                var secondTd = itr.SelectSingleNode("td[2]"); 

                var rowContentType = firstTd.InnerText;
                if (rowContentType == "Oggetto:")
                {
                    row.Subject = secondTd.InnerText.Clean();
                }
                else if (rowContentType == "Messaggio:")
                {
                    row.Message = secondTd.InnerText.Clean();
                }
                else if (rowContentType == "File:")
                {
                    var linkText = secondTd.InnerText.Clean();

                    var code = rowIndex + "_" + subRowIndex;
                    
                    var file = new DownloadableFile(_httpClient, code, linkText);
                    files.Add(file);
                }
                else if (rowContentType == "Url:")
                {
                    var urlLink = secondTd.InnerText.Clean();
                    row.Url = urlLink;
                }
                else if (rowContentType == "Presa Visione:")
                {
                    // ignoriamo
                }
                else if (rowContentType == "Adesione:")
                {
                    // ignoriamo
                }
                else throw new ApplicationException("Could not parse bulleting board");
            }
            
            row.Files = files.AsReadOnly();
            return row;
        }
    }
}
