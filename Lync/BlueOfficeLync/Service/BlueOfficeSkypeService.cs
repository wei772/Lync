using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BlueOfficeSkype.Service
{
    public class BlueOfficeSkypeService
    {

        private string _baseUrl = "http://bocorp-test.ioffice100.net/tt";
        public void GetSkypeMeeting(Action<bool, GetSkypeMeetingResult> callback)
        {
            var url = string.Format("{0}/SkypeMeetings/Fetch?talkId={1}", _baseUrl, Guid.NewGuid());
            var client = new WebClient();
            var responseStr = client.DownloadString(url);

            dynamic resObject = JsonConvert.DeserializeObject(responseStr);
            if (resObject != null && resObject.Code == 0)
            {
                var skypeMeeting = resObject.SkypeMeeting;
                var skype = new GetSkypeMeetingResult() { TalkId = skypeMeeting.TalkId, Url = skypeMeeting.Url };
                if (callback != null)
                {
                    callback(true, skype);
                    return;
                }
            }

            if (callback != null)
            {
                callback(false, null);
            }


        }
    }
}
