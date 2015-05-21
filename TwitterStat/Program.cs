using System;
using System.IO;
using System.Threading;
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
	class MainClass
	{
		public static void Main (string[] args)
		{
            //doing a simplistic configuration method since we just have 4 strings to deal with
            string[] apikeys = File.ReadAllLines("apikeys.txt");
			
            //set the credentials
            TwitterCredentials.SetCredentials(apikeys[0], apikeys[1], apikeys[2], apikeys[3]);

            //instantiate our tweet processsing class
            TwitStatHandler twitHandler = new TwitStatHandler();

			// access the twitter sample stream
			var sampleStream = Tweetinvi.Stream.CreateSampleStream();
			sampleStream.TweetReceived += (sender, arg) => 
            {
                //for each tweet, fire off a processing thread asyncronously
                ThreadPool.QueueUserWorkItem(new WaitCallback(twitHandler.TweetProc));

                //output the current number of processed tweets...later this will just
                //call a method of the handler to update our stats in the console
                Console.Write("\rReceived {0} tweets...    ", twitHandler.tweetsRecd);
            };
			sampleStream.StartStream();
		}
	}
}
