using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace School.Core.Models
{
    public class HomePageModel
    {
        #region fields & consts
        public const string SupportedVersion = "Versione 3.6.1";
        const string StudentsContainerId = "_idJsp75_wrapper";
        const string StudentNameId = "_idJsp72";    // span id nome studente
        const string ClassNameId = "_idJsp84";      // span id nome classe
        const string VersionId = "_idJsp104";       // span id nome versione
        const string SwitchStudentId = "_idJsp75";  // div id per attivare switch studente
        const string AssignedTasksId = "_idJsp27";  // \
        const string AssignedTasks2Id = "_idJsp26"; //  -> ids associati ai compiti assegnati
        const string AssignedTasks3Id = "_idJsp5";  // /

        readonly HttpClient _httpClient;
        readonly HtmlDocument _htmlDocument;
        string[] _students;

        Func<string> _getStudent;
        Func<string> _getClass;
        #endregion

        #region properties
        public string Student => _getStudent();

        public string Class => _getClass();

        public string[] AvailableStudents => _students ?? (_students = ParseStudents());

        public string Version => _htmlDocument.GetElementbyId(VersionId)?.InnerText;
        #endregion

        #region ctor
        internal HomePageModel(HttpClient httpClient, string html)
        {
            _httpClient = httpClient;

            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(html);

            _getStudent = () => _htmlDocument.GetElementbyId(StudentNameId).InnerText.Clean();
            _getClass = () => _htmlDocument.GetElementbyId(ClassNameId).InnerText.Clean();
        }
        #endregion

        #region public methods
        public async Task<HomeTasksModel> AssignedTasksAsync()
        {
            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", $"[evt=menu-serviziclasse:{AssignedTasksId}|event|submit][att={AssignedTasks2Id}|selected|true][att={AssignedTasks3Id}|selected|false]")
            });

            var ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();
            return new HomeTasksModel(html);
        }

        public async Task<BulletinBoardModel> BulletinBoardAsync()
        {
            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", "[evt=bacheca|event|submit|]")
            });
            var ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();

            var html = await ret.Content.ReadAsStringAsync();
            content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", "[evt=sheet-bacheca:tree|event|custom|param1||param2||param3||param4||param5|]")
            });

            ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();
            html = await ret.Content.ReadAsStringAsync();
            return new BulletinBoardModel(_httpClient, html);
        }

        public async Task SwitchToStudentAsync(string studentName)
        {
            if (string.IsNullOrEmpty(studentName))
                throw new ArgumentNullException(nameof(studentName));

            if (!AvailableStudents.Contains(studentName))
                throw new ArgumentException("Student not found");

            var idx = Array.IndexOf(AvailableStudents, studentName);

            var content = new FormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("BackbaseClientDelta", $"[evt={SwitchStudentId}|event|row-selected|rowIndex|{idx}][att={SwitchStudentId}|selectedIndexes|{idx}]")
            });
            var ret = await _httpClient.PostAsync(LoginPageModel.homeUrl, content);
            ret.EnsureSuccessStatusCode();
            var html = await ret.Content.ReadAsStringAsync();

            // adesso dobbiamo parsare la risposta e sperare di mettere tutte le cose a posto
            // fortunatamente sembra che lo stato rimanga sul server e che basti quindi solo reimpostare il nome utente e la classe
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var setTexts = doc.DocumentNode.Descendants("c:setText").ToList();
            var setStudent = setTexts.First(n => n.Attributes["destination"].Value == $"id('{StudentNameId}')");
            var setClass = setTexts.First(n => n.Attributes["destination"].Value == $"id('{ClassNameId}')");
            var newStudentName = setStudent.Attributes["select"].Value.Trim('\'');
            var newClassName = setClass.Attributes["select"].Value.Trim('\'').Clean();

            _getStudent = () => newStudentName;
            _getClass = () => newClassName;
        }
        #endregion

        #region private methods
        string[] ParseStudents()
        {
            var div = _htmlDocument.GetElementbyId(StudentsContainerId);
            var rows = div.Descendants("tr");
            var list = new List<string>();
            foreach (var tr in rows)
            {
                var span = tr.Descendants("span").First(n => n.Attributes.Any(a => a.Name == "id"));
                var name = span.InnerText.Clean();
                list.Add(name);
            }

            return list.ToArray();
        } 
        #endregion
    }
}
