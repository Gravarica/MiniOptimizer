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
    : NULL
    | identifier
    | (MINUS | PLUS)? INTEGER_VALUE
    | (MINUS | PLUS)? DECIMAL_VALUE
    | QUOTED_STRING+
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

INTEGER_VALUE
    : DIGIT+
    ;

DECIMAL_VALUE
    : DECIMAL_DIGITS
    ;

IDENTIFIER
    : (LETTER | DIGIT)+
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
