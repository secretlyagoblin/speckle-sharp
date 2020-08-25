﻿using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Sentry.Protocol;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;

namespace Speckle.Core.Api
{
  public partial class Client
  {
    public string ServerUrl { get => Account.serverInfo.url; }

    public string ApiToken { get => Account.token; }

    public string AccountId { get; set; }

    [JsonIgnore]
    public Account Account { get; set; }

    HttpClient HttpClient { get; set; }

    GraphQLHttpClient GQLClient { get; set; }

    public Client() { }

    public Client(Account account)
    {
      if (account == null)
        Log.CaptureAndThrow(new SpeckleException($"Provided account is null."));

      Account = account;
      AccountId = account.id;

      HttpClient = new HttpClient();
      HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {account.token}");

      GQLClient = new GraphQLHttpClient(
        new GraphQLHttpClientOptions
        {
          EndPoint = new Uri(new Uri(account.serverInfo.url), "/graphql"),
          UseWebSocketForQueriesAndMutations = false,
          OnWebsocketConnected = OnWebSocketConnect,
        },
        new NewtonsoftJsonSerializer(),
        HttpClient);

      var c = new ClientWebSocket();
      c.Options.UseDefaultCredentials = true;
      c.Options.SetRequestHeader("Authorization", $"Bearer {account.token}");
      GQLClient.Options.ConfigureWebsocketOptions(c.Options);

      GQLClient.InitializeWebsocketConnection();

    }

    public Task OnWebSocketConnect(GraphQLHttpClient client)
    {
      //logger.LogInformation("Main websocket is open");

      return Task.CompletedTask;
    }


  }
}
