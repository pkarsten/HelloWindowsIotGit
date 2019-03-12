using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;

namespace HelloWindowsIot
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Readable Names for CommonFolderQuery enums 
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string ReadableName(this CommonFolderQuery enumValue)
        {
            string stringValue = default(String);
            switch (enumValue)
            {
                case CommonFolderQuery.DefaultQuery:
                    stringValue = AppcFuncs.GetLanguage("GroupDefaultQuery");
                    break;
                case CommonFolderQuery.GroupByAlbum:
                    stringValue = AppcFuncs.GetLanguage("GroupByAlbum");
                    break;
                case CommonFolderQuery.GroupByAlbumArtist:
                    stringValue = AppcFuncs.GetLanguage("GroupByAlbumArtist");
                    break;
                case CommonFolderQuery.GroupByArtist:
                    stringValue = AppcFuncs.GetLanguage("GroupByArtist");
                    break;
                case CommonFolderQuery.GroupByAuthor:
                    stringValue = AppcFuncs.GetLanguage("GroupByAuthor");
                    break;
                case CommonFolderQuery.GroupByComposer:
                    stringValue = AppcFuncs.GetLanguage("GroupByComposer");
                    break;
                case CommonFolderQuery.GroupByGenre:
                    stringValue = AppcFuncs.GetLanguage("GroupByGenre");
                    break;
                case CommonFolderQuery.GroupByMonth:
                    stringValue = AppcFuncs.GetLanguage("GroupByMonth");
                    break;
                case CommonFolderQuery.GroupByPublishedYear:
                    stringValue = AppcFuncs.GetLanguage("GroupByPublishedYear");
                    break;
                case CommonFolderQuery.GroupByRating:
                    stringValue = AppcFuncs.GetLanguage("GroupByRating");
                    break;
                case CommonFolderQuery.GroupByTag:
                    stringValue = AppcFuncs.GetLanguage("GroupByTag");
                    break;
                case CommonFolderQuery.GroupByType:
                    stringValue = AppcFuncs.GetLanguage("GroupByType");
                    break;
                case CommonFolderQuery.GroupByYear:
                    stringValue = AppcFuncs.GetLanguage("GroupByYear");
                    break;
                default:
                    stringValue = "";
                    return null;
            }
            return stringValue;
        }
    }
}
