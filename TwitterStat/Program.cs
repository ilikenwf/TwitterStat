using System;
using Tweetinvi;
using Tweetinvi.Streams;

namespace TwitterStat
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            int tweetsRecd = 0;
			TwitterCredentials.SetCredentials("71941386-2e8aKSxp6NMYe0HCqGKVVdJAeeORrqByuGfxjsVJM", 
				"0mZf8ly3bIA9upslt2ONwgqfjRVjLZpD9pVfj4yIDHIbx", 
				"RU19mW1g4Ja3zMkU5MJpcyjPI", 
				"JP2Hh1RWa6OAGaCCox8PTjLENio6epTOXEo8lAMPxgAUP70MbZ");

			Console.WriteLine ("testing...");

			// Access the sample stream
			var sampleStream = Stream.CreateSampleStream();
			sampleStream.TweetReceived += (sender, arg) => { 
                //Console.WriteLine(arg.Tweet.Text); 
                tweetsRecd++;
                Console.WriteLine("Received {0} tweets.", tweetsRecd);
            };
			sampleStream.StartStream();
		}
	}
}
