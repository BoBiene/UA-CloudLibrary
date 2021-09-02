﻿using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UA_CloudLibrary.GraphQL.GraphTypes;
using UACloudLibrary;

namespace UA_CloudLibrary.GraphQL
{
    public class Mutations : ObjectGraphType<object>
    {
        public Mutations()
        {
            PostgresSQLDB postgres = new PostgresSQLDB();

            #region Specification Mutations

            FieldAsync<AddressSpaceType>(
                name: "SubmitAddressSpace",
                arguments: new QueryArguments(
                    new QueryArgument<AddressSpaceInput> { Name = "addressSpace"}
                    ),
                resolve: async context => {
                    // TODO: handle upload
                    Dictionary<string, object> space = (Dictionary<string, object>)context.Arguments["addressSpace"];
                    return null;    
                    }
                );

            FieldAsync<BooleanGraphType>(
                name: "AddAddressSpaceMetadata",
                arguments: new QueryArguments(
                    new QueryArgument<MetadataInput> { Name = "metaTag" },
                    new QueryArgument<IntGraphType> { Name = "nodesetID" }
                    ),
                resolve: async context => {
                    int nodesetID = (int)context.Arguments["nodesetID"];
                    Dictionary<string, object> metaTag = (Dictionary<string, object>)context.Arguments["metaTag"];
                    return await postgres.AddMetaDataToNodeSet(nodesetID, metaTag["name"].ToString(), metaTag["value"].ToString());
                    }
                );

            Field<AddressSpaceType>(
                name: "AddAddressSpaceRelationship",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name="targetAddressSpaceID"},
                    new QueryArgument<StringGraphType> { Name="sourceAddressSpaceID"},
                    new QueryArgument<StringGraphType> { Name="relationshipType"}
                    ),
                resolve: context =>
                {
                    string target = (string)context.Arguments["targetAddressSpaceID"];
                    string source = (string)context.Arguments["sourceAddressSpaceID"];
                    string type = (string)context.Arguments["relationshipType"];
                    
                    // TODO: Handle relationship creation
                    return null;
                    }
                );
            #endregion
        }
    }

    public class AddressSpaceInput : InputObjectGraphType<AddressSpace>
    {
        public AddressSpaceInput()
        {
            // Additional Fields as needed
            Field<AddressSpaceNodesetInput>("nodeset", resolve: context => context.Source.Nodeset.NodesetXml);
            Field<StringGraphType>("title", resolve: context => context.Source.Title);
            Field<StringGraphType>("version", resolve: context => context.Source.Version);
            Field<AddressSpaceLicenseType>("license", resolve: context => context.Source.License);
            Field<StringGraphType>("description", resolve: context => context.Source.Description);
        }
    }

    public class AddressSpaceCategoryInput : InputObjectGraphType<AddressSpaceCategory>
    {
        public AddressSpaceCategoryInput()
        {
            Field<StringGraphType>("id", resolve: context => context.Source.ID);
        }
    }

    public class AddressSpaceNodesetInput : InputObjectGraphType<AddressSpaceNodeset2>
    {
        public AddressSpaceNodesetInput()
        {
            Field<StringGraphType>("NodesetXml", resolve: context => context.Source.NodesetXml);
        }
    }

    public class MetadataInput : InputObjectGraphType
    {
        public MetadataInput()
        {
            Field<StringGraphType>("Name");
            Field<StringGraphType>("Value");
        }
    }
}