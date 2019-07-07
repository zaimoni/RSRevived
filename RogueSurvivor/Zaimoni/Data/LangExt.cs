using System.Collections.Generic;

namespace Zaimoni.Data
{
    // Grammar support.  Note that English has very simple agreement rules compared to most natural languages.
    class Noun
    {
        private static readonly Dictionary<string,string> irregular_plural = new Dictionary<string,string>();
        private static readonly Dictionary<string, string> irregular_feminine = new Dictionary<string, string>();
        private static readonly Dictionary<string, KeyValuePair<string, string>> elided_adj_nouns = new Dictionary<string, KeyValuePair<string, string>>();
//      private static readonly HashSet<string> registered = new HashSet<string>();
        private readonly string _singular;

        static Noun()
        {
            irregular_plural["knife"] = "knives";
            irregular_plural["man"] = "men";
            irregular_plural["woman"] = "women";
            irregular_feminine["man"] = "woman";
            elided_adj_nouns["policeman"] = new KeyValuePair<string, string>("police", "man");
            elided_adj_nouns["policewoman"] = new KeyValuePair<string, string>("police", "woman");
        }

        private Noun(string x)
        {
          _singular = x;
        }

        public static string Plural(string x) {
          if (irregular_plural.TryGetValue(x, out string ret)) return ret;

          if (elided_adj_nouns.TryGetValue(x,out KeyValuePair<string, string> test)) return test.Key+Noun.Plural(test.Value);

          return x+"s";
        }

        public static string Feminine(string x) {
          if (irregular_feminine.TryGetValue(x, out string ret)) return ret;

          if (elided_adj_nouns.TryGetValue(x,out KeyValuePair<string, string> test)) return test.Key+Noun.Feminine(test.Value);

          return x;
        }
    }

    public static class LangExt
    {
        public enum Gender
        {
          MALE = 0,
          FEMALE,
          NEUTER    // last of basic three genders
        }

        public enum Plurality
        {
          SINGULAR = 0,
          PLURAL    // last of basic plurality
        }

        // most languages react to this for agreement purposes
        public static bool StartsWithVowel(this string name)
        {
            return 0 <= "AEIOUaeiou".IndexOf(name[0]);
        }

        public static string Capitalize(this string x)
        {
          if (string.IsNullOrEmpty(x)) return "";
          if (1 == x.Length) return char.ToUpper(x[0]).ToString();
          return char.ToUpper(x[0]).ToString()+x.Substring(1);
        }

        // names of functions are English-centric
        public static string PrefixIndefiniteSingularArticle(this string name)
        {
          return (name.StartsWithVowel() ? "an " : "a ")+name;
        }

        public static string PrefixIndefinitePluralArticle(this string name)
        {
          return "some "+name;
        }
        public static string PrefixDefiniteSingularArticle(this string name)
        {
            return "the "+name;
        }

        public static string PrefixDefinitePluralArticle(this string name)
        {
            return "some "+name;
        }

        public static KeyValuePair<string, string>? SplitLastWord(this string name)
        {
            int n = name.LastIndexOf(' ');
            if (0 <= n) return new KeyValuePair<string, string>(name.Substring(0, n), name.Substring(n + 1));
            return null;
        }

        public static string Feminine(this string name)
        {
            KeyValuePair<string, string>? test = name.SplitLastWord();
            if (null != test) {
                if ("male" == test.Value.Key.ToLowerInvariant()) return "female " + test.Value.Value;
                return test.Value.Key+" "+Noun.Feminine(test.Value.Value);
            }
            return Noun.Feminine(name);
        }

        public static string Conjugate(this string verb, int person, int qty=1)
        {
            if (3 == person && 1 == qty) return verb + "s";
            return verb;
        }

        // XXX incomplete implementation; have a grammar text available but past a certain point you need a Noun or Verb class.
        // some languages also have the notion of dual # so this isn't even a correct API
        public static string Plural(this string name, bool plural)
        {
          if (!plural) return name;
          KeyValuePair<string, string>? test = name.SplitLastWord();
          if (null != test) return test.Value.Key+" "+Noun.Plural(test.Value.Value);
          return Noun.Plural(name);
        }

        public static string Plural(this string name, int qty) { return name.Plural(1 == qty); }

        // numeric.  The verbal version would be FormalQtyDesc or QtyDescFormal
        public static string QtyDesc(this string name, int qty)
        {
            return qty + " " + name.Plural(qty);
        }
    }
}