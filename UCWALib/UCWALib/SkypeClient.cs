using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UCWALib.Models;

namespace UCWALib
{
    public class SkypeClient
    {
        //Sample data of autoDiscoverInfo
        //{
        //  "_links": {
        //    "self": {
        //      "href": "https://webdir0f.online.lync.com/Autodiscover/AutodiscoverService.svc/root"
        //    },
        //    "user": {
        //      "href": "https://webdir0f.online.lync.com/Autodiscover/AutodiscoverService.svc/root/oauth/user"
        //    },
        //    "xframe": {
        //      "href": "https://webdir0f.online.lync.com/Autodiscover/XFrame/XFrame.html"
        //    }
        //  }
        //}
        private dynamic _autoDiscoverInfo = null;

        private dynamic _user = null;

        private string _token = null;

        private dynamic _application = null;

        private string _eventUrl = null;

        private Uri _appUri = null;

        private string _threadId = null;

        public async Task<bool> StartUp()
        {
            _autoDiscoverInfo = await GetJson("https://webdir.online.lync.com/autodiscover/autodiscoverservice.svc/root");
            return true;
        }

        public async Task<string> GetAccessToken(string account, string password)
        {
            var phantomjsFile = HttpContext.Current == null
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"phantomjs\phantomjs.exe")
                : HttpContext.Current.Server.MapPath("~/bin/phantomjs/phantomjs.exe");
            var scriptFile = HttpContext.Current == null
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"phantomjs\getskypeaccesstoken.js")
                : HttpContext.Current.Server.MapPath("~/bin/phantomjs/getskypeaccesstoken.js");
            string output = null;
            await Task.Run(() =>
            {
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = phantomjsFile,
                        Arguments = $"\"{scriptFile}\" \"{account}\" \"{password}\"",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                })
                {
                    process.Start();
                    if (!process.WaitForExit((int) TimeSpan.FromSeconds(60).TotalMilliseconds))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignored
                        }

                        throw new Exception("Timeout!");
                    }
                    output = process.StandardOutput.ReadToEnd().Trim();
                }
            });
            if (output.StartsWith("ACCESSTOKEN:"))
            {
                return output.Substring("ACCESSTOKEN:".Length);
            }
            if (output.StartsWith("ERROR:"))
            {
                throw new Exception(output.Substring("ERROR:".Length));
            }
            throw new Exception("Unkown Error.");
        }

        public string GetResourceUri()
        {
            if (_autoDiscoverInfo == null)
            {
                throw new InvalidOperationException("Execute StartUp first.");
            }
            var uri = new UriBuilder((string) _autoDiscoverInfo._links.self.href) {Path = ""};
            return uri.Uri.ToString();
        }

        public string GetTokenUri(string clientId, string redirectUrl, bool adminConsent = false)
        {
            return string.Format("https://login.windows.net/common/oauth2/authorize?response_type=token&client_id={1}&redirect_uri={2}&resource={0}{3}", HttpUtility.UrlEncode(GetResourceUri()), HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(redirectUrl), adminConsent ? "&prompt=admin_consent" : "");
        }

        public async Task<bool> SignIn(string token)
        {
            if (_autoDiscoverInfo == null)
            {
                throw new InvalidOperationException("Please execute StartUp first.");
            }
            _user = await GetJson((string)_autoDiscoverInfo._links.user.href, null, token);
            _token = token;

            _appUri = new Uri((string)_user._links.applications.href);
            _application = await GetJson(_appUri.ToString(), JsonConvert.SerializeObject(new
            {
                UserAgent = "SkypeWeb/master_0.4.155 SkypeOnlinePreviewApp/1.0.0",
                Culture = "en-us",
                EndpointId = Guid.NewGuid()
            }), _token);
            
            return true;
        }

        public async Task<OnlineMeeting> StartMeeting(string subject)
        {
            if (_user == null)
            {
                throw new InvalidOperationException("Please SignIn first.");
            }

            // Start Meeting
            var operationId = Guid.NewGuid();
            _threadId = Guid.NewGuid().ToString("N");
            //var threadId = "AdGrXoJjJVdXDbaSS7SA/wagio8IFg==";
            var startMeetingUri = GetUri(_appUri, (string)_application._embedded.communication._links.startOnlineMeeting.href);

            await GetJson(startMeetingUri,
                JsonConvert.SerializeObject(new
                {   
                    operationId = operationId,
                    threadId = _threadId,
                    subject = subject,
                    importance = "Normal"
                }), _token);

            // Get Start Meeting Result Event
            return GetOnlineMeetingFromEvent(await GetNextEventObject());
        }

        public async Task<dynamic> GetNextEventObject()
        {
            if (_eventUrl == null)
            {
                _eventUrl = _application._links.events.href;
            }
            var eventsUri = GetUri(_appUri, _eventUrl);
            var eventObj =  await GetJson(eventsUri, null, _token);
            _eventUrl = eventObj._links.next.href;
            return eventObj;
        }

        //public async Task AddMessaging(OnlineMeeting meeting)
        //{
        //    if (_user == null)
        //    {
        //        throw new InvalidOperationException("Please SignIn first.");
        //    }
        //    var appUri = new Uri((string) _user._links.applications.href);
        //    var addMessagingUri = GetUri(appUri, meeting.Links["addMessaging"]);
        //    var operationId = Guid.NewGuid();

        //    await GetJson(GetUri(appUri, meeting.Links["conversation"]), null, _token);

        //    await GetJson(addMessagingUri,
        //        JsonConvert.SerializeObject(new
        //        {
        //            operationId,
        //            _threadId
        //        }), _token);
        //    await GetNextEventObject();

        //}

        public async Task TerminateMeeting(OnlineMeeting meeting)
        {
            if (_user == null)
            {
                throw new InvalidOperationException("Please SignIn first.");
            }
            var appUri = new Uri((string)_user._links.applications.href);
            //var terminateUri = GetUri(appUri, meeting.Links["conversation"]) + "/messaging/terminate";
            //await GetJson(terminateUri, "", _token, "POST");
            await GetJson(GetUri(appUri, meeting.Links["conversation"]), null, _token, "DELETE");
            //await GetNextEventObject();
        }

        private OnlineMeeting GetOnlineMeetingFromEvent(dynamic eventObj)
        {
            if (eventObj == null) return null;
            var conversation = ((JArray)eventObj.sender).OfType<dynamic>().FirstOrDefault(t => t.rel == "conversation");
            if (conversation != null)
            {
                var onlineMeetingEvent = ((JArray)conversation.events).OfType<dynamic>().FirstOrDefault(t => t.link.rel == "onlineMeeting");
                if (onlineMeetingEvent != null)
                {
                    var onlineMeetingJObject = (JObject) onlineMeetingEvent._embedded.onlineMeeting;
                    var onlineMeeting = onlineMeetingJObject.ToObject<OnlineMeeting>();
                    onlineMeeting.Links = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (var link in onlineMeetingJObject.GetValue("_links").Children<JProperty>())
                    {
                        onlineMeeting.Links.Add(link.Name, ((JObject)link.Value).GetValue("href").Value<string>());
                    }

                    //var messagingEvent = ((JArray)conversation.events).OfType<dynamic>().FirstOrDefault(t => t.link.rel == "messaging");
                    //if (messagingEvent != null)
                    //{
                    //    onlineMeeting.Links.Add("addMessaging", (string)messagingEvent._embedded.messaging._links.addMessaging.href);
                    //}

                    return onlineMeeting;
                }
            }
            return null;
        }

        private string GetUri(Uri baseUri, string relativeUri)
        {
            var queryIdx = relativeUri.IndexOf("?");
            if (queryIdx > -1)
            {
                return new Uri(baseUri, relativeUri.Substring(0, queryIdx)).ToString() + relativeUri.Substring(queryIdx);
            }
            return new Uri(baseUri, relativeUri).ToString();
        }

        private async Task<dynamic> GetJson(string uri, string postString = null, string accessToken = null, string method = null)
        {
            var request = System.Net.WebRequest.Create(uri);
            request.Method = method ?? "GET";
            if (!String.IsNullOrEmpty(accessToken))
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            if (postString != null)
            {
                request.ContentType = "application/json";
                request.Method = method ?? "POST";
                var postData = Encoding.UTF8.GetBytes(postString);
                request.ContentLength = postData.Length;
                request.GetRequestStream().Write(postData, 0, postData.Length);
            }

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream, true))
                {
                    return JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
                }
            }
        }
    }
}
