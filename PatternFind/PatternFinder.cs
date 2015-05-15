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
        private List<PatternHolder> patterns = new List<PatternHolder>();

        // regex that checks for timestamp dd-MM-yyyy hh:mm:ss {rest of sentence}
        private const string TIME_STAMP_PREFIX_REGEX = @"^\d{2}-\d{2}-\d{4}\s\d{2}:\d{2}:\d{2}\s";

        // TODO - might consider moving this to an assembly
        private Regex sanitizeDateRegex = new Regex(TIME_STAMP_PREFIX_REGEX,
                   RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Iterates thtough all of the lines in the file and finds and stores 
        /// </summary>
        /// <param name="path"></param>
        public IEnumerable<PatternHolder> FindPatternsInFile(string path)
        {
            using (var sr = File.OpenText(path))
            {
                string sentence = "";

                while ((sentence = sr.ReadLine()) != null)
                {
                    FindPatternsIterative(sentence);
                }

                return patterns;
            }
        }

        /// <summary>
        /// Finds all of the patterns in the given list of sentences and for each pattern the
        /// sentences related to it and words that change between sentences.
        /// </summary>
        /// <param name="sentences"></param>
        /// <returns>Returns a list of PatternHolders for the patterns in the given list.</returns>
        private void FindPatternsIterative(string sentence)
        {
            // remove timestamp from sentence
            var sanitizedSentence = SanitizeSentence(sentence);

            // if there are no patterns then the new sentence is always a pattern
            if (!patterns.Any())
            {
                var firstPattern = new PatternHolder(sanitizedSentence, new List<string>() { sanitizedSentence }, new List<string>());
                patterns.Add(firstPattern);
                return;
            }

            // find the right patterns for the sentence
            var matches = patterns.Where(ptternHolder => FindDiffWords(ptternHolder.pattern, sanitizedSentence).wordIndex >= 0);

            if (matches.Any())
            {
                // for each matching pattern add the sentence to it's group
                foreach (var match in matches)
                {
                    var diffWords = FindDiffWords(match.pattern, sanitizedSentence);

                    if (match.words.Count == 0)
                        match.words.Add(diffWords.diffWordInFirstSentence);

                    match.words.Add(diffWords.diffWordInSecondSentence);
                    match.sentences.Add(sanitizedSentence);
                }
            }
            else
            {
                // insert new pattern
                PatternHolder newPattern = new PatternHolder(sanitizedSentence, new List<string>() { sanitizedSentence }, new List<string>());
                patterns.Add(newPattern);
            }
        }

        /// <summary>
        /// In the example the timestamp of the sentence was disregarded.
        /// If this assumption is true then this would be the place
        /// to replace sentence with a substring of it withput the timestap and pass it
        /// to FindDiffWords.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private string SanitizeSentence(string sentence)
        {
            // take the sentence without the timestamp if timestamp is found
            return sanitizeDateRegex.Replace(sentence, ""); ;
        }

        /// <summary>
        /// Finds the two different words between the two given strings.
        /// The function assumes the input is a grammical proper english sentence.
        /// </summary>
        /// <param name="phrase1"></param>
        /// <param name="phrase2"></param>
        /// <returns>Returns a struct containg the different words and their index in the sentence. If none is found it returns an empty struct.</returns>
        private DiffWords FindDiffWords(string phrase1, string phrase2)
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
                res.diffWordInFirstSentence = phrase1Words.Except(wordsInPhrase1and2).Single();
                res.diffWordInSecondSentence = phrase2Words.Except(wordsInPhrase1and2).Single();

                if (Array.IndexOf(phrase1Words, res.diffWordInFirstSentence) == Array.IndexOf(phrase2Words, res.diffWordInSecondSentence))
                {
                    res.wordIndex = Array.IndexOf(phrase2Words, res.diffWordInSecondSentence);
                }
            }

            return res;
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

        public PatternHolder(string newPattern, ICollection<string> newSentences, ICollection<string> newWords)
            : this()
        {
            this.pattern = newPattern;
            this.sentences = newSentences.ToList();
            this.words = newWords.ToList();
        }
    }
}
