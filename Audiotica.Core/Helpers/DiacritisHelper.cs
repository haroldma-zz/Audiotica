using System.Text;

namespace Audiotica.Core.Helpers
{
    public static class DiacritisHelper
    {
        public enum DictionaryDef
        {
            NotSet,
            Arabic,
            Bulgarian,
            Catalan,
            ChineseSimplified,
            ChineseTraditional,
            Croatian,
            Czech,
            CzechAlt,
            Danish,
            Dutch,
            DutchBelgium,
            English,
            Estonian,
            Finnish,
            French,
            CanadianFrench,
            SwissFrench,
            German,
            Greek,
            Hebrew,
            Hungarian,
            Icelandic,
            Italian,
            Latvian,
            Norwegian,
            Polish,
            Portuguese,
            BrazilianPortuguese,
            Romanian,
            Russian,
            Spanish,
            Slovak,
            SlovakAlt,
            Slovenian,
            Swedish,
            Turkish,
            Ukrainian
        }

        private static readonly char[] arabicAccents =
        {
            'أ', 'إ', 'آ', 'ء', 'پ', 'ض', 'ذ', 'ـ', 'خ', 'خ', 'غ', 'ش', 'ة', 'ث', 'ً',
            'ٰ', 'ؤ', 'ظ', 'ى', 'ئ'
        };

        private static readonly char[] arabicReplace =
        {
            'ا', 'ا', 'ا', 'ا', 'ب', 'ص', 'د', 'ّ', 'ح', 'ك', 'ع', 'س', 'ت', 'ت', 'َ',
            'َ', 'و', 'ط', 'ي', 'ي'
        };

        private static readonly char[] bulgarianAccents = {'ъ', 'ѝ'};
        private static readonly char[] bulgarianReplace = {'ь', 'и'};
        private static readonly char[] catalanAccents = {'à', 'è', 'é', 'í', 'ï', 'ò', 'ó', 'ú', 'ü'};
        private static readonly char[] catalanReplace = {'a', 'e', 'e', 'i', 'i', 'o', 'o', 'u', 'u'};
        private static readonly char[] croatianAccents = {'č', 'ć', 'đ', 'š', 'ž'};
        private static readonly char[] croatianReplace = {'c', 'c', 'd', 's', 'z'};

        private static readonly char[] czechAccents =
        {
            'ã', 'á', 'á', 'č', 'ď', 'é', 'ě', 'í', 'ň', 'ó', 'ř', 'š', 'ť', 'ú', 'ů',
            'ý', 'ž'
        };

        private static readonly char[] czechReplace =
        {
            'a', 'a', 'a', 'c', 'd', 'e', 'e', 'i', 'n', 'o', 'r', 's', 't', 'u', 'u',
            'y', 'z'
        };

        private static readonly char[] dutchAccents = {'é', 'ë', 'ï', 'ó', 'ö', 'ü'};
        private static readonly char[] dutchReplace = {'e', 'e', 'i', 'o', 'o', 'u'};
        private static readonly char[] englishAccents = {'é'};
        private static readonly char[] englishReplace = {'e'};
        private static readonly char[] estonianAccents = {'ä', 'ö', 'õ', 'ü'};
        private static readonly char[] estonianReplace = {'a', 'o', 'o', 'u'};

        private static readonly char[] frenchAccents =
        {
            'à', 'â', 'ä', 'æ', 'ç', 'é', 'è', 'ê', 'ë', 'î', 'ï', 'ô', 'œ', 'ù', 'û',
            'ü'
        };

        private static readonly char[] frenchReplace =
        {
            'a', 'a', 'a', 'a', 'c', 'e', 'e', 'e', 'e', 'i', 'i', 'o', 'o', 'u', 'u',
            'u'
        };

        private static readonly char[] germanAccents = {'ä', 'ö', 'ü', 'ß'};
        private static readonly char[] germanReplace = {'a', 'o', 'u', 's'};
        private static readonly char[] greekAccents = {'ά', 'έ', 'ή', 'ί', 'ϊ', 'ΐ', 'ό', 'ύ', 'ϋ', 'ΰ', 'ώ'};
        private static readonly char[] greekReplace = {'α', 'ε', 'η', 'ι', 'ι', 'ι', 'ο', 'υ', 'υ', 'υ', 'ω'};
        private static readonly char[] hungarianAccents = {'á', 'é', 'í', 'ö', 'ó', 'ő', 'ü', 'ú', 'ű'};
        private static readonly char[] hungarianReplace = {'a', 'e', 'i', 'o', 'o', 'o', 'u', 'u', 'u'};
        private static readonly char[] icelandicAccents = {'ö'};
        private static readonly char[] icelandicReplace = {'o'};
        private static readonly char[] italianAccents = {'à', 'è', 'é', 'ì', 'ò', 'ó', 'ù'};
        private static readonly char[] italianReplace = {'a', 'e', 'e', 'i', 'o', 'o', 'u'};
        private static readonly char[] latvianAccents = {'ē'};
        private static readonly char[] latvianReplace = {'e'};
        private static readonly char[] polishAccents = {'ą', 'ć', 'ę', 'ł', 'ń', 'ó', 'ś', 'ż', 'ź'};
        private static readonly char[] polishReplace = {'a', 'c', 'e', 'l', 'n', 'o', 's', 'z', 'z'};
        private static readonly char[] portugueseAccents = {'ã', 'á', 'â', 'à', 'é', 'ê', 'í', 'õ', 'ó', 'ô', 'ú', 'ü'};
        private static readonly char[] portugueseReplace = {'a', 'a', 'a', 'a', 'e', 'e', 'i', 'o', 'o', 'o', 'u', 'u'};
        private static readonly char[] romanianAccents = {'ă', 'â', 'î', 'ş', 'ș', 'ţ', 'ț'};
        private static readonly char[] romanianReplace = {'a', 'a', 'i', 's', 's', 't', 't'};
        private static readonly char[] russianAccents = {'ъ'};
        private static readonly char[] russianReplace = {'b'};
        private static readonly StringBuilder sbStripAccents = new StringBuilder();

        private static readonly char[] slovakianAccents =
        {
            'á', 'ä', 'č', 'ď', 'é', 'í', 'ĺ', 'ľ', 'ň', 'ó', 'ô', 'ŕ', 'š', 'ť',
            'ú', 'ý', 'ž'
        };

        private static readonly char[] slovakianReplace =
        {
            'a', 'a', 'c', 'd', 'e', 'i', 'l', 'l', 'n', 'o', 'o', 'r', 's', 't',
            'u', 'y', 'z'
        };

        private static readonly char[] spanishAccents = {'á', 'é', 'í', 'ó', 'ú'};
        private static readonly char[] spanishReplace = {'a', 'e', 'i', 'o', 'u'};
        private static readonly char[] turkishAccents = {'ç', 'é', 'ë', 'ğ', 'İ', 'ï', 'ó', 'ö', 'ü'};
        private static readonly char[] turkishReplace = {'c', 'e', 'e', 'g', 'i', 'i', 'o', 'o', 'u'};
        private static readonly char[] ukarainianAccents = {'ї', 'ґ'};
        private static readonly char[] ukarainianReplace = {'i', 'r'};

        private static char[] filipinoAccents =
        {
            'á', 'à', 'â', 'é', 'è', 'ê', 'í', 'ì', 'î', 'ó', 'ò', 'ô', 'ú', 'ù',
            'û'
        };

        private static char[] filipinoReplace =
        {
            'a', 'a', 'a', 'e', 'e', 'e', 'i', 'i', 'i', 'o', 'o', 'o', 'u', 'u',
            'u'
        };

        public static string Remove(string accentedStr)
        {
            accentedStr = Remove(accentedStr, DictionaryDef.Arabic);
            accentedStr = Remove(accentedStr, DictionaryDef.Catalan);
            accentedStr = Remove(accentedStr, DictionaryDef.ChineseSimplified);
            accentedStr = Remove(accentedStr, DictionaryDef.ChineseTraditional);
            accentedStr = Remove(accentedStr, DictionaryDef.Croatian);
            accentedStr = Remove(accentedStr, DictionaryDef.Czech);
            accentedStr = Remove(accentedStr, DictionaryDef.CzechAlt);
            accentedStr = Remove(accentedStr, DictionaryDef.Danish);
            accentedStr = Remove(accentedStr, DictionaryDef.Dutch);
            accentedStr = Remove(accentedStr, DictionaryDef.DutchBelgium);
            accentedStr = Remove(accentedStr, DictionaryDef.English);
            accentedStr = Remove(accentedStr, DictionaryDef.Estonian);
            accentedStr = Remove(accentedStr, DictionaryDef.Finnish);
            accentedStr = Remove(accentedStr, DictionaryDef.French);
            accentedStr = Remove(accentedStr, DictionaryDef.CanadianFrench);
            accentedStr = Remove(accentedStr, DictionaryDef.SwissFrench);
            accentedStr = Remove(accentedStr, DictionaryDef.German);
            accentedStr = Remove(accentedStr, DictionaryDef.Greek);
            accentedStr = Remove(accentedStr, DictionaryDef.Hebrew);
            accentedStr = Remove(accentedStr, DictionaryDef.Hungarian);
            accentedStr = Remove(accentedStr, DictionaryDef.Icelandic);
            accentedStr = Remove(accentedStr, DictionaryDef.Italian);
            accentedStr = Remove(accentedStr, DictionaryDef.Latvian);
            accentedStr = Remove(accentedStr, DictionaryDef.Norwegian);
            accentedStr = Remove(accentedStr, DictionaryDef.Polish);
            accentedStr = Remove(accentedStr, DictionaryDef.Portuguese);
            accentedStr = Remove(accentedStr, DictionaryDef.BrazilianPortuguese);
            accentedStr = Remove(accentedStr, DictionaryDef.Romanian);
            accentedStr = Remove(accentedStr, DictionaryDef.Russian);
            accentedStr = Remove(accentedStr, DictionaryDef.Spanish);
            accentedStr = Remove(accentedStr, DictionaryDef.Slovak);
            accentedStr = Remove(accentedStr, DictionaryDef.SlovakAlt);
            accentedStr = Remove(accentedStr, DictionaryDef.Slovenian);
            accentedStr = Remove(accentedStr, DictionaryDef.Swedish);
            accentedStr = Remove(accentedStr, DictionaryDef.Turkish);
            return Remove(accentedStr, DictionaryDef.Ukrainian);
        }

        public static string Remove(string accentedStr, DictionaryDef eDictionary)
        {
            char[] replacement = null;
            char[] accents = null;
            switch (eDictionary)
            {
                case DictionaryDef.Arabic:
                    replacement = arabicReplace;
                    accents = arabicAccents;
                    break;

                case DictionaryDef.Slovak:
                case DictionaryDef.SlovakAlt:
                    replacement = slovakianReplace;
                    accents = slovakianAccents;
                    break;

                case DictionaryDef.Latvian:
                    replacement = latvianReplace;
                    accents = latvianAccents;
                    break;

                case DictionaryDef.Icelandic:
                    replacement = icelandicReplace;
                    accents = icelandicAccents;
                    break;

                case DictionaryDef.Estonian:
                    replacement = estonianReplace;
                    accents = estonianAccents;
                    break;

                case DictionaryDef.Bulgarian:
                    replacement = bulgarianReplace;
                    accents = bulgarianAccents;
                    break;

                case DictionaryDef.Romanian:
                    replacement = romanianReplace;
                    accents = romanianAccents;
                    break;

                case DictionaryDef.Croatian:
                case DictionaryDef.Slovenian:
                    replacement = croatianReplace;
                    accents = croatianAccents;
                    break;

                case DictionaryDef.English:
                    replacement = englishReplace;
                    accents = englishAccents;
                    break;

                case DictionaryDef.French:
                case DictionaryDef.CanadianFrench:
                case DictionaryDef.SwissFrench:
                    replacement = frenchReplace;
                    accents = frenchAccents;
                    break;

                case DictionaryDef.German:
                    replacement = germanReplace;
                    accents = germanAccents;
                    break;

                case DictionaryDef.Spanish:
                    replacement = spanishReplace;
                    accents = spanishAccents;
                    break;

                case DictionaryDef.Catalan:
                    replacement = catalanReplace;
                    accents = catalanAccents;
                    break;

                case DictionaryDef.Italian:
                    replacement = italianReplace;
                    accents = italianAccents;
                    break;

                case DictionaryDef.Polish:
                    replacement = polishReplace;
                    accents = polishAccents;
                    break;

                case DictionaryDef.Hungarian:
                    replacement = hungarianReplace;
                    accents = hungarianAccents;
                    break;

                case DictionaryDef.Portuguese:
                case DictionaryDef.BrazilianPortuguese:
                    replacement = portugueseReplace;
                    accents = portugueseAccents;
                    break;

                case DictionaryDef.Czech:
                case DictionaryDef.CzechAlt:
                    replacement = czechReplace;
                    accents = czechAccents;
                    break;

                case DictionaryDef.Dutch:
                    replacement = dutchReplace;
                    accents = dutchAccents;
                    break;

                case DictionaryDef.Turkish:
                    replacement = turkishReplace;
                    accents = turkishAccents;
                    break;

                case DictionaryDef.Russian:
                    replacement = russianReplace;
                    accents = russianAccents;
                    break;

                case DictionaryDef.Ukrainian:
                    replacement = ukarainianReplace;
                    accents = ukarainianAccents;
                    break;

                case DictionaryDef.Greek:
                    replacement = greekReplace;
                    accents = greekAccents;
                    break;

                default:
                    return accentedStr;
            }

            if (accents != null &&
                replacement != null &&
                accentedStr.IndexOfAny(accents) > 0)
            {
                sbStripAccents.Length = 0;
                sbStripAccents.Append(accentedStr);
                for (var i = 0;
                    i < accents.Length;
                    i++)
                {
                    sbStripAccents.Replace(accents[i], replacement[i]);
                }

                return sbStripAccents.ToString();
            }
            return accentedStr;
        }
    }
}