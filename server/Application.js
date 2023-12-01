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
var playersInRound = [];
var websockets = {};
var currentPlayer = 0;
var potBalance = 0;
var turnsUntilRaisedBlinds = 8;
var blinds = { small: 1, big: 2 };
var dealerIndex = 0;
var boardCards = [];
var playerBets = [];

/**
 * 
 * @param {websocket.request} request
 */
function websocketHandler(request) {
    var playerName = request.httpRequest.headers.player;

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
    if (playerName == undefined || players.some(p => p.name == playerName)) {
        console.log(new Date() + " - " + request.origin + " rejected connection due to duplicate or non-existent name.")
        connection.sendUTF(JSON.stringify({ Type: "close", Data: { Reason: "You have the same name as another player." } }));
        connection.close();
        return;
    }

    var thisPlayer = new Player(undefined, playerName);

    console.log(new Date() + " - " + request.origin + " connected via WebSocket");
    players.push(thisPlayer);
    websockets[thisPlayer.name] = connection;

    if (players.length == 1) {
        reset();
    }

    websocketServer.broadcastUTF(JSON.stringify({ Type: "player_joined", Data: { Player: thisPlayer.name, Index: thisPlayer.index } }))

    connection.sendUTF(JSON.stringify({
        Type: "connect_data",
        Data: {
            Index: thisPlayer.index,
            Cards: thisPlayer.cards,
            Players: players.filter(p => p.name != thisPlayer.name)
        }
    }));

    connection.on('message', message => {
        if (message.type === 'utf8') {
            console.log(new Date() + " - Message received from (" + thisPlayer.name + ", Current: " + players[currentPlayer].name + "): " + message.utf8Data);

            // If this isn't the player who's turn it is, then just ignore them.
            if (players[currentPlayer].name != thisPlayer.name) return;

            var event = JSON.parse(message.utf8Data);

            if (/bet|fold/.test(event.Type)) {
                if (event.Type == "bet") {
                    if (event.BetAmount + playerBets[currentPlayer] < Math.max(playerBets) && event.BetAmount != players[currentPlayer].balance) {
                        // Player hasn't bet enough to qualify to play more and they have more money which is available to be bet.
                        // Otherwise if this is an all in bet, then let it through
                        return;
                    }

                    if (players[currentPlayer].balance < event.BetAmount) {
                        // Player doesn't have enough money and
                        // will go into debt if this goes through.
                        return;
                    }

                    // Move money from the player to the pot
                    players[currentPlayer].balance -= event.BetAmount;
                    playerBets[currentPlayer] += event.BetAmount;
                    potBalance += event.BetAmount;
                } else if (event.Type == "fold") {
                    // Remove this player from the round
                    playersInRound.splice(playersInRound.indexOf(thisPlayer.name), 1);

                    // If there is only one other player, then move all money from the pot to that person.
                    if (playersInRound.length == 1) {
                        // This was one of the two last players in the match,
                        // which means that the other player who is left is the winner.
                        websocketServer.broadcastUTF(JSON.stringify({
                            Type: "win",
                            Data: {
                                Player: playersInRound[0].name,
                                WinAmount: potBalance
                            }
                        }));

                        playersInRound[0].balance += potBalance;

                        reset();
                    }
                }

                // Turn should advance if the player folds or bets
                currentPlayer = (currentPlayer + 1) % players.length;
                
                // Relay bet/fold event to all players, then the fact that the turn has advanced to a certain player.
                websocketServer.broadcastUTF(message.utf8Data);
                websocketServer.broadcastUTF(JSON.stringify({ Type: "turn_advanced", Data: { Player: players[currentPlayer].name } }));
            } else {
                // Relay event to all clients
                websocketServer.broadcastUTF(message.utf8Data);
            }

        }
    });

    connection.on("close", () => {
        console.log(new Date() + " - Connection with " + thisPlayer.name + " has closed.");

        // Remove the player from the memory
        websockets[thisPlayer.name] = undefined;
        playersInRound.splice(playersInRound.indexOf(thisPlayer.name), 1);
        players.splice(thisPlayer.index, 1);

        // Tell all clients that someone left to make them delete that player from their board.
        websocketServer.broadcastUTF(JSON.stringify({
            Type: "player_left",
            Data: {
                Player: thisPlayer
            }
        }));

        if (playersInRound.length == 1) {
            // *** The way this is coded, it may be subject to a race condition if multiple clients leave at the same time.

            // This was one of the two last players in the match,
            // which means that the other player who is left is the winner.
            websocketServer.broadcastUTF(JSON.stringify({
                Type: "win",
                Data: {
                    Player: playersInRound[0].name,
                    WinAmount: potBalance
                }
            }));

            playersInRound[0].balance += potBalance;

            reset();

            return;
        }

        // Skip the player if it was their turn
        currentPlayer = players.length == 0 ? 0 : currentPlayer % players.length;

        if (players.length == 0) {
            // If there are no players in the lobby,
            // then there's no point in sending out any
            // events
            return;
        }

        // Advance the turn to the next player if it was their turn when they left
        websocketServer.broadcastUTF(JSON.stringify({ Type: "turn_advanced", Data: { Player: players[currentPlayer].name } }));
    });
}

class Player {
    balance;
    name;
    cards = [];

    get index() {
        return players.findIndex(p => p.name == this.name);
    }

    constructor(balance = 5_000, name) {
        this.balance = balance;
        this.name = name;

        if (name == undefined) throw "Name cannot be undefined";
    }
}

var cards = [];
function getCard() {
    var index = Math.floor(Math.random() * cards.length);
    return cards.splice(index, 1)[0];
}

function reset() {
    // Shuffle all cards back into the deck
    cards = [];
    for (var suit = 0; suit < 4; suit++) {
        // Diamonds, Clubs, Spades, Hearts
        var suits = "DCSH";
        for (var value = 1; value < 13; value++) {
            cards.push(suits[suit] + "" + value);
        }
    }

    // All client who are connected at this moment are considered to be in the round.
    playersInRound = [ ...players ];

    // Reset the pot 
    potBalance = 0;

    // Dealer is a person, small blind is then the person left of that, and big blind is the person left to them. (index: +1, +2)
    dealerIndex = players.length == 0 ? 0 : (dealerIndex + 1) % players.length;

    // Raise blinds every once in a while
    turnsUntilRaisedBlinds -= 1;
    if (turnsUntilRaisedBlinds == 0) {
        turnsUntilRaisedBlinds = 8;

        blinds.small *= 2;
        blinds.big   *= 2;
    }

    // Take money from blinds
    websocketServer.broadcastUTF(JSON.stringify({ Type: "bet", Data: { Player: players[(dealerIndex + 1) % players.length].name, BetAmount: blinds.small } }));
    websocketServer.broadcastUTF(JSON.stringify({ Type: "bet", Data: { Player: players[(dealerIndex + 2) % players.length].name, BetAmount: blinds.big   } }));

    // The starting player is the player to the left of big blind (+3) in the first round.
    // After that, it's the small blind (+1) who is the starting player.
    currentPlayer = players.length == 0 ? 0 : (boardCards.length == 0 ? dealerIndex + 3 : dealerIndex + 1) % players.length;

    if (players.length == 0) return;

    for (var i = 0; i < players.length; i++) {
        // Deal two new cards to each player
        players[i].cards = [ getCard(), getCard() ];

        // Send to each player that it's a new round, and notify them of which their new cards are.
        websockets[players[i].name].sendUTF(JSON.stringify({
            Type: "new_round",
            Data: {
                StartPlayer: players[currentPlayer].name,
                Cards: players[i].cards
            }
        }));
    }
}