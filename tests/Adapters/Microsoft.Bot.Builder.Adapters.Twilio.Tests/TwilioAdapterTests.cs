﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Twilio.Rest.Api.V2010.Account;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_TwilioAPIWrapper()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));
            TwilioClientWrapper twilioClientWrapper = null;

            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter(twilioClientWrapper); });
        }

        [Fact]
        public void Constructor_With_TwilioAPIWrapper_Succeeds()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            Assert.NotNull(new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object));
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_ActivityType_Not_Message()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await twilioAdapter.SendActivitiesAsync(new TurnContext(twilioAdapter, activity), activities, default);
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpResponse()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, null, bot.Object, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_Bot()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Succeed_With_HttpBody()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\NoMediaPayload.txt"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));

            var authTokenString = "Test";
            var validationUrlString = new Uri("http://contoso.com");

            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(authTokenString));
            var builder = new StringBuilder(validationUrlString.ToString());

            var byteArray = Encoding.ASCII.GetBytes(payload);

            var values = new Dictionary<string, string>();

            var pairs = payload.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            bot.SetupAllProperties();
            httpResponse.Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));
        }

        [Fact]
        public async void ProcessAsync_Should_Succeed_With_Null_HttpBody()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\NoMediaPayload.txt"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));

            var authTokenString = "Test";
            var validationUrlString = new Uri("http://contoso.com");

            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(authTokenString));
            var builder = new StringBuilder(validationUrlString.ToString());

            var byteArray = Encoding.ASCII.GetBytes(payload);

            var values = new Dictionary<string, string>();

            var pairs = payload.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            bot.SetupAllProperties();
            httpResponse.Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));
        }

        [Fact]
        public async void UpdateActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var activity = new Activity();
            var turnContext = new TurnContext(twilioAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () => { await twilioAdapter.UpdateActivityAsync(turnContext, activity, default); });
        }

        [Fact]
        public async void DeleteActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var activity = new Activity();
            var turnContext = new TurnContext(twilioAdapter, activity);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<NotSupportedException>(async () => { await twilioAdapter.DeleteActivityAsync(turnContext, conversationReference, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_ConversationReference()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_Logic()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Succeed()
        {
            bool callbackInvoked = false;
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(options).Object);
            var conversationReference = new ConversationReference();

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await twilioAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Succeed()
        {
            // Setup mocked ITwilioAdapterOptions
            var options = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

            // Setup mocked Activity and get the message option
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            activity.Object.Conversation = new ConversationAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            var messageOption = TwilioHelper.ActivityToTwilio(activity.Object, "123456789");

            // Setup mocked Twilio API client
            const string resourceIdentifier = "Mocked Resource Identifier";
            var twilioApi = new Mock<TwilioClientWrapper>(options);
            twilioApi.Setup(x => x.SendMessage(It.IsAny<CreateMessageOptions>())).Returns(Task.FromResult(resourceIdentifier));

            // Create a new Twilio Adapter with the mocked classes and get the responses
            var twilioAdapter = new TwilioAdapter(twilioApi.Object);
            var resourceResponses = await twilioAdapter.SendActivitiesAsync(null, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            // Assert the result
            Assert.True(resourceResponses[0].Id == resourceIdentifier);
        }
    }
}
