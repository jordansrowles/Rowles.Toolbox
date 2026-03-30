namespace Rowles.Toolbox.Core.WebNetwork;

public static class HtmlEntityReferenceCore
{
    public sealed record HtmlEntity(string Name, string Character, string DecimalCode, string HexCode, string Category, string Description);

    public static readonly string[] Categories =
        ["Punctuation", "Currency", "Math", "Arrows", "Greek", "Symbols", "Diacritics", "Special"];

    public static string FormatName(HtmlEntity entity) => $"&{entity.Name};";

    public static string CategoryBadgeClass(string category) => category switch
    {
        "Punctuation" => "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300",
        "Currency" => "bg-yellow-100 dark:bg-yellow-900 text-yellow-700 dark:text-yellow-300",
        "Math" => "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
        "Arrows" => "bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300",
        "Greek" => "bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300",
        "Symbols" => "bg-pink-100 dark:bg-pink-900 text-pink-700 dark:text-pink-300",
        "Diacritics" => "bg-orange-100 dark:bg-orange-900 text-orange-700 dark:text-orange-300",
        "Special" => "bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300",
        _ => "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300",
    };

    public static string CategoryIcon(string category) => category switch
    {
        "Punctuation" => "ti ti-quote",
        "Currency" => "ti ti-coin",
        "Math" => "ti ti-plus-minus",
        "Arrows" => "ti ti-arrows-left-right",
        "Greek" => "ti ti-abc",
        "Symbols" => "ti ti-star",
        "Diacritics" => "ti ti-language",
        "Special" => "ti ti-eye-off",
        _ => "ti ti-point",
    };

    public static bool IsInvisible(HtmlEntity entity) =>
        entity.Name is "nbsp" or "ensp" or "emsp" or "thinsp" or "shy" or "zwj" or "zwnj" or "lrm" or "rlm";

    public static readonly List<HtmlEntity> AllEntities =
    [
        // Punctuation
        new("amp", "\u0026", "&#38;", "&#x26;", "Punctuation", "Ampersand"),
        new("lt", "\u003C", "&#60;", "&#x3C;", "Punctuation", "Less-than sign"),
        new("gt", "\u003E", "&#62;", "&#x3E;", "Punctuation", "Greater-than sign"),
        new("quot", "\u0022", "&#34;", "&#x22;", "Punctuation", "Quotation mark"),
        new("apos", "\u0027", "&#39;", "&#x27;", "Punctuation", "Apostrophe"),
        new("nbsp", "\u00A0", "&#160;", "&#xA0;", "Punctuation", "Non-breaking space"),
        new("ndash", "\u2013", "&#8211;", "&#x2013;", "Punctuation", "En dash"),
        new("mdash", "\u2014", "&#8212;", "&#x2014;", "Punctuation", "Em dash"),
        new("lsquo", "\u2018", "&#8216;", "&#x2018;", "Punctuation", "Left single quotation mark"),
        new("rsquo", "\u2019", "&#8217;", "&#x2019;", "Punctuation", "Right single quotation mark"),
        new("ldquo", "\u201C", "&#8220;", "&#x201C;", "Punctuation", "Left double quotation mark"),
        new("rdquo", "\u201D", "&#8221;", "&#x201D;", "Punctuation", "Right double quotation mark"),
        new("hellip", "\u2026", "&#8230;", "&#x2026;", "Punctuation", "Horizontal ellipsis"),
        new("bull", "\u2022", "&#8226;", "&#x2022;", "Punctuation", "Bullet"),
        new("middot", "\u00B7", "&#183;", "&#xB7;", "Punctuation", "Middle dot"),
        new("ensp", "\u2002", "&#8194;", "&#x2002;", "Punctuation", "En space"),
        new("emsp", "\u2003", "&#8195;", "&#x2003;", "Punctuation", "Em space"),
        new("thinsp", "\u2009", "&#8201;", "&#x2009;", "Punctuation", "Thin space"),

        // Currency
        new("cent", "\u00A2", "&#162;", "&#xA2;", "Currency", "Cent sign"),
        new("pound", "\u00A3", "&#163;", "&#xA3;", "Currency", "Pound sign"),
        new("yen", "\u00A5", "&#165;", "&#xA5;", "Currency", "Yen sign"),
        new("euro", "\u20AC", "&#8364;", "&#x20AC;", "Currency", "Euro sign"),
        new("curren", "\u00A4", "&#164;", "&#xA4;", "Currency", "General currency sign"),

        // Math
        new("plus", "\u002B", "&#43;", "&#x2B;", "Math", "Plus sign"),
        new("minus", "\u2212", "&#8722;", "&#x2212;", "Math", "Minus sign"),
        new("times", "\u00D7", "&#215;", "&#xD7;", "Math", "Multiplication sign"),
        new("divide", "\u00F7", "&#247;", "&#xF7;", "Math", "Division sign"),
        new("le", "\u2264", "&#8804;", "&#x2264;", "Math", "Less-than or equal to"),
        new("ge", "\u2265", "&#8805;", "&#x2265;", "Math", "Greater-than or equal to"),
        new("ne", "\u2260", "&#8800;", "&#x2260;", "Math", "Not equal to"),
        new("asymp", "\u2248", "&#8776;", "&#x2248;", "Math", "Almost equal to"),
        new("sum", "\u2211", "&#8721;", "&#x2211;", "Math", "N-ary summation"),
        new("prod", "\u220F", "&#8719;", "&#x220F;", "Math", "N-ary product"),
        new("radic", "\u221A", "&#8730;", "&#x221A;", "Math", "Square root"),
        new("infin", "\u221E", "&#8734;", "&#x221E;", "Math", "Infinity"),
        new("int", "\u222B", "&#8747;", "&#x222B;", "Math", "Integral"),
        new("part", "\u2202", "&#8706;", "&#x2202;", "Math", "Partial differential"),
        new("nabla", "\u2207", "&#8711;", "&#x2207;", "Math", "Nabla / backward difference"),
        new("prop", "\u221D", "&#8733;", "&#x221D;", "Math", "Proportional to"),
        new("forall", "\u2200", "&#8704;", "&#x2200;", "Math", "For all"),
        new("exist", "\u2203", "&#8707;", "&#x2203;", "Math", "There exists"),
        new("empty", "\u2205", "&#8709;", "&#x2205;", "Math", "Empty set"),
        new("isin", "\u2208", "&#8712;", "&#x2208;", "Math", "Element of"),
        new("notin", "\u2209", "&#8713;", "&#x2209;", "Math", "Not an element of"),
        new("sub", "\u2282", "&#8834;", "&#x2282;", "Math", "Subset of"),
        new("sup", "\u2283", "&#8835;", "&#x2283;", "Math", "Superset of"),
        new("cap", "\u2229", "&#8745;", "&#x2229;", "Math", "Intersection"),
        new("cup", "\u222A", "&#8746;", "&#x222A;", "Math", "Union"),
        new("ang", "\u2220", "&#8736;", "&#x2220;", "Math", "Angle"),
        new("perp", "\u22A5", "&#8869;", "&#x22A5;", "Math", "Perpendicular"),
        new("there4", "\u2234", "&#8756;", "&#x2234;", "Math", "Therefore"),
        new("equiv", "\u2261", "&#8801;", "&#x2261;", "Math", "Identical to"),
        new("oplus", "\u2295", "&#8853;", "&#x2295;", "Math", "Circled plus"),

        // Arrows
        new("larr", "\u2190", "&#8592;", "&#x2190;", "Arrows", "Left arrow"),
        new("uarr", "\u2191", "&#8593;", "&#x2191;", "Arrows", "Up arrow"),
        new("rarr", "\u2192", "&#8594;", "&#x2192;", "Arrows", "Right arrow"),
        new("darr", "\u2193", "&#8595;", "&#x2193;", "Arrows", "Down arrow"),
        new("harr", "\u2194", "&#8596;", "&#x2194;", "Arrows", "Left-right arrow"),
        new("lArr", "\u21D0", "&#8656;", "&#x21D0;", "Arrows", "Left double arrow"),
        new("uArr", "\u21D1", "&#8657;", "&#x21D1;", "Arrows", "Up double arrow"),
        new("rArr", "\u21D2", "&#8658;", "&#x21D2;", "Arrows", "Right double arrow"),
        new("dArr", "\u21D3", "&#8659;", "&#x21D3;", "Arrows", "Down double arrow"),
        new("hArr", "\u21D4", "&#8660;", "&#x21D4;", "Arrows", "Left-right double arrow"),
        new("crarr", "\u21B5", "&#8629;", "&#x21B5;", "Arrows", "Carriage return arrow"),

        // Greek uppercase
        new("Alpha", "\u0391", "&#913;", "&#x391;", "Greek", "Greek capital letter Alpha"),
        new("Beta", "\u0392", "&#914;", "&#x392;", "Greek", "Greek capital letter Beta"),
        new("Gamma", "\u0393", "&#915;", "&#x393;", "Greek", "Greek capital letter Gamma"),
        new("Delta", "\u0394", "&#916;", "&#x394;", "Greek", "Greek capital letter Delta"),
        new("Epsilon", "\u0395", "&#917;", "&#x395;", "Greek", "Greek capital letter Epsilon"),
        new("Zeta", "\u0396", "&#918;", "&#x396;", "Greek", "Greek capital letter Zeta"),
        new("Eta", "\u0397", "&#919;", "&#x397;", "Greek", "Greek capital letter Eta"),
        new("Theta", "\u0398", "&#920;", "&#x398;", "Greek", "Greek capital letter Theta"),
        new("Iota", "\u0399", "&#921;", "&#x399;", "Greek", "Greek capital letter Iota"),
        new("Kappa", "\u039A", "&#922;", "&#x39A;", "Greek", "Greek capital letter Kappa"),
        new("Lambda", "\u039B", "&#923;", "&#x39B;", "Greek", "Greek capital letter Lambda"),
        new("Mu", "\u039C", "&#924;", "&#x39C;", "Greek", "Greek capital letter Mu"),
        new("Nu", "\u039D", "&#925;", "&#x39D;", "Greek", "Greek capital letter Nu"),
        new("Xi", "\u039E", "&#926;", "&#x39E;", "Greek", "Greek capital letter Xi"),
        new("Omicron", "\u039F", "&#927;", "&#x39F;", "Greek", "Greek capital letter Omicron"),
        new("Pi", "\u03A0", "&#928;", "&#x3A0;", "Greek", "Greek capital letter Pi"),
        new("Rho", "\u03A1", "&#929;", "&#x3A1;", "Greek", "Greek capital letter Rho"),
        new("Sigma", "\u03A3", "&#931;", "&#x3A3;", "Greek", "Greek capital letter Sigma"),
        new("Tau", "\u03A4", "&#932;", "&#x3A4;", "Greek", "Greek capital letter Tau"),
        new("Upsilon", "\u03A5", "&#933;", "&#x3A5;", "Greek", "Greek capital letter Upsilon"),
        new("Phi", "\u03A6", "&#934;", "&#x3A6;", "Greek", "Greek capital letter Phi"),
        new("Chi", "\u03A7", "&#935;", "&#x3A7;", "Greek", "Greek capital letter Chi"),
        new("Psi", "\u03A8", "&#936;", "&#x3A8;", "Greek", "Greek capital letter Psi"),
        new("Omega", "\u03A9", "&#937;", "&#x3A9;", "Greek", "Greek capital letter Omega"),

        // Greek lowercase
        new("alpha", "\u03B1", "&#945;", "&#x3B1;", "Greek", "Greek small letter alpha"),
        new("beta", "\u03B2", "&#946;", "&#x3B2;", "Greek", "Greek small letter beta"),
        new("gamma", "\u03B3", "&#947;", "&#x3B3;", "Greek", "Greek small letter gamma"),
        new("delta", "\u03B4", "&#948;", "&#x3B4;", "Greek", "Greek small letter delta"),
        new("epsilon", "\u03B5", "&#949;", "&#x3B5;", "Greek", "Greek small letter epsilon"),
        new("zeta", "\u03B6", "&#950;", "&#x3B6;", "Greek", "Greek small letter zeta"),
        new("eta", "\u03B7", "&#951;", "&#x3B7;", "Greek", "Greek small letter eta"),
        new("theta", "\u03B8", "&#952;", "&#x3B8;", "Greek", "Greek small letter theta"),
        new("iota", "\u03B9", "&#953;", "&#x3B9;", "Greek", "Greek small letter iota"),
        new("kappa", "\u03BA", "&#954;", "&#x3BA;", "Greek", "Greek small letter kappa"),
        new("lambda", "\u03BB", "&#955;", "&#x3BB;", "Greek", "Greek small letter lambda"),
        new("mu", "\u03BC", "&#956;", "&#x3BC;", "Greek", "Greek small letter mu"),
        new("nu", "\u03BD", "&#957;", "&#x3BD;", "Greek", "Greek small letter nu"),
        new("xi", "\u03BE", "&#958;", "&#x3BE;", "Greek", "Greek small letter xi"),
        new("omicron", "\u03BF", "&#959;", "&#x3BF;", "Greek", "Greek small letter omicron"),
        new("pi", "\u03C0", "&#960;", "&#x3C0;", "Greek", "Greek small letter pi"),
        new("rho", "\u03C1", "&#961;", "&#x3C1;", "Greek", "Greek small letter rho"),
        new("sigmaf", "\u03C2", "&#962;", "&#x3C2;", "Greek", "Greek small letter final sigma"),
        new("sigma", "\u03C3", "&#963;", "&#x3C3;", "Greek", "Greek small letter sigma"),
        new("tau", "\u03C4", "&#964;", "&#x3C4;", "Greek", "Greek small letter tau"),
        new("upsilon", "\u03C5", "&#965;", "&#x3C5;", "Greek", "Greek small letter upsilon"),
        new("phi", "\u03C6", "&#966;", "&#x3C6;", "Greek", "Greek small letter phi"),
        new("chi", "\u03C7", "&#967;", "&#x3C7;", "Greek", "Greek small letter chi"),
        new("psi", "\u03C8", "&#968;", "&#x3C8;", "Greek", "Greek small letter psi"),
        new("omega", "\u03C9", "&#969;", "&#x3C9;", "Greek", "Greek small letter omega"),

        // Greek variants
        new("thetasym", "\u03D1", "&#977;", "&#x3D1;", "Greek", "Greek theta symbol"),
        new("upsih", "\u03D2", "&#978;", "&#x3D2;", "Greek", "Greek upsilon with hook symbol"),
        new("piv", "\u03D6", "&#982;", "&#x3D6;", "Greek", "Greek pi symbol"),

        // Symbols
        new("copy", "\u00A9", "&#169;", "&#xA9;", "Symbols", "Copyright sign"),
        new("reg", "\u00AE", "&#174;", "&#xAE;", "Symbols", "Registered sign"),
        new("trade", "\u2122", "&#8482;", "&#x2122;", "Symbols", "Trademark sign"),
        new("deg", "\u00B0", "&#176;", "&#xB0;", "Symbols", "Degree sign"),
        new("para", "\u00B6", "&#182;", "&#xB6;", "Symbols", "Pilcrow / paragraph sign"),
        new("sect", "\u00A7", "&#167;", "&#xA7;", "Symbols", "Section sign"),
        new("dagger", "\u2020", "&#8224;", "&#x2020;", "Symbols", "Dagger"),
        new("Dagger", "\u2021", "&#8225;", "&#x2021;", "Symbols", "Double dagger"),
        new("loz", "\u25CA", "&#9674;", "&#x25CA;", "Symbols", "Lozenge"),
        new("spades", "\u2660", "&#9824;", "&#x2660;", "Symbols", "Spade suit"),
        new("clubs", "\u2663", "&#9827;", "&#x2663;", "Symbols", "Club suit"),
        new("hearts", "\u2665", "&#9829;", "&#x2665;", "Symbols", "Heart suit"),
        new("diams", "\u2666", "&#9830;", "&#x2666;", "Symbols", "Diamond suit"),
        new("permil", "\u2030", "&#8240;", "&#x2030;", "Symbols", "Per mille sign"),
        new("prime", "\u2032", "&#8242;", "&#x2032;", "Symbols", "Prime / minutes"),
        new("Prime", "\u2033", "&#8243;", "&#x2033;", "Symbols", "Double prime / seconds"),
        new("oline", "\u203E", "&#8254;", "&#x203E;", "Symbols", "Overline"),
        new("frasl", "\u2044", "&#8260;", "&#x2044;", "Symbols", "Fraction slash"),
        new("weierp", "\u2118", "&#8472;", "&#x2118;", "Symbols", "Weierstrass P"),
        new("alefsym", "\u2135", "&#8501;", "&#x2135;", "Symbols", "Alef symbol"),
        new("check", "\u2713", "&#10003;", "&#x2713;", "Symbols", "Check mark"),
        new("cross", "\u2717", "&#10007;", "&#x2717;", "Symbols", "Ballot X mark"),

        // Diacritics
        new("Agrave", "\u00C0", "&#192;", "&#xC0;", "Diacritics", "Latin A with grave"),
        new("Aacute", "\u00C1", "&#193;", "&#xC1;", "Diacritics", "Latin A with acute"),
        new("Acirc", "\u00C2", "&#194;", "&#xC2;", "Diacritics", "Latin A with circumflex"),
        new("Atilde", "\u00C3", "&#195;", "&#xC3;", "Diacritics", "Latin A with tilde"),
        new("Auml", "\u00C4", "&#196;", "&#xC4;", "Diacritics", "Latin A with diaeresis"),
        new("Aring", "\u00C5", "&#197;", "&#xC5;", "Diacritics", "Latin A with ring above"),
        new("AElig", "\u00C6", "&#198;", "&#xC6;", "Diacritics", "Latin ligature AE"),
        new("Ccedil", "\u00C7", "&#199;", "&#xC7;", "Diacritics", "Latin C with cedilla"),
        new("Egrave", "\u00C8", "&#200;", "&#xC8;", "Diacritics", "Latin E with grave"),
        new("Eacute", "\u00C9", "&#201;", "&#xC9;", "Diacritics", "Latin E with acute"),
        new("Ecirc", "\u00CA", "&#202;", "&#xCA;", "Diacritics", "Latin E with circumflex"),
        new("Euml", "\u00CB", "&#203;", "&#xCB;", "Diacritics", "Latin E with diaeresis"),
        new("Ntilde", "\u00D1", "&#209;", "&#xD1;", "Diacritics", "Latin N with tilde"),
        new("Ograve", "\u00D2", "&#210;", "&#xD2;", "Diacritics", "Latin O with grave"),
        new("Oacute", "\u00D3", "&#211;", "&#xD3;", "Diacritics", "Latin O with acute"),
        new("Ouml", "\u00D6", "&#214;", "&#xD6;", "Diacritics", "Latin O with diaeresis"),
        new("Ugrave", "\u00D9", "&#217;", "&#xD9;", "Diacritics", "Latin U with grave"),
        new("Uacute", "\u00DA", "&#218;", "&#xDA;", "Diacritics", "Latin U with acute"),
        new("Uuml", "\u00DC", "&#220;", "&#xDC;", "Diacritics", "Latin U with diaeresis"),
        new("szlig", "\u00DF", "&#223;", "&#xDF;", "Diacritics", "Latin sharp S (eszett)"),

        // Special
        new("shy", "\u00AD", "&#173;", "&#xAD;", "Special", "Soft hyphen"),
        new("zwj", "\u200D", "&#8205;", "&#x200D;", "Special", "Zero-width joiner"),
        new("zwnj", "\u200C", "&#8204;", "&#x200C;", "Special", "Zero-width non-joiner"),
        new("lrm", "\u200E", "&#8206;", "&#x200E;", "Special", "Left-to-right mark"),
        new("rlm", "\u200F", "&#8207;", "&#x200F;", "Special", "Right-to-left mark"),
    ];
}
