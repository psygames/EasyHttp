using System;

namespace EasyHttp
{
    class Program
    {
        class Item
        {
            public string key;
            public int value;
            public string info;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // DownloadTest();

            var db = new DB<Item>();
            db.Set("1", new Item() { key = "1", info = "safsfa", value = 222 });
            db.Set("2", new Item() { key = "2", info = "geahath", value = 3333 });
            db.Save("item.txt");
            db.Load("item.txt");

            Console.WriteLine(db.Get(a => a.value == 222).info);

            Console.ReadLine();
        }

        private static string url = "http://f.video.weibocdn.com/XVNuWXbJlx07MrQOGEAU0104120460Zf0E020.mp4?label=mp4_1080p&template=1920x1080.25.0&trans_finger=d88af6227b88881484d2b59dfbafa136&media_id=4633899004002402&tp=8x8A3El:YTkl0eM8&us=0&ori=1&bf=3&ot=h&ps=3lckmu&uid=5Bm3J8&ab=3915-g1,5178-g1,966-g1&Expires=1631948610&ssig=DubObjpa6f&KID=unistore,video";
        static async void DownloadTest()
        {
            var result = await Http.Download(url, "a.mp4", new Progress<long[]>((report) =>
            {
                Console.WriteLine($"Download [{report[0]}/{report[1]}]");
            }));
        }
    }
}
