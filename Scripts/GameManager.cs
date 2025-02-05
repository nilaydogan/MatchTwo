using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchTwo.Client.Data;
using MatchTwo.Client.Feedback;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        #region Fields
        
        public TextAsset Level;
        
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private TopUI _topUI;
        [SerializeField] private AudioData _audioData;
        private InputController _inputController;
        private LevelData _currentLevelData;
        private BoardManager _boardManager;
        private AudioManager _audioManager;
        
        #endregion
        
        public bool HasGameEnded => _currentLevelData != null && (_currentLevelData.spawnerData.movesLeft <= 0 || _currentLevelData.levelTarget.targets.All(t => t.IsCompleted));

        #region Unity Methods
        private void Awake()
        {
            var visualsData = BoardItemVisualsData.Instance;
        }

        private void Start()
        {
            _currentLevelData = GetCurrentLevelData();
            _boardManager = GetComponent<BoardManager>();
            _inputController = GetComponent<InputController>();
            _audioManager = GetComponent<AudioManager>();
            _audioManager.Setup(_audioData);
            InitializeDependencies();
        }
        
        #endregion

        #region Public Methods
        
        public Transform GetTargetTransform(BoardItemType type)
        {
            return _topUI.GetGoalItemTransform(type);
        }
        
        public async Task AfterMatch()
        {
            _currentLevelData.spawnerData.movesLeft--;
            
            if (_currentLevelData.levelTarget.targets.All(t => t.IsCompleted))
            {
                await StopGame(hasWon: true);
            }
            else if (_currentLevelData.spawnerData.movesLeft <= 0) // we were out of moves
            {
                await StopGame(hasWon: false);
            }

            async Task StopGame(bool hasWon)
            {
                //hasWon to show LevelEndScreen Win or Lose
                _inputController.EnableControls(false);
                _currentLevelData = GetCurrentLevelData();
                await ResetGameManager(_currentLevelData);
            }
        }

        public void OnBoardItemCollected(BoardItem boardItem)
        {
            if (_currentLevelData.levelTarget.TargetTypes.ContainsKey(boardItem.Type))
            {
                var currentTarget = _currentLevelData.levelTarget.TargetTypes[boardItem.Type];
                if (currentTarget.amount > 0)
                {
                    _audioManager.Play(AudioName.CubeCollectSfx);
                    
                    var newAmount = currentTarget.amount - 1;
                    currentTarget.amount = Mathf.Max(newAmount, 0);
                    _topUI.UpdateTopUI(_currentLevelData.spawnerData.movesLeft, _currentLevelData.levelTarget.TargetTypes);
                }
            }
        }
        
        public void PlayBoardItemCollectedSound(BoardItem boardItem)
        {
            if(boardItem.Definition == BoardItemDefinition.Regular)
                _audioManager.Play(AudioName.CubeExplodeSfx);
        }
        
        #endregion

        #region Private Methods
        
        private async Task ResetGameManager(LevelData levelData)
        {
            _currentLevelData = levelData;

            _topUI.UpdateTopUI(_currentLevelData.spawnerData.movesLeft, _currentLevelData.levelTarget.TargetTypes);
            await _gameBoard.ResetBoard(_currentLevelData.boardData.Board);

            _boardManager.Initialize(this, _gameBoard);

            _inputController.EnableControls(true);
        }
        
        private void InitializeDependencies()
        {
            _gameBoard.Initialize(_currentLevelData.boardData.Board);
            _boardManager.Initialize(this, _gameBoard);
            _inputController.Initialize(_boardManager);
            
            _topUI.Initialize(_currentLevelData.spawnerData.movesLeft, _currentLevelData.levelTarget.TargetTypes);

            _inputController.EnableControls(true);
        }
        
        private LevelData GetCurrentLevelData()
        {
            _currentLevelData = JsonUtility.FromJson<LevelData>(Level.text);
            
            _currentLevelData.spawnerData.movesLeft = _currentLevelData.spawnerData.moveLimit;

            return _currentLevelData;
        }
        
        #endregion
    }

    #region Additional Classes
    
    [Serializable]
    public class LevelData
    {
        public BoardData boardData;
        public LevelTarget levelTarget;
        public SpawnerData spawnerData;
    }

    [Serializable]
    public class BoardData
    {
        public int boardWidth;
        public int boardHeight;
        public string[] board;

        private string[,] _board;

        public string[,] Board
        {
            get
            {
                if (_board == null)
                {
                    if (board == null || board.Length < boardWidth * boardHeight)
                    {
                        _board = null;
                        return _board;
                    }

                    _board = new string[boardWidth, boardHeight];
                    for (var y = boardHeight - 1; y >= 0; y--)
                    {
                        int trueY = boardHeight - 1 - y;
                        for (var x = 0; x < boardWidth; x++)
                            _board[x, trueY] = board[y * boardWidth + x];
                    }
                }

                return _board;
            }
        }
    }

    [Serializable]
    public class LevelTarget
    {
        public List<Target> targets;

        private Dictionary<BoardItemType, Target> _targetTypes;

        public Dictionary<BoardItemType, Target> TargetTypes
        {
            get
            {
                _targetTypes ??= targets.ToDictionary(x => x.Type, x => x);
                return _targetTypes;
            }
        }
    }

    [Serializable]
    public class Target
    {
        public string type;
        public int amount;

        private BoardItemType? _typeEnum;

        public BoardItemType Type
        {
            get
            {
                if (!_typeEnum.HasValue)
                {
                    if (Enum.TryParse(type, true, out BoardItemType parsedType))
                        _typeEnum = parsedType;
                    else
                        throw new ArgumentException($"type: {type} is not part of the enum values of BoardItemLayerType");
                }
                return _typeEnum.Value;
            }
        }

        public bool IsCompleted => amount == 0;
    }

    [Serializable]
    public class SpawnerData
    {
        public int moveLimit;
        [NonSerialized] public int movesLeft;
    }
    
    #endregion
}