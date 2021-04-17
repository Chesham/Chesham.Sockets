# Chesham.Sockets

This C# project aims to provide a light, event-driven, ease for use and high performance TCP sockets server/client architecture.

Please visit the sources under `Chesham.Sockets.Test` for complete examples.

## APIs

### Server End

```cs
// Create a server
var server = new SocketServer();

// For booking connections
var connections = new List<SocketConnection>();

// Subscribe events
server.OnEvent += (_, e_) =>
{
  if (e_ is OnSocketAccepted)
  {
    // Accepting sockets
    var e = e_ as OnSocketAccepted;
    var connection = e.connection;
    connection.OnEvent += (_, e_) =>
    {
      if (e_ is OnSocketReceived)
      {
        // Receiving data from the accepted socket
      }
    }

    // Book connection
    connections.Add(connection);

    // Set isAccept property to true for accepting the socket
    e.isAccept = true;
  }

  // Other events goes here
};

// Start listening and accepting sockets
server.Listen(endPoint);

// Send data to specific connection
await connections.ElementAtOrDefault(0)?.SendAsync(payloadBytes);
```

### Client End

```cs
// Create a client(connection)
var socketConnection = new SocketConnection();

// Subscribe events
socketConnection.OnEvent += (_, e) =>
{
  if (e is OnSocketReceived)
  {
    // Receive data from event
  }
}

// Connect to remote socket asynchronously
await socketConnection.ConnectAsync(endPoint);

// Send data to remote asynchronously
await socketConnection.SendAsync(payloadBytes);
```