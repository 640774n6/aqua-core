﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Serialization.DynamicObject
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using Shouldly;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public abstract class When_serializing_dynamicobject_for_customcollections
    {
        public class DataContractSerializer : When_serializing_dynamicobject_for_customcollections
        {
            public DataContractSerializer() : base(DataContractSerializationHelper.Serialize) { }
        }

        public class JsonSerializer : When_serializing_dynamicobject_for_customcollections
        {
            public JsonSerializer() : base(JsonSerializationHelper.Serialize) { }
        }

        public class XmlSerializer : When_serializing_dynamicobject_for_customcollections
        {
            public XmlSerializer() : base(XmlSerializationHelper.Serialize) { }
        }

#if NET
        public class BinaryFormatter : When_serializing_dynamicobject_for_customcollections
        {
            public BinaryFormatter() : base(BinarySerializationHelper.Serialize) { }
        }
        
        public class NetDataContractSerializer : When_serializing_dynamicobject_for_customcollections
        {
            public NetDataContractSerializer() : base(NetDataContractSerializationHelper.Serialize) { }
        }
#endif

        public sealed class QueryableProxy<T> : IQueryable<T>
        {
            private readonly IQueryable<T> _source;
            
            public QueryableProxy(IEnumerable<T> source)
            {
                _source = source.AsQueryable();
            }

            public QueryableProxy(IQueryable<T> source)
            {
                _source = source;
            }

            public Expression Expression => _source.Expression;

            public Type ElementType => _source.ElementType;

            public IQueryProvider Provider => _source.Provider;

            public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
        }

        public sealed class EnumerableProxy<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;

            public EnumerableProxy(IEnumerable<T> source)
            {
                _source = source;
            }

            public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_source).GetEnumerator();
        }

        Func<DynamicObject, DynamicObject> _serialize;

        protected When_serializing_dynamicobject_for_customcollections(Func<DynamicObject, DynamicObject> serialize)
        {
            _serialize = serialize;
        }

        [Fact]
        public void Should_roundtrip_enumerable()
        {
            var enumerable = new EnumerableProxy<int?>(new int?[] { null, 1, 22, 333 });
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceEqual(enumerable);
        }

        [Fact]
        public void Should_roundtrip_queryable()
        {
            var enumerable = new QueryableProxy<int?>(new int?[] { null, 1, 22, 333 }.AsQueryable());
            var resurrected = Roundtrip(enumerable);
            resurrected.SequenceEqual(enumerable);
        }

        private T Roundtrip<T>(T obj)
        {
            var dynamicObject = new DynamicObjectMapper().MapObject(obj);
            var serializedDynamicObject = _serialize(dynamicObject);
            var resurrected = new DynamicObjectMapper().Map<T>(serializedDynamicObject);
            return resurrected;
        }
    }
}
