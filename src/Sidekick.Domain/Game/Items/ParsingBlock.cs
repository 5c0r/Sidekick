using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sidekick.Domain.Game.Items
{
    /// <summary>
    /// Represents a single item section seperated by dashes when copying an item in-game.
    /// </summary>
    public class ParsingBlock
    {
        private static readonly Regex NEWLINEPATTERN = new Regex("[\\r\\n]+");

        /// <summary>
        /// Represents a single item section seperated by dashes when copying an item in-game.
        /// </summary>
        /// <param name="text">The text of the whole block</param>
        public ParsingBlock(string text)
        {
            Text = text;

            Lines = NEWLINEPATTERN
                .Split(Text)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => new ParsingLine(x))
                .ToList();
        }

        /// <summary>
        /// Contains all the lines inside this block
        /// </summary>
        public List<ParsingLine> Lines { get; set; }

        /// <summary>
        /// Indicates if this block has been successfully parsed by the parser
        /// </summary>
        public bool Parsed
        {
            get => !Lines.Any(x => !x.Parsed);
            set
            {
                foreach (var line in Lines)
                {
                    line.Parsed = value;
                }
            }
        }

        /// <summary>
        /// The text of the whole block
        /// </summary>
        public string Text { get; }
    }
}
