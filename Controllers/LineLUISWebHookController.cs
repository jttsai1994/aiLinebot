using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Web;


namespace isRock.Template
{
    public class LineLUISWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        const string key = "adbd4303dc644ca78c5c86fb4846d050";
        const string endpoint = "westus.api.cognitive.microsoft.com";
        const string appId = "872c840a-1e2f-407d-a17e-6760627903d8";

        [Route("api/LineLUIS")]
        [HttpPost]
        public IActionResult POST()
        {
            var AdminUserId = "U3642fd7bd605b4781e64effed44ec97e";

            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = "dt0W9B9ztQWfHmg1e3ifpp+xEvHTarYhi4SnIvrxuODgAVq5qgi19pWLPIAKqJ4VQnfPt9BTg5EkLshGWLbVM5u/C0RAzHXWs2EMdwxMLJNdJ0uHazOIDgI9NDHTdxZNy0NGNUwvzJpy6k1s467+dwdB04t89/1O/w1cDnyilFU=";
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                var profile = GetUserInfo(LineEvent.source.userId);

                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                var responseMsg = "";
                //準備回覆訊息
                if (LineEvent.type.ToLower() == "follow")
                {
                    isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"{profile.displayName}，使用Smart AI Sports幫助你一起健身吧!");
                    //在TextMessage物件的quickreply屬性中加入items
                    m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"想健身", "我想健身"
                            ));
                    m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyMessageAction(
                            $"身體資訊", "想知道身體資訊"
                        ));
                    m.quickReply.items.Add(
                    new isRock.LineBot.QuickReplyMessageAction(
                            $"聯絡我們", "聯絡我們"
                        ));
                    this.ReplyMessage(LineEvent.replyToken, m);
                    //response OK
                    return Ok();
                }
                // else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "image")
                // {

                // }
                else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    var ret = MakeRequest(LineEvent.message.text);


                    if (LineEvent.message.text.Contains("BMI")||LineEvent.message.text.Contains("TDEE"))
                    {
                        responseMsg="Code還沒打好>< 還不能計算";
                        this.ReplyMessage(LineEvent.replyToken, responseMsg);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("訓練"))
                    {
                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"請問你想做哪種運動\n很抱歉目前只提供以下訓練選擇");
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                    $"棒式", "棒式"
                                ));
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                    $"深蹲", "深蹲"
                                ));
                        this.ReplyMessage(LineEvent.replyToken, m);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("身體"))
                    {
                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"請問你想知道哪種資訊");
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                    $"BMI(身體質量指數)", "BMI"
                                ));
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                    $"TDEE(基礎代謝率)", "TDEE"
                                ));
                        this.ReplyMessage(LineEvent.replyToken, m);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("客服"))
                    {
                        responseMsg = "聯絡電話:0911-222-333\n聯絡信箱:\naisports@hotmail.com";
                        this.ReplyMessage(LineEvent.replyToken, responseMsg);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("None"))
                    {
                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"Sorry~{profile.displayName}，我不太清楚你的意思，不過我能為你做下列的事情喔");
                        //在TextMessage物件的quickreply屬性中加入items
                        m.quickReply.items.Add(
                                new isRock.LineBot.QuickReplyMessageAction(
                                    $"想健身", "我想健身"
                                ));
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"身體資訊", "想知道身體資訊"
                            ));
                        m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyMessageAction(
                                $"聯絡我們", "聯絡我們"
                            ));
                        this.ReplyMessage(LineEvent.replyToken, m);
                        //response OK
                        return Ok();
                    }
                    else if (LineEvent.message.text.Contains("自訂文字"))
                    {
                        //    ...
                    }

                }
                else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "sticker")
                {
                    responseMsg = $"{profile.displayName}有空傳貼圖，倒不如多做兩個伏地挺身";
                    this.ReplyMessage(LineEvent.replyToken, responseMsg);
                    //response OK
                    return Ok();
                }
                else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "location")
                {
                    responseMsg = $"{profile.displayName}....你是不是在{LineEvent.message.address.Substring(3)}想找健身房";
                    this.ReplyMessage(LineEvent.replyToken, responseMsg);
                    //response OK
                    return Ok();
                }
                else
                    responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, responseMsg);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //回覆訊息
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }


        static LUISResult MakeRequest(string utterance)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["query"] = utterance;

            // These optional request parameters are set to their default values
            queryString["verbose"] = "true";
            queryString["show-all-intents"] = "true";
            queryString["staging"] = "false";
            queryString["timezoneOffset"] = "0";

            var endpointUri = String.Format("https://{0}/luis/v2.0/apps/{1}?verbose=true&timezoneOffset=0&subscription-key={3}&q={2}", endpoint, appId, queryString, key);

            var response = client.GetAsync(endpointUri).Result;

            var strResponseContent = response.Content.ReadAsStringAsync().Result;
            var Result = Newtonsoft.Json.JsonConvert.DeserializeObject<LUISResult>(strResponseContent);
            // Display the JSON result from LUIS
            return Result;
        }
    }

    #region "LUIS Model"

    public class TopScoringIntent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Resolution
    {
        public string value { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
        public Resolution resolution { get; set; }
    }

    public class LUISResult
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Intent> intents { get; set; }
        public List<Entity> entities { get; set; }
    }
    #endregion
}