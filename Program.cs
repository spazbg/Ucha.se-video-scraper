using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace uchase
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Console.OutputEncoding = Encoding.UTF8;

            var chromeOptions = new ChromeOptions(); //comment this for GUI browser
            chromeOptions.AddArguments("headless"); //comment this for GUI browser
            IWebDriver driver = new ChromeDriver(chromeOptions);


            driver.Navigate().GoToUrl("https://ucha.se/");

            Thread.Sleep(2000); //2 seconds

            try
            {
                var buttonEnter =
                    driver.FindElement(
                        By.XPath("//a[@class='nav-item btn-sm btn-sm-md btn-lindsey text-s-bold']//span"));
                buttonEnter.Click();

                var usernameBox = driver.FindElement(By.Name("email"));
                var passwordBox = driver.FindElement(By.Name("password"));
                
                Console.BackgroundColor = ConsoleColor.Green;
                Thread.Sleep(1000);
                Console.WriteLine("[+] Email: ");
                var email = Console.ReadLine();

                Console.WriteLine("[+] Password: ");
                var password = Console.ReadLine();

                Console.WriteLine("[+] URL of course to download: ");
                var urlCourse = Console.ReadLine();
                Console.ResetColor();

                usernameBox.SendKeys(email); //mail
                Thread.Sleep(1000);
                passwordBox.SendKeys(password); //password
                Thread.Sleep(1000);

                var loginButton =
                    driver.FindElement(By.Id("send_data"));
                Thread.Sleep(1000);

                loginButton.Click();
                Thread.Sleep(3000);


                driver.Navigate().GoToUrl(urlCourse);


                var pageSource = driver.PageSource;

                var matchedUrls = Urls(pageSource);

                for (var i = 0; i < matchedUrls.Count; i++)
                {
                    var url = matchedUrls[i];
                    GenerateDownloadFile(driver, url, i);
                }

                CutTheEndOfTheTxtFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] The file is ready!");
            Console.ResetColor();
            driver.Quit();
        }

        private static void CutTheEndOfTheTxtFile()
        {
            var text = File.ReadAllText("download.txt");
            var output = Regex.Replace(text, " && $", System.Environment.NewLine + "=====================================================" + System.Environment.NewLine);
            File.WriteAllText("download.txt", output);
        }

        private static void GenerateDownloadFile(IWebDriver driver, string url, int i)
        {
            //go to url
            //get source
            //findm3u8file
            //read m3u8, get sources

            driver.Navigate().GoToUrl(url);
            Thread.Sleep(1000);
            var pageSource = driver.PageSource;

            var m3u8Pattern =
                @"(https:\\\/\\\/proxy[1|2].ucha.se:443\\\/.+?playlist.m3u8).+?;s=([a-z0-9]+)(&quot;,&quot;mp4_level_0&quot;:&quot;)";
            var rg = new Regex(m3u8Pattern);

            var m3U8Files = rg.Matches(pageSource);

            if (rg.IsMatch(pageSource))
            {
                var m3u8FileUrl = m3U8Files[0].Groups[1].Value.Replace(@"\/", @"/"); //replace \/ with /

                var sessionKey = m3U8Files[0].Groups[2].Value;

                Thread.Sleep(1000);

                try
                {
                    var wc = new WebClient();
                    var m3u8Source = wc.DownloadString(m3u8FileUrl);

                    //RESOLUTION = 848x480\n(chunklist_.+?.m3u8)
                    var chunklistPattern = @"(chunklist_.+?.m3u8)";
                    var rgc = new Regex(chunklistPattern);

                    var chunklistColection = rgc.Matches(m3u8Source);

                    var chunkFile = chunklistColection[3].Groups[1].Value;
                    var finalUrl = m3u8FileUrl.Replace("playlist.m3u8", "") + chunkFile + "?s=" + sessionKey;


                    var cookieStr = driver.Manage().Cookies.GetCookieNamed("PHPSESSID").ToString();

                    //(PHPSESSID=.+?);
                    var cookiePattern = @"(PHPSESSID=.+?);";
                    var rgcookie = new Regex(cookiePattern);

                    var cookieColection = rgcookie.Matches(cookieStr);

                    var phpsessidCookie = cookieColection[0].Groups[1].Value;

                    //smil: videos\/ (.+)\/ stream.smil\/?
                    var fileNamePattern = @"smil:videos\/(.+)\/stream.smil\/?";
                    var rgFileName = new Regex(fileNamePattern);

                    var fileNameColection = rgFileName.Matches(m3u8FileUrl);

                    var fileName = fileNameColection[0].Groups[1].Value;
                    var outputFileName = fileName;

                    // ffmpeg.exe - headers "Cookie: PHPSESSID=c063e6b6cce584984d4151b9b9f36a6c; sessionid=5ff4a78cb7137225f6d37d53c252bfd1; user_visit_fingerprint=1; cookie_tooltip=2" - i https://proxy1.neterra.tv/uchase/_definist_/smil:videos/06112019-Azbukata-The-Alphabet/stream.smil/chunklist_b281108_slbul.m3u8?s=a1222ba74ef6bec623babdc72157f2d2 -c copy -bsf:a aac_adtstoasc 01-a1-azbukata-the-alphabet.mp4

                    var ffmpegString =
                        $@"ffmpeg.exe -headers ""Cookie: {phpsessidCookie};"" -i {finalUrl} -c copy -bsf:a aac_adtstoasc {i}-{outputFileName}.mp4";

                    //create download.txt file whit input for ffmpeg.exe
                    var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
                    var directoryFullName = new FileInfo(location.AbsolutePath).Directory.FullName;

                    var pathFile = $@"{directoryFullName}\download.txt";
                    if (File.Exists(pathFile))
                        using (var file = new StreamWriter(pathFile, true))
                        {
                            file.Write(ffmpegString + " && ");
                        }
                    else
                        File.WriteAllText($@"{pathFile}", ffmpegString + " && ");
                }
                catch (WebException we)
                {
                    // error processing
                    we.Message.ToString();
                }
            }
        }

        private static List<string> Urls(string pageSource)
        {
            var urlPattern = @"<a href=""(https:\/\/ucha\.se\/watch\/.+?)""";
            var rg = new Regex(urlPattern);

            var matchedUrls = rg.Matches(pageSource);

            var listMatchedUrls = matchedUrls.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
            return listMatchedUrls;
        }
    }
}