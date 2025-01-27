﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using ProtoBuf;
using System.IO;
using System.Linq;
using ProtoBuf.Meta;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.DataSource;

namespace QuantConnect.DataLibrary.Tests
{
    [TestFixture]
    public class CBOETests
    {
        [Test]
        public void EndTimeShiftedOneDayForward()
        {
            var date = new DateTime(2020, 5, 21);
            var cboe = new CBOE();
            var cboeData = "2020-05-21,1,1,1,1";
            var symbol = new Symbol(SecurityIdentifier.GenerateBase(typeof(CBOE), "VIX", QuantConnect.Market.USA), "VIX");
            var actual = cboe.Reader(new SubscriptionDataConfig(
                typeof(CBOE),
                symbol,
                Resolution.Daily,
                QuantConnect.TimeZones.Utc,
                QuantConnect.TimeZones.Utc,
                false,
                false,
                false,
                true), cboeData, date, false);

            Assert.AreEqual(date, actual.Time);
            Assert.AreEqual(date.AddDays(1), actual.EndTime);
        }

        [Test]
        public void JsonRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();
            var serialized = JsonConvert.SerializeObject(expected);
            var result = JsonConvert.DeserializeObject(serialized, type);

            AssertAreEqual(expected, result);
        }

        [Test, Ignore("ProtoBuf not supported for this data type")]
        public void ProtobufRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();

            RuntimeTypeModel.Default[typeof(BaseData)].AddSubType(2000, type);

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, expected);

                stream.Position = 0;

                var result = Serializer.Deserialize(type, stream);

                AssertAreEqual(expected, result, filterByCustomAttributes: true);
            }
        }

        [Test]
        public void Clone()
        {
            var expected = CreateNewInstance();
            var result = expected.Clone();

            AssertAreEqual(expected, result);
        }

        private void AssertAreEqual(object expected, object result, bool filterByCustomAttributes = false)
        {
            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // we skip Symbol which isn't protobuffed
                if (filterByCustomAttributes && propertyInfo.CustomAttributes.Count() != 0)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(result));
                }
            }
            foreach (var fieldInfo in expected.GetType().GetFields())
            {
                Assert.AreEqual(fieldInfo.GetValue(expected), fieldInfo.GetValue(result));
            }
        }

        private BaseData CreateNewInstance()
        {
            return new CBOE
            {
                Symbol = Symbol.Empty,
                Time = DateTime.Today,
                DataType = MarketDataType.Base,
                Period = TimeSpan.FromDays(1),

                Open = 1m,
                High = 2m,
                Low = 1m,
                Close = 1.5m
            };
        }
    }
}
