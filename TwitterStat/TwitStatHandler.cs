using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace TwitterStat
{
    class TwitStatHandler
    {
        //private stat storage
        private static int _tweetsRecd = 0; //number of tweets received
        private static int _tweetsProcd = 0; //number of tweets processed...debug use
       
        private ConcurrentDictionary<string, int> hashtagContainer = 
            new ConcurrentDictionary<string, int>(); //thread safe storage for our hashtag counts
       
        //getters
        public int tweetsRecd
        {
            get { return _tweetsRecd; }
        }

        public int tweetsProcd
        {
            get { return _tweetsProcd; }
        }

        public void TweetProc(Object state)
        {
            //this is tweets received, not tweets processed
            Interlocked.Increment(ref _tweetsRecd);

            //while this may not be the most ideal way to pass the tweet
            //it is currently the fastest to implement
            object[] array = state as object[];
            ITweet theTweet = (ITweet) array[0];

            addHashtags(theTweet.Hashtags);

            //bump up the number of processed tweets after we have actually processed
            Interlocked.Increment(ref _tweetsProcd);
        }
    
        //output nicely to the console
        public void outputStatus(object source, ElapsedEventArgs e)
        {
            var topHashtags = (from q in hashtagContainer
                               orderby q.Value descending 
                               select q.Key).Take(10);


            //some hashtags may require a unicode compatible font for proper display
            Console.Clear();
            Console.WriteLine("Processed {0} / {1} tweets.", _tweetsRecd, _tweetsProcd);
            Console.WriteLine(String.Format(" {0,0} | {1,10} | {2,20} ", "Top Hashtags", "Top Emoji".PadRight(8), "Top TLDs".PadRight(22)));

            foreach (string hashtag in topHashtags) {
                Console.WriteLine(String.Format(" {0,0} | {1,10} | {2,20} ", new string(hashtag.Take(10).ToArray()).PadRight(12), "x".PadRight(10), "x"));
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
                        Console.WriteLine("Error processing hashtag {0}.'", hashtag.Text);
                    }
                }
            }
        }
    }
}
