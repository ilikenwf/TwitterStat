using System;
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
using System.IO;
using System.Collections.Concurrent;

namespace TwitterStat
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            int tweetsRecd = 0;
            ConcurrentDictionary<string,int> hashtags = new ConcurrentDictionary<string,int>;

            string[] apikeys = File.ReadAllLines("twitapi.txt");
			TwitterCredentials.SetCredentials(apikeys[0], apikeys[1], apikeys[2], apikeys[3]);

			Console.WriteLine ("testing...");

			// Access the sample stream
			var sampleStream = Stream.CreateSampleStream();
			sampleStream.TweetReceived += (sender, arg) => 
            { 
                Console.WriteLine(arg.Tweet.e); 
                tweetsRecd++;
                Console.WriteLine("Received {0} tweets.", tweetsRecd);
                
                TwitHashtag ht;
                ht.parseHashtag(arg.Tweet.Text);
            };
			sampleStream.StartStream();
		}
	}
}
