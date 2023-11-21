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
    var thisPlayer = request.httpRequest.headers.player;

    if (request.origin == undefined) {
        console.log(new Date() + " - " + request.origin + " rejected connection due to undefined origin.")
        request.reject(401);
        return;

    } else if (!request.origin.startsWith("PokerClient")) {
        console.log(new Date() + " - " + request.origin + " rejected connection due to non-whitelisted origin.")
        request.reject(403);
        return;
    } else if (thisPlayer == undefined || players.includes(thisPlayer)) {
        console.log(new Date() + " - " + request.origin + " rejected connection due to duplicate or non-existent name.")
        request.reject();
        return;
    }

    var connection = request.accept('', request.origin);
    console.log(new Date() + " - " + request.origin + " connected via WebSocket");
    players.push(thisPlayer);

    connection.on('message', message => {
        if (message.type === 'utf8') {
            console.log(new Date() + " - Message received from (" + thisPlayer + ", Current: " + players[currentPlayer] + "): " + message.utf8Data);

            // If this isn't the player who's turn it is, then just ignore them.
            if (players[currentPlayer] != thisPlayer) return;

            var event = JSON.parse(message.utf8Data);

            if (/bet|fold/.test(event.Type)) {
                // Turn should advance if the player folds or bets
                currentPlayer = (currentPlayer + 1) % players.length;
            }

            // Relay event to all clients
            websocketServer.broadcastUTF(message.utf8Data);
        }
    });

    connection.on("close", () => {
        console.log(new Date() + " - Connection with " + thisPlayer + " has closed.");

        players.splice(players.indexOf(thisPlayer), 1);
        currentPlayer = players.length == 0 ? 0 : currentPlayer % players.length;
    });
}

