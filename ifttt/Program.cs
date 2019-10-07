using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Threading;

namespace ifttt
{
    class Program
    {
        private const int PASS = 1;
        private const int FAIL = 0;
        static int Main(string[] args)
        {
            var help_string = "run <command1> for <time>[s/m/h] then <command2>" + Environment.NewLine +
                "run <command1> until <time stamp>[hh:mm:ss] then <command2>" + Environment.NewLine +
                "run <command> once";

            var url_template = "https://maker.ifttt.com/trigger/<command>/with/key/<access_key>";

            if (!args.Any())
            {
                Console.WriteLine(help_string);
                return FAIL;
            }

            if (!args.First().ToLower().Equals("run"))
            {
                Console.WriteLine(help_string);
                return FAIL;
            }

            var command = args[1];
            var key = ConfigurationManager.AppSettings["ifttt_key"];
            var RESULT = FAIL;
            
            if (args[2].ToLower().Equals("once"))
            {                
                try
                {
                    url_template = url_template.Replace("<command>", command).Replace("<access_key>", key);
                    Console.WriteLine(Call_Get(url_template, out RESULT));
                    return RESULT;
                }catch(Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.WriteLine(help_string);
                    RESULT = FAIL;
                }
            }
            else if (args[2].ToLower().Equals("for"))
            {
                try
                {
                    var time = args[3];
                    var interval = 0;
                    if (time.ToLower().Last() == 's')
                    {
                        interval = Convert.ToInt32(time.Remove(time.Length - 1));
                    }
                    else if (time.ToLower().Last() == 'm')
                    {
                        interval = Convert.ToInt32(time.Remove(time.Length - 1)) * 60;
                    }
                    else if (time.ToLower().Last() == 'h')
                    {
                        interval = Convert.ToInt32(time.Remove(time.Length - 1)) * 3600;
                    }

                    var command2 = args[5];
                    var url1 = url_template.Replace("<command>", command).Replace("<access_key>", key);
                    var url2 = url_template.Replace("<command>", command2).Replace("<access_key>", key);

                    Console.WriteLine(Call_Get(url1, out RESULT));
                    var ticker = 0;
                    while (ticker < interval)
                    {
                        ticker++;
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine(Call_Get(url2, out RESULT));
                }catch(Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.WriteLine(help_string);
                    RESULT = FAIL;
                }
            }
            else if (args[2].ToLower().Equals("until"))
            {
                try
                {
                    var time_stamp = Convert.ToDateTime(args[3]);

                    if (time_stamp < DateTime.Now)
                    {
                        Console.WriteLine("Command belogs to the past");
                        return FAIL;
                    }

                    var command2 = args[5];
                    var url1 = url_template.Replace("<command>", command).Replace("<access_key>", key);
                    var url2 = url_template.Replace("<command>", command2).Replace("<access_key>", key);

                    Console.WriteLine(Call_Get(url1, out RESULT));

                    while (DateTime.Now < time_stamp)
                    {
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine(Call_Get(url2, out RESULT));
                }catch(Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.WriteLine(help_string);
                    RESULT = FAIL;
                }
            }
            else
            {
                Console.WriteLine(help_string);
                RESULT = FAIL;
            }

            return RESULT;
        }

        private static string Call_Get(string url, out int result)
        {
            result = FAIL;
            var response = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                var res = client.GetAsync(url).Result;
                response = $"Response received with http status code {res.StatusCode}, ifttt says  {res.Content.ReadAsStringAsync().Result}";
                result = res.StatusCode == HttpStatusCode.OK ? PASS : FAIL;
            }
            return response;
        }
    }    
}
