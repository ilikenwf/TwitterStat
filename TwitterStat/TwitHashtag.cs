using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace TwitterStat
{
    class TwitHashtag
    {
        // parse the hashtags from our referenced tweet...no need to copy it
        // we have to reference the ConcurrentDictionary from the main thread, so it can be updated but not overwritten
        public void parseHashtags(ref string tweet, ref ConcurrentDictionary<string,int> hashtagContainer)
        {
            // Get first match.
            Match m = Regex.Match(tweet, @"\B#\w\w+");

            while (m.Success)
            {
                try
                {
                    //create/initalize or increment the counter for the hashtag slot
                    hashtagContainer.AddOrUpdate(m.Value, 1, (key, oldValue) => oldValue + 1);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error processing hashtag {0}.'", m.Value);
                }
            }
        }

        // while normally we'd have a getter here for the top n values, it makes more logical sense to put that on the main thread
        // since the main thread owns the ConcurrentDictionary containing our values and will be outputting them periodically
    }
}
