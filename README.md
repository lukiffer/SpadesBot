SpadesBot
=========

SpadesBot is a client/server/server application that facilitates a game of spades between four JSON-RPC endpoints. The app also has some basic playing AI to fill-in for missing players.

Building Your Your API
======================

Your API should expose the following JSON RPC endpoints. All endpoints should accept and return content-type `application/json`. Also note that if your endpoint throws an exception, your team will forfeit the game.

blind
-----
**POST /{gameId}/{playerId}/blind**
	
	{ 
		"round" : { 
			"player1_bid" : null,
			"player2_bid" : 0,
			"player3_bid" : 3,
			"player4_bid" : 5
		},
		"game" : {
			"dealer" : 1,
			"leader" : 2,
			"team1_score" : 0,
			"team2_score" : 0,
			"team3_score" : 0,
			"team4_score" : 0
		}
	}

This endpoint is called by the application when it is your turn to bid, giving you the opportunity to bid blind nil. It presents all other bids that have been made to that point.

- The game property contains an object with the score of the game, who is dealing and who is leading. 
- The round property contains an object indicating who has bid and what has been bid.

Your endpoint's response should indicate your intention to bid blind nil.
	
	{ "blind" : false }


deal
----

**POST /{gameId}/{playerId}/deal**

	{
		"hand" : [ "As", "Ks", "Qs", "Js", "10s", "9s", "8s", "7s", "6s", "5s", "Ah", "Ac", "Ad" ],
		"round" :  {
			"player1_bid" : null,
			"player2_bid" : 0,
			"player3_bid" : 3,
			"player4_bid" : 5
		},
		"game" : {
			"dealer" : 1,
			"leader" : 2,
			"team1_score" : 0,
			"team2_score" : 0,
			"team3_score" : 0,
			"team4_score" : 0
		}
	}

This endpoint is called subsequently to a call to `/{gameId}/{playerId}/blind`. It contains largely the same data as a blind call with the addition of your hand cards. Cards are presented as a string with rank followed by suit (`s` = spades, `h` = hearts, `d` = diamonds, `c` = clubs). 

If you have already bid blind nil, your endpoint's response to this call will be ignored. If you have not bid blind nil, you must respond with your bid. *You may not bid blind nil from this endpoint, and responses of `< 0` are treated as `0`.*

	{ "bid" : 4 }


bid
---

**POST /{gameId}/{playerId}/bid**

	{
		//...
		"player1_bid" : 4,
		"player2_bid" : 0,
		"player3_bid" : 3,
		"player4_bid" : 5
	}

This endpoint is called once all players have bid. It contains all four players bids. No response is required.


play
----

**POST /{gameId}/{playerId}/play**

	{
		"leader" : 2,
		"player1_card" : null,
		"player2_card" : "Kc",
		"player3_card" : "2c",
		"player4_card" : "4c",
	}

This endpoint is called when it is your turn to play a card. The state of the book is sent in the body. Players who have not yet played will have a `null` value in their respective card property. Respond with the card you wish to play.

	{
		"card" : "Ac"
	}

Card plays are validated against a standard set of rules (must follow suit, cannot prematurely break spades, must be in your hand, cannot have already been played, etc.) Any play of an illegal loses the game for that player's team.

book
----

**POST /{gameId}/{playerId}/book**

	{
		"leader" : 2,
		"player1_card" : "Ac",
		"player2_card" : "Kc",
		"player3_card" : "2c",
		"player4_card" : "4c",
		"winner" : 1
	}

This endpoint is called once all players have played a card in the book and the winner of the book has been decided. No response is required.



Running a Game
==============

Run the solution in the browser to simulate a game. You may specify endpoints manually in the UI or modify the spades.js `config` variable to specify default endpoints. You may specify any number (up to 4) competing endpoints or leave the config set to use the local A.I.
