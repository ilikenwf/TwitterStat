using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        //number of tweets received - incremented by Interlocked.Increment
        private static int _tweetsRecd = 0;
        
        //thread safe storage for our hashtag counts
        private ConcurrentDictionary<string, int> hashtags = new ConcurrentDictionary<string, int>();

        //TODO: thread safe storage for our (other) counts
        //private ConcurrentDictionary<string, int> hashtags = new ConcurrentDictionary<string, int>();

        public int tweetsRecd
        {
            get { return _tweetsRecd; }
        }

        public void TweetProc(Object stateInfo)
        {
            Interlocked.Increment(ref _tweetsRecd);
        }
    }
}
