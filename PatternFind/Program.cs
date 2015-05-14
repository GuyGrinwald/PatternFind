using System;
using System.Collections.Generic;
using System.Linq;

namespace PatternFind
{
    class Program
    {
        static void Main(string[] args)
        {
            //string s1 = "Naomi is getting into the car";
            //string s4 = "George is eating at a diner";
            //string s2 = "George is getting into the car";
            //string s3 = "Naomi is eating at a restaurant";
            //string s5 = "Naomi is eating at a diner";
            //List<string> sentences = new List<string> { s1, s2, s3, s4, s5 };

            if (!args.Any())
            {
                return;
            }

            var path = args[0];
            var patternFinder = new PatternFinder();

            var patterns = patternFinder.FindPatternsInFile(path);
            PrintPatterns(patterns);

            Console.ReadKey();
        }

        /// <summary>
        /// Prints the Patters to the console
        /// </summary>
        /// <param name="patterns">The foud patters and words.</param>
        public static void PrintPatterns(IEnumerable<PatternHolder> patterns)
        {
            //var patternsFound = false;

            // a patterns is only so if there is more than one sentence like it i.e. it's patternholder has more then one sentence in it.
            if (patterns.Any(patternHolder => patternHolder.sentences != null && patternHolder.sentences.Count > 0))
            {
                foreach (var pattern in patterns)
                {
                    PrintPattern(pattern);
                }
            }
            else
            {
                Console.WriteLine("No Patterns Found.");
            }
        }

        /// <summary>
        /// Prints the content of a single pattern holder
        /// </summary>
        /// <param name="pattern"></param>
        private static void PrintPattern(PatternHolder pattern)
        {
            if (pattern.sentences != null && pattern.sentences.Count > 0)
            {
                pattern.sentences.ForEach(sentence => Console.WriteLine(sentence));
                Console.WriteLine(String.Format("The changing word was: {0}", string.Join(",", pattern.words)));
                Console.WriteLine();
            }
        }
    }


}
