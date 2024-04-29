grammar MiniQL;

query
     : SELECT attributeList FROM relationList (WHERE condition)?
     ;

attributeList
    : attribute (COMMA attribute)*
    ;

relationList
    : relation (COMMA relation)*
    ;

condition
    : condition AND condition
    | attribute EQ attribute                    
    ;

attribute
    : identifier                       
    | constant                        
    ;

relation
    : IDENTIFIER                      
    ;

identifier
    : IDENTIFIER DOT IDENTIFIER
    | IDENTIFIER
    ;

constant
    : NUMERIC_LITERAL
    | QUOTED_STRING+
    | identifier
    | NULL
    ;

SELECT: 'SELECT';
FROM: 'FROM';
WHERE: 'WHERE';
JOIN: 'JOIN';


DOT: '.';
COMMA: ',';
ASTERISK: '*';
LEFT_PARENTHESIS: '(';
RIGHT_PARENTHESIS: ')';
EQ: '=';
NOT : '!';
MINUS : '-';
PLUS: '+';
GT: '>';
GE: '>=';
LT: '<';
LE: '<=';
NE: '!=';

AND: 'AND' | 'and' | '&&';
OR: 'OR' | 'or' | '||';


QUOTED_STRING
    : '\'' ( ~('\''|'\\') | ('\\' .) )* '\''
    | '"' ( ~('"'|'\\') | ('\\' .) )* '"'
    ;

NUMERIC_LITERAL
: (PLUS | MINUS)? INTEGER_VALUE
| (PLUS | MINUS)? DECIMAL_VALUE
;

INTEGER_VALUE
: DIGIT+
;

DECIMAL_VALUE
: DIGIT+ '.' DIGIT*
| '.' DIGIT+
;

IDENTIFIER
    : LETTER (LETTER | DIGIT)*
    ;

fragment DECIMAL_DIGITS
    : DIGIT+ '.' DIGIT*
    | '.' DIGIT+
    ;

fragment DIGIT
    : [0-9]
    ;

fragment LETTER
    : [a-zA-Z]
    ;

WS
    : [ \t\r\n]+ -> skip               
    ;
