using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace UIPerformanceAndRefactoring
{ /*

In one of our scenes, the UI shows live information about active gameplay entities. A junior
developer wrote the following code that:
● produces incorrect results;
●
causes performance issues;
● updates far too often.


 * public class CharactersView : MonoBehaviour
{
[SerializedField] private List<Transform> _characters;
void FixedUpdate()
{
float totalValue = 0f;
foreach (Transform characterTransform in _characters)
{
Character character =
characterTransform.gameObject.GetComponents<Character>();
totalValue += character != null ? character.Value : 0f;
}
string text = string.Format(
"Characters: {0} Avg value: {1}",
_characters.Length,
_characters.Length / totalValue
);
gameObject.GetComponent<Text>().text = text;
Debug.Log(text);
}
}

Your Goals
● Fix bugs and logical errors;
●
improve code quality and structure;
● optimize performance in practical and theoretical ways;
●
limit UI updates to once every X frames or a fixed interval;
● briefly explain why you made your changes.
You may rewrite the code entirely.
 */
    
    public class CharactersView : MonoBehaviour
    {
        private const string TextFormat = "Characters: {0} Avg value: {1}"; //Avg value: {1:0.00}
        
        [SerializeField] private float _refreshInterval = 0.2f;
        
        //[SerializeField] private List<Character> _characters;
        [SerializeField] private List<Transform> _charactersTms;
        [SerializeField] private TextMeshProUGUI _text;
        
        private float _timer;
    
        private Character[] _characters;

        private void Awake()
        {
            _characters = _charactersTms
                .Where(t => t != null)
                .Select(t => t.GetComponent<Character>())
                .Where(c => c != null)
                .ToArray();
        }
        
        private void OnEnable()
        {
            _timer = _refreshInterval;
        }

        //можно разбить на методы
        private void Update()
        {
            //обновление можно вынести в корутину, если update считается не хорошей практикой
            _timer += Time.deltaTime;

            if (_timer < _refreshInterval)
            {
                return; 
            }
            
            _timer = 0f;
            
            var totalValue = 0f;

            foreach (var character in _characters)
            {
                totalValue += character.Value;
            }
            
            var average = _characters.Length > 0 ? totalValue / _characters.Length : 0f;
            //сравнение c lastAverage и return
            
            var text = string.Format(TextFormat, _characters.Length, average);
            //_text.SetText("Characters: {0} Avg value: {1}", _characters.Length, average); //Avg value: {1:2} 
            //ZString если SetText не подходит
            
            _text.text = text;
            //? Debug.Log(text);
            // InternalLog
        }

        /*
        [Conditional("UNITY_EDITOR")]
        private void InternalLog(string text)
        {
            Debug.Log(text);
        }
        */
    }
}

/*
 * изменения внесены в случае, если такая вью действительно бы сущестовала в проекте
*/

