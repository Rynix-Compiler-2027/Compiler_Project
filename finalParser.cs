using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = Program();

            if (root != null && Errors.Error_List.Count == 0)
                MessageBox.Show("Success");

            return root;
        }

        // ------------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------------

        // Lookahead: is the current token of the given class?
        bool Is(Token_Class t)
        {
            return InputPointer < TokenStream.Count &&
                   TokenStream[InputPointer].token_type == t;
        }

        bool Add(Node parent, Node child)
        {
            if (child == null)
                return false;
            parent.Children.Add(child);
            return true;
        }

        void SyntaxError(string expected)
        {
            if (InputPointer < TokenStream.Count)
                Errors.Error_List.Add("Parsing Error: Expected " + expected + " but found "
                    + TokenStream[InputPointer].token_type.ToString()
                    + " ('" + TokenStream[InputPointer].lex + "')\r\n");
            else
                Errors.Error_List.Add("Parsing Error: Expected " + expected
                    + " but reached end of input\r\n");
        }

        // FIRST(TopLevel) = { var, val, fun, class }
        bool IsTopLevelStart()
        {
            return Is(Token_Class.Variable) || Is(Token_Class.Val) ||
                   Is(Token_Class.Fun) || Is(Token_Class.Class);
        }

        // FIRST(Statement)
        bool IsStatementStart()
        {
            return Is(Token_Class.Variable) || Is(Token_Class.Val) ||
                   Is(Token_Class.Identifier) || Is(Token_Class.If) ||
                   Is(Token_Class.While) || Is(Token_Class.For) ||
                   Is(Token_Class.Return) || Is(Token_Class.Break) ||
                   Is(Token_Class.Continue) || Is(Token_Class.Print) ||
                   Is(Token_Class.LBrace);
        }

        // FIRST(ClassMember) = { var, fun, init }
        bool IsClassMemberStart()
        {
            return Is(Token_Class.Variable) || Is(Token_Class.Fun) ||
                   Is(Token_Class.Init);
        }

        // FIRST(Expression)
        bool IsExpressionStart()
        {
            return Is(Token_Class.PlusOp) || Is(Token_Class.MinusOp) ||
                   Is(Token_Class.Identifier) || Is(Token_Class.IntegerLiteral) ||
                   Is(Token_Class.FloatLiteral) || Is(Token_Class.StringLiteral) ||
                   Is(Token_Class.CharLiteral) || Is(Token_Class.True) ||
                   Is(Token_Class.False) || Is(Token_Class.Null) ||
                   Is(Token_Class.LParenthesis) || Is(Token_Class.LBracket) ||
                   Is(Token_Class.New) || Is(Token_Class.Input);
        }

        bool IsRelOp()
        {
            return Is(Token_Class.EqualOp) || Is(Token_Class.NotEqualOp) ||
                   Is(Token_Class.LessThanOp) || Is(Token_Class.GreaterThanOp) ||
                   Is(Token_Class.LessThanOrEqualOp) || Is(Token_Class.GreaterThanOrEqualOp);
        }

        // ------------------------------------------------------------------
        // Program -> TopLevel Program | epsilon
        // TopLevel -> VariableDeclaration | ConstantDeclaration
        //           | FunctionDeclaration | ClassDeclaration
        // ------------------------------------------------------------------
        Node Program()
        {
            Node node = new Node("Program");

            if (InputPointer >= TokenStream.Count)
                return node; // epsilon

            if (!Add(node, TopLevel())) return null;
            if (!Add(node, Program())) return null;

            return node;
        }

        Node TopLevel()
        {
            Node node = new Node("TopLevel");

            if (Is(Token_Class.Variable))
            {
                if (!Add(node, VariableDeclaration())) return null;
            }
            else if (Is(Token_Class.Val))
            {
                if (!Add(node, ConstantDeclaration())) return null;
            }
            else if (Is(Token_Class.Fun))
            {
                if (!Add(node, FunctionDeclaration())) return null;
            }
            else if (Is(Token_Class.Class))
            {
                if (!Add(node, ClassDeclaration())) return null;
            }
            else
            {
                SyntaxError("'var', 'val', 'fun' or 'class'");
                return null;
            }

            return node;
        }
        // Variable / Constant Declarations
        // VariableDeclaration -> var Identifier VarTail
        // VarTail             -> : Type InitOpt | = Expression
        // InitOpt             -> = Expression | epsilon
        // ConstantDeclaration -> val Identifier : Type = Expression
        Node VariableDeclaration()
        {
            Node node = new Node("VariableDeclaration");

            if (!Add(node, match(Token_Class.Variable))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, VarTail())) return null;

            return node;
        }

        Node VarTail()
        {
            Node node = new Node("VarTail");

            if (Is(Token_Class.Colon))
            {
                if (!Add(node, match(Token_Class.Colon))) return null;
                if (!Add(node, Type())) return null;
                if (!Add(node, InitOpt())) return null;
            }
            else if (Is(Token_Class.AssignOp))
            {
                if (!Add(node, match(Token_Class.AssignOp))) return null;
                if (!Add(node, Expression())) return null;
            }
            else
            {
                SyntaxError("':' or '='");
                return null;
            }

            return node;
        }

        Node InitOpt()
        {
            Node node = new Node("InitOpt");

            if (Is(Token_Class.AssignOp))
            {
                if (!Add(node, match(Token_Class.AssignOp))) return null;
                if (!Add(node, Expression())) return null;
            }
            // else epsilon

            return node;
        }

        Node ConstantDeclaration()
        {
            Node node = new Node("ConstantDeclaration");

            if (!Add(node, match(Token_Class.Val))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.Colon))) return null;
            if (!Add(node, Type())) return null;
            if (!Add(node, match(Token_Class.AssignOp))) return null;
            if (!Add(node, Expression())) return null;

            return node;
        }

        // ------------------------------------------------------------------
        // Type -> Int | Float | Bool | Char | String | Array < Type > | Identifier
        // ------------------------------------------------------------------
        Node Type()
        {
            Node node = new Node("Type");

            if (InputPointer >= TokenStream.Count)
            {
                SyntaxError("a type");
                return null;
            }

            Token_Class current = TokenStream[InputPointer].token_type;

            if (current == Token_Class.Integer || current == Token_Class.Float ||
                current == Token_Class.Boolean || current == Token_Class.Character ||
                current == Token_Class.String || current == Token_Class.Identifier)
            {
                if (!Add(node, match(current))) return null;
            }
            else if (current == Token_Class.Array)
            {
                if (!Add(node, match(Token_Class.Array))) return null;
                if (!Add(node, match(Token_Class.LessThanOp))) return null;
                if (!Add(node, Type())) return null;
                if (!Add(node, match(Token_Class.GreaterThanOp))) return null;
            }
            else
            {
                SyntaxError("a type");
                return null;
            }

            return node;
        }

        // ------------------------------------------------------------------
        // Functions
        // FunctionDeclaration -> fun Identifier ( ParameterList ) ReturnType Block
        // ReturnType          -> : ReturnTypeValue | epsilon
        // ReturnTypeValue     -> Type | Void
        // ParameterList       -> Parameter ParameterTail | epsilon
        // ParameterTail       -> , Parameter ParameterTail | epsilon
        // Parameter           -> Identifier : Type
        // ------------------------------------------------------------------
        Node FunctionDeclaration()
        {
            Node node = new Node("FunctionDeclaration");

            if (!Add(node, match(Token_Class.Fun))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.LParenthesis))) return null;
            if (!Add(node, ParameterList())) return null;
            if (!Add(node, match(Token_Class.RParenthesis))) return null;
            if (!Add(node, ReturnType())) return null;
            if (!Add(node, Block())) return null;

            return node;
        }

        Node ReturnType()
        {
            Node node = new Node("ReturnType");

            if (Is(Token_Class.Colon))
            {
                if (!Add(node, match(Token_Class.Colon))) return null;
                if (!Add(node, ReturnTypeValue())) return null;
            }
            // else epsilon

            return node;
        }

        Node ReturnTypeValue()
        {
            Node node = new Node("ReturnTypeValue");

            if (Is(Token_Class.Void))
            {
                if (!Add(node, match(Token_Class.Void))) return null;
            }
            else
            {
                if (!Add(node, Type())) return null;
            }

            return node;
        }

        Node ParameterList()
        {
            Node node = new Node("ParameterList");

            if (Is(Token_Class.Identifier))
            {
                if (!Add(node, Parameter())) return null;
                if (!Add(node, ParameterTail())) return null;
            }
            // else epsilon

            return node;
        }

        Node ParameterTail()
        {
            Node node = new Node("ParameterTail");

            if (Is(Token_Class.Comma))
            {
                if (!Add(node, match(Token_Class.Comma))) return null;
                if (!Add(node, Parameter())) return null;
                if (!Add(node, ParameterTail())) return null;
            }
            // else epsilon

            return node;
        }

        Node Parameter()
        {
            Node node = new Node("Parameter");

            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.Colon))) return null;
            if (!Add(node, Type())) return null;

            return node;
        }

        // ------------------------------------------------------------------
        // Classes
        // ClassDeclaration       -> class Identifier { ClassMemberList }
        // ClassMemberList        -> ClassMember ClassMemberList | epsilon
        // ClassMember            -> FieldDeclaration | FunctionDeclaration
        //                         | ConstructorDeclaration
        // FieldDeclaration       -> var Identifier : Type
        // ConstructorDeclaration -> init ( ParameterList ) Block
        // ------------------------------------------------------------------
        Node ClassDeclaration()
        {
            Node node = new Node("ClassDeclaration");

            if (!Add(node, match(Token_Class.Class))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.LBrace))) return null;
            if (!Add(node, ClassMemberList())) return null;
            if (!Add(node, match(Token_Class.RBrace))) return null;

            return node;
        }

        Node ClassMemberList()
        {
            Node node = new Node("ClassMemberList");

            if (IsClassMemberStart())
            {
                if (!Add(node, ClassMember())) return null;
                if (!Add(node, ClassMemberList())) return null;
            }
            // else epsilon

            return node;
        }

        Node ClassMember()
        {
            Node node = new Node("ClassMember");

            if (Is(Token_Class.Variable))
            {
                if (!Add(node, FieldDeclaration())) return null;
            }
            else if (Is(Token_Class.Fun))
            {
                if (!Add(node, FunctionDeclaration())) return null;
            }
            else if (Is(Token_Class.Init))
            {
                if (!Add(node, ConstructorDeclaration())) return null;
            }
            else
            {
                SyntaxError("a class member ('var', 'fun' or 'init')");
                return null;
            }

            return node;
        }

        Node FieldDeclaration()
        {
            Node node = new Node("FieldDeclaration");

            if (!Add(node, match(Token_Class.Variable))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.Colon))) return null;
            if (!Add(node, Type())) return null;

            return node;
        }

        Node ConstructorDeclaration()
        {
            Node node = new Node("ConstructorDeclaration");

            if (!Add(node, match(Token_Class.Init))) return null;
            if (!Add(node, match(Token_Class.LParenthesis))) return null;
            if (!Add(node, ParameterList())) return null;
            if (!Add(node, match(Token_Class.RParenthesis))) return null;
            if (!Add(node, Block())) return null;

            return node;
        }
        // Blocks and Statements
        // Block         -> { StatementList }
        // StatementList -> Statement StatementList | epsilon
        Node Block()
        {
            Node node = new Node("Block");

            if (!Add(node, match(Token_Class.LBrace))) return null;
            if (!Add(node, StatementList())) return null;
            if (!Add(node, match(Token_Class.RBrace))) return null;

            return node;
        }

        Node StatementList()
        {
            Node node = new Node("StatementList");

            if (IsStatementStart())
            {
                if (!Add(node, Statement())) return null;
                if (!Add(node, StatementList())) return null;
            }
            // else epsilon

            return node;
        }

        Node Statement()
        {
            Node node = new Node("Statement");

            if (InputPointer >= TokenStream.Count)
            {
                SyntaxError("a statement");
                return null;
            }

            Token_Class current = TokenStream[InputPointer].token_type;

            if (current == Token_Class.Variable)
            {
                if (!Add(node, VariableDeclaration())) return null;
            }
            else if (current == Token_Class.Val)
            {
                if (!Add(node, ConstantDeclaration())) return null;
            }
            else if (current == Token_Class.Identifier)
            {
                if (!Add(node, IdentifierStatement())) return null;
            }
            else if (current == Token_Class.If)
            {
                if (!Add(node, IfStatement())) return null;
            }
            else if (current == Token_Class.While)
            {
                if (!Add(node, WhileStatement())) return null;
            }
            else if (current == Token_Class.For)
            {
                if (!Add(node, ForStatement())) return null;
            }
            else if (current == Token_Class.Return)
            {
                if (!Add(node, ReturnStatement())) return null;
            }
            else if (current == Token_Class.Break)
            {
                if (!Add(node, BreakStatement())) return null;
            }
            else if (current == Token_Class.Continue)
            {
                if (!Add(node, ContinueStatement())) return null;
            }
            else if (current == Token_Class.Print)
            {
                if (!Add(node, PrintStatement())) return null;
            }
            else if (current == Token_Class.LBrace)
            {
                if (!Add(node, Block())) return null;
            }
            else
            {
                SyntaxError("a statement");
                return null;
            }

            return node;
        }

        // IfStatement -> if Expression Block ElseClause
        Node IfStatement()
        {
            Node node = new Node("IfStatement");

            if (!Add(node, match(Token_Class.If))) return null;
            if (!Add(node, Expression())) return null;
            if (!Add(node, Block())) return null;
            if (!Add(node, ElseClause())) return null;

            return node;
        }

        // ElseClause -> else Block | epsilon
        Node ElseClause()
        {
            Node node = new Node("ElseClause");

            if (Is(Token_Class.Else))
            {
                if (!Add(node, match(Token_Class.Else))) return null;
                if (!Add(node, Block())) return null;
            }
            // else epsilon

            return node;
        }

        // WhileStatement -> while Expression Block
        Node WhileStatement()
        {
            Node node = new Node("WhileStatement");

            if (!Add(node, match(Token_Class.While))) return null;
            if (!Add(node, Expression())) return null;
            if (!Add(node, Block())) return null;

            return node;
        }

        // ForStatement -> for Identifier in Expression RangeOpt Block
        Node ForStatement()
        {
            Node node = new Node("ForStatement");

            if (!Add(node, match(Token_Class.For))) return null;
            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, match(Token_Class.In))) return null;
            if (!Add(node, Expression())) return null;
            if (!Add(node, RangeOpt())) return null;
            if (!Add(node, Block())) return null;

            return node;
        }

        // RangeOpt -> .. Expression | epsilon
        Node RangeOpt()
        {
            Node node = new Node("RangeOpt");

            if (Is(Token_Class.RangeOp))
            {
                if (!Add(node, match(Token_Class.RangeOp))) return null;
                if (!Add(node, Expression())) return null;
            }
            // else epsilon

            return node;
        }

        // ReturnStatement -> return Expression
        Node ReturnStatement()
        {
            Node node = new Node("ReturnStatement");

            if (!Add(node, match(Token_Class.Return))) return null;
            if (!Add(node, Expression())) return null;

            return node;
        }

        // PrintStatement -> print ( Expression )
        Node PrintStatement()
        {
            Node node = new Node("PrintStatement");

            if (!Add(node, match(Token_Class.Print))) return null;
            if (!Add(node, match(Token_Class.LParenthesis))) return null;
            if (!Add(node, Expression())) return null;
            if (!Add(node, match(Token_Class.RParenthesis))) return null;

            return node;
        }

        // BreakStatement -> break
        Node BreakStatement()
        {
            Node node = new Node("BreakStatement");

            if (!Add(node, match(Token_Class.Break))) return null;

            return node;
        }

        // ContinueStatement -> continue
        Node ContinueStatement()
        {
            Node node = new Node("ContinueStatement");

            if (!Add(node, match(Token_Class.Continue))) return null;

            return node;
        }
        // Identifier statements
        // IdentifierStatement     -> Identifier IdentifierStatementTail
        // IdentifierStatementTail -> = Expression
        //                          | . Identifier IdentifierStatementTail
        //                          | [ Expression ] IdentifierStatementTail
        //                          | ( ArgumentList ) CallTail
        // CallTail                -> . Identifier CallTail
        //                          | [ Expression ] CallTail
        //                          | ( ArgumentList ) CallTail
        //                          | epsilon
        Node IdentifierStatement()
        {
            Node node = new Node("IdentifierStatement");

            if (!Add(node, match(Token_Class.Identifier))) return null;
            if (!Add(node, IdentifierStatementTail())) return null;

            return node;
        }

        Node IdentifierStatementTail()
        {
            Node node = new Node("IdentifierStatementTail");

            if (Is(Token_Class.AssignOp))
            {
                if (!Add(node, match(Token_Class.AssignOp))) return null;
                if (!Add(node, Expression())) return null;
            }
            else if (Is(Token_Class.Dot))
            {
                if (!Add(node, match(Token_Class.Dot))) return null;
                if (!Add(node, match(Token_Class.Identifier))) return null;
                if (!Add(node, IdentifierStatementTail())) return null;
            }
            else if (Is(Token_Class.LBracket))
            {
                if (!Add(node, match(Token_Class.LBracket))) return null;
                if (!Add(node, Expression())) return null;
                if (!Add(node, match(Token_Class.RBracket))) return null;
                if (!Add(node, IdentifierStatementTail())) return null;
            }
            else if (Is(Token_Class.LParenthesis))
            {
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, ArgumentList())) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
                if (!Add(node, CallTail())) return null;
            }
            else
            {
                SyntaxError("'=', '.', '[' or '('");
                return null;
            }

            return node;
        }

        Node CallTail()
        {
            Node node = new Node("CallTail");

            if (Is(Token_Class.Dot))
            {
                if (!Add(node, match(Token_Class.Dot))) return null;
                if (!Add(node, match(Token_Class.Identifier))) return null;
                if (!Add(node, CallTail())) return null;
            }
            else if (Is(Token_Class.LBracket))
            {
                if (!Add(node, match(Token_Class.LBracket))) return null;
                if (!Add(node, Expression())) return null;
                if (!Add(node, match(Token_Class.RBracket))) return null;
                if (!Add(node, CallTail())) return null;
            }
            else if (Is(Token_Class.LParenthesis))
            {
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, ArgumentList())) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
                if (!Add(node, CallTail())) return null;
            }
            // else epsilon

            return node;
        }
        // Expressions
        // Expression   -> AdditiveExpr RelTail
        // RelTail      -> RelOp AdditiveExpr | epsilon
        // RelOp        -> == | != | < | > | <= | >=
        // AdditiveExpr -> MultExpr AddTail
        // AddTail      -> + MultExpr AddTail | - MultExpr AddTail | epsilon
        // MultExpr     -> UnaryExpr MultTail
        // MultTail     -> * UnaryExpr MultTail | / UnaryExpr MultTail
        //               | % UnaryExpr MultTail | epsilon
        // UnaryExpr    -> + UnaryExpr | - UnaryExpr | PostfixExpr
        Node Expression()
        {
            Node node = new Node("Expression");

            if (!Add(node, AdditiveExpr())) return null;
            if (!Add(node, RelTail())) return null;

            return node;
        }

        Node RelTail()
        {
            Node node = new Node("RelTail");

            if (IsRelOp())
            {
                if (!Add(node, RelOp())) return null;
                if (!Add(node, AdditiveExpr())) return null;
            }
            // else epsilon

            return node;
        }

        Node RelOp()
        {
            Node node = new Node("RelOp");

            if (IsRelOp())
            {
                if (!Add(node, match(TokenStream[InputPointer].token_type))) return null;
            }
            else
            {
                SyntaxError("a relational operator");
                return null;
            }

            return node;
        }

        Node AdditiveExpr()
        {
            Node node = new Node("AdditiveExpr");

            if (!Add(node, MultExpr())) return null;
            if (!Add(node, AddTail())) return null;

            return node;
        }

        Node AddTail()
        {
            Node node = new Node("AddTail");

            if (Is(Token_Class.PlusOp) || Is(Token_Class.MinusOp))
            {
                if (!Add(node, match(TokenStream[InputPointer].token_type))) return null;
                if (!Add(node, MultExpr())) return null;
                if (!Add(node, AddTail())) return null;
            }
            // else epsilon

            return node;
        }

        Node MultExpr()
        {
            Node node = new Node("MultExpr");

            if (!Add(node, UnaryExpr())) return null;
            if (!Add(node, MultTail())) return null;

            return node;
        }

        Node MultTail()
        {
            Node node = new Node("MultTail");

            if (Is(Token_Class.MultiplyOp) || Is(Token_Class.DivideOp) || Is(Token_Class.ModOp))
            {
                if (!Add(node, match(TokenStream[InputPointer].token_type))) return null;
                if (!Add(node, UnaryExpr())) return null;
                if (!Add(node, MultTail())) return null;
            }
            // else epsilon

            return node;
        }

        Node UnaryExpr()
        {
            Node node = new Node("UnaryExpr");

            if (Is(Token_Class.PlusOp) || Is(Token_Class.MinusOp))
            {
                if (!Add(node, match(TokenStream[InputPointer].token_type))) return null;
                if (!Add(node, UnaryExpr())) return null;
            }
            else
            {
                if (!Add(node, PostfixExpr())) return null;
            }

            return node;
        }

        // ------------------------------------------------------------------
        // Postfix and Primary
        // PostfixExpr -> Primary PostfixTail
        // PostfixTail -> ( ArgumentList ) PostfixTail
        //              | [ Expression ] PostfixTail
        //              | . Identifier PostfixTail | epsilon
        // ------------------------------------------------------------------
        Node PostfixExpr()
        {
            Node node = new Node("PostfixExpr");

            if (!Add(node, Primary())) return null;
            if (!Add(node, PostfixTail())) return null;

            return node;
        }

        Node PostfixTail()
        {
            Node node = new Node("PostfixTail");

            if (Is(Token_Class.LParenthesis))
            {
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, ArgumentList())) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
                if (!Add(node, PostfixTail())) return null;
            }
            else if (Is(Token_Class.LBracket))
            {
                if (!Add(node, match(Token_Class.LBracket))) return null;
                if (!Add(node, Expression())) return null;
                if (!Add(node, match(Token_Class.RBracket))) return null;
                if (!Add(node, PostfixTail())) return null;
            }
            else if (Is(Token_Class.Dot))
            {
                if (!Add(node, match(Token_Class.Dot))) return null;
                if (!Add(node, match(Token_Class.Identifier))) return null;
                if (!Add(node, PostfixTail())) return null;
            }
            // else epsilon

            return node;
        }

        // Primary -> Identifier | IntegerLiteral | FloatLiteral | StringLiteral
        //          | CharLiteral | true | false | null | ( Expression )
        //          | ArrayLiteral | new Identifier ( ArgumentList ) | input ( )
        Node Primary()
        {
            Node node = new Node("Primary");

            if (InputPointer >= TokenStream.Count)
            {
                SyntaxError("an expression");
                return null;
            }

            Token_Class current = TokenStream[InputPointer].token_type;

            if (current == Token_Class.Identifier || current == Token_Class.IntegerLiteral ||
                current == Token_Class.FloatLiteral || current == Token_Class.StringLiteral ||
                current == Token_Class.CharLiteral || current == Token_Class.True ||
                current == Token_Class.False || current == Token_Class.Null)
            {
                if (!Add(node, match(current))) return null;
            }
            else if (current == Token_Class.LParenthesis)
            {
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, Expression())) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
            }
            else if (current == Token_Class.LBracket)
            {
                if (!Add(node, ArrayLiteral())) return null;
            }
            else if (current == Token_Class.New)
            {
                if (!Add(node, match(Token_Class.New))) return null;
                if (!Add(node, match(Token_Class.Identifier))) return null;
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, ArgumentList())) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
            }
            else if (current == Token_Class.Input)
            {
                if (!Add(node, match(Token_Class.Input))) return null;
                if (!Add(node, match(Token_Class.LParenthesis))) return null;
                if (!Add(node, match(Token_Class.RParenthesis))) return null;
            }
            else
            {
                SyntaxError("an expression");
                return null;
            }

            return node;
        }

        // ArrayLiteral -> [ ArgumentList ]
        Node ArrayLiteral()
        {
            Node node = new Node("ArrayLiteral");

            if (!Add(node, match(Token_Class.LBracket))) return null;
            if (!Add(node, ArgumentList())) return null;
            if (!Add(node, match(Token_Class.RBracket))) return null;

            return node;
        }

        // ArgumentList -> Expression ArgumentTail | epsilon
        Node ArgumentList()
        {
            Node node = new Node("ArgumentList");

            if (IsExpressionStart())
            {
                if (!Add(node, Expression())) return null;
                if (!Add(node, ArgumentTail())) return null;
            }
            // else epsilon

            return node;
        }

        // ArgumentTail -> , Expression ArgumentTail | epsilon
        Node ArgumentTail()
        {
            Node node = new Node("ArgumentTail");

            if (Is(Token_Class.Comma))
            {
                if (!Add(node, match(Token_Class.Comma))) return null;
                if (!Add(node, Expression())) return null;
                if (!Add(node, ArgumentTail())) return null;
            }
            // else epsilon

            return node;
        }

        // ------------------------------------------------------------------
        // match (unchanged)
        // ------------------------------------------------------------------
        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    // InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
