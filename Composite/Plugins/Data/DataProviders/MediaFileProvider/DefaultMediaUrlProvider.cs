﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Composite.Core;
using Composite.Core.Extensions;
using Composite.Core.IO;
using Composite.Core.Routing;
using Composite.Core.WebClient;
using Composite.Data;
using Composite.Data.Types;

namespace Composite.Plugins.Data.DataProviders.MediaFileProvider
{
    internal class DefaultMediaUrlProvider: IMediaUrlProvider
    {
        internal static readonly string DefaultMediaStore = "MediaArchive";
        private static readonly string ForbiddenUrlCharacters = @"<>*%&\?#""";

        public string GetUrl(MediaUrlData mediaUrlData)
        {
            IMediaFile file = GetFileById(mediaUrlData.MediaStore, mediaUrlData.MediaId);
            if (file == null)
            {
                return null;
            }

            string pathToFile = UrlUtils.Combine(file.FolderPath, file.FileName);

            pathToFile = RemoveForbiddenCharactersAndNormalize(pathToFile);

            // IIS6 doesn't have wildcard mapping by default, so removing image extension if running in "classic" app pool
            if (!HttpRuntime.UsingIntegratedPipeline)
            {
                int dotOffset = pathToFile.IndexOf(".", StringComparison.Ordinal);
                if (dotOffset >= 0)
                {
                    pathToFile = pathToFile.Substring(0, dotOffset);
                }
            }

            string mediaStore = string.Empty;

            if (!mediaUrlData.MediaStore.Equals(DefaultMediaStore, StringComparison.InvariantCultureIgnoreCase))
            {
                mediaStore = mediaUrlData.MediaStore + "/";
            }


            var url = new UrlBuilder(UrlUtils.PublicRootPath + "/media/" + mediaStore + /* UrlUtils.CompressGuid(*/ mediaUrlData.MediaId /*)*/)
            {
                PathInfo = file.LastWriteTime != null
                    ? "/" + GetDateTimeHash(file.LastWriteTime.Value.ToUniversalTime())
                    : string.Empty
            };

            if (pathToFile.Length > 0)
            {
                url.PathInfo += pathToFile;
            }

            if (mediaUrlData.QueryParameters != null)
            {
                url.AddQueryParameters(mediaUrlData.QueryParameters);
            }

            return url.ToString();
        }


        private static string GetDateTimeHash(DateTime dateTime)
        {
            int hash = dateTime.GetHashCode();
            return Convert.ToBase64String(BitConverter.GetBytes(hash)).Substring(0, 6).Replace('+', '-').Replace('/', '_');
        }


        private static string RemoveForbiddenCharactersAndNormalize(string path)
        {
            // Replacing dots with underscores, so IIS will not intercept requests in some scenarios

            string legalFilePath = RemoveFilePathIllegalCharacters(path);
            string extension = Path.GetExtension(legalFilePath);

            if (!MimeTypeInfo.IsIisServable(extension))
            {
                path = path.Replace('.', '_');
            }

            path = path.Replace('+', ' ');

            foreach (var ch in ForbiddenUrlCharacters)
            {
                path = path.Replace(ch, '#');
            }

            path = path.Replace("#", string.Empty);

            // Removing consecutive white spaces
            while (path.Contains("  "))
            {
                path = path.Replace("  ", " ");
            }

            string[] parts = path.Split('/');

            var result = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                string trimmedPart = parts[i].Trim();
                if (trimmedPart.Length > 0)
                {
                    result.Append("/").Append(trimmedPart);
                }
            }

            // Encoding white spaces
            result.Replace(" ", "%20");

            return result.ToString();
        }


        private static IMediaFile GetFileById(string storeId, Guid fileId)
        {
            using (new DataScope(DataScopeIdentifier.Public))
            {
                var query = DataFacade.GetData<IMediaFile>();

                if (query.IsEnumerableQuery())
                {
                    return (query as IEnumerable<IMediaFile>)
                        .FirstOrDefault(f => f.Id == fileId && f.StoreId == storeId);
                }

                return query
                    .FirstOrDefault(f => f.StoreId == storeId && f.Id == fileId);
            }
        }


        private static string RemoveFilePathIllegalCharacters(string path)
        {
            path = path.Replace('\"', ' ').Replace('<', ' ').Replace('>', ' ').Replace('|', ' ');
            for (int i = 0; i < 31; i++)
            {
                path = path.Replace((char)i, ' ');
            }
            return path;
        }
    }
}
