var http = require("node:http");
var websocket = require("websocket");

var port = 9000;
var server = http.createServer((req, res) => { res.end("You Connected To Http Server"); console.log(new Date() + " - " + req.origin + " connected via HTTP"); });

server.listen(port, () => {
    console.log("HTTP Server started listening on port: " + port);
});


var websocketServer = new websocket.server({
    httpServer: server,
    autoAcceptConnections: false,
});

console.log("WebSocket Server started listening on port: " + port);

websocketServer.on('request', websocketHandler);

/**
 * 
 * @param {websocket.request} request
 */
function websocketHandler(request) {
    console.log(new Date() + " - " + request.origin + " connected via WebSocket");

    var connection = request.accept('', request.origin);

    connection.sendUTF("Hello, You connected to the WebSocket Server");
    connection.on('message', message => {
        if (message.type === 'utf8') {
            connection.sendUTF("Echo: " + message.utf8Data);
        }
    });
}

