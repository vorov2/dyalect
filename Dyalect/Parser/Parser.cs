
#nullable disable
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
	public const int _directive = 2;
	public const int _intToken = 3;
	public const int _floatToken = 4;
	public const int _stringToken = 5;
	public const int _charToken = 6;
	public const int _implicitToken = 7;
	public const int _verbatimStringToken = 8;
	public const int _autoToken = 9;
	public const int _varToken = 10;
	public const int _letToken = 11;
	public const int _funcToken = 12;
	public const int _returnToken = 13;
	public const int _privateToken = 14;
	public const int _continueToken = 15;
	public const int _breakToken = 16;
	public const int _yieldToken = 17;
	public const int _ifToken = 18;
	public const int _forToken = 19;
	public const int _whileToken = 20;
	public const int _typeToken = 21;
	public const int _inToken = 22;
	public const int _arrowToken = 23;
	public const int _dotToken = 24;
	public const int _commaToken = 25;
	public const int _semicolonToken = 26;
	public const int _colonToken = 27;
	public const int _equalToken = 28;
	public const int _parenLeftToken = 29;
	public const int _parenRightToken = 30;
	public const int _curlyLeftToken = 31;
	public const int _curlyRightToken = 32;
	public const int _squareLeftToken = 33;
	public const int _squareRightToken = 34;
	public const int _eq_coa = 35;
	public const int _eq_add = 36;
	public const int _eq_sub = 37;
	public const int _eq_mul = 38;
	public const int _eq_div = 39;
	public const int _eq_rem = 40;
	public const int _eq_and = 41;
	public const int _eq_or = 42;
	public const int _eq_xor = 43;
	public const int _eq_lsh = 44;
	public const int _eq_rsh = 45;
	public const int _minus = 46;
	public const int _plus = 47;
	public const int _not = 48;
	public const int _bitnot = 49;
	public const int _coalesce = 50;
	public const int maxT = 100;




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
		Expect(26);
	}

	void StandardOperators() {
		switch (la.kind) {
		case 47: {
			Get();
			break;
		}
		case 46: {
			Get();
			break;
		}
		case 51: {
			Get();
			break;
		}
		case 52: {
			Get();
			break;
		}
		case 53: {
			Get();
			break;
		}
		case 54: {
			Get();
			break;
		}
		case 55: {
			Get();
			break;
		}
		case 48: {
			Get();
			break;
		}
		case 56: {
			Get();
			break;
		}
		case 57: {
			Get();
			break;
		}
		case 58: {
			Get();
			break;
		}
		case 59: {
			Get();
			break;
		}
		case 60: {
			Get();
			break;
		}
		case 61: {
			Get();
			break;
		}
		case 62: {
			Get();
			break;
		}
		case 63: {
			Get();
			break;
		}
		case 64: {
			Get();
			break;
		}
		case 65: {
			Get();
			break;
		}
		default: SynErr(101); break;
		}
	}

	void FunctionName() {
		if (la.kind == 1) {
			Get();
		} else if (StartOf(1)) {
			StandardOperators();
		} else SynErr(102);
	}

	void Qualident(out string s1, out string s2, out string s3) {
		s1 = null; s2 = null; s3 = null; 
		FunctionName();
		s1 = t.val; 
		if (StartOf(2)) {
			if (la.kind == 24) {
				Get();
			}
			FunctionName();
			s2 = t.val; 
			if (StartOf(2)) {
				if (la.kind == 24) {
					Get();
				}
				FunctionName();
				s3 = t.val; 
			}
		}
	}

	void ImportToken(out string str) {
		str = ""; 
		if (la.kind == 5) {
			Get();
			str = ParseSimpleString(); 
		} else if (la.kind == 1) {
			Get();
			str = t.val; 
			while (la.kind == 24) {
				Get();
				Expect(1);
				str += string.Concat(".", t.val); 
			}
		} else SynErr(103);
	}

	void TestBlock(out DNode node) {
		string name = null; node = null; 
		Expect(66);
		var ot = t; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
		} else if (la.kind == 5) {
			Get();
			name = ParseSimpleString(); 
		} else SynErr(104);
		var block = new DBlock(default);
		var imports = new List<DImport>();
		
		while (StartOf(3)) {
			if (StartOf(4)) {
				Statement(out node);
				block.Nodes.Add(node);
				
			} else {
				Import(out var imp);
				imports.Add(imp); 
				Separator();
			}
		}
		Expect(67);
		var dyc = new DyCodeModel(block, imports.ToArray(), FileName);
		node = new DTestBlock(ot) { Name = name, Body = dyc };
		
	}

	void Statement(out DNode node) {
		node = null; 
		if (StartOf(5)) {
			Binding(out node);
			if (la.kind == 71) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 78) {
			Rebinding(out node);
			if (la.kind == 71) {
				Guard(node, out node);
			}
			Separator();
		} else if (StartOf(6)) {
			ControlFlow(out node);
			if (la.kind == 71) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 21) {
			Type(out node);
			Separator();
		} else if (la.kind == 82) {
			Match(out node);
			if (la.kind == 71) {
				Guard(node, out node);
				Separator();
			}
		} else if (IsPrivateScope()) {
			PrivateScope(out node);
		} else if (StartOf(7)) {
			StatementExpr(out node);
			if (la.kind == 71) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 18) {
			If(out node);
			if (la.kind == 71) {
				Guard(node, out node);
				Separator();
			}
		} else if (la.kind == 19 || la.kind == 20 || la.kind == 87) {
			Loops(out node);
			if (la.kind == 71) {
				Guard(node, out node);
				Separator();
			}
		} else if (la.kind == 12 || la.kind == 75) {
			Function(out node);
		} else if (la.kind == 2) {
			Directive(out node);
			Separator();
		} else if (la.kind == 66) {
			TestBlock(out node);
		} else SynErr(105);
		node = ProcessImplicits(node); 
	}

	void Import(out DImport node) {
		Expect(68);
		var inc = new DImport(t); node = inc; 
		ImportToken(out var str);
		var lastName = str; 
		if (la.kind == 28) {
			Get();
			ImportToken(out str);
			inc.Alias = lastName;
			lastName = str;
			
		}
		while (la.kind == 52) {
			Get();
			ImportToken(out str);
			if (inc.LocalPath != null)
			   inc.LocalPath = string.Concat(inc.LocalPath, "/", lastName);
			else
			   inc.LocalPath = lastName;
			lastName = str;
			
		}
		inc.ModuleName = lastName; 
	}

	void Type(out DNode node) {
		DFunctionDeclaration f = null; 
		Expect(21);
		var typ = new DTypeDeclaration(t);
		node = typ;
		
		Expect(1);
		typ.Name = t.val; node = typ; 
		if (la.kind == 28 || la.kind == 29) {
			if (la.kind == 29) {
				f = new DFunctionDeclaration(t) { Name = typ.Name, IsStatic = true, IsConstructor = true, TypeName = new Qualident(typ.Name) }; 
				TypeArguments(f);
				typ.Constructors.Add(f); 
			} else {
				Get();
				var priv = false; 
				if (la.kind == 14) {
					Get();
					priv = true; 
				}
				Expect(1);
				f = new DFunctionDeclaration(t) { Name = t.val, IsPrivate = priv, IsStatic = true, IsConstructor = true, TypeName = new Qualident(typ.Name) }; typ.Constructors.Add(f); 
				TypeArguments(f);
				while (la.kind == 69) {
					Get();
					if (la.kind == 14) {
						Get();
						priv = true; 
					}
					Expect(1);
					f = new DFunctionDeclaration(t) { Name = t.val, IsPrivate = priv, IsStatic = true, IsConstructor = true, TypeName = new Qualident(typ.Name) }; typ.Constructors.Add(f);
					TypeArguments(f);
				}
			}
		}
	}

	void TypeArguments(DFunctionDeclaration node) {
		Expect(29);
		if (la.kind == 1 || la.kind == 10 || la.kind == 11) {
			TypeArgument(out var arg);
			node.Parameters.Add(arg); 
			while (la.kind == 25) {
				Get();
				TypeArgument(out arg);
				node.Parameters.Add(arg); 
			}
		}
		Expect(30);
	}

	void TypeArgument(out DParameter arg) {
		arg = null; var mut = false; 
		if (la.kind == 10 || la.kind == 11) {
			if (la.kind == 10) {
				Get();
				mut = true; 
			} else {
				Get();
			}
		}
		Expect(1);
		var ot = t; string str = t.val; 
		if (la.kind == 1 || la.kind == 24) {
			if (la.kind == 1) {
				Get();
				arg = new DTypeParameter(ot) { Name = t.val, TypeAnnotation = new Qualident(str), Mutable = mut}; 
			} else {
				Get();
				Expect(1);
				var ta = new Qualident(t.val, str); 
				Expect(1);
				arg = new DTypeParameter(ot) { Name = t.val, TypeAnnotation = ta, Mutable = mut }; 
			}
		}
		if (arg is null)
		   arg = new DTypeParameter(ot) { Name = str, Mutable = mut };
		
		if (la.kind == 28) {
			Get();
			Expr(out var cnode);
			arg.DefaultValue = cnode; 
		}
		if (la.kind == 70) {
			Get();
			arg.IsVarArgs = true; 
		}
	}

	void Expr(out DNode node) {
		node = null; 
		if (la.kind == 18) {
			If(out node);
		} else if (StartOf(7)) {
			SimpleExpr(out node);
		} else if (la.kind == 19 || la.kind == 20 || la.kind == 87) {
			Loops(out node);
		} else if (la.kind == 82) {
			Match(out node);
		} else SynErr(106);
		node = ProcessImplicits(node); 
	}

	void Binding(out DNode node) {
		node = null; 
		if (StartOf(8)) {
			if (la.kind == 10) {
				Get();
			} else if (la.kind == 11) {
				Get();
			} else if (la.kind == 80) {
				Get();
				Deprecated("const"); 
			} else {
				Get();
			}
			var bin = new DBinding(t) { Constant = t.val == "let", Lazy = t.val == "lazy" }; 
			OrPattern(out var pat);
			bin.Pattern = pat; 
			if (la.kind == 28) {
				Get();
				Expr(out node);
				bin.Init = node; 
			}
			node = bin; 
		} else if (la.kind == 9) {
			Get();
			var bin = new DBinding(t) { AutoClose = true, Constant = true }; 
			NamePattern(out var pat);
			bin.Pattern = pat; 
			Expect(28);
			Expr(out node);
			bin.Init = node; node = bin; 
		} else SynErr(107);
	}

	void Guard(DNode src, out DNode node) {
		node = src; 
		var ot = t; 
		Expect(71);
		SimpleExpr(out var cnode);
		var @if = new DIf(ot) { Condition = cnode, True = src }; node = @if; 
		if (la.kind == 72) {
			Get();
			StatementExpr(out cnode);
			@if.False = cnode; 
		}
	}

	void Rebinding(out DNode node) {
		Expect(78);
		var bin = new DRebinding(t); 
		OrPattern(out var pat);
		bin.Pattern = pat; 
		Expect(28);
		Expr(out node);
		bin.Init = node; 
		node = bin; 
	}

	void ControlFlow(out DNode node) {
		node = null; 
		if (la.kind == 16) {
			Break(out node);
		} else if (la.kind == 15) {
			Continue(out node);
		} else if (la.kind == 13) {
			Return(out node);
		} else if (la.kind == 17) {
			Yield(out node);
		} else SynErr(108);
	}

	void Match(out DNode node) {
		node = null; 
		Expect(82);
		var m = new DMatch(t); 
		Expr(out node);
		m.Expression = node; 
		Expect(31);
		MatchEntry(out var entry);
		m.Entries.Add(entry); 
		while (la.kind == 25) {
			Get();
			MatchEntry(out entry);
			m.Entries.Add(entry); 
		}
		Expect(32);
		node = m; 
	}

	void PrivateScope(out DNode node) {
		node = null; 
		Expect(14);
		var ot = t; 
		Block(out node);
		node = new DPrivateScope(ot) { Block = (DBlock)node };
		
	}

	void StatementExpr(out DNode node) {
		SimpleExpr(out node);
		if (StartOf(9)) {
			var ass = new DAssignment(t) { Target = node };
			node = ass;
			BinaryOperator? op = null;
			
			switch (la.kind) {
			case 28: {
				Get();
				break;
			}
			case 35: {
				Get();
				op = BinaryOperator.Coalesce; 
				break;
			}
			case 36: {
				Get();
				op = BinaryOperator.Add; 
				break;
			}
			case 37: {
				Get();
				op = BinaryOperator.Sub; 
				break;
			}
			case 38: {
				Get();
				op = BinaryOperator.Mul; 
				break;
			}
			case 39: {
				Get();
				op = BinaryOperator.Div; 
				break;
			}
			case 40: {
				Get();
				op = BinaryOperator.Rem; 
				break;
			}
			case 41: {
				Get();
				op = BinaryOperator.And; 
				break;
			}
			case 42: {
				Get();
				op = BinaryOperator.Or; 
				break;
			}
			case 43: {
				Get();
				op = BinaryOperator.Xor; 
				break;
			}
			case 44: {
				Get();
				op = BinaryOperator.ShiftLeft; 
				break;
			}
			case 45: {
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

	void If(out DNode node) {
		node = null; 
		Expect(18);
		var @if = new DIf(t); 
		Expr(out node);
		@if.Condition = node; 
		Block(out node);
		@if.True = node; 
		if (la.kind == 72) {
			Get();
			if (la.kind == 31) {
				Block(out node);
				@if.False = node; 
			} else if (la.kind == 18) {
				If(out node);
				@if.False = node; 
			} else SynErr(109);
		}
		node = @if; 
	}

	void Loops(out DNode node) {
		node = null; 
		if (la.kind == 20) {
			While(out node);
		} else if (la.kind == 19) {
			For(out node);
		} else if (la.kind == 87) {
			DoWhile(out node);
			Separator();
		} else SynErr(110);
	}

	void Function(out DNode node) {
		node = null; var st = false; DRecursiveBlock rec = null; 
		if (la.kind == 75) {
			Get();
			st = true; 
		}
		Expect(12);
		FunctionBody(st, out node);
		while (la.kind == 76) {
			if (rec is null)
			{
			   rec = new DRecursiveBlock(t);
			   rec.Functions.Add((DFunctionDeclaration)node);
			}
			
			Get();
			FunctionBody(false, out node);
			rec.Functions.Add((DFunctionDeclaration)node); 
		}
		node = rec ?? node; 
	}

	void Directive(out DNode node) {
		Expect(2);
		var pp = new DDirective(t) { Key = t.val.Substring(1) }; node = pp; 
		while (StartOf(10)) {
			if (la.AfterEol) return; 
			switch (la.kind) {
			case 5: {
				Get();
				pp.Attributes.Add(ParseSimpleString()); 
				break;
			}
			case 3: {
				Get();
				pp.Attributes.Add(ParseInteger()); 
				break;
			}
			case 4: {
				Get();
				pp.Attributes.Add(ParseFloat()); 
				break;
			}
			case 6: {
				Get();
				pp.Attributes.Add(ParseChar()); 
				break;
			}
			case 73: {
				Get();
				pp.Attributes.Add(true); 
				break;
			}
			case 74: {
				Get();
				pp.Attributes.Add(false); 
				break;
			}
			case 1: {
				Get();
				pp.Attributes.Add(t.val); 
				break;
			}
			}
		}
	}

	void SimpleExpr(out DNode node) {
		node = null; 
		if (IsFunction()) {
			FunctionExpr(out node);
		} else if (la.kind == 23) {
			NoArgFunctionExpr(out node);
		} else if (StartOf(11)) {
			Coalesce(out node);
			if (la.kind == 89) {
				TernaryExpr(node, out node);
			}
		} else if (la.kind == 91) {
			TryCatch(out node);
		} else if (la.kind == 90) {
			Throw(out node);
		} else if (la.kind == 31) {
			Block(out node);
		} else SynErr(111);
	}

	void Block(out DNode node) {
		node = null; 
		Expect(31);
		var block = new DBlock(t); 
		if (StartOf(4)) {
			Statement(out node);
			block.Nodes.Add(node); 
			while (StartOf(4)) {
				Statement(out node);
				block.Nodes.Add(node); 
			}
		}
		node = block; 
		Expect(32);
	}

	void FunctionBody(bool st, out DNode node) {
		node = null; var get = false; var set = false; 
		if (la.kind == 77 || la.kind == 78) {
			if (la.kind == 77) {
				Get();
				get = true; 
			} else {
				Get();
				set = true; 
			}
		}
		var f = new DFunctionDeclaration(t) { IsStatic = st, Getter = get, Setter = set };
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
		   f.TypeName = new Qualident(s2, s1);
		}
		
		if (la.kind == 33) {
			Get();
			f.IsIndexer = true;
			
			if (la.kind == 1) {
				FunctionArgument(out var arg);
				f.Parameters.Add(arg); 
				while (la.kind == 25) {
					Get();
					FunctionArgument(out arg);
					f.Parameters.Add(arg); 
				}
			}
			if (f.TypeName is null)
			   f.TypeName = new Qualident(f.Name);
			else
			   f.TypeName = new Qualident(f.Name, f.TypeName.Local);
			
			f.Name = set ? "__op_set" : "__op_get";
			
			Expect(34);
		} else if (la.kind == 29) {
			FunctionArguments(f);
		} else if (la.kind == 79) {
			Get();
			if (f.TypeName is not null && f.TypeName.Parent is not null)
			   AddError(ParserError.InvalidTypeName, t.GetLocation());
			else if (f.TypeName is not null)
			   f.TypeName = new Qualident(f.TypeName.Local, f.Name);
			else
			   f.TypeName = new Qualident(f.Name);
			f.Name = null;
			
			string str1, str2 = null;
			
			Expect(1);
			str1 = t.val; 
			if (la.kind == 24) {
				Get();
				Expect(1);
				str2 = t.val; 
			}
			f.TargetTypeName = str2 is null ? new Qualident(str1) : new Qualident(str1, str2);
			
		} else SynErr(112);
		if (la.kind == 31) {
			Block(out node);
		} else if (la.kind == 23) {
			Get();
			Statement(out node);
		} else SynErr(113);
		f.Body = node;
		node = f;
		functions.Pop();
		
	}

	void FunctionArgument(out DParameter arg) {
		arg = null; 
		Expect(1);
		var ot = t; string str = t.val; 
		if (la.kind == 1 || la.kind == 24) {
			if (la.kind == 1) {
				Get();
				arg = new DParameter(ot) { Name = t.val, TypeAnnotation = new Qualident(str) }; 
			} else {
				Get();
				Expect(1);
				var ta = new Qualident(t.val, str); 
				Expect(1);
				arg = new DParameter(ot) { Name = t.val, TypeAnnotation = ta }; 
			}
		}
		if (arg is null)
		   arg = new DParameter(ot) { Name = str };
		
		if (la.kind == 28) {
			Get();
			Expr(out var cnode);
			arg.DefaultValue = cnode; 
		}
		if (la.kind == 70) {
			Get();
			arg.IsVarArgs = true; 
		}
	}

	void FunctionArguments(DFunctionDeclaration node) {
		Expect(29);
		if (la.kind == 1) {
			FunctionArgument(out var arg);
			node.Parameters.Add(arg); 
			while (la.kind == 25) {
				Get();
				FunctionArgument(out arg);
				node.Parameters.Add(arg); 
			}
		}
		Expect(30);
	}

	void OrPattern(out DPattern node) {
		node = null; 
		AndPattern(out node);
		while (la.kind == 69) {
			var por = new DOrPattern(t) { Left = node }; 
			Get();
			AndPattern(out node);
			por.Right = node; node = por; 
		}
	}

	void NamePattern(out DPattern node) {
		node = null; Token ot = null; string mod = null; string typ = null; string nam = null;
		Expect(1);
		nam = t.val; ot = t; 
		if (la.kind == 24) {
			Get();
			Expect(1);
			typ = nam; nam = t.val; 
			if (la.kind == 24) {
				Get();
				Expect(1);
				mod = typ; typ = nam; nam = t.val; 
			}
		}
		if (la.kind == 29) {
			var ctor = new DCtorPattern(t);
			if (mod is null && typ is null)
			   ctor.Constructor = nam;
			else if (mod is null) {
			   ctor.TypeName = new Qualident(typ);
			   ctor.Constructor = nam;
			}
			else {
			   ctor.TypeName = new Qualident(typ, mod);
			   ctor.Constructor = nam;
			}
			
			Get();
			if (StartOf(12)) {
				if (IsLabelPattern()) {
					LabelPattern(out node);
				} else {
					OrPattern(out node);
				}
				ctor.Arguments.Add(node); 
			}
			while (la.kind == 25) {
				Get();
				if (IsLabelPattern()) {
					LabelPattern(out node);
				} else if (StartOf(12)) {
					OrPattern(out node);
				} else SynErr(114);
				ctor.Arguments.Add(node); 
			}
			Expect(30);
			node = ctor; 
		}
		if (node is null)
		{
		   if (mod is not null && typ is not null && nam is not null)
		       AddError(ParserError.InvalidPattern, t.GetLocation());
		   else if (mod is not null && typ is not null && nam is null)
		   {
		       node = new DTypeTestPattern(ot) {
		           TypeName = typ is null ? new Qualident(nam) : new Qualident(typ, mod) 
		       };
		   }
		   else if (nam is not null)
		   {
		       if (nam == "_")
		           node = new DWildcardPattern(ot);
		       else if (char.IsUpper(nam[0]))
		           node = new DTypeTestPattern(ot) { TypeName = new Qualident(nam) };
		       else
		           node = new DNamePattern(ot) { Name = nam };
		   }
		}
		
	}

	void MatchEntry(out DMatchEntry me) {
		me = new DMatchEntry(t); 
		OrPattern(out var p);
		me.Pattern = p; 
		if (la.kind == 71) {
			Get();
			Expr(out var node);
			me.Guard = node; 
		}
		Expect(23);
		StatementExpr(out var exp);
		me.Expression = exp; 
	}

	void AndPattern(out DPattern node) {
		node = null; 
		RangePattern(out node);
		while (la.kind == 76) {
			var pa = new DAndPattern(t) { Left = node }; 
			Get();
			RangePattern(out node);
			pa.Right = node; node = pa; 
		}
	}

	void RangePattern(out DPattern node) {
		node = null; 
		Pattern(out node);
		if (la.kind == 83) {
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
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 3) {
			IntegerPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 4) {
			FloatPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 6) {
			CharPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 5) {
			StringPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 73 || la.kind == 74) {
			BooleanPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 86) {
			NilPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (IsTuple(allowFields: true)) {
			TuplePattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 29) {
			GroupPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 33) {
			ArrayPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 24) {
			MethodCheckPattern(out node);
			if (la.kind == 1) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 84) {
			NotPattern(out node);
		} else if (StartOf(13)) {
			ComparisonPattern(out node);
		} else SynErr(115);
	}

	void AsPattern(DPattern target, out DPattern node) {
		node = null; 
		if (la.AfterEol) { node = target; return; }
		var asp = new DAsPattern(t) { Pattern = target };
		
		Expect(1);
		asp.Name = t.val; node = asp; 
	}

	void IntegerPattern(out DPattern node) {
		Expect(3);
		node = new DIntegerPattern(t) { Value = ParseInteger() }; 
	}

	void FloatPattern(out DPattern node) {
		Expect(4);
		node = new DFloatPattern(t) { Value = ParseFloat() }; 
	}

	void CharPattern(out DPattern node) {
		Expect(6);
		node = new DCharPattern(t) { Value = ParseChar() }; 
	}

	void StringPattern(out DPattern node) {
		Expect(5);
		node = new DStringPattern(t) { Value = ParseString() }; 
	}

	void BooleanPattern(out DPattern node) {
		if (la.kind == 73) {
			Get();
		} else if (la.kind == 74) {
			Get();
		} else SynErr(116);
		node = new DBooleanPattern(t) { Value = t.val == "true" }; 
	}

	void NilPattern(out DPattern node) {
		Expect(86);
		node = new DNilPattern(t); 
	}

	void TuplePattern(out DPattern node) {
		node = null; 
		Expect(29);
		var tup = new DTuplePattern(t); 
		if (IsLabelPattern()) {
			LabelPattern(out node);
		} else if (StartOf(12)) {
			OrPattern(out node);
		} else SynErr(117);
		tup.Elements.Add(node); 
		while (la.kind == 25) {
			Get();
			if (StartOf(12)) {
				if (IsLabelPattern()) {
					LabelPattern(out node);
				} else {
					OrPattern(out node);
				}
				tup.Elements.Add(node); 
			}
		}
		node = tup; 
		Expect(30);
	}

	void GroupPattern(out DPattern node) {
		Expect(29);
		OrPattern(out node);
		Expect(30);
	}

	void ArrayPattern(out DPattern node) {
		node = null; 
		Expect(33);
		var tup = new DArrayPattern(t); 
		RangePattern(out node);
		tup.Elements.Add(node); 
		while (la.kind == 25) {
			Get();
			RangePattern(out node);
			tup.Elements.Add(node); 
		}
		node = tup; 
		Expect(34);
	}

	void MethodCheckPattern(out DPattern node) {
		var name = ""; 
		Expect(24);
		if (la.kind == 1) {
			Get();
			name = t.val; 
		} else if (la.kind == 29) {
			Get();
			switch (la.kind) {
			case 46: {
				Get();
				break;
			}
			case 47: {
				Get();
				break;
			}
			case 51: {
				Get();
				break;
			}
			case 52: {
				Get();
				break;
			}
			case 56: {
				Get();
				break;
			}
			case 57: {
				Get();
				break;
			}
			case 85: {
				Get();
				break;
			}
			case 59: {
				Get();
				break;
			}
			case 58: {
				Get();
				break;
			}
			case 60: {
				Get();
				break;
			}
			case 61: {
				Get();
				break;
			}
			case 48: {
				Get();
				break;
			}
			case 53: {
				Get();
				break;
			}
			case 63: {
				Get();
				break;
			}
			case 64: {
				Get();
				break;
			}
			case 65: {
				Get();
				break;
			}
			default: SynErr(118); break;
			}
			name = t.val; 
			Expect(30);
		} else SynErr(119);
		node = new DMethodCheckPattern(t) { Name = name }; 
	}

	void NotPattern(out DPattern node) {
		Expect(84);
		var np = new DNotPattern(t); 
		Pattern(out node);
		np.Pattern = node; node = np; 
	}

	void ComparisonPattern(out DPattern node) {
		node = null; BinaryOperator op = default; Token ot = default; 
		if (la.kind == 58) {
			Get();
			op = BinaryOperator.Gt; ot = t; 
		} else if (la.kind == 59) {
			Get();
			op = BinaryOperator.Lt; ot = t; 
		} else if (la.kind == 60) {
			Get();
			op = BinaryOperator.GtEq; ot = t; 
		} else if (la.kind == 61) {
			Get();
			op = BinaryOperator.LtEq; ot = t; 
		} else SynErr(120);
		switch (la.kind) {
		case 3: {
			IntegerPattern(out node);
			break;
		}
		case 4: {
			FloatPattern(out node);
			break;
		}
		case 6: {
			CharPattern(out node);
			break;
		}
		case 5: {
			StringPattern(out node);
			break;
		}
		case 73: case 74: {
			BooleanPattern(out node);
			break;
		}
		case 86: {
			NilPattern(out node);
			break;
		}
		default: SynErr(121); break;
		}
		node = new DComparisonPattern(ot) { Operator = op, Pattern = node }; 
	}

	void LabelPattern(out DPattern node) {
		node = null; 
		Expect(1);
		var la = new DLabelPattern(t) { Label = t.val }; 
		Expect(27);
		Pattern(out var pat);
		la.Pattern = pat; node = la; 
	}

	void While(out DNode node) {
		node = null; 
		Expect(20);
		var @while = new DWhile(t); 
		Expr(out node);
		@while.Condition = node; 
		Block(out node);
		@while.Body = node;
		node = @while;
		
	}

	void For(out DNode node) {
		node = null; 
		Expect(19);
		var @for = new DFor(t); 
		OrPattern(out var pattern);
		@for.Pattern = pattern; 
		Expect(22);
		Expr(out node);
		@for.Target = node; 
		if (la.kind == 71) {
			Get();
			Expr(out node);
			@for.Guard = node; 
		}
		Block(out node);
		@for.Body = node;
		node = @for;
		
	}

	void DoWhile(out DNode node) {
		node = null; 
		var @while = new DWhile(t) { DoWhile = true }; 
		Expect(87);
		Block(out node);
		@while.Body = node; 
		Expect(20);
		Expr(out node);
		@while.Condition = node; node = @while; 
	}

	void Break(out DNode node) {
		Expect(16);
		var br = new DBreak(t); node = br; 
		if (StartOf(14)) {
			if (la.AfterEol) return; 
			Expr(out var exp);
			br.Expression = exp; 
		}
	}

	void Continue(out DNode node) {
		Expect(15);
		node = new DContinue(t); 
	}

	void Return(out DNode node) {
		Expect(13);
		var br = new DReturn(t); node = br;
		if (la.AfterEol) return;
		
		if (StartOf(14)) {
			Expr(out var exp);
			br.Expression = exp; 
		}
	}

	void Yield(out DNode node) {
		Expect(17);
		var ot = t;
		node = null;
		functions.Peek().IsIterator = true;
		
		if (la.kind == 16) {
			Get();
			node = new DYieldBreak(ot); 
		} else if (la.kind == 88) {
			Get();
			var yield = new DYieldMany(t);
			node = yield;
			
			Expr(out var exp);
			yield.Expression = exp; 
		} else if (StartOf(14)) {
			var yield = new DYield(t);
			node = yield;
			
			Expr(out var exp);
			yield.Expression = exp; 
		} else SynErr(122);
	}

	void FunctionExpr(out DNode node) {
		var f = new DFunctionDeclaration(t);
		node = f;
		
		if (la.kind == 1) {
			FunctionArgument(out var a);
			f.Parameters.Add(a); 
		} else if (la.kind == 29) {
			FunctionArguments(f);
		} else SynErr(123);
		functions.Push(f); 
		Expect(23);
		Expr(out var exp);
		f.Body = exp; functions.Pop(); 
	}

	void NoArgFunctionExpr(out DNode node) {
		var f = new DFunctionDeclaration(t);
		node = f;
		
		functions.Push(f); 
		Expect(23);
		Expr(out var exp);
		f.Body = exp; functions.Pop(); 
	}

	void Coalesce(out DNode node) {
		Or(out node);
		while (la.kind == 50) {
			Get();
			var ot = t; 
			Or(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Coalesce, ot); 
		}
	}

	void TernaryExpr(DNode parent, out DNode node) {
		node = null; 
		Expect(89);
		var @if = new DIf(t) { Condition = parent }; 
		SimpleExpr(out node);
		@if.True = node; 
		Expect(27);
		SimpleExpr(out node);
		@if.False = node; node = @if; 
	}

	void TryCatch(out DNode node) {
		node =  null; 
		Expect(91);
		var tc = new DTryCatch(t); 
		Block(out node);
		tc.Expression = node; 
		Expect(92);
		if (la.kind == 31) {
			var m = new DMatch(t); tc.Catch = m; 
			Get();
			MatchEntry(out var entry);
			m.Entries.Add(entry); 
			while (la.kind == 25) {
				Get();
				MatchEntry(out entry);
				m.Entries.Add(entry); 
			}
			Expect(32);
		} else if (la.kind == 1) {
			Get();
			tc.BindVariable = new DName(t) { Value = t.val }; 
			Block(out node);
			tc.Catch = node; 
		} else SynErr(124);
		node = tc; 
	}

	void Throw(out DNode node) {
		node = null; 
		Expect(90);
		var th = new DThrow(t);
		node = th;
		if (la.AfterEol) return;
		
		Expr(out var cexp);
		th.Expression = cexp; 
	}

	void Or(out DNode node) {
		And(out node);
		while (la.kind == 93) {
			Get();
			var ot = t; 
			And(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Or, ot); 
		}
	}

	void And(out DNode node) {
		Is(out node);
		while (la.kind == 94) {
			Get();
			var ot = t; 
			Is(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.And, ot); 
		}
	}

	void Is(out DNode node) {
		In(out node);
		while (la.kind == 95) {
			Get();
			var ot = t; 
			OrPattern(out var pat);
			node = new DBinaryOperation(node, pat, BinaryOperator.Is, ot); 
		}
	}

	void In(out DNode node) {
		Range(out node);
		while (la.kind == 22) {
			Get();
			var ot = t; 
			Range(out var pat);
			node = new DBinaryOperation(node, pat, BinaryOperator.In, ot); 
		}
	}

	void Range(out DNode node) {
		node = null; DNode snode = null; DNode cnode = null; bool exclu = false; 
		if (la.kind == 85) {
			Get();
			FunctionApplication(out snode);
			if (la.kind == 83) {
				Get();
			} else if (la.kind == 96) {
				Get();
				exclu = true; 
			} else SynErr(125);
			var range = new DRange(t) { From = node, Exclusive = exclu, Step = snode }; node = range; 
			if (StartOf(15)) {
				FunctionApplication(out cnode);
				range.To = cnode; 
			}
		} else if (la.kind == 83 || la.kind == 96) {
			if (la.kind == 83) {
				Get();
			} else {
				Get();
				exclu = true; 
			}
			var range = new DRange(t) { From = node, Exclusive = exclu }; node = range; 
			if (StartOf(15)) {
				FunctionApplication(out cnode);
				range.To = cnode; 
			}
		} else if (StartOf(15)) {
			Eq(out node);
			if (la.kind == 85) {
				Get();
				FunctionApplication(out snode);
			}
			if (la.kind == 83 || la.kind == 96) {
				if (la.kind == 83) {
					Get();
				} else {
					Get();
					exclu = true; 
				}
				var range = new DRange(t) { From = node, Exclusive = exclu, Step = snode }; node = range; 
				if (StartOf(15)) {
					FunctionApplication(out cnode);
					range.To = cnode; 
				}
			}
		} else SynErr(126);
	}

	void FunctionApplication(out DNode node) {
		node = null; 
		Unary(out node);
		while (la.kind == 97) {
			var app = new DApplication(node, t); 
			Get();
			Unary(out node);
			app.Arguments.Add(node); node = app; 
		}
	}

	void Eq(out DNode node) {
		Shift(out node);
		while (StartOf(16)) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			switch (la.kind) {
			case 58: {
				Get();
				ot = t; op = BinaryOperator.Gt; 
				break;
			}
			case 59: {
				Get();
				ot = t; op = BinaryOperator.Lt; 
				break;
			}
			case 60: {
				Get();
				ot = t; op = BinaryOperator.GtEq; 
				break;
			}
			case 61: {
				Get();
				ot = t; op = BinaryOperator.LtEq; 
				break;
			}
			case 56: {
				Get();
				ot = t; op = BinaryOperator.Eq; 
				break;
			}
			case 57: {
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
		while (la.kind == 63 || la.kind == 64) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 63) {
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
		while (la.kind == 54) {
			Get();
			var ot = t; 
			Xor(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseOr, ot); 
		}
	}

	void Xor(out DNode node) {
		BitAnd(out node);
		while (la.kind == 62) {
			DNode exp = null; 
			Get();
			var ot = t; 
			BitAnd(out exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Xor, ot); 
		}
	}

	void BitAnd(out DNode node) {
		Add(out node);
		while (la.kind == 55) {
			Get();
			var ot = t; 
			Add(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseAnd, ot); 
		}
	}

	void Add(out DNode node) {
		Mul(out node);
		while (la.kind == 46 || la.kind == 47) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 47) {
				Get();
				ot = t; op = BinaryOperator.Add; 
			} else {
				Get();
				ot = t; op = BinaryOperator.Sub; 
			}
			Mul(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Mul(out DNode node) {
		Cast(out node);
		while (la.kind == 51 || la.kind == 52 || la.kind == 53) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 51) {
				Get();
				ot = t; op = BinaryOperator.Mul; 
			} else if (la.kind == 52) {
				Get();
				ot = t; op = BinaryOperator.Div; 
			} else {
				Get();
				ot = t; op = BinaryOperator.Rem; 
			}
			Cast(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Cast(out DNode node) {
		node = null; 
		FunctionApplication(out node);
		while (la.kind == 79) {
			var @as = new DAs(t) { Expression = node }; string s1, s2 = null; 
			Get();
			Expect(1);
			s1 = t.val; 
			if (la.kind == 24) {
				Get();
				Expect(1);
				s2 = t.val; 
			}
			@as.Expression = node;
			@as.TypeName = s2 is null ? new Qualident(s1) : new Qualident(s1, s2);
			node = @as;
			
		}
	}

	void Unary(out DNode node) {
		node = null;
		var op = default(UnaryOperator);
		var ot = default(Token);
		
		if (la.kind == 48) {
			Get();
			ot = t; op = UnaryOperator.Not; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 46) {
			Get();
			ot = t; op = UnaryOperator.Neg; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 47) {
			Get();
			ot = t; op = UnaryOperator.Plus; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 65) {
			Get();
			ot = t; op = UnaryOperator.BitwiseNot; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (StartOf(17)) {
			Index(out node);
		} else SynErr(127);
	}

	void Index(out DNode node) {
		Literal(out node);
		while (la.kind == 24 || la.kind == 29 || la.kind == 33) {
			if (la.kind == 24) {
				Get();
				var ot = t; 
				Expect(1);
				var fld = new DAccess(ot) { Target = node };
				fld.Name = t.val;
				node = fld;
				
			} else if (la.kind == 33) {
				if (la.AfterEol) return; 
				Get();
				var idx = new DIndexer(t) { Target = node }; 
				Expr(out node);
				idx.Index = node;
				node = idx;
				
				Expect(34);
			} else {
				if (la.AfterEol) return; 
				Get();
				var app = new DApplication(node, t); 
				if (StartOf(18)) {
					ApplicationArguments(app);
				}
				node = app; 
				Expect(30);
			}
		}
	}

	void Literal(out DNode node) {
		node = null; 
		if (la.kind == 1 || la.kind == 14) {
			Name(out node);
		} else if (la.kind == 7) {
			SpecialName(out node);
		} else if (la.kind == 3) {
			Integer(out node);
		} else if (la.kind == 4) {
			Float(out node);
		} else if (la.kind == 5 || la.kind == 8) {
			String(out node);
		} else if (la.kind == 6) {
			Char(out node);
		} else if (la.kind == 73 || la.kind == 74) {
			Bool(out node);
		} else if (la.kind == 86) {
			Nil(out node);
		} else if (IsTuple()) {
			Tuple(out node);
		} else if (la.kind == 29) {
			Group(out node);
		} else if (la.kind == 99) {
			Base(out node);
		} else if (la.kind == 33) {
			Array(out node);
		} else if (la.kind == 98) {
			Iterator(out node);
		} else SynErr(128);
	}

	void ApplicationArguments(DApplication app) {
		var node = default(DNode); 
		if (IsLabel()) {
			Label(out node);
		} else if (StartOf(14)) {
			Expr(out node);
		} else SynErr(129);
		app.Arguments.Add(node); 
		while (la.kind == 25) {
			Get();
			if (IsLabel()) {
				Label(out node);
			} else if (StartOf(14)) {
				Expr(out node);
			} else SynErr(130);
			app.Arguments.Add(node); 
		}
	}

	void Label(out DNode node) {
		bool mut = false; node = null; var name = ""; var fromStr = false; 
		if (la.kind == 10 || la.kind == 11) {
			if (la.kind == 10) {
				Get();
				mut = true; 
			} else {
				Get();
			}
		}
		if (la.kind == 1) {
			Get();
			name = t.val; 
		} else if (la.kind == 5) {
			Get();
			name = ParseSimpleString(); fromStr = true; 
		} else SynErr(131);
		Expect(27);
		var ot = t; 
		if (IsFunction()) {
			FunctionExpr(out node);
		} else if (StartOf(11)) {
			Is(out node);
		} else SynErr(132);
		node = new DLabelLiteral(ot) { Mutable = mut, Label = name, FromString = fromStr, Expression = node }; 
	}

	void Name(out DNode node) {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 14) {
			Get();
		} else SynErr(133);
		node = new DName(t) { Value = t.val }; 
	}

	void SpecialName(out DNode node) {
		Expect(7);
		var nm = int.Parse(t.val.Substring(1));
		node = new DName(t) { Value = "p" + nm };
		if (implicits == null)
		   implicits = new List<int>();
		implicits.Add(nm);
		
	}

	void Integer(out DNode node) {
		Expect(3);
		node = new DIntegerLiteral(t) { Value = ParseInteger() }; 
	}

	void Float(out DNode node) {
		Expect(4);
		node = new DFloatLiteral(t) { Value = ParseFloat() }; 
	}

	void String(out DNode node) {
		node = null; 
		if (la.kind == 5) {
			Get();
			node = ParseString(); 
		} else if (la.kind == 8) {
			Get();
			node = ParseVerbatimString(); 
		} else SynErr(134);
	}

	void Char(out DNode node) {
		Expect(6);
		node = new DCharLiteral(t) { Value = ParseChar() }; 
	}

	void Bool(out DNode node) {
		if (la.kind == 73) {
			Get();
		} else if (la.kind == 74) {
			Get();
		} else SynErr(135);
		node = new DBooleanLiteral(t) { Value = t.val == "true" }; 
	}

	void Nil(out DNode node) {
		Expect(86);
		node = new DNilLiteral(t); 
	}

	void Tuple(out DNode node) {
		node = null; 
		Expect(29);
		var tup = new DTupleLiteral(t); 
		if (IsLabel()) {
			Label(out node);
		} else if (StartOf(14)) {
			Expr(out node);
		} else SynErr(136);
		tup.Elements.Add(node); 
		while (la.kind == 25) {
			Get();
			if (StartOf(18)) {
				if (IsLabel()) {
					Label(out node);
				} else {
					Expr(out node);
				}
				tup.Elements.Add(node); 
			}
		}
		node = tup; 
		Expect(30);
	}

	void Group(out DNode node) {
		node = null; 
		Expect(29);
		Expr(out node);
		Expect(30);
	}

	void Base(out DNode node) {
		Expect(99);
		node = new DBase(t); 
	}

	void Array(out DNode node) {
		node = null; 
		Expect(33);
		var arr = new DArrayLiteral(t); 
		if (StartOf(14)) {
			Expr(out node);
			arr.Elements.Add(node); 
			while (la.kind == 25) {
				Get();
				Expr(out node);
				arr.Elements.Add(node); 
			}
		}
		node = arr; 
		Expect(34);
	}

	void Iterator(out DNode node) {
		node = null; 
		Expect(98);
		Expect(31);
		var it = new DIteratorLiteral(t);
		it.YieldBlock = new DYieldBlock(t);
		
		if (StartOf(14)) {
			Expr(out node);
			it.YieldBlock.Elements.Add(node); 
		}
		while (la.kind == 25) {
			Get();
			Expr(out node);
			it.YieldBlock.Elements.Add(node); 
		}
		node = it; 
		Expect(32);
	}

	void DyalectItem() {
		if (StartOf(4)) {
			Statement(out var node);
			Root.Nodes.Add(node);
			
		} else if (la.kind == 68) {
			Import(out var node);
			Imports.Add(node); 
			Separator();
		} else SynErr(137);
	}

	void Dyalect() {
		DyalectItem();
		while (StartOf(3)) {
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
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _T,_x,_x,_x, _x,_T,_T,_T, _x,_x,_T,_x, _T,_T,_T,_T, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_T,_T,_T, _x,_x,_T,_x, _T,_T,_T,_T, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_T,_x, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_T,_T, _T,_x,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x},
		{_x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_T,_T, _x,_x,_T,_x, _x,_x,_T,_T, _T,_x,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x}

        };

        private void SynErr(int line, int col, int n)
        {
            string s;

            switch (n)
            {
			case 0: s = "EOF expected"; break;
			case 1: s = "identToken expected"; break;
			case 2: s = "directive expected"; break;
			case 3: s = "intToken expected"; break;
			case 4: s = "floatToken expected"; break;
			case 5: s = "stringToken expected"; break;
			case 6: s = "charToken expected"; break;
			case 7: s = "implicitToken expected"; break;
			case 8: s = "verbatimStringToken expected"; break;
			case 9: s = "autoToken expected"; break;
			case 10: s = "varToken expected"; break;
			case 11: s = "letToken expected"; break;
			case 12: s = "funcToken expected"; break;
			case 13: s = "returnToken expected"; break;
			case 14: s = "privateToken expected"; break;
			case 15: s = "continueToken expected"; break;
			case 16: s = "breakToken expected"; break;
			case 17: s = "yieldToken expected"; break;
			case 18: s = "ifToken expected"; break;
			case 19: s = "forToken expected"; break;
			case 20: s = "whileToken expected"; break;
			case 21: s = "typeToken expected"; break;
			case 22: s = "inToken expected"; break;
			case 23: s = "arrowToken expected"; break;
			case 24: s = "dotToken expected"; break;
			case 25: s = "commaToken expected"; break;
			case 26: s = "semicolonToken expected"; break;
			case 27: s = "colonToken expected"; break;
			case 28: s = "equalToken expected"; break;
			case 29: s = "parenLeftToken expected"; break;
			case 30: s = "parenRightToken expected"; break;
			case 31: s = "curlyLeftToken expected"; break;
			case 32: s = "curlyRightToken expected"; break;
			case 33: s = "squareLeftToken expected"; break;
			case 34: s = "squareRightToken expected"; break;
			case 35: s = "eq_coa expected"; break;
			case 36: s = "eq_add expected"; break;
			case 37: s = "eq_sub expected"; break;
			case 38: s = "eq_mul expected"; break;
			case 39: s = "eq_div expected"; break;
			case 40: s = "eq_rem expected"; break;
			case 41: s = "eq_and expected"; break;
			case 42: s = "eq_or expected"; break;
			case 43: s = "eq_xor expected"; break;
			case 44: s = "eq_lsh expected"; break;
			case 45: s = "eq_rsh expected"; break;
			case 46: s = "minus expected"; break;
			case 47: s = "plus expected"; break;
			case 48: s = "not expected"; break;
			case 49: s = "bitnot expected"; break;
			case 50: s = "coalesce expected"; break;
			case 51: s = "\"*\" expected"; break;
			case 52: s = "\"/\" expected"; break;
			case 53: s = "\"%\" expected"; break;
			case 54: s = "\"|||\" expected"; break;
			case 55: s = "\"&&&\" expected"; break;
			case 56: s = "\"==\" expected"; break;
			case 57: s = "\"!=\" expected"; break;
			case 58: s = "\">\" expected"; break;
			case 59: s = "\"<\" expected"; break;
			case 60: s = "\">=\" expected"; break;
			case 61: s = "\"<=\" expected"; break;
			case 62: s = "\"^^^\" expected"; break;
			case 63: s = "\"<<<\" expected"; break;
			case 64: s = "\">>>\" expected"; break;
			case 65: s = "\"~~~\" expected"; break;
			case 66: s = "\"#test\" expected"; break;
			case 67: s = "\"#endtest\" expected"; break;
			case 68: s = "\"import\" expected"; break;
			case 69: s = "\"or\" expected"; break;
			case 70: s = "\"...\" expected"; break;
			case 71: s = "\"when\" expected"; break;
			case 72: s = "\"else\" expected"; break;
			case 73: s = "\"true\" expected"; break;
			case 74: s = "\"false\" expected"; break;
			case 75: s = "\"static\" expected"; break;
			case 76: s = "\"and\" expected"; break;
			case 77: s = "\"get\" expected"; break;
			case 78: s = "\"set\" expected"; break;
			case 79: s = "\"as\" expected"; break;
			case 80: s = "\"const\" expected"; break;
			case 81: s = "\"lazy\" expected"; break;
			case 82: s = "\"match\" expected"; break;
			case 83: s = "\"..\" expected"; break;
			case 84: s = "\"not\" expected"; break;
			case 85: s = "\"^\" expected"; break;
			case 86: s = "\"nil\" expected"; break;
			case 87: s = "\"do\" expected"; break;
			case 88: s = "\"many\" expected"; break;
			case 89: s = "\"?\" expected"; break;
			case 90: s = "\"throw\" expected"; break;
			case 91: s = "\"try\" expected"; break;
			case 92: s = "\"catch\" expected"; break;
			case 93: s = "\"||\" expected"; break;
			case 94: s = "\"&&\" expected"; break;
			case 95: s = "\"is\" expected"; break;
			case 96: s = "\"..<\" expected"; break;
			case 97: s = "\"\\\\\" expected"; break;
			case 98: s = "\"yields\" expected"; break;
			case 99: s = "\"base\" expected"; break;
			case 100: s = "??? expected"; break;
			case 101: s = "invalid StandardOperators"; break;
			case 102: s = "invalid FunctionName"; break;
			case 103: s = "invalid ImportToken"; break;
			case 104: s = "invalid TestBlock"; break;
			case 105: s = "invalid Statement"; break;
			case 106: s = "invalid Expr"; break;
			case 107: s = "invalid Binding"; break;
			case 108: s = "invalid ControlFlow"; break;
			case 109: s = "invalid If"; break;
			case 110: s = "invalid Loops"; break;
			case 111: s = "invalid SimpleExpr"; break;
			case 112: s = "invalid FunctionBody"; break;
			case 113: s = "invalid FunctionBody"; break;
			case 114: s = "invalid NamePattern"; break;
			case 115: s = "invalid Pattern"; break;
			case 116: s = "invalid BooleanPattern"; break;
			case 117: s = "invalid TuplePattern"; break;
			case 118: s = "invalid MethodCheckPattern"; break;
			case 119: s = "invalid MethodCheckPattern"; break;
			case 120: s = "invalid ComparisonPattern"; break;
			case 121: s = "invalid ComparisonPattern"; break;
			case 122: s = "invalid Yield"; break;
			case 123: s = "invalid FunctionExpr"; break;
			case 124: s = "invalid TryCatch"; break;
			case 125: s = "invalid Range"; break;
			case 126: s = "invalid Range"; break;
			case 127: s = "invalid Unary"; break;
			case 128: s = "invalid Literal"; break;
			case 129: s = "invalid ApplicationArguments"; break;
			case 130: s = "invalid ApplicationArguments"; break;
			case 131: s = "invalid Label"; break;
			case 132: s = "invalid Label"; break;
			case 133: s = "invalid Name"; break;
			case 134: s = "invalid String"; break;
			case 135: s = "invalid Bool"; break;
			case 136: s = "invalid Tuple"; break;
			case 137: s = "invalid DyalectItem"; break;

                default:
                    s = "unknown " + n;
                    break;
            }

            AddError(s, line, col);
        }
    }
}
