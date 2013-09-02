var Spades = function () {
    var spades = this;

    this.config = {
        player1_url: 'http://10.211.55.3/',
        player2_url: 'http://10.211.55.3/',
        player3_url: 'http://10.211.55.3/',
        player4_url: 'http://10.211.55.3/',
        limit: 500
    };

    this.start = function () {

        // Initialize SignalR hub.
        var hub = $.connection.spadesHub;

        // Map hub functions.
        hub.client.log = function (message) {
            console.log(message);
        };

        hub.client.roundStart = function () {
            spades.events.onRoundStart();
        };

        hub.client.bid = function (player, bid) {
            spades.events.onBid(player, bid);
        };

        hub.client.play = function (player, card) {
            spades.events.onPlay(player, card);
        };

        hub.client.bookComplete = function (book, round, game) {
            spades.events.onBookComplete(book, round, game);
        };

        hub.client.roundComplete = function (round, game) {
            spades.events.onRoundComplete(round, game);
        };

        hub.client.gameComplete = function (game) {
            spades.events.onGameComplete(game);
        };

        // Start the hub.
        $.connection.hub.start().done(function () {
            hub.server.start(spades.config);
        });

        this.events.onGameStart();
    };

    this.stop = function () {
        $.connection.hub.stop();
    };

    this.events = {
        onGameStart: function () { },
        onRoundStart: function () { },
        onBid: function () { },
        onPlay: function () { },
        onBookComplete: function () { },
        onRoundComplete: function () { },
        onGameComplete: function () { }
    };
};