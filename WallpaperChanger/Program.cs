using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Windows.Forms;
using System.Threading;

namespace WallpaperChanger
{
    class Program
    {
        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uiAction, int uiParam,
            string pvParam, uint fWinIni);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        static IntPtr handle = GetConsoleWindow();

        static string savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\wp.jpg";
        
        static HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();

        static List<string> hrefList = new List<string>();

        static List<string> imageWallHref = new List<string>();

        //Delay between changing wallpapers (unstable below 1 second)
        static int ChangeDelay = 25;

        [STAThread]
        static void Main(string[] args)
        {
            ShowWindow(handle, 0);
            
            for (;;)
            {
                GenerateImageWallList();
                try
                {
                    new WebClient().DownloadFile(@"https://www.hdwallpapers.in/" + hrefList[new Random().Next(0, hrefList.Count - 1)], savePath);
                    Thread.Sleep(100);
                    SetWallpaper();
                    Thread.Sleep(ChangeDelay * 1000);
                    File.Delete(savePath);
                }
                catch (Exception) { }
                imageWallHref.Clear();
                hrefList.Clear();
            }
        }

        static void GenerateImageWallList()
        {
            htmlDocument = new HtmlWeb().Load(@"https://www.hdwallpapers.in/latest_wallpapers/page/" + new Random().Next(1, 1238) + @"/");

            var selectedClassNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class]");
            for (int i = 0; i < selectedClassNodes.Count; i++)
            {
                var link = selectedClassNodes[i];
                if (link.GetAttributeValue("class", null).Contains("thumb"))
                {
                    imageWallHref.Add(link.ChildNodes[0].GetAttributeValue("href", "no link found"));
                }
            }
            CleanImageWallHrefList();
            htmlDocument = new HtmlWeb().Load(@"https://www.hdwallpapers.in/" + imageWallHref[new Random().Next(0, imageWallHref.Count)]);
            foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (link.GetAttributeValue("href", "").Contains("jpg"))
                    hrefList.Add(link.GetAttributeValue("href", ""));
            }
        }

        static void CleanImageWallHrefList()
        {
            foreach (var item in imageWallHref.ToList())
            {
                if(item == "no link found")
                {
                    imageWallHref.Remove(item);
                }
            }
        }

        static void SetWallpaper()
        {
            SystemParametersInfo(0x0014, 0, savePath, 0x0001);
        }
    }
}
