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
        /// Iterates thtough all of the lines in the stream and finds and returns the patterns in the file
        /// </summary>
        /// <param name="path"></param>
        public IEnumerable<PatternHolder> FindPatternsInFile(Stream fileStream)
        {
            using (var sr = new FileIterator(fileStream))
            {
                foreach (var sentence in sr.ReadLines())
                {
                    var matchingPatterns = FindMatchingPatterns(sentence);
                    AddSenteceToPatterns(matchingPatterns, sentence);
                }

                return patterns;
            }
        }

        /// <summary>
        /// Finds all of the patterns that match the given sentence
        /// </summary>
        /// <param name="sentences"></param>
        /// <returns>Returns an enumerable of PatternHolders that match the given sentence.</returns>
        private IEnumerable<PatternHolder> FindMatchingPatterns(string sentence)
        {
            List<PatternHolder> matches = new List<PatternHolder>();

            if (string.IsNullOrEmpty(sentence))
            {
                return matches;
            }

            // remove timestamp from sentence
            var sanitizedSentence = SanitizeSentence(sentence);

            // filter the right patterns for the sentence
            matches = patterns.FindAll(patternHolder => IsSentenceMatchPattern(patternHolder, sanitizedSentence));

            // create new pattern if no matching pattern exists
            if (!matches.Any())
            {
                var newPattern = new PatternHolder(sanitizedSentence, new List<string>(), new List<string>());
                patterns.Add(newPattern);
                matches.Add(newPattern);
            }

            return matches;
        }

        /// <summary>
        /// Adds the given sentence to the given pattern
        /// </summary>
        /// <param name="matchingPatterns"></param>
        /// <param name="sentence"></param>
        private void AddSenteceToPatterns(IEnumerable<PatternHolder> matchingPatterns, string sentence)
        {
            if (!matchingPatterns.Any() || string.IsNullOrEmpty(sentence))
            {
                return;
            }

            var sanitizedSentence = SanitizeSentence(sentence);

            // for each matching pattern add the sentence to it's patternHolder
            foreach (var match in matchingPatterns)
            {
                int diffWordIndex = FindDiffWord(match.pattern, sanitizedSentence);
                var patternWords = match.pattern.Split(' ');
                var sentenceWords = sanitizedSentence.Split(' ');

                // ignore duplicate sentences
                var sanitizedSentences = match.sentences.Select(original => SanitizeSentence(original));
                if (sanitizedSentences.Contains(sanitizedSentence))
                    return;

                // if the pattern is new just add the sentence to it - we cant know the diff word yet
                if (match.sentences.Count == 0)
                {
                    match.sentences.Add(sentence);
                    return;
                }

                // handels the case for the first pattern found - adds it's word to the patternHolder
                if (match.words.Count == 0)
                {
                    match.words.Add(patternWords[diffWordIndex]);
                }

                match.words.Add(sentenceWords[diffWordIndex]);

                match.sentences.Add(sentence);
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
            return sanitizeDateRegex.Replace(sentence, ""); ;
        }

        /// <summary>
        /// Checks if the given sentence matches the given pattern
        /// </summary>
        /// <param name="patternHolder"></param>
        /// <param name="sentence"></param>
        /// <returns>Returns true if the sentence matches the pattern</returns>
        private bool IsSentenceMatchPattern(PatternHolder patternHolder, string sentence)
        {
            if (string.IsNullOrEmpty(patternHolder.pattern) || string.IsNullOrEmpty(sentence))
                return false;

            // if the sentence already exists in the pattern then the new copy of it also matches the pattern
            var sanitizedSentences = patternHolder.sentences.Select(original => SanitizeSentence(original));
            if (sanitizedSentences.Contains(sentence))
                return true;

            // the sentence matches the pattern iff they have only one word that differs between them in the same location;
            var diffWordsIndex = FindDiffWord(patternHolder.pattern, sentence);
            var patternMatch = diffWordsIndex >= 0;

            return patternMatch;
        }

        /// <summary>
        /// Finds the only different word in each string and it's index.
        /// The function assumes the input is a grammical proper english sentence.
        /// </summary>
        /// <param name="phrase1"></param>
        /// <param name="phrase2"></param>
        /// <returns>Returns a struct containg the only different word in each sentence and it's index. If none is found or there are more then one it returns an empty struct.</returns>
        private int FindDiffWord(string phrase1, string phrase2)
        {
            int diffWordLocation = -1;

            if (string.IsNullOrEmpty(phrase1) || string.IsNullOrEmpty(phrase2))
            {
                return diffWordLocation;
            }

            var phrase1Words = phrase1.Split(' ');
            var phrase2Words = phrase2.Split(' ');

            if (phrase1Words.Length != phrase2Words.Length)
            {
                return diffWordLocation;
            }

            // create tuples of words form each collection and their index and filters those whose words are not the same
            var wordPairs = phrase1Words.Select((word, index) => new { wordInPhrase1 = word, wordInPhrase2 = phrase2Words[index], wordIndex = index });
            var diffWords = wordPairs.Where(wordPair => !wordPair.wordInPhrase1.Equals(wordPair.wordInPhrase2));

            if (diffWords.Count() == 1)
            {
                diffWordLocation = diffWords.First().wordIndex;
            }

            return diffWordLocation;
        }
    }

    struct PatternHolder
    {
        public string pattern { get; set; }
        public IList<string> sentences { get; set; }
        public IList<string> words { get; set; }

        public PatternHolder(string newPattern, IList<string> newSentences, IList<string> words)
            : this()
        {
            this.pattern = newPattern;
            this.sentences = newSentences;
            this.words = words;
        }
    }
}
