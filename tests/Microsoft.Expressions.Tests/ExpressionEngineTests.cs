﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        public static object[] Test(string input, object value) => new object[] { input, value };

        public static IEnumerable<object[]> Data => new[]
       {
            Test("1 + 2", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0),
            Test("(one + two) * bag.set.four", 12.0),
            Test("(hello + ' ' + world)", "hello world"),
            Test("items[2]", "two"),
            Test("bag.list[bag.index - 2]", "blue"),
            Test("bag.list[bag.index - 2] + 'more'", "bluemore"),
            Test("min(1.0, two) + max(one, 2.0)", 3.0),

            // operator as functions tests
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("sub(2, 1)", 1),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
            Test("greater(5, 2)", true),
            Test("greater(2, 2)", false),
            Test("greater(one, two)", false),
            Test("greaterOrEquals(one, one)", true),
            Test("greaterOrEquals(one, two)", false),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),




            Test("2^2", 4),
            Test("3^2^2", 81),
            Test("exp(2,2)", 4),

            Test("one > 0.5 && two < 2.5", true),
            Test("one > 0.5 || two < 1.5", true),

            Test("5%2", 1),
            Test("mod(5,2)", 1),

            Test("'string'&'builder'","stringbuilder"),
            Test("hello&world","helloworld"),

            Test("length(hello)",5),
            Test("length('hello')",5),

            Test("replace(hello, 'l', 'k')","hekko"),
            Test("replace(hello, 'L', 'k')","hello"),

            Test("replaceIgnoreCase(hello, 'L', 'k')","hekko"),

            Test("split(hello,'e')",new string[]{ "h","llo"}),

            Test("substring(hello, 0, 10)", "hello"),
            Test("substring(hello, 0, 3)", "hel"),

            Test("toLower('UpCase')", "upcase"),

            Test("toUpper('lowercase')", "LOWERCASE"),

            Test("trim(' hello ')", "hello"),

            Test("and(!one, !!one)", false),//false && true

            Test("and(!!one, !!one)", true),//true && true

            Test("equals(hello, 'hello')", true),
            Test("equals(bag.index, 3)", true),
            Test("equals(bag.index, 2)", false),

            Test("if(!one, 'r1', 'r2')", "r2"),//false
            Test("if(!!one, 'r1', 'r2')", "r1"),//true

            Test("or(!one, !!one)", true),//false && true
            Test("or(!one, !one)", false),//false && false

            Test("rand(1, 2)", 1),

            Test("sum(1, 2)", 3),
            Test("sum(one, two, 3)", 6.0),

            Test("average(1, 2)", 1.5),
            Test("average(one, two, 3)", 2.0),

            //Date and time function test
            //init dateTime: 2018-03-15T13:00:00Z
            Test("addDays(timestamp, 1)", "2018-03-16T13:00:00.0000000Z"),
            Test("addDays(timestamp, 1,'g')", "3/16/2018 1:00 PM"),
            Test("addDays(timestamp, 1,'MM-dd-yy')", "03-16-18"),
            Test("addHours(timestamp, 1)", "2018-03-15T14:00:00.0000000Z"),
            Test("addMinutes(timestamp, 1)", "2018-03-15T13:01:00.0000000Z"),
            Test("addSeconds(timestamp, 1)", "2018-03-15T13:00:01.0000000Z"),
            Test("dayOfMonth(timestamp)", 15),
            Test("dayOfWeek(timestamp)", 4),//Thursday
            Test("dayOfYear(timestamp)", 74),
            Test("month(timestamp)", 3),
            Test("date(timestamp)", "3/15/2018"),
            Test("year(timestamp)", 2018),
            Test("formatDateTime(timestamp)", "2018-03-15T13:00:00.0000000Z"),
            Test("formatDateTime(timestamp, 'g')", "3/15/2018 1:00 PM"),
            Test("formatDateTime(timestamp, 'MM-dd-yy')", "03-15-18"),
            Test("subtractFromTime(timestamp, 1, 'Day')", "2018-03-14T13:00:00.0000000Z"),
            Test("subtractFromTime(timestamp, 1, 'Day','g')", "3/14/2018 1:00 PM"),
            Test("dateReadBack(timestamp, addDays(timestamp, 1))", "Tomorrow"),
            Test("dateReadBack(timestamp, addDays(timestamp, 2))", "The day after tomorrow"),
            Test("dateReadBack(addDays(timestamp, 1),timestamp))", "Yesterday"),
            Test("dateReadBack(addDays(timestamp, 2),timestamp))", "The day before yesterday"),
            Test("getTimeOfDay('2018-03-15T00:00:00Z')", "midnight"),
            Test("getTimeOfDay('2018-03-15T08:00:00Z')", "morning"),
            Test("getTimeOfDay('2018-03-15T12:00:00Z')", "noon"),
            Test("getTimeOfDay('2018-03-15T13:00:00Z')", "afternoon"),
            Test("getTimeOfDay('2018-03-15T18:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T22:00:00Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T23:00:00Z')", "night"),

            //Conversion functions test
            Test("float('10.333')", 10.333f),


            Test("!one", false),
            Test("!!one", true),
            Test("!one || !!two", true),
            Test("not(one)", false),
            Test("not(not(one))", true),
            Test("not(0)", true),
            Test("exist(one)", true),
            Test("exist(xxx)", false),
            Test("exist(one.xxx)", false),

        };

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Parse(string input, object value)
        {
            var parsed = ExpressionEngine.Parse(input);
            Assert.IsNotNull(parsed);
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected)
        {
            var scope = new
            {
                one = 1.0,
                two = 2.0,
                hello = "hello",
                world = "world",
                bag = new
                {
                    three = 3.0,
                    set = new
                    {
                        four = 4.0,
                    },
                    index = 3,
                    list = new[] { "red", "blue" }
                },
                items = new string[] { "zero", "one", "two" },
                timestamp = "2018-03-15T13:00:00Z"
            };

            var parsed = ExpressionEngine.Parse(input);
            var actual = ExpressionEngine.Evaluate(parsed, scope);

            AssertObjectEquals(expected, actual);
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void TryEvaluate(string input, object expected)
        {
            var scope = new
            {
                one = 1.0,
                two = 2.0,
                hello = "hello",
                world = "world",
                bag = new
                {
                    three = 3.0,
                    set = new
                    {
                        four = 4.0,
                    },
                    index = 3,
                    list = new[] { "red", "blue" }
                },
                items = new string[] { "zero", "one", "two" },
                timestamp = "2018-03-15T13:00:00Z"
            };

            object actual = null;
            var success = ExpressionEngine.TryEvaluate(input, scope, out actual);
            Assert.IsTrue(success);

            AssertObjectEquals(expected, actual);
        }

        public static IEnumerable<object[]> JsonData => new[]
        {
            //Test("exist(one)", true),
            Test("items[0] == 'item1'", true),
            // Test("'item1' == items[0]", false), // false because string.CompareTo(JValue) will get exception
        };

        [DataTestMethod]
        [DynamicData(nameof(JsonData))]
        public void EvaluateJSON(string input, object expected)
        {
            var scope = JsonConvert.DeserializeObject(@"{
                            'one': 1,
                            'two': 2,
                            'hello': 'hello',
            
                            'items': ['item1', 'item2', 'item3']
                        }");

            var parsed = ExpressionEngine.Parse(input);
            var actual = ExpressionEngine.Evaluate(parsed, scope);
            AssertObjectEquals(expected, actual);
        }

        private void AssertObjectEquals(object expected, object actual)
        {
            // Compare two arrays
            if (expected is object[] expectedArray
                && actual is object[] actualArray)
            {
                Assert.AreEqual(expectedArray.Length, actualArray.Length);
                for (var i = 0; i < expectedArray.Length; i++)
                {
                    Assert.AreEqual(expectedArray[i], actualArray[i]);
                }
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
