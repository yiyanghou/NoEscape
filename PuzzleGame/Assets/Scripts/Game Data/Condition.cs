using PuzzleGame.EventSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltEvents;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// A very simple boolean condition parser to build to-game logic
    /// For expressions involving more than 1 boolean, must use parenthesis around the expression
    /// </summary>
    [CreateAssetMenu(menuName = "PuzzleGame/Condition")]
    public class Condition : ScriptableObject
    {
        [Serializable]
        public class Token
        {
            public enum TokenType
            {
                OPEN_PAREN,
                CLOSE_PAREN,

                //operators
                NOT,
                AND,
                OR,

                //literals
                VARIABLE,
                FUNCTION
            }

            public TokenType type;
            public BoolVariable variable;
            public PersistentCall boolPredicate;

            public override string ToString()
            {
                switch(type)
                {
                    case TokenType.OPEN_PAREN:
                        return "(";
                    case TokenType.CLOSE_PAREN:
                        return ")";
                    case TokenType.AND:
                        return "&&";
                    case TokenType.OR:
                        return "||";
                    case TokenType.NOT:
                        return "!";
                    case TokenType.VARIABLE:
                        return variable == null ? "none" : variable.name;
                    case TokenType.FUNCTION:
                        return boolPredicate == null ? "none" : boolPredicate.Method == null ? "none" : $"{boolPredicate.Method.Name}()";
                    default:
                        return "{unkown token}";
                }
            }
        }

        //serialized
        [SerializeField] bool _oneTime;
        [SerializeField] List<Token> _expression;

        //runtime
        Stack<bool> _evaluationStack;
        List<Token> _postfixExpr;
        bool _isValid;

        void Invalidate()
        {
            Debug.LogError($"invalid expression in condition {name}");
            _isValid = false;
        }

        void CheckExpression()
        {
            for (int i = 0; i < _expression.Count; i++)
            {
                if(_expression[i].type == Token.TokenType.FUNCTION)
                {
                    var meth = _expression[i].boolPredicate.Method;
                    if(meth.GetReturnType() != typeof(bool))
                    {
                        Invalidate();
                        return;
                    }
                }
            }
        }

        List<Token> InfixToPostfix()
        {
            List<Token> ret = new List<Token>();

            //single boolean or function
            if (_expression.Count == 1)
            {
                if(_expression[0].type != Token.TokenType.VARIABLE && _expression[0].type != Token.TokenType.FUNCTION)
                {
                    Invalidate();
                    return null;
                }

                ret.Add(_expression[0]);
                return ret;
            }

            Stack<Token> stack = new Stack<Token>();

            for(int i=0; i<_expression.Count; i++)
            {
                Token t = _expression[i];

                switch (t.type)
                {
                    case Token.TokenType.VARIABLE:
                    case Token.TokenType.FUNCTION:
                        ret.Add(t);
                        break;
                    case Token.TokenType.AND:
                    case Token.TokenType.OR:
                    case Token.TokenType.NOT:
                    case Token.TokenType.OPEN_PAREN:
                        stack.Push(t);
                        break;
                    case Token.TokenType.CLOSE_PAREN:
                        while (stack.Peek().type != Token.TokenType.OPEN_PAREN)
                        {
                            ret.Add(stack.Pop());

                            if (stack.Count == 0)
                            {
                                Invalidate();
                                return null;
                            }
                        }
                        stack.Pop();

                        while (stack.Count > 0 && stack.Peek().type == Token.TokenType.NOT)
                        {
                            ret.Add(stack.Pop());
                        }
                        break;
                    default:
                        Invalidate();
                        return null;
                }
            }

            while (stack.Count > 0)
            {
                if(stack.Peek().type == Token.TokenType.OPEN_PAREN)
                {
                    Invalidate();
                    return null;
                }

                ret.Add(stack.Pop());
            }

            return ret;
        }

        public List<Token> expression { get { return _expression; } }

        private void OnEnable()
        {
            Messenger.AddPersistentListener(M_EventType.ON_GAME_RESTART, Init);
            Init();
        }

        private void Init()
        {
            _postfixExpr = null;
            _isValid = true;
            _evaluationStack = new Stack<bool>();
        }

        public bool Evaluate()
        {
            if (_expression == null || _expression.Count == 0)
            {
                Debug.LogWarning($"Condition {name} does not have an expression to evaluate!");
                Invalidate();
            }

            if (!_isValid)
            {
                return false;
            }

            //init
            if (_postfixExpr == null || _postfixExpr.Count == 0)
            {
                CheckExpression();
                _postfixExpr = InfixToPostfix();

                if (_isValid)
                {
                    //Debug.Log($"Infix expression for {name}:" + string.Join(" ", _expression));
                    //Debug.Log($"Postfix expression for {name}:" + string.Join(" ", _postfixExpr));
                }
                else
                {
                    return false;
                }
            }

            for (int i=0; i<_postfixExpr.Count; i++)
            {
                if(_postfixExpr[i].type == Token.TokenType.FUNCTION)
                {
                    _evaluationStack.Push((bool)_postfixExpr[i].boolPredicate.Invoke());
                }
                else if(_postfixExpr[i].type == Token.TokenType.VARIABLE)
                {
                    _evaluationStack.Push(_postfixExpr[i].variable.val);
                }
                else if (_postfixExpr[i].type == Token.TokenType.NOT)
                {
                    bool operand = _evaluationStack.Pop();
                    _evaluationStack.Push(!operand);
                }
                else if (_postfixExpr[i].type == Token.TokenType.AND)
                {
                    bool operand1 = _evaluationStack.Pop(), operand2 = _evaluationStack.Pop();
                    _evaluationStack.Push(operand1 && operand2);
                }
                else if (_postfixExpr[i].type == Token.TokenType.OR)
                {
                    bool operand1 = _evaluationStack.Pop(), operand2 = _evaluationStack.Pop();
                    _evaluationStack.Push(operand1 || operand2);
                }
                else
                {
                    Debug.Assert(false, $"invalid token type encountered in evaluation of condition {name}, type = {_postfixExpr[i].type}");
                }
            }

            Debug.Assert(_evaluationStack.Count == 1);
            bool result = _evaluationStack.Pop();

            //if this is a one-time condition, i.e. not valid after first truth
            if(result && _oneTime)
            {
                _isValid = false;
            }

            return result;
        }
    }
}
