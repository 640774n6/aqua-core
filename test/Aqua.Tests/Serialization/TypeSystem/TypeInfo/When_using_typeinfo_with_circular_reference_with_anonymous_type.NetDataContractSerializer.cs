﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET

namespace Aqua.Tests.Serialization.TypeSystem.TypeInfo
{
    partial class When_using_typeinfo_with_circular_reference_with_anonymous_type
    {
        public class NetDataContractSerializer : When_using_typeinfo_with_circular_reference_with_anonymous_type
        {
            public NetDataContractSerializer()
                : base(NetDataContractSerializationHelper.Serialize)
            {
            }
        }
    }
}

#endif