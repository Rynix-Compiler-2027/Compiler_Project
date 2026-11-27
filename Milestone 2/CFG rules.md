# CFG Rules for Rynix Programming Language

## 1. Terminals

### Reserved Words
`var`, `val`, `fun`, `class`, `if`, `else`, `while`, `for`, `in`, `return`, `true`, `false`, `null`, `new`, `print`, `input`, `break`, `continue`, `init`

### Data Types
`Int`, `Float`, `Bool`, `Char`, `String`, `Void`, `Array`

### Literals and Identifiers
`Identifier`, `IntegerLiteral`, `FloatLiteral`, `StringLiteral`, `CharLiteral`

### Operators
`=`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `+`, `-`, `*`, `/`, `%`, `..`

### Delimiters
`:`, `,`, `{`, `}`, `(`, `)`, `[`, `]`, `.`

---

## 2. Production Rules

### 2.1 Program Structure

```text
Program → TopLevel Program | ε

TopLevel → Statement | FunctionDeclaration | ClassDeclaration
```

### 2.2 Variable Declarations

```text
VariableDeclaration → var Identifier VarTail | val Identifier : Type = Expression

VarTail → : Type InitOpt | = Expression

InitOpt → = Expression | ε
```

### 2.3 Types

```text
Type → Int | Float | Bool | Char | String | Void | Array < Type > | Identifier
```

### 2.4 Function Declarations

```text
FunctionDeclaration → fun Identifier ( ParameterList ) ReturnType Block

ReturnType → : Type | ε

ParameterList → Parameter ParameterTail | ε

ParameterTail → , Parameter ParameterTail | ε

Parameter → Identifier : Type
```

### 2.5 Class Declarations

```text
ClassDeclaration → class Identifier { ClassMemberList }

ClassMemberList → ClassMember ClassMemberList | ε

ClassMember → VariableDeclaration
            | FunctionDeclaration
            | init ( ParameterList ) Block
```

### 2.6 Blocks and Statements

```text
Block → { StatementList }

StatementList → Statement StatementList | ε

Statement → VariableDeclaration
         | IdentifierStatement
         | if Expression Block ElseClause
         | while Expression Block
         | for Identifier in Expression RangeOpt Block
         | return Expression
         | break
         | continue
         | print ( Expression )
         | Block

ElseClause → else Block | ε

RangeOpt → .. Expression | ε
```

### 2.7 Identifier Statements

```text
IdentifierStatement → Identifier PostfixTail AssignOpt

AssignOpt → = Expression | ε
```

### 2.8 Expressions

```text
Expression → AdditiveExpr RelTail

RelTail → RelOp AdditiveExpr | ε

RelOp → == | != | < | > | <= | >=
```

### 2.9 Additive Expressions

```text
AdditiveExpr → MultExpr AddTail

AddTail → + MultExpr AddTail
        | - MultExpr AddTail
        | ε
```

### 2.10 Multiplicative Expressions

```text
MultExpr → UnaryExpr MultTail

MultTail → * UnaryExpr MultTail
         | / UnaryExpr MultTail
         | % UnaryExpr MultTail
         | ε
```

### 2.11 Unary Expressions

```text
UnaryExpr → + UnaryExpr
          | - UnaryExpr
          | PostfixExpr
```

### 2.12 Postfix Expressions

```text
PostfixExpr → Primary PostfixTail

PostfixTail → ( ArgumentList ) PostfixTail
            | [ Expression ] PostfixTail
            | . Identifier PostfixTail
            | ε
```

### 2.13 Primary Expressions

```text
Primary → Identifier
        | IntegerLiteral
        | FloatLiteral
        | StringLiteral
        | CharLiteral
        | true
        | false
        | null
        | ( Expression )
        | ArrayLiteral
        | new Identifier ( ArgumentList )
        | input ( )
```

### 2.14 Array Literals

```text
ArrayLiteral → [ ArgumentList ]
```

### 2.15 Argument Lists

```text
ArgumentList → Expression ArgumentTail | ε

ArgumentTail → , Expression ArgumentTail | ε
```
