﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Fluent;

    public class When_created_based_on_object_with_guid_properties
    {
        class ClassWithGuidProperties
        {
            public Guid Guid1 { get; set; }
            public Guid? Guid2 { get; set; }
            public Guid? Guid3 { get; set; }
        }

        ClassWithGuidProperties source;
        DynamicObject dynamicObject;

        public When_created_based_on_object_with_guid_properties()
        {
            source = new ClassWithGuidProperties
            {
                Guid1 = Guid.NewGuid(),
                Guid2 = Guid.Empty,
                Guid3 = null,
            };

            dynamicObject = new DynamicObject(source);
        }

        [Fact]
        public void Type_property_should_be_set_to_custom_class()
        {
            dynamicObject.Type.Type.ShouldBe(typeof(ClassWithGuidProperties));
        }

        [Fact]
        public void Should_have_three_members_stored()
        {
            dynamicObject.MemberCount.ShouldBe(3);
        }

        [Fact]
        public void Member_name_should_be_name_of_property()
        {
            dynamicObject.MemberNames.Any(name => name == "Guid1").ShouldBeTrue();
            dynamicObject.MemberNames.Any(name => name == "Guid2").ShouldBeTrue();
            dynamicObject.MemberNames.Any(name => name == "Guid3").ShouldBeTrue();
        }

        [Fact]
        public void Dynamic_guid_properties_should_be_of_type_guid()
        {
            dynamicObject["Guid1"].ShouldBeOfType<Guid>();
            dynamicObject["Guid2"].ShouldBeOfType<Guid>();
        }

        [Fact]
        public void Dynamic_guid_properties_should_contain_expected_guid_values()
        {
            dynamicObject["Guid1"].ShouldBe(source.Guid1);
            dynamicObject["Guid2"].ShouldBe(source.Guid2);
            dynamicObject["Guid3"].ShouldBeNull();
        }
    }
}
