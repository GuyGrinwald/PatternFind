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
                    FindMatchingPatterns(sentence);
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
        private void FindMatchingPatterns(string sentence)
        {
            // remove timestamp from sentence
            var sanitizedSentence = SanitizeSentence(sentence);

            // if there are no patterns then the new sentence is a pattern
            if (!patterns.Any())
            {
                var firstPattern = new PatternHolder(sanitizedSentence, new List<string>() { sentence }, new List<string>());
                patterns.Add(firstPattern);
                return;
            }

            // filter the right patterns for the sentence
            var matches = patterns.Where(patternHolder => SentenceMatchPattern(patternHolder, sanitizedSentence));

            if (matches.Any())
            {
                // for each matching pattern add the sentence to it's patternHolder
                foreach (var match in matches)
                {
                    var diffWords = FindFirstDiffWords(match.pattern, sanitizedSentence);

                    // handels the case for the first pattern found - adds it's word to the patternHolder
                    if (match.words.Count == 0)
                        match.words.Add(diffWords.diffWordInFirstSentence);

                    match.words.Add(diffWords.diffWordInSecondSentence);
                    match.sentences.Add(sentence);
                }
            }
            else
            {
                // insert new pattern
                PatternHolder newPattern = new PatternHolder(sanitizedSentence, new List<string>() { sentence }, new List<string>());
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
        /// Checks if the given sentence matches the given pattern
        /// </summary>
        /// <param name="patternHolder"></param>
        /// <param name="sentence"></param>
        /// <returns>Returns true if the sentence matches the pattern</returns>
        private bool SentenceMatchPattern(PatternHolder patternHolder, string sentence)
        {
            if (string.IsNullOrEmpty(sentence) || string.IsNullOrEmpty(patternHolder.pattern))
                return false;

            if (patternHolder.sentences.Any(orig => SanitizeSentence(orig).Equals(sentence)))
                return true;

            var patternWords = patternHolder.pattern.Split(' ');
            var sentenceWords = sentence.Split(' ');

            if (patternWords.Length != sentenceWords.Length)
                return false;

            var patternMatch = FindFirstDiffWords(patternHolder.pattern, sentence).wordIndex >= 0;
                        
            return patternMatch;
        }

        /// <summary>
        /// Finds first different word in each string and it's index.
        /// The function assumes the input is a grammical proper english sentence.
        /// </summary>
        /// <param name="phrase1"></param>
        /// <param name="phrase2"></param>
        /// <returns>Returns a struct containg the first different word in each sentence and it's index. If none is found it returns an empty struct.</returns>
        private DiffWords FindFirstDiffWords(string phrase1, string phrase2)
        {
            var res = new DiffWords();
            res.wordIndex = -1;

            var phrase1Words = phrase1.Split(' ');
            var phrase2Words = phrase2.Split(' ');
           
            var phrase1WordsCount = phrase1Words.Count();
            var phrase2WordsCount = phrase2Words.Count();

            var wordsInPhrase1and2 = phrase1Words.Intersect(phrase2Words);
            var wordsInPhrase1and2Count = wordsInPhrase1and2.Count();

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
