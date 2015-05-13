using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PatternFind
{
    class PatternFinder
    {
        // holds the patterns for the solution
        private static List<PatternHolder> patterns = new List<PatternHolder>();
        
        // regex that checks for timestamp dd-MM-yyyy hh:mm:ss {rest of sentence}
        private const string TIME_STAMP_PREFIX_REGEX = @"^(\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}) (.+)$";

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

            using (var sr = File.OpenText(path))
            {
                string sentence = "";
                while ((sentence = sr.ReadLine()) != null)
                {
                    FindPatternsIterative(sentence);
                }
            }

            PrintPatterns();

            Console.ReadKey();
        }

		/// <summary>
        /// Finds all of the patterns in the given list of sentences and for each pattern the
		/// sentences related to it and words that change between sentences.
        /// </summary>
        /// <param name="sentences"></param>
        /// <returns>Returns a list of PatternHolders for the patterns in the given list.</returns>
        private static void FindPatternsIterative(string sentence)
        {
            // remove timestamp from sentence
            var sanitizedSentence = SanitizeSentence(sentence);

            bool foundMatch = false;

            // if there are no patterns then the sentence is always a pattern
            if (!patterns.Any())
            {
                var firstPattern = new PatternHolder(sanitizedSentence, new List<string>() { sanitizedSentence }, new List<string>());
                patterns.Add(firstPattern);
            }

            // find the right patterns for the sentence
            var matchs = patterns.Where(p => FindDiffWords(p.pattern, sanitizedSentence).wordIndex >= 0);

            // for each matching pattern add the sentence to it's group
            foreach(var match in matchs)
            {
                var temp = FindDiffWords(match.pattern, sentence);

                if (match.words.Count == 0)
                    match.words.Add(temp.diffWordInFirstSentence);

                match.words.Add(temp.diffWordInSecondSentence);
                match.sentences.Add(sentence);

                foundMatch = true;
            }

            if (!foundMatch)
            {
                // insert new pattern
                PatternHolder newPattern = new PatternHolder(sentence, new List<string>() { sentence }, new List<string>());
                patterns.Add(newPattern);
            }
        }

        /// <summary>
        /// in the example the timestamp of the sentence was disregarded.
        /// If this assumption is true then this would be the place
        /// to replace sentence with a substring of it withput the timestap and pass it
        /// to FindDiffWords.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private static string SanitizeSentence(string sentence)
        {
            var dateFinder = new Regex(TIME_STAMP_PREFIX_REGEX);
            var datematch = dateFinder.Match(sentence);
            var sentenceWithoutTimeStamp = sentence;

            // take the sentence withou the timestamp if timestamp is found
            if (datematch.Groups.Count > 1)
                sentenceWithoutTimeStamp = datematch.Groups[2].Value;

            return sentenceWithoutTimeStamp;
        }

        /// <summary>
        /// Finds the two different words between the two given strings.
        /// The function assumes the input is a grammical proper english sentence.
        /// </summary>
        /// <param name="phrase1"></param>
        /// <param name="phrase2"></param>
        /// <returns>Returns a struct containg the different words and their index in the sentence. If none is found it returns an empty struct.</returns>
        private static DiffWords FindDiffWords(string phrase1, string phrase2)
        {
            var res = new DiffWords();
            res.wordIndex = -1;
			
			if (phrase1 == null || phrase2 == null)
			{
				throw new ArgumentNullException("Phrases can not be null!");
			}

            var phrase1Words = phrase1.Split(' ');
            var phrase2Words = phrase2.Split(' ');

            var wordsInPhrase1and2 = phrase1Words.Intersect(phrase2Words);
            var phrase1WordsCount = phrase1Words.Count();
            var phrase2WordsCount = phrase2Words.Count();
            var wordsInPhrase1and2Count = wordsInPhrase1and2.Count();

            if (phrase1WordsCount != phrase2WordsCount)
            {
                return res;
            }

            if ((wordsInPhrase1and2Count == phrase1WordsCount - 1) && wordsInPhrase1and2Count > 0)
            {
                res.diffWordInFirstSentence = phrase1Words.Except(wordsInPhrase1and2).First();
                res.diffWordInSecondSentence = phrase2Words.Except(wordsInPhrase1and2).First();

                if (Array.IndexOf(phrase1Words, res.diffWordInFirstSentence) == Array.IndexOf(phrase2Words, res.diffWordInSecondSentence))
                {
                    res.wordIndex = Array.IndexOf(phrase2Words, res.diffWordInSecondSentence);
                }
            }

            return res;
        }

        /// <summary>
        /// Prints the Patters to the console
        /// </summary>
        /// <param name="patterns">The foud patters and words.</param>
        private static void PrintPatterns()
        {
            var patternsFound = false;

            foreach (var patternHolder in patterns)
            {
                if (patternHolder.sentences != null && patternHolder.sentences.Count > 0)
                {
                    patternsFound = true;

                    foreach (var sentence in patternHolder.sentences)
                    {
                        Console.WriteLine(sentence);
                    }

                    Console.WriteLine(String.Format("The changing word was: {0}", string.Join(",", patternHolder.words)));
                    Console.WriteLine();
                }
            }

            // a patterns is only so if there is more than one sentence like it.
            if (!patternsFound)
            {
                Console.WriteLine("No Patterns Found");
            }
        }
    }

    struct DiffWords
    {
        public string diffWordInFirstSentence { get; set; }
        public string diffWordInSecondSentence { get; set; }
        public int wordIndex { get; set; }
    }

    struct PatternHolder
    {
        public string pattern { get; set; }
        public List<string> sentences { get; set; }
        public List<string> words { get; set; }

        public PatternHolder(string newPattern, List<string> newSentences, List<string> newWords)
            :this()
        {
            pattern = newPattern;
            sentences = newSentences;
            words = newWords;
        }
    }
}
