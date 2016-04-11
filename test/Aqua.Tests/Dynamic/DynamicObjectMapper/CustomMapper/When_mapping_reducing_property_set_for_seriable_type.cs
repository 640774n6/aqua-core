﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObjectMapper.CustomMapper
{
    using Aqua.Dynamic;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using Xunit.Fluent;

    public class When_mapping_reducing_property_set_for_seriable_type
    {
        [Serializable]
        class DataObject
        {
            public string PropertyOne { get; set; }

            public string PropertyTwo { get; set; }
        }

        class CustomMapper : DynamicObjectMapper
        {
            protected override IEnumerable<PropertyInfo> GetPropertiesForMapping(Type type)
            {
                if (type == typeof(DataObject))
                {
                    return new[] { type.GetProperty("PropertyTwo") };
                }

                return null;
            }
        }

        DynamicObject dynamicObject;

        public When_mapping_reducing_property_set_for_seriable_type()
        {
            var dynamicObjectMapper = new CustomMapper();

            dynamicObject = dynamicObjectMapper.MapObject(new DataObject
            {
                PropertyOne = "one",
                PropertyTwo = "two"
            });
        }

        [Fact]
        public void Dynamic_object_should_contain_property_two_only()
        {
            dynamicObject.MemberNames.Single().ShouldBe("PropertyTwo");

            dynamicObject["PropertyTwo"].ShouldBe("two");
        }
    }
}
