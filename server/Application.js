var http = require("node:http");
var websocket = require("websocket");

var port = 9000;
var server = http.createServer((req, res) => { res.end("You Connected To HTTP Server, There's Nothing Interesting Here. Connect Over Websocket instead."); console.log(new Date() + " - " + req.origin + " connected via HTTP"); });

server.listen(port, () => {
    console.log("Server started on port: " + port);
});


var websocketServer = new websocket.server({
    httpServer: server,
    autoAcceptConnections: false,
});

websocketServer.on('request', websocketHandler);

var players = [];
var websockets = {};
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
    }

    var connection = request.accept('', request.origin);
    if (thisPlayer == undefined || players.includes(thisPlayer)) {
        console.log(new Date() + " - " + request.origin + " rejected connection due to duplicate or non-existent name.")
        connection.sendUTF(JSON.stringify({ Type: "close", Data: { Code: 1201, Reason: "You have the same name as another player." } }));
        connection.close();
        return;
    }

    console.log(new Date() + " - " + request.origin + " connected via WebSocket");
    players.push(thisPlayer);
    connection.sendUTF(JSON.stringify({
        Type: "connect_data",
        Data: {
            Index: players.indexOf(thisPlayer),

        }

    }))

    websockets[thisPlayer] = connection;

    connection.on('message', message => {
        if (message.type === 'utf8') {
            console.log(new Date() + " - Message received from (" + thisPlayer + ", Current: " + players[currentPlayer] + "): " + message.utf8Data);

            // If this isn't the player who's turn it is, then just ignore them.
            if (players[currentPlayer] != thisPlayer) return;

            var event = JSON.parse(message.utf8Data);

            if (/bet|fold/.test(event.Type)) {
                // Turn should advance if the player folds or bets
                currentPlayer = (currentPlayer + 1) % players.length;
                
                // Relay bet/fold event to all players, then the fact that the turn has advanced to a certain player.
                websocketServer.broadcastUTF(message.utf8Data);
                websocketServer.broadcastUTF(JSON.stringify({ Type: "turn_advanced", Data: { Player: players[currentPlayer] } }));
            } else {
                // Relay event to all clients
                websocketServer.broadcastUTF(message.utf8Data);
            }

        }
    });

    connection.on("close", () => {
        console.log(new Date() + " - Connection with " + thisPlayer + " has closed.");

        // Remove the player from the memory
        websockets[thisPlayer] = undefined;
        players.splice(players.indexOf(thisPlayer), 1);

        // Skip the player if it was their turn
        currentPlayer = players.length == 0 ? 0 : currentPlayer % players.length;

        if (player.length == 0) {
            // If there are no players in the lobby,
            // then there's no point in sending out any
            // events
            return;
        }

        // Tell all clients that someone left to make them delete that player from their board.
        websocketServer.broadcastUTF(JSON.stringify({
            Type: "player_left",
            Data: {
                Player: thisPlayer
            }
        }));

        // Advance the turn to the next player if it was their turn when they left
        websocketServer.broadcastUTF(JSON.stringify({ Type: "turn_advanced", Data: { Player: players[currentPlayer] } }));
    });
}