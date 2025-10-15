using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class MiscTests : BaseTests
    {
        [TestMethod]
        public async Task DailyScan()
        {
            DateTime midnight = Convert.ToDateTime("2025-03-11");
            string result = DBHelper.Serialize(await _homeContext.DailyScan(midnight));
            Assert.AreNotEqual(0, result.Length);
        }

        [TestMethod]
        public async Task DailyMonitor()
        {
            string result = await ControllerSvc.DailyMonitor();
            OpenInNotepad(result);
            Assert.AreNotEqual(0, result.Length);
        }

        [TestMethod]
        public async Task Message()
        {
            //List<Message> msgs = await _awsContext.Message.OrderByDescending(x => x.Id).ToListAsync();
            var sql = "select top 200 id, createdon, subject, body from message order by id desc";
            string json = await _awsContext.FetchJson(sql, null, 30);
            List<Message> msgs = DBHelper.Deserialize<List<Message>>(json);

            foreach (Message m in msgs)
            {
                m.Body = m.Body.Replace("\'", "\u0022").Replace("<", "\u003C").Replace(">", "\u003E").Replace("\\n", "\n").Replace("\\r", "\r");
            }
            json = DBHelper.Serialize(msgs);
            Assert.AreNotEqual(0, json.Length);
        }

        [TestMethod]
        public async Task unicode()
        {
            string encoded = "request={\\u0022version\\u0022:\\u00222.0\\u0022,\\u0022routeKey\\u0022:\\u0022$default\\u0022,\\u0022rawPath\\u0022:\\u0022/api/python/GetDailyScan\\u0022,\\u0022rawQueryString\\u0022:\\u0022midnight=03%2F14%2F2025\\u0022,\\u0022headers\\u0022:{\\u0022sec-fetch-mode\\u0022:\\u0022cors\\u0022,\\u0022referer\\u0022:\\u0022https://maximum-pain.com/\\u0022,\\u0022x-amzn-tls-version\\u0022:\\u0022TLSv1.3\\u0022,\\u0022sec-fetch-site\\u0022:\\u0022cross-site\\u0022,\\u0022x-forwarded-proto\\u0022:\\u0022https\\u0022,\\u0022accept-language\\u0022:\\u0022en-US,en;q=0.9\\u0022,\\u0022origin\\u0022:\\u0022https://maximum-pain.com\\u0022,\\u0022x-forwarded-port\\u0022:\\u0022443\\u0022,\\u0022x-forwarded-for\\u0022:\\u002273.43.16.191\\u0022,\\u0022accept\\u0022:\\u0022application/json, text/plain, */*\\u0022,\\u0022x-amzn-tls-cipher-suite\\u0022:\\u0022TLS_AES_128_GCM_SHA256\\u0022,\\u0022sec-ch-ua\\u0022:\\u0022\\\\u0022Chromium\\\\u0022;v=\\\\u0022134\\\\u0022, \\\\u0022Not:A-Brand\\\\u0022;v=\\\\u002224\\\\u0022, \\\\u0022Google Chrome\\\\u0022;v=\\\\u0022134\\\\u0022\\u0022,\\u0022x-amzn-trace-id\\u0022:\\u0022Root=1-67d81795-059f0c8f5f39b11e0a0f7723\\u0022,\\u0022sec-ch-ua-mobile\\u0022:\\u0022?0\\u0022,\\u0022sec-ch-ua-platform\\u0022:\\u0022\\\\u0022Windows\\\\u0022\\u0022,\\u0022host\\u0022:\\u0022hcapr4ndhwksq5dq7ird3yujp\"\r\n";
            string decoded = Utility.DecodeEncodedNonAsciiCharacters(encoded);
            OpenInNotepad($"{encoded}\r\n\r\n\r\n{decoded}");
            Assert.AreNotEqual(encoded, decoded);
        }



        [TestMethod]
        public async Task Serialization()
        {
            var ds = new DailyScan()
            {
                Ticker = "AAPL",
                Volume = 200,
            };

            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(ds, jsonOptions);
            OpenInNotepad(json);
        }

        [TestMethod]
        public async Task Serialization2()
        {
            string json = @"
    [
      {
        ""Id"": 70049,
        ""Ticker"": ""GILD"",
        ""Source"": ""sp500"",
        ""CurrentPrice"": 114.43,
        ""RSRating"": 114.9731742416816,
        ""Sma10Day"": 114.8,
        ""Sma20Day"": 110.65,
        ""Sma50Day"": 100.47,
        ""Sma150Day"": 89.86,
        ""Sma200Day"": 84.26,
        ""Week52Low"": 61.56,
        ""Week52High"": 117.41,
        ""Volume"": 11478000,
        ""Volume20"": 9854190,
        ""VolumePerc"": 16.47837112943834,
        ""ADR"": 4.726092276428438,
        ""BBUpper"": 121.4643070612353,
        ""BBMiddle"": 110.6475,
        ""BBLower"": 99.83069293876468,
        ""BBW"": 19.551832732299076,
        ""Date"": ""2025-03-11T00:00:00"",
        ""CreatedOn"": ""2025-03-12T12:23:03.033"",
        ""ProgressCurrentPrice"": 111.44,
        ""ProgressModifiedOn"": ""2025-03-17T12:24:00"",
        ""WatchFlag"": {}
      },
      {
        ""Id"": 70060,
        ""Ticker"": ""LICN"",
        ""Source"": ""nasdaq"",
        ""CurrentPrice"": 4.8,
        ""RSRating"": 111.68804691570561,
        ""Sma10Day"": 3.81,
        ""Sma20Day"": 1.94,
        ""Sma50Day"": 0.86,
        ""Sma150Day"": 1.43,
        ""Sma200Day"": 1.49,
        ""Week52Low"": 0.04,
        ""Week52High"": 6,
        ""Volume"": 29600,
        ""Volume20"": 28551210,
        ""VolumePerc"": -99.89632663554364,
        ""ADR"": 43.851310435385905,
        ""BBUpper"": 7.066916454768981,
        ""BBMiddle"": 1.9380000000000002,
        ""BBLower"": -3.19091645476898,
        ""BBW"": 529.2999437326089,
        ""Date"": ""2025-03-11T00:00:00"",
        ""CreatedOn"": ""2025-03-12T12:23:03.033"",
        ""ProgressCurrentPrice"": 4.45,
        ""ProgressModifiedOn"": ""2025-03-17T12:24:00"",
        ""WatchFlag"": {}
      },
      {
        ""Id"": 70088,
        ""Ticker"": ""VTR"",
        ""Source"": ""sp500"",
        ""CurrentPrice"": 66.7,
        ""RSRating"": 110.61448889464769,
        ""Sma10Day"": 68.65,
        ""Sma20Day"": 67,
        ""Sma50Day"": 62.33,
        ""Sma150Day"": 61.98,
        ""Sma200Day"": 59.07,
        ""Week52Low"": 40.71,
        ""Week52High"": 70.47,
        ""Volume"": 3335300,
        ""Volume20"": 3225865,
        ""VolumePerc"": 3.392423427514791,
        ""ADR"": 4.043812648910041,
        ""BBUpper"": 72.55903879374326,
        ""BBMiddle"": 67.00399999999999,
        ""BBLower"": 61.44896120625673,
        ""BBW"": 16.581215431148173,
        ""Date"": ""2025-03-11T00:00:00"",
        ""CreatedOn"": ""2025-03-12T12:23:03.033"",
        ""ProgressCurrentPrice"": 66.89,
        ""ProgressModifiedOn"": ""2025-03-17T12:24:00"",
        ""WatchFlag"": {}
      },
      {
        ""Id"": 70070,
        ""Ticker"": ""MO"",
        ""Source"": ""sp500"",
        ""CurrentPrice"": 58.15,
        ""RSRating"": 110.35523599994082,
        ""Sma10Day"": 56.73,
        ""Sma20Day"": 55.4,
        ""Sma50Day"": 53.41,
        ""Sma150Day"": 52.09,
        ""Sma200Day"": 50.32,
        ""Week52Low"": 38.36,
        ""Week52High"": 58.99,
        ""Volume"": 12038100,
        ""Volume20"": 9197465,
        ""VolumePerc"": 30.884977545443228,
        ""ADR"": 3.045035762137668,
        ""BBUpper"": 58.90656822009812,
        ""BBMiddle"": 55.404999999999994,
        ""BBLower"": 51.903431779901865,
        ""BBW"": 12.639899720596082,
        ""Date"": ""2025-03-11T00:00:00"",
        ""CreatedOn"": ""2025-03-12T12:23:03.033"",
        ""ProgressCurrentPrice"": 58.91,
        ""ProgressModifiedOn"": ""2025-03-17T12:24:00"",
        ""WatchFlag"": {}
      },
      {
        ""Id"": 70066,
        ""Ticker"": ""MCK"",
        ""Source"": ""sp500"",
        ""CurrentPrice"": 653.19,
        ""RSRating"": 101.52457498105296,
        ""Sma10Day"": 640.84,
        ""Sma20Day"": 621.98,
        ""Sma50Day"": 602.5,
        ""Sma150Day"": 566.51,
        ""Sma200Day"": 571.17,
        ""Week52Low"": 477.99,
        ""Week52High"": 658.84,
        ""Volume"": 838400,
        ""Volume20"": 891055,
        ""VolumePerc"": -5.909287305497416,
        ""ADR"": 3.3412279982531046,
        ""BBUpper"": 664.887412320473,
        ""BBMiddle"": 621.9755,
        ""BBLower"": 579.063587679527,
        ""BBW"": 13.798586060213944,
        ""Date"": ""2025-03-11T00:00:00"",
        ""CreatedOn"": ""2025-03-12T12:23:03.033"",
        ""ProgressCurrentPrice"": 650.43,
        ""ProgressModifiedOn"": ""2025-03-17T12:24:00"",
        ""WatchFlag"": {}
      }
    ]
        
            ";

            //OpenInNotepad(json);
            List<DailyScan> list = DBHelper.Deserialize<List<DailyScan>>(json);
            Assert.AreEqual(5, list.Count);
        }

        [TestMethod]
        public async Task Base64()
        {
            string base64 = "H4sIAAAAAAAAA72cS28byRGA/4vPu42uqn7ubZFrgCySnBLvQZKNrBBjLdjKBkGw/z3N6e56NMkRLVLxwRgOKXXxY72rqHf/ff/u6+d/fXn4+P7dD+/f/eXhl3/f3b9/9127+/z54Z/bzR9//OmPcuunL4/bi6EWF2Nstx9/ff745ePX5z/fPR+eCA4Pd3/7/Onu+fHT4/N/2j3fbjx8+dhe8OFPv26/FD3G7338HvxfIf9A9AMFV2rMgPi3cdiXuw8fPn382l7+9ybl52cWBqOPkP7gva+x/be9/OHT00EoP05/uBuP6PDgfnuALm9PfX6cEv02Lx4PV5DIUQqHxx+2d3i4+sfdfM3zL/xjH/ktzXtP2/He+cMPPd2p6/v5Cj726XAY+u1yOfjpw/zVT3zw03bw9+33edp+ml/ST//9uzN0wHtLp2ZXGU6tDicbfsKiAa/gRHTt1/5/4Cg0fOyN0SyKU8mFwmiCi4zmtNYEARPQQYlvCgYRUBRH4PDRt4UDi94UrTelusB02jNB6HTLO0hGWfgQOsI3xUM+08RDWITPPPrGeBbdKeSi4JkOaMODLu3qDqIr+W2Niiiy7kQQNnx0h7OBgAsIzZe9RAkXJcpsXrk6mIRy6V7IEiLteIBcgTe2rxhZgZprUJT48LeitOhSbp8K61IOQ7Hu9TObxMCm1n3ykBZcquV2qPAYFcTNHXZbS1mRmmdfQip9Oyla9Clpp5SqeOz2RDlWKRSNqsll/7ZhHrIYXSiBKfHRb6ROtKhTQg1JhbX2RDiGVBSk7CJdaXV4UpUGsM6plsKcUDDNwxUmPIepGkx4Caaw6FIszjOmqNxTPKlLPSGYkvprsyKNKSpMqHy4F0wBNCcPiw8PZzhBMZzoIk6LOkXSnGg4pA0UHWOqok2l6T1emQTQGUyiTQdnOjEhim/i0y/BlL4dU1zUKWjXFJRrak9kceElsQ8n0aicXEpwO1R0UqNaybEd0v14rcyKjz/Najoi4UWv4LWoVUtjpWgLwYEAawnUbs3WoqIvVypWOKNYxLRi8uzOyYuf4tMVrLQDCw2reAmrtOgWlZ5S3m2fm6gWmUw8ILCzYlYpOaxXsoqKVVKsorAqUqsAgcDi4xWsugfLWmK+iNaiWdQKa9Ys0ukUmXQqpiK2yMAOkeBKS8yKV1a8EvPKMXjhlYh5zdMFF4DBhRaXjYPlElx5US6sCheqOIg6TQcfJPsUxxUPJXS+DlcVXKBoVfFbLVXnUIigmih8vOKVDS8yvILGBZI29Jxsh9khymhmXNdgHtFxIxZVgoXs6VFweRfrddoFKgsFlWCBhMTSta53C0DRmqcLLURDK1haxhghXI5rsUhsWUNmYuQqA0PdgwIs0oaKBIythdB0Xb4FyoeB8vcg/r7mkVVukgTpKMzTFTWrY9FQiyY4Qr6cWl6UDLotbtC4bYfdSoaO5TyBiQcL0fXUsePyA7ExiQcTnh6mtD0id3rzjU2CRRGsimARO01ipskrgFMgIUg2YiZDMEVNEC8307K4NmhxM0yE7QHrXbsm1cIKSu9UVhZopCiTYyADMhmQvdfBUldNEhTIHuLu1HUHiRJOIYfAJhwxS3yYIgnJaEBWA7KY3KMHCwGJOyAXfweph4K7fh2YY1SlEhWOEKhsF5orNxQ7mikyGIhVM9yURBhqbSQFkRRECgwRY5UYG7NUBiyRQNwyAaYIJsiCzeBoMWjaobgOKkiVCBBU4GjPqIZz9KQ6PdI5bKlfULlcQ+mNQva4zHJvDpthBguzKphRFaNRhRSlkOQlYQkE0txgmQRmNd4RjG3byiEUyzLssFyc46HzxChh0LvvD1RKHOoEmUUlWzoNARTHkgxHNDoJIWuOBmP3TQNjVjqZlU5mnSeLh6S41YJPViTGCGR0Ek2eDNZFJrAc43mOdR0P6RHIqLvu+32hOFzdaI4WAZlcBa2QpccdFjobkIU0yGxiDRKThJHX3PVrrmV7OBzNkQyZS1lo/yTasFCCspJBmTVKtObdPzhBWXZQLk4yq4lA7tq59XB1jQajy711KFRLiZojjVmRzN35TZnJBG0E4yfrWZApCMiUBGQSksGHwJbd3pFwnCIxR0ymKUAmt0YytYiNNTsxe53mRpc5VRzR5b7fFojQP9Ge90StkMHVpCimYPRxE1EoBh2xu5s/ZdnoiCu6ds25T7uW7BEhAps2QK7KRQ6hmOMYzrBUxXBMumcHqrmygdxxkXVxkaG9w0lSzcRnq2UkkJVNO2IATZK6vQ6UsbvBKfTWuBWU3XGw0Db5kVhDjrtUtDmYDeS429Vx9KU2AymqeGGBGGOwGIOJ2liCwbjo43mzxnWArrvp7VpqF1dVIt5+mlNxzAVU1G7vNCiQ/QFLbQ07lh2OiTmOpsZT/2gTFzRBxWxs6sADroggHeQpEZOM0fRkgjXsnjqdIYnns8jDtoeN2cM4N5ITY7upIk3JKhMnkD4WNdyoYzaRqWqCwUjBYASDUSw7qeZDUnVh0r0tQBl+NXNUGKdIzDElE2hMNY3RKqQN2Zh3MK5ppLJr6CXEIIl6VpirSsdzqhpljDpqYzdm9kc2jawm/Skm//Fi2rm31p569GMn2WcpTHK0FDYB++utSIwyR5NFkqmx0Tbsi43ZdD7aoF98pFfRpl1zuBkZ3Iw3NbCXLFXHG3QBtFb2xEwSoGJQRpORJ2PdXqy7OtZK0O2KqlkCCMpilDLYDmIhm/2YaAPZJOTJGjedjza47ot4lUjO604ySORuGQTKwL9VKEmjzMWgBDAobVLe529T7mhidze8bivouF8GY+LQcyA1FvElcgpUq9HKLpKwLHtJOZn2YrR9C9qxcMBVK3uNPOpbEJb9fu8hU2SlDD6qXJJasNYRx2cTcmyZOCyay1urlCqZDC5wgdMeFCYZVF4OpTBKUD00lolRVjSdCzAGDrZTS4ta7kTvdU/HS+PCy+7baGEN887Ud5V6rwpV6G7xNGlP6Y11g+1dJGPdtgHkJehAUuadlXm3J5KEnRKKakeaPKjLJCSDid5gJyq7PaA9+z7ylBCEJASFUsbA0RdOzKEEU9+YJpC30dsbP+mDKW/QhG/VSxsdvU6yjvhz3x+IeUeUbrjyk0MgoWiLG2/cpE8m4KAN3mEn4KwbUaNRPzEq01bJZCpe9S2STD6pRdZgFBJNVr60JHs+b1rRpzgiuMJeElENRNszSiVbWZM4FaqqeTGlEpjZzhVsfzearYSlQx52Eko88pNe+UlfBabPhiZbd07SJ6fqUnd3DNOwNCnlqMDNMOQky1Z1cY2DIx+/79cqqZTOpFCc4iiKJtos0xnTAVqGM7sQjzykVylQv1YD3lnfqM0FSkUS8wAtiTAUYXfcYKINnDNtTCNW6xlb55h6P6VzTBXZSwZxkiyUsCwmC1rngnZms5j3+RYv4pGX9EnBTCdhUkhs3oAg6WQglykblrTHUhfd3haLmmV1wJGbvGZZNMsTOskCKY5xjyNojkupGHaizbroNys6taExOaoJKwEKSCoSuEN0ox7/YDTNjNEVSOMnezBaRogELnNXkrhJ3m4rgiHKZhuqoc2URlG0lm2n+pQNxXI5xWP/qCmqmO0lZoOXvVLKUiaGlqeYUGN7uwtCMFWi7pILQnQ8QyTWQRqNoHNfBphiCLu6h85uJtnCMJzvi+O6RDmjgVqomujUUkRAlB0SJDWDzS4v9Ew/dxl7gQkuPp6il1wUfLxWQsYdYlE7zWaCnVeINkAvWzh2bTBeTHHdsfRjvnenyKkNvkkx9XJlk3pgmwsLSMlQDDsUvZnA9s9sodgCA0MMbMXBWHEzI+mIM0OWRTGMewzP7cj1zYodhseaeAHD2r+WNXYnJDzH2LynKQbtzPAVCJNCyHoYrB72ifoSTViWmyDcqafX3czFmCXpFn65Jq5cQlRfRYgtb1Pr9YxsWUDh5W2BByfgRaV/EWXfqd3XLceT9KYgip7qkRlyYCPIks/slM/rluaifKfIpRB471C1vWNxWG7GLaDjdDD24VrnllTz4UT0YCkugXbuCwq9JD0PbV3XvAAaJhkXgGpzJ3SU6q2opbH70xvcKMVI2oXGQlwLbSdSrEubLzOTwYBMqVJyFOKtcGXviK0zZcfxIaVdXFOIS3DFPVw7fYR1a/NlXqVUTo5Vd7C9yZG03ATZbBBsbX8UZHlXw1gIhay8CtlOobtuIX6DikkKksMs9m+BC+Q7RFm3/vLsRp0FNsW4BFjYA7ZTzK7rc98ATOlXyzUh3QpYDmr9vCgPVvxLxKYcVxPbKbjWDa9vIKa+JwNzmHkLYg1S5a2PMnZSOrLo6h4xluMSYue+p7YR2/H76/7R64gFV/LNjLKS0rGKspdQX9IxluMSYrhHbKceWBdkXkestBgFtyJWDu01JqZ1rM4x+llkU5CrkZ3P/2ld4XgZGRaUjSKmVsFlupme5VY4yT6WRyebbR7UCswJaizIJdRgj9r53J/WPYNXKVo91Hg3S2C94NJaBv4FV8ZSXM3rfNpPxxPw1/Aq+g8lXGuXRS1hwGH2LcgAXrDMKcklyHaInfdkdDybfQWwg60svd0rw2VQyFTSD5D0Tv4JYiLKtczOJ/50PEB8FbTDlhjdChpAdJGnXIe/KiGuDOFFalOWC6gtX/Q+wvbzduPxof/FoJ9/f/c/B8QSK9xIAAA=";
            byte[] buffer = Convert.FromBase64String(base64);
            string json = ReadGZip(buffer);

            OpenInNotepad(json);
            Assert.IsTrue(base64.Length < json.Length);
        }

        [TestMethod]
        public async Task TextMessage()
        {
            string msg = "unit test";

            var googleVoice = "4707373225";
            var cell = "4043086715";

            string result = await SMSSvc.SendTextMessage(cell, msg);
            OpenInNotepad(result);
            Assert.AreNotEqual(0, result.Length);
        }

        [TestMethod]
        public async Task SendWhatsapp()
        {
            var googleVoice = "4707373225";
            var cell = "4043086715";

            string msg = "unit test";

            var result = await SMSSvc.SendWhatsapp(cell, msg);
            OpenInNotepad(result);
            Assert.AreNotEqual(0, result.Length);
        }

        private string ReadGZip(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
                return unzip.ReadToEnd();
        }

        public static string Decompress(byte[] data)
        {
            // Read the last 4 bytes to get the length
            byte[] lengthBuffer = new byte[4];
            Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
            int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, uncompressedSize);
                }
            }
            return Encoding.UTF8.GetString(buffer);
        }

        [TestMethod]
        public async Task GetMessage()
        {
            string json = await LambdaSvc.MessageGet(null);
        }
    }
}
