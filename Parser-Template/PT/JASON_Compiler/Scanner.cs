using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum Token_Class
{ //Reserved words
  Variable,Val,Fun,Class,If,Else,While,For,In,Return,True,False, Null,New,Print,Input,Break,Continue,Init, 
  //Data Types 
    Integer, Float, Boolean, Character, Array, Void, String,
    //Literals & Identifiers 
    Identifier, IntegerLiteral, FloatLiteral, StringLiteral, CharLiteral,
    //Operators 
    EqualOp, LessThanOrEqualOp, GreaterThanOrEqualOp, NotEqualOp, PlusOp, MinusOp, DivideOp,
    MultiplyOp, ModOp, AssignOp, LessThanOp, GreaterThanOp, RangeOp,
    //Delimiters 
    Colon, Comma, LBrace, RBrace, LParenthesis, RParenthesis, LBracket, RBracket, Dot,
}
namespace JASON_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
        public Token()
        {
        }
        public Token(string lex, Token_Class token_type)
        {
            this.lex = lex;
            this.token_type = token_type;
        }  
    } 

public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();
        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("var", Token_Class.Variable);
            ReservedWords.Add("val", Token_Class.Val);
            ReservedWords.Add("fun", Token_Class.Fun);
            ReservedWords.Add("class", Token_Class.Class);
            ReservedWords.Add("while", Token_Class.While);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("for", Token_Class.For);
            ReservedWords.Add("in", Token_Class.In);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("true", Token_Class.True);
            ReservedWords.Add("false", Token_Class.False);
            ReservedWords.Add("null", Token_Class.Null);
            ReservedWords.Add("new", Token_Class.New);
            ReservedWords.Add("print", Token_Class.Print);
            ReservedWords.Add("input", Token_Class.Input);
            ReservedWords.Add("break", Token_Class.Break);
            ReservedWords.Add("continue", Token_Class.Continue);
            ReservedWords.Add("init", Token_Class.Init);
            ReservedWords.Add("Int", Token_Class.Integer);
            ReservedWords.Add("Float", Token_Class.Float);
            ReservedWords.Add("String", Token_Class.String);
            ReservedWords.Add("Bool", Token_Class.Boolean);
            ReservedWords.Add("Char", Token_Class.Character);
            ReservedWords.Add("Array", Token_Class.Array);
            ReservedWords.Add("Void", Token_Class.Void);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParenthesis);
            Operators.Add(")", Token_Class.RParenthesis);
            Operators.Add("[", Token_Class.LBracket);
            Operators.Add("]", Token_Class.RBracket);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
            Operators.Add("==", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("!=", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("%", Token_Class.ModOp);
            Operators.Add("=", Token_Class.AssignOp);
            Operators.Add("<=", Token_Class.LessThanOrEqualOp);
            Operators.Add(">=", Token_Class.GreaterThanOrEqualOp);
            Operators.Add("..", Token_Class.RangeOp);
        }
        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();
            for (int i = 0; i < SourceCode.Length; i++)
            {
                char CurrentChar = SourceCode[i];
                // 1. Whitespace Handling (includes spaces, tabs, newlines) 
                if (char.IsWhiteSpace(CurrentChar))
                    continue;
                // 2. Comments or Division Operator ('/') 
                if (CurrentChar == '/')
                {
                    // Single-line comment // 
                    if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '/')
                    {
                        int j = i + 2;
                        while (j < SourceCode.Length && SourceCode[j] != '\n')
                            j++;
                        i = j - 1;
                        continue;
                    }
                    // Multi-line comment /* ... */ 
                    else if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                    {
                        int j = i + 2;
                        bool isClosed = false;

                        while (j + 1 < SourceCode.Length)
                        {
                            if (SourceCode[j] == '*' && SourceCode[j + 1] == '/')
                            {
                                j += 2;
                                isClosed = true;
                                break;
                            }
                            j++;
                        }

                        if (isClosed)
                        {
                            i = j - 1;
                        }
                        else
                        {
                            Errors.Error_List.Add("Lexical Error: Unclosed multi-line comment '/*'");
                            i = SourceCode.Length - 1;
                        }
                        continue;
                    }
                    // Single division operator '/' 
                    else
                    {
                        Tokens.Add(new Token("/", Token_Class.DivideOp));
                        continue;
                    }
                }
                // 3. Identifiers & Reserved Words 
                if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    int j = i;
                    string currentLexeme = "";
                    while (j < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[j]) || SourceCode[j] == '_'))
                    {
                        currentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    if (ReservedWords.ContainsKey(currentLexeme))
                    {
                        Tokens.Add(new Token(currentLexeme, ReservedWords[currentLexeme]));
                    }
                    else
                    {
                        Tokens.Add(new Token(currentLexeme, Token_Class.Identifier));
                    }
                    continue;
                }
                // 4. String Literals ("...") 
                if (CurrentChar == '"')
                {
                    int j = i + 1;
                    string currentLexeme = "\"";
                    bool isClosed = false;

                    while (j < SourceCode.Length)
                    {
                        char nextChar = SourceCode[j];

                        if (nextChar == '\n' || nextChar == '\r')
                            break;
                        else if (nextChar == '"')
                        {
                            currentLexeme += "\"";
                            j++;
                            isClosed = true;
                            break;
                        }
                        else
                        {
                            currentLexeme += nextChar;
                            j++;
                        }
                    }

                    i = j - 1;

                    if (isClosed)
                    {
                        Tokens.Add(new Token(currentLexeme, Token_Class.StringLiteral));
                    }
                    else
                    {
                        Errors.Error_List.Add("Lexical Error: Unclosed string literal: " + currentLexeme);
                    }
                    continue;
                }

                // 5. Character Literals ('c') 
                if (CurrentChar == '\'')
                {
                    int j = i + 1;
                    string currentLexeme = "'";
                    bool isClosed = false;

                    while (j < SourceCode.Length)
                    {
                        char nextChar = SourceCode[j];

                        if (nextChar == '\n' || nextChar == '\r')
                            break;
                        else if (nextChar == '\'')
                        {
                            currentLexeme += "'";
                            j++;
                            isClosed = true;
                            break;
                        }
                        else
                        {
                            currentLexeme += nextChar;
                            j++;
                        }
                    }

                    i = j - 1;

                    if (isClosed && currentLexeme.Length == 3)
                    {
                        Tokens.Add(new Token(currentLexeme, Token_Class.CharLiteral));
                    }
                    else
                    {
                        Errors.Error_List.Add("Lexical Error: Invalid character literal: " + currentLexeme);
                    }
                    continue;
                }

                // 6. Numeric Literals (Int & Float + Malformed Trap States) 
                if (char.IsDigit(CurrentChar))
                {
                    int j = i;
                    string numLexeme = "";
                    int dotCount = 0;

                    while (j < SourceCode.Length)
                    {
                        char nextChar = SourceCode[j];

                        if (char.IsDigit(nextChar))
                        {
                            numLexeme += nextChar;
                            j++;
                        }
                        else if (nextChar == '.' && !(j + 1 < SourceCode.Length && SourceCode[j + 1] == '.'))
                        {
                            dotCount++;
                            numLexeme += nextChar;
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    // Trap check: Number immediately followed by letters/identifier (e.g. 12x) 
                    if (j < SourceCode.Length && (char.IsLetter(SourceCode[j]) || SourceCode[j] == '_'))
                    {
                        while (j < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[j]) || SourceCode[j] == '_'))
                        {
                            numLexeme += SourceCode[j];
                            j++;
                        }
                        Errors.Error_List.Add("Lexical Error: Invalid identifier starting with digits '" + numLexeme + "'");
                        i = j - 1;
                        continue;
                    }
                    i = j - 1;
                    if (dotCount == 0)
                    {
                        Tokens.Add(new Token(numLexeme, Token_Class.IntegerLiteral));
                    }
                    else if (dotCount == 1 && !numLexeme.EndsWith("."))
                    {
                        Tokens.Add(new Token(numLexeme, Token_Class.FloatLiteral));
                    }
                    else
                    {
                        Errors.Error_List.Add("Lexical Error: Malformed number literal '" + numLexeme + "'");
                    }
                    continue;
                }
                // 7. Two-Character Operators (==, !=, <=, >=, ..) 
                if (i + 1 < SourceCode.Length)
                {
                    string twoChar = SourceCode.Substring(i, 2);
                    if (Operators.ContainsKey(twoChar))
                    {
                        Tokens.Add(new Token(twoChar, Operators[twoChar]));
                        i += 1;
                        continue;
                    }
                }
                // 8. Single-Character Operators & Delimiters 
                string oneChar = CurrentChar.ToString();
                if (Operators.ContainsKey(oneChar))
                {
                    Tokens.Add(new Token(oneChar, Operators[oneChar]));
                    continue;
                }
                // 9. Trap States (Illegal '!' or completely unrecognized symbol) 
                if (CurrentChar == '!')
                    Errors.Error_List.Add("Lexical Error: Unexpected character '!'. Did you mean '!='?\r\n");
                else
                    Errors.Error_List.Add("Lexical Error: Unrecognized character '" + CurrentChar + "'\r\n");

            }

            JASON_Compiler.TokenStream = Tokens;
        }

    }
}