grammar OASISGrammar;
 
@parser::members
{
    protected const int EOF = Eof;
}
 
@lexer::members
{
    protected const int EOF = Eof;
    protected const int HIDDEN = Hidden;
}
 
/*
 * Parser Rules
 */
 
program: global* (script|objectcode|stringpack)+  ;

global:
	t=('bool'|'byte') IDENT ('as' NUMBER)? ';' # GlobalDeclare
	;

stringpack: 'stringpack' NUMBER '{' STRING (';' STRING)* ';'? '}'	#StringpackMain
	;

objectcode: 'objectcode' NUMBER block				#ObjectCodeMain
	;

script: 'script' NUMBER block 						#ScriptMain
	;

block: '{' declaration* statement* '}'
	;

declaration:
	t=('bool'|'byte') IDENT ('=' NUMBER)? ';'			# Declare
	;

statement 
    :   IDENT ':' statement										# Label
	|	assignment ';'											# TopAssignmentStatement
    |   block													# ABlock
    |   'if' '(' logicalexpression ')' statement ('else' statement)?	# IfStatement
    |   iterationStatement										# AnIterationStatement
    |   'goto' IDENT ';'										# GotoStatement
	|	IDENT '(' argumentlist? ')' ';'							# CommandCall
    ;


iterationStatement
    :   'while' '(' logicalexpression ')' statement									# WhileStatement
    |   'do' statement 'while' '(' logicalexpression ')' ';'						# DoWhileStatement
    |   'for' '(' assignment? ';' logicalexpression? ';' assignment? ')' statement	# ForStatement
    ;

assignment: IDENT '=' (expression|logicalexpression)		#AssignmentStatement
	;
 
expression  
	 :	expression op=('*'|'/') expression  # MulDiv
     |	expression op=('+'|'-') expression  # AddSub
     |	NUMBER								# Number
     |	'(' expression ')'					# Parens
	 |	IDENT								# Identifier
	 |	IDENT '(' argumentlist? ')'			# FunctionCall
     ;
 
 argumentlist
	: (expression|logicalexpression) (',' (expression|logicalexpression))*
	; 

logicalexpression
	: expression op=('>' | '>=' | '<' | '<=' | '==' | '!=') expression		# RelationalExpression
	| logicalexpression op=('&&' | '||') logicalexpression					# AndOrLExpression
	| '!' logicalexpression													# NotLExpression
    | '(' logicalexpression ')'												# LParens
	| IDENT '(' argumentlist? ')'											# LFunctionCall
	| IDENT																	# LIdentifier
	| tv=('true'|'false')													# LConstant					
	;

/*
 * Lexer Rules
 */
TYPEBOOL: 'bool';
TYPEBYTE: 'byte';

TRUE: 'true';
FALSE: 'false';
	
IDENT
   : [a-zA-Z] [a-zA-Z0-9._"]*
   ;
NUMBER
   : [0-9]+
   ;
COMMENT
   : '//' ~ [\r\n]* -> skip
   ;
STRING
   : '"' ~ ["]* '"'
   ;
MUL : '*';
DIV : '/';
ADD : '+';
SUB : '-';

LT: '<';
GT: '>';
LE: '<=';
GE: '>=';
EQ: '==';
NE: '!=';

LAND: '&&';
LOR: '||';
LNOT: '!';

WS
    :   (' ' | '\r' | '\n' | '\t') -> channel(HIDDEN)
    ;