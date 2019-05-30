
using System;
using System.Linq;
using System.Collections.Generic;
using Dyalect.Parser.Model;


namespace Dyalect.Parser
{
    partial class InternalParser
    {
	public const int _EOF = 0;
	public const int _identToken = 1;
	public const int _intToken = 2;
	public const int _floatToken = 3;
	public const int _stringToken = 4;
	public const int _charToken = 5;
	public const int _implicitToken = 6;
	public const int _varToken = 7;
	public const int _constToken = 8;
	public const int _funcToken = 9;
	public const int _returnToken = 10;
	public const int _continueToken = 11;
	public const int _breakToken = 12;
	public const int _yieldToken = 13;
	public const int _ifToken = 14;
	public const int _forToken = 15;
	public const int _whileToken = 16;
	public const int _doToken = 17;
	public const int _typeToken = 18;
	public const int _arrowToken = 19;
	public const int _dotToken = 20;
	public const int _commaToken = 21;
	public const int _semicolonToken = 22;
	public const int _colonToken = 23;
	public const int _equalToken = 24;
	public const int _parenLeftToken = 25;
	public const int _parenRightToken = 26;
	public const int _curlyLeftToken = 27;
	public const int _curlyRightToken = 28;
	public const int _squareLeftToken = 29;
	public const int _squareRightToken = 30;
	public const int _minus = 31;
	public const int _plus = 32;
	public const int _not = 33;
	public const int _bitnot = 34;
	public const int maxT = 79;




        private void Get() 
        {
            for (;;)
            {
                t = la;
                la = scanner.Scan();
            
                if (la.kind <= maxT)
                {
                    ++errDist;
                    break;
                }

                la = t;
            }
        }

	void Separator() {
		Expect(22);
	}

	void StandardOperators() {
		switch (la.kind) {
		case 32: {
			Get();
			break;
		}
		case 31: {
			Get();
			break;
		}
		case 35: {
			Get();
			break;
		}
		case 36: {
			Get();
			break;
		}
		case 37: {
			Get();
			break;
		}
		case 38: {
			Get();
			break;
		}
		case 39: {
			Get();
			break;
		}
		case 33: {
			Get();
			break;
		}
		case 40: {
			Get();
			break;
		}
		case 41: {
			Get();
			break;
		}
		case 42: {
			Get();
			break;
		}
		case 43: {
			Get();
			break;
		}
		case 44: {
			Get();
			break;
		}
		case 45: {
			Get();
			break;
		}
		case 46: {
			Get();
			break;
		}
		case 47: {
			Get();
			break;
		}
		case 48: {
			Get();
			break;
		}
		case 34: {
			Get();
			break;
		}
		default: SynErr(80); break;
		}
	}

	void FunctionName() {
		if (la.kind == 1) {
			Get();
		} else if (StartOf(1)) {
			StandardOperators();
		} else SynErr(81);
	}

	void Qualident(out string s1, out string s2, out string s3) {
		s1 = null; s2 = null; s3 = null; 
		FunctionName();
		s1 = t.val; 
		if (StartOf(2)) {
			if (la.kind == 20) {
				Get();
			}
			FunctionName();
			s2 = t.val; 
			if (StartOf(2)) {
				if (la.kind == 20) {
					Get();
				}
				FunctionName();
				s3 = t.val; 
			}
		}
	}

	void Import() {
		Expect(49);
		var inc = new DImport(t); 
		Expect(1);
		inc.ModuleName = t.val; 
		if (la.kind == 24) {
			Get();
			Expect(1);
			inc.Alias = inc.ModuleName;
			inc.ModuleName = t.val;
			
		}
		if (la.kind == 25) {
			Get();
			Expect(4);
			inc.Dll = ParseSimpleString(); 
			Expect(26);
		}
		Imports.Add(inc); 
	}

	void Type(out DNode node) {
		Expect(18);
		var typ = new DTypeDeclaration(t);
		node = typ;
		
		Expect(1);
		typ.Name = t.val; 
		if (la.kind == 27) {
			Get();
			if (la.kind == 1) {
				TypeField(out var fld);
				typ.Fields.Add(fld); 
				while (la.kind == 21) {
					Get();
					Expect(1);
					typ.Fields.Add(new DFieldDeclaration(t) { Name = t.val }); 
				}
			}
			Expect(28);
		}
	}

	void TypeField(out DFieldDeclaration node) {
		Expect(1);
		node = new DFieldDeclaration(t) { Name = t.val }; 
	}

	void Statement(out DNode node) {
		node = null; 
		if (StartOf(3)) {
			ControlFlow(out node);
			Separator();
		} else if (la.kind == 18) {
			Type(out node);
			Separator();
		} else if (la.kind == 52) {
			Match(out node);
		} else if (StartOf(4)) {
			SimpleExpr(out node);
			Separator();
		} else if (IsIterator()) {
			Iterator(out node);
		} else if (la.kind == 27) {
			Block(out node);
		} else if (la.kind == 14) {
			If(out node);
		} else if (la.kind == 15 || la.kind == 16) {
			Loops(out node);
		} else if (la.kind == 9 || la.kind == 50) {
			Function(out node);
		} else SynErr(82);
	}

	void ControlFlow(out DNode node) {
		node = null; 
		if (la.kind == 12) {
			Break(out node);
		} else if (la.kind == 11) {
			Continue(out node);
		} else if (la.kind == 10) {
			Return(out node);
		} else if (la.kind == 13) {
			Yield(out node);
		} else SynErr(83);
	}

	void Match(out DNode node) {
		node = null; 
		Expect(52);
		var m = new DMatch(t); 
		Expr(out node);
		m.Expression = node; 
		Expect(27);
		MatchEntry(out var entry);
		m.Entries.Add(entry); 
		while (la.kind == 21) {
			Get();
			MatchEntry(out entry);
			m.Entries.Add(entry); 
		}
		Expect(28);
		node = m; 
	}

	void SimpleExpr(out DNode node) {
		node = null; 
		if (la.kind == 7 || la.kind == 8) {
			Binding(out node);
		} else if (IsFunction()) {
			FunctionExpr(out node);
		} else if (IsLabel()) {
			Label(out node);
		} else if (StartOf(5)) {
			Assignment(out node);
		} else if (la.kind == 65) {
			TryCatch(out node);
		} else if (la.kind == 64) {
			Throw(out node);
		} else SynErr(84);
		if (implicits != null)
		{
		   var func = new DFunctionDeclaration(node.Location)
		   {
		       Body = node
		   };
		   implicits.Sort();
		   func.Parameters.AddRange(implicits.Distinct().Select(i => new DParameter(t) { Name = "p" + i }));
		   node = func;
		   implicits = null;
		}
		
	}

	void Iterator(out DNode node) {
		node = null; 
		Expect(27);
		var it = new DIteratorLiteral(t);
		it.YieldBlock = new DYieldBlock(t);
		
		Expr(out node);
		it.YieldBlock.Elements.Add(node); 
		Expect(21);
		if (StartOf(6)) {
			Expr(out node);
			it.YieldBlock.Elements.Add(node); 
			while (la.kind == 21) {
				Get();
				Expr(out node);
				it.YieldBlock.Elements.Add(node); 
			}
		}
		node = it; 
		Expect(28);
	}

	void Block(out DNode node) {
		node = null; 
		Expect(27);
		var block = new DBlock(t); 
		if (StartOf(7)) {
			Statement(out node);
			block.Nodes.Add(node); 
			while (StartOf(7)) {
				Statement(out node);
				block.Nodes.Add(node); 
			}
		}
		node = block; 
		Expect(28);
	}

	void If(out DNode node) {
		node = null; 
		Expect(14);
		var @if = new DIf(t); 
		Expr(out node);
		@if.Condition = node; 
		Block(out node);
		@if.True = node; 
		if (la.kind == 62) {
			Get();
			if (la.kind == 27) {
				Block(out node);
				@if.False = node; 
			} else if (la.kind == 14) {
				If(out node);
				@if.False = node; 
			} else SynErr(85);
		}
		node = @if; 
	}

	void Loops(out DNode node) {
		node = null; 
		if (la.kind == 16) {
			While(out node);
		} else if (la.kind == 15) {
			For(out node);
		} else SynErr(86);
	}

	void Function(out DNode node) {
		node = null; bool st = false; 
		if (la.kind == 50) {
			Get();
			st = true; 
		}
		Expect(9);
		var f = new DFunctionDeclaration(t) { IsStatic = st };
		functions.Push(f);
		
		Qualident(out var s1, out var s2, out var s3);
		if (s2 == null && s3 == null)
		   f.Name = s1;
		else if (s3 == null)
		{
		   f.Name = s2;
		   f.TypeName = new Qualident(s1);
		}
		else
		{
		   f.Name = s3;
		   f.TypeName = new Qualident(s1, s2);
		}
		
		FunctionArguments(f);
		Block(out node);
		f.Body = node;
		node = f;
		functions.Pop();
		
	}

	void FunctionArguments(DFunctionDeclaration node) {
		Expect(25);
		if (la.kind == 1) {
			FunctionArgument(out var arg);
			node.Parameters.Add(arg); 
			while (la.kind == 21) {
				Get();
				FunctionArgument(out arg);
				node.Parameters.Add(arg); 
			}
		}
		Expect(26);
	}

	void FunctionArgument(out DParameter arg) {
		arg = null; 
		Expect(1);
		arg = new DParameter(t) { Name = t.val }; 
		if (la.kind == 24) {
			Get();
			Expr(out var cnode);
			arg.DefaultValue = cnode; 
		}
		if (la.kind == 51) {
			Get();
			arg.IsVarArgs = true; 
		}
	}

	void Expr(out DNode node) {
		node = null; 
		if (la.kind == 14) {
			If(out node);
		} else if (StartOf(4)) {
			SimpleExpr(out node);
		} else if (IsIterator()) {
			Iterator(out node);
		} else if (la.kind == 27) {
			Block(out node);
		} else if (la.kind == 15 || la.kind == 16) {
			Loops(out node);
		} else if (la.kind == 52) {
			Match(out node);
		} else SynErr(87);
	}

	void Binding(out DNode node) {
		if (la.kind == 7) {
			Get();
		} else if (la.kind == 8) {
			Get();
		} else SynErr(88);
		var bin = new DBinding(t) { Constant = t.val == "const" }; 
		OrPattern(out var pat);
		bin.Pattern = pat; 
		if (la.kind == 24) {
			Get();
			Expr(out node);
			bin.Init = node; 
		}
		node = bin; 
	}

	void OrPattern(out DPattern node) {
		node = null; 
		AndPattern(out node);
		while (la.kind == 54) {
			var por = new DOrPattern(t) { Left = node }; 
			Get();
			AndPattern(out node);
			por.Right = node; node = por; 
		}
	}

	void MatchEntry(out DMatchEntry me) {
		me = new DMatchEntry(t); 
		OrPattern(out var p);
		me.Pattern = p; 
		if (la.kind == 53) {
			Get();
			Expr(out var node);
			me.Guard = node; 
		}
		Expect(19);
		Expr(out var exp);
		me.Expression = exp; 
	}

	void AndPattern(out DPattern node) {
		node = null; 
		RangePattern(out node);
		while (la.kind == 55) {
			var pa = new DAndPattern(t) { Left = node }; 
			Get();
			RangePattern(out node);
			pa.Right = node; node = pa; 
		}
	}

	void RangePattern(out DPattern node) {
		node = null; 
		Pattern(out node);
		if (la.kind == 56) {
			var r = new DRangePattern(t) { From = node }; 
			Get();
			Pattern(out node);
			r.To = node; node = r; 
		}
	}

	void Pattern(out DPattern node) {
		node = null; 
		if (la.kind == 1) {
			NamePattern(out node);
		} else if (la.kind == 2) {
			IntegerPattern(out node);
		} else if (la.kind == 3) {
			FloatPattern(out node);
		} else if (la.kind == 5) {
			CharPattern(out node);
		} else if (la.kind == 4) {
			StringPattern(out node);
		} else if (la.kind == 60 || la.kind == 61) {
			BooleanPattern(out node);
		} else if (la.kind == 59) {
			NilPattern(out node);
		} else if (IsRecordPattern()) {
			RecordPattern(out node);
		} else if (IsTuple(allowFields: false)) {
			TuplePattern(out node);
		} else if (la.kind == 25) {
			GroupPattern(out node);
		} else if (la.kind == 29) {
			ArrayPattern(out node);
		} else if (la.kind == 20) {
			MethodCheckPattern(out node);
		} else SynErr(89);
		if (la.kind == 57) {
			Get();
			var asp = new DAsPattern(t) { Pattern = node }; 
			Expect(1);
			asp.Name = t.val; node = asp; 
		}
	}

	void NamePattern(out DPattern node) {
		node = null; string nm2 = null; string nm1 = null; Token ot = null; 
		Expect(1);
		nm1 = t.val; ot = t; 
		if (la.kind == 20) {
			Get();
			Expect(1);
			nm2 = null; 
		}
		if (nm2 == null) {
		   if (t.val == "_") 
		       node = new DWildcardPattern(ot);
		   else
		       node = new DNamePattern(ot) { Name = nm1 };
		} else {
		   var q = new Qualident(nm2, nm1);
		   node = new DTypeTestPattern(ot) { TypeName = q };
		}
		
	}

	void IntegerPattern(out DPattern node) {
		Expect(2);
		node = new DIntegerPattern(t) { Value = ParseInteger() }; 
	}

	void FloatPattern(out DPattern node) {
		Expect(3);
		node = new DFloatPattern(t) { Value = ParseFloat() }; 
	}

	void CharPattern(out DPattern node) {
		Expect(5);
		node = new DCharPattern(t) { Value = ParseChar() }; 
	}

	void StringPattern(out DPattern node) {
		Expect(4);
		node = new DStringPattern(t) { Value = ParseString() }; 
	}

	void BooleanPattern(out DPattern node) {
		if (la.kind == 60) {
			Get();
		} else if (la.kind == 61) {
			Get();
		} else SynErr(90);
		node = new DBooleanPattern(t) { Value = t.val == "true" }; 
	}

	void NilPattern(out DPattern node) {
		Expect(59);
		node = new DNilPattern(t); 
	}

	void RecordPattern(out DPattern node) {
		node = null; 
		Expect(25);
		var rec = new DRecordPattern(t); 
		FieldPattern(out var lab);
		rec.Elements.Add(lab); 
		while (la.kind == 21) {
			Get();
			FieldPattern(out lab);
			rec.Elements.Add(lab); 
		}
		node = rec; 
		Expect(26);
	}

	void TuplePattern(out DPattern node) {
		node = null; 
		Expect(25);
		var tup = new DTuplePattern(t); 
		OrPattern(out node);
		tup.Elements.Add(node); 
		while (la.kind == 21) {
			Get();
			OrPattern(out node);
			tup.Elements.Add(node); 
		}
		node = tup; 
		Expect(26);
	}

	void GroupPattern(out DPattern node) {
		node = null; 
		Expect(25);
		OrPattern(out node);
		Expect(26);
	}

	void ArrayPattern(out DPattern node) {
		node = null; 
		Expect(29);
		var tup = new DArrayPattern(t); 
		RangePattern(out node);
		tup.Elements.Add(node); 
		while (la.kind == 21) {
			Get();
			RangePattern(out node);
			tup.Elements.Add(node); 
		}
		node = tup; 
		Expect(30);
	}

	void MethodCheckPattern(out DPattern node) {
		node = null; 
		Expect(20);
		Expect(1);
		node = new DMethodCheckPattern(t) { Name = t.val }; 
		Expect(58);
	}

	void FieldPattern(out DLabelPattern node) {
		node = null; 
		Expect(1);
		var la = new DLabelPattern(t) { Label = t.val }; 
		Expect(23);
		RangePattern(out var pat);
		la.Pattern = pat; node = la; 
	}

	void While(out DNode node) {
		node = null; 
		Expect(16);
		var @while = new DWhile(t); 
		Expr(out node);
		@while.Condition = node; 
		Block(out node);
		@while.Body = node;
		node = @while;
		
	}

	void For(out DNode node) {
		node = null; 
		Expect(15);
		var @for = new DFor(t); 
		OrPattern(out var pattern);
		@for.Pattern = pattern; 
		Expect(63);
		Expr(out node);
		@for.Target = node; 
		if (la.kind == 53) {
			Get();
			Expr(out node);
			@for.Guard = node; 
		}
		Block(out node);
		@for.Body = node;
		node = @for;
		
	}

	void Break(out DNode node) {
		Expect(12);
		var br = new DBreak(t); node = br; 
		if (StartOf(6)) {
			if (la.AfterEol) return; 
			Expr(out var exp);
			br.Expression = exp; 
		}
	}

	void Continue(out DNode node) {
		Expect(11);
		node = new DContinue(t); 
	}

	void Return(out DNode node) {
		Expect(10);
		var br = new DReturn(t); node = br; 
		if (StartOf(6)) {
			Expr(out var exp);
			br.Expression = exp; 
		}
	}

	void Yield(out DNode node) {
		Expect(13);
		var yield = new DYield(t);
		node = yield;
		functions.Peek().IsIterator = true; 
		
		Expr(out var exp);
		yield.Expression = exp; 
	}

	void FunctionExpr(out DNode node) {
		var f = new DFunctionDeclaration(t);
		node = f;
		
		if (la.kind == 1) {
			FunctionArgument(out var a);
			f.Parameters.Add(a); 
		} else if (la.kind == 25) {
			FunctionArguments(f);
		} else SynErr(91);
		functions.Push(f); 
		Expect(19);
		Expr(out var exp);
		f.Body = exp; 
	}

	void Label(out DNode node) {
		node = null; var name = ""; 
		Expect(1);
		name = t.val; 
		Expect(23);
		var ot = t; 
		Coalesce(out node);
		node = new DLabelLiteral(ot) { Label = name, Expression = node }; 
	}

	void Assignment(out DNode node) {
		Coalesce(out node);
		if (StartOf(8)) {
			var ass = new DAssignment(t) { Target = node };
			node = ass;
			BinaryOperator? op = null;
			
			switch (la.kind) {
			case 24: {
				Get();
				break;
			}
			case 67: {
				Get();
				op = BinaryOperator.Add; 
				break;
			}
			case 68: {
				Get();
				op = BinaryOperator.Sub; 
				break;
			}
			case 69: {
				Get();
				op = BinaryOperator.Mul; 
				break;
			}
			case 70: {
				Get();
				op = BinaryOperator.Div; 
				break;
			}
			case 71: {
				Get();
				op = BinaryOperator.Rem; 
				break;
			}
			case 72: {
				Get();
				op = BinaryOperator.And; 
				break;
			}
			case 73: {
				Get();
				op = BinaryOperator.Or; 
				break;
			}
			case 74: {
				Get();
				op = BinaryOperator.Xor; 
				break;
			}
			case 75: {
				Get();
				op = BinaryOperator.ShiftLeft; 
				break;
			}
			case 76: {
				Get();
				op = BinaryOperator.ShiftRight; 
				break;
			}
			}
			Expr(out node);
			ass.Value = node;
			ass.AutoAssign = op;
			node = ass;
			
		}
	}

	void TryCatch(out DNode node) {
		node =  null; 
		Expect(65);
		var tc = new DTryCatch(t); 
		Block(out node);
		tc.Expression = node; 
		Expect(66);
		if (la.kind == 27) {
			var m = new DMatch(t); tc.Catch = m; 
			Get();
			MatchEntry(out var entry);
			m.Entries.Add(entry); 
			while (la.kind == 21) {
				Get();
				MatchEntry(out entry);
				m.Entries.Add(entry); 
			}
			Expect(28);
		} else if (la.kind == 1) {
			Get();
			tc.BindVariable = new DName(t) { Value = t.val }; 
			Block(out node);
			tc.Catch = node; 
		} else SynErr(92);
		node = tc; 
	}

	void Throw(out DNode node) {
		node = null; 
		Expect(64);
		var th = new DThrow(t); 
		Expr(out node);
		th.Expression = node; node = th; 
	}

	void Coalesce(out DNode node) {
		Or(out node);
		while (la.kind == 77) {
			Get();
			var ot = t; 
			Or(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Coalesce, ot); 
		}
	}

	void Or(out DNode node) {
		And(out node);
		while (la.kind == 54) {
			Get();
			var ot = t; 
			And(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Or, ot); 
		}
	}

	void And(out DNode node) {
		Eq(out node);
		while (la.kind == 55) {
			Get();
			var ot = t; 
			Eq(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.And, ot); 
		}
	}

	void Eq(out DNode node) {
		Shift(out node);
		while (StartOf(9)) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			switch (la.kind) {
			case 42: {
				Get();
				ot = t; op = BinaryOperator.Gt; 
				break;
			}
			case 43: {
				Get();
				ot = t; op = BinaryOperator.Lt; 
				break;
			}
			case 44: {
				Get();
				ot = t; op = BinaryOperator.GtEq; 
				break;
			}
			case 45: {
				Get();
				ot = t; op = BinaryOperator.LtEq; 
				break;
			}
			case 40: {
				Get();
				ot = t; op = BinaryOperator.Eq; 
				break;
			}
			case 41: {
				Get();
				ot = t; op = BinaryOperator.NotEq; 
				break;
			}
			}
			Shift(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Shift(out DNode node) {
		BitOr(out node);
		while (la.kind == 47 || la.kind == 48) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 47) {
				Get();
				ot = t; op = BinaryOperator.ShiftLeft; 
			} else {
				Get();
				ot = t; op = BinaryOperator.ShiftRight; 
			}
			BitOr(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void BitOr(out DNode node) {
		Xor(out node);
		while (la.kind == 38) {
			Get();
			var ot = t; 
			Xor(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseOr, ot); 
		}
	}

	void Xor(out DNode node) {
		BitAnd(out node);
		while (la.kind == 46) {
			DNode exp = null; 
			Get();
			var ot = t; 
			BitAnd(out exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Xor, ot); 
		}
	}

	void BitAnd(out DNode node) {
		Add(out node);
		while (la.kind == 39) {
			Get();
			var ot = t; 
			Add(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseAnd, ot); 
		}
	}

	void Add(out DNode node) {
		Mul(out node);
		while (la.kind == 31 || la.kind == 32) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 32) {
				if (la.AfterEol) return; 
				Get();
				ot = t; op = BinaryOperator.Add; 
			} else {
				if (la.AfterEol) return; 
				Get();
				ot = t; op = BinaryOperator.Sub; 
			}
			Mul(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Mul(out DNode node) {
		Unary(out node);
		while (la.kind == 35 || la.kind == 36 || la.kind == 37) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 35) {
				Get();
				ot = t; op = BinaryOperator.Mul; 
			} else if (la.kind == 36) {
				Get();
				ot = t; op = BinaryOperator.Div; 
			} else {
				Get();
				ot = t; op = BinaryOperator.Rem; 
			}
			Unary(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Unary(out DNode node) {
		node = null;
		var op = default(UnaryOperator);
		var ot = default(Token);
		
		if (la.kind == 33) {
			Get();
			ot = t; op = UnaryOperator.Not; 
			Range(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 31) {
			Get();
			ot = t; op = UnaryOperator.Neg; 
			Range(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 32) {
			Get();
			ot = t; op = UnaryOperator.Plus; 
			Range(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 34) {
			Get();
			ot = t; op = UnaryOperator.BitwiseNot; 
			Range(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (StartOf(10)) {
			Range(out node);
		} else SynErr(93);
	}

	void Range(out DNode node) {
		node = null; 
		FieldOrIndex(out node);
		if (la.kind == 56) {
			Get();
			var range = new DRange(t) { From = node }; 
			FieldOrIndex(out node);
			range.To = node; node = range; 
		}
	}

	void FieldOrIndex(out DNode node) {
		Literal(out node);
		while (la.kind == 20 || la.kind == 25 || la.kind == 29) {
			if (la.kind == 20) {
				Get();
				var ot = t; 
				Expect(1);
				var nm = t.val; DMemberCheck chk = null; 
				if (la.kind == 58) {
					Get();
					chk = new DMemberCheck(ot) { Target = node };
					chk.Name = nm;
					node = chk;
					
				}
				if (chk == null) 
				{
				   var fld = new DAccess(ot) { Target = node };
				   fld.Name = nm;
				   node = fld;
				}
				
			} else if (la.kind == 29) {
				if (la.AfterEol) return; 
				Get();
				var idx = new DIndexer(t) { Target = node }; 
				Expr(out node);
				idx.Index = node;
				node = idx;
				
				Expect(30);
			} else {
				if (la.AfterEol) return;
				var app = new DApplication(node, t); 
				
				Get();
				if (StartOf(6)) {
					ApplicationArguments(app);
				}
				node = app; 
				Expect(26);
			}
		}
	}

	void Literal(out DNode node) {
		node = null; 
		if (la.kind == 1) {
			Name(out node);
		} else if (la.kind == 6) {
			SpecialName(out node);
		} else if (la.kind == 2) {
			Integer(out node);
		} else if (la.kind == 3) {
			Float(out node);
		} else if (la.kind == 4) {
			String(out node);
		} else if (la.kind == 5) {
			Char(out node);
		} else if (la.kind == 60 || la.kind == 61) {
			Bool(out node);
		} else if (la.kind == 59) {
			Nil(out node);
		} else if (IsTuple()) {
			Tuple(out node);
		} else if (la.kind == 29) {
			Array(out node);
		} else if (la.kind == 25) {
			Group(out node);
		} else if (la.kind == 78) {
			Base(out node);
		} else SynErr(94);
	}

	void ApplicationArguments(DApplication app) {
		var node = default(DNode); 
		Expr(out node);
		app.Arguments.Add(node); 
		while (la.kind == 21) {
			Get();
			Expr(out node);
			app.Arguments.Add(node); 
		}
	}

	void Name(out DNode node) {
		Expect(1);
		node = new DName(t) { Value = t.val }; 
	}

	void SpecialName(out DNode node) {
		Expect(6);
		var nm = int.Parse(t.val.Substring(1));
		node = new DName(t) { Value = "p" + nm };
		if (implicits == null)
		   implicits = new List<int>();
		implicits.Add(nm);
		
	}

	void Integer(out DNode node) {
		Expect(2);
		node = new DIntegerLiteral(t) { Value = ParseInteger() }; 
	}

	void Float(out DNode node) {
		Expect(3);
		node = new DFloatLiteral(t) { Value = ParseFloat() }; 
	}

	void String(out DNode node) {
		Expect(4);
		node = ParseString(); 
	}

	void Char(out DNode node) {
		Expect(5);
		node = new DCharLiteral(t) { Value = ParseChar() }; 
	}

	void Bool(out DNode node) {
		if (la.kind == 60) {
			Get();
		} else if (la.kind == 61) {
			Get();
		} else SynErr(95);
		node = new DBooleanLiteral(t) { Value = t.val == "true" }; 
	}

	void Nil(out DNode node) {
		Expect(59);
		node = new DNilLiteral(t); 
	}

	void Tuple(out DNode node) {
		node = null; 
		Expect(25);
		var tup = new DTupleLiteral(t); 
		Expr(out node);
		tup.Elements.Add(node); 
		while (la.kind == 21) {
			Get();
			Expr(out node);
			tup.Elements.Add(node); 
		}
		node = tup; 
		Expect(26);
	}

	void Array(out DNode node) {
		node = null; 
		Expect(29);
		var arr = new DArrayLiteral(t); 
		if (StartOf(6)) {
			Expr(out node);
			arr.Elements.Add(node); 
			while (la.kind == 21) {
				Get();
				Expr(out node);
				arr.Elements.Add(node); 
			}
		}
		node = arr; 
		Expect(30);
	}

	void Group(out DNode node) {
		node = null; 
		Expect(25);
		Expr(out node);
		Expect(26);
	}

	void Base(out DNode node) {
		Expect(78);
		node = new DBase(t); 
	}

	void DyalectItem() {
		if (StartOf(7)) {
			Statement(out var node);
			Root.Nodes.Add(node); 
		} else if (la.kind == 49) {
			Import();
			Separator();
		} else SynErr(96);
	}

	void Dyalect() {
		DyalectItem();
		while (StartOf(11)) {
			DyalectItem();
		}
	}



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
		Dyalect();
		Expect(0);

        }

        static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x}

        };

        private void SynErr(int line, int col, int n)
        {
            string s;

            switch (n)
            {
			case 0: s = "EOF expected"; break;
			case 1: s = "identToken expected"; break;
			case 2: s = "intToken expected"; break;
			case 3: s = "floatToken expected"; break;
			case 4: s = "stringToken expected"; break;
			case 5: s = "charToken expected"; break;
			case 6: s = "implicitToken expected"; break;
			case 7: s = "varToken expected"; break;
			case 8: s = "constToken expected"; break;
			case 9: s = "funcToken expected"; break;
			case 10: s = "returnToken expected"; break;
			case 11: s = "continueToken expected"; break;
			case 12: s = "breakToken expected"; break;
			case 13: s = "yieldToken expected"; break;
			case 14: s = "ifToken expected"; break;
			case 15: s = "forToken expected"; break;
			case 16: s = "whileToken expected"; break;
			case 17: s = "doToken expected"; break;
			case 18: s = "typeToken expected"; break;
			case 19: s = "arrowToken expected"; break;
			case 20: s = "dotToken expected"; break;
			case 21: s = "commaToken expected"; break;
			case 22: s = "semicolonToken expected"; break;
			case 23: s = "colonToken expected"; break;
			case 24: s = "equalToken expected"; break;
			case 25: s = "parenLeftToken expected"; break;
			case 26: s = "parenRightToken expected"; break;
			case 27: s = "curlyLeftToken expected"; break;
			case 28: s = "curlyRightToken expected"; break;
			case 29: s = "squareLeftToken expected"; break;
			case 30: s = "squareRightToken expected"; break;
			case 31: s = "minus expected"; break;
			case 32: s = "plus expected"; break;
			case 33: s = "not expected"; break;
			case 34: s = "bitnot expected"; break;
			case 35: s = "\"*\" expected"; break;
			case 36: s = "\"/\" expected"; break;
			case 37: s = "\"%\" expected"; break;
			case 38: s = "\"|\" expected"; break;
			case 39: s = "\"&\" expected"; break;
			case 40: s = "\"==\" expected"; break;
			case 41: s = "\"!=\" expected"; break;
			case 42: s = "\">\" expected"; break;
			case 43: s = "\"<\" expected"; break;
			case 44: s = "\">=\" expected"; break;
			case 45: s = "\"<=\" expected"; break;
			case 46: s = "\"^\" expected"; break;
			case 47: s = "\"<<\" expected"; break;
			case 48: s = "\">>\" expected"; break;
			case 49: s = "\"import\" expected"; break;
			case 50: s = "\"static\" expected"; break;
			case 51: s = "\"...\" expected"; break;
			case 52: s = "\"match\" expected"; break;
			case 53: s = "\"when\" expected"; break;
			case 54: s = "\"||\" expected"; break;
			case 55: s = "\"&&\" expected"; break;
			case 56: s = "\"..\" expected"; break;
			case 57: s = "\"as\" expected"; break;
			case 58: s = "\"?\" expected"; break;
			case 59: s = "\"nil\" expected"; break;
			case 60: s = "\"true\" expected"; break;
			case 61: s = "\"false\" expected"; break;
			case 62: s = "\"else\" expected"; break;
			case 63: s = "\"in\" expected"; break;
			case 64: s = "\"throw\" expected"; break;
			case 65: s = "\"try\" expected"; break;
			case 66: s = "\"catch\" expected"; break;
			case 67: s = "\"+=\" expected"; break;
			case 68: s = "\"-=\" expected"; break;
			case 69: s = "\"*=\" expected"; break;
			case 70: s = "\"/=\" expected"; break;
			case 71: s = "\"%=\" expected"; break;
			case 72: s = "\"&=\" expected"; break;
			case 73: s = "\"|=\" expected"; break;
			case 74: s = "\"^=\" expected"; break;
			case 75: s = "\"<<=\" expected"; break;
			case 76: s = "\">>=\" expected"; break;
			case 77: s = "\"??\" expected"; break;
			case 78: s = "\"base\" expected"; break;
			case 79: s = "??? expected"; break;
			case 80: s = "invalid StandardOperators"; break;
			case 81: s = "invalid FunctionName"; break;
			case 82: s = "invalid Statement"; break;
			case 83: s = "invalid ControlFlow"; break;
			case 84: s = "invalid SimpleExpr"; break;
			case 85: s = "invalid If"; break;
			case 86: s = "invalid Loops"; break;
			case 87: s = "invalid Expr"; break;
			case 88: s = "invalid Binding"; break;
			case 89: s = "invalid Pattern"; break;
			case 90: s = "invalid BooleanPattern"; break;
			case 91: s = "invalid FunctionExpr"; break;
			case 92: s = "invalid TryCatch"; break;
			case 93: s = "invalid Unary"; break;
			case 94: s = "invalid Literal"; break;
			case 95: s = "invalid Bool"; break;
			case 96: s = "invalid DyalectItem"; break;

                default:
                    s = "unknown " + n;
                    break;
            }

            AddError(s, line, col);
        }
    }
}
