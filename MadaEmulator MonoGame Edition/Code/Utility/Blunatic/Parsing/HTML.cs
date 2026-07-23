using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Parsing
{
    public static class HTML
    {
        public class Element
        {
            public string Keyword;
            public Dictionary<string, string> Parameters;
            public List<Element> Elements;

            public Element()
            {
                Keyword = null;
                Parameters = new Dictionary<string, string>();
                Elements = new List<Element>();
            }

            public bool HasParameter(string parameterName)
            {
                return Parameters.ContainsKey(parameterName);
            }

            public bool TryGetParameter(string parameterName, out string parameterValue)
            {
                return Parameters.TryGetValue(parameterName, out parameterValue);
            }

            public bool HasElements()
            {
                return Elements.Count > 0;
            }

            public bool HasElementWithKeyword(string keyword)
            {
                foreach (Element e in Elements)
                {
                    if (e.Keyword == keyword) return true;
                }
                return false;
            }

            public bool HasElementWithParameter(string parameterName, string parameterValue)
            {
                foreach (Element e in Elements)
                {
                    if (e.HasParameter(parameterName) && e.Parameters[parameterName] == parameterValue) return true;
                }
                return false;
            }

            public bool TryGetElementWithKeyword(string keyword, out Element element)
            {
                foreach (Element e in Elements)
                {
                    if (e.Keyword == keyword)
                    {
                        element = e;
                        return true;
                    }
                }
                element = null;
                return false;
            }

            public bool TryGetElementWithParameter(string parameterName, string parameterValue, out Element element)
            {
                foreach (Element e in Elements)
                {
                    if (e.HasParameter(parameterName) && e.Parameters[parameterName] == parameterValue)
                    {
                        element = e;
                        return true;
                    }
                }
                element = null;
                return false;
            }

            public bool HasParameters()
            {
                return Elements.Count > 0;
            }
        }
        public static Element Parse(string path)
        {
            string text;

            {
                string read = File.ReadAllText(path).Replace("\r", "").Replace("\n", "");

                List<string> generalUseList = new List<string>();

                string[] inOrOutOfString = read.Split('\"');
                for (int i = 0; i < inOrOutOfString.Length; i += 2)
                {
                    string[] comments = inOrOutOfString[i].Split('#');
                    for (int j = 0; j < comments.Length; j += 2)
                    {
                        generalUseList.Add(comments[j]);
                    }
                    inOrOutOfString[i] = generalUseList.Aggregate(string.Empty, (s, x) => $"{s}{x}");
                    generalUseList.Clear();
                }

                text = string.Join('\"', inOrOutOfString);
            }

            int textIndex = 0;
            Element masterElement = new Element();

            char NextCharacter()
            {
                char characterToReturn = text[textIndex];
                textIndex++;
                return characterToReturn;
            }
            bool EndOfText()
            {
                return textIndex == text.Length;
            }
            void Ignore(params char[] charToIgnore)
            {
                while (charToIgnore.Contains(NextCharacter())) { }
                BackUp();
            }
            void Expect(params char[] charToExpect)
            {
                while (!charToExpect.Contains(NextCharacter())) { }
                BackUp();
            }
            string GetUntil(params char[] charToStopBefore)
            {
                string output = string.Empty;
                while (true)
                {
                    char nextChar = NextCharacter();
                    if (charToStopBefore.Contains(nextChar))
                    {
                        BackUp();
                        return output;
                    }
                    output += nextChar;
                }
            }
            void PassBy(params char[] charToCheck)
            {
                char nextChar = NextCharacter();
                if (!charToCheck.Contains(nextChar))
                {
                    throw new InvalidDataException($"Failed to parse {path}. Failed at {textIndex}, didn't expect {{{nextChar}}}");
                }
            }
            char ToCome()
            {
                return text[textIndex];
            }
            void BackUp()
            {
                textIndex--;
            }

            Element NextElement()
            {
                Element element = new Element();
                PassBy('<');
                Ignore(' ');
                element.Keyword = GetUntil(' ', '>', '/');
                Ignore(' ');
                bool gettingParameters = true;
                while (gettingParameters)
                {
                    switch (ToCome())
                    {
                        case '/':
                            NextCharacter();
                            NextCharacter();
                            return element;
                        case '>':
                            NextCharacter();
                            gettingParameters = false;
                            break;
                        default:
                            {
                                string parameterName = GetUntil('=');
                                PassBy('=');
                                PassBy('\"');
                                string parameterValue = GetUntil('\"');
                                PassBy('\"');
                                Ignore(' ');
                                element.Parameters.Add(parameterName, parameterValue);
                            }
                            break;
                    }
                }
                while (true)
                {
                    Ignore(' ');
                    PassBy('<');
                    if (ToCome() == '/')
                    {
                        Expect('>');
                        PassBy('>');
                        return element;
                    }
                    BackUp();
                    element.Elements.Add(NextElement());
                }
            }

            while (!EndOfText())
            {
                masterElement.Elements.Add(NextElement());
            }

            return masterElement;
        }
        public static string MakeElementReadable(Element element)
        {
            string output = string.Empty;
            string indent = string.Empty;
            void IndentUp()
            {
                indent += "   ";
            }
            void IndentDown()
            {
                indent = indent.Substring(3);
            }
            void WriteLine(string line)
            {
                output += $"{indent}{line}\n";
            }

            void ExplainElement(Element element)
            {
                if (element.Keyword == null)
                {
                    WriteLine($"MASTER ELEMENT");
                }
                else
                {
                    WriteLine($"# {element.Keyword} #");
                }
                if (element.Parameters.Count == 0)
                {
                    WriteLine($"Parameters: None");
                }
                else
                {
                    WriteLine($"Parameters:");
                    foreach (KeyValuePair<string, string> kvp in element.Parameters)
                    {
                        WriteLine($"{{{kvp.Key} : {kvp.Value}}}");
                    }
                }
                if (element.Elements.Count == 0)
                {
                    WriteLine($"Elements: None");
                }
                else
                {
                    WriteLine($"Elements:");
                    WriteLine($"{{");
                    IndentUp();

                    foreach (Element e in element.Elements)
                    {
                        ExplainElement(e);
                    }

                    IndentDown();
                    WriteLine($"}}");
                }
            }

            ExplainElement(element);

            return output;
        }
    }
}
