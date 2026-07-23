using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Parsing
{
    public class ArgumentReader
    {
        // Constants
        private const char STRING_OPEN = '(';
        private const char STRING_CLOSE = ')';

        // Classes
        public class Result
        {
            // Properties
            public string ID { get; private set; }
            public Dictionary<string, string> Arguments { get; private set; }

            // Constructors
            public Result(string id, Dictionary<string, string> arguments)
            {
                ID = id;
                Arguments = arguments;
            }

            // Methods
            public bool TryGetInteger(string id, int minimum, int maximum, out int value)
            {
                value = 0;
                if (!Arguments.TryGetValue(id, out string str)) return false;
                if (!int.TryParse(str, out int result)) return false;
                if (result < minimum || result > maximum) return false;
                value = result;
                return true;
            }
            public bool TryGetInteger(string id, out int value) 
            {
                bool result = TryGetInteger(id, int.MinValue, int.MaxValue, out int internalValue);
                value = internalValue;
                return result;
            }
            public bool TryGetString(string id, out string value)
            {
                value = null;
                if (!Arguments.TryGetValue(id, out string str)) return false;
                value = str;
                return true;
            }
        }

        // Enums
        private enum Token
        {
            Keyword,
            String,
            Integer,
        }

        // Fields
        private HTML.Element _tree;

        // Constructors
        public ArgumentReader(string commandTreePath)
        {
            _tree = HTML.Parse(commandTreePath);
        }

        // Methods
        private (Token, string)[] _getTokens(string[] args)
        {
            string fullString = args.Aggregate(string.Empty, (i, x) => $"{i} {x}").Substring(Math.Min(1, args.Length));
            return _getTokens(fullString);
        }
        private (Token, string)[] _getTokens(string argString)
        {
            List<(Token, string)> trueArgs = new List<(Token, string)>();

            for (int i = 0; i < argString.Length; i++)
            {
                if (argString[i] == STRING_OPEN)
                {
                    try
                    {
                        string token = string.Empty;
                        while (argString[++i] != STRING_CLOSE) token += argString[i];
                        if (i != argString.Length - 1 && argString[++i] != ' ') throw new FormatException("Invalid string syntax.");
                        trueArgs.Add((Token.String, token));
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new FormatException("Parenthesis never closed.");
                    }
                }
                else if (argString[i] >= '0' && argString[i] <= '9' || argString[i] == '-')
                {
                    string token = new string(argString[i], 1);
                    while (i < argString.Length - 1 && argString[++i] != ' ')
                    {
                        if (!(argString[i] >= '0' && argString[i] <= '9')) throw new FormatException("Invalid integer syntax.");
                        token += argString[i];
                    }
                    if (token == "-") throw new FormatException("Invalid integer syntax.");
                    trueArgs.Add((Token.Integer, token));
                }
                else if (argString[i] >= 'a' && argString[i] <= 'z' || argString[i] >= 'A' && argString[i] <= 'Z')
                {
                    string token = new string(argString[i], 1);
                    while (i < argString.Length - 1 && argString[++i] != ' ')
                    {
                        if (!(argString[i] >= 'a' && argString[i] <= 'z' || argString[i] >= 'A' && argString[i] <= 'Z')) throw new FormatException("Invalid keyword syntax.");
                        token += argString[i];
                    }
                    trueArgs.Add((Token.Keyword, token));
                }
                else if (argString[i] != ' ')
                {
                    throw new FormatException($"Unexpected character at position {i} ('{argString[i]}')");
                }
            }

            return trueArgs.ToArray();
        }
        private Result _getResult((Token, string)[] tokens)
        {
            HTML.Element element = _tree;

            Dictionary<string, string> resultArguments = new Dictionary<string, string>();

            for (int i = 0; i < tokens.Length; i++)
            {
                Token token = tokens[i].Item1;
                string value = tokens[i].Item2;

                switch (token)
                {
                    case Token.Keyword:
                        {
                            bool pathIdentified = false;

                            foreach (HTML.Element e in element.Elements)
                            {
                                if (e.Keyword != "keyword") continue;
                                if (!e.TryGetParameter("Value", out string eVal)) continue;
                                if (eVal != value) continue;

                                pathIdentified = true;
                                element = e;
                                break;
                            }

                            if (!pathIdentified)
                            {
                                throw new FormatException($"Unexpected keyword '{value}'.");
                            }
                        }
                        break;
                    case Token.String:
                        {
                            if (!element.TryGetElementWithKeyword("string", out HTML.Element e)) throw new FormatException($"Unexpected string.");
                            element = e;
                            if (!element.TryGetParameter("ID", out string resultArgName)) break;
                            resultArguments.Add(resultArgName, value);
                        }
                        break;
                    case Token.Integer:
                        {
                            if (!element.TryGetElementWithKeyword("int", out HTML.Element e)) throw new FormatException($"Unexpected int.");
                            element = e;
                            if (!element.TryGetParameter("ID", out string resultArgName)) break;
                            resultArguments.Add(resultArgName, value);
                        }
                        break;
                    default:
                        throw new NotImplementedException($"ArgReader does not currently support tokens of type {token}.");
                }
            }

            if (!element.TryGetElementWithKeyword("endpoint", out HTML.Element endpoint))
            {
                List<Token> expectedTokens = new List<Token>();

                if (element.HasElementWithKeyword("keyword")) expectedTokens.Add(Token.Keyword);
                if (element.HasElementWithKeyword("string")) expectedTokens.Add(Token.String);
                if (element.HasElementWithKeyword("int")) expectedTokens.Add(Token.Integer);

                if (expectedTokens.Count == 0) throw new NotImplementedException("Command Tree hit an unexpected dead end.");

                if (expectedTokens.Count == 1) throw new FormatException($"Expected token of type {expectedTokens[0]}.");
                throw new FormatException($"Expected token of one of the following types: {expectedTokens.Aggregate(string.Empty, (i, x) => $"{i}, {x}").Substring(2)}.");
            }

            if (!endpoint.TryGetParameter("Execute", out string execute)) throw new NotImplementedException("Command Tree hit an unexpected invalid endpoint.");

            foreach (HTML.Element e in endpoint.Elements)
            {
                if (e.Keyword != "parameter") continue;
                if (!e.TryGetParameter("ID", out string id)) throw new NotImplementedException("Command Tree hit an unexpected invalid endpoint parameter (missing 'ID').");
                if (!e.TryGetParameter("Value", out string value)) throw new NotImplementedException("Command Tree hit an unexpected invalid endpoint parameter (missing 'Value').");
                resultArguments.Add(id, value);
            }

            return new Result(execute, resultArguments);
        }
        public Result ReadCommand(string argString)
        {
            return _getResult(_getTokens(argString));
        }
        public Result ReadCommand(string[] args)
        {
            return _getResult(_getTokens(args));
        }

    }
}
