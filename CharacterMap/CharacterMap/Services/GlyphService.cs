﻿//#define GENERATE_DATA

using CharacterMap.Core;
using CharacterMap.Provider;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace CharacterMap.Services
{
    public interface IGlyphData
    {
        int UnicodeIndex { get; }
        string UnicodeHex { get; }
        string Description { get; }
    }

    public class GlyphDescription : IGlyphData
    {
        [PrimaryKey]
        public int UnicodeIndex { get; set; }

        [Indexed]
        [MaxLength(5)]
        public string UnicodeHex { get; set; }

        public string Description { get; set; }
    }

    public class MDL2Glyph : GlyphDescription { }

    public class FontAwesomeGlyph : GlyphDescription { }

    public interface IGlyphDataProvider
    {
        void Initialise();
        string GetCharacterDescription(int unicodeIndex, FontVariant variant);
        Task<IReadOnlyList<IGlyphData>> SearchAsync(string query, FontVariant variant);
    }

    public static partial class GlyphService
    {
        private static IGlyphDataProvider _provider { get; set; }

        private static Task _init { get; set; }

        public static IReadOnlyList<IGlyphData> EMPTY_SEARCH = new List<IGlyphData>();

        static GlyphService()
        {
            _provider = new SQLiteGlyphProvider();
            _provider.Initialise();
        }

        public static Task InitializeAsync()
        {
            if (_init == null)
                _init = InitializeInternalAsync();

            return _init;
        }

        private static Task InitializeInternalAsync()
        {
#if DEBUG && GENERATE_DATA
            if (_provider is SQLiteGlyphProvider p)
            {
                return p.InitialiseDatabaseAsync();
            }
#endif
            return Task.CompletedTask;
        }

        internal static string GetCharacterDescription(int unicodeIndex, FontVariant variant)
        {
            if (variant == null || _provider == null)
                return null;

            return _provider.GetCharacterDescription(unicodeIndex, variant);
        }

        internal static Task<IReadOnlyList<IGlyphData>> SearchAsync(string query, FontVariant variant)
        {
            if (variant == null)
                return Task.FromResult(EMPTY_SEARCH);

            return _provider.SearchAsync(query, variant);
        }
    }
}
