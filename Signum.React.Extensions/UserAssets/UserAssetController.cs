﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Signum.Entities;
using Signum.Utilities;
using Signum.React.ApiControllers;
using Signum.Engine.Basics;
using Signum.Entities.UserAssets;
using Signum.Entities.DynamicQuery;
using Signum.Engine;
using Signum.React.Files;
using Signum.Engine.UserAssets;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Signum.React.Filters;

namespace Signum.React.UserAssets
{
    [ValidateModelFilter]
    public class UserAssetController : ControllerBase
    {
        [HttpPost("api/userAssets/parseFilters")]
        public List<FilterResponse> ParseFilters([Required, FromBody]ParseFiltersRequest request)
        {
            var queryName = QueryLogic.ToQueryName(request.queryKey);
            var qd = QueryLogic.Queries.QueryDescription(queryName);
            var options = SubTokensOptions.CanAnyAll | SubTokensOptions.CanElement | (request.canAggregate ? SubTokensOptions.CanAggregate : 0);

            using (request.entity != null ? CurrentEntityConverter.SetCurrentEntity(request.entity.Retrieve()) : null)
            {
                var result = ToFilterList(request.filters, qd, options, 0).ToList();

                return result;
            }
        }

        public static List<FilterResponse> ToFilterList(IEnumerable<ParseFilterRequest> filters, QueryDescription qd, SubTokensOptions options, int indent)
        {
            return filters.GroupWhen(filter => filter.indentation == indent).Select(gr =>
            {
                if (!gr.Key.isGroup)
                {
                    if (gr.Count() != 0)
                        throw new InvalidOperationException("Unexpected childrens of condition");

                    var filter = gr.Key;

                    var token = QueryUtils.Parse(filter.tokenString, qd, options);

                    var value = FilterValueConverter.Parse(filter.valueString, token.Type, filter.operation.Value.IsList());

                    return (FilterResponse)new FilterConditionResponse
                    {
                        token = new QueryTokenTS(token, true),
                        operation = filter.operation.Value,
                        value = value
                    };
                }
                else
                {
                    var group = gr.Key;

                    var token = group.tokenString == null ? null : QueryUtils.Parse(group.tokenString, qd, options);

                    return (FilterResponse)new FilterGroupResponse
                    {
                        groupOperation = group.groupOperation.Value,
                        token = token == null ? null : new QueryTokenTS(token, true),
                        filters = ToFilterList(gr, qd, options, indent + 1).ToList()
                    };
                }
            }).ToList();
        }

        public class ParseFiltersRequest
        {
            public string queryKey;
            public bool canAggregate;
            public List<ParseFilterRequest> filters;
            public Lite<Entity> entity;
        }

        public class ParseFilterRequest
        {
            public bool isGroup;
            public string tokenString;
            public FilterOperation? operation;
            public string valueString;
            public FilterGroupOperation? groupOperation;
            public int indentation;
        }

        public class FilterResponse
        {
        }

        public class FilterConditionResponse : FilterResponse
        {
            public QueryTokenTS token;
            public FilterOperation operation;
            public object value;

        }

        public class FilterGroupResponse : FilterResponse
        {
            public FilterGroupOperation groupOperation;
            public QueryTokenTS token;
            public List<FilterResponse> filters;
        }

        [HttpPost("api/userAssets/export")]
        public FileStreamResult Export([Required, FromBody]Lite<IUserAssetEntity> lite)
        {
            var bytes = UserAssetsExporter.ToXml(lite.Retrieve());

            return FilesController.GetFileStreamResult(new MemoryStream(bytes), "{0}{1}.xml".FormatWith(lite.EntityType.Name, lite.Id));
        }

        [HttpPost("api/userAssets/importPreview")]
        public UserAssetPreviewModel ImportPreview([Required, FromBody]FileUpload file)
        {
            return UserAssetsImporter.Preview(file.content);
        }

        [HttpPost("api/userAssets/import")]
        public void Import([Required, FromBody]FileUploadWithModel file)
        {
            UserAssetsImporter.Import(file.file.content, file.model);
        }

        public class FileUpload
        {
            public string fileName;
            public byte[] content;
        }

        public class FileUploadWithModel
        {
            public FileUpload file;
            public UserAssetPreviewModel model;
        }
    }
}
