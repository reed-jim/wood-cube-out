using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentElement : MonoBehaviour
{
    //DATA
    private string _name;
    private int _level;
    private Sprite _avatar;
    private Rank _rank;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _nameText.text = _name;
        }
    }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            _levelText.text = _level.ToString();
        }
    }

    public Sprite Avatar
    {
        get => _avatar;
        set
        {
            _avatar = value;
            _avatarUI.sprite = _avatar;
        }
    }

    public Rank Rank
    {
        get => _rank;
        set
        {
            _rank = value;
            _rankText.text = _rank.ToString();
        }
    }

    //VIEW
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _avatarUI;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
