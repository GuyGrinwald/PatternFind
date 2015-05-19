using System;
using System.Collections.Generic;
using System.Linq;

namespace PatternFind
{
    class Program
    {
        static void Main(string[] args)
        {
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
            foreach (var sentence in pattern.sentences)
            {
                Console.WriteLine(sentence);
            }

            Console.WriteLine(String.Format("The changing word was: {0}", string.Join(", ", pattern.words)));
            Console.WriteLine();
        }
    }
}
