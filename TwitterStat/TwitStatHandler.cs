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
        private static int _tweetsProcd = 0; //number of tweets processed
       
        private ConcurrentDictionary<string, int> hashtags = 
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

            //Console.WriteLine("Creator: {0}", theTweet.Creator);

            //bump up the number of processed tweets after we have actually processed
            Interlocked.Increment(ref _tweetsProcd);
        }

        public void outputStatus(object source, ElapsedEventArgs e)
        {
            Console.Write("\rProcessed {0} / {1} tweets...    ", _tweetsRecd, _tweetsProcd);
        }
    }
}
