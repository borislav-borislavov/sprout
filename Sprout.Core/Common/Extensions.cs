using Sprout.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sprout.Core.Services.Queries.QueryService;

namespace Sprout.Core.Common
{
    public static class Extensions
    {

        public static string PurifyTableName(this string tableName)
        {
            var friendlyName = tableName;

            if (friendlyName == null)
                return friendlyName;

            if (friendlyName.IndexOf('.') > -1)
                friendlyName = friendlyName.Substring(friendlyName.IndexOf('.') + 1);

            friendlyName = friendlyName.Trim('[', ']');

            return friendlyName;
        }

        public static bool ContainsIgnoreCase(this string haystack, string needle)
        {
            if (string.IsNullOrEmpty(haystack))
                return false;

            if (string.IsNullOrEmpty(needle))
                return false;

            return haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static bool EqualsCaseInsensitive(this string str1, string str2)
        {
            return $"{str1}".Equals($"{str2}", StringComparison.InvariantCultureIgnoreCase);
        }

        public static IEnumerable<string> Split(this string text, string delimiter, bool preserveEmptyLines = false)
        {
            if (preserveEmptyLines)
                return text.Split(new string[] { delimiter }, StringSplitOptions.None);

            return text.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToLowerFirstChar(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// The text starts from the the first occurence of the starting phrase
        /// Slices the text to start from the starting phrase
        /// </summary>
        public static string StartFrom(this string text, string startPhrase, int correction = 0)
        {
            var idx = text.IndexOf(startPhrase, StringComparison.OrdinalIgnoreCase);

            if (idx == -1) return null;

            return text.Substring(idx + correction);
        }

        public static string StartFrom(this string text, string startPhrase, bool startAfterStartPhrase)
        {
            int correction = 0;

            if (startAfterStartPhrase)
            {
                correction = $"{startPhrase}".Length;
            }

            return text.StartFrom(startPhrase, correction);
        }

        /// <summary>
        /// The text starts from the the last occurence of the starting phrase
        /// </summary>
        public static string StartFromLast(this string text, string startPhrase, int correction = 0)
        {
            var idx = text.LastIndexOf(startPhrase, StringComparison.OrdinalIgnoreCase);

            if (idx == -1) return null;

            return text.Substring(idx + correction);
        }

        public static string StartFromLast(this string text, string startPhrase, bool startAfterStartPhrase)
        {
            int correction = 0;

            if (startAfterStartPhrase)
            {
                correction = $"{startPhrase}".Length;
            }

            return StartFromLast(text, startPhrase, correction);
        }

        /// <summary>
        /// Take text until the first encounter of the stop phrase
        /// </summary>
        public static string StopAt(this string text, string stopPhrase, int correction = 0)
        {
            var stopIdx = text.IndexOf(stopPhrase, StringComparison.OrdinalIgnoreCase);

            if (stopIdx == -1) return text;

            return text.Substring(0, stopIdx + correction);
        }

        /// <summary>
        /// Take text until the last encounter of the stop phrase
        /// </summary>
        public static string StopAtLast(this string text, string stopPhrase, int correction = 0)
        {
            var stopIdx = text.LastIndexOf(stopPhrase, StringComparison.OrdinalIgnoreCase);

            if (stopIdx == -1) return text;

            return text.Substring(0, stopIdx + correction);
        }

        /// <summary>
        /// Return the index at the end of the needle
        /// </summary>
        public static int IndexOfEnd(this string needle, string haystack)
        {
            var idx = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);

            if (idx == -1) return idx;

            return idx + needle.Length;
        }

        public static Scope NextScope(this string text, char open = '{', char close = '}', int startIndex = 0)
        {
            var res = new Scope(text);
            int openBraceIdx = text.IndexOf(open, startIndex);
            int closeBraceIdx = -1;
            int extraOpens = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (i <= openBraceIdx) continue;

                var c = text[i];

                if (c == close)
                {
                    if (extraOpens > 0)
                    {
                        extraOpens--;
                    }
                    else
                    {
                        closeBraceIdx = i - 1;
                        break;
                    }
                }

                if (c == open)
                {
                    extraOpens++;
                }
            }

            if (closeBraceIdx == -1)
                return null;

            res.OpenIdx = openBraceIdx;
            res.CloseIdx = closeBraceIdx;
            res.Content = text.Substring(openBraceIdx + 1, closeBraceIdx - openBraceIdx);

            return res;
        }

        public static string GetFirstValueBetween(this string text, ReadOnlySpan<char> open, ReadOnlySpan<char> close, out int endIdx)
        {
            var span = text.AsSpan();

            endIdx = -1;
            var startIdx = span.IndexOf(open);

            if (startIdx == -1) return null;

            endIdx = span[startIdx..].IndexOf(close);

            if (endIdx == -1) return null;

            endIdx = endIdx + close.Length + startIdx;

            var realEndIdx = endIdx - close.Length;
            var realStartIdx = startIdx + open.Length;

            return span[realStartIdx..realEndIdx].ToString();
        }

        /// <summary>
        /// Get the value between the first occurrence of open and close.
        /// The result will also contain the open and close values
        /// </summary>
        /// <param name="span"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public static ReadOnlySpan<char> GetFirstBetween(this ReadOnlySpan<char> span, ReadOnlySpan<char> open, ReadOnlySpan<char> close, out int endIdx)
        {
            endIdx = -1;
            var startIdx = span.IndexOf(open);

            if (startIdx == -1) return null;

            endIdx = span[startIdx..].IndexOf(close) + close.Length + startIdx;

            return span[startIdx..endIdx];
        }

        public static ReadOnlySpan<char> GetFirstBetween(this ReadOnlySpan<char> span,
            ReadOnlySpan<char> open,
            ReadOnlySpan<char> close,
            int startingIdx,
            out int endIdx)
        {
            endIdx = -1;
            var endIdxInternal = -1;
            var startIdx = span[startingIdx..].IndexOf(open);

            if (startIdx == -1) return null;

            endIdxInternal = span[startingIdx..][startIdx..].IndexOf(close) + close.Length + startIdx;
            endIdx = endIdxInternal + startingIdx;

            return span[startingIdx..][startIdx..endIdxInternal];
        }

        /// <summary>
        /// Get the value between the first occurrence of open and close.
        /// The result will also contain the open and close values
        /// </summary>
        /// <param name="span"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public static ReadOnlySpan<char> GetFirstBetween(this ReadOnlySpan<char> span, ReadOnlySpan<char> open, char close, out int endIdx)
        {
            endIdx = -1;
            var startIdx = span.IndexOf(open);

            if (startIdx == -1) return null;

            endIdx = span[startIdx..].IndexOf(close) + 1 + startIdx;

            return span[startIdx..endIdx];
        }

        public static ObservableCollection<T> AsObservable<T>(this IEnumerable<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        /// <summary>
        /// Search starting from a directory, if no files are found go one folder back and then search recursively all folders and files for a file that matches the fileMask
        /// </summary>
        public static IEnumerable<string> BackwardsFileNameSearch(
            this string dir,
            string fileMask,
            string stopFilePath = null)
        {
            //if starting from a file, get its directory
            if (Path.GetExtension(dir) != null)
            {
                dir = Path.GetDirectoryName(dir);
            }

            var searchFolder = dir;
            IEnumerable<string> fileMatches;

            do
            {
                fileMatches = Directory.EnumerateFiles(searchFolder, fileMask, SearchOption.AllDirectories);

                if (fileMatches.Any())
                {
                    break;
                }

                if (stopFilePath != null && searchFolder == stopFilePath)
                {
                    break;
                }

                searchFolder = Directory.GetParent(searchFolder).FullName;

            } while (true);

            return fileMatches;
        }
    }
}
