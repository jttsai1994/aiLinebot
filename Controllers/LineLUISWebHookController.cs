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
                    m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyLocationAction(
                            "搜尋附近的健身房"));
                    this.ReplyMessage(LineEvent.replyToken, m);
                    //response OK
                    return Ok();
                }
                //設定用戶回傳訊息類型為image時的回覆
                else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "image")
                {
                    Uri imgUri = new Uri("https://i.imgur.com/o9uT3Aa.png");
                    var msgs = new List<isRock.LineBot.MessageBase>();
                    var msgadd = new isRock.LineBot.TextMessage($"這照片真讚\n{profile.displayName}要運動了嗎");
                    msgadd.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyLocationAction(
                   "找附近的健身房"));
                    //add messages to 
                    msgs.Add(new isRock.LineBot.ImageMessage(imgUri, imgUri));
                    msgs.Add(msgadd);

                    this.ReplyMessage(LineEvent.replyToken, msgs);
                    //response OK
                    return Ok();
                }
                //設定用戶回傳訊息類型為text時的回覆
                else if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    var ret = MakeRequest(LineEvent.message.text);
                    //用戶回傳訊息為BMI時 此段為判斷intent="身體" 後 接續quickreply為BMI/TDEE的計算
                    if (LineEvent.message.text.ToUpper() == "BMI")
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        var msgadd1 = new isRock.LineBot.TextMessage("感謝您！");
                        var msgadd2 = new isRock.LineBot.TextMessage("-BMI\n身高:\n體重:");
                        //add messages to 
                        msgs.Add(new isRock.LineBot.TextMessage("請複製下面表格並填入資料後回覆~"));
                        msgs.Add(msgadd1);
                        msgs.Add(msgadd2);

                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        //response OK
                        return Ok();
                    }
                    else if (LineEvent.message.text.ToUpper() == "TDEE")
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        var msgadd = new isRock.LineBot.TextMessage("-TDEE\n身高:\n體重:\n年齡:\n性別:男/女");
                        //add messages to 
                        msgs.Add(new isRock.LineBot.TextMessage("請複製下面表格並填入資料後回覆~"));
                        msgs.Add(msgadd);

                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        //response OK
                        return Ok();
                    }

                    else if (LineEvent.message.text.Contains("-BMI")) // 算BMI
                    {
                        float height = float.Parse(LineEvent.message.text.Split("\n")[1].Split(":")[1]) / 100;
                        float weight = float.Parse(LineEvent.message.text.Split("\n")[2].Split(":")[1]);
                        float userbmi = weight / (height * height);
                        var bmiResult = String.Format($"Your BMI ： {userbmi:0.00} ");

                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        msgs.Add(new isRock.LineBot.TextMessage("經過我快速精密的計算過後...."));
                        var msgadd2 = new isRock.LineBot.TextMessage($"{bmiResult}");
                        msgs.Add(msgadd2);
                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        return Ok();
                    }

                    else if (LineEvent.message.text.Contains("-TDEE")) // 算TDEE
                    {
                        double height = double.Parse(LineEvent.message.text.Split("\n")[1].Split(":")[1]);
                        double weight = double.Parse(LineEvent.message.text.Split("\n")[2].Split(":")[1]);
                        double age = double.Parse(LineEvent.message.text.Split("\n")[3].Split(":")[1]);
                        string sex = LineEvent.message.text.Split("\n")[4].Split(":")[1];
                        int light, medi, high;
                        double BMR;
                        if (sex == "男")
                        {
                            BMR = (13.7 * weight) + (5 * height) - (6.8 * age) + 66;
                            light = Convert.ToInt32(BMR * 1.2);
                            medi = Convert.ToInt32(BMR * 1.375);
                            high = Convert.ToInt32(BMR * 1.55);
                        }
                        else if (sex == "女")
                        {
                            BMR = (9.6 * weight) + (1.8 * height) - (4.7 * age) + 655;
                            light = Convert.ToInt32(BMR * 1.2);
                            medi = Convert.ToInt32(BMR * 1.375);
                            high = Convert.ToInt32(BMR * 1.55);
                        }
                        else
                        {
                            responseMsg = "請輸入正確資料。";
                            this.ReplyMessage(LineEvent.replyToken, responseMsg);
                            //response OK
                            return Ok();
                        }
                        string userbmi = (weight / (height * height)).ToString();
                        var tdeeResult = String.Format($"Your 基礎代謝： {BMR:0.00}\n每日總消耗熱量\n久坐：{light}\n輕量活動：{medi}\n中量活動：{high}");
                        //一次回復訊息及計算結果
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        msgs.Add(new isRock.LineBot.TextMessage("經過我快速精密的計算過後...."));
                        var msgadd2 = new isRock.LineBot.TextMessage($"{tdeeResult}");
                        msgs.Add(msgadd2);
                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        //response OK
                        return Ok();
                    }
                    //若intent為打招呼 回傳default功能的Quickreply
                    else if (ret.topScoringIntent.intent.Contains("打招呼"))
                    {
                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"你好！{profile.displayName}！\n歡迎使用Smart AI Sports\n我能為你做下列的事情喔");
                        //在TextMessage物件的quickreply屬性中加入items
                        m.quickReply.items.Add(
                                new isRock.LineBot.QuickReplyMessageAction(
                                    $"想健身", "我想健身", new Uri("https://image.flaticon.com/295/png/512/1616/1616456.png?size=1200x630f")
                                ));
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"身體資訊", "想知道身體資訊", new Uri("https://image.flaticon.com/219/png/512/1754/1754237.png?size=1200x630f")
                            ));
                        m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyMessageAction(
                                $"聯絡我們", "聯絡我們", new Uri("https://i.imgur.com/rwwI5XW.png")
                            ));
                        m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyLocationAction(
                            "搜尋附近的健身房", new Uri("https://image.flaticon.com/179/png/512/458/458369.png?size=1200x630f")));


                        this.ReplyMessage(LineEvent.replyToken, m);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("訓練"))
                    {
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        //建立buttonsTemplate
                        var button = new isRock.LineBot.ButtonsTemplate()
                        {
                            altText = "altText",
                            text = "提供姿勢偵測",
                            title = "Smart AI Sports",
                            thumbnailImageUrl = new Uri("https://i.imgur.com/qOmWNyi.jpg")
                        };
                        //actions
                        button.actions.Add(new isRock.LineBot.UriAction() { label = "棒式動作示範", uri = new Uri("https://www.youtube.com/watch?v=UiklJLUh6qU") });

                        button.actions.Add(new isRock.LineBot.UriAction() { label = "二頭彎舉示範", uri = new Uri("https://www.youtube.com/watch?v=d71MiVhUBlw&t=395s") });
                        button.actions.Add(new isRock.LineBot.UriAction() { label = "臀推動作示範", uri = new Uri("https://www.youtube.com/watch?v=7i8iqg3s5sY") });
                        // button.actions.Add(new isRock.LineBot.CamerarollAction() { label = "上傳你的照片" });
                        // 
                        var msgs = new List<isRock.LineBot.MessageBase>();

                        // msgs.Add(new isRock.LineBot.TextMessage("請複製下面表格並填入資料後回覆O_o"));

                        foreach (var showlist in ret.entities)
                        {
                            Console.WriteLine(showlist.type);
                            Console.WriteLine(showlist.entity);
                            var trainlist = "";
                            if (showlist.type == "訓練名稱")
                            {
                                trainlist += showlist.entity;
                                msgs.Add(new isRock.LineBot.TextMessage($"看來你今天想練{trainlist}\n目前提供以下訓練選擇"));
                            }
                            else
                            {
                                msgs.Add(new isRock.LineBot.TextMessage($"請問你今天想練甚麼?\n目前提供以下訓練選擇"));

                            }
                            var ButtonsTmp = new isRock.LineBot.TemplateMessage(button);
                            msgs.Add(ButtonsTmp);
                            var msgadd2 = new isRock.LineBot.TextMessage("請用電腦開啟以下網站進行姿勢偵測");
                            msgs.Add(msgadd2);
                            var msgadd3 = new isRock.LineBot.TextMessage("http://192.168.36.30:3000/fitness");
                            msgs.Add(msgadd3);
                            this.ReplyMessage(LineEvent.replyToken, msgs);
                            //response OK
                            return Ok();
                        }
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
                        isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        var msgadd = new isRock.LineBot.TextMessage($"{profile.displayName}，這是我們能提供的服務");
                        // new isRock.LineBot.TextMessage m = 
                        //在TextMessage物件的quickreply屬性中加入items
                        msgadd.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"想健身", "我想健身", new Uri("https://image.flaticon.com/295/png/512/1616/1616456.png?size=1200x630f")));
                        msgadd.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"身體資訊", "想知道身體資訊", new Uri("https://image.flaticon.com/219/png/512/1754/1754237.png?size=1200x630f")));
                        msgadd.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyLocationAction(
                            "搜尋附近的健身房", new Uri("https://image.flaticon.com/179/png/512/458/458369.png?size=1200x630f")));
                        //add messages to 
                        msgs.Add(new isRock.LineBot.TextMessage("連絡電話：0911-222-333\n連絡信箱：aisports@hotmail.com"));
                        msgs.Add(msgadd);

                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        //response OK
                        return Ok();
                    }
                    else if (ret.topScoringIntent.intent.Contains("感謝"))
                    {
                        Uri imgUri = new Uri("https://i.imgur.com/o9uT3Aa.png");
                        var msgs = new List<isRock.LineBot.MessageBase>();
                        var msgadd = new isRock.LineBot.ImageMessage(imgUri, imgUri);
                        //add messages to 
                        msgs.Add(new isRock.LineBot.TextMessage("很高興能幫助到您！要繼續運動保持健康喔！"));
                        msgs.Add(msgadd);

                        this.ReplyMessage(LineEvent.replyToken, msgs);
                        //response OK
                        return Ok();

                    }
                    else if (ret.topScoringIntent.intent.Contains("None"))
                    {

                        isRock.LineBot.TextMessage m = new isRock.LineBot.TextMessage($"Sorry~{profile.displayName}，我不太清楚你的意思，不過我能為你做下列的事情喔");
                        //在TextMessage物件的quickreply屬性中加入items
                        m.quickReply.items.Add(
                                new isRock.LineBot.QuickReplyMessageAction(
                                    $"想健身", "我想健身", new Uri("https://image.flaticon.com/295/png/512/1616/1616456.png?size=1200x630f")
                                ));
                        m.quickReply.items.Add(
                            new isRock.LineBot.QuickReplyMessageAction(
                                $"身體資訊", "想知道身體資訊", new Uri("https://image.flaticon.com/219/png/512/1754/1754237.png?size=1200x630f")
                            ));
                        m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyMessageAction(
                                $"聯絡我們", "聯絡我們", new Uri("https://i.imgur.com/rwwI5XW.png")
                            ));
                        m.quickReply.items.Add(
                        new isRock.LineBot.QuickReplyLocationAction(
                            "搜尋附近的健身房", new Uri("https://image.flaticon.com/179/png/512/458/458369.png?size=1200x630f")));

                        this.ReplyMessage(LineEvent.replyToken, m);
                        //response OK
                        return Ok();
                    }
                    // else if (LineEvent.message.text.Contains("自訂文字"))
                    // {
                    //     //    ...
                    // }

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
                    double locx = LineEvent.message.latitude;
                    double locy = LineEvent.message.longitude;
                    string gymlink = $"打開以下連結查看你附近的健身房\nhttps://www.google.com.tw/maps/search/%E5%81%A5%E8%BA%AB%E6%88%BF/@{locx},{locy},15z/";
                    isRock.LineBot.Bot bot = new isRock.LineBot.Bot(this.ChannelAccessToken);
                    var msgs = new List<isRock.LineBot.MessageBase>();
                    var msgadd = new isRock.LineBot.TextMessage(gymlink);
                    //add messages to 
                    msgs.Add(new isRock.LineBot.TextMessage($"{profile.displayName}....你是不是在{LineEvent.message.address.Substring(3)}想找健身房"));
                    msgs.Add(msgadd);
                    this.ReplyMessage(LineEvent.replyToken, msgs);
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