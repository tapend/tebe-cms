﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataGraphType : ObjectGraphType<NamedContentData>
    {
        public ContentDataGraphType(Schema schema, IGraphQLContext qlContext)
        {
            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            Name = $"{schema.Name.ToPascalCase()}DataDto";

            foreach (var field in schema.Fields.Where(x => !x.IsHidden))
            {
                var fieldInfo = qlContext.GetGraphType(field);

                if (fieldInfo.ResolveType != null)
                {
                    var fieldName = field.RawProperties.Label.WithFallback(field.Name);

                    var fieldGraphType = new ObjectGraphType
                    {
                        Name = $"{schema.Name.ToPascalCase()}Data{field.Name.ToPascalCase()}Dto"
                    };

                    var partition = qlContext.ResolvePartition(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionItem.Key,
                            Resolver = fieldInfo.Resolver,
                            ResolvedType = fieldInfo.ResolveType,
                            Description = field.RawProperties.Hints
                        });
                    }

                    fieldGraphType.Description = $"The structure of the {fieldName} of a {schemaName} content type.";

                    var fieldResolver = new FuncFieldResolver<NamedContentData, ContentFieldData>(c => c.Source.GetOrDefault(field.Name));

                    AddField(new FieldType
                    {
                        Name = field.Name.ToCamelCase(),
                        Resolver = fieldResolver,
                        ResolvedType = fieldGraphType
                    });
                }
            }

            Description = $"The structure of a {schemaName} content type.";
        }
    }
}