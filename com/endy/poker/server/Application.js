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

var players = [];
var currentPlayer = 0;

/**
 * 
 * @param {websocket.request} request
 */
function websocketHandler(request) {
    console.log(new Date() + " - " + request.origin + " connected via WebSocket");

    if (request.origin == undefined) {
        console.log(new Date() + " - " + "Rejected connection due to undefined origin.")
        request.reject(401);
        return;

    } else if (!request.origin.startsWith("PokerClient")) {
        console.log(new Date() + " - " + "Rejected connection due to non-whitelisted origin.")
        request.reject(403);
        return;
    }

    var connection = request.accept('', request.origin);

    connection.on('message', message => {
        if (message.type === 'utf8') {
            console.log(new Date() + " - Message received: " + message.utf8Data);

            var event = JSON.parse(message.utf8Data);

            if (event.Type == "") {
                // Turn should advance if the player folds or bets

            }

            // Currently just echoes the data back
//            connection.sendUTF(message.utf8Data);
        }
    });
}

