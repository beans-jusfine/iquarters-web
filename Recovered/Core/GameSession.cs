// Adapted from supplied Assets/Scripts/GameManagerScript.cs.
// Rules retained; Unity presentation dependencies removed; state is per session.
using System;
using System.Collections;
namespace IQuarters.Core;

[Serializable]
public class GameSession
{
	public ArrayList playerData = new ArrayList();

	public bool IsPractice { get; }

	public event Action SecretRoundUnlocked;

	public GameSession(int players = 1, bool practice = false)
	{
		if (players < 1 || players > 4) throw new ArgumentOutOfRangeException(nameof(players));
		totPlayers = players;
		IsPractice = practice;
		Awake();
		Start();
	}

	public bool replayFlag;

	public float shotMagnitude = 6f;

	public float defaultShotAngle = 50f;

	public float shotAngle = 50f;

	public float shotMagX;

	public float shotVelZ;

	public int shotsPerRound = 3;

	public int curShotThisRound;

	public int curMadeShotsThisRound;

	public int numRounds = 12;

	public int curRound;

	public int curPlayer;

	public int totPlayers = 1;

	public int flagNone;

	public int flagPlayerChange = 1;

	public int flagRoundChange = 2;

	public int flagPlayerGameOver = 4;

	public int flagGameOver = 8;

	public int flagOutOfShotsReason = 16;

	public int inputTypeShake;

	public int inputTypeFlick = 1;

	public int inputTypeMax = 2;

	public bool secretRoundUnlocked;

	public int secretRoundNumber = 12;

	public int roundBeforeSecretRound = 8;

	public int roundAfterSecretRound = 9;





	public float iPadBackEndScaleX = 1.11f;

	public int[] userSecretCodes;

	public int numUserSecretCodes;

	public int maxUserSecretCodes = 8;

	public int[] unlockSecretCodes;

	public int numUnlockSecretCodes;

	public float iPhoneTallWidth = 320f;

	public float iPhoneTallHeight = 480f;

	public float iPadTallWidth = 768f;

	public float iPadTallHeight = 1024f;

	public void Start()
	{
		userSecretCodes = new int[maxUserSecretCodes];
		ResetUserSecretCodes();
		numUnlockSecretCodes = 6;
		unlockSecretCodes = new int[] { 1, 2, 3, 1, 2, 0 };
		secretRoundUnlocked = false;
	}

	public void StoreSecretRoundUnlockCode(int code)
	{
		if (IsPractice || secretRoundUnlocked || totPlayers != 1 ||
			curRound != roundBeforeSecretRound || curMadeShotsThisRound != shotsPerRound - 1 ||
			numUserSecretCodes >= numUnlockSecretCodes)
		{
			return;
		}

		userSecretCodes[numUserSecretCodes++] = code;
		bool matches = true;
		for (int index = 0; index < numUserSecretCodes; index++)
		{
			if (userSecretCodes[index] != unlockSecretCodes[index])
			{
				matches = false;
				break;
			}
		}

		if (!matches)
		{
			ResetUserSecretCodes();
		}
		else if (numUserSecretCodes == numUnlockSecretCodes)
		{
			SecretRoundUnlocked?.Invoke();
			secretRoundUnlocked = true;
		}
	}

	public void ResetUserSecretCodes()
	{
		numUserSecretCodes = 0;
		for (int index = 0; index < maxUserSecretCodes; index++)
		{
			userSecretCodes[index] = -1;
		}
	}

	public bool IsLastActivePlayer(int player)
	{
		if (player < 0 || player >= totPlayers)
		{
			return false;
		}
		int lastActive = 0;
		for (int index = 0; index < totPlayers; index++)
		{
			if (PlayerAt(index).shotsLeft > 0)
			{
				lastActive = index;
			}
		}
		return player == lastActive;
	}

	public void ResetShotAngleToDefault()
	{
		shotAngle = defaultShotAngle;
	}

	public int GetCurrentRoundScore()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).roundScore : 0;
	}

	public int SetCurrentRoundScore(int roundScore)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).roundScore = roundScore;
		return 0;
	}

	public int UpdateCurRicochet(int curRicochet)
	{
		if (IsValidPlayer(curPlayer) && PlayerAt(curPlayer).maxRicochet < curRicochet)
		{
			PlayerAt(curPlayer).maxRicochet = curRicochet;
		}
		return 0;
	}

	public void AddRoundScore(int scoreAddition)
	{
		SetCurrentRoundScore(GetCurrentRoundScore() + scoreAddition);
	}

	public void AddScore(int scoreAddition)
	{
		AddRoundScore(scoreAddition);
		SetCurrentScore(GetCurrentScore() + scoreAddition);
		if (scoreAddition == 0)
		{
			SetCurrentTotalStreak(0);
			SetCurrentRoundStreak(0);
		}
	}

	public void IncrementCurrentStreak()
	{
		SetCurrentTotalStreak(GetCurrentTotalStreak() + 1);
		SetCurrentRoundStreak(GetCurrentRoundStreak() + 1);
	}

	public void SetCurrentStreak(int score)
	{
		SetCurrentTotalStreak(score == 0 ? 0 : GetCurrentTotalStreak() + 1);
		SetCurrentRoundStreak(score == 0 ? 0 : GetCurrentRoundStreak() + 1);
	}

	public void ClearAllScores()
	{
		ClearScores();
		ClearTotalStreaks();
		ClearRoundStreaks();
		ClearAllGameOverFlags();
	}

	public void ResetGameManagerVars()
	{
		curPlayer = 0;
		curShotThisRound = 0;
		curMadeShotsThisRound = 0;
		curRound = 0;
		ClearAllScores();
	}

	public void UpdateRound()
	{
		ClearRoundStreaks();
		ClearAllPlayedThisRoundFlags();
		SetCurrentRoundScore(0);
		if (secretRoundUnlocked && curRound == roundBeforeSecretRound)
		{
			curRound = secretRoundNumber;
		}
		else if (curRound == secretRoundNumber)
		{
			curRound = roundAfterSecretRound;
		}
		else
		{
			curRound++;
			if (curRound >= numRounds) curRound = 0;
		}
	}

	public bool GetNextCurrentPlayer()
	{
		int previousPlayer = curPlayer;
		for (int player = previousPlayer; player < totPlayers; player++)
		{
			if (!GetPlayedThisRound(player) && GetPlayerShotsLeft(player) > 0)
			{
				curPlayer = player;
				return true;
			}
		}
		for (int player = 0; player < previousPlayer; player++)
		{
			if (!GetPlayedThisRound(player) && GetPlayerShotsLeft(player) > 0)
			{
				curPlayer = player;
				return true;
			}
		}

		if (curRound < numRounds - 1)
		{
			int playedWithShots = 0;
			int outOfShots = 0;
			int firstPlayedWithShots = -1;
			for (int player = 0; player < totPlayers; player++)
			{
				if (GetPlayedThisRound(player) && GetPlayerShotsLeft(player) > 0)
				{
					playedWithShots++;
					if (firstPlayedWithShots == -1) firstPlayedWithShots = player;
				}
				else if (GetPlayerShotsLeft(player) == 0)
				{
					outOfShots++;
				}
			}
			if (playedWithShots + outOfShots == totPlayers && playedWithShots > 0)
			{
				curPlayer = firstPlayedWithShots;
				return false;
			}
		}
		curPlayer = 0;
		return false;
	}

	public int UpdateShotCount(int score)
	{
		int result = flagNone;
		int previousRound = curRound;
		int previousPlayer = curPlayer;
		if (score == 0)
		{
			if (DecrementCurrentShotsLeft() == 1) SetCurrentPlayedThisRound(true);
		}
		else
		{
			curMadeShotsThisRound++;
		}
		curShotThisRound++;

		if (curMadeShotsThisRound >= shotsPerRound || GetCurrentShotsLeft() < 1)
		{
			SetCurrentPlayedThisRound(true);
			SetCurrentRoundScore(0);
			if ((curRound >= numRounds - 1 && curRound != secretRoundNumber) || GetCurrentShotsLeft() < 1)
			{
				result |= flagPlayerGameOver;
				SetCurrentGameOverFlag(true);
				if (AllPlayersGameOver()) result |= flagGameOver;
				if (GetCurrentShotsLeft() < 1) result |= flagOutOfShotsReason;
			}
			curShotThisRound = 0;
			curMadeShotsThisRound = 0;
			if (!GetNextCurrentPlayer()) UpdateRound();
		}

		if (curPlayer != previousPlayer) result |= flagPlayerChange;
		if (curRound != previousRound) result |= flagRoundChange;
		return result;
	}

	public void ClearAllGameOverFlags()
	{
		foreach (PlayerInfoClass player in playerData) player.gameOver = false;
	}

	public int SetCurrentGameOverFlag(bool playedF)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).gameOver = playedF;
		return 0;
	}

	public bool AllPlayersGameOver()
	{
		foreach (PlayerInfoClass player in playerData)
		{
			if (!player.gameOver) return false;
		}
		return true;
	}

	public void ClearAllPlayedThisRoundFlags()
	{
		foreach (PlayerInfoClass player in playerData) player.playedThisRound = false;
	}

	public int SetCurrentPlayedThisRound(bool playedF)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).playedThisRound = playedF;
		return 0;
	}

	public bool GetCurrentPlayedThisRound()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).playedThisRound : true;
	}

	public bool GetPlayedThisRound(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).playedThisRound : true;
	}

	public void SetAllShotsLeft(int shotsLeft)
	{
		foreach (PlayerInfoClass player in playerData) player.shotsLeft = shotsLeft;
	}

	public bool AllPlayersOutOfShots()
	{
		foreach (PlayerInfoClass player in playerData)
		{
			if (player.shotsLeft > 0) return false;
		}
		return true;
	}

	public int SetCurrentInputType(int input)
	{
		if (IsValidPlayer(curPlayer) && input >= 0 && input < inputTypeMax)
		{
			PlayerAt(curPlayer).inputType = input;
		}
		return 0;
	}

	public int GetCurrentInputType()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).inputType : 0;
	}

	public int SetCurrentShotsLeft(int shotsLeft)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).shotsLeft = shotsLeft;
		return 0;
	}

	public int GetCurrentShotsLeft(int curPlayer)
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).shotsLeft : 0;
	}

	public int GetPlayerShotsLeft(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).shotsLeft : 0;
	}

	public int DecrementCurrentShotsLeft()
	{
		if (!IsValidPlayer(curPlayer)) return 0;
		PlayerInfoClass player = PlayerAt(curPlayer);
		player.shotsLeft--;
		if (player.shotsLeft < 1)
		{
			player.shotsLeft = 0;
			return 1;
		}
		return 0;
	}

	public int GetCurrentShotsLeft()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).shotsLeft : 0;
	}

	public void ClearScores()
	{
		foreach (PlayerInfoClass player in playerData)
		{
			player.score = 0;
			player.roundScore = 0;
		}
	}

	public int AddPlayerScore(int addedScore, int playerNumber)
	{
		if (IsValidPlayer(playerNumber)) PlayerAt(playerNumber).score += addedScore;
		return 0;
	}

	public int SetCurrentScore(int score)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).score = score;
		return 0;
	}

	public int GetCurrentScore()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).score : 0;
	}

	public int GetPlayerScore(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).score : 0;
	}

	public void ClearRoundStreaks()
	{
		foreach (PlayerInfoClass player in playerData) player.roundStreak = 0;
	}

	public int SetCurrentRoundStreak(int newStreak)
	{
		if (IsValidPlayer(curPlayer)) PlayerAt(curPlayer).roundStreak = newStreak;
		return 0;
	}

	public int GetPlayerMaxStreak(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).maxStreak : 0;
	}

	public int GetPlayerMaxRicochet(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).maxRicochet : 0;
	}

	public int GetCurrentRoundStreak()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).roundStreak : 0;
	}

	public int SetCurrentTotalStreak(int newStreak)
	{
		if (IsValidPlayer(curPlayer))
		{
			PlayerInfoClass player = PlayerAt(curPlayer);
			player.streak = newStreak;
			if (player.maxStreak < newStreak) player.maxStreak = newStreak;
		}
		return 0;
	}

	public void ClearTotalStreaks()
	{
		foreach (PlayerInfoClass player in playerData)
		{
			player.streak = 0;
			player.maxStreak = 0;
		}
	}

	public int GetCurrentTotalStreak()
	{
		return IsValidPlayer(curPlayer) ? PlayerAt(curPlayer).streak : 0;
	}

	public bool SetName(string playerName, int playerNumber)
	{
		if (!IsValidPlayer(playerNumber)) return true;
		PlayerAt(playerNumber).playerName = playerName;
		return false;
	}

	public string GetName(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).playerName : "error";
	}

	public bool GetNameEnteredFlag(int playerNumber)
	{
		return IsValidPlayer(playerNumber) ? PlayerAt(playerNumber).nameEnteredFlag : true;
	}

	public bool SetNameEnteredFlag(int playerNumber)
	{
		if (!IsValidPlayer(playerNumber)) return true;
		PlayerAt(playerNumber).nameEnteredFlag = true;
		return false;
	}

	public void ResetPlayerData()
	{
		playerData.Clear();
		for (int index = 0; index < totPlayers; index++)
		{
			playerData.Add(new PlayerInfoClass());
		}
	}

	public void Awake()
	{
		replayFlag = false;
		// Player count is supplied by the host.
		ResetPlayerData();
		ClearAllScores();
		ResetGameManagerVars();
	}

	public void OnApplicationQuit()
	{
	}

	private bool IsValidPlayer(int playerNumber)
	{
		return playerNumber >= 0 && playerNumber < playerData.Count;
	}

	private PlayerInfoClass PlayerAt(int playerNumber)
	{
		return (PlayerInfoClass)playerData[playerNumber];
	}

	public void Main()
	{
	}
}
