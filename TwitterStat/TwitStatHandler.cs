using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Tweetinvi;
using Tweetinvi.Core;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Controllers;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.Models.Parameters;
using Tweetinvi.Core.Interfaces.Streaminvi;
using Tweetinvi.Core.Interfaces.WebLogic;
using Tweetinvi.Json;
using Tweetinvi.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace TwitterStat
{
    class TwitStatHandler
    {
        //private stat storage
        private static int _tweetsRecd = 0; //number of tweets received
        private static int _tweetsProcd = 0; //number of tweets processed...debug use
        private static int _numURL = 0; //workaround because dictionary summing isn't working
        private static JArray emoji = JArray.Parse(File.ReadAllText("emoji.json"));

        private ConcurrentDictionary<string, int> hashtagContainer = 
            new ConcurrentDictionary<string, int>(); //thread safe storage for our hashtag counts

        private ConcurrentDictionary<string, int> emojiContainer =
            new ConcurrentDictionary<string, int>(); //thread safe storage for our emoji counts

        private ConcurrentDictionary<string, int> tldContainer =
            new ConcurrentDictionary<string, int>(); //thread safe storage for our tld counts

        public void TweetProc(Object state)
        {
            //this is tweets received, not tweets processed
            Interlocked.Increment(ref _tweetsRecd);

            //while this may not be the most ideal way to pass the tweet
            //it is currently the fastest to implement
            object[] array = state as object[];
            ITweet theTweet = (ITweet) array[0];

            //process the tweet hashtags
            addHashtags(theTweet.Hashtags);

            //process the tweet emoji
            addEmoji(theTweet.Text);

            //process the tweet URL's
            addTLD(theTweet.Urls);

            //bump up the number of processed tweets after we have actually processed
            Interlocked.Increment(ref _tweetsProcd);
        }
    
        //output nicely to the console
        public void outputStatus(object source, ElapsedEventArgs e)
        {
            var topHashtags = (from q in hashtagContainer
                               orderby q.Value descending
                               select q.Key).Take(10);

            var topEmoji = (from q in emojiContainer
                               orderby q.Value descending
                               select q.Key).Take(10);

            var topDomains = (from q in tldContainer
                            orderby q.Value descending
                            select q.Key).Take(10);

            //this is nasty but for the sake of nice output...
            string[] ht = topHashtags.Cast<string>().ToArray();
            string[] te = topEmoji.Cast<string>().ToArray();
            string[] td = topDomains.Cast<string>().ToArray();

            //% with urls
            double percDone = 0;

            if (_numURL > 0 && _tweetsRecd > 0)
            {
                percDone = Math.Round(((double)_numURL / (double) _tweetsRecd) * 100,2);
            }

            //some hashtags may require a unicode compatible font for proper display
            Console.Clear();
            Console.WriteLine("Processed {0} tweets.", _tweetsRecd);

            Console.WriteLine();
            Console.WriteLine("% w/URL: {0}", percDone);

            Console.WriteLine(String.Format(" {0,0} | {1,20} | {2,40} ", 
                "Hashtags  ", "Emoji     ", "TLD's".PadRight(100)));

            for (int i = 1; i <= 10; i++)
            {
                string hash = (i < ht.Length) ? ht[i] : "";
                string emo = (i < te.Length) ? te[i] : "";
                string tld = (i < td.Length) ? td[i] : "";

                Console.WriteLine(String.Format(" {0,0} | {1,20} | {2,40} ", 
                    new string(hash.Take(10).ToArray()).PadRight(10),
                    new string(emo.Take(20).ToArray()).PadRight(10), 
                    tld.PadRight(40)));
            }
        }

        private void addHashtags(List<Tweetinvi.Core.Interfaces.Models.Entities.IHashtagEntity> hashtags)
        {
            if (hashtags != null && hashtags.Count > 0) {
                foreach (Tweetinvi.Core.Interfaces.Models.Entities.IHashtagEntity hashtag in hashtags)
                {
                    try
                    {
                        //create/initalize or increment the counter for the hashtag slot
                        hashtagContainer.AddOrUpdate(hashtag.Text, 1, (key, oldValue) => oldValue + 1);
                    }
                    catch (Exception)
                    {
                        //haven't hit this one yet...
                        Console.WriteLine("Error processing hashtag {0}.'", hashtag.Text);
                    }
                }
            }
        }

        private void addEmoji(string tweetText)
        {
            //get all of the surrogate values for this tweet
            int[] surrogateDecimals = SurrogatesToCodePoints(tweetText);

            foreach (int decValue in surrogateDecimals)
            {
                try { 
                    var emojiUsed = emoji.Children().First(entry => entry["unified"].Value<string>() == decValue.ToString("X"));
                    //create/initalize or increment the counter for the hashtag slot
                    emojiContainer.AddOrUpdate((string) emojiUsed["name"], 1, (key, oldValue) => oldValue + 1);
                }
                catch
                {
                    //TODO: figure out what causes occasional breakage here
                }
            }
        }


        private void addTLD(List<Tweetinvi.Core.Interfaces.Models.Entities.IUrlEntity> tldList)
        {
            foreach (Tweetinvi.Core.Interfaces.Models.Entities.IUrlEntity url in tldList)
            {
                try
                {
                    Interlocked.Increment(ref _numURL);
                    Uri uri = new Uri(url.URL);
                    string domain = uri.GetLeftPart(UriPartial.Authority);
                    tldContainer.AddOrUpdate(domain, 1, (key, oldValue) => oldValue + 1);
                }
                catch
                {
                    Console.WriteLine("Error inserting TLD.");
                }
            }
        }

        //this one was tough...
        //stackoverflow saves the day again
        //http://stackoverflow.com/questions/687359/how-would-you-get-an-array-of-unicode-code-points-from-a-net-string
        private static int[] SurrogatesToCodePoints(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            var codePoints = new List<int>(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsHighSurrogate(str[i])) {
                    codePoints.Add(Char.ConvertToUtf32(str, i));
                    i += 1;
                }
            }

            return codePoints.ToArray();
        }
    }
}
