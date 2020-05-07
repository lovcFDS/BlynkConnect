using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace blynk
{
    /*
     * 对blynk server的Http接口进行封装，提供使用的接口。
     * 2019/10/20
     */
    class BlynkConnect
    {
        public string Url { get; set; }     //服务地址
        public string Auth { get; set; }    //应用的auth码
        public string ImageFilename { get; set; }   //克隆项目图片地址
        public string ZipFileName { get; set; }     //历史数据压缩包地址

        /*
         *  1个参数构造，默认为blynk官方服务器
         */
        public BlynkConnect(string auth)
        {
            Url = "http://blynk-cloud.com/";
            Auth = auth;
            ImageFilename = "qr.jpg";
        }
        /*
         * 自定义服务器和应用码，
         * 样例输入 （"http://blynk-cloud.com/","fuioasdgjfd"）
         */
        public BlynkConnect(string url, string auth)
        {
            Url = url;
            Auth = auth;
            ImageFilename = "qr.jpg";
        }
        /*
         *检测硬件是否在线的方法
         */
        public bool HardwareIsOnline()
        {
            return (Get(Url + Auth + "/isHardwareConnected") == "true");
        }

        /*
         *检测应用端是否在线的方法
         */
        public bool ApplicationIsOnline()
        {
            return (Get(Url + Auth + "/isAppConnected") == "true");
        }
        /*
         *获取引脚当前值
         * 样例输入 （“V1”） 虚拟引脚1的值
         */
        public string[] GetPinValue(string pin)
        {
            string response = Conversion(Get(Url + Auth + "/get/" + pin.Trim()));
            string[] pinDatas = response.Split(',');
            return pinDatas;
        }

        /*
         *设置引脚值
         */
        public bool SetPinValue(string pin, string value)
        {
            return (Get(Url + Auth + "/update/" + pin.Trim() + "?value=" + value.Trim()) == "");
        }
        /*
         *获取工程信息
         */
        public string GetProject()
        {
            return Get(Url + Auth + "/project");
        }
        /*
         *克隆工程项目
         */
        public string CloneProject()
        {
            WebRequest request = (WebRequest)HttpWebRequest.Create(Url+Auth+"/qr");
            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();
            FileStream fileStream = File.Create(ImageFilename);
            byte[] buffer = new byte[1024];
            int numReadByte = 0;
            while ((numReadByte = stream.Read(buffer, 0, 1024)) != 0)
            {
                fileStream.Write(buffer, 0, numReadByte);
            }
            fileStream.Close();
            stream.Close();
            return ImageFilename;
        }
        /*
         *获取某引脚历史数据
         */
        public String GetHistoryData(string pin)
        {
            ZipFileName = "";
            WebRequest request = (WebRequest)HttpWebRequest.Create(Url + Auth + "/data/"+pin.Trim());
            try
            {
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();

                //解压gz压缩文件流
                Stream gzipStream = new GZipInputStream(stream);
                StreamReader sr1 = new StreamReader(gzipStream);
                //Console.WriteLine(sr1.ReadToEnd());

                //保存.gz文件
                FileStream fileStream = File.Create(ZipFileName);
                StreamReader sr = new StreamReader(stream);

                byte[] buffer = new byte[1024];
                int numReadByte = 0;
                while ((numReadByte = stream.Read(buffer, 0, 1024)) != 0)
                {
                    fileStream.Write(buffer, 0, numReadByte);
                }
                fileStream.Close();
                stream.Close();
                return ZipFileName;
            }
            catch(WebException we)
            {
                return we.Message;
            }
            
        }
        /*******************************************************************
         * 转化接受到的字符串
         * 去除括号和引号，并将引号内的内容取出
         *******************************************************************/
        private string Conversion(string msg)
        {
            string str = "";
            bool b1 = false;
            bool b2 = false;
            int index = 0;
            //检测是否返回错误
            if (msg[index] == '[')
                b1 = true;
            while (b1)
            {
                if (msg[index] == '"')
                {
                    b2 = !b2;
                }
                else if (msg[index] == ']')
                    b1 = false;
                else if (msg[index] == ',')
                    str += ',';
                else
                {
                    if (b2)
                        str += msg[index];
                }
                index++;
            }
            return str;
        }
        /*******************************************************************
         **发起GET网络请求并返回字符
         *******************************************************************/
        private static string Get(string url)
        {
            string result = "";
            HttpWebRequest req;
            HttpWebResponse resp;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch
            {
                return "bad requests!";
            }
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }
            return result;
        }
        /*******************************************************************
        **发起POST网络请求
        * bady "{a:1,b:2}"
        *******************************************************************/
        private string PostHttp(string url ,string body)
        {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;

            byte[] btBodys = Encoding.UTF8.GetBytes(body);
            httpWebRequest.ContentLength = btBodys.Length;
            httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();

            httpWebResponse.Close();
            streamReader.Close();
            httpWebRequest.Abort();
            httpWebResponse.Close();

            return responseContent;
        }
    }
}
